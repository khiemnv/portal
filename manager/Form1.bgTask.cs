#define none_stop

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_binding
{
    public delegate void taskCallback1(object data);
    public delegate void taskCallback0();
    public class myTask
    {
        public string sender;
        public string receiver;
        public int iType;
        public object data;
    }
    public class BgTask : myTask
    {
        public enum bgTaskType
        {
            bgExec = 0x00,      //execute callback no param
            DP_BG_SEARCH,       //<MOD>_BG_<TASK>
            NOOP,
        }
        public bgTaskType eType { get { return (bgTaskType)iType; } set{ iType = (int)value; } }
    }
    public class FgTask : myTask
    {
        public enum fgTaskType
        {
            fgExec = 0x10000,
            DP_FG_UPDATESTS,
            F1_FG_UPDATESTS,
            DP_FG_SEARCH,
            F1_FG_UPDATEPRG,
            DP_FG_UPDATESUM,
        }
        public fgTaskType eType
        {
            get { return (fgTaskType)iType; }
            set { iType = (int)value; }
        }
        public int percent;
    }
    public class myWorker
    {
        BackgroundWorker m_worker;
        Queue<myTask> m_msgQueue;
        private myWorker()
        {
            m_msgQueue = new Queue<myTask>();
            m_worker = new BackgroundWorker();
            m_worker.DoWork += msgLoop;
            m_worker.ProgressChanged += onProgressChanged;
            m_worker.RunWorkerCompleted += onCompleted;
            m_worker.WorkerReportsProgress = true;
#if none_stop
            m_worker.RunWorkerAsync();
#endif
        }

        public static myWorker s_worker;
        public static myWorker getWorker()
        {
            if (s_worker == null)
            {
                s_worker = new myWorker();
            }
            return s_worker;
        }

        private void onCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void onProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Debug.WriteLine("onProgressChanged " + e.UserState.ToString());
            OnFgProcess((FgTask)e.UserState);
        }

        private void msgLoop(object sender, DoWorkEventArgs e)
        {
            uint i = 0;
            for (; ; i++)
            {
                if (m_msgQueue.Count > 0)
                {
                    var msg = m_msgQueue.Dequeue();
                    
                    var bgTsk = msg as BgTask;
                    if (bgTsk != null)
                        OnBgProcess(bgTsk);

                    var fgTsk = msg as FgTask;
                    if (fgTsk != null)
                        m_worker.ReportProgress(fgTsk.percent, fgTsk);
                }
                else
                {
#if none_stop
                    Debug.WriteLine("M_srchWorker_DoWork fall sleep, i = {0}", i);
                    sleep(1000);
#else
                    break;
#endif
                }
            }
        }

        bool m_WorkerIsRunning { get { return m_worker.IsBusy; } }
        public void qryBgTask(BgTask task, bool bResume = false)
        {
            Debug.WriteLine("[qryBgTask] {0}", task.eType.ToString());
            m_msgQueue.Enqueue(task);
#if none_stop
#else
            if (bResume && !m_WorkerIsRunning)
            {
                m_worker.RunWorkerAsync();
            }
#endif
        }
        public void execFgTask(FgTask task)
        {
            Debug.Assert(m_WorkerIsRunning, "worker is not running");
            m_worker.ReportProgress(task.percent, task);
        }
        public void qryFgTask(FgTask task, bool bResume = false)
        {
            Debug.WriteLine("[qryFgTask] {0}", task.eType.ToString());
            m_msgQueue.Enqueue(task);
#if none_stop
#else
            if (bResume && !m_WorkerIsRunning)
            {
                m_worker.RunWorkerAsync();
            }
#endif
        }

        public class FillTableCompletedEventArgs : EventArgs
        {
            public Int64 Sum { get; set; }
            public DateTime TimeComplete { get; set; }
        }
        static Dictionary<string, EventHandler<myTask>> m_dict =
            new Dictionary<string, EventHandler<myTask>>();
        private void addEvent(string zType, ref EventHandler<myTask> handler, EventHandler<myTask> value)
        {
            string key = zType + value.Target.ToString();
            if (!m_dict.ContainsKey(key))
            {
                m_dict.Add(key, value);
                handler += value;
            }
            else
            {
                handler -= m_dict[key];
                m_dict[key] = value;
                handler += value;
            }
        }
        private EventHandler<myTask> mBgProcess;
        public event EventHandler<myTask> BgProcess
        {
            add { addEvent("BgProcess", ref mBgProcess, value); }
            remove { }
        }
        private EventHandler<myTask> mFgProcess;
        public event EventHandler<myTask> FgProcess
        {
            add { addEvent("FgProcess", ref mFgProcess, value); }
            remove { }
        }

        protected virtual void OnBgProcess(BgTask task)
        {
            Debug.WriteLine("OnBgProcess " + task.eType.ToString());
            if (task.eType == BgTask.bgTaskType.NOOP)
            {
                //sleep
                sleep((int)task.data);
                return;
            }

            if (mBgProcess != null) mBgProcess.Invoke(this, task);
        }
        protected virtual void OnFgProcess(FgTask task)
        {
            Debug.WriteLine("OnFgProcess " + task.eType.ToString());
            if (mFgProcess != null) mFgProcess.Invoke(this, task);
        }

        //sleep ?ms
        public static void sleep(int timeout)
        {
            var t = Task.Run(() => Task.Delay(timeout));
            t.Wait();
        }
    }

    //custom task
    public class srchTsk : FgTask
    {
        public List<string> m_exprs;
        public List<SearchParam> m_srchParams;
        public srchTsk(List<string> exprs, List<SearchParam> srchParams)
        {
            eType = fgTaskType.DP_FG_SEARCH;
            m_exprs = exprs;
            m_srchParams = srchParams;
            data = this;
        }
    }
    public class updateStsTsk : FgTask
    {
        public string m_txt;
        public updateStsTsk(object owner, string txt)
        {
            eType = fgTaskType.DP_FG_UPDATESTS;
            m_txt = txt;
            data = this;
        }
    }
}
