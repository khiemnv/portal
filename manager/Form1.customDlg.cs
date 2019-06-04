using System.Windows.Forms;
using System;
using System.Drawing;
//using Microsoft.Reporting.WinForms;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace test_binding
{
    public class lReportDlg : Form
    {
        #region gui
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.paymentRadio = new System.Windows.Forms.RadioButton();
            this.buildingRadio = new System.Windows.Forms.RadioButton();
            this.constrorgRadio = new System.Windows.Forms.RadioButton();
            this.daysumRd = new System.Windows.Forms.RadioButton();
            this.startDate = new System.Windows.Forms.DateTimePicker();
            this.endDate = new System.Windows.Forms.DateTimePicker();
            this.paymentRptType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buildingCmb = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.printBtn = new System.Windows.Forms.Button();
            this.constrorgCmb = new System.Windows.Forms.ComboBox();
            this.rptRcptRad = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // paymentRadio
            // 
            this.paymentRadio.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.paymentRadio.AutoSize = true;
            this.paymentRadio.Checked = true;
            this.paymentRadio.Location = new System.Drawing.Point(8, 36);
            this.paymentRadio.Name = "paymentRadio";
            this.paymentRadio.Size = new System.Drawing.Size(82, 17);
            this.paymentRadio.TabIndex = 0;
            this.paymentRadio.TabStop = true;
            this.paymentRadio.Text = "Báo cáo chi";
            this.paymentRadio.UseVisualStyleBackColor = true;
            // 
            // buildingRadio
            // 
            this.buildingRadio.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buildingRadio.AutoSize = true;
            this.buildingRadio.Location = new System.Drawing.Point(8, 63);
            this.buildingRadio.Name = "buildingRadio";
            this.buildingRadio.Size = new System.Drawing.Size(73, 17);
            this.buildingRadio.TabIndex = 1;
            this.buildingRadio.TabStop = true;
            this.buildingRadio.Text = "Công trình";
            this.buildingRadio.UseVisualStyleBackColor = true;
            // 
            // constrorgRadio
            // 
            this.constrorgRadio.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.constrorgRadio.AutoSize = true;
            this.constrorgRadio.Location = new System.Drawing.Point(8, 90);
            this.constrorgRadio.Name = "constrorgRadio";
            this.constrorgRadio.Size = new System.Drawing.Size(73, 17);
            this.constrorgRadio.TabIndex = 2;
            this.constrorgRadio.TabStop = true;
            this.constrorgRadio.Text = "Đơn vị TC";
            this.constrorgRadio.UseVisualStyleBackColor = true;
            // 
            // daysumRd
            // 
            this.daysumRd.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.daysumRd.AutoSize = true;
            this.daysumRd.Location = new System.Drawing.Point(8, 115);
            this.daysumRd.Name = "daysumRd";
            this.daysumRd.Size = new System.Drawing.Size(58, 17);
            this.daysumRd.TabIndex = 3;
            this.daysumRd.TabStop = true;
            this.daysumRd.Text = "Sổ quỹ";
            this.daysumRd.UseVisualStyleBackColor = true;
            // 
            // startDate
            // 
            this.startDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.startDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.startDate.Location = new System.Drawing.Point(97, 8);
            this.startDate.Name = "startDate";
            this.startDate.Size = new System.Drawing.Size(144, 20);
            this.startDate.TabIndex = 4;
            // 
            // endDate
            // 
            this.endDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.endDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.endDate.Location = new System.Drawing.Point(247, 8);
            this.endDate.Name = "endDate";
            this.endDate.Size = new System.Drawing.Size(145, 20);
            this.endDate.TabIndex = 5;
            // 
            // paymentRptType
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.paymentRptType, 2);
            this.paymentRptType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.paymentRptType.FormattingEnabled = true;
            this.paymentRptType.Location = new System.Drawing.Point(97, 34);
            this.paymentRptType.Name = "paymentRptType";
            this.paymentRptType.Size = new System.Drawing.Size(295, 21);
            this.paymentRptType.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Ngày Tháng";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buildingCmb
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.buildingCmb, 2);
            this.buildingCmb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buildingCmb.FormattingEnabled = true;
            this.buildingCmb.Location = new System.Drawing.Point(97, 61);
            this.buildingCmb.Name = "buildingCmb";
            this.buildingCmb.Size = new System.Drawing.Size(295, 21);
            this.buildingCmb.TabIndex = 10;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.buildingCmb, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.startDate, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.endDate, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.daysumRd, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.paymentRadio, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.constrorgRadio, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.paymentRptType, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.buildingRadio, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.printBtn, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.constrorgCmb, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.rptRcptRad, 0, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(400, 200);
            this.tableLayoutPanel1.TabIndex = 11;
            // 
            // printBtn
            // 
            this.printBtn.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.printBtn.Location = new System.Drawing.Point(282, 153);
            this.printBtn.Name = "printBtn";
            this.printBtn.Size = new System.Drawing.Size(75, 23);
            this.printBtn.TabIndex = 11;
            this.printBtn.Text = "Print";
            this.printBtn.UseVisualStyleBackColor = true;
            // 
            // constrorgCmb
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.constrorgCmb, 2);
            this.constrorgCmb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.constrorgCmb.FormattingEnabled = true;
            this.constrorgCmb.Location = new System.Drawing.Point(97, 88);
            this.constrorgCmb.Name = "constrorgCmb";
            this.constrorgCmb.Size = new System.Drawing.Size(295, 21);
            this.constrorgCmb.TabIndex = 12;
            // 
            // rptRcptRad
            // 
            this.rptRcptRad.AutoSize = true;
            this.rptRcptRad.Location = new System.Drawing.Point(8, 138);
            this.rptRcptRad.Name = "rptRcptRad";
            this.rptRcptRad.Size = new System.Drawing.Size(83, 17);
            this.rptRcptRad.TabIndex = 13;
            this.rptRcptRad.TabStop = true;
            this.rptRcptRad.Text = "Báo cáo thu";
            this.rptRcptRad.UseVisualStyleBackColor = true;
            // 
            // lReportDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 200);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "lReportDlg";
            this.Text = "Report";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton paymentRadio;
        private System.Windows.Forms.RadioButton buildingRadio;
        private System.Windows.Forms.RadioButton constrorgRadio;
        private System.Windows.Forms.RadioButton daysumRd;
        private System.Windows.Forms.DateTimePicker startDate;
        private System.Windows.Forms.DateTimePicker endDate;
        private System.Windows.Forms.ComboBox paymentRptType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox buildingCmb;
        private System.Windows.Forms.Button printBtn;
        #endregion

        private ComboBox constrorgCmb;
        private RadioButton rptRcptRad;

        enum receiptsRptType
        {
            byDays,
            byWeek,
            byMonth,
            byYear,
        }
        Dictionary<receiptsRptType, string> m_receiptRptTypes;

        public lReportDlg()
        {
            InitializeComponent();

            startDate.CustomFormat = lConfigMng.getDisplayDateFormat();
            endDate.CustomFormat = lConfigMng.getDisplayDateFormat();

            m_receiptRptTypes = new Dictionary<receiptsRptType, string> {
                {receiptsRptType.byDays, "Báo cáo theo ngày" },
                {receiptsRptType.byWeek, "Báo cáo theo tuần" },
                {receiptsRptType.byMonth, "Báo cáo theo tháng" },
            };
            foreach (var val in m_receiptRptTypes.Values)
            {
                paymentRptType.Items.Add(val);
            }
            paymentRptType.SelectedIndex = 0;

            printBtn.Click += PrintBtn_Click;
            Load += LReportDlg_Load;

            //set font
            //this.Font = lConfigMng.getFont();
            foreach (Control crt in Controls)
            {
                crt.Font = lConfigMng.getFont();
            }
        }

        private void PrintBtn_Click(object sender, EventArgs e)
        {
            lBaseReport rpt = null;
            if (paymentRadio.Checked)
            {
                switch (paymentRptType.SelectedIndex)
                {
                    case (int)receiptsRptType.byDays:
#if true
                        //rpt = new lDaysReport(startDate.Value, endDate.Value);
                        rpt = new lPaymentDays(startDate.Value, endDate.Value);
#else
                        rpt = new lSqlDaysReport(startDate.Value, endDate.Value);
#endif
                        break;
                    case (int)receiptsRptType.byWeek:
#if true
                        rpt = new lWeekReport(startDate.Value, endDate.Value);
#else
                        rpt = new lSqlWeekReport(startDate.Value, endDate.Value);
#endif
                        break;
                    case (int)receiptsRptType.byMonth:
#if true
                        rpt = new lMonthReport(startDate.Value, endDate.Value);
#else
                        rpt = new lSqlMonthReport(startDate.Value, endDate.Value);
#endif
                        break;
                }
            }
            else if (buildingRadio.Checked)
            {
                rpt = new lBuildingReport(buildingCmb.Text, startDate.Value, endDate.Value);
            }
            else if (constrorgRadio.Checked)
            {
                rpt = new lConstrorgReport(constrorgCmb.Text, startDate.Value, endDate.Value);
            }
            else if (daysumRd.Checked)
            {
                rpt = new lDaysumReport(startDate.Value, endDate.Value);
            }
            else if (rptRcptRad.Checked)
            {
                rpt = new lReceiptsDays(startDate.Value, endDate.Value);
            }
            if (rpt != null)
            {
                var previewDlg = new rptPreview();
                previewDlg.mRpt = rpt;
                previewDlg.ShowDialog();
                //rpt.Run();
                rpt.Clean();
                rpt.Dispose();
            }
#if return_main
            Close();
#endif
        }

        private void LReportDlg_Load(object sender, EventArgs e)
        {
            {
                //load data for building combo box
                BindingSource bs = new BindingSource();
                DataContent dc = appConfig.s_contentProvider.CreateDataContent("building");
                bs.DataSource = dc.m_dataTable;
                buildingCmb.DataSource = bs;
                buildingCmb.DisplayMember = dc.m_dataTable.Columns[1].ColumnName;
            }
            //load data for constr org cmb
            {
                BindingSource bs = new BindingSource();
                DataContent dc = appConfig.s_contentProvider.CreateDataContent("constr_org");
                bs.DataSource = dc.m_dataTable;
                constrorgCmb.DataSource = bs;
                constrorgCmb.DisplayMember = dc.m_dataTable.Columns[1].ColumnName;
            }
        }
    }

    public class lEditDlg : Form
    {
        lDataPanel m_dataPanel;
        TableLayoutPanel m_tblPanel;
        protected lEditDlg(lDataPanel dataPanel)
        {
            m_dataPanel = dataPanel;

            InitializeComponent();
            m_dataPanel.initCtrls();

            m_tblPanel = new TableLayoutPanel();
            m_tblPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            m_tblPanel.Dock = DockStyle.Fill;
            m_tblPanel.Controls.Add(m_dataPanel.m_reloadPanel, 0, 1);
            m_tblPanel.Controls.Add(m_dataPanel.m_sumPanel, 1, 1);
            m_tblPanel.Controls.Add(m_dataPanel.m_dataGridView, 0, 2);
            m_tblPanel.SetColumnSpan(m_dataPanel.m_dataGridView, 2);

            Controls.Add(m_tblPanel);

            //font
            //this.Font = lConfigMng.getFont();
        }
        private void InitializeComponent()
        {
            Form form = this;
            form.Location = new Point(0, 0);
            form.Size = new Size(500, 300);
            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            m_dataPanel.LoadData();
            this.Text = m_dataPanel.m_tblInfo.m_tblAlias;
        }
        protected override void OnClosed(EventArgs e)
        {
            m_dataPanel.m_dataContent.m_dataTable.RejectChanges();
            base.OnClosed(e);
        }
    }

    public class lGroupNameEditDlg : lEditDlg
    {
        public lGroupNameEditDlg()
            : base(new lGroupNameDataPanel())
        {
        }
    }
    public class lReceiptsContentEditDlg : lEditDlg
    {
        public lReceiptsContentEditDlg()
            : base(new lReceiptsContentDataPanel())
        {
        }
    }
    public class lBuildingEditDlg : lEditDlg
    {
        public lBuildingEditDlg()
            : base(new lBuildingDataPanel())
        {
        }
    }
    public class lConstrorgEditDlg : lEditDlg
    {
        public lConstrorgEditDlg()
            : base(new lConstrorgDataPanel())
        {
        }
    }


    public class lPasswdDlg : Form
    {
        TableLayoutPanel m_tblPanel;
        TextBox m_passwdTxt;
        Button m_passwdBtn;
        Label m_passwdLbl;
        TextBox m_userTxt;
        Label m_userLbl;

        public string m_md5;

        public lPasswdDlg()
        {
            InitializeComponent();

            //font
            //this.Font = lConfigMng.getFont();
        }

        private void M_passwdBtn_Click(object sender, EventArgs e)
        {
            //trick
            m_md5 = "";
            do
            {
                string ztxt = m_userTxt.Text + m_passwdTxt.Text;
                if (ztxt == "PKTChuaBaVangpkt310118") break;

                MD5 md5Hash = MD5.Create();
                m_md5 = GetMd5Hash(md5Hash, ztxt);
            } while (false);
            this.Close();
        }

        private void InitializeComponent()
        {
            Form form = this;
            form.Location = new Point(0, 0);
            form.Size = new Size(220, 150);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.Text = "Đăng nhập";

            //crt ctrl
            m_passwdTxt = new TextBox();
            m_passwdBtn = new Button();
            m_passwdLbl = new Label();
            m_tblPanel = new TableLayoutPanel();
            m_userLbl = new Label();
            m_userTxt = new TextBox();

            //int label
            m_passwdLbl.Anchor = AnchorStyles.Right;
            m_passwdLbl.Text = "Password";
            m_passwdLbl.TextAlign = ContentAlignment.MiddleCenter;
            //txt
            m_passwdTxt.Anchor = AnchorStyles.Left;
            m_passwdTxt.MaxLength = 16;
            m_passwdTxt.PasswordChar = '●';
            m_passwdTxt.CharacterCasing = CharacterCasing.Lower;
            m_passwdTxt.TextAlign = HorizontalAlignment.Center;

            //int label
            m_userLbl.Anchor = AnchorStyles.Right;
            m_userLbl.Text = "User";
            m_userLbl.TextAlign = ContentAlignment.MiddleCenter;
            //user
            m_userTxt.Anchor = AnchorStyles.Left;
            m_userTxt.MaxLength = 16;
            m_userTxt.TextAlign = HorizontalAlignment.Center;
            m_userTxt.Text = "PKTChuaBaVang";

            //int btn
            m_passwdBtn.Anchor = AnchorStyles.None;
            m_passwdBtn.Text = "OK";
            m_passwdBtn.Click += M_passwdBtn_Click;
            this.AcceptButton = m_passwdBtn;

            //tbl
            m_tblPanel.ColumnCount = 2;
            m_tblPanel.RowCount = 3;
            m_tblPanel.Anchor = AnchorStyles.None;
            m_tblPanel.Dock = DockStyle.None;
            m_tblPanel.Dock = DockStyle.Fill;

            m_tblPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            m_tblPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            m_tblPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            m_tblPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            m_tblPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            m_tblPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

            m_tblPanel.Controls.Add(m_userLbl   , 0, 0);
            m_tblPanel.Controls.Add(m_userTxt   , 1, 0);
            m_tblPanel.Controls.Add(m_passwdLbl , 0, 1);
            m_tblPanel.Controls.Add(m_passwdTxt , 1, 1);
            m_tblPanel.Controls.Add(m_passwdBtn , 0, 2);
            m_tblPanel.SetColumnSpan(m_passwdBtn, 2);

            Controls.Add(m_tblPanel);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
