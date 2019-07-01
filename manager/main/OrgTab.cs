#define use_json
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace test_binding
{
    class OrgTab
    {
        public TabPage m_pg;
        private string m_GrpTmplPath = @"..\..\main\GroupTmpl.html";
        private string m_CBV = "http://chuabavang.com.vn/";

        public OrgTab()
        {
            InitCtrls();
        }
        private void InitCtrls()
        {
            var tbl = new TableLayoutPanel();
            var spl = new SplitContainer();
            var tree = new TreeView();

            //load data
            DataTable dt = GetOrganization();
            Node root = BldOrgTree(dt);
            var tnRoot = new TreeNode(root.name) { Tag = root.id };
            Queue<KeyValuePair<Node, TreeNode>> q = new Queue<KeyValuePair<Node, TreeNode>>();
            q.Enqueue(new KeyValuePair<Node, TreeNode>(root, tnRoot));
            while (q.Count > 0)
            {
                var rec = q.Dequeue();
                foreach (Node child in rec.Key.childs)
                {
                    var tnChild = new TreeNode(child.name) { Tag = child.id };
                    rec.Value.Nodes.Add(tnChild);
                    q.Enqueue(new KeyValuePair<Node, TreeNode>(child, tnChild));
                }
            }

            tree.Dock = DockStyle.Fill;
            tree.NodeMouseClick += Tree_NodeMouseClick;
            tree.Nodes.Add(tnRoot);
            spl.Dock = DockStyle.Fill;
            spl.Orientation = Orientation.Vertical; // spl1 | spl2
            spl.Panel1.Controls.Add(tree);
            spl.FixedPanel = FixedPanel.Panel1;
            spl.SplitterDistance = 150;
            wb.Dock = DockStyle.Fill;
            //wb.ScriptErrorsSuppressed = true;
            wb.DocumentCompleted += Wb_DocumentCompleted;
            wb.Url = new Uri(m_CBV);

            spl.Panel2.Controls.Add(wb);
            tbl.Dock = DockStyle.Fill;
            tbl.Controls.Add(spl);
            m_pg = new TabPage();
            m_pg.Controls.Add(tbl);
            m_pg.Text = "Giới thiệu";
        }

        private bool bUpdate;
        private string curGrpId;
        private void Wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (bUpdate)
            {
                bUpdate = false;
#if !use_json
                var sec = QrySection(curGrpId);
                var document = wb.Document;
                document.GetElementById("group_name").InnerHtml = sec.name;
                document.GetElementById("mng_name").InnerHtml = sec.mng;
                document.GetElementById("mng_desc").InnerHtml = sec.mngD;
                document.GetElementById("ass_name").InnerHtml = sec.assmng;
                document.GetElementById("ass_desc").InnerHtml = sec.assmngD;
                document.GetElementById("grp_regs").InnerHtml = sec.regs;
                var txt = "<table style=\"width: 100 %; margin - left:10px\">";
                foreach (SamonRec i in sec.samons)
                {
                    txt += "<tr>";
                    txt += "<td>" + i.name + "</td>";
                    txt += "</tr>";
                }
                txt += "</table>";
                document.GetElementById("tbl").InnerHtml = txt;
#endif
            }
        }

        WebBrowser wb = new WebBrowser();
        private void Tree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            int id = (int)e.Node.Tag;
            var node = m_nodeDict[id];
            if (node.section_number != "")
            {
#if use_json
                var txt = GenGrpHtml(node.section_number);
                wb.DocumentText = txt;
#else
                var path = Path.GetFullPath( m_GrpTmplPath);
                wb.Url = new Uri( string.Format(@"file://{0}",path));
#endif
                bUpdate = true;
                curGrpId = node.section_number;
            }
            else
            {
                if (Uri.TryCreate(node.uri, UriKind.Absolute, out Uri uri))
                    wb.Url = uri;
            }
        }

        private SearchBuilder m_sectionSB;
        private SearchBuilder m_monkSB;
        private string GenGrpHtml(string group_number)
        {
            var sec = QrySection(group_number);
            Debug.Assert(sec != null);

            string tmpl;
            tmpl = System.IO.File.ReadAllText(m_GrpTmplPath);
            //return tmpl;
            var jsTxt = Sec2Json(sec);
            var rpl = tmpl.Replace("secTxt = '';", string.Format("secTxt = '{0}'", jsTxt));
            return rpl;
        }

        public string Sec2Json(object obj)
        {
            Type[] knownTypes = new Type[] {
                    typeof(SectionRec),
                    typeof(SamonRec)
                };
            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
            settings.IgnoreExtensionDataObject = true;
            settings.EmitTypeInformation = EmitTypeInformation.AsNeeded;
            settings.KnownTypes = knownTypes;
            var x= new DataContractJsonSerializer(typeof(SectionRec), settings);
            var mem = new MemoryStream();
            x.WriteObject(mem, obj);
            StreamReader sr = new StreamReader(mem);
            mem.Position = 0;
            string ret = sr.ReadToEnd();
            return ret;
        }

        [DataContract(Name = "SamonRec")]
        class SamonRec
        {
            [DataMember(Name = "name", EmitDefaultValue = false)]
            public string name;
        }

        [DataContract(Name = "SectionRec")]
        class SectionRec
        {
            [DataMember(Name = "name", EmitDefaultValue = false)]
            public string name;
            [DataMember(Name = "mng_name", EmitDefaultValue = false)]
            public string mng;
            [DataMember(Name = "mng_desc", EmitDefaultValue = false)]
            public string mngD;
            [DataMember(Name = "ass_name", EmitDefaultValue = false)]
            public string assmng;
            [DataMember(Name = "ass_desc", EmitDefaultValue = false)]
            public string assmngD;
            [DataMember(Name = "grp_regs", EmitDefaultValue = false)]
            public string regs;
            [DataMember(Name = "members", EmitDefaultValue = false)]
            public List<SamonRec> samons;
        }

        private SectionRec QrySection(string group_number)
        {
            if (m_sectionSB == null) { m_sectionSB = new SearchBuilder(appConfig.s_config.GetTable(TableIdx.Section)); }
            m_sectionSB.Clear();
            m_sectionSB.Add(SectionTblInfo.ColIdx.sec.ToField(), group_number);
            m_sectionSB.Search();
            var rows = m_sectionSB.dc.m_dataTable.Rows;

            Debug.Assert(rows.Count > 0, "section not exists");
            if (rows.Count == 0) return null;

            var sec = new SectionRec();
            var row = rows[0];
            var mngId = row[SectionTblInfo.ColIdx.mng.ToField()].ToString();
            sec.name = row[SectionTblInfo.ColIdx.name.ToField()].ToString();
            sec.mng = GetMonkName(mngId);
            sec.mngD = row[SectionTblInfo.ColIdx.mngD.ToField()].ToString();
            var assmngId = row[SectionTblInfo.ColIdx.ass.ToField()].ToString();
            sec.assmng = GetMonkName(assmngId);
            sec.assmngD = row[SectionTblInfo.ColIdx.assD.ToField()].ToString();
            sec.regs = row[SectionTblInfo.ColIdx.regs.ToField()].ToString();
            sec.samons = QryMonks(group_number);

            return sec;
        }

        private List<SamonRec> QryMonks(string section_number)
        {
            if (m_monkSB == null) { m_monkSB = new SearchBuilder(appConfig.s_config.GetTable(TableIdx.Samon)); }
            m_monkSB.Clear();
            m_monkSB.Add(SamonTblInfo.ColIdx.sec.ToField(), section_number);
            m_monkSB.Search();
            var lst = new List<SamonRec>();
            var rows = m_monkSB.dc.m_dataTable.Rows;
            for (int i = 0; i < rows.Count; i++)
            {
                var rec = new SamonRec();
                rec.name = rows[i][SamonTblInfo.ColIdx.name.ToField()].ToString();
                lst.Add(rec);
            }
            return lst;
        }

        private string GetMonkName(string monk_number)
        {
            string ret = "";
            if (m_monkSB == null) { m_monkSB = new SearchBuilder(appConfig.s_config.GetTable(TableIdx.Samon)); }
            m_monkSB.Clear();
            m_monkSB.Add(SamonTblInfo.ColIdx.samon.ToField(), monk_number);
            m_monkSB.Search();
            var rows = m_monkSB.dc.m_dataTable.Rows;
            Debug.Assert(rows.Count > 0, "monk_number not exists");
            if (rows.Count > 0)
            {
                ret = rows[0][SamonTblInfo.ColIdx.name.ToField()].ToString();
            }
            return ret;
        }
        private Dictionary<int, Node> m_nodeDict;
        private Node BldOrgTree(DataTable dt)
        {
            var tDict = new Dictionary<int, Node>();
            foreach (DataRow row in dt.Rows)
            {
                int id = int.Parse(row[OrganizationTblInfo.ColIdx.ID.ToField()].ToString());
                string pos = row[OrganizationTblInfo.ColIdx.pos.ToField()].ToString();
                string grp = row[OrganizationTblInfo.ColIdx.sec.ToField()].ToString();
                string pic = row[OrganizationTblInfo.ColIdx.pic.ToField()].ToString();
                string uri = row[OrganizationTblInfo.ColIdx.Note.ToField()].ToString();
                int sup = int.Parse(row[OrganizationTblInfo.ColIdx.sup.ToField()].ToString());
                var node = new Node() { id = id, name = pos, section_number = grp, samon_number = pic, uri = uri };
                tDict.Add(id, node);
                if (sup != 0)
                {
                    var parent = tDict[sup];
                    parent.childs.Add(node);
                }
            }
            m_nodeDict = tDict;
            return tDict[1];
        }

        private DataTable GetOrganization()
        {
#if stub_data
            var obj = new Object[,]{
               {"1  " ,"thầy trụ trì ","nhan_su_id_001 "," 0 " ,""     },
               {"2  " ,"quản chúng   ","nhan_su_id_002 "," 1 " ,""      },
               {"3  " ,"tri sự       ","nhan_su_id_003 "," 2 " ,""          },
               {"4  " ,"trưởng ban 1 ","nhan_su_id_004 "," 3 " ," tri_su "  },
               {"5  " ,"phó ban 1    ","nhan_su_id_005 "," 4 " ," tri_su "      },
               {"6  " ,"trưởng ban 2 ","nhan_su_id_006 "," 3 " ," tri_khach "  },
               {" 7 " ," phó ban 2   ","nhan_su_id_007 "," 6 " ," tri_khach "     },
            };

            var dt = new DataTable();
            var tb = new OrganizationTblInfo();
            foreach (TableInfo.ColInfo col in tb.m_cols)
            {
                var dc = new DataColumn(col.m_field);
                dt.Columns.Add(dc);
            }
            for (int iRow = 0; iRow < (obj.Length / 5); iRow++)
            {
                var row = dt.NewRow();
                for (int iCol = 0; iCol < 5; iCol++)
                {
                    row[iCol] = ((string)obj[iRow, iCol]).Trim();
                }
                dt.Rows.Add(row);
            }
            return dt;
#else
            var tbl = appConfig.s_config.GetTable(TableIdx.Organization);
            var dc = appConfig.s_contentProvider.CreateDataContent(TableIdx.Organization);
            dc.Search(new List<string>(), new List<SearchParam>());
            return dc.m_dataTable;
#endif
        }

        public class Node
        {
            public int id;
            public string name;
            public string section_number;
            public string samon_number;
            public string uri;
            public List<Node> childs = new List<Node>();
        }

    }
}
