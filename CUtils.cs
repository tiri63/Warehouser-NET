using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
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
            var usage = json["usage"].ToString().Replace("{", "").Replace("}", "").Split(",");
            List<int> uin = new List<int>();
            string ust = string.Empty;
            foreach(var usa in usage)
            {
                int oint;
                if (int.TryParse(usa, out oint))
                {
                    uin.Add(oint);
                    foreach(var us in HiroUtils.usages)
                    {
                        if(us.Code == oint)
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

    }

    public class UserClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private string name;
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
            DepartClass depart = DepartClass.Parse(json["depart"].AsObject());
            var role = pri < 0 ? "未激活" : pri > HiroUtils.roles.Count - 1 ? "超级管理员" : HiroUtils.roles[pri];
            return new UserClass()
            {
                Name = json["username"].ToString(),
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
                $"部门信息 : \t{Depart.ToString()}{Environment.NewLine}" +
                $"角色 : \t\t{Role}";
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
            return new UIDClass()
            {
                UID = json["uid"].ToString(),
                Name = json["name"].ToString(),
                Model = json["model"].ToString(),
                Unit = json["unit"].ToString(),
                Price = json["price"].ToString()
            };
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
    }
}
