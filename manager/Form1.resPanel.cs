#define enable_search

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
        protected Label resLbl;
        protected Button srchBtn;
        protected TableInfo m_tblInfo;
        protected SearchBuilder m_srchBld;
        protected UpdateBuilder m_updtBld;
        protected DataContent m_resDC;
        protected virtual string ResIdCol { get { throw new NotImplementedException(); } }
        protected virtual string ResStatusCol { get { throw new NotImplementedException(); } }

        public OrderStatus m_curOrderStatus;

        public string m_tbl { get { return m_tblInfo.m_tblName; } }
        public TableLayoutPanel toprightTLP;
        public DataGridView resDGV;
        public OrderResPanel m_orderResPanel;
        public List<SearchCtrl> m_srchCtrls;
        public ResPanel(string tblName)
        {
            m_tblInfo = appConfig.s_config.GetTable(tblName);
            m_srchBld = new SearchBuilder(m_tblInfo);
            m_resDC = m_srchBld.dc;
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

            m_tblInfo.LoadData();
            lConfigMng.CrtColumns(resDGV, m_tblInfo);
            resDGV.DataSource = m_srchBld.dc.m_bindingSource;
        }
        public virtual void InitCtrl()
        {
            toprightTLP = new TableLayoutPanel();
            toprightTLP.Dock = DockStyle.Fill;

            resDGV = lConfigMng.crtDGV(m_tblInfo);
            resDGV.EnableHeadersVisualStyles = false;
            resDGV.Dock = DockStyle.Fill;
            //hide ID
            //resDGV.ColumnAdded += ResGV_ColumnAdded;
            resDGV.DataBindingComplete += ResGV_DataBindingComplete;
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

        public int m_addedSrchCnd;
        private void SrchBtn_Click(object sender, EventArgs e)
        {
            SearchRes();
            MarkUsedRes();
        }
        protected virtual void SearchRes()
        {
            int n = m_srchBld.exprs.Count;

            for (int i = 0; i < m_srchCtrls.Count; i++)
            {
                m_srchCtrls[i].UpdateSearchParams(m_srchBld.exprs, m_srchBld.srchParams);
            }
            m_addedSrchCnd = m_srchBld.exprs.Count - n;
            m_srchBld.Search();

            for (int i = m_srchBld.exprs.Count - 1; i >= n; i--)
            {
                m_srchBld.exprs.RemoveAt(i);
                m_srchBld.srchParams.RemoveAt(i);
            }
        }
        public virtual void UpdateResDGV(string taskId, DateTime startDate, DateTime endDate, OrderStatus orderStatus)
        {
            //update res list
            m_srchBld.Clear();
            m_srchBld.Search();
            //resDGV.DataSource = m_srchBld.dc.m_bindingSource;
            resLbl.Text = m_tblInfo.m_tblAlias;
        }
        //caller: approve()
        public virtual void SetResStatus(ResStatus sts)
        {
            for (int i = 0; i < m_orderResPanel.m_usedResDict.Count; i++)
            {
                KeyValuePair<string, int> rec = m_orderResPanel.m_usedResDict.ElementAt(i);
                int rowIdx = rec.Value;
                string resId = rec.Key;
                if (rowIdx == -1)
                {
                    m_updtBld.Clear();
                    m_updtBld.Add(ResIdCol, resId, isWhere:true);
                    m_updtBld.Add(ResStatusCol, (int)sts);
                    m_updtBld.Update();
                }
                else
                {
                    m_resDC.m_dataTable.Rows[rowIdx][ResStatusCol] = (int)sts;
                }
            }
        }
        public virtual void SetResStatus(int iRow, ResStatus sts)
        {
            m_resDC.m_dataTable.Rows[iRow][ResStatusCol] = (int)sts;
        }
        public virtual void SetResStatus(string resId, ResStatus sts)
        {
            m_updtBld.Clear();
            m_updtBld.Add(ResIdCol, resId, isWhere:true);
            m_updtBld.Add(ResStatusCol, (int)sts);
            m_updtBld.Update();
        }
        public virtual void UnmarkResRow(int i) { resDGV.Rows[i].DefaultCellStyle.BackColor = Color.White; }
        public virtual void MarkResRow(int i) { resDGV.Rows[i].DefaultCellStyle.BackColor = Color.Gray; }

        private void ResDGV_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            var col = m_tblInfo.m_cols[e.ColumnIndex];
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
            var col = m_tblInfo.m_cols[e.ColumnIndex];
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
            MarkUsedRes();
        }
        protected virtual void MarkUsedRes()
        {
            for( int i = 0; i < m_orderResPanel.m_usedResDict.Count; i++)
            {
                string key = m_orderResPanel.m_usedResDict.Keys.ElementAt(i);
                m_orderResPanel.m_usedResDict[key] = -1;
            }
            for (int i = 0; i < resDGV.Rows.Count; i++)
            {
                var resId = resDGV.Rows[i].Cells[ResIdCol].Value.ToString();
                if (m_orderResPanel.m_usedResDict.ContainsKey(resId))
                {
                    m_orderResPanel.m_usedResDict[resId] = i;
                    MarkResRow(i);
                }
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
                        resDGV.Columns[i].DefaultCellStyle.Format = lConfigMng.GetDisplayDateFormat();
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
    }

    [DataContract(Name = "HumanResPanel")]
    public class HumanResPanel : ResPanel
    {
        public HumanResPanel():base(TableIdx.Human.ToDesc())
        {
#if enable_search
            m_srchCtrls = new List<SearchCtrl> {
                CrtSearchCtrl(m_tblInfo, "name"   , new Point(0, 0), new Size(1, 1),SearchCtrl.SearchMode.like),
                CrtSearchCtrl(m_tblInfo, "gender" , new Point(0, 1), new Size(1, 1),SearchCtrl.SearchMode.match),
            };
            m_updtBld = new UpdateBuilder(m_tblInfo);
#endif
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
        }
        public override void UpdateResDGV(string taskId, DateTime startDate, DateTime endDate, OrderStatus orderStatus)
        {
            m_srchBld.Clear();
            m_srchBld.Add(HumanTblInfo.ColIdx.Enter.ToField(), startDate, "<=");
            m_srchBld.Add(HumanTblInfo.ColIdx.Leave.ToField(), endDate, ">=");
            if (orderStatus == OrderStatus.Request) {
                m_srchBld.Add(HumanTblInfo.ColIdx.Busy.ToField(), (int)ResStatus.Free);
            }
            m_srchBld.Search();

            resDGV.DataSource=m_srchBld.dc.m_bindingSource;
            
            string datef = lConfigMng.GetDisplayDateFormat();
            resLbl.Text = string.Format("{0} {1}-{2}", m_tblInfo.m_tblAlias,
                startDate.ToString(datef), endDate.ToString(datef));
        }
        protected override string ResStatusCol => HumanTblInfo.ColIdx.Busy.ToField();
        protected override string ResIdCol => HumanTblInfo.ColIdx.Human.ToField();
        public override void MarkResRow(int rowIdx)
        {
            base.MarkResRow(rowIdx);
            if (m_curOrderStatus == OrderStatus.Approve)
            {
                SetResStatus(rowIdx, ResStatus.Busy);
            }
        }
        public override void UnmarkResRow(int rowIdx)
        {
            base.UnmarkResRow(rowIdx);
            if (m_curOrderStatus == OrderStatus.Approve)
            {
                SetResStatus(rowIdx, ResStatus.Free);
            }
        }
    }
    [DataContract(Name = "EquipmentResPanel")]
    public class EquipmentResPanel : ResPanel
    {
        public EquipmentResPanel():base(TableIdx.Equip.ToDesc())
        {
#if enable_search
            m_srchCtrls = new List<SearchCtrl> {
                CrtSearchCtrl(m_tblInfo, EquipmentTblInfo.ColIdx.Note.ToField() , new Point(0, 0), new Size(1, 1),SearchCtrl.SearchMode.like),
            };
#endif
        }
        public override void InitCtrl()
        {
            base.InitCtrl();

            resLbl.Text = "Chọn tài nguyên cho Yêu Cầu";
        }

        protected override string ResIdCol => EquipmentTblInfo.ColIdx.Eqpt.ToField();

        protected override string ResStatusCol => EquipmentTblInfo.ColIdx.Used.ToField();
    }
    [DataContract(Name = "CarResPanel")]
    public class CarResPanel : ResPanel
    {
        public CarResPanel():base("car")
        {
#if enable_search
            m_srchCtrls = new List<SearchCtrl> {
                CrtSearchCtrl(m_tblInfo, CarTblInfo.ColIdx.Type.ToField(), new Point(0, 0), new Size(1, 1),SearchCtrl.SearchMode.like),
                CrtSearchCtrl(m_tblInfo, CarTblInfo.ColIdx.Brand.ToField(), new Point(0, 1), new Size(1, 1),SearchCtrl.SearchMode.like),
            };
#endif
        }
        public override void InitCtrl()
        {
            base.InitCtrl();
            
            resLbl.Text = "Chọn phương tiện cho Yêu Cầu";
        }
        protected override string ResIdCol => CarTblInfo.ColIdx.Car.ToField();
        protected override string ResStatusCol => CarTblInfo.ColIdx.Used.ToField();
    }
    [DataContract(Name = "OrderResPanel")]
    public class OrderResPanel
    {
        public TableLayoutPanel botRightTLP;
        protected DataGridView orderResDGV;
        protected SearchBuilder orderResSB;
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
        public OrderStatus m_curOrderStatus;
        public string m_curTask;
        public ResPanel m_resPanel;
        public OrderResPanel(string tblName)
        {
            m_tblInfo = appConfig.s_config.GetTable(tblName);
            orderResSB = new SearchBuilder(m_tblInfo);
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
            //tflow.Controls.AddRange(new Control[] { downBtn, upBtn, saveResBtn });
            tflow.Controls.AddRange(new Control[] { downBtn, upBtn });
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
        public virtual void UpdateDGV(string orderId, string taskId)
        {
            m_curOrder = orderId;
            m_curTask = taskId;
        }

        public virtual void RmBusyRes()
        {
            List<int> idxLst = new List<int>();
            List<string> resIdLst = new List<string>();
            for (int i = 0; i < m_usedResDict.Count;i++)
            {
                KeyValuePair<string, int> rec = m_usedResDict.ElementAt(i);
                if (rec.Value == -1)
                {
                    idxLst.Add(i);
                    resIdLst.Add(rec.Key);
                }
            }
            if (idxLst.Count > 0)
            {
                //show warning msg
                string msg = string.Format("The busy resource is invalid!\nRows[{0}] was removed!",
                    string.Join(", ", idxLst));
                lConfigMng.ShowInputError(msg);
            }
            foreach(string resId in resIdLst)
            {
                m_usedResDict.Remove(resId);
            }
            RemoveOrderResByIdx(idxLst);
        }
        protected void UpdateOrderResDGV(string orderIdCol, string resIdCol)
        {
            var tblInfo = appConfig.s_config.GetTable(m_tbl);
            orderResSB.Clear();
            orderResSB.Add(orderIdCol, m_curOrder);
            orderResSB.Search();
            orderResDGV.DataSource = orderResSB.dc.m_bindingSource;
            
            //build dict
            m_usedResDict.Clear();
            var rows = orderResSB.dc.m_dataTable.Rows;
            int i;
            for (i = 0; i < rows.Count; i++)
            {
                var key = rows[i][resIdCol].ToString();
                m_usedResDict.Add(key, -1); //not yet set res row index
            }
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
                var resId = GetResId(row);

                //udpate dict & gui
                int resRowIndex = m_usedResDict[resId];
                m_usedResDict.Remove(resId);
                if (resRowIndex == -1)
                {
                    //search res
                    Debug.Assert(m_resPanel.m_addedSrchCnd > 0);
                    if (m_curOrderStatus == OrderStatus.Approve)
                    {
                        //update res status by exec sql qry
                        m_resPanel.SetResStatus(resId, ResStatus.Free);
                    }
                }
                else
                {
                    m_resPanel.UnmarkResRow(resRowIndex);
                    if (m_curOrderStatus == OrderStatus.Approve)
                    {
                        m_resPanel.SetResStatus(resRowIndex, ResStatus.Free);
                    }
                }

                idxLst.Add(row.Index);
            }
            RemoveOrderResByIdx(idxLst);
            Save();
        }
        protected virtual string GetResId(DataGridViewRow row) { return row.Cells[2].Value.ToString(); }
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
                    newRow[3] = m_curTask;
                    orderResTbl.Rows.Add(newRow);

                    //udpate dict & gui
                    m_usedResDict.Add(resId, resRowIdx);
                    m_resPanel.MarkResRow(row.Index);
                    if (m_curOrderStatus == OrderStatus.Approve)
                    {
                        m_resPanel.SetResStatus(resRowIdx, ResStatus.Busy);
                    }
                }
            }
            //orderResDC.Submit();
            Save();
        }
        private void RemoveOrderResByIdx(List<int> idxLst)
        {
            DataContent orderResDC = appConfig.s_contentProvider.CreateDataContent(m_curOrderResTbl);
            //remove in datatable
            idxLst.Sort();
            for (int i = idxLst.Count - 1; i >= 0; i--)
            {
                orderResDC.m_bindingSource.RemoveAt(idxLst[i]);
            }
            //orderResDC.Submit();
        }
        private void SaveResBtn_Click(object sender, EventArgs e)
        {
            Save();
        }
        public void Save()
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
        public OrderHumanPanel() : base(TableIdx.HumanOR.ToDesc())
        {
        }
        public override void UpdateDGV(string orderId, string taskId)
        {
            base.UpdateDGV(orderId, taskId);
            UpdateOrderResDGV(OrderHumanTblInfo.ColIdx.Order.ToField(), OrderHumanTblInfo.ColIdx.Human.ToField());
        }
    }
    [DataContract(Name = "OrderEquipmentPanel")]
    public class OrderEquipmentPanel:OrderResPanel
    {
        public OrderEquipmentPanel() : base(TableIdx.EquipOR.ToDesc())
        {
        }

        public override void UpdateDGV(string orderId, string taskId)
        {
            base.UpdateDGV(orderId, taskId);
            UpdateOrderResDGV(OrderEquipmentTblInfo.ColIdx.Order.ToField(), OrderEquipmentTblInfo.ColIdx.Equip.ToField());
        }
    }
    [DataContract(Name = "OrderCarPanel")]
    public class OrderCarPanel : OrderResPanel
    {
        public OrderCarPanel():base(TableIdx.CarOR.ToDesc())
        {
        }

        public override void UpdateDGV(string orderId, string taskId)
        {
            base.UpdateDGV(orderId, taskId);
            UpdateOrderResDGV(OrderCarTblInfo.ColIdx.Order.ToField(), OrderCarTblInfo.ColIdx.Car.ToField());
        }
    }
}
