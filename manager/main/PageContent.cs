using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace test_binding
{
    public class PageContent
    {
        public lContentProvider m_cp;
        public string m_tmpl;
        protected string Obj2Json(object obj, Type[] knownTypes)
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
        protected string GenHtml(string jsTxt)
        {
            string tmpl;
            tmpl = File.ReadAllText(m_tmpl);
            //jsTxt = '';
            //var jsObj = eval("(" + jsTxt + ")");
            var rpl = tmpl.Replace("//jsTxt = '';", string.Format("jsTxt = '({0})';\njsObj = eval(jsTxt)", jsTxt));
            return rpl;
        }
        public virtual string GenHtmlContent(List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    public class LectContent : PageContent
    {
        public LectContent()
        {
#if !CFG_MNG_ANY
            m_tmpl = @"..\..\..\main\LectTmpl.html";
#else
            m_tmpl = @"..\..\main\LectTmpl.html";
#endif
        }

        public override string GenHtmlContent(List<string> args)
        {
            var tc = QryTabContent(args);
            var knownTypes = new Type[] {
                    typeof(TabContent),
                    typeof(LectRec),
                };
            var jsTxt = Obj2Json(tc, knownTypes);
            var htmlTxt = GenHtml(jsTxt);
            return htmlTxt;
        }

        [DataContract]
        public class LectRec
        {
            public string lect; //lecture_number
            [DataMember] public string title;
            [DataMember] public string auth;
            [DataMember] public string target;
            [DataMember] public string topic;
            [DataMember] public string crt;
            [DataMember] public string content;
            [DataMember] public string link;
        }

        [DataContract]
        public class TabContent
        {
            [DataMember] public List<string> cols;
            [DataMember] public List<LectRec> recs;
        }

        private object QryTabContent(List<string> lst)
        {
            var tc = new TabContent();
            var bgtb = appConfig.s_config.GetTable(TableIdx.Lecture);
            var bgsb = new SearchBuilder(bgtb, m_cp);
            bgsb.Clear();
            bgsb.Add(LectureTblInfo.ColIdx.lect.ToField(), lst);
            bgsb.Search();
            tc.recs = new List<LectRec>();
            for (int i = 0; i < bgsb.dc.m_dataTable.Rows.Count; i++)
            {
                var rec = new LectRec();
                var row = bgsb.dc.m_dataTable.Rows[i];
                rec.lect = row[LectureTblInfo.ColIdx.lect.ToField()].ToString();
                rec.title = row[LectureTblInfo.ColIdx.title.ToField()].ToString();
                var auth = (LectureTblInfo.Author)int.Parse(row[LectureTblInfo.ColIdx.auth.ToField()].ToString());
                rec.auth = auth.ToDesc();
                var target = (LectureTblInfo.Target)int.Parse(row[LectureTblInfo.ColIdx.target.ToField()].ToString());
                rec.target = target.ToDesc();
                rec.topic = row[LectureTblInfo.ColIdx.topic.ToField()].ToString();
                var date = (DateTime)row[LectureTblInfo.ColIdx.crt.ToField()];
                rec.crt = date.ToString(lConfigMng.GetDisplayDateFormat());
                rec.content = row[LectureTblInfo.ColIdx.content.ToField()].ToString();
                rec.link = row[LectureTblInfo.ColIdx.link.ToField()].ToString();
                tc.recs.Add(rec);
            }

            tc.cols = new List<string> {
                LectureTblInfo.ColIdx.title.ToAlias(),
                LectureTblInfo.ColIdx.auth.ToAlias(),
                LectureTblInfo.ColIdx.target.ToAlias(),
                LectureTblInfo.ColIdx.topic.ToAlias(),
                LectureTblInfo.ColIdx.crt.ToAlias(),
                LectureTblInfo.ColIdx.content.ToAlias(),
                LectureTblInfo.ColIdx.link.ToAlias(),
            };
            return tc;
        }

        public List<LectRec> QryLectures()
        {
            var dc = m_cp.CreateDataContent(TableIdx.Lecture);
            dc.Search(new List<string>(), new List<SearchParam>());
            var lst = new List<LectRec>();
            foreach (DataRow row in dc.m_dataTable.Rows)
            {
                var rec = new LectRec
                {
                    lect = row[LectureTblInfo.ColIdx.lect.ToField()].ToString(),
                    title = row[LectureTblInfo.ColIdx.title.ToField()].ToString(),
                    auth = ((LectureTblInfo.Author)int.Parse(row[LectureTblInfo.ColIdx.auth.ToField()].ToString())).ToDesc(),
                    target = ((LectureTblInfo.Target)int.Parse(row[LectureTblInfo.ColIdx.target.ToField()].ToString())).ToDesc(),
                    topic = row[LectureTblInfo.ColIdx.topic.ToField()].ToString(),
                    crt = row[LectureTblInfo.ColIdx.crt.ToField()].ToString(),
                    content = row[LectureTblInfo.ColIdx.content.ToField()].ToString(),
                    link = row[LectureTblInfo.ColIdx.link.ToField()].ToString(),
                };
                lst.Add(rec);
            }
            return lst;
        }
    }

    public class TrainContent : PageContent
    {
        public TrainContent()
        {
#if !CFG_MNG_ANY
            m_tmpl = @"..\..\..\main\TrainTmpl.html";
#else
            m_tmpl = @"..\..\main\TrainTmpl.html";
#endif
        }

        [DataContract]
        public class TrngRec
        {
            [DataMember] public string date;
            [DataMember] public string topic;
            [DataMember] public string trainer;
            //[DataMember] public string question;
            [DataMember] public string cmnt;
            [DataMember] public string star;
        }

        [DataContract]
        public class BudgrpRec
        {
            public string numb;
            [DataMember] public string name;
            [DataMember] public string about;
        }

        [DataContract]
        public class TabContent
        {
            [DataMember] public BudgrpRec budgrpRec;
            [DataMember] public List<string> trngCols;
            [DataMember] public List<TrngRec> recs;
        }

        public override string GenHtmlContent(List<string> args)
        {
            var grpId = args[0];
            var tc = QryTabContent(grpId);
            var knownTypes = new Type[] {
                    typeof(TabContent),
                    typeof(BudgrpRec),
                    typeof(TrngRec),
                };
            var jsTxt = Obj2Json(tc, knownTypes);
            var htmlTxt = GenHtml(jsTxt);
            return htmlTxt;
        }

        private object QryTabContent(string grpId)
        {
            var tc = new TabContent();
            var bgtb = appConfig.s_config.GetTable(TableIdx.Budgrp);
            var bgsb = new SearchBuilder(bgtb, m_cp);
            bgsb.Clear();
            bgsb.Add(BudgrpTblInfo.ColIdx.grp.ToField(), grpId);
            bgsb.Search();
            var bgrec = new BudgrpRec();
            for (int i = 0; i < bgsb.dc.m_dataTable.Rows.Count; i++)
            {
                var row = bgsb.dc.m_dataTable.Rows[i];
                bgrec.name = row[BudgrpTblInfo.ColIdx.name.ToField()].ToString();
                bgrec.about = row[BudgrpTblInfo.ColIdx.about.ToField()].ToString();
                break;
            }
            tc.budgrpRec = bgrec;

            var trntb = appConfig.s_config.GetTable(TableIdx.Training);
            var trnsb = new SearchBuilder(trntb, m_cp);
            trnsb.Clear();
            trnsb.Add(TrainingTblInfo.ColIdx.bgrp.ToField(), grpId);
            trnsb.Search();
            tc.trngCols = new List<string> {
                TrainingTblInfo.ColIdx.date.ToAlias(),
                TrainingTblInfo.ColIdx.topic.ToAlias(),
                TrainingTblInfo.ColIdx.trnr.ToAlias(),
                TrainingTblInfo.ColIdx.cmnt.ToAlias(),
                TrainingTblInfo.ColIdx.star.ToAlias(),
            };
            tc.recs = new List<TrngRec>();
            foreach (DataRow row in trnsb.dc.m_dataTable.Rows)
            {
                var trnrec = new TrngRec();
                DateTime dateTime = (DateTime)row[TrainingTblInfo.ColIdx.date.ToField()];
                trnrec.date = dateTime.ToString(lConfigMng.GetDisplayDateFormat());
                trnrec.topic = row[TrainingTblInfo.ColIdx.topic.ToField()].ToString();
                TrainingTblInfo.Trainer trainer = (TrainingTblInfo.Trainer)int.Parse(row[TrainingTblInfo.ColIdx.trnr.ToField()].ToString());
                trnrec.trainer = trainer.ToDesc();
                trnrec.cmnt = row[TrainingTblInfo.ColIdx.cmnt.ToField()].ToString();
                TrainingTblInfo.Star star = (TrainingTblInfo.Star)int.Parse(row[TrainingTblInfo.ColIdx.star.ToField()].ToString());
                trnrec.star = star.ToDesc();
                tc.recs.Add(trnrec);
            }
            return tc;
        }

        public List<BudgrpRec> QryBudgrps()
        {
            var dc = MngForm.s_contentProvider.CreateDataContent(TableIdx.Budgrp);
            dc.Search(new List<string>(), new List<SearchParam>());
            var lst = new List<BudgrpRec>();
            foreach (DataRow row in dc.m_dataTable.Rows)
            {
                var rec = new BudgrpRec
                {
                    numb = row[(int)BudgrpTblInfo.ColIdx.grp].ToString(),
                    name = row[(int)BudgrpTblInfo.ColIdx.name].ToString()
                };
                lst.Add(rec);
            }
            return lst;
        }
    }
    public class TaskPageContent : PageContent
    {
        public TaskPageContent()
        {
#if !CFG_MNG_ANY
            m_tmpl = @"..\..\..\main\TaskTmpl.html";
#else
            m_tmpl = @"..\..\main\TaskTmpl.html";
#endif
        }
        public override string GenHtmlContent(List<string> args)
        {
            var secLst = args;
            var tc = QryTabContent(secLst);
            var knownTypes = new Type[] {
                    typeof(TabContent),
                    typeof(DayTask),
                    typeof(TaskRec),
                    typeof(PlanRec),
                };
            var jsTxt = Obj2Json(tc, knownTypes);
            var htmlTxt = GenHtml(jsTxt);
            return htmlTxt;
        }
        private SearchBuilder m_taskSB;
        private SearchBuilder m_grpSB;
        public List<string> QryGrps()
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
        private TabContent QryTabContent(List<string> secLst)
        {
            //List<DateTime> dtLst = new List<DateTime> {new DateTime(2019,6,15), new DateTime(2019, 6, 16) };
            List<DateTime> dtLst = new List<DateTime>();
            {
                var dt = DateTime.Now;
                var wd = dt.DayOfWeek;
                for (int i = 1; i < 8; i++)
                {
                    dtLst.Add(dt.AddDays(i - (int)wd));
                }
            }

            var tc = new TabContent();
            tc.planCols = new List<string> { "Kế hoạch", "Ban", "Tình trạng" };
            tc.taskCols = new List<string> { "Công việc", "Ban", "Tình trạng" };
            tc.recs = new List<DayTask>();
            //qry task
            foreach (DateTime dt in dtLst)
            {
                List<TaskRec> tasks = QryTask(dt, secLst);
                var rec = new DayTask
                {
                    date = dt.ToString(lConfigMng.GetDisplayDateFormat()),
                    tasks = tasks
                };
                tc.recs.Add(rec);
            }
            return tc;
        }
        private List<TaskRec> QryTask(DateTime startDt, List<string> sectionLst)
        {
            var lst = new List<TaskRec>();
            if (m_taskSB == null) { m_taskSB = new SearchBuilder(appConfig.s_config.GetTable(TableIdx.Task), MngForm.s_contentProvider); }
            m_taskSB.Clear();
            m_taskSB.Add(TaskTblInfo.ColIdx.Begin.ToField(), startDt);
            if (sectionLst != null)
            {
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
    }

    public class DocContent : PageContent
    {
        public DocContent()
        {
#if !CFG_MNG_ANY
            m_tmpl = @"..\..\..\main\DocTmpl.html";
#else
            m_tmpl = @"..\..\main\DocTmpl.html";
#endif
        }

        [DataContract]
        public class DocRec
        {
            [DataMember] public string title;
            [DataMember] public string topic;
            [DataMember] public string link;
            [DataMember] public string note;
        }


        [DataContract]
        public class TabContent
        {
            [DataMember] public List<string> docCols;
            [DataMember] public List<DocRec> recs;
        }

        public override string GenHtmlContent(List<string> args)
        {
            var topic = args[0];
            var tc = QryTabContent(topic);
            var knownTypes = new Type[] {
                    typeof(TabContent),
                    typeof(DocRec),
                };
            var jsTxt = Obj2Json(tc, knownTypes);
            var htmlTxt = GenHtml(jsTxt);
            return htmlTxt;
        }

        private object QryTabContent(string topic)
        {
            var tc = new TabContent();

            var tbl = appConfig.s_config.GetTable(TableIdx.Document);
            var sb = new SearchBuilder(tbl, m_cp);
            sb.Clear();
            sb.Add(DocumentTblInfo.ColIdx.topic.ToField(), topic);
            sb.Search();
            tc.docCols = new List<string> {
                DocumentTblInfo.ColIdx.title.ToAlias(),
                DocumentTblInfo.ColIdx.topic.ToAlias(),
                DocumentTblInfo.ColIdx.link.ToAlias(),
                DocumentTblInfo.ColIdx.note.ToAlias(),
            };
            tc.recs = new List<DocRec>();
            foreach (DataRow row in sb.dc.m_dataTable.Rows)
            {
                var rec = new DocRec();
                rec.title = row[DocumentTblInfo.ColIdx.title.ToField()].ToString();
                rec.topic = row[DocumentTblInfo.ColIdx.topic.ToField()].ToString();
                rec.link = row[DocumentTblInfo.ColIdx.link.ToField()].ToString();
                rec.note = row[DocumentTblInfo.ColIdx.note.ToField()].ToString();
                tc.recs.Add(rec);
            }
            return tc;
        }

        public List<string> QryTopics()
        {
            var dc = m_cp.CreateDataContent(TableIdx.Topic);
            dc.Search(new List<string>(), new List<SearchParam>());
            var lst = new List<string>();
            foreach (DataRow row in dc.m_dataTable.Rows)
            {
                var rec = row[TopicTblInfo.ColIdx.topic.ToField()].ToString();
                lst.Add(rec);
            }
            return lst;
        }
    }
}
