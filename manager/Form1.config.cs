#define use_custom_font
#define use_sqlite

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;

namespace test_binding
{

    public class appConfig
    {
        public static lContentProvider s_contentProvider;
        public static lConfigMng s_config;
    }

    [DataContract(Name = "config")]
    public class lConfigMng
    {
        static string m_cfgPath = @"..\..\config.xml";
        //string m_sqliteDbPath = @"..\..\appData.db";
        //string m_cnnStr = @"Data Source=DESKTOP-GOEF1DS\SQLEXPRESS;Initial Catalog=accounting;Integrated Security=true";

        [DataMember(Name = "md5")]
        public string m_md5;

        [DataMember(Name = "printToPdf")]
        public bool m_printToPdf = true;
        [DataMember(Name = "font")]
        public string m_zFont = "";       //Arial,10

        [DataMember(Name = "dbSchema")]
        public lDbSchema m_dbSchema;
        [DataMember(Name = "panels")]
        public List<lBasePanel> m_panels;

        XmlObjectSerializer m_Serializer;

        static XmlObjectSerializer createSerializer()
        {
            Type[] knownTypes = new Type[] {
#if use_sqlite
                    typeof(lSQLiteDbSchema),
#else
                    typeof(lSqlDbSchema),
#endif
                    typeof(lReceiptsTblInfo),
                    typeof(lInternalPaymentTblInfo),
                    typeof(lExternalPaymentTblInfo),
                    typeof(lSalaryTblInfo),
                    typeof(lGroupNameTblInfo),
                    typeof(lBuildingTblInfo),
                    typeof(lReceiptsContentTblInfo),
                    typeof(lConstrorgTblInfo),
                    typeof(lTaskTblInfo),
                    typeof(lOrderTblInfo),

                    typeof(lReceiptsViewInfo),
                    typeof(lInterPaymentViewInfo),
                    typeof(lExterPaymentViewInfo),
                    typeof(lSalaryViewInfo),
                    typeof(lDaysumViewInfo),

                    typeof(lReceiptsDataPanel),
                    typeof(lInterPaymentDataPanel),
                    typeof(lExternalPaymentDataPanel),
                    typeof(lSalaryDataPanel),
                    typeof(lAdvanceDataPanel),
                    typeof(lTaskDataPanel),

                    typeof(lReceiptsReport),
                    typeof(lCurReceiptsReport),
                    typeof(lInternalPaymentReport),
                    typeof(lCurInterPaymentReport),
                    typeof(lExternalPaymentReport),
                    typeof(lCurExterPaymentReport),
                    typeof(lSalaryReport),
                    typeof(lCurSalaryReport),

                    typeof(lSearchCtrlText),
                    typeof(lSearchCtrlDate),
                    typeof(lSearchCtrlNum),
                    typeof(lSearchCtrlCurrency),

                    typeof(lReceiptsSearchPanel),
                    typeof(lInterPaymentSearchPanel),
                    typeof(lExternalPaymentSearchPanel),
                    typeof(lSalarySearchPanel),
                    typeof(lTaskSearchPanel),

                    typeof(lReceiptsPanel),
                    typeof(lInterPaymentPanel),
                    typeof(lExternalPaymentPanel),
                    typeof(lSalaryPanel),
                    typeof(lTaskPanel)
                };
#if false
                DataContractSerializerSettings settings = new DataContractSerializerSettings();
                settings.IgnoreExtensionDataObject = true;
                settings.KnownTypes = knownTypes;
                m_serializer = new DataContractSerializer(
                    typeof(List<lBasePanel>), settings);
#else
            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
            settings.IgnoreExtensionDataObject = true;
            settings.EmitTypeInformation = EmitTypeInformation.AsNeeded;
            settings.KnownTypes = knownTypes;
            return new DataContractJsonSerializer(
                typeof(lConfigMng), settings);
#endif
        }
        static lConfigMng m_instance;
        public static lConfigMng crtInstance()
        {
            string cfgPath = m_cfgPath;
            if (m_instance == null)
            {
                XmlObjectSerializer sz = createSerializer();
                if (File.Exists(cfgPath))
                {
                    XmlReader xrd = XmlReader.Create(cfgPath);
                    xrd.Read();
                    xrd.ReadToFollowing("config");
                    var obj = sz.ReadObject(xrd, false);
                    xrd.Close();
                    m_instance = (lConfigMng)obj;
                }
                else
                {
                    m_instance = new lConfigMng();
                }
                m_instance.loadFont();
                m_instance.m_Serializer = sz;
            }
            return m_instance;
        }

        private Font m_font;
        public static Font getFont()
        {
#if use_custom_font
            return (m_instance != null) ? m_instance.m_font : new Font("Arial", 10);
#else
            return SystemFonts.DefaultFont;
#endif
        }

        private void loadFont()
        {
            do
            {
                if (m_zFont == null) break;

                var arr = m_zFont.Split(',');
                if (arr.Length < 2) break;

                float fontSize;
                if (!float.TryParse(arr[1], out fontSize)) break;

                var tFont = new Font(arr[0], fontSize);
                if (tFont.Name != arr[0]) break;

                m_font = tFont;
                return;
            } while (false);
            m_font = new Font("Arial", 10);
        }
        public static void setFont(Font newFont)
        {
            if (m_instance != null)
            {
                m_instance.m_zFont = string.Format("{0},{1}", newFont.Name, newFont.Size);
                m_instance.m_font = newFont;
                m_instance.UpdateConfig();
            }
        }
        public static string getCurrencyFormat() { return "#,0"; }
        public static bool checkDateString(string zDate)
        {
            Regex reg = new Regex(@"\d{4}-\d{2}-\d{2}");
            return reg.IsMatch(zDate);
        }
#if true
        public static string getDateFormat() { return "yyyy-MM-dd"; }
        public static string getDisplayDateFormat() { return "dd/MM/yyyy"; }
        public static bool parseDisplayDate(string txt, out DateTime dt) {
            //txt = "dd/MM/yyyy"
            bool ret = false;
            dt = DateTime.Now;
            do
            {
                var arr = txt.Split('/');
                if (arr.Length != 3) break;
                //year 1-9999, month 1-12, day
                int y, m, d;
                if (!int.TryParse(arr[2], out y)) break;
                if (!int.TryParse(arr[1], out m)) break;
                if (!int.TryParse(arr[0], out d)) break;
                try
                {
                    dt = new DateTime(y, m, d);
                    ret = true;
                }
                catch
                {
                    Debug.Assert(false, "invalid date string");
                }
            } while (false);
            return ret;
        }
#else
        public static string getDateFormat() { return "dd/MM/yyyy"; }
#endif
        lConfigMng()
        {
        }

        public void UpdateConfig()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.Encoding = Encoding.Unicode;

            XmlWriter xwriter;
            xwriter = XmlWriter.Create(m_cfgPath, settings);
            xwriter.WriteStartElement("config");
            m_Serializer.WriteObjectContent(xwriter, this);
            xwriter.WriteEndElement();
            xwriter.Close();
        }
        public lTableInfo getTable(string tblName)
        {
            List<lTableInfo> tbls = new List<lTableInfo>();
            tbls.AddRange(m_dbSchema.m_tables);
            tbls.AddRange(m_dbSchema.m_views);
            foreach (lTableInfo tbl in tbls)
            {
                if (tbl.m_tblName == tblName)
                    return tbl;
            }
            return null;
        }
        public void test(lReceiptsPanel receiptsPanel)
        {
            DataContractSerializer sz;
            //sz = new DataContractSerializer(typeof(lTableInfo), new Type[] { typeof(lReceiptsTblInfo) });
            //sz.WriteObject(Console.OpenStandardOutput(), m_receiptsPanel.m_tblInfo);
            //sz = new DataContractSerializer(typeof(lDataPanel), new Type[] {
            //    typeof(lReceiptsTblInfo),
            //    typeof(lReceiptsDataPanel),
            //});
            //sz.WriteObject(Console.OpenStandardOutput(), m_receiptsPanel.m_dataPanel);
            //sz = new DataContractSerializer(typeof(lBaseReport), 
            //    new Type[] {
            //        typeof(lReceiptsReport),
            //    }
            //);
            //sz.WriteObject(Console.OpenStandardOutput(), m_receiptsPanel.m_report);
            //sz = new DataContractSerializer(typeof(lSearchPanel),
            //    new Type[] {
            //        typeof(lSearchCtrlText),
            //        typeof(lSearchCtrlDate),
            //        typeof(lSearchCtrlNum),
            //        typeof(lSearchCtrlCurrency),
            //        typeof(lReceiptsSearchPanel),
            //    }
            //);
            //sz.WriteObject(Console.OpenStandardOutput(), m_receiptsPanel.m_searchPanel);
            sz = new DataContractSerializer(typeof(lBasePanel), new Type[] {
                    typeof(lReceiptsTblInfo),
                    typeof(lReceiptsDataPanel),
                    typeof(lReceiptsReport),
                    typeof(lSearchCtrlText),
                    typeof(lSearchCtrlDate),
                    typeof(lSearchCtrlNum),
                    typeof(lSearchCtrlCurrency),
                    typeof(lReceiptsSearchPanel),
                    typeof(lReceiptsPanel)
                });
            sz.WriteObject(Console.OpenStandardOutput(), receiptsPanel);
        }

        public static Button crtButton()
        {
            var btn = new Button();
            btn.Font = getFont();
            return btn;
        }
        public static Label crtLabel()
        {
            var ctrl = new Label();
            ctrl.Font = getFont();
            return ctrl;
        }
        public static TextBox crtTextBox()
        {
            var ctrl = new TextBox();
            ctrl.Font = getFont();
            return ctrl;
        }
        public static CheckBox crtCheckBox()
        {
            var ctrl = new CheckBox();
            ctrl.Font = getFont();
            return ctrl;
        }
        public static ComboBox crtComboBox()
        {
            var ctrl = new ComboBox();
            ctrl.Font = getFont();
            return ctrl;
        }
        public static DataGridView crtDGV()
        {
            var ctrl = new DataGridView();
            ctrl.Font = getFont();
            return ctrl;
        }
        public static MenuStrip CrtMenuStrip()
        {
            var ctrl = new MenuStrip();
            ctrl.Font = getFont();
            return ctrl;
        }
        public static ToolStripMenuItem CrtStripMI()
        {
            var ctrl = new ToolStripMenuItem();
            ctrl.Font = getFont();
            return ctrl;
        }

        private static Size getSize(string txt)
        {
            var size = TextRenderer.MeasureText(txt, getFont());
            return size;
        }
        public static int getWidth(string txt)
        {
            var w = getSize(txt).Width;
            return w;
        }
        public static int getHeight(string txt)
        {
            var h = getSize(txt).Height;
            return h;
        }

        //msg box
        public static void showInputError(string msg)
        {
            MessageBox.Show(msg, "Input error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    class common
    {
        public static string CurrencyToTxt(long amount)
        {
            //n = không, một, ... chín
            //x = a trăm b mươi c
            //b = x tỷ x triệu x nghìn x (đồng chẵn)
            // 10^9  10^6    10^3
            string arrB = "";
            const long oneB = (1000 * 1000 * 1000);
            for (int i = 0; i < 3 && amount > (oneB - 1); i++)
            {
                arrB += bConvert(amount / oneB) + " tỷ ";
                amount = amount % oneB;
            }
            arrB += bConvert(amount);
            //upper case 1st char & trim
            char[] chs = arrB.ToCharArray();
            chs[0] = char.ToUpper(chs[0]);
            int len;
            for (len = chs.Length - 1; chs[len] == ' '; len--) { }
            return new string(chs, 0, len + 1);
        }

        private static string bConvert(long v)
        {
            string ret = "";
            var m = v / 1000000;
            if (m > 0) { ret += tConvert(m) + " triệu "; }
            v = v % 1000000;
            var t = v / 1000;
            if (t > 0) { ret += tConvert(t) + " nghìn "; }
            v = v % 1000;
            if (v > 0) { ret += tConvert(v); }
            return ret;
        }

        private static string tConvert(long v)
        {
            string ret = "";
            var d = v / 100;
            List<string> arr = new List<string>();
            bool reqPadd = false;
            if (d > 0) { arr.Add(dConvert(d) + " trăm"); reqPadd = true; }
            v = v % 100;
            d = v / 10;
            if (d > 0) { arr.Add(dConvert(d) + " mươi"); reqPadd = false; }
            d = v % 10;
            if (d > 0) { if (reqPadd) arr.Add("lẻ"); arr.Add(dConvert(d)); }
            ret = string.Join(" ", arr);
            ret = ret.Replace("một mươi", "mười");
            ret = ret.Replace("mươi một", "mươi mốt");
            return ret;
        }

        private static string dConvert(long d)
        {
            string[] arr = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
            return arr[d];
        }
    }

    class myCallback
    {

    }
}
