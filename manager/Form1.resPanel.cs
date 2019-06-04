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

        public string m_tbl;
        public TableLayoutPanel toprightTLP;
        public DataGridView resDGV;
        public OrderResPanel m_orderResPanel;

        public ResPanel()
        {

        }
        public virtual void InitCtrl()
        {
            toprightTLP = new TableLayoutPanel();
            toprightTLP.Dock = DockStyle.Fill;
        }
        public virtual void UpdateResDGV(string taskId, DateTime startDate, DateTime endDate)
        {

        }
        public void unmarkResRow(int i) { resDGV.Rows[i].DefaultCellStyle.BackColor = Color.White; }
        public void markResRow(int i) { resDGV.Rows[i].DefaultCellStyle.BackColor = Color.Gray; }

    }

    [DataContract(Name = "HumanResPanel")]
    public class HumanResPanel : ResPanel
    {
        SearchBuilder m_humanSB;

        public HumanResPanel()
        {
            m_tbl = "human";
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
            resDGV = lConfigMng.crtDGV();
            resDGV.EnableHeadersVisualStyles = false;
            resDGV.Dock = DockStyle.Fill;
            //hide ID
            resDGV.ColumnAdded += ResGV_ColumnAdded;
            resDGV.DataBindingComplete += ResGV_DataBindingComplete;
            resDGV.CellFormatting += ResDGV_CellFormatting;
            resDGV.CellParsing += ResDGV_CellParsing;
            //resGV.AutoGenerateColumns = false;
            resDGV.AllowUserToAddRows = false;
            resDGV.AllowUserToDeleteRows = false;
            toprightTLP.Controls.Add(resDGV, 0, ++iRow);
        }
        public override void UpdateResDGV(string taskId, DateTime startDate, DateTime endDate)
        {
            base.UpdateResDGV(taskId, startDate, endDate);
            var tblInfo = appConfig.s_config.getTable("human");
            if (m_humanSB == null) { m_humanSB = new SearchBuilder(tblInfo); }
            m_humanSB.clear();
            m_humanSB.add("start_date", startDate, "<=");
            m_humanSB.add("end_date", endDate, ">=");
            m_humanSB.search();
            UpdateResDGV(tblInfo, m_humanSB.dc);
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
            for (int i = 0; i < resDGV.Rows.Count; i++)
            {
                var key = resDGV.Rows[i].Cells[1].Value.ToString();
                if (m_orderResPanel.m_usedResDict.ContainsKey(key))
                {
                    m_orderResPanel.m_usedResDict[key] = i;
                    markResRow(i);
                }
            }
        }
        private void ResGV_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            //hide colnum ID
            if (e.Column.HeaderText == "ID")
            {
                e.Column.Visible = false;
            }
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
            resDGV = lConfigMng.crtDGV();
            resDGV.EnableHeadersVisualStyles = false;
            resDGV.Dock = DockStyle.Fill;
            //hide ID
            resDGV.ColumnAdded += ResGV_ColumnAdded;
            resDGV.DataBindingComplete += ResGV_DataBindingComplete;
            resDGV.CellFormatting += ResDGV_CellFormatting;
            resDGV.CellParsing += ResDGV_CellParsing;
            //resGV.AutoGenerateColumns = false;
            resDGV.AllowUserToAddRows = false;
            resDGV.AllowUserToDeleteRows = false;
            toprightTLP.Controls.Add(resDGV, 0, ++iRow);
        }
        public override void UpdateResDGV(string taskId, DateTime startDate, DateTime endDate)
        {
            base.UpdateResDGV(taskId, startDate, endDate);

            //update res list
            var tblInfo = appConfig.s_config.getTable("equipment");
            if (m_equipSB == null) { m_equipSB = new SearchBuilder(tblInfo); }
            m_equipSB.clear();
            m_equipSB.search();
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
            for (int i = 0; i < resDGV.Rows.Count; i++)
            {
                var key = resDGV.Rows[i].Cells[1].Value.ToString();
                if (m_orderResPanel.m_usedResDict.ContainsKey(key))
                {
                    m_orderResPanel.m_usedResDict[key] = i;
                    markResRow(i);
                }
            }
        }
        private void ResGV_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            //hide colnum ID
            if (e.Column.HeaderText == "ID")
            {
                e.Column.Visible = false;
            }
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
        protected string m_tbl;
        public Dictionary<string, int> m_usedResDict = new Dictionary<string, int>();

        public virtual int RowCount { get { return orderResDGV.RowCount; } }
        public string m_curOrder;
        public ResPanel m_resPanel;
        public OrderResPanel()
        { }
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

            orderResDGV = lConfigMng.crtDGV();
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
                m_resPanel.unmarkResRow(resRowIndex);
                m_usedResDict.Remove(resId);

                idxLst.Add(row.Index);
            }
            removeOrderResByIdx(idxLst);
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
                    m_resPanel.markResRow(row.Index);
                }
            }
            orderResDC.Submit();
        }
        private void removeOrderResByIdx(List<int> idxLst)
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

        public OrderHumanPanel()
        {
            m_tbl = "order_human";
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
            m_orderHumanSB.clear();
            m_orderHumanSB.add("order_number", m_curOrder);
            m_orderHumanSB.search();
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

        public OrderEquipmentPanel()
        {
            m_tbl = "order_equipment";
        }

        public override void UpdateDGV(string orderId)
        {
            base.UpdateDGV(orderId);
            UpdateOrderEquipmentResDGV();
        }
        protected void UpdateOrderEquipmentResDGV()
        {
            var tblInfo = appConfig.s_config.getTable("order_equipment");
            if (m_orderEquipmentSB == null) { m_orderEquipmentSB = new SearchBuilder(tblInfo); }
            m_orderEquipmentSB.clear();
            m_orderEquipmentSB.add("order_number", m_curOrder);
            m_orderEquipmentSB.search();
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
}
