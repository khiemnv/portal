//#define DEBUG_DRAWING
#define use_custom_dgv
#define manual_crt_dgv_columns
#define use_custom_cols
#define init_datatable_cols
#define format_currency
#define use_cmd_params
#define header_blue
//#define fit_txt_size
//#define use_bg_work

using System.Windows.Forms;
using System;
using System.Drawing;
using System.Runtime.Serialization;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Data.SQLite;

namespace test_binding
{
    /// <summary>
    /// Data Panel
    /// + sum
    /// + data grid
    /// + update()
    ///     cal sum
    ///     update data grid - auto
    /// </summary>
    [DataContract(Name = "DataPanel")]
    public class lDataPanel : IDisposable
    {
        //public TableLayoutPanel m_tbl = new TableLayoutPanel();
        public FlowLayoutPanel m_reloadPanel;
        public Button m_reloadBtn;
        public Button m_submitBtn;
        public Label m_status;

        public FlowLayoutPanel m_sumPanel;
        protected Label m_sumLabel;
        protected TextBox m_sumTxt;

        public DataGridView m_dataGridView;

        public DataContent m_dataContent;

        public TableInfo m_tblInfo { get { return appConfig.s_config.GetTable(m_tblName); } }

        [DataMember(Name = "tblName")]
        public string m_tblName;
        [DataMember(Name = "countOn")]
        public string m_countOn = "";

        protected lDataPanel() { }
        /// <summary>
        /// create new instance
        /// </summary>
        /// <param name="dataPanel"></param>
        /// <returns></returns>
        public static lDataPanel crtDataPanel(lDataPanel dataPanel)
        {
            lDataPanel newDataPanel = new lDataPanel();
            newDataPanel.Init(dataPanel);
            return newDataPanel;
        }

        private void Init(lDataPanel dataPanel)
        {
            m_countOn = dataPanel.m_countOn;
            //m_tblInfo = dataPanel.m_tblInfo;
        }

        public virtual void InitCtrls()
        {
            m_reloadPanel = new FlowLayoutPanel();
            m_sumPanel = new FlowLayoutPanel();

            m_reloadBtn = new Button();
            m_submitBtn = new Button();
            m_status = new Label();
            m_sumLabel = new Label();
            m_sumTxt = new TextBox();

            m_reloadBtn.Text = "Reload";
            m_submitBtn.Text = "Save";
            m_status.AutoSize = true;
            m_status.TextAlign = ContentAlignment.MiddleLeft;
            m_status.Dock = DockStyle.Fill;

            m_reloadBtn.Click += new System.EventHandler(reloadButton_Click);
            m_submitBtn.Click += new System.EventHandler(submitButton_Click);


#if use_custom_dgv
            m_dataGridView = 
                m_tblInfo.m_tblName == "internal_payment"   ? new lInterPaymentDGV(m_tblInfo) :
                m_tblInfo.m_tblName == "salary"             ? new lSalaryDGV(m_tblInfo) :
                m_tblInfo.m_tblName == "advance"            ? new lInterPaymentDGV(m_tblInfo) :
                new lCustomDGV(m_tblInfo);
#else
                m_dataGridView = new DataGridView();
                m_dataGridView.CellClick += M_dataGridView_CellClick;
                m_dataGridView.CellEndEdit += M_dataGridView_CellEndEdit;
                m_dataGridView.Scroll += M_dataGridView_Scroll;
#endif  //use custom dgv

            //m_dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.Silver;
            m_dataGridView.EnableHeadersVisualStyles = false;

            //reload panel with reload and save buttons
            m_reloadPanel.AutoSize = true;
            //m_reloadPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
            m_reloadPanel.Dock = DockStyle.Left;
#if DEBUG_DRAWING
                m_reloadPanel.BorderStyle = BorderStyle.FixedSingle;
#endif
            m_reloadPanel.Controls.AddRange(new Control[] { m_reloadBtn, m_submitBtn, m_status });

            //sum panel
            initSumCtrl();

            m_dataGridView.Anchor = AnchorStyles.Top & AnchorStyles.Left;
            m_dataGridView.Dock = DockStyle.Fill;

            //set font
            List<Control> ctrls = new List<Control> { m_dataGridView,
            m_sumLabel, m_sumTxt, m_reloadBtn, m_submitBtn, m_status};
            foreach (var c in ctrls)
            {
                c.Font = lConfigMng.getFont();
            }

            //update status
            m_stsMng = new statusMng(UpdateStsTxt);
        }

        protected virtual void updateSumCtrl()
        {
            Int64 sum = getSum();
            string txt = sum.ToString(lConfigMng.getCurrencyFormat());
#if fit_txt_size
            int w = lConfigMng.getWidth(txt) + 10;
            m_sumTxt.Width = w;
#endif
            m_sumTxt.Text = txt;
            m_sumLabel.Text = "Sum = " + txt;
        }

        protected virtual void initSumCtrl()
        {
            m_sumLabel.Text = "Sum";
            m_sumPanel.AutoSize = true;
            //m_sumPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
            m_sumPanel.Anchor = AnchorStyles.Right;
#if DEBUG_DRAWING
                m_sumPanel.BorderStyle = BorderStyle.FixedSingle;
#endif
            //sum panel with label and text ctrls
            m_sumPanel.Controls.AddRange(new Control[] {
                m_sumLabel,
                //m_sumTxt
            });

            m_sumLabel.Anchor = AnchorStyles.Right;
            m_sumLabel.TextAlign = ContentAlignment.MiddleRight;
            m_sumLabel.AutoSize = true;

#if fit_txt_size
            m_sumTxt.Width = lConfigMng.getWidth("000,000,000,000");
#else
            m_sumTxt.Width = 100;
#endif
        }


        private void updateCols()
        {
            m_dataGridView.Columns[0].Visible = false;
            TableInfo tblInfo = m_tblInfo;
            int i = 1;
            for (; i < m_dataGridView.ColumnCount; i++)
            {
                //show hide columns
                if (tblInfo.m_cols[i].m_visible == false)
                {
                    m_dataGridView.Columns[i].Visible = false;
                    continue;
                }

                m_dataGridView.Columns[i].HeaderText = tblInfo.m_cols[i].m_alias;

#if header_blue
                //header color blue
                m_dataGridView.Columns[i].HeaderCell.Style.BackColor = Color.Blue;
                m_dataGridView.Columns[i].HeaderCell.Style.ForeColor = Color.White;
#endif

                switch (tblInfo.m_cols[i].m_type)
                {
                    case TableInfo.ColInfo.ColType.currency:
                        m_dataGridView.Columns[i].DefaultCellStyle.Format = lConfigMng.getCurrencyFormat();
                        break;
                    case TableInfo.ColInfo.ColType.dateTime:
                        m_dataGridView.Columns[i].DefaultCellStyle.Format = lConfigMng.GetDisplayDateFormat();
                        break;
                }
#if false
                    m_dataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    m_dataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    m_dataGridView.Columns[i].FillWeight = 1;
#endif
            }
            m_dataGridView.Columns[i - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            m_dataGridView.Columns[i - 1].FillWeight = 1;
        }

#if !use_custom_dgv
            private myCustomCtrl m_customCtrl;
            private void M_dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
            {
                Debug.WriteLine("OnCellEndEdit");
                hideCustomCtrl();
            }
            private void M_dataGridView_Scroll(object sender, ScrollEventArgs e)
            {
                if (m_customCtrl != null)
                {
                    m_customCtrl.reLocation();
                }
            }
            private void M_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
            {
                Debug.WriteLine("OnCellClick");
                showCustomCtrl(e.ColumnIndex, e.RowIndex);
            }
            private void showCustomCtrl(int col, int row)
            {
                Debug.WriteLine("showDtp");
                if (m_tblInfo.m_cols[col].m_type == lTableInfo.lColInfo.lColType.dateTime)
                {
                    m_customCtrl = new myDateTimePicker(m_dataGridView);
                }
                else if (m_tblInfo.m_cols[col].m_lookupData != null)
                {
                    m_customCtrl = new myComboBox(m_dataGridView, m_tblInfo.m_cols[col].m_lookupData.m_dataSource);
                }
                if (m_customCtrl != null)
                {
                    m_customCtrl.m_iRow = row;
                    m_customCtrl.m_iCol = col;
                    m_dataGridView.Controls.Add(m_customCtrl.getControl());
                    m_customCtrl.setValue(m_dataGridView.CurrentCell.Value.ToString());
                    Rectangle rec = m_dataGridView.GetCellDisplayRectangle(col, row, true);
                    m_customCtrl.show(rec);

                    //ActiveControl = m_dtp;
                    m_dataGridView.BeginEdit(true);
                }
            }
            private void hideCustomCtrl()
            {
                if (m_customCtrl != null)
                {
                    Debug.WriteLine("hideDtp");
                    m_customCtrl.hide();

                    if (m_customCtrl.isChanged())
                    {
                        m_dataGridView.CurrentCell.Value = m_customCtrl.getValue();
                    }

                    m_dataGridView.Controls.Remove(m_customCtrl.getControl());
                    m_customCtrl = null;
                }
            }
#endif
#if !use_cmd_params
            public void search(string where)
            {
                //m_dataContent.GetData(qry);
                m_dataContent.Search(where);
                update();
            }
#endif
#if use_cmd_params
        public void search(List<string> exprs, List<SearchParam> srchParams)
        {
#if use_bg_work
            //move code to lower - search panel
#else
            m_stsMng.onTaskBegin("Searching");
            m_dataContent.Search(exprs, srchParams);
#endif
            //update();
        }
#endif
            #region status_txt
            delegate void callBack_z(string txt);
        class statusMng {
            public DateTime m_startTime;
            public string m_stsTxt;
            private bool m_isEnable;
            private callBack_z m_udpateStsCb;
            public statusMng(callBack_z cb_z)
            {
                m_udpateStsCb = cb_z;
                m_isEnable = false;
            }
            public void onTaskBegin(string txt)
            {
                m_isEnable = true;
                m_stsTxt = txt;
                m_startTime = DateTime.Now;
                m_udpateStsCb(txt);
            }
            public void onTaskEnd(DateTime endTime)
            {
                if (!m_isEnable) return;

                string preTxt = m_stsTxt;
                var elapsed = endTime - m_startTime;
                m_stsTxt = string.Format("{0} completed in {1:0.00} s", preTxt, elapsed.TotalSeconds);
                m_udpateStsCb(m_stsTxt);
                m_isEnable = false;
            }
        }
        statusMng m_stsMng;
        private void UpdateStsTxt(string txt)
        {
            m_status.Text = txt;
        }
        #endregion  //status_txt

#if use_bg_work
        myWorker m_wkr;
#endif

        private void reloadButton_Click(object sender, System.EventArgs e)
        {
#if use_bg_work
            m_dataContent.Reload();
            m_wkr.qryFgTask(new FgTask() {
                receiver = "F1," + m_tblName,
                sender = "DP," + m_tblName,
                eType = FgTask.fgTaskType.F1_FG_UPDATESTS,
                data = "Reloading Completed!"
            }, true);
#else
            m_stsMng.onTaskBegin("Reloading");
            m_dataContent.Reload();
#endif
            //update();
            //m_status.Text = "Reloading completed!";
        }
        private void submitButton_Click(object sender, System.EventArgs e)
        {
#if use_bg_work
            m_dataContent.Submit();
            m_wkr.qryFgTask(new FgTask()
            {
                receiver = "F1," + m_tblName,
                sender = "DP," + m_tblName,
                eType = FgTask.fgTaskType.F1_FG_UPDATESTS,
                data = "Submiting Completed!"
            }, true);
#else
            OnSubmit();
#endif
        }

        protected virtual void OnSubmit()
        {
            m_stsMng.onTaskBegin("Saving");
            m_dataContent.Submit();
        }

        public virtual Int64 getSum()
        {
            Int64 sum = 0;
            int iCol = m_tblInfo.getColIndex(m_countOn);
            BindingSource bs = m_dataContent.m_bindingSource;
            DataTable tbl = (DataTable)bs.DataSource;

            foreach (DataRow row in tbl.Rows)
            {
                if (row[iCol] != DBNull.Value) sum += (Int64)row[iCol];
            }
            return sum;
        }
        private void update()
        {
#if !manual_crt_dgv_columns
            if (m_dataGridView.AutoGenerateColumns == true)
            {
                updateCols();
                m_dataGridView.AutoGenerateColumns = false;
            }
#endif
            //fix col["ID"] not hide
            if (m_dataGridView.Columns[0].Visible)
            {
                m_dataGridView.Columns[0].Visible = false;
            }

            //update sum
            updateSumCtrl();
        }

        public virtual void LoadData()
        {
            //m_tblInfo = s_config.getTable(m_tblName);
            m_tblInfo.LoadData();

#if manual_crt_dgv_columns
                m_dataGridView.AutoGenerateColumns = false;
                lConfigMng.CrtColumns(m_dataGridView,m_tblInfo);
#endif
            m_dataContent = appConfig.s_contentProvider.CreateDataContent(m_tblInfo.m_tblName);
#if !use_bg_work
            m_dataContent.FillTableCompleted += M_dataContent_FillTableCompleted;
            m_dataContent.UpdateTableCompleted += M_dataContent_FillTableCompleted;
#endif
#if !init_datatable_cols
                m_dataContent.Load();
#endif

            m_dataGridView.DataSource = m_dataContent.m_bindingSource;
            DataTable tbl = (DataTable)m_dataContent.m_bindingSource.DataSource;
            if (tbl != null)
            {
                update();
            }
            else
            {
                Debug.Assert(false, "tbl not created!");
            }

#if use_bg_work
            m_wkr = myWorker.getWorker();
            m_wkr.BgProcess += M_wkr_BgProcess;
            m_wkr.FgProcess += M_wkr_FgProcess;
#endif
        }
#if use_bg_work
        private void M_wkr_FgProcess(object sender, myTask e)
        {
            FgTask t = (FgTask)e;
            if (t == null) return;
            if (t.receiver != ("DP," + m_tblName)) return;

            OnFgProccess(t);
        }
        protected virtual void OnFgProccess(FgTask t)
        {
            switch (t.eType)
            {
                case FgTask.fgTaskType.DP_FG_UPDATESTS:
                    var updtsk = (updateStsTsk)t.data;
                    m_status.Text = (string)updtsk.m_txt;
                    break;
                case FgTask.fgTaskType.DP_FG_SEARCH:
                    var tsk = (srchTsk)t.data;
                    m_dataContent.Search(tsk.m_exprs, tsk.m_srchParams);
                    update();
                    break;
                case FgTask.fgTaskType.DP_FG_UPDATESUM:
                    update();
                    break;
            }
        }

        private void M_wkr_BgProcess(object sender, myTask e)
        {
            BgTask t = (BgTask)e;
            if (t == null) return;
            if (t.receiver != ("DP," + m_tblName)) return;

            OnBgProccess(t);
        }
        protected virtual void OnBgProccess(BgTask t)
        {
            switch (t.eType)
            {
                case BgTask.bgTaskType.DP_BG_SEARCH:
                    var tsk = (srchTsk)t.data;
                    m_dataContent.Search(tsk.m_exprs, tsk.m_srchParams);
                    break;
            }
        }
#endif

        private void M_dataContent_FillTableCompleted(object sender, DataContent.FillTableCompletedEventArgs e)
        {
            update();
            m_stsMng.onTaskEnd(e.TimeComplete);
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
        ~lDataPanel()
        {
#if !use_bg_work
            m_dataContent.FillTableCompleted -= M_dataContent_FillTableCompleted;
            m_dataContent.UpdateTableCompleted -= M_dataContent_FillTableCompleted;
#endif
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)  
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                m_reloadPanel.Dispose();
                m_sumPanel.Dispose();

                m_reloadBtn.Dispose();
                m_submitBtn.Dispose();
                m_status.Dispose();
                m_sumLabel.Dispose();
                m_sumTxt.Dispose();

                m_dataGridView.Dispose();

                appConfig.s_contentProvider.ReleaseDataContent(m_tblName);
            }
            // free native resources if there are any.  
        }
#endregion
    }

    [DataContract(Name = "InterPaymentDataPanel")]
    public class lInterPaymentDataPanel : lDataPanel
    {
        public lInterPaymentDataPanel()
        {
            m_tblName = "internal_payment";
            m_countOn = "actually_spent";
        }
    }

    [DataContract(Name = "ReceiptsDataPanel")]
    public class lReceiptsDataPanel : lDataPanel
    {
        public lReceiptsDataPanel()
        {
            m_tblName = "receipts";
            m_countOn = "amount";
        }
    }

    [DataContract(Name = "ExterPaymentDataPanel")]
    public class lExternalPaymentDataPanel : lDataPanel
    {
        public lExternalPaymentDataPanel()
        {
            m_tblName = "external_payment";
            m_countOn = "spent";
        }
    }

    [DataContract(Name = "SalaryDataPanel")]
    public class lSalaryDataPanel : lDataPanel
    {
        public lSalaryDataPanel()
        {
            m_tblName = "salary";
            m_countOn = "salary";
        }
    }

    [DataContract(Name = "AdvanceDataPanel")]
    public class lAdvanceDataPanel : lDataPanel
    {
        public lAdvanceDataPanel()
        {
            m_tblName = "advance";
            m_countOn = "actually_spent";
        }
#if false
        protected override void updateSumCtrl()
        {
            BindingSource bs = m_dataContent.m_bindingSource;
            DataTable tbl = (DataTable)bs.DataSource;

            Int64 adv = 0, act = 0;
            int iAdv = m_tblInfo.getColIndex("advance_payment");
            int iAct = m_tblInfo.getColIndex("actually_spent");
            foreach (DataRow row in tbl.Rows)
            {
                var col1 = row[iAdv];
                var col2 = row[iAct];
                if (col1 != DBNull.Value) { adv += (Int64)col1; }
                if (col2 != DBNull.Value) { act += (Int64)col2; }
            }
            string txt = string.Format("Sum = {4}",
                m_tblInfo.m_cols[iAdv].m_alias,
                m_tblInfo.m_cols[iAct].m_alias,
                adv.ToString(lConfigMng.getCurrencyFormat()),
                act.ToString(lConfigMng.getCurrencyFormat()),
                (adv + act).ToString(lConfigMng.getCurrencyFormat()));
            m_sumLabel.Text = txt;
        }
#endif
    }


    public class lGroupNameDataPanel : lDataPanel
    {
        public lGroupNameDataPanel()
        {
            m_tblName = "group_name";
        }
        public override Int64 getSum()
        {
            BindingSource bs = (BindingSource)m_dataGridView.DataSource;
            DataTable tbl = (DataTable)bs.DataSource;
            return tbl.Rows.Count;
        }
    }

    public class lTopicDataPanel : lDataPanel
    {
        public lTopicDataPanel()
        {
            m_tblName = "topic";
        }
        public override Int64 getSum()
        {
            BindingSource bs = (BindingSource)m_dataGridView.DataSource;
            DataTable tbl = (DataTable)bs.DataSource;
            return tbl.Rows.Count;
        }
    }

    public class lReceiptsContentDataPanel : lGroupNameDataPanel
    {
        public lReceiptsContentDataPanel()
        {
            m_tblName = "receipts_content";
        }
    }
    public class lBuildingDataPanel : lGroupNameDataPanel
    {
        public lBuildingDataPanel()
        {
            m_tblName = "building";
        }
    }
    public class lConstrorgDataPanel : lGroupNameDataPanel
    {
        public lConstrorgDataPanel()
        {
            m_tblName = "constr_org";
        }
    }

    [DataContract(Name = "lTaskDataPanel")]
    public class TaskDataPanel : lDataPanel
    {
        public TaskDataPanel()
        {
            m_tblName = TableIdx.Task.ToDesc();
        }
        public override Int64 getSum()
        {
            BindingSource bs = (BindingSource)m_dataGridView.DataSource;
            DataTable tbl = (DataTable)bs.DataSource;
            return tbl.Rows.Count;
        }
        public override void InitCtrls()
        {
            base.InitCtrls();
            m_dataGridView.AllowUserToAddRows = false;
        }

        private void M_dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //data table row state is unchanged
        }

        private void M_dataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            //call dgv.CommitEdit() to save edited value to cur cell
        }

        protected override void OnSubmit()
        {
            base.OnSubmit();
            
            //release resource if task is complete
            foreach (DataGridViewRow row in m_dataGridView.Rows)
            {
                int sts = int.Parse( row.Cells[TaskTblInfo.ColIdx.Stat.ToField()].Value.ToString());
                if (sts == (int)TaskStatus.Done)
                {
                    string taskId = row.Cells[TaskTblInfo.ColIdx.Task.ToField()].Value.ToString();
                    FreeResByTask(taskId);
                }
            }
        }

        private void FreeResByTask(string taskId)
        {
            //get all order
            var orderSB = new SearchBuilder(appConfig.s_config.GetTable(TableIdx.Order));
            orderSB.Clear();
            orderSB.Add(OrderTblInfo.ColIdx.Task.ToField(), taskId);
            orderSB.Search();

            var cnn = appConfig.s_contentProvider.GetCnn();
            int ret;
            foreach (DataRow row in orderSB.dc.m_dataTable.Rows)
            {
                var orderType = int.Parse(row[OrderTblInfo.ColIdx.Type.ToField()].ToString());
                FreeResByOrder(cnn, taskId, (OrderType)orderType);
            }

            //delete order
            var delOrderQry = string.Format("delete from {0} where {1}=@task_number", 
            TableIdx.Order.ToDesc(), TaskTblInfo.ColIdx.Task.ToField());
            var delOrderCmd = new SQLiteCommand(delOrderQry, (SQLiteConnection)cnn);
            delOrderCmd.Parameters.Add(new SQLiteParameter("@task_number", taskId));
            ret = delOrderCmd.ExecuteNonQuery();
            Debug.WriteLine(string.Format("remove {0} orders", ret));
        }

        private void FreeResByOrder(object cnn, string taskId, OrderType orderType)
        {
            string resTbl=null;
            string stsCol=null;
            string resCol=null;
            string refCol=null;
            string mapTbl=null;
            string taskCol=null;
            bool bExec = true;
            switch (orderType)
            {
                case OrderType.Worker:
                    //update human set status = 0 
                    //where human_number in (select human_number from order_human where task_number = 'CV2019004')
                    resTbl = TableIdx.Human.ToDesc();
                    stsCol = HumanTblInfo.ColIdx.Busy.ToField();
                    resCol = HumanTblInfo.ColIdx.Human.ToField();
                    refCol = OrderHumanTblInfo.ColIdx.Human.ToField();
                    mapTbl = TableIdx.HumanOR.ToDesc();
                    taskCol = OrderHumanTblInfo.ColIdx.Task.ToField();
                    break;
                case OrderType.Car:
                    resTbl = TableIdx.Car.ToDesc();
                    stsCol = CarTblInfo.ColIdx.Used.ToField();
                    resCol = CarTblInfo.ColIdx.Car.ToField();
                    refCol = OrderCarTblInfo.ColIdx.Car.ToField();
                    mapTbl = TableIdx.CarOR.ToDesc();
                    taskCol = OrderCarTblInfo.ColIdx.Task.ToField();
                    break;
                case OrderType.Equip:
                    resTbl = TableIdx.Equip.ToDesc();
                    stsCol = EquipmentTblInfo.ColIdx.Used.ToField();
                    resCol = EquipmentTblInfo.ColIdx.Eqpt.ToField();
                    refCol = OrderEquipmentTblInfo.ColIdx.Equip.ToField();
                    mapTbl = TableIdx.EquipOR.ToDesc();
                    taskCol = OrderEquipmentTblInfo.ColIdx.Task.ToField();
                    break;
                default:
                    bExec = false;
                    break;
            }
            if (bExec)
            {
                string qry = string.Format("update {0} set {1}=@status where "
                + " {2} in (select {3} from {4} where {5} = @task_number)",
                resTbl, stsCol,
                resCol, refCol,
                mapTbl, taskCol);
                var sqlcmd = new SQLiteCommand(qry, (SQLiteConnection)cnn);
                int iStat = (int)ResStatus.Free;
                sqlcmd.Parameters.Add(new SQLiteParameter("@status", iStat.ToString()));
                sqlcmd.Parameters.Add(new SQLiteParameter("@task_number", taskId));
                int ret = sqlcmd.ExecuteNonQuery();
                Debug.WriteLine(string.Format("free {0} resources", ret));

                //delete order - res
                var delQry = string.Format("delete from {0} where {1}=@task_number",
                    mapTbl, taskCol);
                var delCmd = new SQLiteCommand(delQry, (SQLiteConnection)cnn);
                delCmd.Parameters.Add(new SQLiteParameter("@task_number", taskId));
                ret = delCmd.ExecuteNonQuery();
                Debug.WriteLine(string.Format("remove {0} order-res records", ret));
            }
        }
    }

    [DataContract(Name = "lOrderDataPanel")]
    public class OrderDataPanel : TaskDataPanel
    {
        public OrderDataPanel()
        {
            m_tblName = TableIdx.Order.ToDesc();
        }
    }

    [DataContract(Name = "lHumanDataPanel")]
    public class HumanDataPanel : lDataPanel
    {
        public HumanDataPanel()
        {
            m_tblName = TableIdx.Human.ToDesc();
        }
        public override Int64 getSum()
        {
            BindingSource bs = (BindingSource)m_dataGridView.DataSource;
            DataTable tbl = (DataTable)bs.DataSource;
            return tbl.Rows.Count;
        }
    }
    [DataContract(Name = "lEquipmentDataPanel")]
    public class EquipmentDataPanel : HumanDataPanel
    {
        public EquipmentDataPanel()
        {
            m_tblName = TableIdx.Equip.ToDesc();
        }
    }

    [DataContract(Name = "lLectureDataPanel")]
    public class LectureDataPanel : lDataPanel
    {
        public LectureDataPanel()
        {
            m_tblName = TableIdx.Lecture.ToDesc();
        }
        public override Int64 getSum()
        {
            BindingSource bs = (BindingSource)m_dataGridView.DataSource;
            DataTable tbl = (DataTable)bs.DataSource;
            return tbl.Rows.Count;
        }
        public override void InitCtrls()
        {
            base.InitCtrls();
            m_dataGridView.AllowUserToAddRows = false;
        }
    }
}