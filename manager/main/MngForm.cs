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
using test_binding.lecture;

namespace test_binding
{
    public partial class MngForm : Form
    {
        public static lContentProvider s_contentProvider;
        private TabControl m_tabCtrl;

        public MngForm()
        {
            InitializeComponent();

            //init config & load config
            appConfig.s_config = lConfigMng.crtInstance();

            if (appConfig.s_config.m_dbSchema == null)
            {
#if use_sqlite
                appConfig.s_config.m_dbSchema = new lSQLiteDbSchema();
#else
                appConfig.s_config.m_dbSchema = new lSqlDbSchema();
#endif  //use_sqlite
            }

            //init content provider
#if use_sqlite
            s_contentProvider = lSQLiteContentProvider.CrtInstance(this);
#else
                appConfig.s_contentProvider = lSqlContentProvider.getInstance(null);
#endif  //use_sqlite

            Menu = new MainMenu();
            var miWindow = new MenuItem("Windows");
            Menu.MenuItems.Add(miWindow);
            var miHelp = new MenuItem("Help");
            Menu.MenuItems.Add(miHelp);
            var miMng = new MenuItem("Task Manager");
            miMng.Click += MiMng_Click;
            miWindow.MenuItems.Add(miMng);
            var miLect = new MenuItem("Lecture Manager");
            miLect.Click += MiLectMng_Click;
            miWindow.MenuItems.Add(miLect);

            var tc = new TabControl();
            tc.Dock = DockStyle.Fill;
            tc.TabPages.AddRange(new TabPage[] {
                new OrgTab().m_pg,
                new TaskTab().m_pg,
                new TrainingTab().m_pg,
                new LectureTab().m_pg,
                new DocumentTab().m_pg,
            }) ;
            tc.SelectedIndex = 0;
            this.Controls.Add(tc);
        }

        private void MiLectMng_Click(object sender, EventArgs e)
        {
            var form = new LectForm();
            form.ShowDialog();
        }

        private void MiMng_Click(object sender, EventArgs e)
        {
            var form = new Form1();
            form.ShowDialog();
        }
    }
}
