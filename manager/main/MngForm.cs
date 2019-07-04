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

namespace test_binding
{
    public partial class MngForm : Form
    {

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
            appConfig.s_contentProvider = lSQLiteContentProvider.getInstance(null);
#else
                appConfig.s_contentProvider = lSqlContentProvider.getInstance(null);
#endif  //use_sqlite

            Menu = new MainMenu();
            var miWindow = new MenuItem("Windows");
            var miHelp = new MenuItem("Help");
            var miMng = new MenuItem("TaskManager");
            miMng.Click += MiMng_Click;
            miWindow.MenuItems.Add(miMng);
            Menu.MenuItems.Add(miWindow);
            Menu.MenuItems.Add(miHelp);

            var tc = new TabControl();
            tc.Dock = DockStyle.Fill;
            tc.TabPages.AddRange( new TabPage[] {
                new OrgTab().m_pg,
                new TaskTab().m_pg,
            } );
            tc.SelectedIndex = 0;
            this.Controls.Add(tc);
        }

        private void MiMng_Click(object sender, EventArgs e)
        {
            var form = new Form1();
            form.Show();
        }
    }
}
