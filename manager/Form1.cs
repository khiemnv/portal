//#define DEBUG_DRAWING
#define use_sqlite
#define tab_header_blue
//#define use_bg_work
#if !DEBUG
#define chck_pass
#define save_config
#else   //DEBUG
#define load_input
#endif  //DEBUG

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;

namespace test_binding
{
    public partial class Form1 : Form
    {

        private lDbSchema m_dbSchema
        {
            get { return appConfig.s_config.m_dbSchema; }
        }
        List<lBasePanel> m_panels
        {
            get { return appConfig.s_config.m_panels; }
        }

        private TabControl m_tabCtrl;

#if use_bg_work
        //bg process
        public myWorker m_bgwork;
#endif

        public Form1()
        {
            //this.Font = lConfigMng.getFont();

            InitializeComponent();

            //init config & load config
            appConfig.s_config = lConfigMng.crtInstance();
            if (appConfig.s_config.m_dbSchema == null)
            {
#if use_sqlite
                appConfig.s_config.m_dbSchema = new lSQLiteDbSchema();
#else
                appConfig.s_config.m_dbSchema = new lSqlDbSchema();
#endif  //use_sqlite
                appConfig.s_config.m_panels = new List<lBasePanel> {
                    //new lReceiptsPanel(),
                    //new lInterPaymentPanel(),
                    //new lExternalPaymentPanel(),
                    //new lSalaryPanel(),
                    new lTaskPanel(),
                    new lOrderPanel(),
                    new lHumanPanel(),
                    new lEquipmentPanel(),
                };
#if save_config
                appConfig.s_config.UpdateConfig();
#endif
            }

            //init content provider
#if use_sqlite
            appConfig.s_contentProvider = lSQLiteContentProvider.getInstance(this);
#else
            appConfig.s_contentProvider = lSqlContentProvider.getInstance(this);
#endif  //use_sqlite

            //menu
            var mn = crtMenu();

            //tab control
            m_tabCtrl = new TabControl();
            //Controls.Add(m_tabCtrl);
            //m_tabCtrl.Anchor = AnchorStyles.Top | AnchorStyles.Left |AnchorStyles.Right|AnchorStyles.Bottom;
            m_tabCtrl.Dock = DockStyle.Fill;

#if crtnew_panel
            List<lBasePanel> newPanels = new List<lBasePanel>();
            foreach(lBasePanel panel in m_panels)
            {
                lBasePanel newPanel = lBasePanel.crtPanel(panel);
                newPanels.Add(newPanel);
                TabPage newTab = crtTab(newPanel);
                m_tabCtrl.TabPages.Add(newTab);
            }
            m_panels = newPanels;
#else
            foreach (lBasePanel panel in m_panels)
            {
                panel.Restore();
                TabPage newTab = crtTab(panel);
                m_tabCtrl.TabPages.Add(newTab);
            }
#endif  //crtnew_panel

            m_tabCtrl.SelectedIndex = 0;

            Load += new System.EventHandler(Form1_Load);

#if tab_header_blue
            //set tab header blue
            m_tabCtrl.DrawMode = TabDrawMode.OwnerDrawFixed;
            m_tabCtrl.DrawItem += tabControl1_DrawItem;
#endif

            //set font
            //this.Font = lConfigMng.getFont();
            m_tabCtrl.Font = lConfigMng.getFont();

            Label tmpLbl = new Label();
            //tmpLbl.BorderStyle = BorderStyle.FixedSingle;
            tmpLbl.Anchor = AnchorStyles.Right;
            tmpLbl.Text = "© 2017 BAN TRI KHÁCH CHÙA BA VÀNG";
            tmpLbl.AutoSize = true;
            tmpLbl.BackColor = Color.Transparent;

            TableLayoutPanel tmpTbl = new TableLayoutPanel();
            tmpTbl.Dock = DockStyle.Fill;
            tmpTbl.RowCount = 3;
            tmpTbl.RowStyles.Add(new RowStyle());
            tmpTbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tmpTbl.RowStyles.Add(new RowStyle());

            if (mn != null)
            {
                tmpTbl.Controls.Add((MenuStrip)mn, 0, 0);
            }

            tmpTbl.Controls.Add(tmpLbl, 1, 0);
            tmpTbl.Controls.Add(m_tabCtrl, 0, 1);
            tmpTbl.SetColumnSpan(m_tabCtrl, 2);

#if use_bg_work
            sts = new StatusStrip();
            sts.Dock = DockStyle.Bottom;
            stsLbl = new ToolStripStatusLabel();
            stsLbl.Dock = DockStyle.Left;
            prg = new ToolStripProgressBar();
            prg.Dock = DockStyle.Right;
            prg.Maximum = 100;
            prg.Visible = false;
            sts.Items.AddRange(new ToolStripItem[] { stsLbl, prg });

            tmpTbl.Controls.Add(sts, 0, 2);
            tmpTbl.SetColumnSpan(sts, 2);
#endif

            Controls.Add(tmpTbl);

#if use_bg_work
            //background work
            m_bgwork = myWorker.getWorker();
            m_bgwork.BgProcess += bg_process;
            m_bgwork.FgProcess += fg_process;
#endif
        }

#if use_bg_work
        StatusStrip sts;
        ToolStripStatusLabel stsLbl;
        ToolStripProgressBar prg;

        private void bg_process(object sender, myTask e)
        {
            var t = e as BgTask;
            if (t == null) return;
            Debug.WriteLine(string.Format("F1 bg_process {0}", t.eType.ToString()));
            if (!t.receiver.Contains("F1,")) return;

            switch (t.eType)
            {
                case BgTask.bgTaskType.bgExec:
                    var cb = (taskCallback0)e.data;
                    cb.Invoke();
                    break;
            }
        }

        private void fg_process(object sender, myTask e)
        {
            var t = e as FgTask;
            if (t == null) return;
            Debug.WriteLine(string.Format("F1 fg_process {0}", t.eType.ToString()));
            if (!t.receiver.Contains("F1,")) return;

            switch (t.eType)
            {
                case FgTask.fgTaskType.fgExec:
                    Invoke((taskCallback0)e.data);
                    break;
                case FgTask.fgTaskType.F1_FG_UPDATESTS:
                    stsLbl.Text = t.data.ToString();
                    break;
                case FgTask.fgTaskType.F1_FG_UPDATEPRG:
                    prg.Value = t.percent;
                    break;
            }
        }
#endif

#if tab_header_blue
        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            //e.DrawBackground();
            Color cl = Color.Transparent;
            var brush = Brushes.Black;
            //TabColors[tabControl1.TabPages[e.Index]])
            TabControl tabControl1 = m_tabCtrl;
            if (e.Index == tabControl1.SelectedIndex)
            {
                cl = Color.Blue;
                brush = Brushes.White;
            }
            using (Brush br = new SolidBrush(cl))
            {
                e.Graphics.FillRectangle(br, e.Bounds);
                SizeF sz = e.Graphics.MeasureString(tabControl1.TabPages[e.Index].Text, e.Font);
                e.Graphics.DrawString(tabControl1.TabPages[e.Index].Text, e.Font, brush, e.Bounds.Left + (e.Bounds.Width - sz.Width) / 2, e.Bounds.Top + (e.Bounds.Height - sz.Height) / 2 + 1);

                Rectangle rect = e.Bounds;
                rect.Offset(0, 1);
                rect.Inflate(0, -1);
                e.Graphics.DrawRectangle(Pens.DarkGray, rect);
                e.DrawFocusRectangle();
            }
        }
#endif //tab_header_blue

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (lBasePanel panel in m_panels)
            {
                panel.LoadData();
            }

#if chck_pass
            //get passwd
            string zMd5 = getPasswd();

            if (zMd5 == "")
            {
                //user PKTChuaBaVang passwd PKT310118
            }
            else if (appConfig.s_config.m_md5 == "")
            {
                //passwd is reseted
                appConfig.s_config.m_md5 = zMd5;
                appConfig.s_config.UpdateConfig();
            }
            else if (appConfig.s_config.m_md5 != zMd5)
            {
                //not match with existing passwd
                this.Close();
            }
#endif

#if load_input
            //load input
            //openInputForm(inputFormType.receiptIF);
            //MiReport_Click(this, null);
#endif  //load_input
        }
        enum inputFormType
        {
            receiptIF,
            exterPayIF,
            interPayIF,
            salaryIF,
            advanceIF,
            taskIF,
            orderIF,
            approveIF
        }
        private void openInputForm(inputFormType type)
        {
            inputF inputDlg = null;
            switch (type)
            {
                //case inputFormType.receiptIF:
                //    inputDlg = new lReceiptsInputF();
                //    break;
                //case inputFormType.exterPayIF:
                //    inputDlg = new lExterPayInputF();
                //    break;
                //case inputFormType.interPayIF:
                //    inputDlg = new lInterPayInputF();
                //    break;
                //case inputFormType.salaryIF:
                //    inputDlg = new lSalaryInputF();
                //    break;
                //case inputFormType.advanceIF:
                //    inputDlg = new lAdvanceInputF();
                //    break;
                case inputFormType.taskIF:
                    inputDlg = new lTaskInputF();
                    break;
                case inputFormType.orderIF:
                    //not open order when task DGV empty
                    if (chkTaskDGV())
                    {
                        inputDlg = new lOrderInputF();
                    }
                    else
                    {
                        lConfigMng.ShowInputError("Không có CV nào trong bảng");
                    }
                    break;
                case inputFormType.approveIF:
                    //not open order when task DGV empty
                    if (chkTaskDGV())
                        inputDlg = new lApproveInputF();
                    else
                        lConfigMng.ShowInputError("Không có CV nào trong bảng");
                    break;
            }

#if fullscreen_onload
            inputDlg.WindowState = FormWindowState.Maximized;
#endif
            //chk error
            if (inputDlg != null) { inputDlg.ShowDialog(); }
        }

        private bool chkTaskDGV()
        {
            DataContent taskDC = appConfig.s_contentProvider.CreateDataContent("task");
            bool bChk = taskDC.m_dataTable.Rows.Count > 0;
            return bChk;
        }

        private TabPage crtTab(lBasePanel newPanel)
        {
            TabPage newTabPage = new TabPage();
            newPanel.initCtrls();
            newTabPage.Controls.Add(newPanel.m_panel);
            newTabPage.Text = newPanel.m_tblInfo.m_tblAlias;
            return newTabPage;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
        }

        string getPasswd()
        {
            var passwdDlg = new lPasswdDlg();
            passwdDlg.ShowDialog();
            string md5 = passwdDlg.m_md5;
            passwdDlg.Dispose();
            return md5;
        }
    }

    [DataContract(Name = "Panel")]
    public class lBasePanel : IDisposable
    {
        public TableInfo m_tblInfo { get { return m_dataPanel.m_tblInfo; } }
        //public lDataContent m_data;
        [DataMember(Name = "dataPanel")]
        public lDataPanel m_dataPanel;
        [DataMember(Name = "searchPanel")]
        public SearchPanel m_searchPanel;
        [DataMember(Name = "report")]
        public lBaseReport m_report;

        public TableLayoutPanel m_panel;
        public Button m_printBtn;

        protected lBasePanel() { }
        public static lBasePanel crtPanel(lBasePanel panel)
        {
            lDataPanel dataPanel = lDataPanel.crtDataPanel(panel.m_dataPanel);
            SearchPanel searchPanel = SearchPanel.crtSearchPanel(dataPanel, panel.m_searchPanel.m_searchCtrls);
            lBaseReport report = lBaseReport.crtReport(panel.m_report);
            lBasePanel newPanel = new lBasePanel()
            {
                m_dataPanel = dataPanel,
                m_searchPanel = searchPanel,
                m_report = report
            };
            newPanel.init();
            return newPanel;
        }

        public void Restore()
        {
            m_searchPanel.m_dataPanel = m_dataPanel;
        }

        protected void init()
        {
        }

        protected virtual void OnPrint()
        {
            if (m_report != null)
            {
                var previewDlg = new rptPreview();
                previewDlg.mRpt = m_report;
                previewDlg.ShowDialog();
                //rpt.Run();
                m_report.Clean();
                m_report.Dispose();
            }
        }

        private void printBtn_Click(object sender, EventArgs e)
        {
            OnPrint();
        }

        public virtual void initCtrls()
        {
            //create table layout & add controls to
            // +----------------+----------------+
            // |search panel    |          print |
            // |                |                |
            // +----------------+----------------+
            // |reload & save btn         sum    |
            // +----------------+----------------+
            // |data grid view                   |
            // |                                 |
            // +----------------+----------------+
            m_panel = new TableLayoutPanel();
            m_panel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            m_panel.Dock = DockStyle.Fill;
            m_panel.ColumnCount = 2;
            m_panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
#if DEBUG_DRAWING
                m_panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
#endif
            m_printBtn = lConfigMng.crtButton();
            m_printBtn.Text = "Print";
            m_printBtn.Click += new System.EventHandler(printBtn_Click);

            //add search panel to table layout
            m_searchPanel.initCtrls();
            m_panel.Controls.Add(m_searchPanel.m_tblPanel, 0, 0);

            //add print btn to table layout
            m_printBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            m_panel.Controls.Add(m_printBtn, 1, 0);

            //add data panel ctrls to table layout
            m_dataPanel.initCtrls();
            //reload tbl panel
            TableLayoutPanel reloadTbl = new TableLayoutPanel();
            reloadTbl.AutoSize = true;
            reloadTbl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            reloadTbl.Controls.Add(m_dataPanel.m_reloadPanel, 0, 0);
            m_dataPanel.m_sumPanel.Anchor = AnchorStyles.Right;
            reloadTbl.Controls.Add(m_dataPanel.m_sumPanel, 1, 0);
            m_panel.Controls.Add(reloadTbl, 0, 1);
            m_panel.SetColumnSpan(reloadTbl, 2);
            //data gird view
            m_panel.Controls.Add(m_dataPanel.m_dataGridView, 0, 2);
            m_panel.SetColumnSpan(m_dataPanel.m_dataGridView, 2);
        }

        public virtual void LoadData()
        {
            m_dataPanel.LoadData();
            m_searchPanel.LoadData();
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
        ~lBasePanel()
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
                m_printBtn.Dispose();
                m_panel.Dispose();
                m_dataPanel.Dispose();
                m_searchPanel.Dispose();
                m_report.Dispose();
            }
            // free native resources if there are any. 
        }
#endregion
    }

    [DataContract(Name = "InternalPaymentPanel")]
    public class lInterPaymentPanel : lBasePanel
    {
        rptAssist m_rptAsst;
        public lInterPaymentPanel()
        {
            m_dataPanel = new lInterPaymentDataPanel();
            m_searchPanel = new lInterPaymentSearchPanel(m_dataPanel);
            m_report = new lCurInterPaymentReport();
            base.init();

            m_rptAsst = rptAssist.Create(m_dataPanel.m_tblName);
        }

        protected override void OnPrint()
        {
            //get selected record
            var rows = m_dataPanel.m_dataGridView.SelectedRows;
            if (rows.Count > 0)
            {
                var row = rows[0];
                DataRow dr = ((DataRowView)row.DataBoundItem).Row;
                m_rptAsst.setData(dr);
                var dt = m_rptAsst.getData();

                var rpt = new lBillReport();
                rpt.rptAsst = m_rptAsst;

                var previewDlg = new rptPreview();
                previewDlg.mRpt = rpt;
                previewDlg.ShowDialog();
                rpt.Clean();
                rpt.Dispose();
            }
        }
    }

    [DataContract(Name = "ReceiptsPanel")]
    public class lReceiptsPanel : lBasePanel
    {
        public lReceiptsPanel()
        {
            m_dataPanel = new lReceiptsDataPanel();
            m_searchPanel = new lReceiptsSearchPanel(m_dataPanel);
            m_report = new lCurReceiptsReport();
        }
    }

    [DataContract(Name = "ExternalPaymentPanel")]
    public class lExternalPaymentPanel : lBasePanel
    {
        public lExternalPaymentPanel()
        {
            m_dataPanel = new lExternalPaymentDataPanel();
            m_searchPanel = new lExternalPaymentSearchPanel(m_dataPanel);
            m_report = new lCurExterPaymentReport();
            base.init();
        }
    }

    [DataContract(Name = "SalaryPanel")]
    public class lSalaryPanel : lBasePanel
    {
        public lSalaryPanel()
        {
            m_dataPanel = new lSalaryDataPanel();
            m_searchPanel = new lSalarySearchPanel(m_dataPanel);
            m_report = new lCurSalaryReport();
            base.init();
        }
    }

    [DataContract(Name = "AdvancePanel")]
    public class lAdvancePanel: lBasePanel
    {
        public lAdvancePanel()
        {
            m_dataPanel = new lAdvanceDataPanel();
            m_searchPanel = new lAdvanceSearchPanel(m_dataPanel);
            m_report = new lCurAdvanceReport();
            base.init();
        }
    }

    [DataContract(Name = "TaskPanel")]
    public class lTaskPanel : lBasePanel
    {
        public lTaskPanel()
        {
            m_dataPanel = new lTaskDataPanel();
            m_searchPanel = new lTaskSearchPanel(m_dataPanel);
            m_report = new lTaskReport();
            base.init();
        }
    }

    [DataContract(Name = "OrderPanel")]
    public class lOrderPanel : lBasePanel
    {
        public lOrderPanel()
        {
            m_dataPanel = new lOrderDataPanel();
            m_searchPanel = new OrderSearchPanel(m_dataPanel);
            m_report = new lOrderReport();
            base.init();
        }
    }

    [DataContract(Name = "HumanPanel")]
    public class lHumanPanel : lBasePanel
    {
        public lHumanPanel()
        {
            m_dataPanel = new lHumanDataPanel();
            m_searchPanel = new HumanSearchPanel(m_dataPanel);
            m_report = new lHumanReport();
            base.init();
        }
    }

    [DataContract(Name = "EquipmentPanel")]
    public class lEquipmentPanel : lBasePanel
    {
        public lEquipmentPanel()
        {
            m_dataPanel = new lEquipmentDataPanel();
            m_searchPanel = new EquipmentSearchPanel(m_dataPanel);
            m_report = new lEquipmentReport();
            base.init();
        }
    }
}
