#define format_currency
#define use_sqlite
//#define use_bg_work

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;

namespace test_binding
{
    [DataContract(Name = "InputCtrl")]
    public class InputCtrl : SearchCtrl
    {
        protected new Label m_label;
        public InputCtrl() { }
        public InputCtrl(string fieldName, string alias, CtrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
            m_label = lConfigMng.crtLabel();
            m_label.Text = alias + " : ";
            m_label.TextAlign = ContentAlignment.MiddleLeft;
            m_panel.BorderStyle = BorderStyle.None;
        }
        public virtual bool ReadOnly { get; set; }
        public virtual string Text { get; set; }
        //private EventHandler<string> m_EditingCompleted;
        protected virtual void addEvent(EventHandler<string> handler)
        {
            //m_EditingCompleted += handler;
            throw new NotImplementedException();
        }
        public event EventHandler<string> EditingCompleted {
            add { addEvent(value); }
            remove { }
        }
        protected virtual void onEditingCompleted()
        {
            //if (m_EditingCompleted != null) { m_EditingCompleted(this, Text); }
            throw new NotImplementedException();
        }
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        m_label.Dispose();
        //    }
        //    base.Dispose();
        //}
    }

    [DataContract(Name = "InputCtrlText")]
    public class lInputCtrlText : InputCtrl
    {
        protected TextBox m_text;
        ComboBox m_combo;
        string m_value
        {
            get
            {
                if (m_text != null) return m_text.Text;
                else return m_combo.Text;
            }
        }
        public lInputCtrlText(string fieldName, string alias, CtrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
            m_text = lConfigMng.crtTextBox();
            m_text.Width = 200;

            m_panel.Controls.AddRange(new Control[] { m_label, m_text });
        }
        public override void UpdateInsertParams(List<string> exprs, List<SearchParam> srchParams)
        {
            exprs.Add(m_fieldName);
            srchParams.Add(
                new SearchParam()
                {
                    key = string.Format("@{0}", m_fieldName),
                    val = m_value
                }
            );
        }
#if use_auto_complete
        lDataSync m_autoCompleteData;
#endif
        public override void LoadData()
        {
            if (m_colInfo != null && m_colInfo.m_lookupData != null)
            {
#if use_auto_complete
                m_text.Validated += M_text_Validated;
                m_autoCompleteData = m_colInfo.m_lookupData;
                AutoCompleteStringCollection col = m_autoCompleteData.m_colls;
                m_text.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                m_text.AutoCompleteSource = AutoCompleteSource.CustomSource;
                m_text.AutoCompleteCustomSource = col;
#endif  //use_auto_complete
#if true    //use combo
                m_combo = lConfigMng.crtComboBox();
                m_text.Hide();

                m_combo.Size = m_text.Size;

                m_panel.Controls.Remove(m_text);
                m_panel.Controls.Add(m_combo);

                DataTable tbl = m_colInfo.m_lookupData.m_dataSource;
                BindingSource bs = new BindingSource();
                bs.DataSource = tbl;
                m_combo.DataSource = bs;
                m_combo.DisplayMember = tbl.Columns[1].ColumnName;

                m_combo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                m_combo.AutoCompleteSource = AutoCompleteSource.CustomSource;
                AutoCompleteStringCollection col = m_colInfo.m_lookupData.m_colls;
                m_combo.AutoCompleteCustomSource = col;
                m_combo.Validated += M_combo_Validated;

                m_text.Dispose();
                m_text = null;

                m_combo.SelectedValueChanged += M_combo_SelectedValueChanged;
#endif
            }
        }

        //public event EventHandler<string> EditingCompleted;
        private EventHandler<string> m_EditingCompleted;
        protected override void addEvent(EventHandler<string> handler)
        {
            m_EditingCompleted += handler;
        }
        protected override void onEditingCompleted()
        {
            if (m_EditingCompleted != null) { m_EditingCompleted(this, Text); }
        }
        private void M_combo_SelectedValueChanged(object sender, EventArgs e)
        {
            onEditingCompleted();
        }
#if use_auto_complete
        private void M_text_Validated(object sender, EventArgs e)
        {
            TextBox edt = (TextBox)sender;
            Debug.WriteLine("M_text_Validated:" + edt.Text);
            string selectedValue = edt.Text;
            if (selectedValue != "")
            {
                m_autoCompleteData.Update(selectedValue);
            }
        }
#endif

        private void M_combo_Validated(object sender, EventArgs e)
        {
            string key = m_combo.Text;
            string val = m_colInfo.m_lookupData.find(key);
            if (val != null)
                m_combo.Text = val;
        }
        public override bool ReadOnly
        {
            get
            {
                if (m_text != null)
                {
                    return m_text.ReadOnly;
                }
                else
                {
                    return m_combo.Enabled;
                }
            }

            set
            {
                if (m_text != null) {
                    m_text.ReadOnly = value;
                    m_text.TabStop = !value;
                }
                else
                {
                    m_combo.Enabled = value;
                    m_combo.TabStop = !value;
                    m_combo.DropDownStyle = ComboBoxStyle.DropDownList;
                }
            }
        }
        public override string Text
        {
            get
            {
                return m_value;
            }
            set
            {
                if (m_text != null)
                {
                    m_text.Text = value;
                }
                else
                {
                    m_combo.Text = value;
                }
            }

        }

    }
    [DataContract(Name = "InputCtrlDate")]
    public class lInputCtrlDate : InputCtrl
    {
        private DateTimePicker m_date = new DateTimePicker();
        public lInputCtrlDate(string fieldName, string alias, CtrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
#if fit_txt_size
            int w = lConfigMng.getWidth(lConfigMng.getDateFormat()) + 20;
#else
            int w = 100;
#endif
            m_date.Width = w;
            m_date.Format = DateTimePickerFormat.Custom;
            m_date.CustomFormat = lConfigMng.getDisplayDateFormat();
            m_date.Font = lConfigMng.getFont();

            m_panel.Controls.AddRange(new Control[] { m_label, m_date });
        }

        public override void UpdateInsertParams(List<string> exprs, List<SearchParam> srchParams)
        {
            string zStartDate = m_date.Value.ToString(lConfigMng.getDateFormat());
            exprs.Add(m_fieldName);
            srchParams.Add(
                new SearchParam()
                {
                    key = string.Format("@{0}", m_fieldName),
                    val = string.Format("{0} 00:00:00", zStartDate),
                    type = DbType.Date
                }
            );
        }
    }
    [DataContract(Name = "InputCtrlNum")]
    public class lInputCtrlNum : lInputCtrlText
    {
        public lInputCtrlNum(string fieldName, string alias, CtrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
            m_text.KeyPress += onKeyPress;
        }
        private void onKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
    [DataContract(Name = "InputCtrlCurrency")]
    public class lInputCtrlCurrency : InputCtrl
    {
        private TextBox m_val = lConfigMng.crtTextBox();
        //private Label m_lab = lConfigMng.crtLabel();
        public lInputCtrlCurrency(string fieldName, string alias, CtrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
#if fit_txt_size
            int w = lConfigMng.getWidth("000,000,000,000");
#else
            int w = 100;
#endif
            m_val.Width = w;
            m_val.RightToLeft = RightToLeft.Yes;
            m_val.KeyPress += onKeyPress;
            m_val.KeyUp += onKeyUp;
            m_val.Validated += M_val_Validated;
            m_panel.Controls.AddRange(new Control[] { m_label, m_val });
        }

        //public event EventHandler<string> EditingCompleted;
        private EventHandler<string> m_EditingCompleted;
        protected override void addEvent(EventHandler<string> handler)
        {
            m_EditingCompleted += handler;
        }
        protected override void onEditingCompleted()
        {
            if (m_EditingCompleted != null) { m_EditingCompleted(this, Text); }
        }
        private void M_val_Validated(object sender, EventArgs e)
        {
            onEditingCompleted();
        }

        private void onKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        int selectStart;
        private void onKeyUp(object sender, KeyEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (e.KeyCode == Keys.Back)
            {
                //0,000 ->0000
                string txt = tb.Text;
                char[] buff = new char[txt.Length];
                int s = 0;
                int i = txt.Length - 1;
                for (; i >= 0; i--)
                {
                    char ch = txt[i];
                    s++;
                    if (ch == ',') { s = 0; }
                    if (s == 4)
                    {
                        string newVal = "";
                        if (i > 0) newVal = txt.Substring(0, i);
                        newVal = newVal + new string(buff, i + 1, txt.Length - i - 1);
                        selectStart = tb.SelectionStart - 1;
                        chgTxt(tb,newVal);
                        return;
                    }
                    buff[i] = ch;
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                //0,000 ->0000
                string txt = tb.Text;
                char[] buff = new char[txt.Length];
                int s = 0;
                int i = txt.Length - 1;
                for (; i >= 0; i--)
                {
                    char ch = txt[i];
                    s++;
                    if (ch == ',') { s = 0; }
                    if (s == 4)
                    {
                        string newVal = "";
                        newVal = txt.Substring(0, i + 1);
                        newVal = newVal + new string(buff, i + 2, txt.Length - i - 2);
                        selectStart = tb.SelectionStart;
                        chgTxt(tb, newVal);
                        return;
                    }
                    buff[i] = ch;
                }
            }

            selectStart = tb.SelectionStart;
            chgTxt(tb,tb.Text);
        }

        private void chgTxt(TextBox tb, string val)
        {
            Int64 amount = 0;
            //display in 000,000
            char[] buff = new char[64];
            Debug.Assert(val.Length < 48, "currency too long");
            int j = 63;
            for (int i = val.Length; i > 0; i--)
            {
                char ch = val[i - 1];
                if (ch >= '0' && ch <= '9')
                {
                    amount = amount * 10 + (ch - '0');
                    if (j % 4 == 0)
                    {
                        buff[j] = ',';
                        j--;
                    }
                    buff[j] = ch;
                    j--;
                }
            }
            string newVal = new string(buff, j + 1, 63 - j);
            tb.Text = newVal;

            selectStart += newVal.Length - val.Length;
            if (selectStart >= 0) { tb.Select(selectStart, 0); }

            //update size
            int w = lConfigMng.getWidth(val);
            if (w > 100) tb.Width = w;
#if display_amount_tooltip
            if (amount > 0)
            {
                tt.IsBalloon = true;
                tt.InitialDelay = 0;
                tt.ShowAlways = true;
                tt.SetToolTip(m_val, common.amountToTxt(amount));
            }
#endif
        }

        void getInputRange(out string val)
        {
            val = m_val.Text.Replace(",", "");
            if (val == "") val = "0";
        }
        public override bool ReadOnly
        {
            get
            {
                return m_val.ReadOnly;
            }

            set
            {
                m_val.ReadOnly = value;
                m_val.TabStop = !value;
            }
        }
        public override string Text
        {
            get
            {
                return m_val.Text;
            }

            set
            {
                m_val.Text = value;
            }
        }
        public override void UpdateInsertParams(List<string> exprs, List<SearchParam> srchParams)
        {
            string val;
            getInputRange(out val);
            srchParams.Add(
                new SearchParam()
                {
                    key = "@" + m_fieldName,
                    val = val,
                    type = DbType.UInt64
                }
            );
            exprs.Add(m_fieldName);
        }
    }
    [DataContract(Name = "InputCtrlEnum")]
    public class InputCtrlEnum : InputCtrl
    {
        ComboBox m_combo;
        public InputCtrlEnum(string fieldName, string alias, CtrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
            m_combo = lConfigMng.crtComboBox();
            m_combo.DropDownStyle = ComboBoxStyle.DropDownList;
            m_combo.Width = 100;
            m_panel.Controls.AddRange(new Control[] { m_label, m_combo });
        }

        //public event EventHandler<string> EditingCompleted;
        private EventHandler<string> m_EditingCompleted;
        protected override void addEvent(EventHandler<string> handler)
        {
            m_EditingCompleted += handler;
        }
        protected override void onEditingCompleted()
        {
            if (m_EditingCompleted != null) { m_EditingCompleted(this, Text); }
        }
        private void M_combo_SelectedValueChanged(object sender, EventArgs e)
        {
            onEditingCompleted();
        }
        
        public class ComboItem
        {
            public string name;
            public int val;
        }
        bool isInit = false;
        public void Init(Dictionary<string,int>dict)
        {
            var dt = new DataTable();
            dt.Columns.Add("name");
            dt.Columns.Add("val");
            for (int i = 0; i< dict.Count;i++)
            {
                var newRow = dt.NewRow();
                newRow[0] = dict.Keys.ElementAt(i);
                newRow[1] = i;
                dt.Rows.Add(newRow);
            }
            m_combo.DataSource = dt;
            m_combo.DisplayMember = "name";
            m_combo.ValueMember = "val";

            m_combo.SelectedValueChanged += M_combo_SelectedValueChanged;

            Debug.Assert(!isInit);
            isInit = true;
        }
        public override void UpdateInsertParams(List<string> exprs, List<SearchParam> srchParams)
        {
            string zVal = m_combo.SelectedValue.ToString();
            exprs.Add(m_fieldName);
            srchParams.Add(
                new SearchParam()
                {
                    key = string.Format("@{0}", m_fieldName),
                    val = zVal,
                    type = DbType.Int16
                }
            );
        }
        public override string Text
        {
            get
            {
                return m_combo.SelectedValue.ToString();
            }

            set
            {
                m_combo.Text = value;
            }
        }
    }

    public delegate void ConvertRowCompletedCb(DataRow inRow, DataRow outRow);
    public class rptAssist
    {
        DataTable m_data;
        Dictionary<string, string> m_convMap;
        public ConvertRowCompletedCb ConvertRowCompleted;
        public rptAssist(int billType, Dictionary<string, string> convMap)
        {
            m_convMap = convMap;

            m_data = new DataTable();
            m_data.Columns.Add(new DataColumn("name"));
            m_data.Columns.Add(new DataColumn("addr"));
            m_data.Columns.Add(new DataColumn("date", typeof(DateTime)));
            m_data.Columns.Add(new DataColumn("num"));
            m_data.Columns.Add(new DataColumn("content"));
            m_data.Columns.Add(new DataColumn("note"));
            m_data.Columns.Add(new DataColumn("amount", typeof(Int64)));
            m_data.Columns.Add(new DataColumn("amountTxt"));

            m_type = billType;
        }
        //receipt = 1
        //payment = 2
        int m_type;
        public DataTable getData() { return m_data; }
        public void clearData() { m_data.Clear(); }
        public void setData(DataRow dr)
        {
            m_data.Clear();
            var newRow = m_data.NewRow();
            newRow["name"] = dr[m_convMap["name"]];
            newRow["addr"] = dr[m_convMap["addr"]];
            newRow["date"] = dr[m_convMap["date"]];
            newRow["num"] = dr[m_convMap["num"]];
            newRow["content"] = dr[m_convMap["content"]];
            newRow["note"] = dr[m_convMap["note"]];
            newRow["amount"] = dr[m_convMap["amount"]];

            if (ConvertRowCompleted != null) ConvertRowCompleted(dr, newRow);

            long amount = (long)newRow["amount"];
            var amountTxt = "";
            if (amount > 0)
            {
                amountTxt = common.CurrencyToTxt(amount);
            }
            newRow["amountTxt"] = amountTxt;

            m_data.Rows.Add(newRow);
            m_data.ImportRow(newRow);
        }
        public List<ReportParameter> crtParams()
        {
            return new List<ReportParameter>()
                {
                    new ReportParameter("type", m_type.ToString())
                };
        }

        internal static rptAssist Create(string m_tblName)
        {
            rptAssist rptAsst = null;
            switch (m_tblName)
            {
        case "internal_payment":
            Dictionary<string, string> dict = new Dictionary<string, string> {
                { "name","name" },
                { "addr","addr" },
                { "date","date" },
                { "num","payment_number" },
                { "content","content" },
                { "note","note" },
                { "amount","actually_spent" },
            };
            rptAsst = new rptAssist(2, dict);

            rptAsst.ConvertRowCompleted = (inR, outR) =>
            {
                //Debug.Assert((Int64)inR["advance_payment"] > 0, "advance should not zero");
                var sts = inR["status"];
                Debug.Assert(sts != DBNull.Value);
                switch (int.Parse(sts.ToString()))
                {
                    case 0:     //advance
                        outR["amount"] = inR["advance_payment"];
                        outR["note"] = lAdvanceStatus.zAdvance;
                        break;
                    case 1:
                        //outR["amount"] = inR["actually_spent"];
                        outR["note"] = lAdvanceStatus.zRemain;
                        break;
                    case 2:
                        outR["note"] = lAdvanceStatus.zActual;
                        //outR["amount"] = inR["actually_spent"];
                        break;
                }
            };
            break;
            }
            return rptAsst;
        }
    }

    [DataContract(Name = "InputPanel")]
    public class InputPanel
    {
        public DataContent m_dataContent;

#if use_bg_work
        myWorker m_wkr;
#endif
        //convert currency to text
        protected rptAssist m_rptAsst;
        public virtual DataTable billRptData { get { return m_rptAsst.getData(); } }
        public virtual List<ReportParameter> billRptParams { get { return m_rptAsst.crtParams(); } }

        [DataMember(Name = "inputCtrls")]
        public List<InputCtrl> m_inputsCtrls;

        public class PreviewEventArgs : EventArgs
        {
            public DataTable tbl;
        }
        public event EventHandler<PreviewEventArgs> RefreshPreview;
        protected string m_tblName;
        protected TableInfo m_tblInfo { get { return appConfig.s_config.getTable(m_tblName); } }

        public TableLayoutPanel m_tbl;
        protected DataGridView m_dataGridView;
        protected FlowLayoutPanel m_flow;
        protected Button m_addBtn;
        protected Button m_saveBtn;
        public virtual void initCtrls()
        {
            //create table layout & add ctrls to
            //  +-------------------------+
            //  |search ctrl|             |
            //  +-------------------------+
            //  |search ctrl|             |
            //  +-------------------------+
            //  |    save btn|delete btn  |
            //  +-------------------------+
            //  +-------------------------+
            //  |    grid view            |
            //  +-------------------------+
            m_tbl = new TableLayoutPanel();
#if DEBUG_DRAWING
                m_tbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
#endif

            //add search ctrls to table layout
            int lastRow = 0;
            foreach (var ctrl in m_inputsCtrls)
            {
                m_tbl.Controls.Add(ctrl.m_panel, ctrl.m_pos.X, ctrl.m_pos.Y);
                m_tbl.SetColumnSpan(ctrl.m_panel, ctrl.m_size.Width);
                m_tbl.SetRowSpan(ctrl.m_panel, ctrl.m_size.Height);
                lastRow = Math.Max(lastRow, ctrl.m_pos.Y);
            }

            //flow  |    save btn|delete btn  |
            m_addBtn = lConfigMng.crtButton();
            m_addBtn.Text = "Add";
            m_addBtn.Click += M_addBtn_Click;

            m_saveBtn = lConfigMng.crtButton();
            m_saveBtn.Text = "Save";
            m_saveBtn.Click += M_saveBtn_Click;

            Button m_editBtn = lConfigMng.crtButton();
            m_editBtn.Text = "Edit";
            m_editBtn.Click += M_editBtn_Click;
            Button m_clearBtn = lConfigMng.crtButton();
            m_clearBtn.Text = "Clear";
            m_clearBtn.Click += M_clearBtn_Click;
            m_flow = new FlowLayoutPanel();
            m_flow.FlowDirection = FlowDirection.LeftToRight;
            m_flow.Controls.AddRange(new Control[] { m_addBtn, m_saveBtn, m_clearBtn });
            m_flow.AutoSize = true;

            //  add buttons to last row
            m_tbl.Controls.Add(m_flow, 0, ++lastRow);

            // add data grid view
            m_dataGridView = lConfigMng.crtDGV(m_tblInfo);
            m_dataGridView.EnableHeadersVisualStyles = false;
            m_dataGridView.Dock = DockStyle.Fill;
            m_dataGridView.CellClick += M_dataGridView_CellClick;
            
            ////fix date dd/MM/yyyy
            //m_dataGridView.CellParsing += M_dataGridView_CellParsing;
            ////enum ->string
            //m_dataGridView.CellFormatting += M_dataGridView_CellFormatting;
            ////check valid input
            //m_dataGridView.CellValidating += M_dataGridView_CellValidating;
            ////show tool tip
            //m_dataGridView.EditingControlShowing += M_dataGridView_EditingControlShowing; ;

            m_tbl.Controls.Add(m_dataGridView, 0, ++lastRow);
            m_tbl.Dock = DockStyle.Fill;
            m_tbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            m_tbl.RowCount = lastRow;
        }

        private void M_dataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            var col = m_tblInfo.m_cols[m_dataGridView.CurrentCell.ColumnIndex];
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
            }
        }

        private void M_dataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            var col = m_tblInfo.m_cols[e.ColumnIndex];
            switch (col.m_type)
            {
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
                        lConfigMng.showInputError(msg);
                        e.Cancel = true;
                    }
                    break;
            }
        }
        private void M_dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null) return;

            var col = m_tblInfo.m_cols[e.ColumnIndex];
            switch (col.m_type)
            {
                case TableInfo.ColInfo.ColType.map:
                    string txt;
                    int n;
                    if( int.TryParse(e.Value.ToString(), out n))
                    {
                        if (col.ParseEnum(n,out txt))
                        {
                            e.Value = txt;
                            e.FormattingApplied = true;
                        }
                    }
                    break;
            }
        }

        private void M_dataGridView_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            var col = m_tblInfo.m_cols[e.ColumnIndex];
            switch (col.m_type)
            {
                case TableInfo.ColInfo.ColType.dateTime:
                    {
                        Debug.WriteLine("OnCellParsing parsing date");
                        if (lConfigMng.getDisplayDateFormat() == "dd/MM/yyyy")
                        {
                            string val = e.Value.ToString();
                            DateTime dt;
                            if (lConfigMng.parseDisplayDate(val, out dt))
                            {
                                e.ParsingApplied = true;
                                e.Value = dt;
                            }
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

        //
        protected virtual void onDGV_CellClick()
        {

        }
        private void M_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var rows = m_dataGridView.SelectedRows;
            if (rows.Count > 0)
            {
                var row = rows[0];
                DataRow dr = ((DataRowView)row.DataBoundItem).Row;
                //m_rptAsst.setData(dr);

                //if (RefreshPreview != null) { RefreshPreview(this, null); }
            }
            onDGV_CellClick();
        }

        private void Clear()
        {
            m_dataGridView.CancelEdit();
            m_dataContent.m_dataTable.Clear();

            //clean bills
            //m_rptAsst.clearData();
            //if (RefreshPreview != null) { RefreshPreview(this, null); }

#if use_bg_work
            m_wkr.qryFgTask(new FgTask
            {
                sender = "IP," + m_tblName,
                receiver = "DP," + m_tblName,
                eType = FgTask.fgTaskType.DP_FG_UPDATESUM,
            }, true);
#endif
        }
        private void M_clearBtn_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void M_editBtn_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        bool bIncKeyReq;
        private void M_addBtn_Click(object sender, EventArgs e)
        {
            Add();
        }
        protected virtual void Add()
        {
            bool bRet = false;
            DataRow newRow = null;
            List<string> exprs = new List<string>();
            List<SearchParam> srchParams = new List<SearchParam>();
            foreach (SearchCtrl ctrl in m_inputsCtrls)
            {
                ctrl.UpdateInsertParams(exprs, srchParams);
            }
            string key = m_keyCtrl.Text;

            m_dataContent.BeginTrans();
            do
            {
                //check key is unique
                if (!m_keyMng.IsUniqKey(key))
                {
                    //case: multi users
                    Debug.Assert(false, "other user inputting");
                    lConfigMng.showInputError("Mã này đã tồn tại");
                    m_keyCtrl.Text = m_keyMng.IncKey();
                    break;
                }

                //crt new row
                DataTable tbl = m_dataContent.m_dataTable;
                newRow = tbl.NewRow();
                for (int i = 0; i < exprs.Count; i++)
                {
                    string field = exprs[i];
                    var newValue = srchParams[i].val;
                    newRow[field] = newValue;
                }
                tbl.Rows.Add(newRow);
                m_dataContent.Submit();

                bRet = true;
            } while (false);
            m_dataContent.EndTrans();
#if use_bg_work
            m_wkr.qryFgTask(new FgTask
            {
                sender = "IP," + m_tblName,
                receiver = "DP," + m_tblName,
                eType = FgTask.fgTaskType.DP_FG_UPDATESUM,
            }, true);
#endif
            //add new rec success
            if (bRet)
            {
                //inc key
                bIncKeyReq = true;
                //add new record to bills
                //m_rptAsst.setData(newRow);
                //if (RefreshPreview != null) { RefreshPreview(this, null); }
            }
                
            //clear control text
            ClearInputCtrls();
        }

        protected virtual void ClearInputCtrls()
        {
            int i = 2;
            for (; i < m_inputsCtrls.Count; i++)
            {
                var ctrl = m_inputsCtrls[i];
                ctrl.Text = "";
            }
        }

        protected virtual InputCtrl m_keyCtrl { get; }

        protected virtual keyMng m_keyMng { get; }
        protected virtual string InitKey()
        {
            return m_keyMng.InitKey();
        }
        protected virtual string IncKey()
        {
            return m_keyMng.IncKey();
        }
        private void M_saveBtn_Click(object sender, EventArgs e)
        {
            Save();

            //if (RefreshPreview != null)
            //{
            //    RefreshPreview(this, new PreviewEventArgs { tbl = m_dataContent.m_dataTable });
            //}
        }
        protected virtual void Save()
        {
            //billRptData.Clear();
            //if (RefreshPreview != null) { RefreshPreview(this, null); }

            m_dataContent.Submit();
#if use_bg_work
            m_wkr.qryFgTask(new FgTask {
                sender = "IP," + m_tblName,
                receiver = "DP," + m_tblName,
                eType = FgTask.fgTaskType.DP_FG_UPDATESUM,
            }, true);
#endif
        }
        protected virtual void InsertRec()
        {
            List<string> exprs = new List<string>();
            List<SearchParam> srchParams = new List<SearchParam>();
            foreach (SearchCtrl ctrl in m_inputsCtrls)
            {
                ctrl.UpdateInsertParams(exprs, srchParams);
            }

            m_dataContent.AddRec(exprs, srchParams);
        }
        protected virtual void Dispose(bool disposing)
        {

        }

        public virtual void LoadData()
        {
            m_tblInfo.LoadData();
            //m_dataGridView.AutoGenerateColumns = false;
            //crtColumns();
            m_dataContent = appConfig.s_contentProvider.CreateDataContent(m_tblInfo.m_tblName);

            //init gridview
            m_dataGridView.DataSource = m_dataContent.m_bindingSource;
            DataTable tbl = (DataTable)m_dataContent.m_bindingSource.DataSource;
            if (tbl != null)
            {
                UpdateDGVCols(m_dataGridView,m_tblInfo);
                m_dataGridView.AutoGenerateColumns = false;
            }
            else
            {
                Debug.Assert(tbl != null, "table not loaded");
            }
            m_dataGridView.AllowUserToAddRows = false;

            m_keyCtrl.Text = InitKey();
            m_dataContent.UpdateTableCompleted += M_dataContent_UpdateTableCompleted;

            //load input ctrls data
            foreach (var ctrl in m_inputsCtrls)
            {
                ctrl.LoadData();
            }

#if use_bg_work
            m_wkr = myWorker.getWorker();
#endif
        }

        private void M_dataContent_UpdateTableCompleted(object sender, DataContent.FillTableCompletedEventArgs e)
        {
            //if add compeplete
            if (bIncKeyReq)
            {
                m_keyCtrl.Text = IncKey();
                bIncKeyReq = false;
            }
#if single_preview
            //if delete or add complete
            if (RefreshPreview != null)
            {
                RefreshPreview(this, new PreviewEventArgs { tbl = m_dataContent.m_dataTable });
            }
#endif
        }

        protected void crtColumns(DataGridView dgv, TableInfo tblInfo)
        {
            int i = 0;
            foreach (var field in tblInfo.m_cols)
            {
#if !use_custom_cols
                i = dgv.Columns.Add(field.m_field, field.m_alias);
                var dgvcol = dgv.Columns[i];
#else
                    DataGridViewColumn dgvcol;
                    if (field.m_type == lTableInfo.lColInfo.lColType.dateTime)
                    {
                        dgvcol = new CalendarColumn();
                        dgvcol.SortMode = DataGridViewColumnSortMode.Automatic;
                    }
                    else if (field.m_lookupTbl != null)
                    {
                        var cmb = new DataGridViewComboBoxColumn();
                        DataTable tbl = field.m_lookupData.m_dataSource;
                        BindingSource bs = new BindingSource();
                        bs.DataSource = tbl;
                        cmb.DataSource = bs;
                        cmb.DisplayMember = tbl.Columns[1].ColumnName;
                        cmb.AutoComplete = true;
                        cmb.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
                        cmb.FlatStyle = FlatStyle.Flat;
                        dgvcol = cmb;
                        dgvcol.SortMode = DataGridViewColumnSortMode.Automatic;
                    }
                    else
                    { 
                        dgvcol = new DataGridViewTextBoxColumn();
                    }
                    i = dgv.Columns.Add(dgvcol);
                    dgvcol.HeaderText = field.m_alias;
                    dgvcol.Name = field.m_field;
#endif //use_custom_cols
                dgvcol.DataPropertyName = field.m_field;
                switch (field.m_type)
                {
#if format_currency
                    case TableInfo.ColInfo.ColType.currency:
                        dgvcol.DefaultCellStyle.Format = lConfigMng.getCurrencyFormat();
                        break;
#endif
                    case TableInfo.ColInfo.ColType.dateTime:
                        dgvcol.DefaultCellStyle.Format = lConfigMng.getDisplayDateFormat();
                        break;
                }
            }
            dgv.Columns[0].Visible = false;
            //last columns
            var lastCol = dgv.Columns[i];
            lastCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            lastCol.FillWeight = 1;
#if set_column_order
            for (; i > 0; i--)
            {
                dgv.Columns[i].DisplayIndex = i - 1;
            }
#endif
        }

        protected void UpdateDGVCols(DataGridView dgv, TableInfo ti)
        {
            dgv.Columns[0].Visible = false;
            int i = 1;
            for (; i < dgv.ColumnCount; i++)
            {
                //show hide columns
                if (ti.m_cols[i].m_visible == false)
                {
                    dgv.Columns[i].Visible = false;
                    continue;
                }

                dgv.Columns[i].HeaderText = ti.m_cols[i].m_alias;

#if header_blue
                //header color blue
                dgv.Columns[i].HeaderCell.Style.BackColor = Color.Blue;
                dgv.Columns[i].HeaderCell.Style.ForeColor = Color.White;
#endif

                switch (ti.m_cols[i].m_type)
                {
                    case TableInfo.ColInfo.ColType.currency:
                        dgv.Columns[i].DefaultCellStyle.Format = lConfigMng.getCurrencyFormat();
                        break;
                    case TableInfo.ColInfo.ColType.dateTime:
                        dgv.Columns[i].DefaultCellStyle.Format = lConfigMng.getDisplayDateFormat();
                        break;
                }
#if false
                    dgv.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgv.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dgv.Columns[i].FillWeight = 1;
#endif
#if set_column_order
                dgv.Columns[i].DisplayIndex = i - 1;
#endif
            }
            dgv.Columns[1].ReadOnly = true;
            dgv.Columns[i - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv.Columns[i - 1].FillWeight = 1;
        }
        public InputCtrl crtInputCtrl(TableInfo tblInfo, string colName, Point pos, Size size)
        {
            return crtInputCtrl(tblInfo, colName, pos, size, SearchCtrl.SearchMode.match);
        }
        public InputCtrl crtInputCtrl(TableInfo tblInfo, string colName, Point pos, Size size, SearchCtrl.SearchMode mode)
        {
            int iCol = tblInfo.getColIndex(colName);
            if (iCol != -1)
            {
                return crtInputCtrl(tblInfo, iCol, pos, size, mode);
            }
            return null;
        }
        public InputCtrl crtInputCtrl(TableInfo tblInfo, int iCol, Point pos, Size size)
        {
            return crtInputCtrl(tblInfo, iCol, pos, size, SearchCtrl.SearchMode.match);
        }
        public InputCtrl crtInputCtrl(TableInfo tblInfo, int iCol, Point pos, Size size, SearchCtrl.SearchMode mode)
        {
            TableInfo.ColInfo col = tblInfo.m_cols[iCol];
            switch (col.m_type)
            {
                case TableInfo.ColInfo.ColType.text:
                case TableInfo.ColInfo.ColType.uniqueText:
                    lInputCtrlText textCtrl = new lInputCtrlText(col.m_field, col.m_alias, SearchCtrl.CtrlType.text, pos, size);
                    textCtrl.m_mode = mode;
                    textCtrl.m_colInfo = col;
                    return textCtrl;
                case TableInfo.ColInfo.ColType.dateTime:
                    lInputCtrlDate dateCtrl = new lInputCtrlDate(col.m_field, col.m_alias, SearchCtrl.CtrlType.dateTime, pos, size);
                    return dateCtrl;
                case TableInfo.ColInfo.ColType.num:
                    lInputCtrlNum numCtrl = new lInputCtrlNum(col.m_field, col.m_alias, SearchCtrl.CtrlType.num, pos, size);
                    return numCtrl;
                case TableInfo.ColInfo.ColType.currency:
                    lInputCtrlCurrency currencyCtrl = new lInputCtrlCurrency(col.m_field, col.m_alias, SearchCtrl.CtrlType.currency, pos, size);
                    return currencyCtrl;
                case TableInfo.ColInfo.ColType.map:
                    InputCtrlEnum enumCtrl = new InputCtrlEnum(col.m_field, col.m_alias, SearchCtrl.CtrlType.map, pos, size);
                    enumCtrl.Init(col.GetDict());
                    return enumCtrl;
            }
            return null;
        }
    }
    public class keyMng
    {
        string m_preFix;
        string m_tblName;
        string m_keyField;
        string m_dateField;
        Regex m_reg;
        DateTime m_preDate;
        int m_preNo;
        public keyMng(string preFix, string tblName, string keyField, string dateField = "date")
        {
            m_preFix = preFix;
            m_tblName = tblName;
            m_keyField = keyField;
            m_dateField = dateField;
            //@"(PT\d{8})(\d{3})"
            m_reg = new Regex(@"(\d{4})(\d{3})");
        }
        private string genKey(DateTime date, int n)
        {
            string zKey = m_preFix + date.ToString("yyyy") + n.ToString("D3");
            m_preDate = date;
            m_preNo = n;
            return zKey;
        }
        public string InitKey()
        {
            DateTime curDate = DateTime.Now.Date;
            //PTyyyy001
            string zKey = genKey(curDate, 1);
            string zDate = curDate.ToString(lConfigMng.getDateFormat());
#if use_sqlite
            string sql = string.Format("SELECT * FROM {0} ORDER BY ID DESC LIMIT 1", m_tblName);
#else
            string sql = string.Format("SELECT TOP 1 * FROM {0} ORDER BY ID DESC", m_tblName);
#endif
            DataTable tbl = appConfig.s_contentProvider.GetData(sql);
            if (tbl.Rows.Count > 0)
            {
                int no = 1;
                string curKey = tbl.Rows[0][m_keyField].ToString();
                Match m = m_reg.Match(curKey);
                int year = int.Parse(m.Groups[1].Value);
                if (year == curDate.Year)
                {
                    no = int.Parse(m.Groups[2].Value);
                    zKey = genKey(curDate, no + 1);
                }
                //else reset key by year
            }
            if (!IsUniqKey(zKey))
            {
                Debug.Assert(false, "gen key error - multi user inputting");
                zKey = IncKey();
            }
            return zKey;
        }
#if key_reset_daily
        public string InitKey()
        {
            DateTime curDate = DateTime.Now.Date;
            //PTyyyyMMdd001
            string zKey = genKey(curDate, 1);
            string zDate = curDate.ToString(lConfigMng.getDateFormat());
            string sql = string.Format("select {0} from {1} where {2} = '{3} 00:00:00' order by {0}",
                m_keyField, m_tblName, m_dateField, zDate);
            DataTable tbl = appConfig.s_contentProvider.GetData(sql);
            if (tbl.Rows.Count > 0)
            {
                int no = 1;
                int i = 1;
                for (; i <= tbl.Rows.Count; i++)
                {
                    string curKey = tbl.Rows[i - 1][0].ToString();
                    Match m = m_reg.Match(curKey);
                    no = int.Parse(m.Groups[2].Value);
                    if (i != no) break;
                }
                zKey = genKey(curDate, i);
            }
            if (!IsUniqKey(zKey))
            {
                Debug.Assert(false);    //invalid manual gen key
                zKey = IncKey();
            }
            return zKey;
        }
#endif //key_reset_daily
        public bool IsUniqKey(string val)
        {
            var bRet = true;
            string sql = string.Format("select id, {0} from {1} where {0} = '{2}'",
                m_keyField, m_tblName, val);
            var tbl = appConfig.s_contentProvider.GetData(sql);
            if (tbl.Rows.Count > 0)
            {
                Debug.WriteLine("{0} {1} not unique value {2}", this, "checkUniqKey() check unique", val);
                bRet = false;
            }
            return bRet;
        }
        public string IncKey()
        {
            //PTyyyyMMdd000
            string newKey = "";
            //string curKey = m_preKey;
            //Match m = m_reg.Match(curKey);
            //if (m.Success)
            {
                //int no = int.Parse(m.Groups[2].Value);
                int no = m_preNo + 1;
                for (; no < 999; no++)
                {
                    newKey = genKey(m_preDate, no);
                    if (IsUniqKey(newKey))
                    {
                        break;
                    }
                }
            }
            return newKey;
        }
    }

    [DataContract(Name = "ReceiptsInputPanel")]
    public class lReceiptsInputPanel : InputPanel
    {
        protected override InputCtrl m_keyCtrl { get { return m_inputsCtrls[0]; } }
        private keyMng m_key;
        protected override keyMng m_keyMng { get { return m_key; } }
        public lReceiptsInputPanel()
        {
            m_tblName = "receipts";

            m_inputsCtrls = new List<InputCtrl> {
                crtInputCtrl(m_tblInfo, "receipt_number", new Point(0, 0), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "date"          , new Point(0, 1), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "name"          , new Point(0, 2), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "addr"          , new Point(0, 3), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "content"       , new Point(0, 4), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "note"          , new Point(0, 5), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "amount"        , new Point(0, 6), new Size(1, 1)),
            };
            m_inputsCtrls[0].ReadOnly = true;
            m_key = new keyMng("PT", m_tblName, "receipt_number");

            Dictionary<string, string> dict = new Dictionary<string, string> {
                { "name","name" },
                { "addr","addr" },
                { "date","date" },
                { "num","receipt_number" },
                { "content","content" },
                { "note","note" },
                { "amount","amount" },
            };
            m_rptAsst = new rptAssist(1, dict);
        }
    }
    [DataContract(Name = "InterPayInputPanel")]
    public class lInterPayInputPanel : InputPanel
    {
        protected override InputCtrl m_keyCtrl { get { return m_inputsCtrls[0]; } }
        private keyMng m_key;
        protected override keyMng m_keyMng { get { return m_key; } }
        InputCtrl advance_payment;
        InputCtrlEnum status;
        InputCtrl actually_spent;
        InputCtrl note;
        public lInterPayInputPanel()
        {
            m_tblName = "internal_payment";

            advance_payment = crtInputCtrl(m_tblInfo, "advance_payment", new Point(0, 6), new Size(1, 1));
            actually_spent = crtInputCtrl(m_tblInfo, "actually_spent",   new Point(0, 7), new Size(1, 1));
            status = (InputCtrlEnum)crtInputCtrl(m_tblInfo, "status", new Point(0, 8), new Size(1, 1));
            note = crtInputCtrl(m_tblInfo, "note", new Point(0, 9), new Size(1, 1));
            //status.init(lAdvanceStatus.lst);

            m_inputsCtrls = new List<InputCtrl>
            {
                crtInputCtrl(m_tblInfo, "payment_number"    , new Point(0, 0), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "date"              , new Point(0, 1), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "name"              , new Point(0, 2), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "addr"              , new Point(0, 3), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "group_name"        , new Point(0, 4), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "content"           , new Point(0, 5), new Size(1, 1)),
#if true
                advance_payment,
                actually_spent,
                status,
                note,
#else
                crtInputCtrl(m_tblInfo, "actually_spent"    , new Point(0, 6), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "note"              , new Point(0, 7), new Size(1, 1)),
#endif
            };
            m_inputsCtrls[0].ReadOnly = true;
#if true
            advance_payment.EditingCompleted += advanceComp;
            actually_spent.EditingCompleted += spentComp;
            
#endif
            m_key = new keyMng("PCN", m_tblName, "payment_number");
            m_rptAsst = rptAssist.Create(m_tblName);
        }

        private void advanceComp(object sender, string val)
        {
            long advance;
            string txt = advance_payment.Text;
            bool bRet = long.TryParse(txt.Replace(",", ""), out advance);
            if (bRet && advance != 0)
            {
                actually_spent.Text = txt;
                status.Text = lAdvanceStatus.zAdvance;
                note.Text = lAdvanceStatus.zAdvance;
            }
        }
        private void spentComp(object sender, string val)
        {
            long actual;
            bool bRet = long.TryParse(actually_spent.Text.Replace(",", ""), out actual);
            if (bRet & actual != 0 && advance_payment.Text == "")
            {
                status.Text = lAdvanceStatus.zActual;
                note.Text = lAdvanceStatus.zActual;
            }
        }

        public override void initCtrls()
        {
            base.initCtrls();

            m_dataGridView.CellEndEdit += (s, e) =>
            {
                //if (e.ColumnIndex == )
                //{

                //}
            };
        }
    }
    [DataContract(Name = "ExterPayInputPanel")]
    public class lExterPayInputPanel : InputPanel
    {
        protected override InputCtrl m_keyCtrl { get { return m_inputsCtrls[0]; } }
        private keyMng m_key;
        protected override keyMng m_keyMng { get { return m_key; } }
        public lExterPayInputPanel()
        {
            m_tblName = "external_payment";
            m_inputsCtrls = new List<InputCtrl>
            {
                crtInputCtrl(m_tblInfo, "payment_number", new Point(0, 0), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "date"          , new Point(0, 1), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "name"          , new Point(0, 2), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "addr"          , new Point(0, 3), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "content"       , new Point(0, 4), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "constr_org"    , new Point(0, 5), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "building"      , new Point(0, 6), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "group_name"    , new Point(0, 7), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "spent"         , new Point(0, 8), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "note"          , new Point(0, 9), new Size(1, 1)),
            };
            m_inputsCtrls[0].ReadOnly = true;
            m_key = new keyMng("PCG", m_tblName, "payment_number");
            Dictionary<string, string> dict = new Dictionary<string, string> {
                { "name","name" },
                { "addr","addr" },
                { "date","date" },
                { "num","payment_number" },
                { "content","content" },
                { "note","note" },
                { "amount","spent" },
            };
            m_rptAsst = new rptAssist(2, dict);
        }
    }
    [DataContract(Name = "SalaryInputPanel")]
    public class lSalaryInputPanel : InputPanel
    {
        protected override InputCtrl m_keyCtrl { get { return m_inputsCtrls[0]; } }
        private keyMng m_key;
        protected override keyMng m_keyMng { get { return m_key; } }
        InputCtrl bsalary;
        InputCtrl esalary;
        InputCtrl salary;
        public lSalaryInputPanel()
        {
            m_tblName = "salary";
            bsalary = crtInputCtrl(m_tblInfo, "bsalary", new Point(0, 6), new Size(1, 1));
            esalary = crtInputCtrl(m_tblInfo, "esalary", new Point(0, 7), new Size(1, 1));
            salary = crtInputCtrl(m_tblInfo, "salary", new Point(0, 8), new Size(1, 1));
            m_inputsCtrls = new List<InputCtrl> {
                crtInputCtrl(m_tblInfo, "payment_number", new Point(0, 0), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "date"          , new Point(0, 1), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "name"          , new Point(0, 2), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "addr"          , new Point(0, 3), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "group_name"    , new Point(0, 4), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "content"       , new Point(0, 5), new Size(1, 1)),
                bsalary,
                esalary,
                salary,
                crtInputCtrl(m_tblInfo, "note"          , new Point(0, 9), new Size(1, 1)),
            };
            m_inputsCtrls[0].ReadOnly = true;
            bsalary.EditingCompleted += onEditSalaryCompleted;
            esalary.EditingCompleted += onEditSalaryCompleted;
            salary.ReadOnly = true;
            m_key = new keyMng("PCL", m_tblName, "payment_number");
            Dictionary<string, string> dict = new Dictionary<string, string> {
                { "name","name" },
                { "addr","addr" },
                { "date","date" },
                { "num","payment_number" },
                { "content","content" },
                { "note","note" },
                { "amount","salary" },
            };
            m_rptAsst = new rptAssist(2, dict);
        }

        private void onEditSalaryCompleted(object sender, string e)
        {
            Int64 val;
            Int64 sum = 0;
            if (Int64.TryParse(bsalary.Text.Replace(",",""), out val)) { sum += val; }
            if (Int64.TryParse(esalary.Text.Replace(",", ""), out val)) { sum += val; }
            salary.Text = string.Format("{0:#,0}", sum);
        }
    }
#if false
    [DataContract(Name = "AdvanceInputPanel")]
    public class lAdvanceInputPanel : lInputPanel
    {
        protected override lInputCtrl m_keyCtrl { get { return m_inputsCtrls[0]; } }
        private keyMng m_key;
        protected override keyMng m_keyMng { get { return m_key; } }
        public lAdvanceInputPanel()
        {
            m_tblName = "advance";
#if true
            lInputCtrl advance_payment = crtInputCtrl(m_tblInfo, "advance_payment", new Point(0, 6), new Size(1, 1));
            lInputCtrl reimbursement = crtInputCtrl(m_tblInfo, "reimbursement", new Point(0, 7), new Size(1, 1));
            lInputCtrl actually_spent = crtInputCtrl(m_tblInfo, "actually_spent", new Point(0, 8), new Size(1, 1));
#endif
            m_inputsCtrls = new List<lInputCtrl>
            {
                crtInputCtrl(m_tblInfo, "payment_number"    , new Point(0, 0), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "date"              , new Point(0, 1), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "name"              , new Point(0, 2), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "addr"              , new Point(0, 3), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "group_name"        , new Point(0, 4), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "content"           , new Point(0, 5), new Size(1, 1)),
#if true
                advance_payment,
                reimbursement,
                actually_spent,
                crtInputCtrl(m_tblInfo, "note"              , new Point(0, 9), new Size(1, 1)),
#else
                crtInputCtrl(m_tblInfo, "advance_payment"   , new Point(0, 6), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "reimbursement"     , new Point(0, 7), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "note"              , new Point(0, 8), new Size(1, 1)),
#endif
            };
            m_inputsCtrls[0].ReadOnly = true;
#if true
            reimbursement.EditingCompleted += (s, e) =>
            {
                long advance, reimbur;
                bool bRet = long.TryParse(advance_payment.Text.Replace(",", ""), out advance);
                bRet &= long.TryParse(reimbursement.Text.Replace(",", ""), out reimbur);
                if (bRet)
                {
                    Debug.Assert(advance >= reimbur);
                    actually_spent.Text = (advance - reimbur).ToString();
                }
            };
#endif
            m_key = new keyMng("PTU", m_tblName, "payment_number");
            Dictionary<string, string> dict = new Dictionary<string, string> {
                { "name","name" },
                { "addr","addr" },
                { "date","date" },
                { "num","payment_number" },
                { "content","content" },
                { "note","note" },
                { "amount","advance_payment" },
            };
            m_rptAsst = new rptAssist(2, dict);
            m_rptAsst.ConvertRowCompleted = (inR, outR) =>
            {
                Debug.Assert((Int64)inR["advance_payment"] > 0, "advance should not zero");
                var obj = inR["actually_spent"];
                if (obj != DBNull.Value)
                {
                    Int64 act = (Int64)obj;
                    if (act > 0)
                    {
                        outR["amount"] = act;
                    }
                }
            };
        }
        public override void initCtrls()
        {
            base.initCtrls();

            m_dataGridView.CellEndEdit += (s, e) =>
            {
            };
        }
    }
#endif
    [DataContract(Name = "TaskInputPanel")]
    public class lTaskInputPanel : InputPanel
    {
        protected override InputCtrl m_keyCtrl { get { return m_inputsCtrls[0]; } }
        private keyMng m_key;
        protected override keyMng m_keyMng { get { return m_key; } }
        public lTaskInputPanel()
        {
            m_tblName = "task";

            m_inputsCtrls = new List<InputCtrl> {
                crtInputCtrl(m_tblInfo, "task_number", new Point(0, 0), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "group_name" , new Point(0, 1), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "task_name"  , new Point(0, 2), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "start_date" , new Point(0, 3), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "end_date"   , new Point(0, 4), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "note"       , new Point(0, 5), new Size(1, 1)),
            };
            m_inputsCtrls[0].ReadOnly = true;
            m_key = new keyMng("CV", m_tblName, "task_number");
        }

        protected override void onDGV_CellClick()
        {
            base.onDGV_CellClick();
            DataGridViewSelectedRowCollection rows = m_dataGridView.SelectedRows;
            if (rows.Count > 0)
            {
                lOrderInputF inputDlg;
                inputDlg = new lOrderInputF();
                var cell = rows[0].Cells["task_number"];
                inputDlg.m_panel.m_taskNumber = (string)cell.Value;
                cell = rows[0].Cells["start_date"];
                inputDlg.m_panel.m_taskStartDate = (DateTime)cell.Value;
                cell = rows[0].Cells["end_date"];
                inputDlg.m_panel.m_taskEndDate = (DateTime)cell.Value;
                inputDlg.ShowDialog();
            }
        }
    }

    [DataContract(Name = "OrderInputPanel")]
    public class OrderInputPanel : InputPanel
    {
        protected override InputCtrl m_keyCtrl { get { return m_inputsCtrls[1]; } }
        private keyMng m_key;
        protected override keyMng m_keyMng { get { return m_key; } }

        //left panel
        Button removeBtn = lConfigMng.crtButton();
        //right panel
        public SplitContainer rightSC;

        public OrderInputPanel()
        {
            m_tblName = "order_tbl";

            m_inputsCtrls = new List<InputCtrl> {
                crtInputCtrl(m_tblInfo, "task_number" , new Point(0, 0), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "order_number", new Point(0, 1), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "order_type"  , new Point(0, 2), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "number"      , new Point(0, 3), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "order_status", new Point(0, 4), new Size(1, 1)),
                crtInputCtrl(m_tblInfo, "note"        , new Point(0, 5), new Size(1, 1)),
            };

            m_inputsCtrls[1].ReadOnly = true;
            m_inputsCtrls[4].ReadOnly = true;
            m_key = new keyMng("YC", m_tblName, "order_number");
            //oder type change ->update resource
            //m_inputsCtrls[2].EditingCompleted += LOrderInputPanel_EditingCompleted;
        }

        private InputCtrl taskCmb;
        public override void LoadData()
        {
            base.LoadData();    //init combo box & data

            m_humanRes.LoadData();
            m_equipmentRes.LoadData();
            m_carRes.LoadData();

            taskCmb = m_inputsCtrls[0];
            taskCmb.ReadOnly = true;
            taskCmb.EditingCompleted += taskCmb_EditingCompleted;
            if (m_taskNumber != null)
            {
                taskCmb.Text = m_taskNumber;
            }

            string taskNumber = taskCmb.Text;
            updateOrderDGV(taskCmb.Text);

        }
        
        SearchBuilder m_orderSB;
        private void taskCmb_EditingCompleted(object sender, string e)
        {
            if (sender == taskCmb) { 
                if ( e != "")
                {
                    string taskNumber = e;
                    if (m_orderSB == null) { m_orderSB = new SearchBuilder(appConfig.s_config.getTable("order_tbl")); }
                    m_orderSB.Clear();
                    m_orderSB.Add("task_number", taskNumber);
                    m_orderSB.Search();

                    //clear lbl, resLst, orderResLst
                    ClearRightPanel();

                    updateTaskInfo(taskNumber);
                }
            }
        }

        private void ClearRightPanel()
        {
            //resLbl.Text = "Chọn tài nguyên cho Yêu Cầu";
            //resDGV.DataSource = null;
            //orderResDGV.DataSource = null;
            
        }

        protected void updateOrderDGV(string taskNumber)
        {
            if (m_orderSB == null) { m_orderSB = new SearchBuilder(appConfig.s_config.getTable("order_tbl")); }
            m_orderSB.Clear();
            m_orderSB.Add("task_number", taskNumber);
            m_orderSB.Search();

            updateTaskInfo(taskNumber);
        }

        private void updateTaskInfo(string taskNumber)
        {
            m_taskNumber = taskNumber;
            //access task data - singleton
            DataContent dc = appConfig.s_contentProvider.CreateDataContent("task");
            var rows = dc.m_dataTable.Rows;
            for (int i = 0; i < rows.Count; i++)
            {
                string tskNum = rows[i]["task_number"].ToString();
                if (tskNum == taskNumber)
                {
                    m_taskStartDate = (DateTime)rows[i]["start_date"];
                    m_taskEndDate = (DateTime)rows[i]["end_date"];
                    break;
                }
            }
        }


        public override void initCtrls()
        {
            //spliter panel 1
            base.initCtrls();
            //flow      |add    |remove |save   |
            m_flow.Controls.Clear();
            m_flow.Controls.AddRange(new Control[] { m_addBtn, removeBtn, m_saveBtn });


#if DEBUG_DRAWING
                m_tbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
#endif
            //spliter panel 2
            //lable     | <res tbl name>        |
            //res DGV   | res data              |
            //flow      | up    |down   |save   |
            //order-res | order-res map record  |
            rightSC = new SplitContainer();
            rightSC.Dock = DockStyle.Fill;
            rightSC.Orientation = Orientation.Horizontal;
            int n = m_inputsCtrls.Count;
            //  res
            //rightSC.Panel1.Controls.Add(toprightTLP);

            //rightSC.Panel2.Controls.Add(botrightTLP);

            //enum: already process in InputPanel
            //order DGV: single select (ref delete order)
            m_dataGridView.MultiSelect = false;
            m_dataGridView.AllowUserToDeleteRows = false;

            //remove btn
            removeBtn.Text = "Remove";
            removeBtn.Click += RemoveBtn_Click;

            m_humanRes.InitCtrl();
            m_orderHuman.InitCtrl();
            m_humanRes.m_orderResPanel = m_orderHuman;
            m_orderHuman.m_resPanel = m_humanRes;

            m_equipmentRes.InitCtrl();
            m_orderEquipment.InitCtrl();
            m_equipmentRes.m_orderResPanel = m_orderEquipment;
            m_orderEquipment.m_resPanel = m_equipmentRes;

            m_carRes.InitCtrl();
            m_orderCar.InitCtrl();
            m_carRes.m_orderResPanel = m_orderCar;
            m_orderCar.m_resPanel = m_carRes;
        }

        private OrderResPanel curOrderResPanel;
        private ResPanel curResPanel;

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (m_dataGridView.SelectedRows.Count > 0)
            {
                //req: all order - res rec were removed
                if (curOrderResPanel.RowCount == 0)
                {
                    List<int> idxLst = new List<int>();
                    for (int i = 0; i<m_dataGridView.SelectedRows.Count;i++)
                    {
                        var row = m_dataGridView.SelectedRows[i];
                        idxLst.Add(row.Index);
                    }
                    idxLst.Sort();
                    for (int i = idxLst.Count -1; i >= 0; i--)
                    {
                        m_dataGridView.Rows.RemoveAt(idxLst[i]);
                    }
                    m_dataContent.Submit();

                    CleanRightPanel();
                }
                else
                {
                    lConfigMng.showInputError("Hãy xóa tất cả các liên kết của yêu cầu với tài nguyên");
                }
            }
            else
            {
                lConfigMng.showInputError("Không có dòng nào được chọn");
            }
        }

        private void CleanRightPanel()
        {
            //clearn resDGV, orderResDGV
            //orderResDGV.DataSource = null;
            //resDGV.DataSource = null;
        }

        public string m_taskNumber;
        public DateTime m_taskStartDate;
        public DateTime m_taskEndDate;
        private void getTaskInfo(ref DateTime startDate, ref DateTime endDate)
        {
            if (m_taskNumber == null)
            {
                startDate = DateTime.Now;
                endDate = DateTime.Now;
            }
            else
            {
                startDate = m_taskStartDate;
                endDate = m_taskEndDate;
            }
        }

        private string m_curOrder;
        private OrderType m_curOrderType;
        private string m_curResTbl;
        private string m_curOrderResTbl;
        protected override void onDGV_CellClick()
        {
            base.onDGV_CellClick();
            DataGridViewSelectedRowCollection rows = m_dataGridView.SelectedRows;
            if (rows.Count > 0)
            {
                //save cur order
                string orderId = rows[0].Cells["order_number"].Value.ToString();

                //get type
                //get date
                var cell = rows[0].Cells["order_type"];
                OrderType orderType = (OrderType)int.Parse(cell.Value.ToString());
                UpdateRightPanel(orderId, orderType);
            }
        }
        protected void UpdateRightPanel(string orderId, OrderType orderType)
        {
            m_curOrder = orderId;
            m_curOrderType = orderType;
            switch (orderType)
            {
                case OrderType.Worker: //human
                    {
                        m_curResTbl = "human";
                        m_curOrderResTbl = "order_human";

                        //update order-human
                        UpdateOrderResDGV();

                        //update human list
                        DateTime startDate = DateTime.Now; ;
                        DateTime endDate = DateTime.Now; ;
                        getTaskInfo(ref startDate, ref endDate);

                        //var tblInfo = appConfig.s_config.getTable("human");
                        //if (m_humanSB == null) { m_humanSB = new SearchBuilder(tblInfo); }
                        //m_humanSB.clear();
                        //m_humanSB.add("start_date", startDate, "<=");
                        //m_humanSB.add("end_date", endDate, ">=");
                        //m_humanSB.search();

                        //UpdateResDGV(tblInfo, m_humanSB.dc);
                        m_humanRes.UpdateResDGV(m_curOrder, startDate, endDate);
                    }
                    break;
                case OrderType.Equip: //equipment
                    {
                        m_curResTbl = "equipment";
                        m_curOrderResTbl = "order_equipment";

                        //update order-equipment
                        UpdateOrderResDGV();

                        //update res list
                        //var tblInfo = appConfig.s_config.getTable("equipment");
                        //if (m_equipSB == null) { m_equipSB = new SearchBuilder(tblInfo); }
                        //m_equipSB.clear();
                        //m_equipSB.search();
                        //UpdateResDGV(tblInfo, m_equipSB.dc);
                        //ShowEqipmentSearchPanel();
                        DateTime startDate = DateTime.Now; ;
                        DateTime endDate = DateTime.Now; ;
                        getTaskInfo(ref startDate, ref endDate);
                        m_equipmentRes.UpdateResDGV(m_curOrder, startDate, endDate);
                    }
                    break;
                case OrderType.Car: //car
                    {
                        m_curResTbl = "car";
                        m_curOrderResTbl = "order_car";

                        //update order-car
                        UpdateOrderResDGV();
                        
                        DateTime startDate = DateTime.Now; ;
                        DateTime endDate = DateTime.Now; ;
                        getTaskInfo(ref startDate, ref endDate);
                        m_equipmentRes.UpdateResDGV(m_curOrder, startDate, endDate);
                    }
                    break;
                case OrderType.Expense:
                    CleanRightPanel();
                    break;
            }
        }
        private void ShowEqipmentSearchPanel()
        {
            
        }

        private HumanResPanel m_humanRes = new HumanResPanel();
        private EquipmentResPanel m_equipmentRes = new EquipmentResPanel();
        private ResPanel m_carRes = new CarResPanel();
        private OrderHumanPanel m_orderHuman = new OrderHumanPanel();
        private OrderEquipmentPanel m_orderEquipment = new OrderEquipmentPanel();
        private OrderResPanel m_orderCar = new OrderCarPanel();
        protected void UpdateOrderResDGV()
        {
            switch (m_curResTbl)
            {
                case "human":
                    curOrderResPanel = m_orderHuman;
                    curResPanel = m_humanRes;
                    rightSC.Panel1.Controls.Clear();
                    rightSC.Panel1.Controls.Add(m_humanRes.toprightTLP);
                    rightSC.Panel2.Controls.Clear();
                    rightSC.Panel2.Controls.Add(m_orderHuman.botRightTLP);
                    curOrderResPanel.UpdateDGV(m_curOrder);
                    break;
                case "equipment":
                    curOrderResPanel = m_orderEquipment;
                    curResPanel = m_equipmentRes;
                    rightSC.Panel1.Controls.Clear();
                    rightSC.Panel1.Controls.Add(m_equipmentRes.toprightTLP);
                    rightSC.Panel2.Controls.Clear();
                    rightSC.Panel2.Controls.Add(m_orderEquipment.botRightTLP);
                    curOrderResPanel.UpdateDGV(m_curOrder);
                    break;
                case "car":
                    curOrderResPanel = m_orderCar;
                    curResPanel = m_carRes;
                    rightSC.Panel1.Controls.Clear();
                    rightSC.Panel1.Controls.Add(curResPanel.toprightTLP);
                    rightSC.Panel2.Controls.Clear();
                    rightSC.Panel2.Controls.Add(curOrderResPanel.botRightTLP);
                    curOrderResPanel.UpdateDGV(m_curOrder);
                    break;
            }
        }
    }
    [DataContract(Name = "ApproveInputPanel")]
    public class ApproveInputPanel: OrderInputPanel
    {
        DataGridView taskDGV;
        DataGridView orderDGV;
        public SplitContainer leftSC;
        public ApproveInputPanel()
        {
            m_tblName = "order_tbl";

            //create public ctrl
            leftSC = new SplitContainer();
        }
        public override void initCtrls()
        {
            base.initCtrls();
            leftSC.Orientation = Orientation.Horizontal;
            leftSC.Dock = DockStyle.Fill;
            //re-arrange left panel
            //  +-------------------------+
            //  |task grid view           |
            //  +-------------------------+
            //      spliter
            //  +-------------------------+
            //  |    save btn|            |
            //  +-------------------------+
            //  |    grid view            |
            //  +-------------------------+
            orderDGV = m_dataGridView;
            orderDGV.Dock = DockStyle.Fill;
            orderDGV.AllowUserToAddRows = false;
            orderDGV.AllowUserToDeleteRows = false;
            taskDGV = lConfigMng.crtDGV();
            taskDGV.Dock = DockStyle.Fill;
            taskDGV.AllowUserToAddRows = false;
            taskDGV.AllowUserToDeleteRows = false;

            TableLayoutPanel topLeftTLP = new TableLayoutPanel();
            Label topLeftLbl = lConfigMng.crtLabel();
            topLeftLbl.Text = "Dang sách các Công Việc:";   //<search cnd>
            topLeftLbl.AutoSize = true;
            topLeftTLP.Controls.Add(topLeftLbl, 0, 0);
            topLeftTLP.Controls.Add(taskDGV, 0, 1);
            topLeftTLP.Dock = DockStyle.Fill;
            leftSC.Panel1.Controls.Add(topLeftTLP);

            TableLayoutPanel orderTbl = new TableLayoutPanel();
            orderTbl.Dock = DockStyle.Fill;
            //flow  |    save btn  |
            FlowLayoutPanel tFlow = new FlowLayoutPanel();
            tFlow.AutoSize = true;
            tFlow.Controls.AddRange(new Control[] { m_saveBtn });
            orderTbl.Controls.Add(tFlow, 0, 0);
            // add data grid view
            orderTbl.Controls.Add(orderDGV, 0, 1);

            leftSC.Panel2.Controls.Add(orderTbl);

            //event
            taskDGV.CellClick += TaskDGV_CellClick;
        }

        private void TaskDGV_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewSelectedRowCollection rows = taskDGV.SelectedRows;
            if (rows.Count > 0)
            {
                string taskId = (string)rows[0].Cells["task_number"].Value;
                if (taskId == null) return;
                //get task info

                updateOrderDGV(taskId);
            }
        }

        public override void LoadData()
        {
            //base.LoadData();
            //task
            TableInfo taskTI = appConfig.s_config.getTable("task");
            taskTI.LoadData();
            DataContent taskDC = appConfig.s_contentProvider.CreateDataContent(taskTI.m_tblName);
            taskDGV.DataSource = taskDC.m_bindingSource;
            //order
            m_tblInfo.LoadData();
            m_dataContent = appConfig.s_contentProvider.CreateDataContent(m_tblInfo.m_tblName);
            m_dataGridView.DataSource = m_dataContent.m_bindingSource;
            //search order of first task
            //UpdateDGVCols(taskDGV, taskTI);
            //UpdateDGVCols(orderDGV, m_tblInfo);
        }
    }
}

