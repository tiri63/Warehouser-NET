using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System;
using System.Windows;
using System.Windows.Controls;
using static ICSharpCode.SharpZipLib.Zip.ExtendedUnixData;
using System.Windows.Input;
using Microsoft.Win32;
using NPOI.XSSF.UserModel;
using System.IO;

namespace Warehouser_NET
{
    /// <summary>
    /// Page_Settings.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_Log : Page
    {
        internal System.Collections.ObjectModel.ObservableCollection<LogClass> log_all = new System.Collections.ObjectModel.ObservableCollection<LogClass>();
        internal System.Collections.ObjectModel.ObservableCollection<LogClass> log_search = new System.Collections.ObjectModel.ObservableCollection<LogClass>();
        internal System.Collections.ObjectModel.ObservableCollection<LogClass> log_p = new System.Collections.ObjectModel.ObservableCollection<LogClass>();
        internal int flag = 0;
        internal bool isolated = false;
        internal int page = 0;
        internal int index = 0;
        private FunWindow? parent = null;
        internal long? time_start = null;
        internal long? time_end = null;
        public Page_Log(FunWindow parent)
        {
            InitializeComponent();
            ItemData.ItemsSource = log_p;
            new Thread(() =>
            {
                GetItems();
            }).Start();
            this.parent = parent;
        }

        private bool GetItems()
        {
            Dispatcher.Invoke(() =>
            {
                ItemPanel.IsEnabled = false;
                ShowMsg("服务器通信", "正在从服务器上获取最新数据……", true);
                log_all.Clear();
            });
            try
            {
                List<string> para = new List<string>();
                List<string> value = new List<string>();
                if (time_start == null)
                {
                    if (time_end == null)
                    {
                        para = new List<string>() { "action", "username", "token", "device" };
                        value = new List<string>() { "0", HiroUtils.userName, HiroUtils.userToken, "PC" };
                    }
                    else
                    {
                        para = new List<string>() { "action", "username", "token", "device", "end" };
                        value = new List<string>() { "0", HiroUtils.userName, HiroUtils.userToken, "PC", time_end.ToString() };
                    }
                }
                else
                {
                    if (time_end == null)
                    {
                        para = new List<string>() { "action", "username", "token", "device", "start" };
                        value = new List<string>() { "0", HiroUtils.userName, HiroUtils.userToken, "PC", time_start.ToString() };
                    }
                    else
                    {
                        if (time_start < time_end)
                        {
                            para = new List<string>() { "action", "username", "token", "device", "start", "end" };
                            value = new List<string>() { "0", HiroUtils.userName, HiroUtils.userToken, "PC", time_start.ToString(), time_end.ToString() };
                        }
                        else
                        {
                            para = new List<string>() { "action", "username", "token", "device", "end", "start" };
                            value = new List<string>() { "0", HiroUtils.userName, HiroUtils.userToken, "PC", time_start.ToString(), time_end.ToString() };
                        }

                    }
                }
                var jo = HiroUtils.ParseJson(HiroUtils.SendRequest("/log", para, value));
                if (jo != null)
                {
                    var ja = jo["msg"].AsArray();
                    Dispatcher.Invoke(() =>
                    {
                        var ap = flag == 0 ? log_all.Count % 20 == 0 ? log_all.Count / 20 + 1 : log_all.Count / 20 : log_search.Count % 20 == 0 ? log_search.Count / 20 + 1 : log_search.Count / 20;
                        StatusLabel.Content = string.Format("第 {0}/{1} 页 共计{2}项", 1, ap, ja.Count);
                    });
                    for (int i = 0; i < ja.Count; i++)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            log_all.Add(LogClass.Parse(ja[i]));
                        });
                    };
                    Dispatcher.Invoke(() =>
                    {
                        ItemPanel.IsEnabled = false;
                        Load_Page(1);
                        HideMsg();
                    });
                    flag = 0;
                    return true;
                }
                Dispatcher.Invoke(() =>
                {
                    ItemPanel.IsEnabled = false;
                    HideMsg();
                });
                flag = 0;
                return false;

            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    ItemPanel.IsEnabled = false;
                    HideMsg();
                    HiroUtils.LogError(ex, "Exception.Log.Get");
                });
                return false;
            }

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HiroUtils.AddPowerAnimation(0, BaseGrid, null, 50).Begin();
        }

        private void Load_Page(int p)
        {
            var ap = flag == 0 ? HiroUtils.GetPage(log_all.Count) : HiroUtils.GetPage(log_search.Count);
            var ac = flag == 0 ? log_all.Count : log_search.Count;
            if ((p <= ap || ap == 0) && p >= 1)
            {
                p--;
                log_p.Clear();
                page = p;
                for (var i = 0; i < 20; i++)
                {
                    if (p * 20 + i >= ac)
                        break;
                    var ui = flag == 0 ? log_all[p * 20 + i] : log_search[p * 20 + i];
                    log_p.Add(ui);
                }
                StatusLabel.Content = string.Format("第 {0}/{1} 页 共计{2}项", p + 1, ap, ac);
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Height > 60)
                sv.MaxHeight = Height - 60;
        }

        private void ShowMsg(string title, string? content, bool progress = false, bool button = false)
        {
            MsgProgressBar.Visibility = progress ? Visibility.Visible : Visibility.Collapsed;
            MsgOKButton.Visibility = button ? Visibility.Visible : Visibility.Collapsed;
            MsgGrid.Visibility = Visibility.Visible;
            MsgContent.Visibility = content == null ? Visibility.Collapsed : Visibility.Visible;
            ItemPanel.IsEnabled = false;
            MsgTitle.Content = title;
            if (content != null)
                MsgContent.Content = content;
            HiroUtils.AddPowerAnimation(1, MsgGrid, null, 50).Begin();
        }

        private void HideMsg()
        {
            var sb = HiroUtils.AddDoubleAnimaton(0, 150, MsgGrid, "Opacity", null);
            sb.Completed += delegate
            {
                MsgGrid.Visibility = Visibility.Collapsed;
                ItemPanel.IsEnabled = true;
            };
            sb.Begin();
        }

        private void ItemData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ItemData.SelectedIndex != -1)
            {
                index = ItemData.SelectedIndex;
                ShowDetail(ItemData.SelectedIndex);
            }
        }

        private void ShowDetail(int index)
        {
            Title_Label.Content = "详细信息";
            var target = flag == 0 ? log_all[index] : log_search[index];
            ContentText.Text = target.ToString();
            DetailGrid.Visibility = Visibility.Visible;
            ItemPanel.IsEnabled = false;
            HiroUtils.AddPowerAnimation(1, DetailGrid, null, 50).Begin();
        }

        private void HideDetail()
        {
            var sb = HiroUtils.AddDoubleAnimaton(0, 150, DetailGrid, "Opacity", null);
            sb.Completed += delegate
            {
                DetailGrid.Visibility = Visibility.Collapsed;
                ItemPanel.IsEnabled = true;
            };
            sb.Begin();
        }

        private void RefreashBtn_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                GetItems();
            }).Start();
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            HideDetail();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            HideDetail();
        }

        private void PageMinus_Click(object sender, RoutedEventArgs e)
        {
            if (page > 1)
            {
                page--;
                Load_Page(page);
            }
        }

        private void PagePlus_Click(object sender, RoutedEventArgs e)
        {
            var ap = flag == 0 ? HiroUtils.GetPage(log_all.Count) : HiroUtils.GetPage(log_search.Count);
            if (page <= ap)
            {
                page++;
                Load_Page(page);
            }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog ofd = new SaveFileDialog()
            {
                Filter = "Excel 2007+ 格式|*.xlsx",
                ValidateNames = true, // 验证用户输入是否是一个有效的Windows文件名
                CheckPathExists = true,//验证路径的有效性
                Title = "保存文件 - 库存管理"
            };
            if (ofd.ShowDialog() == true) //用户点击确认按钮，发送确认消息
            {
                if (File.Exists(@ofd.FileName))
                {
                    try
                    {
                        File.Delete(@ofd.FileName);
                    }
                    catch (Exception ex)
                    {
                        HiroUtils.LogError(ex, "Exception.File.Export.DeleteExist");
                    }

                }
                new Thread(() =>
                {
                    try
                    {
                        ExportTemplate(@ofd.FileName);
                    }
                    catch (Exception ex)
                    {
                        HiroUtils.LogError(ex, "Exception.File.Export");
                    }
                }).Start();
            }
        }

        internal void ExportTemplate(string path, string? time = null)
        {
            var excel = new XSSFWorkbook();
            var sheet = excel.CreateSheet("日志导出");
            var sheeet = excel.CreateSheet("附加信息");
            sheeet.CreateRow(0).CreateCell(0).SetCellValue("导出时间");
            sheeet.CreateRow(0).CreateCell(1).SetCellValue(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sheeet.CreateRow(1).CreateCell(0).SetCellValue("导出账户");
            sheeet.CreateRow(1).CreateCell(1).SetCellValue(HiroUtils.userNickname + "(" + HiroUtils.userName + ")");
            var row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("时间");
            row.CreateCell(1).SetCellValue("出\\入");
            row.CreateCell(2).SetCellValue("货架 ID");
            row.CreateCell(3).SetCellValue("唯一编码");
            row.CreateCell(4).SetCellValue("名称");
            row.CreateCell(5).SetCellValue("型号");
            row.CreateCell(6).SetCellValue("计量单位");
            row.CreateCell(7).SetCellValue("单价");
            row.CreateCell(8).SetCellValue("数量");
            row.CreateCell(9).SetCellValue("账户");
            row.CreateCell(10).SetCellValue("名称");
            row.CreateCell(11).SetCellValue("部门");
            row.CreateCell(12).SetCellValue("用途");
            var count = flag == 0 ? log_all.Count : log_search.Count;
            for (int i = 0; i < count; i++)
            {
                var item = flag == 0 ? log_all[i] : log_search[i];
                var r = sheet.CreateRow(1 + i);
                r.CreateCell(0).SetCellValue(item.DateTime);
                r.CreateCell(1).SetCellValue(item.CategoryString);
                r.CreateCell(2).SetCellValue(item.Shelf);
                r.CreateCell(3).SetCellValue(item.UID.UID);
                r.CreateCell(4).SetCellValue(item.UID.Name);
                r.CreateCell(5).SetCellValue(item.UID.Model);
                r.CreateCell(6).SetCellValue(item.UID.Unit);
                r.CreateCell(7).SetCellValue(item.UID.Price);
                r.CreateCell(8).SetCellValue(item.Count);
                r.CreateCell(9).SetCellValue(item.User.Name);
                r.CreateCell(10).SetCellValue(item.User.Nickname);
                r.CreateCell(11).SetCellValue(item.User.Depart.Name);
                r.CreateCell(12).SetCellValue(item.UsageText);
            }
            FileStream stream = File.OpenWrite(@path);
            excel.Write(stream);
            stream.Close();
        }

        private void ResetTime_Click(object sender, RoutedEventArgs e)
        {
            DPStart.SelectedDate = null;
            DPEnd.SelectedDate = null;
            time_start = null;
            time_end = null;
            new Thread(() =>
            {
                GetItems();
            }).Start();
        }

        private void DPStart_CalendarClosed(object sender, RoutedEventArgs e)
        {
            var ttime = time_start;
            var d = DPStart.SelectedDate;
            if (d == null)
                time_start = null;
            else
            {
                time_start = ConvertDataTimeLong(new DateTime(d.Value.Year, d.Value.Month, d.Value.Day, 0, 0, 0));
            }
            if (time_start != ttime)
                new Thread(() =>
                {
                    GetItems();
                }).Start();
        }

        public static long ConvertDataTimeLong(DateTime dt)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0));
            TimeSpan toNow = dt.Subtract(dtStart);
            long timeStamp = toNow.Ticks;
            timeStamp = long.Parse(timeStamp.ToString().Substring(0, timeStamp.ToString().Length - 7));
            return timeStamp;
        }

        private void DPEnd_CalendarClosed(object sender, RoutedEventArgs e)
        {
            var ttime = time_end;
            var d = DPStart.SelectedDate;
            if (d == null)
                time_end = null;
            else
            {
                time_end = ConvertDataTimeLong(new DateTime(d.Value.Year, d.Value.Month, d.Value.Day, 0, 0, 0));
            }
            if (time_end != ttime)
                new Thread(() =>
                {
                    GetItems();
                }).Start();
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            ResetBtn.IsEnabled = false;
            if (MessageBox.Show("清空日志会导致所有日志丢失！请确定已经导出所有必要的数据！" + Environment.NewLine + "是否继续？", "警告！！！ - 库存管理", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                new Thread(() =>
                {
                    HiroUtils.ParseJson(HiroUtils.SendRequest("/log", new List<string>() { "action", "username", "token", "device" },
                            new List<string>() { "3", HiroUtils.userName, HiroUtils.userToken, "PC" }));
                    Dispatcher.Invoke(() =>
                    {
                        ResetBtn.IsEnabled = true;
                    });
                }).Start();
            }
            else
                ResetBtn.IsEnabled = true;
        }
    }
}
