#define col_class
#define crt_tables
#define init_datatable_cols
#define use_cmd_params
#define use_single_qry
#define use_progress_dlg
#define use_sqlite
//#define use_bg_work

using System.Windows.Forms;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System;
#if use_sqlite
using System.Data.SQLite;
#endif
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Data.Common;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace test_binding
{
    /// <summary>
    /// table info
    /// + fields
    /// + fileds type
    /// + alias
    /// </summary>
    [DataContract(Name = "TableInfo")]
    public class TableInfo
    {
        //#define col_class
#if col_class
        [DataContract(Name = "ColInfo")]
        public class ColInfo
        {
            [DataContract(Name = "ColType")]
            public enum ColType
            {
                [EnumMember]
                text,
                [EnumMember]
                dateTime,
                [EnumMember]
                num,
                [EnumMember]
                currency,
                [EnumMember]
                uniq,
                [EnumMember]
                map,
            };
            [DataMember(Name = "field", EmitDefaultValue = false)]
            public string m_field;
            [DataMember(Name = "alias", EmitDefaultValue = false)]
            public string m_alias;
            [DataMember(Name = "lookupTbl", EmitDefaultValue = false)]
            public string m_lookupTbl;
            [DataMember(Name = "type", EmitDefaultValue = false)]
            public ColType m_type;
            [DataMember(Name = "visible", EmitDefaultValue = false)]
            public bool m_visible;
            [DataMember(Name = "lst", EmitDefaultValue = false)]
            public string m_lst;        //"idx,val;" ???

            public lDataSync m_lookupData;
            public void Init(string field, string alias, ColType type, string lookupTbl = null, bool visible = true, string lst = null)
            {
                m_lookupTbl = lookupTbl;
                m_field = field;
                m_alias = alias;
                m_type = type;
                m_visible = visible;
                m_lst = lst;
            }
            public ColInfo(string field, string alias, ColType type, string lookupTbl, bool visible)
            {
                Init(field, alias, type, lookupTbl, visible);
            }

            public ColInfo(string field, string alias, ColType type, string param)
            {
                switch (type)
                {
                    case ColType.map:
                        Init(field, alias, type, null, true, param);
                        break;
                    default:
                        Debug.Assert(type == ColType.text);
                        Init(field, alias, type, param);
                        break;
                }
            }
            public ColInfo(string field, string alias, ColType type, bool visible = true)
            {
                Init(field, alias, type, null, visible);
            }

            public string GetHelp()
            {
                if (m_dict == null) { InitDict(); }
                string txt = string.Format("Please input number from 0 to {0}", m_dict.Count-1);
                for (int i = 0; i < m_dict.Count;i++)
                {
                    txt = txt + "\n" + string.Format("  {0} ({1})", i, m_dict.Keys.ElementAt(i));
                }
                return txt;
            }

            Dictionary<string, int> m_dict;
            public Dictionary<string,int> GetDict()
            {
                if (m_dict == null) { InitDict(); }
                return m_dict;
            }
            public bool ParseEnum(int n, out string txt)
            {
                if (m_dict == null) { InitDict(); }
                bool ret = (n < m_dict.Count);
                txt = null;
                if (ret) { txt = m_dict.Keys.ElementAt(n); }
                return ret;
            }
            private void InitDict()
            {
                    m_dict = new Dictionary<string, int>();
                    var arr = m_lst.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < arr.Length;i++)
                    {
                        m_dict.Add(arr[i], i);
                    }
            }
            public bool ParseEnum(string txt, out int n )
            {
                if (m_dict == null) {InitDict();}
                bool ret = m_dict.ContainsKey(txt);
                n = -1;
                if (ret) { n = m_dict[txt]; }
                return ret;
            }
        };

        [DataMember(Name = "cols", EmitDefaultValue = false)]
        public ColInfo[] m_cols;
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string m_tblName;
        [DataMember(Name = "alias", EmitDefaultValue = false)]
        public string m_tblAlias;
        [DataMember(Name = "crtSql", EmitDefaultValue = false)]
        public string m_crtQry;

        public virtual void LoadData()
        {
            foreach (ColInfo colInfo in m_cols)
            {
                if (colInfo.m_lookupTbl != null)
                {
                    //use columns[1] - zero base
                    colInfo.m_lookupData = appConfig.s_contentProvider.CreateDataSync(colInfo.m_lookupTbl);
                    colInfo.m_lookupData.LoadData();
                }
            }
        }
#else
    public struct lColInfo
    {
        public enum lColType
        {
            text,
            dateTime,
            num
        };
        public string m_field;
        public string m_alias;
        public lColType m_type;
    };
    public virtual lColInfo[] getColsInfo() { return null; }
#endif
        public int getColIndex(string colName)
        {
            int i = 0;
            foreach (ColInfo col in m_cols)
            {
                if (col.m_field == colName)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        protected void CrtCols(object[][] map, int n)
        {
            m_cols = new ColInfo[n];
            for (int i = 0; i < map.Length; i++)
            {
                int iCol = (int)map[i][0];
                string field = (string)map[i][1];
                string alias = (string)map[i][2];
                ColInfo.ColType type = (ColInfo.ColType)map[i][3];
                string lookupTbl = (string)map[i][4];
                bool visible = (bool)map[i][5];
                m_cols[iCol] = new ColInfo(field, alias, type, lookupTbl, visible);
            }
        }

        public static string GetDescLst<Tenum>()
        {
            string txt = "";
            var type = typeof(Tenum);
            var arr = Enum.GetValues(type);

            foreach (var v in arr)
            {
                var memberInfo = type.GetMember(v.ToString());
                var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                var attr = attributes.Length > 0
                  ? (DescriptionAttribute)attributes[0]
                  : null;
                txt += attr.Description + ";";
            }
            return txt;
        }
    }

    [DataContract(Name = "ReceiptsTblInfo")]
    public class lReceiptsTblInfo : TableInfo
    {
#if col_class
        public lReceiptsTblInfo()
        {
            m_tblName = "receipts";
            m_tblAlias = "Bảng Thu";
            m_crtQry = "CREATE TABLE if not exists  receipts("
            + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
            + "date datetime,"
            + "receipt_number char(31),"
            + "name char(31),"
            + "addr char(63),"
            + "amount INTEGER,"
            + "content text,"
            + "note text"
            + ")";
            m_cols = new ColInfo[] {
                   new ColInfo( "ID","ID", ColInfo.ColType.num, null, false),
                   new ColInfo( "receipt_number","Mã PT", ColInfo.ColType.uniq),
                   new ColInfo( "date","Ngày Tháng", ColInfo.ColType.dateTime),
                   new ColInfo( "name","Họ tên", ColInfo.ColType.text),
                   new ColInfo( "addr","Địa chỉ", ColInfo.ColType.text),
                   new ColInfo( "content","Nội dung", ColInfo.ColType.text, "receipts_content"),
                   new ColInfo( "note","Ghi chú", ColInfo.ColType.text),
                   new ColInfo( "amount","Số tiền", ColInfo.ColType.currency),
                };
        }
#else
    static lColInfo[] m_cols = new lColInfo[] {
               new lColInfo() {m_field = "ID", m_alias = "ID", m_type = lColInfo.lColType.num }
            };
    public override lColInfo[] getColsInfo()
    {
        return m_cols;
    }
#endif

    };
    [DataContract(Name = "InternalPaymentTblInfo")]
    public class lInternalPaymentTblInfo : TableInfo
    {
        public lInternalPaymentTblInfo()
        {
            m_tblName = "internal_payment";
            m_tblAlias = "Chi Nội Chúng";
            m_crtQry = "CREATE TABLE if not exists internal_payment("
            + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
            + "date datetime,"
            + "payment_number char(31),"
            + "name char(31),"
            + "addr char(63),"
            + "group_name char(31),"
            + "advance_payment INTEGER,"
            //+ "reimbursement INTEGER,"
            + "actually_spent INTEGER,"
            + "status INTEGER,"
            + "content text,"
            + "note text"
            + ")";
            m_cols = new ColInfo[] {
                   new ColInfo( "ID"               ,"ID"           , ColInfo.ColType.num, null, false),
                   new ColInfo( "payment_number"   ,"Mã Phiếu Chi" , ColInfo.ColType.uniq),
                   new ColInfo( "date"             ,"Ngày Tháng"   , ColInfo.ColType.dateTime),
                   new ColInfo( "name"             ,"Họ Tên"       , ColInfo.ColType.text),
                   new ColInfo( "addr"             ,"Địa chỉ"      , ColInfo.ColType.text),
                   new ColInfo( "group_name"       ,"Thuộc ban"    , ColInfo.ColType.text, "group_name"),
                   new ColInfo( "content"          ,"Nội dung"     , ColInfo.ColType.text),
                   new ColInfo( "advance_payment"  ,"Tạm ứng"      , ColInfo.ColType.currency),
                   //new lColInfo( "reimbursement"    ,"Hoàn ứng"     , lColInfo.lColType.currency, null, false),
                   new ColInfo( "actually_spent"   ,"Thực chi"     , ColInfo.ColType.currency),
                   new ColInfo( "status"           ,"Trạng thái"   , ColInfo.ColType.map),
                   new ColInfo( "note"             ,"Ghi Chú"      , ColInfo.ColType.text),
                };
        }
    };
    [DataContract(Name = "ExternalPaymentTblInfo")]
    public class lExternalPaymentTblInfo : TableInfo
    {
        public lExternalPaymentTblInfo()
        {
            m_tblName = "external_payment";
            m_tblAlias = "Chi Ngoại Chúng";
            m_crtQry = "CREATE TABLE if not exists external_payment("
            + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
            + "date datetime,"
            + "payment_number char(31),"
            + "building char(31),"
            + "group_name char(31),"
            + "constr_org char(31),"
            + "name char(31),"
            + "addr char(63),"
            + "spent INTEGER,"
            + "content text,"
            + "note text"
            + ")";
            m_cols = new ColInfo[] {
                   new ColInfo( "ID","ID", ColInfo.ColType.num, null, false),
                   new ColInfo( "payment_number"   ,"Mã Phiếu Chi", ColInfo.ColType.uniq),
                   new ColInfo( "date"             ,"Ngày Tháng", ColInfo.ColType.dateTime),
                   new ColInfo( "name"             ,"Họ Tên", ColInfo.ColType.text),
                   new ColInfo( "addr"             ,"Địa chỉ", ColInfo.ColType.text),
                   new ColInfo( "content"          ,"Nội dung", ColInfo.ColType.text),
                   new ColInfo( "constr_org"       ,"Đơn vị TC", ColInfo.ColType.text, "constr_org"),
                   new ColInfo( "building"         ,"Công trình", ColInfo.ColType.text, "building"),
                   new ColInfo( "group_name"       ,"Thuộc ban", ColInfo.ColType.text, "group_name"),
                   new ColInfo( "spent"            ,"Số tiền", ColInfo.ColType.currency),
                   new ColInfo( "note"             ,"Ghi Chú", ColInfo.ColType.text),
                };
        }
    };
    [DataContract(Name = "SalaryTblInfo")]
    public class lSalaryTblInfo : TableInfo
    {
        public lSalaryTblInfo()
        {
            m_tblName = "salary";
            m_tblAlias = "Bảng Lương";
            m_crtQry = "CREATE TABLE if not exists salary("
            + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
            + "month INTEGER,"
            + "date datetime,"
            + "payment_number char(31),"
            + "name char(31),"
            + "addr char(63),"
            + "group_name char(31),"
            + "bsalary INTEGER,"
            + "esalary INTEGER,"
            + "salary INTEGER,"
            + "content text,"
            + "note text"
            + ")";
            m_cols = new ColInfo[] {
                   new ColInfo( "ID","ID", ColInfo.ColType.num, null, false),
                   new ColInfo( "payment_number"   ,"Mã Phiếu Chi", ColInfo.ColType.uniq),
                   new ColInfo( "month"            ,"Tháng(1...12)", ColInfo.ColType.num, null, false),
                   new ColInfo( "date"             ,"Ngày Tháng", ColInfo.ColType.dateTime),
                   new ColInfo( "name"             ,"Họ Tên", ColInfo.ColType.text),
                   new ColInfo( "addr"             ,"Địa chỉ", ColInfo.ColType.text),
                   new ColInfo( "group_name"       ,"Thuộc ban", ColInfo.ColType.text, "group_name"),
                   new ColInfo( "content"          ,"Nội dung", ColInfo.ColType.text),
                   new ColInfo( "bsalary"           ,"Lương CB", ColInfo.ColType.currency),
                   new ColInfo( "esalary"           ,"Lương TN", ColInfo.ColType.currency),
                   new ColInfo( "salary"           ,"Tổng lương", ColInfo.ColType.currency),
                   new ColInfo( "note"             ,"Ghi Chú", ColInfo.ColType.text),
                };
        }
    };
    [DataContract(Name = "lGroupNameTblInfo")]
    public class GroupNameTblInfo : TableInfo
    {
        public GroupNameTblInfo()
        {
            m_tblName = "group_name";
            m_tblAlias = "Các ban";
            m_crtQry = "CREATE TABLE if not exists group_name("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT, "
                + "name nchar(31))";
            m_cols = new ColInfo[] {
                   new ColInfo( "ID","ID", ColInfo.ColType.num, null, false),
                   new ColInfo( "name","Các ban", ColInfo.ColType.text)
                };
        }
    };
    [DataContract(Name = "lBuildingTblInfo")]
    public class lBuildingTblInfo : TableInfo
    {
        public lBuildingTblInfo()
        {
            m_tblName = "building";
            m_tblAlias = "Công trình";
            m_crtQry = "CREATE TABLE if not exists building("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT, "
                + "name nchar(31))";
            m_cols = new ColInfo[] {
                   new ColInfo( "ID","ID", ColInfo.ColType.num, null, false),
                   new ColInfo( "name","Công trình", ColInfo.ColType.text)
                };
        }
    };
    [DataContract(Name = "lConstrorgTblInfo")]
    public class lConstrorgTblInfo : TableInfo
    {
        public lConstrorgTblInfo()
        {
            m_tblName = "constr_org";
            m_tblAlias = "Đơn vị TC";
            m_crtQry = "CREATE TABLE if not exists constr_org("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT, "
                + "name nchar(31))";
            m_cols = new ColInfo[] {
                   new ColInfo( "ID","ID", ColInfo.ColType.num, null, false),
                   new ColInfo( "name","Đơn vị TC", ColInfo.ColType.text)
                };
        }
    };
    [DataContract(Name = "lReceiptsContentTblInfo")]
    public class lReceiptsContentTblInfo : TableInfo
    {
        public lReceiptsContentTblInfo()
        {
            m_tblName = "receipts_content";
            m_tblAlias = "Ngồn thu";
            m_crtQry = "CREATE TABLE if not exists receipts_content("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                + " content nchar(31))";
            m_cols = new ColInfo[] {
                   new ColInfo( "ID","ID", ColInfo.ColType.num, null, false),
                   new ColInfo( "content","Nguồn thu", ColInfo.ColType.text)
                };
        }
    };

    public enum TableIdx
    {
        [Description("task")        ] Task,
        [Description("order_tbl")   ] Order,
        [Description("human")       ] Human,
        [Description("equipment")   ] Equip,
        [Description("car")         ] Car,
        [Description("order_human") ] HumanOR,
        [Description("order_equipment")] EquipOR,
        [Description("order_car")   ] CarOR,

        Count
    }
    public enum TaskStatus
    {
        [Description("Triển khai") ] Doing,
        [Description("Hoàn thành") ] Done,
    }
    [DataContract(Name = "lTaskTblInfo")]
    public class TaskTblInfo : TableInfo
    {
        public enum ColIdx
        {
            [Field("ID")            ,Alias("ID"           )] ID,
            [Field("task_number")   ,Alias("Mã CV"        )] Task,
            [Field("group_name")    ,Alias("Thuộc ban"    )] Group,
            [Field("task_name")     ,Alias("Tên CV"       )] Name,
            [Field("start_date")    ,Alias("Ngày bắt đầu" )] Begin,
            [Field("end_date")      ,Alias("Ngày kết thúc")] End,
            [Field("task_status")   ,Alias("Trạng thái")   ] Stat,
            [Field("note")          ,Alias("Ghi Chú")      ] Note,

            Count
        }

        public TaskTblInfo()
        {
            m_tblName = "task";
            m_tblAlias = "Công việc";
            m_crtQry = "CREATE TABLE if not exists task("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                + "task_number char(31),"
                + "group_name char(31),"
                + "task_name char(31),"
                + "start_date datetime,"
                + "end_date datetime,"
                + "task_status INTEGER,"
                + "note text)";
            m_cols = new ColInfo[(int)ColIdx.Count];
            m_cols[(int)ColIdx.ID   ] = new ColInfo(ColIdx.ID.ToField()    , ColIdx.ID.ToAlias()    , ColInfo.ColType.num, false);
            m_cols[(int)ColIdx.Task ] = new ColInfo(ColIdx.Task.ToField()  , ColIdx.Task.ToAlias()  , ColInfo.ColType.uniq);
            m_cols[(int)ColIdx.Group] = new ColInfo(ColIdx.Group.ToField() , ColIdx.Group.ToAlias() , ColInfo.ColType.text, "group_name" );
            m_cols[(int)ColIdx.Name ] = new ColInfo(ColIdx.Name.ToField()  , ColIdx.Name.ToAlias()  , ColInfo.ColType.text);
            m_cols[(int)ColIdx.Begin] = new ColInfo(ColIdx.Begin.ToField() , ColIdx.Begin.ToAlias() , ColInfo.ColType.dateTime);
            m_cols[(int)ColIdx.End  ] = new ColInfo(ColIdx.End.ToField()   , ColIdx.End.ToAlias()   , ColInfo.ColType.dateTime);
            m_cols[(int)ColIdx.Stat] = new ColInfo(ColIdx.Stat.ToField(), ColIdx.Stat.ToAlias(), ColInfo.ColType.map, GetDescLst<TaskStatus>());
            m_cols[(int)ColIdx.Note ] = new ColInfo(ColIdx.Note.ToField()  , ColIdx.Note.ToAlias()  , ColInfo.ColType.text);
        }
    };
    [DataContract(Name = "lOrderTblInfo")]
    public class OrderTblInfo : TableInfo
    {
        public enum ColIdx
        {
            [Field("ID")           , Alias( "ID"        ) ] ID,
            [Field("task_number")  , Alias( "Mã YC"     ) ] Task,
            [Field("order_number") , Alias( "Mã CV"     ) ] Order,
            [Field("order_type")   , Alias( "Loại YC"   ) ] Type,
            [Field("number")       , Alias( "Số lượng"  ) ] Amnt,
            [Field("order_status") , Alias( "Trạng thái") ] Stat,
            [Field("note")         , Alias("Ghi Chú"    ) ] Note,

            Count
        }

        public OrderTblInfo()
        {
            m_tblName = "order_tbl";
            m_tblAlias = "Yêu Cầu";
            m_crtQry = "CREATE TABLE if not exists order_tbl("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                + "task_number char(31),"
                + "order_number char(31),"
                + "order_type INTEGER,"
                + "number INTEGER,"
                + "order_status INTEGER,"
                + "note text)";
            m_cols = new ColInfo[(int)ColIdx.Count];  
            m_cols[(int)ColIdx.ID   ] = new ColInfo(ColIdx.ID.ToField()    , ColIdx.ID.ToAlias()    , ColInfo.ColType.num , false);
            m_cols[(int)ColIdx.Order] = new ColInfo(ColIdx.Order.ToField() , ColIdx.Order.ToAlias() , ColInfo.ColType.uniq);
            m_cols[(int)ColIdx.Task ] = new ColInfo(ColIdx.Task.ToField()  , ColIdx.Task.ToAlias()  , ColInfo.ColType.text, "task");  //columns[1]
            m_cols[(int)ColIdx.Type ] = new ColInfo(ColIdx.Type.ToField()  , ColIdx.Type.ToAlias()  , ColInfo.ColType.map, GetDescLst<OrderType>());
            m_cols[(int)ColIdx.Amnt ] = new ColInfo(ColIdx.Amnt.ToField(), ColIdx.Amnt.ToAlias(), ColInfo.ColType.num);
            m_cols[(int)ColIdx.Stat ] = new ColInfo(ColIdx.Stat.ToField()  , ColIdx.Stat.ToAlias()  , ColInfo.ColType.map, GetDescLst<OrderStatus>());
            m_cols[(int)ColIdx.Note ] = new ColInfo(ColIdx.Note.ToField()  , ColIdx.Note.ToAlias()  , ColInfo.ColType.text);
        }
    };
    [DataContract(Name = "lHumanTblInfo")]
    public class HumanTblInfo : TableInfo
    {
        public enum ColIdx
        {
            [Field("ID")            , Alias("ID")]          ID,
            [Field("human_number")  , Alias("Mã NS")]       Human,
            [Field("name")          , Alias("Họ tên")]      Name,
            [Field("start_date")    , Alias("Ngày vào")]    Enter,
            [Field("end_date")      , Alias("Ngày ra")]     Leave,
            [Field("gender")        , Alias("Giới tính")]   Gndr,
            [Field("age")           , Alias("Tuổi")]        Age,
            [Field("status")        , Alias("Đang bận")]    Busy,
            [Field("note")          , Alias("Ghi Chú")]     Note,

            Count
        }

        public HumanTblInfo()
        {
            m_tblName = "human";
            m_tblAlias = "Nhân Sự";
            m_crtQry = "CREATE TABLE if not exists human("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                + "human_number char(31),"
                + "name char(31),"
                + "start_date datetime,"
                + "end_date datetime,"
                + "gender INTEGER,"
                + "age INTEGER,"
                + "status INTEGER,"
                + "note text)";
            m_cols = new ColInfo[(int)ColIdx.Count];
            m_cols[(int)ColIdx.ID]      = new ColInfo(ColIdx.ID.ToField(), ColIdx.ID.ToAlias(), ColInfo.ColType.num, false);
            m_cols[(int)ColIdx.Human]   = new ColInfo(ColIdx.Human.ToField(), ColIdx.Human.ToAlias(), ColInfo.ColType.uniq);
            m_cols[(int)ColIdx.Name]    = new ColInfo(ColIdx.Name.ToField(), ColIdx.Name.ToAlias(), ColInfo.ColType.text);
            m_cols[(int)ColIdx.Enter]   = new ColInfo(ColIdx.Enter.ToField(), ColIdx.Enter.ToAlias(), ColInfo.ColType.dateTime);
            m_cols[(int)ColIdx.Leave]   = new ColInfo(ColIdx.Leave.ToField() ,ColIdx.Leave.ToAlias(), ColInfo.ColType.dateTime);
            m_cols[(int)ColIdx.Gndr]    = new ColInfo(ColIdx.Gndr.ToField() ,ColIdx.Gndr.ToAlias(), ColInfo.ColType.map, GetDescLst<Gender>());
            m_cols[(int)ColIdx.Age]     = new ColInfo(ColIdx.Age.ToField() ,ColIdx.Age.ToAlias(), ColInfo.ColType.num);
            m_cols[(int)ColIdx.Busy]    = new ColInfo(ColIdx.Busy.ToField() ,ColIdx.Busy.ToAlias(), ColInfo.ColType.map, GetDescLst<ResStatus>());
            m_cols[(int)ColIdx.Note]    = new ColInfo(ColIdx.Note.ToField(), ColIdx.Note.ToAlias(), ColInfo.ColType.text);
        }
    };
    [DataContract(Name = "lEquipmentTblInfo")]
    public class EquipmentTblInfo : TableInfo
    {
        public enum ColIdx
        {
            [Field("ID")                , Alias("ID")]              ID,
            [Field("equipment_number")  , Alias("Mã TB")]           Eqpt,
            [Field("equipment_type")    , Alias("Loại TB")]         Type,
            [Field("inuse")             , Alias("Đang sử dụng")]    Used,
            [Field("note")              , Alias("Ghi chú")]         Note,

            Count
        }
        public EquipmentTblInfo()
        {
            m_tblName = "equipment";
            m_tblAlias = "Thiết Bị";
            m_crtQry = "CREATE TABLE if not exists equipment("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                + "equipment_number char(31),"
                + "equipment_type char(31),"
                + "inuse INTEGER,"
                + "note text)";
            m_cols = new ColInfo[(int)ColIdx.Count];
            m_cols[(int)ColIdx.ID] = new ColInfo(ColIdx.ID.ToField(), ColIdx.ID.ToAlias(), ColInfo.ColType.num, false);
            m_cols[(int)ColIdx.Eqpt] = new ColInfo(ColIdx.Eqpt.ToField(), ColIdx.Eqpt.ToAlias(), ColInfo.ColType.uniq);
            m_cols[(int)ColIdx.Type] = new ColInfo(ColIdx.Type.ToField() ,ColIdx.Type.ToAlias(), ColInfo.ColType.text);
            m_cols[(int)ColIdx.Used] = new ColInfo(ColIdx.Used.ToField(), ColIdx.Used.ToAlias(), ColInfo.ColType.map, GetDescLst<ResStatus>());
            m_cols[(int)ColIdx.Note] = new ColInfo(ColIdx.Note.ToField(), ColIdx.Note.ToAlias(), ColInfo.ColType.text);
        }
    };
    [DataContract(Name = "CarTblInfo")]
    public class CarTblInfo : TableInfo
    {
        public enum ColIdx
        {
            [Field("ID")        , Alias("ID")       ] ID,
            [Field("car_number"), Alias("Biển số")  ] Car,
            [Field("car_type")  , Alias("Loại xe")  ] Type,
            [Field("brand")     , Alias("Hãng SX")  ] Brand,
            [Field("inuse")     , Alias("Đang sử dụng")] Used,
            [Field("note")      , Alias("Ghi chú")  ] Note,

            Count
        }
        public CarTblInfo()
        {
            m_tblName = "car";
            m_tblAlias = "Phương Tiện";
            m_crtQry = "CREATE TABLE if not exists car("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                + "car_number char(31),"
                + "car_type char(31),"
                + "brand char(31),"
                + "inuse INTEGER,"
                + "note text)";
            m_cols = new ColInfo[(int)ColIdx.Count];
            m_cols[(int)ColIdx.ID] = new ColInfo(ColIdx.ID.ToField(), ColIdx.ID.ToAlias(), ColInfo.ColType.num, false);
            m_cols[(int)ColIdx.Car] = new ColInfo(ColIdx.Car.ToField(), ColIdx.Car.ToAlias(), ColInfo.ColType.uniq);
            m_cols[(int)ColIdx.Type] = new ColInfo(ColIdx.Type.ToField(), ColIdx.Type.ToAlias(), ColInfo.ColType.text);
            m_cols[(int)ColIdx.Brand] = new ColInfo(ColIdx.Brand.ToField(), ColIdx.Brand.ToAlias(), ColInfo.ColType.text);
            m_cols[(int)ColIdx.Used] = new ColInfo(ColIdx.Used.ToField(), ColIdx.Used.ToAlias(), ColInfo.ColType.map, GetDescLst<ResStatus>());
            m_cols[(int)ColIdx.Note] = new ColInfo(ColIdx.Note.ToField(), ColIdx.Note.ToAlias(), ColInfo.ColType.text);
        }
    };
    [DataContract(Name = "lOrderEquipmentTblInfo")]
    public class OrderEquipmentTblInfo : TableInfo
    {
        public enum ColIdx
        {
            [Field("ID")            , Alias("ID")       ] ID,
            [Field("order_number")  , Alias("Mã YC")    ] Order,
            [Field("equipment_number"), Alias("Mã TB")  ] Equip,
            [Field("task_number")   , Alias("Mã CV")    ] Task,
            [Field("note")          , Alias("Ghi Chú")  ] Note,

            Count
        }
        public OrderEquipmentTblInfo()
        {
            m_tblName = "order_equipment";
            m_tblAlias = "Yêu Cầu - Thiết Bị";
            m_crtQry = "CREATE TABLE if not exists order_equipment("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                + "order_number char(31),"
                + "equipment_number char(31),"
                + "task_number char(31),"
                + "note text)";
            m_cols = new ColInfo[(int)ColIdx.Count];
            m_cols[(int)ColIdx.ID] = new ColInfo(ColIdx.ID.ToField(), ColIdx.ID.ToAlias(), ColInfo.ColType.num, false);
            m_cols[(int)ColIdx.Order] = new ColInfo(ColIdx.Order.ToField(), ColIdx.Order.ToAlias(), ColInfo.ColType.uniq);
            m_cols[(int)ColIdx.Equip] = new ColInfo(ColIdx.Equip.ToField(), ColIdx.Equip.ToAlias(), ColInfo.ColType.uniq);
            m_cols[(int)ColIdx.Task] = new ColInfo(ColIdx.Task.ToField(), ColIdx.Task.ToAlias(), ColInfo.ColType.uniq);
            m_cols[(int)ColIdx.Note] = new ColInfo(ColIdx.Note.ToField(), ColIdx.Note.ToAlias(), ColInfo.ColType.text);
        }
    }
    [DataContract(Name = "lOrderHumanTblInfo")]
    public class OrderHumanTblInfo : TableInfo
    {
        public enum ColIdx
        {
            [Field("ID"), Alias("ID")] ID,
            [Field("order_number"), Alias("Mã YC")] Order,
            [Field("human_number"), Alias("Mã NS")] Human,
            [Field("task_number"), Alias("Mã CV")] Task,
            [Field("note"), Alias("Ghi Chú")] Note,

            Count
        }
        public OrderHumanTblInfo()
        {
            m_tblName = "order_human";
            m_tblAlias = "Yêu Cầu - Nhân Sự";
            m_crtQry = "CREATE TABLE if not exists order_human("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                + "order_number char(31),"
                + "human_number char(31),"
                + "task_number char(31),"
                + "note text)";
            m_cols = new ColInfo[(int)ColIdx.Count];
            m_cols[(int)ColIdx.ID] = new ColInfo(ColIdx.ID.ToField(), ColIdx.ID.ToAlias(), ColInfo.ColType.num, false);
            m_cols[(int)ColIdx.Order] = new ColInfo(ColIdx.Order.ToField(), ColIdx.Order.ToAlias(), ColInfo.ColType.text);
            m_cols[(int)ColIdx.Human] = new ColInfo(ColIdx.Human.ToField(), ColIdx.Human.ToAlias(), ColInfo.ColType.text);
            m_cols[(int)ColIdx.Task] = new ColInfo(ColIdx.Task.ToField(), ColIdx.Task.ToAlias(), ColInfo.ColType.text);
            m_cols[(int)ColIdx.Note] = new ColInfo(ColIdx.Note.ToField(), ColIdx.Note.ToAlias(), ColInfo.ColType.text);
        }
    };
    [DataContract(Name = "OrderCarTblInfo")]
    public class OrderCarTblInfo : TableInfo
    {
        public enum ColIdx
        {
            [Field("ID"), Alias("ID")] ID,
            [Field("order_number"), Alias("Mã YC")] Order,
            [Field("car_number"), Alias("Biển số")] Car,
            [Field("task_number"), Alias("Mã CV")] Task,
            [Field("note"), Alias("Ghi chú")] Note,

            Count
        }
        public OrderCarTblInfo()
        {
            m_tblName = "order_car";
            m_tblAlias = "Yêu Cầu - Phương Tiện";
            m_crtQry = "CREATE TABLE if not exists order_car("
                + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                + "order_number char(31),"
                + "car_number char(31),"
                + "note text)";
            m_cols = new ColInfo[(int)ColIdx.Count];
            m_cols[(int)ColIdx.ID] = new ColInfo(ColIdx.ID.ToField(), ColIdx.ID.ToAlias(), ColInfo.ColType.num, false);
            m_cols[(int)ColIdx.Order] = new ColInfo(ColIdx.Order.ToField(), ColIdx.Order.ToAlias(), ColInfo.ColType.uniq);
            m_cols[(int)ColIdx.Car] = new ColInfo(ColIdx.Car.ToField(), ColIdx.Car.ToAlias(), ColInfo.ColType.uniq);
            m_cols[(int)ColIdx.Task] = new ColInfo(ColIdx.Task.ToField(), ColIdx.Task.ToAlias(), ColInfo.ColType.uniq);
            m_cols[(int)ColIdx.Note] = new ColInfo(ColIdx.Note.ToField(), ColIdx.Note.ToAlias(), ColInfo.ColType.text);
        }
    };

    [DataContract(Name = "lReceiptsViewInfo")]
    public class lReceiptsViewInfo : TableInfo
    {
        public lReceiptsViewInfo()
        {
            m_tblName = "v_receipts";
            m_crtQry = "CREATE VIEW if not exists v_receipts  as  "
                + "select content, amount/1000 as amount, "
                + "cast(strftime('%Y', date) as integer) as year, "
                + "(strftime('%m', date) + 2) / 3 as qtr "
                + "from receipts "
                + "where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;";
            m_cols = new ColInfo[] {
                   new ColInfo( "content","Nội dung", ColInfo.ColType.text),
                   new ColInfo( "amount","Số tiền", ColInfo.ColType.currency),
                   new ColInfo( "year","Năm", ColInfo.ColType.num),
                   new ColInfo( "qtr","Quý", ColInfo.ColType.num),
                };
        }
    };
    [DataContract(Name = "lInterPaymentViewInfo")]
    public class lInterPaymentViewInfo : TableInfo
    {
        public lInterPaymentViewInfo()
        {
            m_tblName = "v_internal_payment";
            m_crtQry = "CREATE VIEW if not exists v_internal_payment as "
                + "select group_name, actually_spent/1000 as actually_spent, "
                + "cast(strftime('%Y', date) as integer) as year, "
                + "(strftime('%m', date) + 2) / 3 as qtr "
                + "from internal_payment "
                + "where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;";
            m_cols = new ColInfo[] {
                   new ColInfo( "group_name","Ban", ColInfo.ColType.text),
                   new ColInfo( "actually_spent","Thực chi", ColInfo.ColType.currency),
                   new ColInfo( "year","Năm", ColInfo.ColType.num),
                   new ColInfo( "qtr","Quý", ColInfo.ColType.num),
                };
        }
    };
    [DataContract(Name = "lExterPaymentViewInfo")]
    public class lExterPaymentViewInfo : TableInfo
    {
        public lExterPaymentViewInfo()
        {
            m_tblName = "v_external_payment";
            m_crtQry = "CREATE VIEW if not exists v_external_payment as "
                + "select group_name, spent/1000 as spent, "
                + "cast(strftime('%Y', date) as integer) as year, "
                + "(strftime('%m', date) + 2) / 3 as qtr "
                + "from external_payment "
                + "where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;";
            m_cols = new ColInfo[] {
                new ColInfo("group_name", "Ban", ColInfo.ColType.text),
                new ColInfo("spent", "Chi", ColInfo.ColType.currency),
                new ColInfo("year", "Năm", ColInfo.ColType.num),
                new ColInfo("qtr", "Quý", ColInfo.ColType.num),
            };
        }
    };
    [DataContract(Name = "lSalaryViewInfo")]
    public class lSalaryViewInfo : TableInfo
    {
        public lSalaryViewInfo()
        {
            m_tblName = "v_salary";
            m_crtQry = "CREATE VIEW if not exists v_salary as "
                + "select group_name, salary/1000 as salary, "
                + "cast(strftime('%Y', date) as integer) as year, "
                + "(strftime('%m', date) + 2) / 3 as qtr "
                + "from salary "
                + "where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;";
            m_cols = new ColInfo[] {
                new ColInfo("group_name", "Ban", ColInfo.ColType.text),
                new ColInfo("salary", "Lương", ColInfo.ColType.currency),
                new ColInfo("year", "Năm", ColInfo.ColType.num),
                new ColInfo("qtr", "Quý", ColInfo.ColType.num),
            };
        }
    };
    [DataContract(Name = "lAdvanceViewInfo")]
    public class lAdvanceViewInfo : TableInfo
    {
        public lAdvanceViewInfo()
        {
            m_tblName = "v_advance";
            m_crtQry = "CREATE VIEW if not exists v_advance as "
                + "select group_name, actually_spent/1000 as actually_spent, "
                + "cast(strftime('%Y', date) as integer) as year, "
                + "(strftime('%m', date) + 2) / 3 as qtr "
                + "from advance "
                + "where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;";
            m_cols = new ColInfo[] {
                   new ColInfo( "group_name","Ban", ColInfo.ColType.text),
                   new ColInfo( "actually_spent","Thực chi", ColInfo.ColType.currency),
                   new ColInfo( "year","Năm", ColInfo.ColType.num),
                   new ColInfo( "qtr","Quý", ColInfo.ColType.num),
                };
        }
    };
    [DataContract(Name = "lDaysumViewInfo")]
    public class lDaysumViewInfo : TableInfo
    {
        public lDaysumViewInfo()
        {
            m_tblName = "v_day_sum";
            m_crtQry = " SELECT date,"
                    + "        sum(receipt) AS receipt,"
                    + "        sum(interpay1) AS inter_pay1,"
                    + "        sum(interpay2) AS inter_pay2,"
                    + "        sum(exterpay) AS exter_pay,"
                    + "        sum(salary) AS salary,"
                    + "        sum(receipt) - sum(interpay1)- sum(interpay2) - sum(exterpay) - sum(salary) AS remain"
                    + "   FROM ("
                    + "            SELECT date,"
                    + "                   amount AS receipt,"
                    + "                   0 AS interpay1,"
                    + "                   0 AS interpay2,"
                    + "                   0 AS exterpay,"
                    + "                   0 AS salary"
                    + "              FROM receipts"
                    + "            UNION ALL"
                    + "            SELECT date,"
                    + "                   0 AS receipt,"
                    + "                   advance_payment AS interpay1,"
                    + "                   actually_spent as interpay2,"
                    + "                   0 AS exterpay,"
                    + "                   0 AS salary"
                    + "              FROM internal_payment"
                    + "            UNION ALL"
                    + "            SELECT date,"
                    + "                   0 AS receipt,"
                    + "                   0 AS interpay1,"
                    + "                   0 AS interpay2,"
                    + "                   spent AS exterpay,"
                    + "                   0 AS salary"
                    + "              FROM external_payment"
                    + "            UNION ALL"
                    + "            SELECT date,"
                    + "                   0 AS receipt,"
                    + "                   0 AS interpay1,"
                    + "                   0 AS interpay2,"
                    + "                   0 AS exterpay,"
                    + "                   salary"
                    + "              FROM salary"
                    + "        )"
                    + "  GROUP BY date";
            m_cols = new ColInfo[] {
                new ColInfo("date", "Ngày", ColInfo.ColType.text),
                new ColInfo("receipt", "Thu", ColInfo.ColType.currency),
                new ColInfo("interpay", "Chi nội", ColInfo.ColType.currency),
                new ColInfo("exterpay", "Chi ngoại", ColInfo.ColType.currency),
                new ColInfo("salary", "Lương", ColInfo.ColType.currency),
                new ColInfo("sum", "Số dư cuối", ColInfo.ColType.currency),
            };
        }
    };

    public class lContentProvider : IDisposable
    {
        protected lContentProvider()
        {
            m_dataSyncs = new Dictionary<string, lDataSync>();
            m_dataContents = new Dictionary<string, DataContent>();
        }

        protected Form1 m_form;
        private Dictionary<string, lDataSync> m_dataSyncs;
        private Dictionary<string, DataContent> m_dataContents;

        protected virtual DataContent newDataContent(string tblName) { return null; }
        protected virtual lDataSync newDataSync(string tblName)
        {
            DataContent dataContent = CreateDataContent(tblName);
            lDataSync dataSync = new lDataSync(dataContent);
            return dataSync;
        }

        public virtual DataTable GetData(string qry) { return null; }
        public DataContent CreateDataContent(TableIdx tblIdx)
        {
            return CreateDataContent(tblIdx.ToDesc());
        }
        public DataContent CreateDataContent(string tblName)
        {
            if (!m_dataContents.ContainsKey(tblName))
            {
                DataContent data = newDataContent(tblName);
                m_dataContents.Add(tblName, data);
                return data;
            }
            else
            {
                return m_dataContents[tblName];
            }
        }
        public bool ReleaseDataContent(string tblName)
        {
            if (!m_dataContents.ContainsKey(tblName))
            {
                return false;
            }
            else
            {
                DataContent data = m_dataContents[tblName];
                m_dataContents.Remove(tblName);
                data.Dispose();
                return true;
            }
        }
        public lDataSync CreateDataSync(string tblName)
        {
            if (!m_dataSyncs.ContainsKey(tblName))
            {
                DataContent dataContent = CreateDataContent(tblName);
                lDataSync dataSync = new lDataSync(dataContent);
                m_dataSyncs.Add(tblName, dataSync);
                return dataSync;
            }
            else
            {
                return m_dataSyncs[tblName];
            }
        }

        public virtual object GetCnn() { throw new NotImplementedException(); }

#region dispose
        // Dispose() calls Dispose(true)  
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // NOTE: Leave out the finalizer altogether if this class doesn't   
        // own unmanaged resources itself, but leave the other methods  
        // exactly as they are.   
        ~lContentProvider()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)  
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (lDataSync ds in m_dataSyncs.Values)
                {
                    ds.Dispose();
                }
                foreach (DataContent dc in m_dataContents.Values)
                {
                    dc.Dispose();
                }
            }
            // free native resources if there are any.
            m_dataSyncs.Clear();
            m_dataContents.Clear();
        }
#endregion
    }
    public class lSqlContentProvider : lContentProvider
    {
        static lSqlContentProvider m_instance;
        public static lContentProvider getInstance(Form1 parent)
        {
            if (m_instance == null)
            {
                m_instance = new lSqlContentProvider();
                m_instance.m_form = parent;
            }
            return m_instance;
        }

        lSqlContentProvider() : base()
        {
            string cnnStr = appConfig.s_config.m_dbSchema.m_cnnStr;
            m_cnn = new SqlConnection(cnnStr);
            m_cnn.Open();
        }

        private SqlConnection m_cnn;

        protected override DataContent newDataContent(string tblName)
        {
            lSqlDataContent data = new lSqlDataContent(tblName, m_cnn);
#if !use_bg_work
            data.m_form = m_form;
#endif
            return data;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public override DataTable GetData(string qry)
        {
            SqlDataAdapter dataAdapter = new SqlDataAdapter();
            dataAdapter.SelectCommand = new SqlCommand(qry, m_cnn);
            // Populate a new data table and bind it to the BindingSource.
            DataTable table = new DataTable();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            dataAdapter.Fill(table);
            return table;
        }

        public override object GetCnn()
        {
            return m_cnn;
        }

#region dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_cnn.Dispose();
            }
            // free native resources if there are any.
            base.Dispose(disposing);
        }
#endregion
    }

    //status - map - colinfo
    public class lAdvanceStatus
    {
        public const int nAdvance = 0;
        public const int nRemain = 1;
        public const int nActual = 2;
        public const string zAdvance = "Tạm ứng";
        public const string zRemain = "Hoàn ứng";
        public const string zActual = "Thực chi";
        public static List<InputCtrlEnum.ComboItem> lst = new List<InputCtrlEnum.ComboItem> {
                new InputCtrlEnum.ComboItem { name = zAdvance, val = nAdvance },
                new InputCtrlEnum.ComboItem { name = zRemain, val = nRemain },
                new InputCtrlEnum.ComboItem { name = zActual, val = nActual },
            };
    }

    #region enum_attr
    public static class EnumExtensions
    {
        // This extension method is broken out so you can use a similar pattern with 
        // other MetaData elements in the future. This is your base method for each.
        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0
              ? (T)attributes[0]
              : null;
        }

        // This method creates a specific call to the above method, requesting the
        // Description MetaData attribute.
        public static string ToDesc(this Enum value) 
        {
            var attribute = value.GetAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static string ToField(this Enum value)
        {
            var attribute = value.GetAttribute<FieldAttribute>();
            return attribute == null ? value.ToString() : attribute.Field;
        }
        public static string ToAlias(this Enum value)
        {
            var attribute = value.GetAttribute<AliasAttribute>();
            return attribute == null ? value.ToString() : attribute.Alias;
        }
    }

    public class FieldAttribute : Attribute
    {
        private string name;
        public FieldAttribute(string name)
        {
            this.name = name;
        }
        public virtual string Field { get{ return name; } }
        protected string FieldValue { get; set; }
    }
    public class AliasAttribute : Attribute
    {
        private string name;
        public AliasAttribute(string name)
        {
            this.name = name;
        }
        public virtual string Alias { get { return name; } }
        protected string AliasValue { get; set; }
    }
    #endregion

    public enum OrderStatus
    {
        [Description("Đang xin phép")] Request,
        [Description("Đã chấp nhận")] Approve
    }

    public enum OrderType
    {
        [Description("Nhân công")] Worker,
        [Description("Thiết bị")] Equip,
        [Description("Phương tiện")] Car,
        [Description("Kinh phí")] Expense
    }

    public enum ResStatus
    {
        [Description("Free")] Free,
        [Description("Busy")] Busy,
    }
    public enum Gender
    {
        [Description("Nam")] Male,
        [Description("Nữ")] Female,
    }

#if use_sqlite
    public class lSQLiteContentProvider : lContentProvider
    {
        static lSQLiteContentProvider m_instance;
        public static lContentProvider getInstance(Form1 parent)
        {
            if (m_instance == null)
            {
                m_instance = new lSQLiteContentProvider();
                m_instance.m_form = parent;
            }
            return m_instance;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        lSQLiteContentProvider() : base()
        {
            //string dbPath = "test.db";
            string dbPath = appConfig.s_config.m_dbSchema.m_cnnStr;
            bool bCrtTbls = false;
            if (!System.IO.File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                bCrtTbls = true;
            }
            m_cnn = new SQLiteConnection(string.Format("Data Source={0};Version=3;", dbPath));
            m_cnn.Open();
#if crt_tables
            if (bCrtTbls)
            {
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = m_cnn;
                List<string> sqls = new List<string>();
                foreach (TableInfo tbl in appConfig.s_config.m_dbSchema.m_tables)
                {
                    sqls.Add(tbl.m_crtQry);
                }
                foreach (TableInfo view in appConfig.s_config.m_dbSchema.m_views)
                {
                    sqls.Add(view.m_crtQry);
                }
                foreach (var sql in sqls)
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
#endif  //crt_tables
        }

        private SQLiteConnection m_cnn;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public override DataTable GetData(string qry)
        {
            SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter();
            dataAdapter.SelectCommand = new SQLiteCommand(qry, m_cnn);
            // Populate a new data table and bind it to the BindingSource.
            DataTable table = new DataTable();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            dataAdapter.Fill(table);
            return table;
        }

        protected override DataContent newDataContent(string tblName)
        {
            lSQLiteDataContent data = new lSQLiteDataContent(tblName, m_cnn);
#if !use_bg_work
            data.m_form = m_form;
#endif
            return data;
        }

        public override object GetCnn()
        {
            return m_cnn;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_cnn.Close();
                m_cnn.Dispose();
            }
            base.Dispose(disposing);
        }
    }
#endif //use_sqlite
    [DataContract(Name = "DbSchema")]
    public class lDbSchema
    {
        [DataMember(Name = "cnnStr")]
        public string m_cnnStr;
#if crt_qry
            [DataMember(Name ="crtTableSqls")]
            public List<string> m_crtTableSqls;
            [DataMember(Name = "crtViewSqls")]
            public List<string> m_crtViewSqls;
#endif
        [DataMember(Name = "tables")]
        public List<TableInfo> m_tables;
        [DataMember(Name = "views")]
        public List<TableInfo> m_views;

        public lDbSchema()
        {
        }
        protected void init()
        {
            m_tables = new List<TableInfo>() {
                    new TaskTblInfo(),
                    new GroupNameTblInfo(),
                    new OrderTblInfo(),
                    new HumanTblInfo(),
                    new EquipmentTblInfo(),
                    new OrderEquipmentTblInfo(),
                    new OrderHumanTblInfo(),
                    new CarTblInfo(),
                    new OrderCarTblInfo(),
                };
            m_views = new List<TableInfo>() {
                    //new lReceiptsViewInfo(),
                    //new lInterPaymentViewInfo(),
                    //new lExterPaymentViewInfo(),
                    //new lSalaryViewInfo(),
                    //new lDaysumViewInfo()
                };
        }
    }
#if use_sqlite
    [DataContract(Name = "SQLiteDbSchema")]
    public class lSQLiteDbSchema : lDbSchema
    {
        public lSQLiteDbSchema()
        {
            m_cnnStr = @"..\..\appData.db";
#if crt_qry
                m_crtTableSqls = new List<string> {
                    "CREATE TABLE if not exists  receipts("
                    + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                    + "date datetime,"
                    + "receipt_number char(31),"
                    + "name char(31),"
                    + "content text,"
                    + "amount INTEGER,"
                    + "note text"
                    + ")",
                    "CREATE TABLE if not exists internal_payment("
                    + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                    + "date datetime,"
                    + "payment_number char(31),"
                    + "name char(31),"
                    + "content text,"
                    + "group_name char(31),"
                    + "advance_payment INTEGER,"
                    + "reimbursement INTEGER,"
                    + "actually_spent INTEGER,"
                    + "note text"
                    + ")",
                    "CREATE TABLE if not exists external_payment("
                    + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                    + "date datetime,"
                    + "payment_number char(31),"
                    + "name char(31),"
                    + "content text,"
                    + "group_name char(31),"
                    + "spent INTEGER,"
                    + "note text"
                    + ")",
                    "CREATE TABLE if not exists salary("
                    + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                    + "month INTEGER,"
                    + "date datetime,"
                    + "payment_number char(31),"
                    + "name char(31),"
                    + "group_name char(31),"
                    + "content text,"
                    + "salary INTEGER,"
                    + "note text"
                    + ")",
                    "CREATE TABLE if not exists receipts_content(ID INTEGER PRIMARY KEY AUTOINCREMENT, content nchar(31));",
                    "CREATE TABLE if not exists group_name(ID INTEGER PRIMARY KEY AUTOINCREMENT, name nchar(31));",
            };
                m_crtViewSqls = new List<string> {
                    "CREATE VIEW if not exists v_receipts "
                    + " as "
                    + " select content, amount, cast(strftime('%Y', date) as integer) as year, (strftime('%m', date) + 2) / 3 as qtr "
                    + " from receipts"
                    + " where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;" ,
                    " CREATE VIEW if not exists v_internal_payment"
                    + " as"
                    + " select group_name, actually_spent, cast(strftime('%Y', date) as integer) as year, (strftime('%m', date) + 2) / 3 as qtr"
                    + " from internal_payment"
                    + " where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;",

                    "CREATE VIEW if not exists v_external_payment"
                    + " as"
                    + " select group_name, spent, cast(strftime('%Y', date) as integer) as year, (strftime('%m', date) + 2) / 3 as qtr"
                    + " from external_payment"
                    + " where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;",
                    "CREATE VIEW if not exists v_salary"
                    + " as"
                    + " select group_name, salary, cast(strftime('%Y', date) as integer) as year, (strftime('%m', date) + 2) / 3 as qtr"
                    + " from salary"
                    + " where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;",
                };
#endif  //crt_qry
            init();
        }
    }
#endif //use_sqlite

    [DataContract(Name = "SqlDbSchema")]
    public class lSqlDbSchema : lDbSchema
    {
        public lSqlDbSchema()
        {
            m_cnnStr = @"Data Source=.\SQLEXPRESS;Initial Catalog=accounting;Integrated Security=True;MultipleActiveResultSets=True";
#if crt_qry
                m_crtTableSqls = new List<string> {
                    "CREATE TABLE if not exists  receipts("
                    + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                    + "date datetime,"
                    + "receipt_number char(31),"
                    + "name char(31),"
                    + "content text,"
                    + "amount INTEGER,"
                    + "note text"
                    + ")",
                    "CREATE TABLE if not exists internal_payment("
                    + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                    + "date datetime,"
                    + "payment_number char(31),"
                    + "name char(31),"
                    + "content text,"
                    + "group_name char(31),"
                    + "advance_payment INTEGER,"
                    + "reimbursement INTEGER,"
                    + "actually_spent INTEGER,"
                    + "note text"
                    + ")",
                    "CREATE TABLE if not exists external_payment("
                    + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                    + "date datetime,"
                    + "payment_number char(31),"
                    + "name char(31),"
                    + "content text,"
                    + "group_name char(31),"
                    + "spent INTEGER,"
                    + "note text"
                    + ")",
                    "CREATE TABLE if not exists salary("
                    + "ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                    + "month INTEGER,"
                    + "date datetime,"
                    + "payment_number char(31),"
                    + "name char(31),"
                    + "group_name char(31),"
                    + "content text,"
                    + "salary INTEGER,"
                    + "note text"
                    + ")",
                    "CREATE TABLE if not exists receipts_content(ID INTEGER PRIMARY KEY AUTOINCREMENT, content nchar(31));",
                    "CREATE TABLE if not exists group_name(ID INTEGER PRIMARY KEY AUTOINCREMENT, name nchar(31));",
            };
                m_crtViewSqls = new List<string> {
                    "CREATE VIEW if not exists v_receipts "
                    + " as "
                    + " select content, amount, cast(strftime('%Y', date) as integer) as year, (strftime('%m', date) + 2) / 3 as qtr "
                    + " from receipts"
                    + " where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;" ,
                    " CREATE VIEW if not exists v_internal_payment"
                    + " as"
                    + " select group_name, actually_spent, cast(strftime('%Y', date) as integer) as year, (strftime('%m', date) + 2) / 3 as qtr"
                    + " from internal_payment"
                    + " where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;",

                    "CREATE VIEW if not exists v_external_payment"
                    + " as"
                    + " select group_name, spent, cast(strftime('%Y', date) as integer) as year, (strftime('%m', date) + 2) / 3 as qtr"
                    + " from external_payment"
                    + " where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;",
                    "CREATE VIEW if not exists v_salary"
                    + " as"
                    + " select group_name, salary, cast(strftime('%Y', date) as integer) as year, (strftime('%m', date) + 2) / 3 as qtr"
                    + " from salary"
                    + " where strftime('%Y', 'now') - strftime('%Y', date) between 0 and 4;",
                };
#endif  //crt_qry
            init();
        }
    }
    /// <summary>
    /// data content
    /// + getdata()
    ///     return databinding
    /// + reload()
    /// + submit()
    /// </summary>
    public class DataContent : ICursor, IDisposable
    {
#region fetch_data
#if use_bg_work
        myWorker m_wkr;
#else
        public Form1 m_form;
#endif
        public DataTable m_dataTable { get; private set; }

#region event
        public class FillTableCompletedEventArgs : EventArgs
        {
            public Int64 Sum { get; set; }
            public DateTime TimeComplete { get; set; }
        }
        static Dictionary<string, EventHandler<FillTableCompletedEventArgs>> m_dict = 
            new Dictionary<string, EventHandler<FillTableCompletedEventArgs>>();
        private void addEvent(string zType, ref EventHandler<FillTableCompletedEventArgs> handler, EventHandler<FillTableCompletedEventArgs> value)
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

        
#region update_complete_event
        private EventHandler<FillTableCompletedEventArgs> mFillTableCompleted;
        public event EventHandler<FillTableCompletedEventArgs> FillTableCompleted
        {
            add{addEvent("FillTableCompleted", ref mFillTableCompleted, value);}
            remove{}
        }
        protected virtual void OnFillTableCompleted(FillTableCompletedEventArgs e)
        {
            if (mFillTableCompleted != null){mFillTableCompleted(this, e);}
        }
        private EventHandler<FillTableCompletedEventArgs> mUpdateTableCompleted;
        public event EventHandler<FillTableCompletedEventArgs> UpdateTableCompleted
        {
            add{addEvent("UpdateTableCompleted", ref mUpdateTableCompleted, value);}
            remove{}
        }
        protected virtual void OnUpdateTableCompleted(FillTableCompletedEventArgs e)
        {
            if (mUpdateTableCompleted != null){mUpdateTableCompleted(this, e);}
        }
#endregion //update_complete_event

        protected virtual void OnProgessCompleted(EventArgs e)
        {
            if (ProgessCompleted != null)
            {
                ProgessCompleted(this, e);
            }
        }
        public event EventHandler ProgessCompleted;
#endregion

        protected virtual Int64 getMaxRowId() { return 0; }
        protected virtual Int64 getRowCount() { return 0; } //not used
        protected virtual void fillTable() { throw new NotImplementedException(); }
        protected virtual void updateTable() { throw new NotImplementedException(); }
        //delegate noParamDelegate
#if !use_bg_work
        void invokeFetchLargeData()
        {
            //fix reload not display progress
            //  set cursor pos to begin
            setPos(0);

            taskCallback0 showProgress = () =>
            {
                Int64 maxId = getMaxRowId();
                ProgressDlg prg = new ProgressDlg();
                prg.TopMost = true;
                Debug.Assert(!m_isView, "not featch large data on view!");
                prg.m_cursor = this;
                prg.m_endPos = maxId;
                prg.m_scale = 1000;
                prg.m_descr = "Getting data ...";
                prg.ShowDialog();
                prg.Dispose();
                OnProgessCompleted(EventArgs.Empty);
            };

            taskCallback0 getData = () =>
            {
                Int64 maxId = getMaxRowId();
                fetchLargeData();
                m_lastId = maxId;
            };

            Task t = Task.Run(() => showProgress());
            m_form.Invoke(getData);
            //m_form.m_bgwork.qryBgTask(new BgTask
            //{
            //    eType = BgTask.bgTaskType.bgExec,
            //    data = showProgress
            //});

            ////m_form.Invoke(getData);
            //m_form.m_bgwork.qryFgTask(new FgTask {
            //    eType =FgTask.fgTaskType.fgExec,
            //    data = getData
            //}
            //);
        }
#endif

        //require execute in form's thread
        void fetchLargeData()
        {
            Debug.Assert(!m_isView, "not fectch large data on view");   //rowid not exist
            using (new myElapsed("fetchLargeData"))
            {
                DataTable tbl = m_dataTable;

                tbl.Clear();
                tbl.Locale = System.Globalization.CultureInfo.InvariantCulture;

                tbl.RowChanged += M_tbl_RowChanged; //udpate last row id
                m_lastId = 0;

                fillTable();

                tbl.RowChanged -= M_tbl_RowChanged;
            }

            OnFillTableCompleted(new FillTableCompletedEventArgs() { TimeComplete = DateTime.Now });

            if (m_refresher != null)
                m_refresher.Refresh();
        }
#if use_single_qry
        Int64 m_lastId = 0;
        private void M_tbl_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            //Debug.WriteLine("{0}.M_tbl_RowChanged {1}", this, e.Row[0]);
            m_lastId = Math.Max(m_lastId, (Int64)e.Row[0]);
        }
#region cursor
        public Int64 getPos()
        {
            return m_lastId;
        }
        public void setPos(Int64 pos) { m_lastId = pos; }
        string m_msgStatus;
        public void setStatus(string msg)
        {
            m_msgStatus = msg;
        }
        public string getStatus()
        {
            return m_msgStatus;
        }
#endregion
#endif
        delegate void noParamDelegate();
        protected virtual void fetchData()
        {
            Debug.Assert(!m_isView, "not fectch data on view");
#if use_bg_work
            fetchLargeData();
#else
            if (getMaxRowId() > 1000)
                invokeFetchLargeData();
            else
                fetchSmallData();
#endif
        }
        void fetchSmallData()
        {
            DataTable table = m_dataTable;
            table.Clear();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            fillTable();

            OnFillTableCompleted(new FillTableCompletedEventArgs() { TimeComplete = DateTime.Now });

            if (m_refresher != null)
                m_refresher.Refresh();
        }
#endregion
        public bool m_isView;
        public BindingSource m_bindingSource { get; private set; }
        protected string m_table;
        public IRefresher m_refresher;
        public DataContent()
        {
            m_dataTable = new DataTable();
            m_bindingSource = new BindingSource();
            m_bindingSource.DataSource = m_dataTable;
        }
        protected void init()
        {
            if (m_dataTable.Columns.Count == 0)
            {
                TableInfo tbl = appConfig.s_config.GetTable(m_table);
                if (tbl == null) return;

                //not need to init cols for views
                if (tbl.m_cols == null) return;

                foreach (var col in tbl.m_cols)
                {
                    DataColumn dc = m_dataTable.Columns.Add(col.m_field);
                    switch (col.m_type)
                    {
                        case TableInfo.ColInfo.ColType.num:
                        case TableInfo.ColInfo.ColType.currency:
                            dc.DataType = typeof(Int64);
                            break;
                        case TableInfo.ColInfo.ColType.dateTime:
                            dc.DataType = typeof(DateTime);
                            break;
                    }
                }
            }

#if use_bg_work
            m_wkr = myWorker.getWorker();
#endif
        }
#if !use_cmd_params
            public virtual void Search(string exprs)
            {
                string sql = string.Format("select * from {0} ", m_table);
                if (exprs != null)
                {
                    sql += " where " + exprs;
                }
                GetData(sql);
            }
#endif
#if use_cmd_params
        public virtual void Search(List<string> exprs, List<SearchParam> srchParams) { throw new NotImplementedException(); }
        public virtual int Update(List<string> setExprs, List<string> whereExprs, List<SearchParam> srchParams) { throw new NotImplementedException(); }
        public virtual void AddRec(List<string> exprs, List<SearchParam> srchParams) { throw new NotImplementedException(); }
#endif
        bool m_changed = true;
        public virtual void Load(bool isView) { throw new NotFiniteNumberException(); }
        public virtual void Load() { if (m_changed) { Reload(); } }
        public virtual void Reload() { m_changed = false; }
        public virtual void Submit()
        {
            m_changed = false;
#if use_bg_work
            updateTable();
#else
            Task delayUpdate = Task.Run(() =>
            {
                Thread.Sleep(100);
                this.m_form.Invoke(new noParamDelegate(() =>
                {
                    updateTable();
                    OnUpdateTableCompleted(new FillTableCompletedEventArgs() { TimeComplete = DateTime.Now });
                }));
            });
#endif
        }
        protected virtual void GetData(string sql) { throw new NotImplementedException(); }
        public virtual void BeginTrans() { }
        public virtual void EndTrans() { }
#region dispose
        // Dispose() calls Dispose(true)  
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // NOTE: Leave out the finalizer altogether if this class doesn't   
        // own unmanaged resources itself, but leave the other methods  
        // exactly as they are.   
        ~DataContent()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)  
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_bindingSource.Dispose();
                m_dataTable.Dispose();
            }
            // free native resources if there are any. 
        }
#endregion
    }
#if use_sqlite
    public class lSQLiteDataContent : DataContent
    {
        private readonly SQLiteConnection m_cnn;
        private SQLiteDataAdapter m_dataAdapter;
        private SQLiteTransaction m_trans;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public lSQLiteDataContent(string tblName, SQLiteConnection cnn)
            : base()
        {
            m_table = tblName;
            m_cnn = cnn;
            m_dataAdapter = new SQLiteDataAdapter();
            m_dataAdapter.SelectCommand = new SQLiteCommand(selectLast(100), cnn);
            m_dataAdapter.RowUpdated += M_dataAdapter_RowUpdated;
#if init_datatable_cols
            init();
#endif
        }

        string selectLast(int count)
        {
            return string.Format("select * from {0} "
                + " where "
                + " id in (SELECT id from {0} order by id desc limit {1})",
                m_table, count);
        }
#if !use_cmd_params
            public override void Search(string exprs)
            {
                string sql = string.Format("select * from {0} ", m_table);
                if (exprs != null)
                {
                    sql += " where " + exprs;
                }
                else
                {
                    sql = selectLast100();
                }
                GetData(sql);
            }
#endif
#if use_cmd_params
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public override void Search(List<string> exprs, List<SearchParam> srchParams)
        {
            SQLiteCommand selectCommand;
            string sql = string.Format("select * from {0} ", m_table);
            if (exprs.Count > 0)
            {
                sql += " where " + string.Join(" and ", exprs);
                selectCommand = new SQLiteCommand(sql, m_cnn);
                foreach (var param in srchParams)
                {
                    selectCommand.Parameters.AddWithValue(param.key, param.val);
                }
            }
            else
            {
                selectCommand = new SQLiteCommand(selectLast(100), m_cnn);
            }
            GetData(selectCommand);
        }

        public override int Update(List<string> setExprs, List<string> whereExprs, List<SearchParam> srchParams)
        {
            SQLiteCommand updateCommand;
            string sql = string.Format("Update {0} ", m_table);
            Debug.Assert(setExprs.Count > 0, "can not empty");
            Debug.Assert(whereExprs.Count > 0, "can not empty");
            sql += " set " + string.Join(" , ", setExprs);
            sql += " where " + string.Join(" and ", whereExprs);
            updateCommand = new SQLiteCommand(sql, m_cnn);
            foreach (var param in srchParams)
            {
                updateCommand.Parameters.AddWithValue(param.key, param.val);
            }
            int ret = updateCommand.ExecuteNonQuery();
            return ret;
        }
#endif
        public override void AddRec(List<string> exprs, List<SearchParam> srchParams)
        {
            SQLiteCommand cmd;

            string cols = string.Join(",", exprs);
            string vals = "";
            foreach (var param in srchParams)
            {
                vals += ", " + param.key;
            }
            string sql = string.Format("insert into {0} ({1}) values ({2}) ", m_table, cols, vals.Substring(1));
            cmd = new SQLiteCommand(sql, m_cnn);
            foreach (var param in srchParams)
            {
                cmd.Parameters.AddWithValue(param.key, param.val);
            }
            int nRet = cmd.ExecuteNonQuery();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public override void Load(bool isView)
        {
            if (isView)
            {
                m_isView = true;
                m_dataAdapter.SelectCommand.CommandText = string.Format("select * from {0}", m_table);
            }
            Reload();
        }
        public override void Reload()
        {
            base.Reload();
            GetData(m_dataAdapter.SelectCommand);
        }
        protected override void updateTable()
        {
            using (SQLiteCommandBuilder builder = new SQLiteCommandBuilder(m_dataAdapter))
            {
                DataTable dt = m_dataTable;
                if (dt != null)
                {
                    m_dataAdapter.UpdateCommand = builder.GetUpdateCommand();
                    m_dataAdapter.Update(dt);
                }
            }
        }

        private void M_dataAdapter_RowUpdated(object sender, System.Data.Common.RowUpdatedEventArgs e)
        {
            //udpate row id
            if (e.StatementType == StatementType.Insert)
            {
                Int64 rowid = m_cnn.LastInsertRowId;
                e.Row[0] = rowid;
                Debug.WriteLine("M_dataAdapter_RowUpdated {0}", e.Row[0]);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        protected override void GetData(string selectStr)
        {
            SQLiteCommand selectCommand = new SQLiteCommand(selectStr, m_cnn);
            GetData(selectCommand);
        }
        private void GetData(SQLiteCommand selectCommand)
        {
            Debug.WriteLine("{0}.GetData {1}", this, selectCommand.CommandText);
            m_dataAdapter.SelectCommand = selectCommand;
            // Populate a new data table and bind it to the BindingSource.
            fetchData();
        }
        public override void BeginTrans()
        {
            m_trans = m_cnn.BeginTransaction();
        }
        public override void EndTrans()
        {
            m_trans.Commit();
            m_trans.Dispose();
        }

#region dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_dataAdapter.Dispose();
            }
            // free native resources if there are any. 
            base.Dispose(disposing);
        }
#endregion

#region fetch_data
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        Int64 qryOne(string qry)
        {
            SQLiteConnection cnn = m_cnn;
            SQLiteCommand cmd = new SQLiteCommand(qry, cnn);
            var ret = cmd.ExecuteScalar();
            if (ret != DBNull.Value)
                return (Int64)ret;
            else
                return 0;
        }
        protected override Int64 getMaxRowId()
        {
            return qryOne(string.Format("select max(id) from {0}", m_table));
        }
        protected override Int64 getRowCount()
        {
            return qryOne(string.Format("select count(*) from {0}", m_table));
        }
        protected override void fillTable()
        {
            m_dataAdapter.Fill(m_dataTable);
        }
#endregion
    }
#endif //use_sqlite
    public class lSqlDataContent : DataContent
    {
        private SqlConnection m_cnn;
        private SqlDataAdapter m_dataAdapter;
        private SqlTransaction m_trans;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public lSqlDataContent(string tblName, SqlConnection cnn)
            : base()
        {
            m_table = tblName;
            m_cnn = cnn;
            m_dataAdapter = new SqlDataAdapter();
            m_dataAdapter.SelectCommand = new SqlCommand(string.Format("select * from {0}", tblName), cnn);
#if init_datatable_cols
            init();
#endif
        }

#if !use_cmd_params
            public override void Search(string exprs)
            {
                string sql = string.Format("select * from {0} ", m_table);
                if (exprs != null)
                {
                    sql += " where " + exprs;
                }
                GetData(sql);
            }
#endif
#if use_cmd_params
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public override void Search(List<string> exprs, List<SearchParam> srchParams)
        {
            string sql = string.Format("select * from {0} ", m_table);

            if (exprs.Count > 0)
            {
                sql += " where " + string.Join(" and ", exprs);
                SqlCommand selectCommand = new SqlCommand(sql, m_cnn);
                foreach (var param in srchParams)
                {
                    var p = selectCommand.Parameters.AddWithValue(param.key, param.val);
                    if (param.type == DbType.String) { p.DbType = DbType.String; }
                }
                GetData(selectCommand);
            }
            else
            {
                GetData(sql);
            }
        }
#endif
        public override void Reload()
        {
            base.Reload();
            GetData(m_dataAdapter.SelectCommand);
        }
        protected override void updateTable()
        {
            using (SqlCommandBuilder builder = new SqlCommandBuilder(m_dataAdapter))
            {
                DataTable dt = (DataTable)m_bindingSource.DataSource;
                if (dt != null)
                {
                    m_dataAdapter.UpdateCommand = builder.GetUpdateCommand();
                    m_dataAdapter.Update(dt);
                }
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        protected override void GetData(string selectStr)
        {
            SqlCommand selectCommand = new SqlCommand(selectStr, m_cnn);
            GetData(selectCommand);
        }
        private void GetData(SqlCommand selectCommand)
        {
            m_dataAdapter.SelectCommand = selectCommand;
            // Populate a new data table and bind it to the BindingSource.
            fetchData();
        }
        protected override void fillTable()
        {
            m_dataAdapter.Fill(m_dataTable);
        }

        public override void BeginTrans()
        {
            m_trans = m_cnn.BeginTransaction();
        }
        public override void EndTrans()
        {
            m_trans.Commit();
            m_trans.Dispose();
        }
    }

    public interface IRefresher
    {
        void Refresh();
    }
    public class lDataSync : IRefresher, IDisposable
    {
        private DataContent m_data;
        public AutoCompleteStringCollection m_colls;
        public Dictionary<string, string> m_maps;
        public int m_col = 1;
        public lDataSync(DataContent data)
        {
            m_data = data;
            m_data.m_refresher = this;
        }

        static Dictionary<string, string> dict = new Dictionary<string, string>() {
                {"[áàảãạ]", "a"  },
                {"[ăằắẵẳặ]", "a"},
                {"[âầấẫẩậ]", "a"},
                {"[đ]", "d"     },
                {"[éèẻẽẹ]", "e" },
                {"[êềếễểệ]", "e"},
                {"[íìĩỉị]", "i" },
                {"[òóỏõọ]",  "o"},
                {"[ồôỗốộổ]",  "o"},
                {"[ơờớỡởợ]", "o"},
                {"[úùủũụ]", "u" },
                {"[ừưữứửự]", "u"},
            };
        static string genKey(string value)
        {
            string key = value.ToLower();
            foreach (var i in dict)
            {
                key = Regex.Replace(key, i.Key, i.Value);
            }
            return key;
        }
        public void Refresh()
        {
            m_colls = new AutoCompleteStringCollection();
            m_maps = new Dictionary<string, string>();
            DataTable tbl = m_dataSource;
            foreach (DataRow row in tbl.Rows)
            {
                string val = row[m_col].ToString();
                string key = genKey(val);
                m_colls.Add(val);
                m_colls.Add(key);
                try
                {
                    m_maps.Add(key, val);
                }
                catch { }
            }
        }
        public void LoadData()
        {
            m_data.Load();
            Refresh();
        }
        public BindingSource m_bindingSrc
        {
            get { return m_data.m_bindingSource; }
        }
        public DataTable m_dataSource
        {
            get { return m_data.m_dataTable; }
        }
        public void Update(string selectedValue)
        {
            Debug.WriteLine("{0}.Update {1}", this, selectedValue);
            string key = genKey(selectedValue);
            if (!m_maps.ContainsKey(key))
            {
                m_colls.Add(key);
                m_colls.Add(selectedValue);
                m_maps.Add(key, selectedValue);
                //update database
                Add(selectedValue);
            }
        }
        private void Add(string newValue)
        {
            //single col tables
            DataRow newRow = m_dataSource.NewRow();
            newRow[1] = newValue;
            m_dataSource.Rows.Add(newRow);
            m_data.Submit();
        }
        public string find(string key)
        {
            key = genKey(key);
            if (m_maps.ContainsKey(key))
                return m_maps[key];
            else
                return null;
        }

#region dispose
        // Dispose() calls Dispose(true)  
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // NOTE: Leave out the finalizer altogether if this class doesn't   
        // own unmanaged resources itself, but leave the other methods  
        // exactly as they are.   
        ~lDataSync()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)  
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                m_colls.Clear();
                m_maps.Clear();
            }
            // free native resources if there are any.  
        }
#endregion
    }
}