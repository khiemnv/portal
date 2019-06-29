using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_gui
{
    public partial class ban : Form
    {
        public ban()
        {
            InitializeComponent();

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
            var tnRoot = new TreeNode(root.name) { Tag = root.id};
            Queue<KeyValuePair<Node,TreeNode>> q = new Queue<KeyValuePair<Node, TreeNode>>();
            q.Enqueue(new KeyValuePair<Node,TreeNode>( root,tnRoot));
            while(q.Count > 0)
            {
                var rec = q.Dequeue();
                foreach(Node child in rec.Key.childs) {
                    var tnChild = new TreeNode(child.name) { Tag = child.id };
                    rec.Value.Nodes.Add(tnChild);
                    q.Enqueue(new KeyValuePair<Node, TreeNode>(child,tnChild));
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
            wb.ScriptErrorsSuppressed = true;
            wb.Url = new Uri("http://chuabavang.com.vn/");
            spl.Panel2.Controls.Add(wb);
            tbl.Dock = DockStyle.Fill;
            tbl.Controls.Add(spl);
            this.Controls.Add(tbl);
        }

        WebBrowser wb = new WebBrowser();
        private void Tree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            int id = (int)e.Node.Tag;
            var node = m_nodeDict[id];
            wb.DocumentText = string.Format("<html>Ban {0} Chức vụ {1} Phụ trách {2}</html>",node.group_number, node.name, node.human_number);
            //wb.Url = new Uri("");
        }

        private Dictionary<int, Node> m_nodeDict;
        private Node BldOrgTree(DataTable dt)
        {
            var tDict = new Dictionary<int, Node>();
            foreach(DataRow row in dt.Rows)
            {
                int id = int.Parse(row[OrgTbl.ColIdx.ID.ToField()].ToString());
                string pos = row[OrgTbl.ColIdx.pos.ToField()].ToString();
                string grp = row[OrgTbl.ColIdx.grp.ToField()].ToString();
                string man = row[OrgTbl.ColIdx.man.ToField()].ToString();
                int sup = int.Parse(row[OrgTbl.ColIdx.sup.ToField()].ToString());
                var node = new Node() { id = id, name = pos, group_number = grp, human_number = man};
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
            var obj = new Object[,]{
               {"1  " ,"thầy trụ trì ","nhan_su_id_001 "," 0 " ,""     },
               {"2  " ,"quản chúng   ","nhan_su_id_002 "," 1 " ,""      },
               {"3  " ,"tri sự       ","nhan_su_id_003 "," 2 " ,""          },
               {"4  " ,"trưởng ban 1 ","nhan_su_id_004 "," 3 " ," ban_id_1 "  },
               {"5  " ,"phó ban 1    ","nhan_su_id_005 "," 4 " ," ban_id_1 "      },
               {"6  " ,"trưởng ban 2 ","nhan_su_id_006 "," 3 " ," ban_id_2 "  },
               {" 7 " ," phó ban 2   ","nhan_su_id_007 "," 6 " ," ban_id_2 "     },
            };

            var dt = new DataTable();
            var tb = new OrgTbl();
            foreach (TableInfo.ColInfo col in tb.m_cols)
            {
                var dc = new DataColumn(col.m_field);
                dt.Columns.Add(dc);
            }
            for(int iRow = 0; iRow<(obj.Length/5);iRow++)
            {
                var row = dt.NewRow();
                for (int iCol = 0;iCol < 5; iCol++)
                {
                    row[iCol] = ((string)obj[iRow, iCol]).Trim();
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        public class Node
        {
            public int id;
            public string name;
            public string group_number;
            public string human_number;
            public List<Node> childs= new List<Node>();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

        }
    }

    public class OrgTbl:TableInfo
    {
        public enum ColIdx
        {
            [Field("ID"), Alias("ID")] ID,
            [Field("position"), Alias("Chức vụ")] pos,
            [Field("human_number"), Alias("Mã NS")] man,
            [Field("superior"), Alias("Cấp trên")] sup,
            [Field("group_number"), Alias("Ban")] grp,
        }
        public OrgTbl()
        {
            m_tblName = "human";
            m_tblAlias = "Nhân Sự";
            m_crtQry = "CREATE TABLE if not exists organization("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                + "position char(31),"
                + "human_number char(31),"
                + "superior INTEGER,"
                + "group_number char(31))";
            m_cols = new ColInfo[GetCount<ColIdx>()];
            m_cols[(int)ColIdx.ID] = new ColInfo(ColIdx.ID.ToField(), ColIdx.ID.ToAlias(), ColInfo.ColType.num, false);
            m_cols[(int)ColIdx.pos] = new ColInfo(ColIdx.pos.ToField(), ColIdx.pos.ToAlias(), ColInfo.ColType.uniq);
            m_cols[(int)ColIdx.man] = new ColInfo(ColIdx.man.ToField(), ColIdx.man.ToAlias(), ColInfo.ColType.text);
            m_cols[(int)ColIdx.sup] = new ColInfo(ColIdx.sup.ToField(), ColIdx.sup.ToAlias(), ColInfo.ColType.num);
            m_cols[(int)ColIdx.grp] = new ColInfo(ColIdx.grp.ToField(), ColIdx.grp.ToAlias(), ColInfo.ColType.text);
        }
    }
}
