#define use_custom_font
//#define use_menuitem

using System;
using System.Drawing;
using System.Windows.Forms;

namespace test_binding
{
    public partial class Form1 : Form
    {
#if use_menuitem
        private MenuItem crtMenuItem(string text)
        {
#if use_custom_font
            return new myMenuItem(text.Replace("&", ""));
#else
            return new MenuItem(text);
#endif
        }
        private void addChild(MenuItem parent, MenuItem child)
        {
            parent.MenuItems.Add(child);
        }
#else //ToolStripMenuItem
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
#endif  //use_menuitem
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

            var miEditGroupName = crtMenuItem("Danh Sách Các Ban");
            miEditGroupName.Click += MiEditGroupName_Click;
            addChild(miEdit, miEditGroupName);

            //var miEditReceiptsContent = crtMenuItem("Nguồn Thu");
            //miEditReceiptsContent.Click += MiEditReceiptsContent_Click;
            //addChild(miEdit, miEditReceiptsContent);

            //var miBuilding = crtMenuItem("Công Trình");
            //miBuilding.Click += MiBuilding_Click;
            //addChild(miEdit, miBuilding);

            //var miConstrorg = crtMenuItem("Đơn vị TC");
            //miConstrorg.Click += MiConstrorg_Click;
            //addChild(miEdit, miConstrorg);

            //Report
            //var miReport = crtMenuItem("&Report");
            //miReport.Click += MiReport_Click;

            //Config
            var miConfig = crtMenuItem("&Config");
            var miFont = crtMenuItem("&Font");
            miFont.Click += MiFont_Click;
            addChild(miConfig, miFont);

            //Input
            var miInput = crtMenuItem("Input");
            //var miReceipt = crtMenuItem("Phiếu Thu");
            //miReceipt.Click += MiReceipt_Click;
            //addChild(miInput, miReceipt);
            //var miInterpay = crtMenuItem("Phiếu Chi Nội Chúng");
            //miInterpay.Click += miInterpay_Click;
            //addChild(miInput, miInterpay);
            //var miExterpay = crtMenuItem("Phiếu Chi Ngoại Chúng");
            //miExterpay.Click += miExterpay_Click;
            //addChild(miInput, miExterpay);
            //var miSalary = crtMenuItem("Phiếu Chi Lương");
            //miSalary.Click += miSalary_Click;
            //addChild(miInput, miSalary);
            //var miAdvance = crtMenuItem("Phiếu Chi Tạm Ứng");
            //miAdvance.Click += MiAdvance_Click;
            //addChild(miInput, miAdvance);
            var miTask = crtMenuItem("Công Việc");
            miTask.Click += MiTask_Click;
            addChild(miInput, miTask);
            var miOrder = crtMenuItem("Yêu Cầu");
            miOrder.Click += MiOrder_Click;
            addChild(miInput, miOrder);

            var miApporve = crtMenuItem("Phê Duyệt YC");
            miApporve.Click += MiApprove_Click;
            addChild(miInput, miApporve);
#if use_menuitem
            mainMenu.MenuItems.AddRange(new MenuItem[] { miFile, miEdit, miReport, miConfig, miHelp });
            this.Menu = mainMenu;
            return null;
#else
            mainMenu.Items.AddRange(new ToolStripMenuItem[] { miFile, miInput, miEdit, miConfig, miHelp });
            return mainMenu;
#endif
        }

        private void miSalary_Click(object sender, EventArgs e)
        {
            openInputForm(inputFormType.salaryIF);
        }

        private void miExterpay_Click(object sender, EventArgs e)
        {
            openInputForm(inputFormType.exterPayIF);
        }

        private void miInterpay_Click(object sender, EventArgs e)
        {
            openInputForm(inputFormType.interPayIF);
        }

        private void MiReceipt_Click(object sender, EventArgs e)
        {
            //load input
            openInputForm(inputFormType.receiptIF);
        }

        //private void MiAdvance_Click(object sender, EventArgs e)
        //{
        //    openInputForm(inputFormType.advanceIF);
        //}

        private void MiTask_Click(object sender, EventArgs e)
        {
            //load input
            openInputForm(inputFormType.taskIF);
        }

        private void MiOrder_Click(object sender, EventArgs e)
        {
            //load input
            openInputForm(inputFormType.orderIF);
        }

        private void MiApprove_Click(object sender, EventArgs e)
        {
            //load input
            openInputForm(inputFormType.approveIF);
        }

        private void MiFont_Click(object sender, EventArgs e)
        {
            FontDialog fontDialog1 = new FontDialog();

            fontDialog1.Font = lConfigMng.getFont();

            if (fontDialog1.ShowDialog() != DialogResult.Cancel)
            {
                lConfigMng.setFont(fontDialog1.Font);
                string msg = string.Format("Selected font {0} size {1} will be applied after restart application",
                    fontDialog1.Font.Name, fontDialog1.Font.Size);
                MessageBox.Show(msg, "Font changed!",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void MiReport_Click(object sender, EventArgs e)
        {
            lReportDlg dlg = new lReportDlg();
            dlg.ShowDialog();
            dlg.Dispose();
        }

        private void MiConstrorg_Click(object sender, EventArgs e)
        {
            lEditDlg edtDlg = new lConstrorgEditDlg();
            edtDlg.ShowDialog();
            edtDlg.Dispose();
        }

        private void MiBuilding_Click(object sender, EventArgs e)
        {
            lEditDlg edtDlg = new lBuildingEditDlg();
            edtDlg.ShowDialog();
            edtDlg.Dispose();
        }

        private void MiEditReceiptsContent_Click(object sender, EventArgs e)
        {
            lEditDlg edtDlg = new lReceiptsContentEditDlg();
            edtDlg.ShowDialog();
            edtDlg.Dispose();
        }

        private void MiEditGroupName_Click(object sender, EventArgs e)
        {
            lEditDlg edtDlg = new lGroupNameEditDlg();
            edtDlg.ShowDialog();
            edtDlg.Dispose();
        }

        private void MiClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        class lAboutDlg : Form
        {
            public lAboutDlg()
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                //show about dlg
                Form aboutDlg = this;
                aboutDlg.Location = new Point(0, 0);
                aboutDlg.Size = new Size(150, 120);
                aboutDlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                aboutDlg.MinimizeBox = false;
                aboutDlg.MaximizeBox = false;

                TableLayoutPanel tblPanel = new TableLayoutPanel();
                tblPanel.ColumnCount = 1;
                tblPanel.Dock = DockStyle.Fill;

                Label txt = lConfigMng.crtLabel();
                txt.Text = "About this app";
                //txt.AutoSize = true;
                txt.Dock = DockStyle.Fill;
                //txt.Dock = DockStyle.Fill;

                Button btn = lConfigMng.crtButton();
                btn.Text = "OK";
                btn.TextAlign = ContentAlignment.MiddleCenter;
                btn.Anchor = AnchorStyles.None;
                btn.AutoSize = true;
                btn.Click += Btn_Click;
                aboutDlg.AcceptButton = btn;

                tblPanel.Controls.Add(txt, 0, 0);
                tblPanel.Controls.Add(btn, 0, 1);

                aboutDlg.Controls.Add(tblPanel);
            }

            private void Btn_Click(object sender, EventArgs e)
            {
                Close();
            }
        }
        private void MiAbout_Click(object sender, EventArgs e)
        {
#if false
            var ret = MessageBox.Show("About this app");
#else
            lAboutDlg aboutDlg = new lAboutDlg();
            DialogResult ret = aboutDlg.ShowDialog();
            aboutDlg.Dispose();
#endif
        }
    }
}
