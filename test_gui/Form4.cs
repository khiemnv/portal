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
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
            Button newBtn = new Button();
            newBtn.Text = "new btn";
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
