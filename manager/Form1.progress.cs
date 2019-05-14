using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace test_binding
{
    public interface ICursor
    {
        Int64 getPos();
        void setPos(Int64 pos);
        void setStatus(string msg);
        string getStatus();
    }
    public class myElapsed : IDisposable
    {
        int m_begin;
        string m_msg = "";
        public myElapsed(string msg)
        {
            m_msg = msg;
            m_begin = Environment.TickCount;
        }
        public myElapsed()
        {
            m_begin = Environment.TickCount;
        }
        #region dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~myElapsed()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Debug.WriteLine("[{0}] elapsed {1} ms", m_msg, Environment.TickCount - m_begin);
            }
        }
        #endregion
    }
    public class ProgressDlg : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.m_descrTxt = new System.Windows.Forms.Label();
            this.m_cancelBtn = new System.Windows.Forms.Button();
            this.m_percentTxt = new System.Windows.Forms.Label();
            this.m_stepTxt = new System.Windows.Forms.Label();
            this.m_prg = new System.Windows.Forms.ProgressBar();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            //this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.m_descrTxt, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.m_cancelBtn, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.m_percentTxt, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.m_stepTxt, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.m_prg, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42.85714F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.04762F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 23.80952F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(284, 131);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.m_descrTxt.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.m_descrTxt, 3);
            this.m_descrTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_descrTxt.Location = new System.Drawing.Point(4, 1);
            this.m_descrTxt.Name = "label1";
            this.m_descrTxt.Size = new System.Drawing.Size(276, 54);
            this.m_descrTxt.TabIndex = 0;
            this.m_descrTxt.Text = "descripton";
            this.m_descrTxt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button1
            // 
            this.m_cancelBtn.Location = new System.Drawing.Point(216, 101);
            this.m_cancelBtn.Name = "button1";
            this.m_cancelBtn.Size = new System.Drawing.Size(64, 23);
            this.m_cancelBtn.TabIndex = 3;
            this.m_cancelBtn.Text = "Cancel";
            this.m_cancelBtn.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.m_percentTxt.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.m_percentTxt.AutoSize = true;
            this.m_percentTxt.Location = new System.Drawing.Point(14, 80);
            this.m_percentTxt.Name = "label3";
            this.m_percentTxt.Size = new System.Drawing.Size(44, 13);
            this.m_percentTxt.TabIndex = 4;
            this.m_percentTxt.Text = "Percent";
            // 
            // label4
            // 
            this.m_stepTxt.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.m_stepTxt.AutoSize = true;
            this.m_stepTxt.Location = new System.Drawing.Point(234, 80);
            this.m_stepTxt.Name = "label4";
            this.m_stepTxt.Size = new System.Drawing.Size(27, 13);
            this.m_stepTxt.TabIndex = 5;
            this.m_stepTxt.Text = "step";
            // 
            // progressBar1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.m_prg, 3);
            this.m_prg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_prg.Location = new System.Drawing.Point(4, 59);
            this.m_prg.Name = "progressBar1";
            this.m_prg.Size = new System.Drawing.Size(276, 17);
            this.m_prg.TabIndex = 6;
            // 
            // ProgressDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 131);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form1";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label m_descrTxt;
        private System.Windows.Forms.Button m_cancelBtn;
        private System.Windows.Forms.Label m_percentTxt;
        private System.Windows.Forms.Label m_stepTxt;
        private System.Windows.Forms.ProgressBar m_prg;

        public object m_param;
        public Int64 m_endPos;
        public Int64 m_scale;
        public string m_descr;
        public ICursor m_cursor;
        public bool m_isCancel = false;
        int m_timeOut;
        enum state
        {
            init,
            completed,
            canceled,
            closed,
        };
        state m_state = state.init;
        Thread m_task;
        Int64 m_nStep;

#if false
        class myMutex
        {
            Mutex m_mutex;
            public myMutex()
            {
                m_mutex = new Mutex();
            }
            public void enter()
            {
                Debug.WriteLine("{0}.enter {1}", this, Thread.CurrentThread.ManagedThreadId);
                m_mutex.WaitOne();
                Debug.WriteLine("+ enter done");
            }
            public void leave()
            {
                Debug.WriteLine("{0}.leave {1}", this, Thread.CurrentThread.ManagedThreadId);
                m_mutex.ReleaseMutex();
                Debug.WriteLine("+ leave done");
            }
        }
#endif

        public ProgressDlg()
        {
            InitializeComponent();
#if enable_cancel
                m_cancelBtn.Click += M_cancelBtn_Click;
#else
            m_cancelBtn.Visible = false;
            this.ControlBox = false;
            //this.FormBorderStyle = FormBorderStyle.None;
#endif

            this.Load += ProgressDlg_Load;
            m_endPos = 1000;
            m_scale = 1;

            m_timeOut = 100;        //100 ms
        }

        void cancel()
        {
#if enable_cancel
            m_isCancel = true;
#endif
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (m_state == state.init)
            {
                cancel();
                e.Cancel = true;
            }
        }

        private void M_cancelBtn_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("{0}.M_cancelBtn_Click cur thread {1}", this, Thread.CurrentThread.ManagedThreadId);
            cancel();
        }

        private void ProgressDlg_Load(object sender, EventArgs e)
        {
            this.Text = m_descr;
            if (m_cursor == null)
                m_prg.Style = ProgressBarStyle.Marquee;
            start();
        }

        void start()
        {
            m_nStep = m_endPos / m_scale;
            m_prg.Maximum = (int)m_nStep;
            m_prg.Value = 0;
            m_task = new Thread(new ThreadStart(() =>
            {
                for (int i = 1; ; i++)
                {
                    Int64 curPos = m_cursor.getPos();
                    m_descr = m_cursor.getStatus();
                    if (curPos == m_endPos)
                    {
                        m_state = state.completed;
                        break;
                    }
                    if (m_isCancel)
                    {
                        m_state = state.canceled;
                        break;
                    }
                    incPrgCallback((int)(curPos/m_scale), i / 10);
                    Thread.Sleep(m_timeOut);
                    Debug.WriteLine("{0}.m_task cur thread {1} elapsed {2} s",
                        this,
                        Thread.CurrentThread.ManagedThreadId,
                        i / 10);
                }
                closeDlgCallback();
            }
            ));
            m_task.Start();
        }

        delegate void lCloseDlg();
        void closeDlgCallback()
        {
            Debug.WriteLine("{0}.closeDlgCallback cur thread {1}", this,
                Thread.CurrentThread.ManagedThreadId);
            if (this.m_prg.InvokeRequired)
            {
                Debug.WriteLine("+ invoked req");
                //call it self in form thread
                lCloseDlg d = new lCloseDlg(this.closeDlgCallback);
                this.Invoke(d);
            }
            else
            {
                Debug.WriteLine("+ close");
                this.Close();
            }
        }

        delegate void lIncProgress(int iStep, int elapsed);
        void incPrgCallback(int iStep, int elapsed)
        {
            Debug.WriteLine("{0}.incPrgCallback cur thread {1}", this, Thread.CurrentThread.ManagedThreadId);
            if (this.m_prg.InvokeRequired)
            {
                Debug.WriteLine("+ invoke req");
                //call it self in form thread
                lIncProgress d = new lIncProgress(this.incPrgCallback);
                this.Invoke(d, new object[] { iStep, elapsed });
            }
            else
            {
                Debug.WriteLine("+ method body {0} {1}", iStep, elapsed);
                this.m_prg.Value = iStep;
                double percent = (double)iStep / m_nStep;
                m_percentTxt.Text = percent.ToString("0.00%");
                m_stepTxt.Text = string.Format("{0}/{1}", iStep, m_nStep);
                m_descrTxt.Text = string.Format("{0} \nElapsed {1} s", m_descr, elapsed);
            }
        }
    }
}
