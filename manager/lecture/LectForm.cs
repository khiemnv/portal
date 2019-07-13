#define use_sqlite

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_binding.lecture
{
    public partial class LectForm : Form
    {
        //public static lContentProvider s_contentProvider;
        private lDbSchema m_dbSchema
        {
            get { return appConfig.s_config.m_dbSchema; }
        }
        List<lBasePanel> m_panels;

        private TabControl m_tabCtrl;
        public LectForm()
        {
            InitializeComponent();

            bool bSaveCfg = false;
            if (appConfig.s_config.m_dbSchema == null)
            {
#if use_sqlite
                appConfig.s_config.m_dbSchema = new lSQLiteDbSchema();
#else
                appConfig.s_config.m_dbSchema = new lSqlDbSchema();
#endif  //use_sqlite
                bSaveCfg = true;
            }
            m_panels = new List<lBasePanel> {
                new LecturePanel(),
            };
            if (bSaveCfg)
            {
#if save_config
                appConfig.s_config.UpdateConfig();
#endif
            }

            //init content provider
            appConfig.s_contentProvider = lSQLiteContentProvider.CrtInstance(this);

            //menu
            var mn = crtMenu();

            //tab control
            m_tabCtrl = new TabControl();
            m_tabCtrl.Dock = DockStyle.Fill;

            foreach (lBasePanel panel in m_panels)
            {
                panel.Restore();
                TabPage newTab = crtTab(panel);
                m_tabCtrl.TabPages.Add(newTab);
            }
            m_tabCtrl.SelectedIndex = 0;

            Load += new EventHandler(LectLoad);

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
            tmpLbl.Text = "© 2019 BAN TRI KHÁCH CHÙA BA VÀNG";
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

            Controls.Add(tmpTbl);
        }

        private void LectLoad(object sender, EventArgs e)
        {
            foreach (lBasePanel panel in m_panels)
            {
                panel.LoadData();
            }
        }

        private object crtMenu()
        {
#if use_menuitem
            MainMenu mainMenu = new MainMenu();
            this.Menu = mainMenu;
#else
            var mainMenu = lConfigMng.CrtMenuStrip();
#endif
            //File
            //  Close
            var miFile = crtMenuItem("&File");
            var miClose = crtMenuItem("&Close");
            addChild(miFile, miClose);
            miClose.Click += MiClose_Click;

            //Help
            //  About
            var miHelp = crtMenuItem("&Help");
            var miAbout = crtMenuItem("&About");
            addChild(miHelp, miAbout);
            miAbout.Click += MiAbout_Click;

            //Edit
            //  GroupName
            //  ReceiptsContent
            //  Building
            var miEdit = crtMenuItem("&Edit");

            var miEditTopic = crtMenuItem("Chủ đề");
            miEditTopic.Click += miEditTopic_Click;
            addChild(miEdit, miEditTopic);

            //Input
            var miInput = crtMenuItem("Input");
            var miLect = crtMenuItem("Bài Giảng");
            miLect.Click += miLect_Click;
            addChild(miInput, miLect);

#if use_menuitem
            mainMenu.MenuItems.AddRange(new MenuItem[] { miFile, miEdit, miReport, miConfig, miHelp });
            this.Menu = mainMenu;
            return null;
#else
            mainMenu.Items.AddRange(new ToolStripMenuItem[] { miFile, miInput, miEdit, miHelp });
            return mainMenu;
#endif
        }
        private TabPage crtTab(lBasePanel newPanel)
        {
            TabPage newTabPage = new TabPage();
            newPanel.initCtrls();
            newTabPage.Controls.Add(newPanel.m_panel);
            newTabPage.Text = newPanel.m_tblInfo.m_tblAlias;
            return newTabPage;
        }

        private ToolStripMenuItem crtMenuItem(string text)
        {
            //ToolStripMenuItem mi = new ToolStripMenuItem();
            var mi = lConfigMng.CrtStripMI();
            mi.Text = text;
            return mi;
        }
        private void addChild(ToolStripMenuItem parent, ToolStripMenuItem child)
        {
            parent.DropDownItems.Add(child);
        }
        private void MiClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void MiAbout_Click(object sender, EventArgs e)
        {
            lAboutDlg aboutDlg = new lAboutDlg();
            DialogResult ret = aboutDlg.ShowDialog();
            aboutDlg.Dispose();
        }
        private void miEditTopic_Click(object sender, EventArgs e)
        {
            lEditDlg edtDlg = new lTopicEditDlg();
            edtDlg.ShowDialog();
            edtDlg.Dispose();
        }
        private void miLect_Click(object sender, EventArgs e)
        {
            //load input
            openInputForm(inputFormType.lectIF);
        }
        enum inputFormType
        {
            lectIF,
        }
        private void openInputForm(inputFormType type)
        {
            inputF inputDlg = null;
            switch (type)
            {
                case inputFormType.lectIF:
                    inputDlg = new lLectInputF();
                    break;
            }

#if fullscreen_onload
            inputDlg.WindowState = FormWindowState.Maximized;
#endif
            //chk error
            if (inputDlg != null) { inputDlg.ShowDialog(); }
        }
    }
}
