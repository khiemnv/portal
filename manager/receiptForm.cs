using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_binding
{
    public partial class receiptForm : Form
    {
        public receiptForm()
        {
            InitializeComponent();

            crtDate.CustomFormat = lConfigMng.getDisplayDateFormat();
            
            pcNoTxt.Validating += PcNoTxt_Validating;
            printBtn.Click += PrintBtn_Click;

            Load += ReceiptForm_Load;
        }

        private void ReceiptForm_Load(object sender, EventArgs e)
        {
           m_cnn = (SQLiteConnection)appConfig.s_contentProvider.GetCnn();
        }

        //select path to print

        private void PrintBtn_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            LocalReport report = new LocalReport();
            report.ReportPath = @"..\..\rpt_receiptSingle.rdlc";
            //add report params
            List<ReportParameter> rpParams = new List<ReportParameter>()
            {
                new ReportParameter("crtDate", crtDate.Value.ToString(lConfigMng.getDateFormat())),
                new ReportParameter("bookNo", bookNumTxt.Text),
                new ReportParameter("pcNo", pcNoTxt.Text),
                new ReportParameter("name", nameTxt.Text),
                new ReportParameter("addr", addrTxt.Text),
                new ReportParameter("reason", reasonTxt.Text),
                new ReportParameter("note", noteTxt.Text),
                new ReportParameter("amount", amountTxt.Text)
            };
            report.SetParameters(rpParams);
            report.Refresh();

            fileExporter exprt = new fileExporter();
            exprt.export(report);
        }

        private void PcNoTxt_Validating(object sender, CancelEventArgs e)
        {
            string rcptNo = pcNoTxt.Text;
            if (!checkUniqKey(rcptNo))
            {
                //show error msg
                lConfigMng.showInputError("Mã này đã tồn tại!");
                e.Cancel = true;
            }
        }
        
        public SQLiteConnection m_cnn;

        bool checkUniqKey(string val)
        {
            var bRet = true;
            string sql = string.Format("select id, {0} from {1} where {0} = '{2}'",
                "receipt_number", "receipts", val);
            var tbl = appConfig.s_contentProvider.GetData(sql);
            if (tbl.Rows.Count > 0)
            {
                Debug.WriteLine("{0} {1} not unique value {2}", this, "OnCellValidating() check unique", val);
                bRet = false;
            }
            return bRet;
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            //get input data
            string sql = "insert into receipts date = @date, " //date
                + " receipt_number = @rcptno, "
                + " name = @name, "
                + " content = @content, "
                + " amount = @amount, "
                + " note = @note ";
            SQLiteCommand cmd;
            cmd = new SQLiteCommand(sql, m_cnn);
            cmd.Parameters.AddWithValue("@date", crtDate.Value.ToString(lConfigMng.getDateFormat()));
            string rcptNo = pcNoTxt.Text;
            cmd.Parameters.AddWithValue("@rcptno", rcptNo);
            cmd.Parameters.AddWithValue("@name", nameTxt.Text);
            cmd.Parameters.AddWithValue("@content", reasonTxt.Text);
            cmd.Parameters.AddWithValue("@amount", amountTxt.Text);
            cmd.Parameters.AddWithValue("@note", noteTxt.Text);

            if (checkUniqKey(rcptNo))
            {
                int nRet = cmd.ExecuteNonQuery();
                //show input success
            }
            else
            {
                Debug.Assert(false, "not used");
            }
        }
    }
}
