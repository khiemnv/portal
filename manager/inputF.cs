using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_binding
{
    public partial class inputF : Form
    {
        public inputF()
        {
            InitializeComponent();

            initCtrls();

            Load += InputF_Load;
        }

        private void InputF_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        protected virtual lInputPanel CrtInputPanel()
        {
            return new lReceiptsInputPanel();
        }

        List<ReportParameter> crtParams()
        {
            //List<ReportParameter> rpParams = new List<ReportParameter>()
            //{
            //    new ReportParameter("amountTxts", m_inputPanel.m_amountTxs.ToArray())
            //};
            //return rpParams;
            return m_inputPanel.billRptParams;
        }

        private void showSingleBill()
        {
            //set report data
            var dt = m_inputPanel.billRptData;
            if (dt.Rows.Count > 0)
            {
                reportViewer2.ProcessingMode = ProcessingMode.Local;
                reportViewer2.Clear();

                LocalReport report = reportViewer2.LocalReport;
                report.ReportPath = GetBill();
                report.DataSources.Add(new ReportDataSource("DataSet1", dt));
                report.SetParameters(crtParams());
                report.Refresh();

                reportViewer2.SetDisplayMode(DisplayMode.PrintLayout);
                reportViewer2.ResetPageSettings();
                reportViewer2.RefreshReport();
            }
        }

        protected virtual void LoadData()
        {
            m_inputPanel.LoadData();
            m_inputPanel.RefreshPreview += refreshPreview;
            
        }

        private void refreshPreview(object sender, lInputPanel.PreviewEventArgs e)
        {
            showSingleBill();
        }

        protected lInputPanel m_inputPanel;
        protected virtual string GetBill()
        {
            return @"..\..\bill_general.rdlc";
        }

        protected virtual void initCtrls()
        {
            m_inputPanel = CrtInputPanel();
            m_inputPanel.initCtrls();
            tableLayoutPanel1.Controls.Add(m_inputPanel.m_tbl);
            tableLayoutPanel1.Dock = DockStyle.Fill;
        }
    }

    public class lReceiptsInputF : inputF
    {
        protected override void initCtrls()
        {
            this.Text = "Phiếu Thu";
            base.initCtrls();
        }
#if use_general_bill
        protected override string GetBill()
        {
            return @"..\..\bill_receipts.rdlc";
        }
#endif
        protected override lInputPanel CrtInputPanel()
        {
            return new lReceiptsInputPanel();
        }
    }
    public class lExterPayInputF : inputF
    {
        protected override void initCtrls()
        {
            this.Text = "Phiếu Chi Ngoại";
            base.initCtrls();
        }
#if use_general_bill
        protected override string GetBill()
        {
            return @"..\..\bill_exterpay.rdlc";
        }
#endif
        protected override lInputPanel CrtInputPanel()
        {
            return new lExterPayInputPanel();
        }
    }
    public class lInterPayInputF : inputF
    {
        protected override void initCtrls()
        {
            this.Text = "Phiếu Chi Nội";
            base.initCtrls();
        }
#if use_general_bill
        protected override string GetBill()
        {
            return @"..\..\bill_interpay.rdlc";
        }
#endif
        protected override lInputPanel CrtInputPanel()
        {
            return new lInterPayInputPanel();
        }
    }
    public class lSalaryInputF : inputF
    {
        protected override void initCtrls()
        {
            this.Text = "Phiếu Chi Lương";
            base.initCtrls();
        }
#if use_general_bill
        protected override string GetBill()
        {
            return @"..\..\bill_salary.rdlc";
        }
#endif
        protected override lInputPanel CrtInputPanel()
        {
            return new lSalaryInputPanel();
        }
    }
#if false
    public class lAdvanceInputF : inputF
    {
        protected override void initCtrls()
        {
            this.Text = "Phiếu Chi Tạm Ứng";
            base.initCtrls();
        }
#if use_general_bill
        protected override string GetBill()
        {
            return @"..\..\bill_salary.rdlc";
        }
#endif
        protected override lInputPanel CrtInputPanel()
        {
            return new lAdvanceInputPanel();
        }
    }
#endif
    public class lTaskInputF : inputF
    {
        protected override void initCtrls()
        {
            this.Text = "Công Việc";
            base.initCtrls();
        }
        protected override lInputPanel CrtInputPanel()
        {
            return new lTaskInputPanel();
        }
    }
    public class lOrderInputF : inputF
    {
        protected override void initCtrls()
        {
            this.Text = "Yêu Cầu";
            base.initCtrls();

            //splitContainer1.Panel2Collapsed = true;
            //splitContainer1.Panel2.Hide();
            //splitContainer1.Panel1.Anchor = AnchorStyles.Right;
            //splitContainer1.Panel2.Controls.Add(rightTbl);
            splitContainer1.Panel2.Controls.Clear();
            splitContainer1.Panel2.Controls.Add(m_inputPanel.m_tbl2);
        }
        public lOrderInputPanel m_panel;
        protected override lInputPanel CrtInputPanel()
        {
            m_panel = new lOrderInputPanel();
            return m_panel;
        }
    }
    public class lApproveInputF : inputF
    {
        protected override void initCtrls()
        {
            this.Text = "Phê Duyệt YC";
            base.initCtrls();
            
            splitContainer1.Panel2.Controls.Clear();
            splitContainer1.Panel2.Controls.Add(m_inputPanel.m_tbl2);
        }
        public lApproveInputPanel m_panel;
        protected override lInputPanel CrtInputPanel()
        {
            m_panel = new lApproveInputPanel();
            return m_panel;
        }
    }
}
