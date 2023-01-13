using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Warehouser_NET;

public class HiroUtils
{
    internal static string? userName = null;
    internal static string? userToken = null;
    internal static string? userDepart = null;
    internal static string? userNickname = null;
    //internal const string baseURL = "http://10.3.201.64/warehouser";
    internal const string baseURL = "http://101.34.8.69/warehouser";
    internal static HttpClient? hc = null;
    internal static string LogFilePath = "<current>\\users\\<hiuser>\\log\\";
    internal static string ConfigFilePath = "<current>\\users\\<hiuser>\\user.hsf";
    internal static System.Collections.ObjectModel.ObservableCollection<UsageClass> usages = new System.Collections.ObjectModel.ObservableCollection<UsageClass>();
    internal static System.Collections.ObjectModel.ObservableCollection<DepartClass> depart_all = new System.Collections.ObjectModel.ObservableCollection<DepartClass>();
    internal static List<string> roles = new List<string>() { "游客", "用户", "管理员", "超级管理员" };
    public HiroUtils()
    {

    }

    internal static JsonNode? ParseJson(string json)
    {
        LogtoFile(json);
        JsonNode? ret = null;
        try
        {
            ret = JsonObject.Parse(json);
            string retCode = ret["ret"].AsValue().ToString();
            switch (retCode)
            {
                case "0":
                    return ret;
                case "ec-001":
                    Notify("提供的参数不足", "通信错误");
                    break;
                case "ec-002":
                    Notify("未定义的操作", "服务器错误");
                    break;
                case "ec-003":
                    Notify("账户不支持这样的操作", "权限不足");
                    break;
                case "ec-004":
                    Notify("提供的参数有空", "通信错误");
                    break;
                case "ec-005":
                    Notify("提供的参数无法完成类型转换", "通信错误");
                    break;
                case "ec-006":
                    Notify("未能在数据库中找到指定内容", "数据库错误");
                    break;
                case "ec-007":
                    Notify("进行数据库操作时出错", "数据库错误");
                    break;
                case "ec-008":
                    Notify("指定的项目未找到", "同步错误");
                    break;
                case "ec-009":
                    Notify("库存已经耗尽，无法进行此操作", "同步错误");
                    break;
                case "ec-010":
                    Notify("生成输出时出错", "服务器错误");
                    break;
                case "ec-011":
                    Notify("身份验证失败", "凭据失效");
                    break;
                case "ec-012":
                    Notify("账户密码和 ID 不匹配", "登陆失败");
                    break;
                case "ec-013":
                    Notify("操作未能成功进行", "操作失败");
                    break;
                case "ec-014":
                    Notify("部分操作未能成功进行", "操作失败");
                    break;
                default:
                    Notify(ret["msg"].AsObject().ToString(), "服务器消息");
                    break;
            }
        }
        catch (Exception ex)
        {
            Notify("Json 解析失败", "通信错误");
            return null;
        }
        return ret;
    }

    internal static string SendRequest(string uri, List<string> param, List<string> value)
    {
        if (hc == null)
            hc = new HttpClient();
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{baseURL}{uri}");
        request.Headers.Add("UserAgent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.71 Safari/537.36");
        request.Content = new StringContent("");
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        int num = Math.Max(param.Count, value.Count);
        for (int i = 0; i < num; i++)
        {
            request.Headers.Add(param[i], value[i].Replace(Environment.NewLine, " "));
        }
        //这里设置协议
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;// SecurityProtocolType.Tls1.2; 
        ServicePointManager.CheckCertificateRevocationList = true;
        ServicePointManager.DefaultConnectionLimit = 100;
        ServicePointManager.Expect100Continue = false;
        try
        {

            HttpResponseMessage response = hc.SendAsync(request).Result;
            if (response.Content != null)
            {
                using (Stream stream = response.Content.ReadAsStreamAsync().Result)
                {
                    string result = string.Empty;

                    StreamReader sr = new StreamReader(stream);
                    result = sr.ReadToEnd();
                    return result;
                }
            }
            else
            {
                return string.Empty;
            }
        }
        catch
        {
        }
        return string.Empty;
    }

    internal static void InitializeConfig()
    {
        LogFilePath = Path_Prepare(LogFilePath);
        ConfigFilePath = Path_Prepare(ConfigFilePath);
        CreateFolder(Path_Prepare(LogFilePath));
        CreateFolder(Path_Prepare(ConfigFilePath));
        userName = Read_Ini(ConfigFilePath, "User", "UserName", " ");
        userToken = Read_Ini(ConfigFilePath, "User", "UserToken", " ");
        userDepart = Read_Ini(ConfigFilePath, "User", "UserDepart", " ");
        userNickname = Read_Ini(ConfigFilePath, "User", "NickName", " ");
    }
    internal static void Notify(string content, string title)
    {
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            if (App.funWindow != null)
                App.funWindow.Notify(content, title);
            else
            {
                if (title == "")
                {
                    MessageBox.Show(content, "信息 - 库存管理");
                }
                else
                {
                    MessageBox.Show(content, title);
                }
            }
        });
    }
    internal static void Notify(string content)
    {
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            if (App.funWindow != null)
                App.funWindow.Notify(content, "");
            else
                MessageBox.Show(content, "信息 - 库存管理");
        });
    }

    internal static void ExitApp()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Application.Current.Shutdown(Environment.ExitCode);
        });

    }

    #region 读Ini文件
    public static string Read_Ini(string iniFilePath, string Section, string Key, string defaultText)
    {
        if (File.Exists(iniFilePath))
        {
            byte[] buffer = new byte[1024];
            int ret = GetPrivateProfileString(Encoding.GetEncoding("utf-8").GetBytes(Section), Encoding.GetEncoding("utf-8").GetBytes(Key), Encoding.GetEncoding("utf-8").GetBytes(defaultText), buffer, 1024, iniFilePath);
            return DeleteUnVisibleChar(Encoding.GetEncoding("utf-8").GetString(buffer, 0, ret)).Trim();
        }
        else
        {
            var pFilePath = Path_Prepare(Path_Prepare_EX(iniFilePath));
            if (File.Exists(pFilePath))
            {
                byte[] buffer = new byte[1024];
                int ret = GetPrivateProfileString(Encoding.GetEncoding("utf-8").GetBytes(Section), Encoding.GetEncoding("utf-8").GetBytes(Key), Encoding.GetEncoding("utf-8").GetBytes(defaultText), buffer, 1024, pFilePath);
                return DeleteUnVisibleChar(Encoding.GetEncoding("utf-8").GetString(buffer, 0, ret)).Trim();
            }
            else
                return defaultText;
        }
    }
    #endregion

    #region 写Ini文件
    public static bool Write_Ini(string iniFilePath, string Section, string Key, string Value)
    {
        try
        {
            var pFilePath = Path_Prepare(Path_Prepare_EX(iniFilePath));
            if (!File.Exists(pFilePath))
                File.Create(pFilePath).Close();
            long OpStation = WritePrivateProfileString(Encoding.GetEncoding("utf-8").GetBytes(Section), Encoding.GetEncoding("utf-8").GetBytes(Key), Encoding.GetEncoding("utf-8").GetBytes(Value), pFilePath);
            if (OpStation == 0)
                return false;
            else
                return true;
        }
        catch (Exception ex)
        {
            LogError(ex, "Exception.Config.Update");
            return false;
        }

    }
    #endregion

    #region 新建完全限定路径文件夹
    public static bool CreateFolder(string path)
    {
        int pos = path.IndexOf("\\") + 1;
        string vpath;
        DirectoryInfo? di;
        try
        {
            while (pos > 0)
            {
                vpath = path[..pos];
                pos = path.IndexOf("\\", pos) + 1;
                di = new DirectoryInfo(vpath);
                if (!di.Exists)
                    di.Create();
            }
        }
        catch (Exception ex)
        {
            LogError(ex, $"Exception.Directory.Create");
            return false;
        }
        return true;

    }
    #endregion

    #region 写日志相关

    public static void LogError(Exception ex, string Module)
    {
        StringBuilder str = new StringBuilder();
        if (ex.InnerException == null)
        {
            str.Append($"{Environment.NewLine}[ERROR]{Module}{Environment.NewLine}");
            str.Append($"Object: {ex.Source}{Environment.NewLine}");
            str.Append($"Exception: {ex.GetType().Name}{Environment.NewLine}");
            str.Append($"Details: {ex.Message}");
            str.Append($"StackTrace: {ex.StackTrace}");
        }
        else
        {
            str.Append($"{Environment.NewLine}[ERROR]{Module}.InnerException{Environment.NewLine}");
            str.Append($"Object: {ex.InnerException.Source}{Environment.NewLine}");
            str.Append($"Exception: {ex.InnerException.GetType().Name}{Environment.NewLine}");
            str.Append($"Details: {ex.InnerException.Message}");
            str.Append($"StackTrace: {ex.InnerException.StackTrace}");
        }
        Notify($"错误位置 : {Module}{Environment.NewLine}具体信息请查看日志", "发生错误");
        LogtoFile(str.ToString());
    }

    public static void LogtoFile(string val)
    {
        try
        {
            var filePath = Path_Prepare(LogFilePath) + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            if (!File.Exists(filePath))
                File.Create(filePath).Close();
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
            StreamReader sr = new StreamReader(fs);
            var str = sr.ReadToEnd();
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(DateTime.Now.ToString("[HH:mm:ss]") + val + Environment.NewLine);
            sw.Flush();
            sw.Close();
            sr.Close();
            sr.Dispose();
            fs.Close();
        }
        catch (Exception ex)
        {
            try
            {
                LogError(ex, "Exception.Log");
            }
            catch
            {

            }
        }
    }
    #endregion

    #region 内置变量

    public static string Path_Replace(string path, string toReplace, string replaced, bool CaseSensitive = false)
    {
        var resu = (replaced.EndsWith("\\")) ? replaced[0..^1] : replaced;
        if (CaseSensitive)
            resu = path.Replace(toReplace, resu);
        else
            resu = Strings.Replace(path, toReplace, resu, 1, -1, CompareMethod.Text);
        if (resu != null)
            return resu;
        else
            return "";
    }

    public static String Path_Prepare(string path)
    {
        path = Path_Replace(path, "<current>", AppDomain.CurrentDomain.BaseDirectory);
        path = Path_Replace(path, "<system>", Environment.SystemDirectory);
        path = Path_Replace(path, "<systemx86>", Microsoft.WindowsAPICodePack.Shell.KnownFolders.SystemX86.Path);
        path = Path_Replace(path, "<idesktop>", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        path = Path_Replace(path, "<ideskdir>", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        path = Path_Replace(path, "<cdeskdir>", Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory));
        path = Path_Replace(path, "<idocument>", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        path = Path_Replace(path, "<cdocument>", Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments));
        path = Path_Replace(path, "<iappdata>", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        path = Path_Replace(path, "<cappdata>", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        path = Path_Replace(path, "<imusic>", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
        path = Path_Replace(path, "<cmusic>", Environment.GetFolderPath(Environment.SpecialFolder.CommonMusic));
        path = Path_Replace(path, "<ipicture>", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
        path = Path_Replace(path, "<cpicture>", Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures));
        path = Path_Replace(path, "<istart>", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
        path = Path_Replace(path, "<cstart>", Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));
        path = Path_Replace(path, "<istartup>", Environment.GetFolderPath(Environment.SpecialFolder.Startup));
        path = Path_Replace(path, "<cstartup>", Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup));
        path = Path_Replace(path, "<ivideo>", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));
        path = Path_Replace(path, "<cvideo>", Environment.GetFolderPath(Environment.SpecialFolder.CommonVideos));
        path = Path_Replace(path, "<iprogx86>", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
        path = Path_Replace(path, "<cprogx86>", Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86));
        path = Path_Replace(path, "<iprog>", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
        path = Path_Replace(path, "<cprog>", Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles));
        path = Path_Replace(path, "<idownload>", Microsoft.WindowsAPICodePack.Shell.KnownFolders.Downloads.Path);
        path = Path_Replace(path, "<cdownload>", Microsoft.WindowsAPICodePack.Shell.KnownFolders.PublicDownloads.Path);
        path = Path_Replace(path, "<win>", Environment.GetFolderPath(Environment.SpecialFolder.Windows));
        path = Path_Replace(path, "<recent>", Environment.GetFolderPath(Environment.SpecialFolder.Recent));
        path = Path_Replace(path, "<profile>", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        path = Path_Replace(path, "<sendto>", Environment.GetFolderPath(Environment.SpecialFolder.SendTo));
        path = Path_Replace(path, "<hiuser>", Environment.UserName);
        path = Path_Replace(path, "<nop>", "");
        return path;
    }

    public static String Path_Prepare_EX(String path)
    {
        path = Path_Replace(path, "<yyyyy>", DateTime.Now.ToString("yyyyy"));
        path = Path_Replace(path, "<yyyy>", DateTime.Now.ToString("yyyy"));
        path = Path_Replace(path, "<yyy>", DateTime.Now.ToString("yyy"));
        path = Path_Replace(path, "<yy>", DateTime.Now.ToString("yy"));
        path = Path_Replace(path, "<MMMM>", DateTime.Now.ToString("MMMM"));
        path = Path_Replace(path, "<MMM>", DateTime.Now.ToString("MMM"));
        path = Path_Replace(path, "<MM>", DateTime.Now.ToString("MM"), true);
        path = Path_Replace(path, "<M>", DateTime.Now.ToString("M"), true);
        path = Path_Replace(path, "<dddd>", DateTime.Now.ToString("dddd"));
        path = Path_Replace(path, "<ddd>", DateTime.Now.ToString("ddd"));
        path = Path_Replace(path, "<dd>", DateTime.Now.ToString("dd"));
        path = Path_Replace(path, "<d>", DateTime.Now.ToString("d"));
        path = Path_Replace(path, "<HH>", DateTime.Now.ToString("HH"), true);
        path = Path_Replace(path, "<hh>", DateTime.Now.ToString("hh"), true);
        path = Path_Replace(path, "<mm>", DateTime.Now.ToString("mm"), true);
        path = Path_Replace(path, "<m>", DateTime.Now.ToString("m"), true);
        path = Path_Replace(path, "<ss>", DateTime.Now.ToString("ss"));
        path = Path_Replace(path, "<s>", DateTime.Now.ToString("s"));
        path = Path_Replace(path, "<date>", DateTime.Now.ToString("yyyyMMdd"));
        path = Path_Replace(path, "<time>", DateTime.Now.ToString("HHmmss"));
        path = Path_Replace(path, "<now>", DateTime.Now.ToString("yyMMddHHmmss"));
        return path;
    }

    #endregion


    #region 读文件
    [DllImport("kernel32")]//返回0表示失败，非0为成功
    private static extern long WritePrivateProfileString(byte[] section, byte[] key, byte[] val, string filePath);
    [DllImport("kernel32")]//返回取得字符串缓冲区的长度
    private static extern int GetPrivateProfileString(byte[] section, byte[] key, byte[] def, byte[] retVal, int size, string filePath);
    #endregion

    public static string DeleteUnVisibleChar(string sourceString)
    {
        StringBuilder sBuilder = new StringBuilder(131);
        for (int i = 0; i < sourceString.Length; i++)
        {
            int Unicode = sourceString[i];
            if (Unicode >= 16)
            {
                sBuilder.Append(sourceString[i]);
            }
        }
        return sBuilder.ToString();
    }

    #region 添加double动画
    public static Storyboard AddDoubleAnimaton(double? to, double mstime, DependencyObject value, string PropertyPath, Storyboard? sb, double? from = null)
    {
        sb ??= new Storyboard();
        DoubleAnimation? da = new DoubleAnimation();
        if (from != null)
            da.From = from;
        if (to != null)
            da.To = to;
        da.Duration = TimeSpan.FromMilliseconds(mstime);
        da.DecelerationRatio = 0.9;
        Storyboard.SetTarget(da, value);
        Storyboard.SetTargetProperty(da, new PropertyPath(PropertyPath));
        sb.Children.Add(da);
        sb.FillBehavior = FillBehavior.Stop;
        sb.Completed += delegate
        {
            da = null;
            sb = null;
        };
        return sb;
    }
    #endregion

    #region 添加thickness动画
    public static Storyboard AddThicknessAnimaton(Thickness? to, double mstime, DependencyObject value, string PropertyPath, Storyboard? sb, Thickness? from = null, double DecelerationRatio = 0.9)
    {
        sb ??= new Storyboard();
        ThicknessAnimation? da = new ThicknessAnimation();
        if (from != null)
            da.From = from;
        if (to != null)
            da.To = to;
        da.Duration = TimeSpan.FromMilliseconds(mstime);
        da.DecelerationRatio = DecelerationRatio;
        Storyboard.SetTarget(da, value);
        Storyboard.SetTargetProperty(da, new PropertyPath(PropertyPath));
        sb.Children.Add(da);
        sb.FillBehavior = FillBehavior.Stop;
        sb.Completed += delegate
        {
            da = null;
            sb = null;
        };
        return sb;
    }
    #endregion

    #region 添加Color动画
    public static Storyboard AddColorAnimaton(System.Windows.Media.Color to, double mstime, DependencyObject value, string PropertyPath, Storyboard? sb, System.Windows.Media.Color? from = null)
    {
        sb ??= new Storyboard();
        ColorAnimation? da;
        if (from != null)
            da = new ColorAnimation((System.Windows.Media.Color)from, to, TimeSpan.FromMilliseconds(mstime));
        else
            da = new ColorAnimation(to, TimeSpan.FromMilliseconds(mstime));
        da.DecelerationRatio = 0.9;
        Storyboard.SetTarget(da, value);
        Storyboard.SetTargetProperty(da, new PropertyPath(PropertyPath));
        sb.Children.Add(da);
        sb.FillBehavior = FillBehavior.Stop;
        sb.Completed += delegate
        {
            da = null;
            sb = null;
        };
        return sb;
    }
    #endregion

    #region 增强动效
    public static Storyboard AddPowerAnimation(int Direction, FrameworkElement value, Storyboard? sb, double? from = null, double? to = null)
    {
        sb ??= new Storyboard();
        var th1 = value.Margin;
        var th2 = value.Margin;
        if (to != null && from != null)
        {
            if (Direction == 0)
            {
                th1.Left += (double)from;
                th2.Left += (double)to;
            }
            if (Direction == 1)
            {
                th1.Top += (double)from;
                th2.Top += (double)to;
            }
            if (Direction == 2)
            {
                th1.Right += (double)from;
                th2.Right += (double)to;
            }
            if (Direction == 3)
            {
                th1.Bottom += (double)from;
                th2.Bottom += (double)to;
            }
            AddThicknessAnimaton(th2, 450, value, "Margin", sb, th1);
        }
        if (to != null && from == null)
        {
            if (Direction == 0)
            {
                th2.Left += (double)to;
            }
            if (Direction == 1)
            {
                th2.Top += (double)to;
            }
            if (Direction == 2)
            {
                th2.Right += (double)to;
            }
            if (Direction == 3)
            {
                th2.Bottom += (double)to;
            }
            AddThicknessAnimaton(th2, 450, value, "Margin", sb, null);
        }
        if (to == null && from != null)
        {
            if (Direction == 0)
            {
                th1.Left += (double)from;
            }
            if (Direction == 1)
            {
                th1.Top += (double)from;
            }
            if (Direction == 2)
            {
                th1.Right += (double)from;
            }
            if (Direction == 3)
            {
                th1.Bottom += (double)from;
            }
            AddThicknessAnimaton(null, 450, value, "Margin", sb, th1);
        }
        AddDoubleAnimaton(null, 350, value, "Opacity", sb, 0);
        return sb;
    }
    #endregion

    /// <summary>
    /// 对字符进行UrlEncode编码
    /// string转Encoding格式
    /// </summary>
    /// <param name="text"></param>
    /// <param name="encod">编码格式</param>
    /// <param name="cap">是否输出大写字母</param>
    /// <returns></returns>
    public static string UrlEncode(string text, Encoding encod, bool cap = true)
    {
        if (cap)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in text)
            {
                if (System.Web.HttpUtility.UrlEncode(c.ToString(), encod).Length > 1)
                {
                    builder.Append(System.Web.HttpUtility.UrlEncode(c.ToString(), encod).ToUpper());
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }
        else
        {
            string encodString = System.Web.HttpUtility.UrlEncode(text, encod);
            return encodString;
        }
    }

    /// <summary>
    /// 对字符进行UrlDecode解码
    /// Encoding转string格式
    /// </summary>
    /// <param name="encodString"></param>
    /// <param name="encod">编码格式</param>
    /// <returns></returns>
    public static string UrlDecode(string encodString, Encoding encod)
    {
        string text = System.Web.HttpUtility.UrlDecode(encodString, encod);
        return text;
    }
    public static int GetPage(int num)
    {
        if (num % 20 == 0)
            return num / 20;
        else
            return num / 20 + 1;
    }

    public static bool getDeparts()
    {
        HiroInvoke(() =>
        {
            depart_all.Clear();
        });
        try
        {
            var jo = ParseJson(SendRequest("/depart", new List<string>() { "action" }, new List<string>() { "2" }));
            if (jo != null)
            {
                var ja = jo["msg"].AsArray();
                for (int i = 0; i < ja.Count; i++)
                {
                    HiroInvoke(() =>
                    {
                        depart_all.Add(DepartClass.Parse(ja[i]));
                    });

                };
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            HiroInvoke(() =>
            {
                LogError(ex, "Exception.Depart.Get");
            });
            return false;
        }
    }

    internal static void HiroInvoke(Action callback)
    {
        Application.Current.Dispatcher.Invoke(callback);
    }
}
