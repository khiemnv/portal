//#define use_custom_cols
#define format_currency
#define use_sqlite
//#define check_number_input

using System.Windows.Forms;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using System.Globalization;

namespace test_binding
{
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
            m_dtp.CustomFormat = lConfigMng.getDisplayDateFormat();
            m_dtp.ValueChanged += ctrl_ValueChanged;
        }
        public override Control getControl()
        {
            return m_dtp;
        }
        public override string getValue()
        {
            return m_dtp.Value.ToString(lConfigMng.getDisplayDateFormat());
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
        protected lTableInfo m_tblInfo;
        myCustomCtrl m_customCtrl;
        DataRow m_newRow;
        DataTable m_dataTable;

        public lCustomDGV(lTableInfo tblInfo)
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
                        case lTableInfo.lColInfo.lColType.currency:
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
            var colInfo = m_tblInfo.m_cols[e.ColumnIndex];
            string val = e.FormattedValue.ToString();
            switch (colInfo.m_type)
            {
                case lTableInfo.lColInfo.lColType.uniqueText:
                    {
                        string rowid = Rows[e.RowIndex].Cells[0].Value.ToString();
#if use_sqlite
                        string sql = string.Format("select rowid, {0} from {1} where {0} = '{2}'",
                            colInfo.m_field, m_tblInfo.m_tblName, val);
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
            hideCustomCtrl();
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
            do
            {
                if (m_customCtrl != null) break;

                m_autoCompleteData = m_tblInfo.m_cols[CurrentCell.ColumnIndex].m_lookupData;
                if (m_autoCompleteData == null) break;

                AutoCompleteStringCollection col = m_autoCompleteData.m_colls;
                DataGridViewTextBoxEditingControl edt = (DataGridViewTextBoxEditingControl)e.Control;
                edt.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                edt.AutoCompleteSource = AutoCompleteSource.CustomSource;
                edt.AutoCompleteCustomSource = col;

                edt.Validated += Edt_Validated;
            } while (false);
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

            if (m_tblInfo.m_cols[col].m_type == lTableInfo.lColInfo.lColType.dateTime)
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
        public virtual void hideCustomCtrl()
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
        public virtual bool hideCustomCtrl(out string val)
        {
            bool bRet = false;
            val = "";
            if (m_customCtrl != null)
            {
                Debug.WriteLine("hideDtp");
                m_customCtrl.hide();

                if (m_customCtrl.isChanged())
                {
                    bRet = true;
                    val = m_customCtrl.getValue();
                }

                this.Controls.Remove(m_customCtrl.getControl());
                m_customCtrl.Dispose();
                m_customCtrl = null;
            }
            return bRet;
        }
#endif  //use_custom_cols

        protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
        {
            //Debug.WriteLine("OnCellFormatting");
            base.OnCellFormatting(e);
            var col = m_tblInfo.m_cols[e.ColumnIndex];
            switch (col.m_type)
            {
                case lTableInfo.lColInfo.lColType.dateTime:

                    break;
            }
        }

        class myDtFp : IFormatProvider
        {
            public object GetFormat(Type formatType)
            {
                return lConfigMng.getDisplayDateFormat();
            }
        }

        protected override void OnCellParsing(DataGridViewCellParsingEventArgs e)
        {
            base.OnCellParsing(e);
            var col = m_tblInfo.m_cols[e.ColumnIndex];
            switch (col.m_type)
            {
                case lTableInfo.lColInfo.lColType.dateTime:
                    Debug.WriteLine("OnCellParsing parsing date");

                    if (lConfigMng.getDisplayDateFormat() == "dd/MM/yyyy")
                    {
                        bool bChg = false;
                        string val = e.Value.ToString();
                        if (m_customCtrl != null)
                        {
                            bChg = hideCustomCtrl(out val);
                        }
                        DateTime dt;
                        if (lConfigMng.parseDisplayDate(val, out dt))
                        {
                            e.ParsingApplied = true;
                            e.Value = dt;
                        }
                    }
                    
                    break;
            }
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
        public lInterPaymentDGV(lTableInfo tblInfo) : base(tblInfo)
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
        public lSalaryDGV(lTableInfo tblInfo) : base(tblInfo)
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
}