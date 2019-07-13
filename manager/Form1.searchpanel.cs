//#define DEBUG_DRAWING
#define use_cmd_params
#define use_sqlite
//#define fit_txt_size
//#define use_bg_work

using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System;
using System.Data;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Linq;

namespace test_binding
{
    [DataContract(Name = "SearchCtrl")]
    public class SearchCtrl : IDisposable
    {
        public enum CtrlType
        {
            text,
            dateTime,
            num,
            currency,
            map
        };
        public TableInfo.ColInfo m_colInfo;
        [DataMember(Name = "field", EmitDefaultValue = false)]
        public string m_fieldName;
        public string m_alias;
        public CtrlType m_type;
        [DataMember(Name = "pos", EmitDefaultValue = false)]
        public Point m_pos;
        [DataMember(Name = "size", EmitDefaultValue = false)]
        public Size m_size;

        [DataContract(Name = "SeachMode")]
        public enum SearchMode
        {
            [EnumMember]
            like,
            [EnumMember]
            match
        };

        [DataMember(Name = "mode", EmitDefaultValue = false)]
        public SearchMode m_mode = SearchMode.like;

        public FlowLayoutPanel m_panel = new FlowLayoutPanel();
        public CheckBox m_label = lConfigMng.crtCheckBox();

        public SearchCtrl() { }
        public SearchCtrl(string fieldName, string alias, CtrlType type, Point pos, Size size)
        {
            m_fieldName = fieldName;
            m_alias = alias;
            m_type = type;
            m_pos = pos;
            m_size = size;

            m_label.Text = alias;
#if fit_txt_size
            m_label.AutoSize = true;
#else
            m_label.Width = 100;
#endif
            m_label.TextAlign = ContentAlignment.MiddleLeft;
            m_panel.AutoSize = true;
#if true
            m_panel.BorderStyle = BorderStyle.FixedSingle;
#endif
        }

        public virtual void UpdateInsertParams(List<string> exprs, List<SearchParam> srchParams) { }
        public virtual void UpdateSearchParams(List<string> exprs, List<SearchParam> srchParams) { }
        public virtual string GetSearchParams() { return null; }
        public virtual void LoadData() { }
        protected virtual void ValueChanged(object sender, EventArgs e)
        {
            m_label.Checked = true;
        }

        #region dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~SearchCtrl()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_panel.Dispose();
                m_label.Dispose();
            }
        }
        #endregion
    };

    public class SearchParam
    {
        public string key;
        public string val;
        public DbType type;
    }

    public class SearchBuilder
    {
        TableInfo m_tblInfo;
        Dictionary<string, TableInfo.ColInfo> m_dict;
        public List<string> exprs = new List<string>();
        public List<SearchParam> srchParams = new List<SearchParam>();
        public DataContent dc;
        public SearchBuilder(TableInfo tblInfo, lContentProvider contentProvider = null)
        {
            m_tblInfo = tblInfo;
            m_dict = new Dictionary<string, TableInfo.ColInfo>();
            foreach (TableInfo.ColInfo colInfo in m_tblInfo.m_cols)
            {
                m_dict.Add(colInfo.m_field, colInfo);
            }
            if (contentProvider != null)
                dc = contentProvider.CreateDataContent(m_tblInfo.m_tblName);
            else
                dc = appConfig.s_contentProvider.CreateDataContent(m_tblInfo.m_tblName);
        }
        public void Clear()
        {
            exprs.Clear();
            srchParams.Clear();
        }

        public void Add(string col, DateTime start, string oper="=")
        {
            Debug.Assert(m_dict.ContainsKey(col));

            TableInfo.ColInfo colInfo = m_dict[col];
            Debug.Assert(colInfo.m_type == TableInfo.ColInfo.ColType.dateTime);

            exprs.Add(string.Format("({0}{1}@startDate)",colInfo.m_field, oper));
            string zStartDate = start.ToString(lConfigMng.GetDateFormat());
            srchParams.Add(
                    new SearchParam()
                    {
                        key = "@startDate",
                        val = string.Format("{0} 00:00:00", zStartDate),
                        type = DbType.Date
                    }
                );
        }
        public void Add(string col, DateTime startDate, DateTime endDate)
        {
            Debug.Assert (m_dict.ContainsKey(col));

            TableInfo.ColInfo colInfo = m_dict[col];
            Debug.Assert(colInfo.m_type == TableInfo.ColInfo.ColType.dateTime);

            exprs.Add(string.Format("({0} between @startDate and @endDate)",colInfo.m_field));
            string zStartDate = startDate.ToString(lConfigMng.GetDateFormat());
            string zEndDate = endDate.ToString(lConfigMng.GetDateFormat());
            srchParams.Add(
                new SearchParam()
                {
                    key = "@startDate",
                    val = string.Format("{0} 00:00:00", zStartDate),
                    type = DbType.Date
                }
            );
            srchParams.Add(
                new SearchParam()
                {
                    key = "@endDate",
                    val = string.Format("{0} 00:00:00", zEndDate),
                    type = DbType.Date
                }
            );
        }
        public void Add(string col, int arg1)
        {
            Add(col, arg1.ToString());
        }
        public void Add(string col, string arg1, SearchCtrl.SearchMode mode = SearchCtrl.SearchMode.match)
        {
            Debug.Assert(m_dict.ContainsKey(col));

            TableInfo.ColInfo colInfo = m_dict[col];
            switch (colInfo.m_type)
            {
                case TableInfo.ColInfo.ColType.text:
                case TableInfo.ColInfo.ColType.num:
                case TableInfo.ColInfo.ColType.uniq:
                case TableInfo.ColInfo.ColType.map:
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

#if use_sqlite
            if (mode == SearchCtrl.SearchMode.like)
            {
                exprs.Add(string.Format("({0} like @{0})", colInfo.m_field));
                srchParams.Add(
                    new SearchParam()
                    {
                        key = string.Format("@{0}", colInfo.m_field),
                        val = string.Format("%{0}%", arg1)
                    }
                );
            }
            else
            {
                exprs.Add(string.Format("({0}=@{0})", colInfo.m_field));
                srchParams.Add(
                    new SearchParam()
                    {
                        key = string.Format("@{0}", colInfo.m_field),
                        val = arg1
                    }
                );
            }
#else   //use sql server
                    exprs.Add(string.Format("({0} like @{0})", m_fieldName));
                    srchParams.Add(
                        new lSearchParam()
                        {
                            key = string.Format("@{0}", m_fieldName),
                            val = string.Format("%{0}%", m_value),
                            type = DbType.String
                        }
                    );
                    //exprs.Add(string.Format("({0} like @{0})", m_fieldName));
                    //srchParams.Add(string.Format("@{0}", m_fieldName), string.Format("%{0}%", m_value));
#endif
        }
        public void Add(string col, List<string> args)
        {
            Debug.Assert(m_dict.ContainsKey(col));

            TableInfo.ColInfo colInfo = m_dict[col];
            switch (colInfo.m_type)
            {
                case TableInfo.ColInfo.ColType.text:
                case TableInfo.ColInfo.ColType.num:
                case TableInfo.ColInfo.ColType.uniq:
                case TableInfo.ColInfo.ColType.map:
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

#if use_sqlite
            var keys = new List<string>();
            for (int i = 0; i < args.Count; i++)
            {
                var key = string.Format("@{0}_{1}", colInfo.m_field, i);
                srchParams.Add(
                    new SearchParam()
                    {
                        key = key,
                        val = string.Format("{0}", args[i])
                    }
                );
                keys.Add(key);
            }
            exprs.Add(string.Format("( {0} in ({1}) )", colInfo.m_field, string.Join(",",keys)));
#else   //use sql server
                    exprs.Add(string.Format("({0} like @{0})", m_fieldName));
                    srchParams.Add(
                        new lSearchParam()
                        {
                            key = string.Format("@{0}", m_fieldName),
                            val = string.Format("%{0}%", m_value),
                            type = DbType.String
                        }
                    );
                    //exprs.Add(string.Format("({0} like @{0})", m_fieldName));
                    //srchParams.Add(string.Format("@{0}", m_fieldName), string.Format("%{0}%", m_value));
#endif
        }
        public void Search()
        {
            dc.Search(exprs, srchParams);
        }
    }
    public class UpdateBuilder
    {
        TableInfo m_tblInfo;
        Dictionary<string, TableInfo.ColInfo> m_dict;
        public List<string> setExprs = new List<string>();
        public List<string> whereExprs = new List<string>();
        public List<SearchParam> srchParams = new List<SearchParam>();
        public DataContent dc;
        public UpdateBuilder(TableInfo tblInfo)
        {
            m_tblInfo = tblInfo;
            m_dict = new Dictionary<string, TableInfo.ColInfo>();
            foreach (TableInfo.ColInfo colInfo in m_tblInfo.m_cols)
            {
                m_dict.Add(colInfo.m_field, colInfo);
            }
            dc = appConfig.s_contentProvider.CreateDataContent(m_tblInfo.m_tblName);
        }
        public void Clear()
        {
            whereExprs.Clear();
            setExprs.Clear();
            srchParams.Clear();
        }

        public void Add(string col, DateTime date)
        {
            Debug.Assert(m_dict.ContainsKey(col));

            TableInfo.ColInfo colInfo = m_dict[col];
            Debug.Assert(colInfo.m_type == TableInfo.ColInfo.ColType.dateTime);

            setExprs.Add(string.Format("{0}=@startDate", colInfo.m_field));
            string zStartDate = date.ToString(lConfigMng.GetDateFormat());
            srchParams.Add(
                    new SearchParam()
                    {
                        key = "@startDate",
                        val = string.Format("{0} 00:00:00", zStartDate),
                        type = DbType.Date
                    }
                );
        }
        public void Add(string col, int arg1)
        {
            Add(col, arg1.ToString());
        }
        public void Add(string col, string arg1, bool isWhere = false)
        {
            Debug.Assert(m_dict.ContainsKey(col));

            TableInfo.ColInfo colInfo = m_dict[col];
            switch (colInfo.m_type)
            {
                case TableInfo.ColInfo.ColType.text:
                case TableInfo.ColInfo.ColType.num:
                case TableInfo.ColInfo.ColType.uniq:
                case TableInfo.ColInfo.ColType.map:
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

#if use_sqlite
            {
                List<string> tExpr = isWhere ?whereExprs: setExprs;
                tExpr.Add(string.Format("{0}=@{0}", colInfo.m_field));
                srchParams.Add(
                    new SearchParam()
                    {
                        key = string.Format("@{0}", colInfo.m_field),
                        val = arg1
                    }
                );
            }
#else   //use sql server
                    exprs.Add(string.Format("({0} like @{0})", m_fieldName));
                    srchParams.Add(
                        new lSearchParam()
                        {
                            key = string.Format("@{0}", m_fieldName),
                            val = string.Format("%{0}%", m_value),
                            type = DbType.String
                        }
                    );
                    //exprs.Add(string.Format("({0} like @{0})", m_fieldName));
                    //srchParams.Add(string.Format("@{0}", m_fieldName), string.Format("%{0}%", m_value));
#endif
        }
        public int Update()
        {
            return dc.Update(setExprs, whereExprs, srchParams);
        }

    }
    [DataContract(Name = "SearchCtrlText")]
    public class SearchCtrlText : SearchCtrl
    {
        protected TextBox m_text;
        protected ComboBox m_combo;
        protected virtual string m_value
        {
            get
            {
                if (m_text != null) return m_text.Text;
                else return m_combo.Text;
            }
        }
        public SearchCtrlText(string fieldName, string alias, CtrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
            m_text = lConfigMng.crtTextBox();
            m_text.Width = 200;
            m_text.TextChanged += ValueChanged;
            m_panel.Controls.AddRange(new Control[] { m_label, m_text });
        }

        public override string GetSearchParams()
        {
            string srchParam = null;
            if (m_label.Checked)
            {
                if (m_mode == SearchMode.like)
                    srchParam = string.Format("({0} like '%{1}%')", m_fieldName, m_value);
                else
                {
#if use_sqlite
                    srchParam = string.Format("({0} = '{1}')", m_fieldName, m_value);
#else
                    srchParam = string.Format("({0} like N'{1}')", m_fieldName, m_value);
#endif
                }
            }
            return srchParam;
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
        public override void UpdateSearchParams(List<string> exprs, List<SearchParam> srchParams)
        {
            if (m_label.Checked)
            {
#if use_sqlite
                if (m_mode == SearchMode.like)
                {
                    exprs.Add(string.Format("({0} like @{0})", m_fieldName));
                    srchParams.Add(
                        new SearchParam()
                        {
                            key = string.Format("@{0}", m_fieldName),
                            val = string.Format("%{0}%", m_value)
                        }
                    );
                }
                else
                {
                    exprs.Add(string.Format("({0}=@{0})", m_fieldName));
                    srchParams.Add(
                        new SearchParam()
                        {
                            key = string.Format("@{0}", m_fieldName),
                            val = m_value
                        }
                    );
                }
#else   //use sql server
                    exprs.Add(string.Format("({0} like @{0})", m_fieldName));
                    srchParams.Add(
                        new lSearchParam()
                        {
                            key = string.Format("@{0}", m_fieldName),
                            val = string.Format("%{0}%", m_value),
                            type = DbType.String
                        }
                    );
                    //exprs.Add(string.Format("({0} like @{0})", m_fieldName));
                    //srchParams.Add(string.Format("@{0}", m_fieldName), string.Format("%{0}%", m_value));
#endif
            }
        }
        public override void LoadData()
        {
            if (m_colInfo != null && m_colInfo.m_lookupData != null)
            {
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

                m_combo.Click += ValueChanged;
                m_combo.Validated += M_combo_Validated;

                m_text.Dispose();
                m_text = null;
            }
        }

        private void M_combo_Validated(object sender, EventArgs e)
        {
            string key = m_combo.Text;
            string val = m_colInfo.m_lookupData.find(key);
            if (val != null)
                m_combo.Text = val;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_text != null) m_text.Dispose();
                if (m_combo != null) m_combo.Dispose();
            }
            base.Dispose(disposing);
        }
    }
    [DataContract(Name = "SearchCtrlEnum")]
    public class SearchCtrlEnum: SearchCtrlText
    {
        protected override string m_value{
            get {
                return m_combo.SelectedValue.ToString();
            } }
        public SearchCtrlEnum(string fieldName, string alias, CtrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
            m_mode = SearchMode.match;
            m_combo = lConfigMng.crtComboBox();
            m_combo.Width = 200;
            m_combo.Click += ValueChanged;
            m_panel.Controls.Clear();
            m_panel.Controls.AddRange(new Control[] { m_label, m_combo });
        }
        public override void LoadData()
        {
            if (m_colInfo != null)
            {
                Dictionary<string, int> dict = m_colInfo.GetDict();
                var dt = new DataTable();
                dt.Columns.Add("name");
                dt.Columns.Add("val");
                for (int i = 0; i < dict.Count; i++)
                {
                    var newRow = dt.NewRow();
                    newRow[0] = dict.Keys.ElementAt(i);
                    newRow[1] = i;
                    dt.Rows.Add(newRow);
                }
                m_combo.DataSource = dt;
                m_combo.DisplayMember = "name";
                m_combo.ValueMember = "val";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_combo != null) m_combo.Dispose();
            }
            base.Dispose(disposing);
        }
    }
    [DataContract(Name = "SearchCtrlDate")]
    public class lSearchCtrlDate : SearchCtrl
    {
        private DateTimePicker m_startdate = new DateTimePicker();
        private DateTimePicker m_enddate = new DateTimePicker();
        private CheckBox m_to = new CheckBox();
        public lSearchCtrlDate(string fieldName, string alias, CtrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
#if fit_txt_size
            int w = lConfigMng.getWidth(lConfigMng.getDateFormat()) + 20;
#else
            int w = 100;
#endif
            m_to.Text = "to";
            m_to.AutoSize = true;
            m_startdate.Width = w;
            m_startdate.Format = DateTimePickerFormat.Custom;
            m_startdate.CustomFormat = lConfigMng.GetDisplayDateFormat();
            m_enddate.Width = w;
            m_enddate.Format = DateTimePickerFormat.Custom;
            m_enddate.CustomFormat = lConfigMng.GetDisplayDateFormat();
            FlowLayoutPanel datePanel = new FlowLayoutPanel();
            datePanel.BorderStyle = BorderStyle.FixedSingle;
            datePanel.Dock = DockStyle.Top;
            datePanel.AutoSize = true;
            datePanel.Controls.AddRange(new Control[] { m_startdate, m_to, m_enddate });

            m_panel.Controls.AddRange(new Control[] { m_label, datePanel });

            m_startdate.TextChanged += ValueChanged;
            m_enddate.TextChanged += M_enddate_TextChanged;

            //set font
            m_startdate.Font = lConfigMng.getFont();
            m_enddate.Font = lConfigMng.getFont();
            m_to.Font = lConfigMng.getFont();
        }

        private void M_enddate_TextChanged(object sender, EventArgs e)
        {
            m_to.Checked = true;
        }

        public override string GetSearchParams()
        {
            string srchParams = null;
            if (m_label.Checked)
            {
                string zStartDate = m_startdate.Value.ToString(lConfigMng.GetDateFormat());
                string zEndDate = m_enddate.Value.ToString(lConfigMng.GetDateFormat());
                if (m_to.Checked)
                    srchParams = string.Format("({0} between '{1}  00:00:00' and '{2} 00:00:00')", m_fieldName, zStartDate, zEndDate);
                else
                    srchParams = string.Format("({0}='{1} 00:00:00')", m_fieldName, zStartDate);
            }
            return srchParams;
        }
        public override void UpdateInsertParams(List<string> exprs, List<SearchParam> srchParams)
        {
            string zStartDate = m_startdate.Value.ToString(lConfigMng.GetDateFormat());
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
        public override void UpdateSearchParams(List<string> exprs, List<SearchParam> srchParams)
        {
            if (m_label.Checked)
            {
                string zStartDate = m_startdate.Value.ToString(lConfigMng.GetDateFormat());
                srchParams.Add(
                    new SearchParam()
                    {
                        key = "@startDate",
                        val = string.Format("{0} 00:00:00", zStartDate),
                        type = DbType.Date
                    }
                );
                if (m_to.Checked)
                {
                    exprs.Add(string.Format("({0} between @startDate and @endDate)", m_fieldName));
                    string zEndDate = m_enddate.Value.ToString(lConfigMng.GetDateFormat());
                    srchParams.Add(
                        new SearchParam()
                        {
                            key = "@endDate",
                            val = string.Format("{0} 00:00:00", zEndDate),
                            type = DbType.Date
                        }
                    );
                }
                else
                {
                    exprs.Add(string.Format("({0}=@startDate)",m_fieldName));
                }
            }
        }
    }
    [DataContract(Name = "SearchCtrlNum")]
    public class lSearchCtrlNum : SearchCtrlText
    {
        public lSearchCtrlNum(string fieldName, string alias, CtrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
            m_mode = SearchMode.match;
        }
    }
    [DataContract(Name = "SearchCtrlCurrency")]
    public class lSearchCtrlCurrency : SearchCtrl
    {
        private TextBox m_endVal = lConfigMng.crtTextBox();
        private TextBox m_startVal = lConfigMng.crtTextBox();
        private CheckBox m_to = lConfigMng.crtCheckBox();

        public lSearchCtrlCurrency(string fieldName, string alias, CtrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
            m_to.Text = "to";
            m_to.AutoSize = true;
#if fit_txt_size
            int w = lConfigMng.getWidth("000,000,000,000");
#else
            int w = 100;
#endif
            m_startVal.Width = w;
            m_endVal.Width = w;

            FlowLayoutPanel datePanel = new FlowLayoutPanel();
            datePanel.BorderStyle = BorderStyle.FixedSingle;
            datePanel.Dock = DockStyle.Top;
            datePanel.AutoSize = true;
            datePanel.Controls.AddRange(new Control[] { m_startVal, m_to, m_endVal });

            m_panel.Controls.AddRange(new Control[] { m_label, datePanel });

            m_startVal.TextChanged += ValueChanged;
            m_endVal.TextChanged += M_endVal_TextChanged;
        }

        private void M_endVal_TextChanged(object sender, EventArgs e)
        {
            m_to.Checked = true;
        }

        void getInputRange(out string startVal, out string endVal)
        {
            startVal = m_startVal.Text.Replace(",", "");
            endVal = m_endVal.Text.Replace(",", "");
            if (startVal == "") startVal = "0";
            if (endVal == "") endVal = UInt64.MaxValue.ToString();
        }
        public override string GetSearchParams()
        {
            string startVal;
            string endVal;
            string srchParams = null;

            getInputRange(out startVal, out endVal);
            if (m_label.Checked)
            {
                if (m_to.Checked)
                {
                    srchParams = string.Format("({0} between '{1}' and '{2}')",
                        m_fieldName, startVal, endVal);
                }
                else
                {
                    srchParams = string.Format("({0}='{1}')",
                        m_fieldName, startVal);
                }
            }
            return srchParams;
        }

        public override void UpdateInsertParams(List<string> exprs, List<SearchParam> srchParams)
        {
            string startVal;
            string endVal;

            getInputRange(out startVal, out endVal);
            if (m_label.Checked)
            {
                srchParams.Add(
                    new SearchParam()
                    {
                        key = "@" + m_fieldName,
                        val = string.Format("{0}", startVal),
                        type = DbType.UInt64
                    }
                );
                exprs.Add(m_fieldName);
            }
        }
        public override void UpdateSearchParams(List<string> exprs, List<SearchParam> srchParams)
        {
            string startVal;
            string endVal;

            getInputRange(out startVal, out endVal);
            if (m_label.Checked)
            {
                srchParams.Add(
                    new SearchParam()
                    {
                        key = "@startVal",
                        val = string.Format("{0}", startVal),
                        type = DbType.UInt64
                    }
                );
                if (m_to.Checked)
                {
                    exprs.Add(string.Format("({0} between @startVal and @endVal)", m_fieldName));
                    srchParams.Add(
                        new SearchParam()
                        {
                            key = "@endVal",
                            val = string.Format("{0}", endVal),
                            type = DbType.UInt64
                        }
                    );
                }
                else
                {
                    exprs.Add(string.Format("({0}=@startVal)", m_fieldName));
                }
            }
        }
    }

    /// <summary>
    /// search panel
    /// + search ctrl
    /// + search btn
    /// + getWhereQry
    /// </summary>
    [DataContract(Name = "SearchPanel")]
    public class SearchPanel : IDisposable
    {
        public lDataPanel m_dataPanel;
        public TableInfo m_tblInfo { get { return m_dataPanel.m_tblInfo; } }

        public TableLayoutPanel m_tblPanel;
        public Button m_searchBtn;

        [DataMember(Name = "searchCtrls")]
        public List<SearchCtrl> m_searchCtrls;

#if use_bg_work
        myWorker m_wkr;
#endif

        protected SearchPanel() { }

        public static SearchPanel crtSearchPanel(lDataPanel dataPanel, List<SearchCtrl> searchCtrls)
        {
            SearchPanel newPanel = new SearchPanel();
            newPanel.init(dataPanel, searchCtrls);
            return newPanel;
        }

        protected void init(lDataPanel dataPanel, List<SearchCtrl> searchCtrls)
        {
            m_dataPanel = dataPanel;
            m_searchCtrls = searchCtrls;
        }

        public virtual void initCtrls()
        {
            //crt search btn
            m_searchBtn = lConfigMng.crtButton();
            m_searchBtn.Text = "Search";
            m_searchBtn.Click += new System.EventHandler(searchButton_Click);

            //create search ctrls
            List<SearchCtrl> searchCtrls = m_searchCtrls;
            m_searchCtrls = new List<SearchCtrl>();
            foreach (SearchCtrl ctrl in searchCtrls)
            {
                m_searchCtrls.Add(
                    CrtSearchCtrl(
                        m_tblInfo,
                        ctrl.m_fieldName,
                        ctrl.m_pos,
                        ctrl.m_size,
                        ctrl.m_mode
                        )
                    );
            }

            //create table layout & add ctrls to
            //  +-------------------------+
            //  |search ctrl|             |
            //  +-------------------------+
            //  |search ctrl|             |
            //  +-------------------------+
            //  |       search btn        |
            //  +-------------------------+
            m_tblPanel = new TableLayoutPanel();
            m_tblPanel.AutoSize = true;
#if DEBUG_DRAWING
                m_tbl.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
#endif

            //add search ctrls to table layout
            int lastRow = 0;
            foreach (SearchCtrl searchCtrl in m_searchCtrls)
            {
                m_tblPanel.Controls.Add(searchCtrl.m_panel, searchCtrl.m_pos.X, searchCtrl.m_pos.Y);
                m_tblPanel.SetColumnSpan(searchCtrl.m_panel, searchCtrl.m_size.Width);
                m_tblPanel.SetRowSpan(searchCtrl.m_panel, searchCtrl.m_size.Height);
                lastRow = Math.Max(lastRow, searchCtrl.m_pos.Y);
            }

            //  add search button to last row
            m_tblPanel.Controls.Add(m_searchBtn, 1, lastRow + 1);
            m_searchBtn.Anchor = AnchorStyles.Right;
        }

        private void searchButton_Click(object sender, System.EventArgs e)
        {
#if use_cmd_params
            List<string> exprs = new List<string>();
            List<SearchParam> srchParams = new List<SearchParam>();
            foreach (SearchCtrl searchCtrl in m_searchCtrls)
            {
                searchCtrl.UpdateSearchParams(exprs, srchParams);
            }
#if use_bg_work
            //send to form 1
            m_wkr.qryFgTask(new FgTask {
                sender = "SP," + m_tblInfo.m_tblName,
                receiver = "F1," + m_tblInfo.m_tblName,
                eType = FgTask.fgTaskType.F1_FG_UPDATESTS,
                data = "Searching" });
            //sent to special data panel
            m_wkr.qryFgTask(new srchTsk(exprs, srchParams) {
                sender = "SP," + m_tblInfo.m_tblName,
                receiver = "DP," + m_tblInfo.m_tblName });
            //send to form 1
            m_wkr.qryFgTask(new FgTask {
                sender = "SP," + m_tblInfo.m_tblName ,
                receiver = "F1," + m_tblInfo.m_tblName,
                eType = FgTask.fgTaskType.F1_FG_UPDATESTS,
                data = "Searching completed!" }, true);
#else
            m_dataPanel.search(exprs, srchParams);
#endif //end use_bg_work
#else   //!use_cmd_params
                string where = null;
                List<string> exprs = new List<string> ();
                foreach (lSearchCtrl searchCtrl in m_searchCtrls)
                {
                    string expr = searchCtrl.getSearchParams();
                    if (expr != null)
                        exprs.Add(expr);
                }
                if (exprs.Count > 0)
                {
                    where = string.Join(" and ", exprs);
                }
                m_dataPanel.search(where);
#endif  //end use_cmd_params
        }

        public virtual void LoadData()
        {
            foreach (SearchCtrl ctrl in m_searchCtrls)
            {
                ctrl.LoadData();
            }
#if use_bg_work
            m_wkr = myWorker.getWorker();
#endif
        }

        public SearchCtrl CrtSearchCtrl(TableInfo tblInfo, string colName, Point pos, Size size)
        {
            return CrtSearchCtrl(tblInfo, colName, pos, size, SearchCtrl.SearchMode.match);
        }
        public SearchCtrl CrtSearchCtrl(TableInfo tblInfo, string colName, Point pos, Size size, SearchCtrl.SearchMode mode)
        {
            int iCol = tblInfo.getColIndex(colName);
            if (iCol != -1)
            {
                return CrtSearchCtrl(tblInfo, iCol, pos, size, mode);
            }
            return null;
        }
        public SearchCtrl crtSearchCtrl(TableInfo tblInfo, int iCol, Point pos, Size size)
        {
            return CrtSearchCtrl(tblInfo, iCol, pos, size, SearchCtrl.SearchMode.match);
        }
        public SearchCtrl CrtSearchCtrl(TableInfo tblInfo, int iCol, Point pos, Size size, SearchCtrl.SearchMode mode)
        {
            TableInfo.ColInfo col = tblInfo.m_cols[iCol];
            switch (col.m_type)
            {
                case TableInfo.ColInfo.ColType.text:
                case TableInfo.ColInfo.ColType.uniq:
                    SearchCtrlText textCtrl = new SearchCtrlText(col.m_field, col.m_alias, SearchCtrl.CtrlType.text, pos, size);
                    textCtrl.m_mode = mode;
                    textCtrl.m_colInfo = col;
                    return textCtrl;
                case TableInfo.ColInfo.ColType.dateTime:
                    lSearchCtrlDate dateCtrl = new lSearchCtrlDate(col.m_field, col.m_alias, SearchCtrl.CtrlType.dateTime, pos, size);
                    return dateCtrl;
                case TableInfo.ColInfo.ColType.num:
                    lSearchCtrlNum numCtrl = new lSearchCtrlNum(col.m_field, col.m_alias, SearchCtrl.CtrlType.num, pos, size);
                    return numCtrl;
                case TableInfo.ColInfo.ColType.currency:
                    lSearchCtrlCurrency currencyCtrl = new lSearchCtrlCurrency(col.m_field, col.m_alias, SearchCtrl.CtrlType.currency, pos, size);
                    return currencyCtrl;
                case TableInfo.ColInfo.ColType.map:
                    SearchCtrlEnum srchCtrl = new SearchCtrlEnum(col.m_field, col.m_alias, SearchCtrl.CtrlType.map, pos, size);
                    srchCtrl.m_colInfo = col;
                    return srchCtrl;
            }
            return null;
        }

        // Dispose() calls Dispose(true)  
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // NOTE: Leave out the finalizer altogether if this class doesn't   
        // own unmanaged resources itself, but leave the other methods  
        // exactly as they are.   
        ~SearchPanel()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)  
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources  
                foreach (var ctrl in m_searchCtrls)
                {
                    ctrl.Dispose();
                }
            }
            // free native resources if there are any.  
            m_tblPanel.Dispose();
            m_searchBtn.Dispose();
            m_searchCtrls.Clear();
        }
    }

    [DataContract(Name = "ReceiptsSearchPanel")]
    public class lReceiptsSearchPanel : SearchPanel
    {
        public lReceiptsSearchPanel(lDataPanel dataPanel)
        {
            m_dataPanel = dataPanel;
            m_searchCtrls = new List<SearchCtrl> {
                    CrtSearchCtrl(m_tblInfo, "date", new Point(0, 0), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "receipt_number", new Point(0, 1), new Size(1, 1), SearchCtrl.SearchMode.match),
                    CrtSearchCtrl(m_tblInfo, "name", new Point(1, 0), new Size(1, 1), SearchCtrl.SearchMode.like),
                    CrtSearchCtrl(m_tblInfo, "content", new Point(1, 1), new Size(1, 1), SearchCtrl.SearchMode.match),
                };
        }
    }

    [DataContract(Name = "InterPaymentSearchPanel")]
    public class lInterPaymentSearchPanel : SearchPanel
    {
        public lInterPaymentSearchPanel(lDataPanel dataPanel)
        {
            m_dataPanel = dataPanel;
            m_searchCtrls = new List<SearchCtrl> {
                    CrtSearchCtrl(m_tblInfo, "date", new Point(0, 0), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "payment_number", new Point(0, 1), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "name", new Point(1, 0), new Size(1, 1), SearchCtrl.SearchMode.like),
                    CrtSearchCtrl(m_tblInfo, "group_name", new Point(1, 1), new Size(1, 1), SearchCtrl.SearchMode.match),
                    //crtSearchCtrl(m_tblInfo, "advance_payment", new Point(0, 2), new Size(1, 1)),
                    //crtSearchCtrl(m_tblInfo, "reimbursement", new Point(1, 2), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "content", new Point(0, 2), new Size(1, 1), SearchCtrl.SearchMode.like),
                };
        }
    }

    [DataContract(Name = "ExternalPaymentSearchPanel")]
    public class lExternalPaymentSearchPanel : SearchPanel
    {
        public lExternalPaymentSearchPanel(lDataPanel dataPanel)
        {
            m_dataPanel = dataPanel;
            m_searchCtrls = new List<SearchCtrl> {
                    CrtSearchCtrl(m_tblInfo, "date", new Point(0, 0), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "payment_number", new Point(0, 1), new Size(1, 1), SearchCtrl.SearchMode.match),
                    CrtSearchCtrl(m_tblInfo, "name", new Point(0, 2), new Size(1, 1), SearchCtrl.SearchMode.like),
                    CrtSearchCtrl(m_tblInfo, "group_name", new Point(1, 0), new Size(1, 1), SearchCtrl.SearchMode.match),
                    CrtSearchCtrl(m_tblInfo, "content", new Point(1, 1), new Size(1, 1), SearchCtrl.SearchMode.like),
                    CrtSearchCtrl(m_tblInfo, "building", new Point(1, 2), new Size(1, 1), SearchCtrl.SearchMode.match),
                    CrtSearchCtrl(m_tblInfo, "constr_org", new Point(1, 3), new Size(1, 1), SearchCtrl.SearchMode.match),
                };
        }
    }

    [DataContract(Name = "SalarySearchPanel")]
    public class lSalarySearchPanel : SearchPanel
    {
        public lSalarySearchPanel(lDataPanel dataPanel)
        {
            m_dataPanel = dataPanel;
            m_searchCtrls = new List<SearchCtrl> {
                    //crtSearchCtrl(m_tblInfo, "month", new Point(0, 0), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "date"             , new Point(0, 0), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "payment_number"   , new Point(0, 1), new Size(1, 1), SearchCtrl.SearchMode.match),
                    CrtSearchCtrl(m_tblInfo, "name"             , new Point(1, 0), new Size(1, 1), SearchCtrl.SearchMode.like),
                    CrtSearchCtrl(m_tblInfo, "group_name"       , new Point(1, 1), new Size(1, 1), SearchCtrl.SearchMode.match),
                    CrtSearchCtrl(m_tblInfo, "content"          , new Point(1, 2), new Size(1, 1), SearchCtrl.SearchMode.like),
                };
        }
    }

    [DataContract(Name = "AdvanceSearchPanel")]
    public class lAdvanceSearchPanel : SearchPanel
    {
        public lAdvanceSearchPanel(lDataPanel dataPanel)
        {
            m_dataPanel = dataPanel;
            m_searchCtrls = new List<SearchCtrl> {
                    //crtSearchCtrl(m_tblInfo, "month", new Point(0, 0), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "date"             , new Point(0, 0), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "payment_number"   , new Point(0, 1), new Size(1, 1), SearchCtrl.SearchMode.match),
                    CrtSearchCtrl(m_tblInfo, "name"             , new Point(1, 0), new Size(1, 1), SearchCtrl.SearchMode.like),
                    CrtSearchCtrl(m_tblInfo, "group_name"       , new Point(1, 1), new Size(1, 1), SearchCtrl.SearchMode.match),
                    CrtSearchCtrl(m_tblInfo, "content"          , new Point(1, 2), new Size(1, 1), SearchCtrl.SearchMode.like),
                };
        }
    }

    [DataContract(Name = "TaskSearchPanel")]
    public class TaskSearchPanel : SearchPanel
    {
        public TaskSearchPanel(lDataPanel dataPanel)
        {
            m_dataPanel = dataPanel;
            m_searchCtrls = new List<SearchCtrl> {
                    CrtSearchCtrl(m_tblInfo, TaskTblInfo.ColIdx.Begin.ToField(), new Point(0, 0), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, TaskTblInfo.ColIdx.End.ToField() , new Point(0, 1), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, TaskTblInfo.ColIdx.Task.ToField(), new Point(0, 2), new Size(1, 1), SearchCtrl.SearchMode.match),
                    CrtSearchCtrl(m_tblInfo, TaskTblInfo.ColIdx.Name.ToField(), new Point(1, 0), new Size(1, 1), SearchCtrl.SearchMode.like),
                    CrtSearchCtrl(m_tblInfo, TaskTblInfo.ColIdx.Group.ToField(), new Point(1, 1), new Size(1, 1), SearchCtrl.SearchMode.match),
                    CrtSearchCtrl(m_tblInfo, TaskTblInfo.ColIdx.Stat.ToField(), new Point(1, 2), new Size(1, 1), SearchCtrl.SearchMode.match),
                };
        }
    }
    [DataContract(Name = "OrderSearchPanel")]
    public class OrderSearchPanel : SearchPanel
    {
        public OrderSearchPanel(lDataPanel dataPanel)
        {
            m_dataPanel = dataPanel;
            m_searchCtrls = new List<SearchCtrl> {
                    CrtSearchCtrl(m_tblInfo, "task_number"    , new Point(0, 0), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "order_number"   , new Point(0, 1), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "order_type"     , new Point(1, 0), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "order_status"   , new Point(1, 1), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "note"           , new Point(1, 2), new Size(1, 1), SearchCtrl.SearchMode.like),
                };
        }
    }

    [DataContract(Name = "HumanSearchPanel")]
    public class HumanSearchPanel : SearchPanel
    {
        public HumanSearchPanel(lDataPanel dataPanel)
        {
            m_dataPanel = dataPanel;
            m_searchCtrls = new List<SearchCtrl> {
                    CrtSearchCtrl(m_tblInfo, "human_number" , new Point(0, 0), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "start_date"   , new Point(0, 1), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "end_date"     , new Point(0, 2), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "gender"          , new Point(1, 0), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "note"         , new Point(1, 1), new Size(1, 1), SearchCtrl.SearchMode.like),
                };
        }
    }

    [DataContract(Name = "EquipmentSearchPanel")]
    public class EquipmentSearchPanel : SearchPanel
    {
        public EquipmentSearchPanel(lDataPanel dataPanel)
        {
            m_dataPanel = dataPanel;
            m_searchCtrls = new List<SearchCtrl> {
                    CrtSearchCtrl(m_tblInfo, "equipment_number" , new Point(0, 0), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, "note"             , new Point(0, 1), new Size(1, 1), SearchCtrl.SearchMode.like),
                };
        }
    }

    [DataContract(Name = "LectureSearchPanel")]
    public class LectureSearchPanel : SearchPanel
    {
        public LectureSearchPanel(lDataPanel dataPanel)
        {
            m_dataPanel = dataPanel;
            m_searchCtrls = new List<SearchCtrl> {
                    CrtSearchCtrl(m_tblInfo, LectureTblInfo.ColIdx.lect.ToField(), new Point(0, 0), new Size(1, 1),SearchCtrl.SearchMode.match),
                    CrtSearchCtrl(m_tblInfo, LectureTblInfo.ColIdx.title.ToField() , new Point(0, 1), new Size(1, 1),SearchCtrl.SearchMode.like),
                    CrtSearchCtrl(m_tblInfo, LectureTblInfo.ColIdx.auth.ToField(), new Point(0, 2), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, LectureTblInfo.ColIdx.target.ToField(), new Point(1, 0), new Size(1, 1)),
                    CrtSearchCtrl(m_tblInfo, LectureTblInfo.ColIdx.topic.ToField(), new Point(1, 1), new Size(1, 1), SearchCtrl.SearchMode.match),
                    CrtSearchCtrl(m_tblInfo, LectureTblInfo.ColIdx.crt.ToField(), new Point(1, 2), new Size(1, 1)),
                };
        }
    }
}