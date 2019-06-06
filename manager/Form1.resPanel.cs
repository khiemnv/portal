using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_binding
{
    [DataContract(Name = "ResPanel")]
    public class ResPanel
    {
        protected TableInfo resTblInfo;
        protected Label resLbl;
        protected Button srchBtn;
        private TableInfo m_tblInfo;
        protected TableInfo tblInfo { get{
                if (m_tblInfo == null) { m_tblInfo = appConfig.s_config.getTable(m_tbl); }
                return m_tblInfo; } }
        private SearchBuilder m_srchBld;
        protected SearchBuilder srchBld { get {
                if (m_srchBld == null) { m_srchBld = new SearchBuilder(tblInfo); }
                return m_srchBld; } }

        public string m_tbl;
        public TableLayoutPanel toprightTLP;
        public DataGridView resDGV;
        public OrderResPanel m_orderResPanel;
        public List<SearchCtrl> m_srchCtrls;
        public ResPanel()
        {

        }
        public virtual void LoadData()
        {
            if (m_srchCtrls != null)
            {
                for (int i = 0; i < m_srchCtrls.Count; i++)
                {
                    m_srchCtrls[i].LoadData();
                }
            }
        }
        public virtual void InitCtrl()
        {
            toprightTLP = new TableLayoutPanel();
            toprightTLP.Dock = DockStyle.Fill;

            resDGV = lConfigMng.crtDGV(tblInfo);
            resDGV.EnableHeadersVisualStyles = false;
            resDGV.Dock = DockStyle.Fill;
            //hide ID
            //resDGV.ColumnAdded += ResGV_ColumnAdded;
            //resDGV.DataBindingComplete += ResGV_DataBindingComplete;
            //resDGV.CellFormatting += ResDGV_CellFormatting;
            //resDGV.CellParsing += ResDGV_CellParsing;
            //resGV.AutoGenerateColumns = false;
            resDGV.AllowUserToAddRows = false;
            resDGV.AllowUserToDeleteRows = false;

            int iRow = 0;
            resLbl = lConfigMng.crtLabel();
            //lable |<res table >       |
            resLbl.AutoSize = true;
            toprightTLP.Controls.Add(resLbl, 0, ++iRow);
            //search ctrls
            if (m_srchCtrls != null)
            {
                for (int i = 0; i < m_srchCtrls.Count; i++)
                {
                    toprightTLP.Controls.Add(m_srchCtrls[i].m_panel, 0, ++iRow);
                }
                srchBtn = lConfigMng.crtButton();
                srchBtn.Text = "Search";
                srchBtn.Click += SrchBtn_Click;
                srchBtn.Anchor = AnchorStyles.Right;
                toprightTLP.Controls.Add(srchBtn, 0, ++iRow);
            }
            //resLbl.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
            //resLbl.BorderStyle = BorderStyle.None;
            toprightTLP.Controls.Add(resDGV, 0, ++iRow);
        }

        private void SrchBtn_Click(object sender, EventArgs e)
        {
            SearchRes();
        }
        protected virtual void SearchRes() { }
        public virtual void UpdateResDGV(string taskId, DateTime startDate, DateTime endDate)
        {

        }
        public void UnmarkResRow(int i) { resDGV.Rows[i].DefaultCellStyle.BackColor = Color.White; }
        public void MarkResRow(int i) { resDGV.Rows[i].DefaultCellStyle.BackColor = Color.Gray; }

        private void ResDGV_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            var col = resTblInfo.m_cols[e.ColumnIndex];
            switch (col.m_type)
            {
                case TableInfo.ColInfo.ColType.map:
                    {
                        Debug.WriteLine("OnCellParsing parsing enum");
                        string val = e.Value.ToString();
                        if (col.ParseEnum(val, out int n))
                        {
                            e.ParsingApplied = true;
                            e.Value = n;
                        }
                    }
                    break;
            }
        }
        private void ResDGV_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null) return;
            var col = resTblInfo.m_cols[e.ColumnIndex];
            switch (col.m_type)
            {
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
        private void ResGV_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            //for (int i = 0; i < resDGV.Rows.Count; i++)
            //{
            //    var key = resDGV.Rows[i].Cells[1].Value.ToString();
            //    if (m_orderResPanel.m_usedResDict.ContainsKey(key))
            //    {
            //        m_orderResPanel.m_usedResDict[key] = i;
            //        MarkResRow(i);
            //    }
            //}

#if !manual_crt_dgv_columns
            if (resDGV.AutoGenerateColumns == true)
            {
                UpdateCols();
                resDGV.AutoGenerateColumns = false;
            }
#endif
            //fix col["ID"] not hide
            if (resDGV.Columns[0].Visible)
            {
                resDGV.Columns[0].Visible = false;
            }
        }
        private void UpdateCols()
        {
            resDGV.Columns[0].Visible = false;
            TableInfo tblInfo = m_tblInfo;
            int i = 1;
            for (; i < resDGV.ColumnCount; i++)
            {
                //show hide columns
                if (tblInfo.m_cols[i].m_visible == false)
                {
                    resDGV.Columns[i].Visible = false;
                    continue;
                }

                resDGV.Columns[i].HeaderText = tblInfo.m_cols[i].m_alias;

#if header_blue
                //header color blue
                m_dataGridView.Columns[i].HeaderCell.Style.BackColor = Color.Blue;
                m_dataGridView.Columns[i].HeaderCell.Style.ForeColor = Color.White;
#endif

                switch (tblInfo.m_cols[i].m_type)
                {
                    case TableInfo.ColInfo.ColType.currency:
                        resDGV.Columns[i].DefaultCellStyle.Format = lConfigMng.getCurrencyFormat();
                        break;
                    case TableInfo.ColInfo.ColType.dateTime:
                        resDGV.Columns[i].DefaultCellStyle.Format = lConfigMng.getDisplayDateFormat();
                        break;
                }
#if false
                    m_dataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    m_dataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    m_dataGridView.Columns[i].FillWeight = 1;
#endif
            }
            resDGV.Columns[i - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            resDGV.Columns[i - 1].FillWeight = 1;
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
        public SearchCtrl CrtSearchCtrl(TableInfo tblInfo, int iCol, Point pos, Size size, SearchCtrl.SearchMode mode)
        {
            TableInfo.ColInfo col = tblInfo.m_cols[iCol];
            switch (col.m_type)
            {
                case TableInfo.ColInfo.ColType.text:
                case TableInfo.ColInfo.ColType.uniqueText:
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
    }

    [DataContract(Name = "HumanResPanel")]
    public class HumanResPanel : ResPanel
    {
        public HumanResPanel()
        {
            m_tbl = "human";
            m_srchCtrls = new List<SearchCtrl> {
                CrtSearchCtrl(tblInfo, "human_number" , new Point(0, 0), new Size(1, 1),SearchCtrl.SearchMode.match),
                CrtSearchCtrl(tblInfo, "name"   , new Point(0, 1), new Size(1, 1),SearchCtrl.SearchMode.like),
                CrtSearchCtrl(tblInfo, "gender" , new Point(0, 2), new Size(1, 1),SearchCtrl.SearchMode.match),
                CrtSearchCtrl(tblInfo, "age"    , new Point(0, 3), new Size(1, 1),SearchCtrl.SearchMode.match),
            };
        }
        public override void InitCtrl()
        {
            base.InitCtrl();
            
            //  lable <human dd/mm/yy - dd/mm/yy>
            resLbl.Text = "Chọn tài nguyên cho Yêu Cầu";
        }
        protected override void SearchRes()
        {
            base.SearchRes();
            int n = srchBld.exprs.Count;

            for (int i = 0;i<m_srchCtrls.Count;i++)
            {
                m_srchCtrls[i].UpdateSearchParams(srchBld.exprs, srchBld.srchParams);
            }
            srchBld.Search();
            
            for (int i = srchBld.exprs.Count -1; i >= n; i--)
            {
                srchBld.exprs.RemoveAt(i);
                srchBld.srchParams.RemoveAt(i);
            }
        }
        public override void UpdateResDGV(string taskId, DateTime startDate, DateTime endDate)
        {
            base.UpdateResDGV(taskId, startDate, endDate);
            srchBld.Clear();
            srchBld.Add("start_date", startDate, "<=");
            srchBld.Add("end_date", endDate, ">=");
            srchBld.Search();
            UpdateResDGV(tblInfo, srchBld.dc);
        }
        private void UpdateResDGV(TableInfo tblInfo, DataContent dc)
        {
            resTblInfo = tblInfo;
            resDGV.DataSource = dc.m_bindingSource;

            //hide col["ID"]
            //UpdateDGVCols(resDGV, tblInfo);

            //update lable
            //UpdateResLabel(tblInfo.m_tblAlias);
        }
        private void UpdateResLabel(string resTblAlias, DateTime startDate, DateTime endDate)
        {
            string datef = lConfigMng.getDisplayDateFormat();
            resLbl.Text = string.Format("{0} {1}-{2}",resTblAlias,
                startDate.ToString(datef), endDate.ToString(datef));
        }
     }
    [DataContract(Name = "EquipmentResPanel")]
    public class EquipmentResPanel : ResPanel
    {
        SearchBuilder m_equipSB;

        public EquipmentResPanel()
        {
            m_tbl = "equipment";
        }
        public override void InitCtrl()
        {
            base.InitCtrl();

            int iRow = 0;
            resLbl = lConfigMng.crtLabel();
            toprightTLP.Controls.Add(resLbl, 0, ++iRow);
            //  lable <human dd/mm/yy - dd/mm/yy>
            resLbl.Text = "Chọn tài nguyên cho Yêu Cầu";
            //lable |<res table >       |
            resLbl.AutoSize = true;
            //resLbl.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
            //resLbl.BorderStyle = BorderStyle.None;
            toprightTLP.Controls.Add(resDGV, 0, ++iRow);
        }
        public override void UpdateResDGV(string taskId, DateTime startDate, DateTime endDate)
        {
            base.UpdateResDGV(taskId, startDate, endDate);

            //update res list
            var tblInfo = appConfig.s_config.getTable("equipment");
            if (m_equipSB == null) { m_equipSB = new SearchBuilder(tblInfo); }
            m_equipSB.Clear();
            m_equipSB.Search();
            UpdateResDGV(tblInfo, m_equipSB.dc);
        }
        private void UpdateResDGV(TableInfo tblInfo, DataContent dc)
        {
            resTblInfo = tblInfo;
            resDGV.DataSource = dc.m_bindingSource;

            //hide col["ID"]
            //UpdateDGVCols(resDGV, tblInfo);

            //update lable
            UpdateResLabel(tblInfo.m_tblAlias);
        }
        private void UpdateResLabel(string resTblAlias)
        {
            resLbl.Text = resTblAlias;
        }
    }
    [DataContract(Name = "CarResPanel")]
    public class CarResPanel : ResPanel
    {
        SearchBuilder m_resSB;

        public CarResPanel()
        {
            m_tbl = "car";
        }
        public override void InitCtrl()
        {
            base.InitCtrl();

            int iRow = 0;
            resLbl = lConfigMng.crtLabel();
            toprightTLP.Controls.Add(resLbl, 0, ++iRow);
            //  lable <human dd/mm/yy - dd/mm/yy>
            resLbl.Text = "Chọn phương tiện cho Yêu Cầu";
            //lable |<res table >       |
            resLbl.AutoSize = true;
            //resLbl.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
            //resLbl.BorderStyle = BorderStyle.None;
            toprightTLP.Controls.Add(resDGV, 0, ++iRow);
        }
        public override void UpdateResDGV(string taskId, DateTime startDate, DateTime endDate)
        {
            base.UpdateResDGV(taskId, startDate, endDate);

            //update res list
            var tblInfo = appConfig.s_config.getTable(m_tbl);
            if (m_resSB == null) { m_resSB = new SearchBuilder(tblInfo); }
            m_resSB.Clear();
            m_resSB.Search();
            UpdateResDGV(tblInfo, m_resSB.dc);
        }
        private void UpdateResDGV(TableInfo tblInfo, DataContent dc)
        {
            resTblInfo = tblInfo;
            resDGV.DataSource = dc.m_bindingSource;

            //hide col["ID"]
            //UpdateDGVCols(resDGV, tblInfo);

            //update lable
            UpdateResLabel(tblInfo.m_tblAlias);
        }
        private void UpdateResLabel(string resTblAlias)
        {
            resLbl.Text = resTblAlias;
        }
    }
    [DataContract(Name = "OrderResPanel")]
    public class OrderResPanel
    {
        public TableLayoutPanel botRightTLP;
        protected DataGridView orderResDGV;
        Button downBtn;
        Button upBtn;
        Button saveResBtn;

        protected virtual string m_curOrderResTbl { get { return m_tbl; } }
        protected virtual string m_curResTbl { get { return m_resPanel.m_tbl; } }
        protected string m_tbl { get { return m_tblInfo.m_tblName; } }
        protected readonly TableInfo  m_tblInfo;
        public Dictionary<string, int> m_usedResDict = new Dictionary<string, int>();

        public virtual int RowCount { get { return orderResDGV.RowCount; } }
        public string m_curOrder;
        public ResPanel m_resPanel;
        public OrderResPanel(string tblName)
        {
            m_tblInfo = appConfig.s_config.getTable(tblName);
        }
        public virtual void InitCtrl()
        {
            botRightTLP = new TableLayoutPanel();
            botRightTLP.Dock = DockStyle.Fill;
            int iRow = 0;
            downBtn = lConfigMng.crtButton();
            upBtn = lConfigMng.crtButton();
            saveResBtn = lConfigMng.crtButton();
            //  up  | down  | save
            downBtn.Text = "Down";
            downBtn.Click += DownBtn_Click;
            upBtn.Text = "Up";
            upBtn.Click += UpBtn_Click;
            saveResBtn.Text = "Save";
            saveResBtn.Click += SaveResBtn_Click;
            FlowLayoutPanel tflow = new FlowLayoutPanel();
            tflow.FlowDirection = FlowDirection.LeftToRight;
            tflow.Controls.AddRange(new Control[] { downBtn, upBtn, saveResBtn });
            tflow.AutoSize = true;
            tflow.Anchor = AnchorStyles.Right;
            botRightTLP.Controls.Add(tflow, 0, ++iRow);

            orderResDGV = lConfigMng.crtDGV(m_tblInfo);
            orderResDGV.EnableHeadersVisualStyles = false;
            orderResDGV.Dock = DockStyle.Fill;
            //orderResGV.AutoGenerateColumns = false;
            orderResDGV.ColumnAdded += OrderResGV_ColumnAdded;
            orderResDGV.AllowUserToAddRows = false;
            orderResDGV.AllowUserToDeleteRows = false;
            //  order - res
            botRightTLP.Controls.Add(orderResDGV, 0, ++iRow);
        }
        public virtual void UpdateDGV(string orderId)
        {
            m_curOrder = orderId;
        }
        private void OrderResGV_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            //hide colnum ID
            if (e.Column.HeaderText == "ID")
            {
                e.Column.Visible = false;
            }
        }

        private void UpBtn_Click(object sender, EventArgs e)
        {
            DataContent orderResDC = appConfig.s_contentProvider.CreateDataContent(m_curOrderResTbl);
            DataTable orderResTbl = orderResDC.m_dataTable;
            List<int> idxLst = new List<int>();
            for (int i = 0; i < orderResDGV.SelectedRows.Count; i++)
            {
                DataGridViewRow row = orderResDGV.SelectedRows[i];
                var resId = row.Cells[2].Value.ToString();

                //udpate dict & gui
                int resRowIndex = m_usedResDict[resId];
                m_resPanel.UnmarkResRow(resRowIndex);
                m_usedResDict.Remove(resId);

                idxLst.Add(row.Index);
            }
            RemoveOrderResByIdx(idxLst);
        }
        private void DownBtn_Click(object sender, EventArgs e)
        {
            DataContent orderResDC = appConfig.s_contentProvider.CreateDataContent(m_curOrderResTbl);
            DataTable orderResTbl = orderResDC.m_dataTable;
            for (int i = 0; i < m_resPanel.resDGV.SelectedRows.Count; i++)
            {
                DataGridViewRow row = m_resPanel.resDGV.SelectedRows[i];
                var resId = row.Cells[1].Value.ToString();
                if (!m_usedResDict.ContainsKey(resId))
                {
                    var resRowIdx = row.Index;

                    var newRow = orderResTbl.NewRow();
                    newRow[1] = m_curOrder;
                    newRow[2] = resId;
                    orderResTbl.Rows.Add(newRow);

                    //udpate dict & gui
                    m_usedResDict.Add(resId, resRowIdx);
                    m_resPanel.MarkResRow(row.Index);
                }
            }
            orderResDC.Submit();
        }
        private void RemoveOrderResByIdx(List<int> idxLst)
        {
            DataContent orderResDC = appConfig.s_contentProvider.CreateDataContent(m_curOrderResTbl);
            //remove in datatable
            idxLst.Sort();
            for (int idx = idxLst.Count - 1; idx >= 0; idx--)
            {
                orderResDC.m_bindingSource.RemoveAt(idx);
                //orderResGV.Rows.RemoveAt(idx);
            }
            orderResDC.Submit();
        }
        private void SaveResBtn_Click(object sender, EventArgs e)
        {
            DataContent orderResDC = appConfig.s_contentProvider.CreateDataContent(m_curOrderResTbl);
            DataContent resDC = appConfig.s_contentProvider.CreateDataContent(m_curResTbl);
            orderResDC.Submit();
            resDC.Submit();
        }
    }
    [DataContract(Name = "OrderHumanPanel")]
    public class OrderHumanPanel:OrderResPanel
    {
        SearchBuilder m_orderHumanSB;

        public OrderHumanPanel() : base("order_human")
        {
        }
        public override void UpdateDGV(string orderId)
        {
            base.UpdateDGV(orderId);
            UpdateOrderHumanResDGV();
        }
        protected void UpdateOrderHumanResDGV()
        {
            var tblInfo = appConfig.s_config.getTable("order_human");
            if (m_orderHumanSB == null) { m_orderHumanSB = new SearchBuilder(tblInfo); }
            m_orderHumanSB.Clear();
            m_orderHumanSB.Add("order_number", m_curOrder);
            m_orderHumanSB.Search();
            orderResDGV.DataSource = m_orderHumanSB.dc.m_dataTable;

            //UpdateDGVCols(orderResDGV, tblInfo);

            //build dict
            m_usedResDict.Clear();
            var rows = m_orderHumanSB.dc.m_dataTable.Rows;
            int i;
            for (i = 0;i< rows.Count;i++)
            {
                var key = rows[i]["human_number"].ToString();
                m_usedResDict.Add(key, -1); //not set res row index
            }
        }
    }
    [DataContract(Name = "OrderEquipmentPanel")]
    public class OrderEquipmentPanel:OrderResPanel
    {
        SearchBuilder m_orderEquipmentSB;

        public OrderEquipmentPanel() : base("order_equipment")
        {
        }

        public override void UpdateDGV(string orderId)
        {
            base.UpdateDGV(orderId);
            UpdateOrderEquipmentResDGV();
        }
        protected void UpdateOrderEquipmentResDGV()
        {
            if (m_orderEquipmentSB == null) { m_orderEquipmentSB = new SearchBuilder(m_tblInfo); }
            m_orderEquipmentSB.Clear();
            m_orderEquipmentSB.Add("order_number", m_curOrder);
            m_orderEquipmentSB.Search();
            orderResDGV.DataSource = m_orderEquipmentSB.dc.m_dataTable;

            //UpdateDGVCols(orderResDGV, tblInfo);

            //build dict
            m_usedResDict.Clear();
            var rows = m_orderEquipmentSB.dc.m_dataTable.Rows;
            int i;
            for (i = 0; i < rows.Count; i++)
            {
                var key = rows[i]["equipment_number"].ToString();
                m_usedResDict.Add(key, -1); //not set res row index
            }
        }
    }
    [DataContract(Name = "OrderCarPanel")]
    public class OrderCarPanel : OrderResPanel
    {
        SearchBuilder m_orderResSB;
        string m_resNumberCol;
        public OrderCarPanel():base("car")
        {
            m_resNumberCol = "car_number";
        }

        public override void UpdateDGV(string orderId)
        {
            base.UpdateDGV(orderId);
            UpdateOrderCarResDGV();
        }
        protected void UpdateOrderCarResDGV()
        {
            var tblInfo = appConfig.s_config.getTable(m_tbl);
            if (m_orderResSB == null) { m_orderResSB = new SearchBuilder(tblInfo); }
            m_orderResSB.Clear();
            m_orderResSB.Add("order_number", m_curOrder);
            m_orderResSB.Search();
            orderResDGV.DataSource = m_orderResSB.dc.m_dataTable;

            //UpdateDGVCols(orderResDGV, tblInfo);

            //build dict
            m_usedResDict.Clear();
            var rows = m_orderResSB.dc.m_dataTable.Rows;
            int i;
            for (i = 0; i < rows.Count; i++)
            {
                var key = rows[i][m_resNumberCol].ToString();
                m_usedResDict.Add(key, -1); //not set res row index
            }
        }
    }
}
