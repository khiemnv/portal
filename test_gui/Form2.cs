#define use_custom_cols

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_gui
{
    public partial class Form2 : Form
    {
        protected Button m_btn;
        protected DataGridView m_dataGridView;
        protected TableInfo m_tblInfo;
        public Form2()
        {
            InitializeComponent();
            m_tblInfo = new HumanTblInfo();
            m_btn = new Button() { Text = "Refresh" };
            m_dataGridView = new DataGridView();
            m_dataGridView.Anchor = AnchorStyles.Top & AnchorStyles.Left;
            m_dataGridView.Dock = DockStyle.Fill;

            m_dataGridView.EditingControlShowing += M_dataGridView_EditingControlShowing;
            m_dataGridView.CellValidating += M_dataGridView_CellValidating;
            m_dataGridView.CellParsing += M_dataGridView_CellParsing;
            crtColumns();

            var mainMenu = new MenuStrip();
            var miFile = new ToolStripMenuItem("&Window");
            var miMng = new ToolStripMenuItem("Manager");
            miMng.Click += MiMng_Click;
            miFile.DropDownItems.Add(miMng);
            mainMenu.Items.Add(miFile);

            Size = new Size(800,500);
            //splitContainer1.Panel1.Controls.Add (mainMenu);
            splitContainer1.Panel1.Controls.Add(m_dataGridView);
        }

        private void MiMng_Click(object sender, EventArgs e)
        {
            var form = new Form1();
            form.Show();
        }

        private void M_dataGridView_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            OnCellParsing(e);
        }

        private void M_dataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            OnCellValidating(e);
        }

        private void M_dataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DateTimePicker ctrl)
            {
                var cell = m_dataGridView.SelectedCells[0];
                //ctrl.Value = (DateTime)cell.Value;
                ctrl.Value = DateTime.Now;
            }
            if (e.Control is ComboBox cmb)
            {
                //var cell = m_dataGridView.SelectedCells[0];
                //var colInfo = m_tblInfo.m_cols[cell.ColumnIndex];
                //cmb.DataSource = colInfo.DataTable;
                //cmb.DisplayMember = colInfo.DisplayMember;
                //cmb.ValueMember = colInfo.ValueMember;
            }
        }

        public static string GetDateFormat() { return "yyyy-MM-dd"; }
        public static string GetDisplayDateFormat() { return "dd/MM/yyyy"; }
        public static bool ParseDisplayDate(string txt, out DateTime dt)
        {
            //txt = "dd/MM/yyyy"
            return DateTime.TryParseExact(txt,
                            "d/M/yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out dt);
        }

        protected void OnCellParsing(DataGridViewCellParsingEventArgs e)
        {
            var col = m_tblInfo.m_cols[e.ColumnIndex];
            switch (col.m_type)
            {
                case TableInfo.ColInfo.ColType.dateTime:
                    Debug.WriteLine("OnCellParsing parsing date");

                    if (GetDisplayDateFormat() == "dd/MM/yyyy")
                    {
                        string val = e.Value.ToString();
                        DateTime dt;
                        if (ParseDisplayDate(val, out dt))
                        {
                            e.ParsingApplied = true;
                            e.Value = dt;
                        }
                    }

                    break;
                case TableInfo.ColInfo.ColType.map:
                    {
                        Debug.WriteLine("OnCellParsing parsing enum {0}",e.Value);
                        string val = e.Value.ToString();
                        int n;
                        if (col.ParseEnum(val, out n))
                        {
                            e.ParsingApplied = true;
                            e.Value = n;
                        }
                    }
                    break;
            }
        }

        protected void OnCellValidating(DataGridViewCellValidatingEventArgs e)
        {
            Debug.WriteLine("OnCellValidating {0}", e.FormattedValue);
            //check unique value
            var col = m_tblInfo.m_cols[e.ColumnIndex];
            string val = e.FormattedValue.ToString();
            switch (col.m_type)
            {
                case TableInfo.ColInfo.ColType.dateTime:
                    DateTime dt;
                    bool isOk = ParseDisplayDate (val,out dt);
                    if (!isOk) e.Cancel = true;
                break;
                case TableInfo.ColInfo.ColType.uniq:

                    break;
                case TableInfo.ColInfo.ColType.map:
                    string txt = e.FormattedValue.ToString();
                    int n;
                    bool bChk;
                    if (int.TryParse(txt, out n))
                    {
                        bChk = col.ParseEnum(n, out txt);
                    }
                    else
                    {
                        bChk = col.ParseEnum(txt, out n);
                    }
                    if (bChk == false)
                    {
                        string msg = string.Format("Invalid input for {0}\n{1}", col.m_alias, col.GetHelp());
                        showInputError(msg);
                        e.Cancel = true;
                    }
                    break;
#if check_number_input
                case lTableInfo.lColInfo.lColType.currency:
                case lTableInfo.lColInfo.lColType.num:
                    {
                        UInt64 tmp;
                        if (!UInt64.TryParse(val.Replace(",","") , out tmp))
                        {
                            Debug.WriteLine("{0} {1} not numberic value {2}", this, "OnCellValidating() check unique", val);
                            MessageBox.Show("This field must be numberic!", "Input error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Cancel = true;
                        }
                    }
                    break;
#endif
            }
        }
        void showInputError(string msg)
        {
            MessageBox.Show(msg, "Input error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void crtColumns()
        {
            int i = 0;
            foreach (var field in m_tblInfo.m_cols)
            {
#if !use_custom_cols
                i = m_dataGridView.Columns.Add(field.m_field, field.m_alias);
                var dgvcol = m_dataGridView.Columns[i];
#else
                DataGridViewColumn dgvcol;
                if (field.m_type == TableInfo.ColInfo.ColType.dateTime)
                {
                    dgvcol = new CalendarColumn();
                    dgvcol.SortMode = DataGridViewColumnSortMode.Automatic;
                }
                //else if (field.m_lookupTbl != null)
                //{
                //    //var cmb = new DataGridViewComboBoxColumn();
                //    //DataTable tbl = field.m_lookupData.m_dataSource;
                //    //BindingSource bs = new BindingSource();
                //    //bs.DataSource = tbl;
                //    //cmb.DataSource = bs;
                //    //cmb.DisplayMember = tbl.Columns[1].ColumnName;
                //    //cmb.AutoComplete = true;
                //    //cmb.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
                //    //cmb.FlatStyle = FlatStyle.Flat;
                //    //dgvcol = cmb;
                //    //dgvcol.SortMode = DataGridViewColumnSortMode.Automatic;
                //}
                else if (field.m_type == TableInfo.ColInfo.ColType.map)
                {
                    //DataGridViewComboBoxColumn column = new DataGridViewComboBoxColumn();

                    Dictionary<string, int> dict = field.GetDict();
                    var dt = new DataTable();
                    dt.Columns.Add("name");
                    dt.Columns.Add("val");
                    for (int idx = 0; idx < dict.Count; idx++)
                    {
                        var newRow = dt.NewRow();
                        newRow[0] = dict.Keys.ElementAt(idx);
                        newRow[1] = idx;
                        dt.Rows.Add(newRow);
                    }
                    EnumColumn column = new EnumColumn();
                    column.DataSource = dt;
                    column.ValueMember = "val";
                    column.DisplayMember = "name";
                    column.FlatStyle = FlatStyle.Flat;

                    dgvcol = column;
                }
                else
                {
                    dgvcol = new DataGridViewTextBoxColumn();
                }
                i = m_dataGridView.Columns.Add(dgvcol);
                dgvcol.HeaderText = field.m_alias;
                dgvcol.Name = field.m_field;
#endif //use_custom_cols
                dgvcol.DataPropertyName = field.m_field;
                switch (field.m_type)
                {
#if format_currency
                    case TableInfo.ColInfo.ColType.currency:
                        dgvcol.DefaultCellStyle.Format = lConfigMng.getCurrencyFormat();
                        break;
#endif
                    case TableInfo.ColInfo.ColType.dateTime:
                        dgvcol.DefaultCellStyle.Format = "dd/MM/yyyy";
                        dgvcol.DefaultCellStyle.NullValue = "";
                        break;
                }
                //show hide col
                dgvcol.Visible = field.m_visible;
            }
            //last columns
            var lastCol = m_dataGridView.Columns[i];
            lastCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            lastCol.FillWeight = 1;
        }

        private void updateCols()
        {
            m_dataGridView.Columns[0].Visible = false;
            TableInfo tblInfo = m_tblInfo;
            int i = 1;
            for (; i < m_dataGridView.ColumnCount; i++)
            {
                //show hide columns
                if (tblInfo.m_cols[i].m_visible == false)
                {
                    m_dataGridView.Columns[i].Visible = false;
                    continue;
                }

                m_dataGridView.Columns[i].HeaderText = tblInfo.m_cols[i].m_alias;

#if header_blue
                //header color blue
                m_dataGridView.Columns[i].HeaderCell.Style.BackColor = Color.Blue;
                m_dataGridView.Columns[i].HeaderCell.Style.ForeColor = Color.White;
#endif

                switch (tblInfo.m_cols[i].m_type)
                {
                    case TableInfo.ColInfo.ColType.currency:
                        m_dataGridView.Columns[i].DefaultCellStyle.Format = "#,0";
                        break;
                    case TableInfo.ColInfo.ColType.dateTime:
                        m_dataGridView.Columns[i].DefaultCellStyle.Format = "dd/MM/yyyy";
                        break;
                }
#if false
                    m_dataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    m_dataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    m_dataGridView.Columns[i].FillWeight = 1;
#endif
            }
            m_dataGridView.Columns[i - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            m_dataGridView.Columns[i - 1].FillWeight = 1;
        }
        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
