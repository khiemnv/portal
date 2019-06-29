//#define use_custom_cols
#define format_currency
#define use_sqlite
//#define check_number_input
#define col_class

using System.Windows.Forms;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using System.Globalization;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace test_gui
{
#if custom_control
    public class myCustomCtrl : IDisposable
    {
        public DataGridView m_DGV;
        public Control m_ctrl { get { return getControl(); } }
        public bool m_bChanged = false;
        public int m_iRow;
        public int m_iCol;

        protected myCustomCtrl(DataGridView dgv)
        {
            m_DGV = dgv;
            //m_ctrl = ctrl;
        }

        public virtual void show(Rectangle rec)
        {
            m_ctrl.Location = rec.Location;
            m_ctrl.Size = rec.Size;
            m_ctrl.Visible = true;

        }
        public virtual void hide() { m_ctrl.Visible = false; }
        public virtual bool isChanged() { return m_bChanged; }
        public virtual string getValue() { return ""; }
        public virtual void setValue(string text) { }
        public virtual Control getControl() { return null; }
        public virtual void ctrl_ValueChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("ctrl_ValueChanged");
            m_bChanged = true;
            m_DGV.NotifyCurrentCellDirty(true);
        }

        internal void reLocation()
        {
            Rectangle rec = m_DGV.GetCellDisplayRectangle(m_iCol, m_iRow, true);
            m_ctrl.Size = rec.Size;
            m_ctrl.Location = rec.Location;
        }

    #region dispose
        // Dispose() calls Dispose(true)  
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // NOTE: Leave out the finalizer altogether if this class doesn't   
        // own unmanaged resources itself, but leave the other methods  
        // exactly as they are.   
        ~myCustomCtrl()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)  
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            // free native resources if there are any.  
            m_ctrl.Dispose();
        }
    #endregion
    }
    public class myComboBox : myCustomCtrl
    {
        private ComboBox m_combo;

        //data table has single column
        public myComboBox(DataGridView dgv, lDataSync data)
            : base(dgv)
        {
            m_combo = new ComboBox();
            m_combo.DataSource = data.m_bindingSrc;
            DataTable tbl = (DataTable)data.m_bindingSrc.DataSource;
            m_combo.DisplayMember = tbl.Columns[1].ColumnName;
            m_combo.SelectedValueChanged += ctrl_ValueChanged;
        }
        public override Control getControl()
        {
            return m_combo;
        }
        public override string getValue()
        {
            Debug.WriteLine("getValue: " + m_combo.Text);
            return m_combo.Text;
        }

        public override void setValue(string text)
        {
            Debug.WriteLine("setValue: " + text);
            m_combo.Text = text;
        }
    }
    public class myDateTimePicker : myCustomCtrl
    {
        public DateTimePicker m_dtp;

        public myDateTimePicker(DataGridView dgv)
            : base(dgv)
        {
            m_dtp = new DateTimePicker();
            m_dtp.Format = DateTimePickerFormat.Custom;
            m_dtp.CustomFormat = lConfigMng.GetDisplayDateFormat();
            m_dtp.ValueChanged += ctrl_ValueChanged;
        }
        public override Control getControl()
        {
            return m_dtp;
        }
        public override string getValue()
        {
            return m_dtp.Value.ToString(lConfigMng.GetDisplayDateFormat());
        }
        public override void setValue(string text)
        {
            DateTime dt;
            if (text.Length == 0)
            {
                //do no thing
            }
            else if (DateTime.TryParse(text, out dt))
            {
                m_dtp.Value = dt;
            }
            else
            {
                Debug.Assert(false, "invalid date string");
            }
        }
    }

    public class lCustomDGV : DataGridView
    {
        protected TableInfo m_tblInfo;
        myCustomCtrl m_customCtrl;
        DataRow m_newRow;
        DataTable m_dataTable;

        public lCustomDGV(TableInfo tblInfo)
        {
            m_tblInfo = tblInfo;
            DataTable dt = appConfig.s_contentProvider.CreateDataContent(m_tblInfo.m_tblName).m_dataTable;
            dt.TableNewRow += Dt_TableNewRow;
            m_dataTable = dt;
        }

        private void Dt_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            m_newRow = e.Row;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Control)
                switch (e.KeyCode)
                {
                    case Keys.C:
                        //copy to clip board
                        copyClipboard();
                        break;
                    case Keys.V:
                        //paste data
                        pasteClipboard();
                        break;
                }
        }

        private void pasteClipboard()
        {
            Debug.WriteLine("{0}.pasteClipboard {1}", this, Clipboard.GetText());
            string inTxt = Clipboard.GetText();
            var lines = inTxt.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            int baseRow = CurrentCell.RowIndex;
            int baseCol = CurrentCell.ColumnIndex;
            DataTable tbl = m_dataTable;
            int iRow = baseRow;
            foreach (var line in lines)
            {
                bool bNewRow = false;
                var fields = line.Split(new char[] { '\t', ';' });
                DataRow row;
                if (iRow < tbl.Rows.Count)
                {
                    row = tbl.Rows[iRow];
                }
                else
                {
                    row = (m_newRow != null) ? m_newRow : tbl.NewRow();
                    m_newRow = null;
                    bNewRow = true;
                }
                int iCol = baseCol;
                foreach (var field in fields)
                {
                    //m_dataGridView[iCol, iRow].Value = field;
                    switch (m_tblInfo.m_cols[iCol].m_type)
                    {
#if format_currency
                        case TableInfo.ColInfo.ColType.currency:
                            row[iCol] = field.Replace(",", "");
                            break;
#endif
                        default:
                            row[iCol] = field;
                            break;
                    }
                    iCol++;
                }
                Debug.WriteLine("before tbl add row");
                if (bNewRow) tbl.Rows.Add(row);
                Debug.WriteLine("after tbl add row");
                iRow++;
            }
            //m_dataGridView.CurrentCell = m_dataGridView[baseCol, baseRow];
            Debug.WriteLine("{0}.pasteClipboard end", this);
        }

        private void copyClipboard()
        {
            Clipboard.SetDataObject(GetClipboardContent());
        }

        protected override void OnDataError(bool displayErrorDialogIfNoHandler, DataGridViewDataErrorEventArgs e)
        {
            //base.OnDataError(displayErrorDialogIfNoHandler, e);
            //do nothing
        }
#if !use_custom_cols

        void showInputError(string msg)
        {
            MessageBox.Show(msg, "Input error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void OnCellValidating(DataGridViewCellValidatingEventArgs e)
        {
            Debug.WriteLine("OnCellValidating");
            base.OnCellValidating(e);
            //check unique value
            var col = m_tblInfo.m_cols[e.ColumnIndex];
            string val = e.FormattedValue.ToString();
            switch (col.m_type)
            {
                case TableInfo.ColInfo.ColType.uniq:
                    {
                        string rowid = Rows[e.RowIndex].Cells[0].Value.ToString();
#if use_sqlite
                        string sql = string.Format("select rowid, {0} from {1} where {0} = '{2}'",
                            col.m_field, m_tblInfo.m_tblName, val);
#else
                        string sql = string.Format("select id, {0} from {1} where {0} = '{2}'",
                            colInfo.m_field, m_tblInfo.m_tblName, val);
#endif //use_sqlite
                        var tbl = appConfig.s_contentProvider.GetData(sql);
                        if ((tbl.Rows.Count > 0) && (rowid != tbl.Rows[0][0].ToString()))
                        {
                            Debug.WriteLine("{0} {1} not unique value {2}", this, "OnCellValidating() check unique", val);
                            showInputError("Mã này đã tồn tại!");
                            e.Cancel = true;
                        }
                    }
                    break;
                case TableInfo.ColInfo.ColType.map:
                    string txt = e.FormattedValue.ToString();
                    int n;
                    bool bChk;
                    if (int.TryParse(txt, out n))
                    {
                        bChk = col.ParseEnum(n, out txt);
                    }
                    else
                    {
                        bChk = col.ParseEnum(txt, out n);
                    }
                    if (bChk == false)
                    {
                        string msg = string.Format("Invalid input for {0}\n{1}", col.m_alias, col.GetHelp());
                        lConfigMng.ShowInputError(msg);
                        e.Cancel = true;
                    }
                    break;
#if check_number_input
                case lTableInfo.lColInfo.lColType.currency:
                case lTableInfo.lColInfo.lColType.num:
                    {
                        UInt64 tmp;
                        if (!UInt64.TryParse(val.Replace(",","") , out tmp))
                        {
                            Debug.WriteLine("{0} {1} not numberic value {2}", this, "OnCellValidating() check unique", val);
                            MessageBox.Show("This field must be numberic!", "Input error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Cancel = true;
                        }
                    }
                    break;
#endif
            }
        }

        protected override void OnCellEndEdit(DataGridViewCellEventArgs e)
        {
            base.OnCellEndEdit(e);
            Debug.WriteLine("OnCellEndEdit");
            HideCustomCtrl();
            //update selected value
            lDataSync data = m_tblInfo.m_cols[e.ColumnIndex].m_lookupData;
            if (data != null && CurrentCell.Value != null)
            {
                string key = CurrentCell.Value.ToString();
                string val = data.find(key);
                if (val != null)
                    CurrentCell.Value = val;
            }
        }
        protected override void OnCellLeave(DataGridViewCellEventArgs e)
        {
            Debug.WriteLine("OnCellLeave");
            base.OnCellLeave(e);
            //HideCustomCtrl();
        }
        protected override void OnCellDoubleClick(DataGridViewCellEventArgs e)
        {
            base.OnCellDoubleClick(e);
            Debug.WriteLine("OnCellClick");
            if (e.ColumnIndex != -1) showCustomCtrl(e.ColumnIndex, e.RowIndex);
        }

        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);
            if (m_customCtrl != null)
            {
                m_customCtrl.reLocation();
            }
        }

        lDataSync m_autoCompleteData;

        protected override void OnEditingControlShowing(DataGridViewEditingControlShowingEventArgs e)
        {
            base.OnEditingControlShowing(e);
            Debug.WriteLine("OnEditingControlShowing");
            var col = m_tblInfo.m_cols[CurrentCell.ColumnIndex];
            switch (col.m_type)
            {
                case TableInfo.ColInfo.ColType.map:
                    ToolTip tt = new ToolTip
                    {
                        IsBalloon = true,
                        InitialDelay = 0,
                        ShowAlways = true
                    };
                    tt.SetToolTip(e.Control, col.GetHelp());
                    break;
                default:
                    do
                    {
                        if (m_customCtrl != null) break;

                        m_autoCompleteData = m_tblInfo.m_cols[CurrentCell.ColumnIndex].m_lookupData;
                        if (m_autoCompleteData == null) break;

                        AutoCompleteStringCollection coll = m_autoCompleteData.m_colls;
                        DataGridViewTextBoxEditingControl edt = (DataGridViewTextBoxEditingControl)e.Control;
                        edt.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                        edt.AutoCompleteSource = AutoCompleteSource.CustomSource;
                        edt.AutoCompleteCustomSource = coll;

                        edt.Validated += Edt_Validated;
                    } while (false);
                    break;
            }
        }

        private void Edt_Validated(object sender, EventArgs e)
        {
            TextBox edt = (TextBox)sender;
            Debug.WriteLine("Edt_Validated:" + edt.Text);
            string selectedValue = edt.Text;
            if (selectedValue != "")
            {
                m_autoCompleteData.Update(selectedValue);
            }

            m_autoCompleteData = null;
            edt.Validated -= Edt_Validated;
        }

        public virtual void showCustomCtrl(int col, int row)
        {
            Debug.WriteLine("showDtp");

            //fix error control not hide
            if (m_customCtrl != null)
            {
                Debug.Assert(false, "previous ctrl should be disposed");
                m_customCtrl.Dispose();
                m_customCtrl = null;
                return;
            }

            if (m_tblInfo.m_cols[col].m_type == TableInfo.ColInfo.ColType.dateTime)
            {
                m_customCtrl = new myDateTimePicker(this);
            }
            else if (m_tblInfo.m_cols[col].m_lookupData != null)
            {
                m_customCtrl = new myComboBox(this, m_tblInfo.m_cols[col].m_lookupData);
            }
            if (m_customCtrl != null)
            {
                m_customCtrl.m_iRow = row;
                m_customCtrl.m_iCol = col;
                this.Controls.Add(m_customCtrl.getControl());
                if (CurrentCell.Value != null)
                {
                    m_customCtrl.setValue(this.CurrentCell.Value.ToString());
                }
                Rectangle rec = this.GetCellDisplayRectangle(col, row, true);
                m_customCtrl.show(rec);

                //ActiveControl = m_dtp;
                this.BeginEdit(true);
            }
        }
        public virtual void HideCustomCtrl()
        {
            if (m_customCtrl != null)
            {
                Debug.WriteLine("hideDtp");
                m_customCtrl.hide();

                if (m_customCtrl.isChanged())
                {
                    var val = m_customCtrl.getValue();
                    this.CurrentCell.Value = m_customCtrl.getValue();
                }

                this.Controls.Remove(m_customCtrl.getControl());
                m_customCtrl.Dispose();
                m_customCtrl = null;
            }
        }
        public virtual bool HideCustomCtrl(out string val)
        {
            bool bRet = false;
            val = "";
            do
            {
                if (m_customCtrl == null) break;
                Debug.WriteLine("hideDtp");
                m_customCtrl.hide();

                if (m_customCtrl == null) break;
                if (m_customCtrl.isChanged())
                {
                    bRet = true;
                    val = m_customCtrl.getValue();
                }

                this.Controls.Remove(m_customCtrl.getControl());
                m_customCtrl.Dispose();
                m_customCtrl = null;
            } while (false);
            return bRet;
        }
#endif  //use_custom_cols

        protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
        {
            //Debug.WriteLine("OnCellFormatting");
            base.OnCellFormatting(e);

            if (e.Value == null) return;
            var col = m_tblInfo.m_cols[e.ColumnIndex];
            switch (col.m_type)
            {
                case TableInfo.ColInfo.ColType.dateTime:

                    break;
                case TableInfo.ColInfo.ColType.map:
                    string txt;
                    int n;
                    if (int.TryParse(e.Value.ToString(), out n))
                    {
                        if (col.ParseEnum(n, out txt))
                        {
                            e.Value = txt;
                            e.FormattingApplied = true;
                        }
                    }
                    break;
            }
        }

        class myDtFp : IFormatProvider
        {
            public object GetFormat(Type formatType)
            {
                return lConfigMng.GetDisplayDateFormat();
            }
        }

        protected override void OnCellParsing(DataGridViewCellParsingEventArgs e)
        {
            base.OnCellParsing(e);
            var col = m_tblInfo.m_cols[e.ColumnIndex];
            switch (col.m_type)
            {
                case TableInfo.ColInfo.ColType.dateTime:
                    Debug.WriteLine("OnCellParsing parsing date");

                    if (lConfigMng.GetDisplayDateFormat() == "dd/MM/yyyy")
                    {
                        bool bChg = false;
                        string val = e.Value.ToString();
                        if (m_customCtrl != null)
                        {
                            bChg = HideCustomCtrl(out val);
                        }
                        DateTime dt;
                        if (lConfigMng.ParseDisplayDate(val, out dt))
                        {
                            e.ParsingApplied = true;
                            e.Value = dt;
                        }
                    }

                    break;
                case TableInfo.ColInfo.ColType.map:
                    {
                        Debug.WriteLine("OnCellParsing parsing enum");
                        string val = e.Value.ToString();
                        int n;
                        if (col.ParseEnum(val, out n))
                        {
                            e.ParsingApplied = true;
                            e.Value = n;
                        }
                    }
                    break;
            }
        }

        protected override void OnDataBindingComplete(DataGridViewBindingCompleteEventArgs e)
        {
            base.OnDataBindingComplete(e);


#if !manual_crt_dgv_columns
            if (AutoGenerateColumns == true)
            {
                updateCols();
                AutoGenerateColumns = false;
            }
#endif
            //fix col["ID"] not hide
            if (Columns[0].Visible)
            {
                Columns[0].Visible = false;
            }
        }
        private void updateCols()
        {
            Columns[0].Visible = false;
            TableInfo tblInfo = m_tblInfo;
            int i = 1;
            for (; i < ColumnCount; i++)
            {
                //show hide columns
                if (tblInfo.m_cols[i].m_visible == false)
                {
                    Columns[i].Visible = false;
                    continue;
                }

                Columns[i].HeaderText = tblInfo.m_cols[i].m_alias;

#if header_blue
                //header color blue
                m_dataGridView.Columns[i].HeaderCell.Style.BackColor = Color.Blue;
                m_dataGridView.Columns[i].HeaderCell.Style.ForeColor = Color.White;
#endif

                switch (tblInfo.m_cols[i].m_type)
                {
                    case TableInfo.ColInfo.ColType.currency:
                        Columns[i].DefaultCellStyle.Format = lConfigMng.getCurrencyFormat();
                        break;
                    case TableInfo.ColInfo.ColType.dateTime:
                        Columns[i].DefaultCellStyle.Format = lConfigMng.GetDisplayDateFormat();
                        break;
                }
#if false
                    m_dataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    m_dataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    m_dataGridView.Columns[i].FillWeight = 1;
#endif
            }
            Columns[i - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Columns[i - 1].FillWeight = 1;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_dataTable.TableNewRow -= Dt_TableNewRow;
                if (m_customCtrl != null)
                    m_customCtrl.Dispose();
            }
            base.Dispose(disposing);
        }
    }
    public class lInterPaymentDGV : lCustomDGV
    {
        public lInterPaymentDGV(TableInfo tblInfo) : base(tblInfo)
        {
        }
        protected override void OnCellEndEdit(DataGridViewCellEventArgs e)
        {
            base.OnCellEndEdit(e);
            if (m_tblInfo.m_cols[e.ColumnIndex].m_field == "reimbursement")
            {
                try
                {
                    Int64 advance = (Int64)Rows[e.RowIndex].Cells["advance_payment"].Value;
                    Int64 remain = (Int64)Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    this.Rows[e.RowIndex].Cells["actually_spent"].Value = advance - remain;
                }
                catch
                {
                    //if cannot covert advance & remain => not auto fill actually_spent
                    Debug.WriteLine("{0} {1} cannot auto fill actually_spent", this, "OnCellEndEdit");
                }
            }
        }
    }
    public class lSalaryDGV : lCustomDGV
    {
        public lSalaryDGV(TableInfo tblInfo) : base(tblInfo)
        {
        }
        protected override void OnCellEndEdit(DataGridViewCellEventArgs e)
        {
            base.OnCellEndEdit(e);
            string field = m_tblInfo.m_cols[e.ColumnIndex].m_field;
            if (field == "date")
            {
                var val = Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                if (val != DBNull.Value)
                {
                    DateTime cur = (DateTime)val;
                    if (Rows[e.RowIndex].Cells["month"].Value == DBNull.Value)
                        this.Rows[e.RowIndex].Cells["month"].Value = cur.Month;
                }
            }
            if ((field == "bsalary") || (field == "esalary"))
            {
                Int64 sum = 0;
                var val = Rows[e.RowIndex].Cells["bsalary"].Value;
                if (val != DBNull.Value) { sum += (long)val; }
                val = Rows[e.RowIndex].Cells["esalary"].Value;
                if (val != DBNull.Value) { sum += (long)val; }
                Rows[e.RowIndex].Cells["salary"].Value = sum;
            }
        }
    }

    public class myMenuItem : MenuItem
    {
        private Font _font;
        public Font Font
        {
            get
            {
                return _font;
            }
            set
            {
                _font = value;
            }
        }

        public myMenuItem()
        {
            OwnerDraw = true;
            Font = lConfigMng.getFont();
        }

        public myMenuItem(string text)
            : this()
        {
            Text = text;
        }

        // ... Add other constructor overrides as needed

        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            // I would've used a Graphics.FromHwnd(this.Handle) here instead,
            // but for some reason I always get an OutOfMemoryException,
            // so I've fallen back to TextRenderer

            var size = TextRenderer.MeasureText(Text, Font);
            e.ItemWidth = size.Width;
            e.ItemHeight = size.Height;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            Debug.WriteLine(string.Format("OnDrawItem {0}", e.State));

            //e.DrawBackground();
            Color cl = Color.Transparent;
            var brush = Brushes.Black;
            Brush br = new SolidBrush(cl);

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                br = new SolidBrush(Color.Blue);
            }
            else if ((e.State & DrawItemState.HotLight) == DrawItemState.HotLight)
            {
                br = new SolidBrush(Color.Blue);
            }
            else if ((e.State & DrawItemState.Inactive) == DrawItemState.Inactive)
            {
                br = new SolidBrush(Color.Silver);
            }
            else if ((e.State & DrawItemState.NoAccelerator) == DrawItemState.NoAccelerator)
            {
                br = new SolidBrush(Color.Silver);
            }
            e.Graphics.FillRectangle(br, e.Bounds);

            SolidBrush fontColor = new SolidBrush(e.ForeColor);
            e.Graphics.DrawString(Text, Font, fontColor, e.Bounds);

#if false
            SizeF sz = e.Graphics.MeasureString(Text, Font);
            e.Graphics.DrawString(Text, Font, brush, e.Bounds);

            Rectangle rect = e.Bounds;
            rect.Offset(0, 1);
            rect.Inflate(0, -1);
            e.Graphics.DrawRectangle(Pens.DarkGray, rect);
            e.DrawFocusRectangle();
#endif
        }
    }
#endif

    public class TableInfo
    {
        public static int GetCount<Tenum>()
        {
            return Enum.GetValues(typeof(Tenum)).Length;
        }
        //#define col_class
#if col_class
        [DataContract(Name = "ColInfo")]
        public class ColInfo
        {
            [DataContract(Name = "ColType")]
            public enum ColType
            {
                [EnumMember]
                text,
                [EnumMember]
                dateTime,
                [EnumMember]
                num,
                [EnumMember]
                currency,
                [EnumMember]
                uniq,
                [EnumMember]
                map,
            };
            [DataMember(Name = "field", EmitDefaultValue = false)]
            public string m_field;
            [DataMember(Name = "alias", EmitDefaultValue = false)]
            public string m_alias;
            [DataMember(Name = "lookupTbl", EmitDefaultValue = false)]
            public string m_lookupTbl;
            [DataMember(Name = "type", EmitDefaultValue = false)]
            public ColType m_type;
            [DataMember(Name = "visible", EmitDefaultValue = false)]
            public bool m_visible;
            [DataMember(Name = "lst", EmitDefaultValue = false)]
            public string m_lst;        //"idx,val;" ???
            
            public void Init(string field, string alias, ColType type, string lookupTbl = null, bool visible = true, string lst = null)
            {
                m_lookupTbl = lookupTbl;
                m_field = field;
                m_alias = alias;
                m_type = type;
                m_visible = visible;
                m_lst = lst;
            }
            public ColInfo(string field, string alias, ColType type, string lookupTbl, bool visible)
            {
                Init(field, alias, type, lookupTbl, visible);
            }

            public ColInfo(string field, string alias, ColType type, string param)
            {
                switch (type)
                {
                    case ColType.map:
                        Init(field, alias, type, null, true, param);
                        break;
                    default:
                        Debug.Assert(type == ColType.text);
                        Init(field, alias, type, param);
                        break;
                }
            }
            public ColInfo(string field, string alias, ColType type, bool visible = true)
            {
                Init(field, alias, type, null, visible);
            }

            public string GetHelp()
            {
                if (m_dict == null) { InitDict(); }
                string txt = string.Format("Please input number from 0 to {0}", m_dict.Count-1);
                for (int i = 0; i < m_dict.Count;i++)
                {
                    txt = txt + "\n" + string.Format("  {0} ({1})", i, m_dict.Keys.ElementAt(i));
                }
                return txt;
            }

            Dictionary<string, int> m_dict;
            private DataTable dataTable;
            public string DisplayMember = "name";
            public string ValueMember = "val";
            public DataTable DataTable {
                get {
                    if (dataTable == null) {
                        Dictionary<string, int> dict = GetDict();
                        dataTable = new DataTable();
                        dataTable.Columns.Add("name");
                        dataTable.Columns.Add("val");
                        for (int idx = 0; idx < dict.Count; idx++)
                        {
                            var newRow = dataTable.NewRow();
                            newRow[0] = dict.Keys.ElementAt(idx);
                            newRow[1] = idx;
                            dataTable.Rows.Add(newRow);
                        }
                    }
                    return dataTable;
                } }
            public Dictionary<string,int> GetDict()
            {
                if (m_dict == null) { InitDict(); }
                return m_dict;
            }
            public bool ParseEnum(int n, out string txt)
            {
                if (m_dict == null) { InitDict(); }
                bool ret = (n < m_dict.Count);
                txt = null;
                if (ret) { txt = m_dict.Keys.ElementAt(n); }
                return ret;
            }
            
            private void InitDict()
            {
                    m_dict = new Dictionary<string, int>();
                    var arr = m_lst.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < arr.Length;i++)
                    {
                        m_dict.Add(arr[i], i);
                    }
            }
            public bool ParseEnum(string txt, out int n )
            {
                if (m_dict == null) {InitDict();}
                bool ret = m_dict.ContainsKey(txt);
                n = -1;
                if (ret) { n = m_dict[txt]; }
                return ret;
            }
        };

        [DataMember(Name = "cols", EmitDefaultValue = false)]
        public ColInfo[] m_cols;
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string m_tblName;
        [DataMember(Name = "alias", EmitDefaultValue = false)]
        public string m_tblAlias;
        [DataMember(Name = "crtSql", EmitDefaultValue = false)]
        public string m_crtQry;

        public virtual void LoadData()
        {
            foreach (ColInfo colInfo in m_cols)
            {
                if (colInfo.m_lookupTbl != null)
                {
                    //use columns[1] - zero base
                }
            }
        }
#else
        public struct lColInfo
        {
            public enum lColType
            {
                text,
                dateTime,
                num
            };
            public string m_field;
            public string m_alias;
            public lColType m_type;
        };
        public virtual lColInfo[] getColsInfo() { return null; }
#endif
        public int getColIndex(string colName)
        {
            int i = 0;
            foreach (ColInfo col in m_cols)
            {
                if (col.m_field == colName)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        protected void CrtCols(object[][] map, int n)
        {
            m_cols = new ColInfo[n];
            for (int i = 0; i < map.Length; i++)
            {
                int iCol = (int)map[i][0];
                string field = (string)map[i][1];
                string alias = (string)map[i][2];
                ColInfo.ColType type = (ColInfo.ColType)map[i][3];
                string lookupTbl = (string)map[i][4];
                bool visible = (bool)map[i][5];
                m_cols[iCol] = new ColInfo(field, alias, type, lookupTbl, visible);
            }
        }

        public static string GetDescLst<Tenum>()
        {
            string txt = "";
            var type = typeof(Tenum);
            var arr = Enum.GetValues(type);

            foreach (var v in arr)
            {
                var memberInfo = type.GetMember(v.ToString());
                var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                var attr = attributes.Length > 0
                  ? (DescriptionAttribute)attributes[0]
                  : null;
                txt += attr.Description + ";";
            }
            return txt;
        }
    }

    #region enum_attr
    public static class EnumExtensions
    {
        // This extension method is broken out so you can use a similar pattern with 
        // other MetaData elements in the future. This is your base method for each.
        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0
              ? (T)attributes[0]
              : null;
        }

        // This method creates a specific call to the above method, requesting the
        // Description MetaData attribute.
        public static string ToDesc(this Enum value)
        {
            var attribute = value.GetAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static string ToField(this Enum value)
        {
            var attribute = value.GetAttribute<FieldAttribute>();
            return attribute == null ? value.ToString() : attribute.Field;
        }
        public static string ToAlias(this Enum value)
        {
            var attribute = value.GetAttribute<AliasAttribute>();
            return attribute == null ? value.ToString() : attribute.Alias;
        }
    }

    public class FieldAttribute : Attribute
    {
        private string name;
        public FieldAttribute(string name)
        {
            this.name = name;
        }
        public virtual string Field { get { return name; } }
        protected string FieldValue { get; set; }
    }
    public class AliasAttribute : Attribute
    {
        private string name;
        public AliasAttribute(string name)
        {
            this.name = name;
        }
        public virtual string Alias { get { return name; } }
        protected string AliasValue { get; set; }
    }
    #endregion

    public class HumanTblInfo : TableInfo
    {
        public enum ColIdx
        {
            [Field("ID"), Alias("ID")] ID,
            [Field("human_number"), Alias("Mã NS")] Human,
            [Field("name"), Alias("Họ tên")] Name,
            [Field("start_date"), Alias("Ngày vào")] Enter,
            [Field("end_date"), Alias("Ngày ra")] Leave,
            [Field("gender"), Alias("Giới tính")] Gndr,
            [Field("age"), Alias("Tuổi")] Age,
            [Field("status"), Alias("Đang bận")] Busy,
            [Field("note"), Alias("Ghi Chú")] Note,

            Count
        }
        public enum ResStatus
        {
            [Description("Free")] Free,
            [Description("Busy")] Busy,
        }
        public enum Gender
        {
            [Description("Nam")] Male,
            [Description("Nữ")] Female,
        }
        public HumanTblInfo()
        {
            m_tblName = "human";
            m_tblAlias = "Nhân Sự";
            m_crtQry = "CREATE TABLE if not exists human("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                + "human_number char(31),"
                + "name char(31),"
                + "start_date datetime,"
                + "end_date datetime,"
                + "gender INTEGER,"
                + "age INTEGER,"
                + "status INTEGER,"
                + "note text)";
            m_cols = new ColInfo[(int)ColIdx.Count];
            m_cols[(int)ColIdx.ID] = new ColInfo(ColIdx.ID.ToField(), ColIdx.ID.ToAlias(), ColInfo.ColType.num, false);
            m_cols[(int)ColIdx.Human] = new ColInfo(ColIdx.Human.ToField(), ColIdx.Human.ToAlias(), ColInfo.ColType.uniq);
            m_cols[(int)ColIdx.Name] = new ColInfo(ColIdx.Name.ToField(), ColIdx.Name.ToAlias(), ColInfo.ColType.text);
            m_cols[(int)ColIdx.Enter] = new ColInfo(ColIdx.Enter.ToField(), ColIdx.Enter.ToAlias(), ColInfo.ColType.dateTime);
            m_cols[(int)ColIdx.Leave] = new ColInfo(ColIdx.Leave.ToField(), ColIdx.Leave.ToAlias(), ColInfo.ColType.dateTime);
            m_cols[(int)ColIdx.Gndr] = new ColInfo(ColIdx.Gndr.ToField(), ColIdx.Gndr.ToAlias(), ColInfo.ColType.map, GetDescLst<Gender>());
            m_cols[(int)ColIdx.Age] = new ColInfo(ColIdx.Age.ToField(), ColIdx.Age.ToAlias(), ColInfo.ColType.num);
            m_cols[(int)ColIdx.Busy] = new ColInfo(ColIdx.Busy.ToField(), ColIdx.Busy.ToAlias(), ColInfo.ColType.map, GetDescLst<ResStatus>());
            m_cols[(int)ColIdx.Note] = new ColInfo(ColIdx.Note.ToField(), ColIdx.Note.ToAlias(), ColInfo.ColType.text);
        }
    };
}
