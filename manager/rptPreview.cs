using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_binding
{
    public partial class rptPreview : Form
    {
        public rptPreview()
        {
            InitializeComponent();

            reportViewer1.Padding = new Padding(5, 5, 5, 5);
            Resize += Form1_Resize;
            reportViewer1.ZoomChange += ReportViewer1_ZoomChange;

            //menu
            this.Menu = new MainMenu();
            var miFile = new MenuItem("File");
            var miSaveAs = new MenuItem("Save As...");
            miSaveAs.Click += MiSaveAs_Click;
            miFile.MenuItems.Add(miSaveAs);

            var miOption = new MenuItem("Option");
            var miAutoScale = new MenuItem("Auto Scale");
            miAutoScale.Click += MiAutoScale_Click;

            miOption.MenuItems.Add(miAutoScale);

            Menu.MenuItems.AddRange(new MenuItem[] { miFile, miOption });

            //font
            //this.Font = lConfigMng.getFont();
            miAutoScale.Checked = true;
            mAutoScale = true;
        }

        //data
        public lBaseReport mRpt;

        //option
        bool mAutoScale = false;

        //gui udpate
        private void MiAutoScale_Click(object sender, EventArgs e)
        {
            mAutoScale = !mAutoScale;
            MenuItem mi = (MenuItem)sender;
            mi.Checked = mAutoScale;
        }
        private void ReportViewer1_ZoomChange(object sender, ZoomChangeEventArgs e)
        {
            if (!mAutoScale) return;
            int reqW = getPrintPageW(e.ZoomPercent);
            sizeFormW(reqW);
        }

        int setPrintPageW(int reqW)
        {
            int pgW = getPrintPageWidth();
            if (pgW == -1) return -1;

            int percent = reqW * 100 / pgW;
            reportViewer1.ZoomPercent = percent;
            return reqW;
        }

        int getPrintPageW(int percent)
        {
            int pgW = getPrintPageWidth();
            int reqW = percent * pgW / 100;
            return reqW;
        }
        int sizeFormW(int reqW = -1)
        {
            if (reqW == -1)
            {
                reqW = Size.Width;
            }
            else
            {
                int reqH = this.Size.Height;
                Size = new Size(reqW, reqH);
            }
            return reqW;
        }
     

        int getPrintPageWidth()
        {
            var pg = reportViewer1.GetPageSettings();
            if (pg == null) return -1;
            int pgW = (int)pg.PrintableArea.Width;
            if (pg.Landscape)
            {
                pgW = (int)pg.PrintableArea.Height;
            }
            return pgW;
        }
        int getZoomPercent()
        {
            int percent = reportViewer1.ZoomPercent;
            return percent;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (!mAutoScale) return;
            int reqW = Size.Width;
            setPrintPageW(reqW);
        }

        //data processing
        private void MiSaveAs_Click(object sender, EventArgs e)
        {
            string path;
            //"Excel" "EXCELOPENXML" "IMAGE" "PDF" "WORD" "WORDOPENXML"
            selectPath(out path);
        }
        class lMapExt
        {
            public string ext;
            public string format;
            public exportCallback cb;
        }

        delegate void exportCallback(LocalReport report, string pdfPath, string format);

        int selectPath(out string path)
        {
            int ret = 0;
            //select output path
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            path = "";
            saveFileDialog1.Filter = "pdf files (*.pdf)|*.pdf"
                + "|Excel files (*.xls;*.xlsx)|*.xls;*.xlsx"
                + "|Word files (*.doc;*.docx)|*.doc;*.docx"
                + "|Image Files (*.bmp;*.jpg;*.gif)|*.bmp;*.jpg;*.gif"
                + "|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            var map = new lMapExt[]
            {
                new lMapExt { ext = ".pdf" , format = "PDF", cb = Export2PDF },
                new lMapExt { ext = ".xls" , format = "Excel", cb = Export2 },
                new lMapExt { ext = ".xlsx" , format = "EXCELOPENXML", cb = Export2},
                new lMapExt { ext = ".doc" , format = "WORD", cb = Export2 },
                new lMapExt { ext = ".docx" , format = "WORDOPENXML", cb = Export2 },
                new lMapExt { ext = ".bmp" , format = "IMAGE", cb = Export2 },
                new lMapExt { ext = ".jpg" , format = "IMAGE", cb = Export2 },
                new lMapExt { ext = ".gif" , format = "IMAGE", cb = Export2 },
            };

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = saveFileDialog1.FileName;
                string ext = Path.GetExtension(path).ToLower();
                foreach (var i in map)
                {
                    if (i.ext == ext)
                    {
                        i.cb(reportViewer1.LocalReport, path, i.format);
                        break;
                    }
                }
            }
            saveFileDialog1.Dispose();
            return ret;
        }

        protected void Export2PDF(LocalReport report, string pdfPath, string format)
        {
            string deviceInfo =
              @"<DeviceInfo>
                    <OutputFormat>EMF</OutputFormat>
                    <PageWidth>8.5in</PageWidth>
                    <PageHeight>11in</PageHeight>
                    <MarginTop>0.25in</MarginTop>
                    <MarginLeft>0.25in</MarginLeft>
                    <MarginRight>0.25in</MarginRight>
                    <MarginBottom>0.25in</MarginBottom>
                </DeviceInfo>";

            {
                byte[] bytes = report.Render("PDF", deviceInfo);
                FileStream fs = new FileStream(pdfPath, FileMode.Create);
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
            }
        }
        protected void Export2(LocalReport report, string pdfPath, string format)
        {
            byte[] bytes = report.Render(format);
            FileStream fs = new FileStream(pdfPath, FileMode.Create);
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
        }
        private void rptPreview_Load(object sender, EventArgs e)
        {
            reportViewer1.ProcessingMode = ProcessingMode.Local;

            //fill up data
            mRpt.Fill(reportViewer1.LocalReport);
            
            //update viewer
            reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
            reportViewer1.ResetPageSettings();
            var pg = reportViewer1.GetPageSettings();
            //pg.Landscape = true;
            reportViewer1.SetPageSettings(pg);
            this.reportViewer1.RefreshReport();
        }
    }
}
