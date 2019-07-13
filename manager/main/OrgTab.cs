#define use_json
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
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
    public class BaseTab
    {
        public string Obj2Json(object obj, Type[]knownTypes)
        {
            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
            settings.IgnoreExtensionDataObject = true;
            settings.EmitTypeInformation = EmitTypeInformation.AsNeeded;
            settings.KnownTypes = knownTypes;
            var x = new DataContractJsonSerializer(obj.GetType(), settings);
            var mem = new MemoryStream();
            x.WriteObject(mem, obj);
            StreamReader sr = new StreamReader(mem);
            mem.Position = 0;
            string ret = sr.ReadToEnd();
            return ret;
        }
    }
    class TaskTab:BaseTab
    {
        public TabPage m_pg;
        protected TableLayoutPanel m_tbl;
        protected SplitContainer m_spl;
        protected TreeView m_tree;
        protected WebBrowser m_wb;
        public TaskTab()
        {
            InitCtrls();
            BuildTree();
            InitEvent();
        }

        private void InitEvent()
        {
            m_tree.AfterCheck += Tree_AfterCheck;
            m_tree.BeforeCheck += M_tree_BeforeCheck;
            //m_tree.MouseClick += M_tree_MouseClick;
            m_tree.NodeMouseClick += M_tree_NodeMouseClick;
            //m_tree.KeyDown += M_tree_KeyDown;
        }

        private void M_tree_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        private void M_tree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //throw new NotImplementedException();
            OnNodeMouseClick(e);
        }

        TreeViewAction m_action = TreeViewAction.Unknown;
        protected void OnNodeMouseClick(System.Windows.Forms.TreeNodeMouseClickEventArgs e)
        {
            //base.OnNodeMouseClick(e);

            // is the click on the checkbox?  If not, discard it
            System.Windows.Forms.TreeViewHitTestInfo info = m_tree.HitTest(e.X, e.Y);
            if (info == null || info.Location != System.Windows.Forms.TreeViewHitTestLocations.StateImage)
            {
                return;
            }

            // toggle the node's checked status.  This will then fire OnAfterCheck
            System.Windows.Forms.TreeNode tn = e.Node;
            m_action = TreeViewAction.ByMouse;
            tn.Checked = !tn.Checked;
            m_action = TreeViewAction.Unknown;
        }
        protected void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            //base.OnKeyDown(e);

            // is the keypress a space?  If not, discard it
            if (e.KeyCode == System.Windows.Forms.Keys.Space)
            {
                // toggle the node's checked status.  This will then fire OnAfterCheck
                m_tree.SelectedNode.Checked = !m_tree.SelectedNode.Checked;
            }
        }

        private void M_tree_MouseClick(object sender, MouseEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void M_tree_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            //if(e.Action != TreeViewAction.Unknown)
            return;
            if (m_ignore == 0)
            {
                var prev = e.Node.StateImageIndex;
                if (prev != 1) {
                    e.Node.StateImageIndex = 1; }
                else
                {
                    e.Node.StateImageIndex = 0;
                }
            }
        }

        int m_ignore = 0;
        private void Tree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            //if (e.Action != TreeViewAction.Unknown)
            if (m_ignore == 0)
            {
                // check/uncheck tree nodes
                var val = Check(e.Node);
                var lst = new List<TreeNode>();
                lst.Add(e.Node);
                m_ignore++;
                while (lst.Count > 0)
                {
                    var node = lst[0];
                    lst.RemoveAt(0);
                    var parent = node.Parent;
                    if (parent != null)
                    {
                        CheckParentNode(parent, val);
                        lst.Add(parent);
                    }
                }
                lst.Add(e.Node);
                while (lst.Count > 0)
                {
                    var node = lst[0];
                    lst.RemoveAt(0);
                    if (Check(node) != val) { Check(node, val); }
                    lst.AddRange(node.Nodes.Cast<TreeNode>());
                }
                m_ignore--;

                // get task by section lst
                var rootN = m_tree.Nodes["All"];
                List<string> secLst = null;
                if (Check(rootN))
                {
                    //all node is checked
                }
                else
                {
                    secLst = new List<string>();
                    foreach (TreeNode tn in rootN.Nodes)
                    {
                        if (Check(tn)) { secLst.Add(tn.Text); }
                    }
                }
                if(secLst != null)
                {
                    Debug.Print("secLst: {0}",secLst.Count);
                    foreach(var i in secLst) { Debug.Print("    {0}", i); }
                }
                UpdateContent(secLst);
            }
        }

        private List<TreeNode> GetAllLeafs(TreeNode parent)
        {
            var lst = new List<TreeNode>();
            var q = new List<TreeNode>();
            q.Add(parent);
            while(q.Count > 0)
            {
                var n = q[0];
                q.RemoveAt(0);
                if (n.Nodes.Count == 0) { lst.Add(n); }
                else
                {
                    q.AddRange(n.Nodes.Cast<TreeNode>().ToList());
                }
            }
            return lst;
        }

        private void CheckParentNode(TreeNode parent, bool val)
        {
            int i = 0;
            var childLst = parent.Nodes;
            for(; i< childLst.Count; i++)
            {
                var child = childLst[i];
                if(!(Check(child)==val)) { break; }    //child not checked
            }
            if (i == childLst.Count)
            {
                if (Check(parent) != val) {Check(parent, val); }
                Check(parent,val?1:0);
            }
            else
            {
                if (Check(parent) != false) { Check(parent, false); }
                Check(parent,2);
            }
        }

        private bool Check(TreeNode node, bool val)
        {
            //int idx = val?1:0;
            //node.StateImageIndex = idx;
            //return idx == 1;
            if(node.Checked != val)
            {
                node.Checked = val;
            }
            else
            {
                Debug.Assert(false, "should not reset node state");
            }
            return node.Checked;
        }
        private bool Check(TreeNode node, int idx = -1)
        {
            if (idx == -1)
            {
                //node.StateImageIndex = idx;
                //return node.StateImageIndex == 1;
                return node.Checked;
            }
            else
            {
                node.StateImageIndex = idx;
                return node.Checked;
            }
        }

        private void UpdateContent(List<string> secLst)        {
            
            //List<DateTime> dtLst = new List<DateTime> {new DateTime(2019,6,15), new DateTime(2019, 6, 16) };
            List<DateTime> dtLst = new List<DateTime>();
            var dt = DateTime.Now;
            var wd = dt.DayOfWeek;
            for (int i = 1; i < 8; i++)
            {
                dtLst.Add(dt.AddDays(i - (int)wd));
            }
            var tc = QryTabContent(dtLst, secLst);
            var knownTypes = new Type[] {
                    typeof(TabContent),
                    typeof(DayTask),
                    typeof(TaskRec),
                    typeof(PlanRec),
                };
            var jsTxt = Obj2Json(tc, knownTypes);
            var htmlTxt = GenTabHtml(jsTxt);
            m_wb.DocumentText = htmlTxt;
        }

        private void CheckAllChildNodes(TreeNode node, bool val)
        {
            foreach (TreeNode i in node.Nodes)
            {
                if (i.Checked != val) { Check(i, val); }
                if (i.Nodes.Count > 0) { CheckAllChildNodes(i, val); }
            }
        }
        private string GenTabHtml(string jsTxt)
        {
            string tmpl="";
            //var fin = File.OpenText(@"..\..\main\TaskTmpl.html");
            //while (!fin.EndOfStream) { 
            //string line = fin.ReadLine();
            //if (line.IndexOf("//jsTxt = '';") != -1)
            //{
            //    fin.ReadLine();

            //    tmpl += string.Format("jsTxt = '{0}';\n", jsTxt);
            //    tmpl += "jsObj = eval(\"(\" + jsTxt + \")\");\n";
            //}
            //else
            //    tmpl += line + "\n";
            //}
            //fin.Close();
            tmpl = File.ReadAllText(@"..\..\main\TaskTmpl.html");
            //jsTxt = '';
            //var jsObj = eval("(" + jsTxt + ")");
            var rpl = tmpl.Replace("//jsTxt = '';", string.Format("jsTxt = '({0})';\njsObj = eval(jsTxt)", jsTxt));
            return rpl;
        }

        private SearchBuilder m_taskSB;
        private SearchBuilder m_grpSB;
        private List<string> QryGrps()
        {
            if (m_grpSB == null) { m_grpSB = new SearchBuilder(appConfig.s_config.GetTable(TableIdx.GrpName), MngForm.s_contentProvider); }
            m_grpSB.Clear();
            m_grpSB.Search();
            var lst = new List<string>();
            foreach (DataRow row in m_grpSB.dc.m_dataTable.Rows)
            {
                lst.Add(row[1].ToString());
            }
            return lst;
        }
        private TabContent QryTabContent(List<DateTime>dtLst,List<string>secLst)
        {
            var tc = new TabContent();
            tc.planCols = new List<string> {"Kế hoạch","Ban","Tình trạng" };
            tc.taskCols = new List<string> { "Công việc", "Ban", "Tình trạng" };
            tc.recs = new List<DayTask>();
            //qry task
            foreach (DateTime dt in dtLst)
            {
                List<TaskRec> tasks = QryTask(dt, secLst);
                var rec = new DayTask {
                    date = dt.ToString(lConfigMng.GetDisplayDateFormat()),
                    tasks = tasks};
                tc.recs.Add(rec);
            }
            return tc;
        }
        private List< TaskRec> QryTask(DateTime startDt, List<string> sectionLst)
        {
            var lst = new List<TaskRec>();
            if (m_taskSB == null) { m_taskSB = new SearchBuilder(appConfig.s_config.GetTable(TableIdx.Task), MngForm.s_contentProvider); }
            m_taskSB.Clear();
            m_taskSB.Add(TaskTblInfo.ColIdx.Begin.ToField(), startDt);
            if (sectionLst != null) {
                m_taskSB.Add(TaskTblInfo.ColIdx.Group.ToField(), sectionLst);
            }
            m_taskSB.Search();
            foreach (DataRow row in m_taskSB.dc.m_dataTable.Rows)
            {
                TaskStatus sts = (TaskStatus)(int.Parse(row[TaskTblInfo.ColIdx.Stat.ToField()].ToString()));
                var rec = new TaskRec()
                {
                    name = row[TaskTblInfo.ColIdx.Name.ToField()].ToString(),
                    section = row[TaskTblInfo.ColIdx.Group.ToField()].ToString(),
                    status = sts.ToDesc()
                };
                lst.Add(rec);
            }
            return lst;
        }

        [DataContract]
        public class PlanRec
        {
            [DataMember] public string name;
            [DataMember] public string section;
            [DataMember] public string status;
        }
        [DataContract]
        public class TaskRec
        {
            [DataMember] public string name;
            [DataMember] public string section;
            [DataMember] public string status;
        }
        [DataContract]
        public class DayTask
        {
            [DataMember] public string date;
            [DataMember] public List<PlanRec> plans;
            [DataMember] public List<TaskRec> tasks;
        }
        [DataContract]
        public class TabContent
        {
            [DataMember] public List<string> taskCols;
            [DataMember] public List<string> planCols;
            [DataMember] public List<DayTask> recs;
        }

        private void InitCtrls()
        {
            var tbl = new TableLayoutPanel();
            tbl.Dock = DockStyle.Fill;
            var spl = new SplitContainer();

            spl.Dock = DockStyle.Fill;
            spl.Orientation = Orientation.Vertical; // spl1 | spl2
            spl.FixedPanel = FixedPanel.Panel1;
            spl.SplitterDistance = 150;

            tbl.Controls.Add(spl);
            var pg = new TabPage();
            pg.Controls.Add(tbl);
            pg.Text = "Công Việc";

            var trvw = new TreeView();
            //var trvw  = new RikTheVeggie.TriStateTreeView();
            trvw.Dock = DockStyle.Fill;
            //trvw.CheckBoxes = true;
            spl.Panel1.Controls.Add(trvw);

            var wb = new WebBrowser();
            wb.Dock = DockStyle.Fill;
            spl.Panel2.Controls.Add(wb);

            //save control handles
            m_wb = wb;
            m_tree = trvw;
            m_pg = pg;
            m_tbl = tbl;
            m_spl = spl;
        }


        private void BuildTree()
        {
            m_tree.CheckBoxes = false;
            m_tree.StateImageList = new ImageList();
            //var imgLst = new Image[]
            //{
            //    Image.FromFile(@"..\..\img\cb_unchecked.jpg"),
            //    Image.FromFile(@"..\..\img\cb_checked.jpg"),
            //    Image.FromFile(@"..\..\img\cb_gray.bmp"),
            //};
            //m_tree.StateImageList.Images.AddRange(imgLst);
            for (int i = 0; i < 3; i++)
            {
                // Create a bitmap which holds the relevent check box style
                // see http://msdn.microsoft.com/en-us/library/ms404307.aspx and http://msdn.microsoft.com/en-us/library/system.windows.forms.checkboxrenderer.aspx

                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(16, 16);
                System.Drawing.Graphics chkGraphics = System.Drawing.Graphics.FromImage(bmp);
                switch (i)
                {
                    // 0,1 - offset the checkbox slightly so it positions in the correct place
                    case 0:
                        System.Windows.Forms.CheckBoxRenderer.DrawCheckBox(chkGraphics, new System.Drawing.Point(0, 1), System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);
                        break;
                    case 1:
                        System.Windows.Forms.CheckBoxRenderer.DrawCheckBox(chkGraphics, new System.Drawing.Point(0, 1), System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal);
                        break;
                    case 2:
                        System.Windows.Forms.CheckBoxRenderer.DrawCheckBox(chkGraphics, new System.Drawing.Point(0, 1), System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal);
                        break;
                }

                m_tree.StateImageList.Images.Add(bmp);
            }

            m_ignore++;

            var node = m_tree.Nodes.Add("All", "All");
            node.Checked = false;
            node.StateImageIndex = 0;
            var lst = QryGrps();
            foreach(string grpName in lst)
            {
                var child = node.Nodes.Add(grpName);
                child.Checked = false;
                child.StateImageIndex = 0;
            }

            //m_tree.CheckBoxes = false;
            
            m_ignore--;
        }
    }
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
            //wb.Url = new Uri(m_CBV);

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
            if (m_sectionSB == null) { m_sectionSB = new SearchBuilder(appConfig.s_config.GetTable(TableIdx.Section), MngForm.s_contentProvider); }
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
            if (m_monkSB == null) { m_monkSB = new SearchBuilder(appConfig.s_config.GetTable(TableIdx.Samon),MngForm.s_contentProvider); }
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
            if (m_monkSB == null) { m_monkSB = new SearchBuilder(appConfig.s_config.GetTable(TableIdx.Samon), MngForm.s_contentProvider); }
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
            var dc = MngForm.s_contentProvider.CreateDataContent(TableIdx.Organization);
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
