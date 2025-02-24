﻿using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading;

namespace Warehouser_NET
{
    internal class CUtils
    {
    }

    public class ItemClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private ShelfClass shelf;
        private UIDClass uid;
        private int count;
        private string functionString;
        private List<int> functions = new List<int>();
        public static ItemClass Parse(JsonNode json)
        {
            ShelfClass shelf = ShelfClass.Parse(json["shelf"].AsObject());
            UIDClass uid = UIDClass.Parse(json["uid"].AsObject());
            int count;
            if (!int.TryParse(json["count"].ToString(), out count))
                count = -1;
            var usage = json["function"].ToString().Replace("{", "").Replace("}", "").Split(",");
            List<int> uin = new List<int>();
            string ust = string.Empty;
            foreach (var usa in usage)
            {
                int oint;
                if (int.TryParse(usa, out oint))
                {
                    uin.Add(oint);
                    foreach (var us in HiroUtils.usages)
                    {
                        if (us.Code == oint)
                        {
                            if (ust == string.Empty)
                                ust = us.Info;
                            else
                                ust = ust + "," + us.Info;
                        }
                    }
                }

            }
            return new ItemClass()
            {
                Shelf = shelf,
                UID = uid,
                Count = count,
                Functions = uin,
                FunctionString = ust
            };
        }

        public ShelfClass Shelf
        {
            get { return shelf; }
            set
            {
                shelf = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Shelf)));
            }
        }

        public string ShelfFID
        {
            get { return shelf.FID; }
        }

        public string ShelfDepart
        {
            get { return shelf.Depart.Name; }
        }


        public UIDClass UID
        {
            get { return uid; }
            set
            {
                uid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UID)));
            }
        }

        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            }
        }

        public List<int> Functions
        {
            get { return functions; }
            set
            {
                functions = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Functions)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FunctionString)));
            }
        }

        public string FunctionString
        {
            get { return functionString; }
            set
            {
                functionString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FunctionString)));
            }
        }

        public override string ToString()
        {
            return $"{Shelf.ToString()}{Environment.NewLine}" +
                $"货物名称 : \t{UID.Name}{Environment.NewLine}" +
                $"唯一编码 : \t{UID.UID}{Environment.NewLine}" +
                $"数量 : \t\t{Count}{Environment.NewLine}" +
                $"用途 : \t\t{FunctionString}";
        }

        internal object toJson()
        {
            throw new NotImplementedException();
        }
    }

    public class ItemClassForImport : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private string shelf;
        private string uid;
        private int count;
        private string functionString;
        private List<int> functions = new List<int>();

        public string Shelf
        {
            get { return shelf; }
            set
            {
                shelf = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Shelf)));
            }
        }

        public string UID
        {
            get { return uid; }
            set
            {
                uid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UID)));
            }
        }

        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            }
        }

        public List<int> Functions
        {
            get { return functions; }
            set
            {
                functions = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Functions)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FunctionString)));
            }
        }

        public string FunctionString
        {
            get { return functionString; }
            set
            {
                functionString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FunctionString)));
            }
        }

        internal object toJson()
        {
            var a = string.Empty;
            foreach (var i in Functions)
            {
                if (a.Equals(string.Empty))
                    a = i.ToString();
                else
                    a = a + "," + i.ToString();
            };
            var ret = new JsonObject();
            ret.Add("shelf", Shelf);
            ret.Add("uid", UID);
            ret.Add("count", Count.ToString());
            ret.Add("function", a);
            return ret;
        }
    }

    public class UserClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private string name;
        private string pwd;
        private DepartClass depart;
        private string nick;
        private int privilege;
        private string role;


        public static UserClass Parse(string str)
        {
            return Parse(JsonNode.Parse(str));
        }

        public static UserClass Parse(JsonNode json)
        {
            var pri = -1;
            if (!int.TryParse(json["privilege"].ToString(), out pri))
                pri = -1;
            var pwd = string.Empty;
            try
            {
                pwd = json["pwd"].ToString();
            }
            catch { };
            DepartClass depart = DepartClass.Parse(json["depart"].AsObject());
            var role = pri < 0 ? "未激活" : pri > HiroUtils.roles.Count - 1 ? "超级管理员" : HiroUtils.roles[pri];
            return new UserClass()
            {
                Name = json["username"].ToString(),
                Password = pwd,
                Nickname = json["nickname"].ToString(),
                Depart = depart,
                Privilege = pri,
                Role = role
            };
        }


        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public string Password
        {
            get { return pwd; }
            set
            {
                pwd = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password)));
            }
        }

        public string Nickname
        {
            get { return nick; }
            set
            {
                nick = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nickname)));
            }
        }

        public DepartClass Depart
        {
            get { return depart; }
            set
            {
                depart = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Depart)));
            }
        }

        public int Privilege
        {
            get { return privilege; }
            set
            {
                privilege = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Privilege)));
            }
        }


        public string Role
        {
            get { return role; }
            set
            {
                role = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Role)));
            }
        }

        public override string ToString()
        {
            return $"账户 : \t\t{Name}{Environment.NewLine}" +
                $"名称 : \t\t{Nickname}{Environment.NewLine}" +
                $"部门 : \t\t{Depart.ToString()}{Environment.NewLine}" +
                $"角色 : \t\t{Role}";
        }

        public string ToStringWithoutRole()
        {
            return $"账户 : \t\t{Name}{Environment.NewLine}" +
                $"名称 : \t\t{Nickname}{Environment.NewLine}" +
                $"部门 : \t\t{Depart.ToString()}{Environment.NewLine}";
        }

        public JsonNode toJson()
        {
            var ret = new JsonObject();
            ret.Add("username", Name);
            ret.Add("pwd", Password);
            ret.Add("nickname", Nickname);
            ret.Add("depart", Depart.toJson().ToString());
            ret.Add("privilege", Privilege.ToString());
            return ret;
        }
    }

    public class ShelfClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private DepartClass depart;
        private string fid;
        private int mid;
        private int sid;
        private string alias;
        private string? info;


        public static ShelfClass Parse(string str)
        {
            var json = JsonNode.Parse(str);
            return Parse(json);
        }

        public static ShelfClass Parse(JsonNode json)
        {
            var dmid = -1;
            if (!int.TryParse(json["mid"].ToString(), out dmid))
                dmid = -1;
            var dsid = -1;
            if (!int.TryParse(json["sid"].ToString(), out dsid))
                dsid = -1;
            DepartClass depart = DepartClass.Parse(json["depart"].AsObject());
            return new ShelfClass()
            {
                Depart = depart,
                FID = json["fid"].ToString(),
                MID = dmid,
                SID = dsid,
                Alias = json["alias"].ToString(),
                Info = json["desp"].ToString()
            };
        }

        public DepartClass Depart
        {
            get { return depart; }
            set
            {
                depart = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Depart)));
            }
        }

        public string FID
        {
            get { return fid; }
            set
            {
                fid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FID)));
            }
        }

        public string Alias
        {
            get { return alias; }
            set
            {
                alias = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Alias)));
            }
        }

        public string Info
        {
            get
            {
                if (info == null)
                    return string.Empty;
                return info;
            }
            set
            {
                info = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Info)));
            }
        }

        public int MID
        {
            get { return mid; }
            set
            {
                mid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MID)));
            }
        }

        public int SID
        {
            get { return sid; }
            set
            {
                sid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SID)));
            }
        }

        public override string ToString()
        {
            return $"部门 : \t\t{Depart.ToString()}{Environment.NewLine}" +
                $"货架 ID : \t{FID}{Environment.NewLine}" +
                $"\t主 ID : \t{MID}{Environment.NewLine}" +
                $"\t副 ID : \t{SID}{Environment.NewLine}" +
                $"货架别称 : \t{Alias}{Environment.NewLine}" +
                $"货架信息 : \t{Info}";
        }

        internal JsonObject toJson()
        {
            var ret = new JsonObject();
            ret.Add("depart", Depart.toJson().ToString());
            ret.Add("mshelf", MID.ToString());
            ret.Add("sshelf", SID.ToString());
            ret.Add("alias", Alias);
            ret.Add("desp", Info);
            return ret;
        }
    }

    internal class UsageClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private int code;
        private string alias;
        private string? info;
        private bool hide;
        private string hidestr;

        public static UsageClass Parse(string str)
        {
            var json = JsonNode.Parse(str);
            return Parse(json);
        }

        public static UsageClass Parse(JsonNode json)
        {
            var id = json["id"].ToString();
            var code = -1;
            if (!int.TryParse(id, out code))
                code = -1;
            return new UsageClass()
            {
                Code = code,
                Alias = json["name"].ToString(),
                Info = json["info"].ToString(),
                Hide = json["hide"].ToString().ToLower().Equals("true") || json["hide"].ToString().Equals("1"),
                HideStr = json["hide"].ToString().ToLower().Equals("true") || json["hide"].ToString().Equals("1") ? "隐藏" : "可见"
            };
        }

        public int Code
        {
            get { return code; }
            set
            {
                code = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Code)));
            }
        }

        public string Alias
        {
            get { return alias; }
            set
            {
                alias = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Alias)));
            }
        }

        public string Info
        {
            get
            {
                if (info == null)
                    return string.Empty;
                return info;
            }
            set
            {
                info = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Info)));
            }
        }

        public bool Hide
        {
            get { return hide; }
            set
            {
                hide = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Hide)));
            }
        }

        public string HideStr
        {
            get { return hidestr; }
            set
            {
                hidestr = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HideStr)));
            }
        }

        public UsageClass(int c, string a, string? i = null)
        {
            Code = c;
            Alias = a;
            Info = i;
            Hide = false;
            HideStr = "可见";
        }

        public UsageClass()
        {

        }
        /*
                private int code;
                private string alias;
                private string? info;*/

        public override string ToString()
        {
            return $"代码 : \t\t{Code}{Environment.NewLine}" +
                $"名称 : \t\t{Alias}{Environment.NewLine}" +
                $"附加信息 :\t{(Info == null ? string.Empty : Info)}{Environment.NewLine}" +
                $"可见性 :\t\t{(HideStr)}";
        }

        internal JsonObject toJson()
        {
            var ret = new JsonObject();
            ret.Add("id", Code.ToString());
            ret.Add("name", Alias);
            ret.Add("info", Info);
            if (Hide)
                ret.Add("hide", "1");
            else
                ret.Add("hide", "0");
            return ret;
        }
    }

    public class UIDClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private string uid;
        private string name;
        private string model;
        private string unit;
        private string price;

        public static UIDClass Parse(string str)
        {
            var json = JsonNode.Parse(str);
            return Parse(json);
        }

        public static UIDClass Parse(JsonNode json)
        {
            try
            {
                return new UIDClass()
                {
                    UID = json["uid"].ToString(),
                    Name = json["name"].ToString(),
                    Model = json["model"].ToString(),
                    Unit = json["unit"].ToString(),
                    Price = json["price"].ToString()
                };
            }
            catch (Exception ex)
            {
                return new UIDClass()
                {
                    UID = "出错",
                    Name = "出错",
                    Model = "出错",
                    Unit = "出错",
                    Price = "出错"
                };
            }

        }

        public string UID
        {
            get { return uid; }
            set
            {
                uid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UID)));
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
        public string Model
        {
            get { return model; }
            set
            {
                model = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Model)));
            }
        }
        public string Unit
        {
            get { return unit; }
            set
            {
                unit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Unit)));
            }
        }
        public string Price
        {
            get { return price; }
            set
            {
                price = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Price)));
            }
        }
        public override string ToString()
        {
            return $"唯一编码 : \t{UID}{Environment.NewLine}" +
                $"名称 : \t\t{Name}{Environment.NewLine}" +
                $"型号 : \t\t{Model}{Environment.NewLine}" +
                $"计量单位 : \t{Unit}{Environment.NewLine}" +
                $"单价 : (RMB)\t{Price}{Environment.NewLine}";
        }
        public JsonNode toJson()
        {
            var ret = new JsonObject();
            ret.Add("uid", UID);
            ret.Add("name", Name);
            ret.Add("model", Model);
            ret.Add("unit", Unit);
            ret.Add("price", Price);
            return ret;
        }
    }

    public class DepartClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private int id;
        private string name;

        public static DepartClass Parse(string str)
        {
            var json = JsonObject.Parse(str);
            return Parse(json);
        }

        public static DepartClass Parse(JsonNode json)
        {
            int did;
            if (int.TryParse(json["id"].ToString(), out did) != true)
                did = -1;
            return new DepartClass()
            {
                ID = did,
                Name = json["name"].ToString()
            };
        }

        public int ID
        {
            get { return id; }
            set
            {
                id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(id)));
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        public string ToIDString()
        {
            return $"部门 ID : \t{ID}{Environment.NewLine}" +
                $"部门名称 : \t{Name}";
        }
        public JsonNode toJson()
        {
            var ret = new JsonObject();
            ret.Add("id", ID.ToString());
            ret.Add("name", Name);
            return ret;
        }
    }

    public class LogClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private UIDClass uid;
        private UserClass user;
        private string shelf;
        private string unixTime;
        private string dateTime;
        private int category;
        private string categoryString;
        private int count;
        private string usageText;


        public static LogClass Parse(string str)
        {
            var json = JsonObject.Parse(str);
            return Parse(json);
        }

        public static LogClass Parse(JsonNode json)
        {
            UIDClass uid = UIDClass.Parse(json["uid"].ToString());
            UserClass user = UserClass.Parse(json["user"].ToString());
            var ca = -1;
            int.TryParse(json["action"].ToString(), out ca);
            var co = -1;
            int.TryParse(json["count"].ToString(), out co);
            var ti = json["time"].ToString();
            var usage = json["function"].ToString().Replace("{", "").Replace("}", "").Split(",");
            List<int> uin = new List<int>();
            string ust = string.Empty;
            foreach (var usa in usage)
            {
                int oint;
                if (int.TryParse(usa, out oint))
                {
                    uin.Add(oint);
                    foreach (var us in HiroUtils.usages)
                    {
                        if (us.Code == oint)
                        {
                            if (ust == string.Empty)
                                ust = us.Info;
                            else
                                ust = ust + "," + us.Info;
                        }
                    }
                }

            }
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            long lTime = long.Parse(ti + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime dtResult = dtStart.Add(toNow);
            return new LogClass()
            {
                Category = ca,
                CategoryString = ca != 0 ? "出库" : "入库",
                Count = co,
                UnixTime = ti,
                DateTime = dtResult.ToString("yyyy-MM-dd HH:mm:ss"),
                Shelf = json["sfid"].ToString(),
                UID = uid,
                User = user,
                UsageText = ust
            };
        }

        public UserClass User
        {
            get { return user; }
            set
            {
                user = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(User)));
            }
        }

        public UIDClass UID
        {
            get { return uid; }
            set
            {
                uid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UID)));
            }
        }
        public string Shelf
        {
            get { return shelf; }
            set
            {
                shelf = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Shelf)));
            }
        }

        public string UnixTime
        {
            get { return unixTime; }
            set
            {
                unixTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnixTime)));
            }
        }

        public string DateTime
        {
            get { return dateTime; }
            set
            {
                dateTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DateTime)));
            }
        }

        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            }
        }

        public int Category
        {
            get { return category; }
            set
            {
                category = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Category)));
            }
        }

        public string CategoryString
        {
            get { return categoryString; }
            set
            {
                categoryString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CategoryString)));
            }
        }

        public string UsageText
        {
            get { return usageText; }
            set
            {
                usageText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UsageText)));
            }
        }
        public override string ToString()
        {
            return $"时间:\t\t{DateTime}{Environment.NewLine}" +
                $"出\\入:\t\t{CategoryString}{Environment.NewLine}" +
                $"货架ID:\t\t{Shelf}{Environment.NewLine}" +
                $"{UID.ToString()}" +
                $"数量:\t\t{Count}{Environment.NewLine}" +
                $"{User.ToStringWithoutRole()}" +
                $"用途:\t\t{UsageText}";
        }
    }
    #region 通知项目定义
    public class Hiro_Notice
    {
        public string title;
        public string msg;
        public int time;
        public Action? act;
        public Hiro_Notice(string ms = "NULL", int ti = 1, string tit = "新消息", Action? ac = null)
        {
            msg = ms;
            time = ti;
            title = tit;
            act = ac;
        }
    }
    #endregion
}
