using Microsoft.Win32;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ICSharpCode.SharpZipLib.Zip.ExtendedUnixData;

namespace Warehouser_NET
{
    /// <summary>
    /// Import_User.xaml の相互作用ロジック
    /// </summary>
    public partial class Import_User : Page
    {
        internal bool isolated = false;
        private FunWindow? parent = null;
        internal int page = 0;
        internal int index = 0;
        internal int fflag = 0;
        internal System.Collections.ObjectModel.ObservableCollection<UserClass> iusers = new System.Collections.ObjectModel.ObservableCollection<UserClass>();
        internal System.Collections.ObjectModel.ObservableCollection<UserClass> iusersp = new System.Collections.ObjectModel.ObservableCollection<UserClass>();
        public Import_User()
        {
            InitializeComponent();
            ItemData.ItemsSource = iusersp;
            Loaded += delegate
            {
                new System.Threading.Thread(() =>
                {
                    HiroUtils.getDeparts();
                    Dispatcher.Invoke(() =>
                    {
                        DepartCombo.Items.Clear();
                        for(int i = 0;i<HiroUtils.depart_all.Count;i++)
                        {
                            DepartCombo.Items.Add(HiroUtils.depart_all[i]);
                        }
                        RoleCombo.Items.Clear();
                        for (int i = 0; i < HiroUtils.roles.Count; i++)
                        {
                            RoleCombo.Items.Add(HiroUtils.roles[i]);
                        }
                    });
                }).Start();
            };
        }

        internal void ExportTemplate(string path)
        {
            var excel = new XSSFWorkbook();
            var sheet = excel.CreateSheet("用户导入模板");
            excel.CreateSheet("附加信息").CreateRow(0).CreateCell(0).SetCellValue("1.0");
            var row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("用户名（英文&数字）");
            row.CreateCell(1).SetCellValue("密码");
            row.CreateCell(2).SetCellValue("昵称");
            row.CreateCell(3).SetCellValue("部门ID");
            row.CreateCell(4).SetCellValue("权限等级");
            row = sheet.CreateRow(1);
            row.CreateCell(0).SetCellValue("zhangsan");
            row.CreateCell(1).SetCellValue("abcd123");
            row.CreateCell(2).SetCellValue("张三");
            row.CreateCell(3).SetCellValue("1");
            row.CreateCell(4).SetCellValue("1");
            row.CreateCell(5).SetCellValue("此行为示例数据，导入时会直接忽略。请不要在此行填写真实数据！权限等级：0->游客，只能查看，1->用户，可出入库，2->管理员，可出入库、导入等。");
            FileStream stream = File.OpenWrite(@path);
            excel.Write(stream);
            stream.Close();
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
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
                        HiroUtils.LogError(ex, "Exception.File.SaveTemplate.DeleteExist");
                    }

                }
                new System.Threading.Thread(() =>
                {
                    try
                    {
                        ExportTemplate(@ofd.FileName);
                    }
                    catch (Exception ex)
                    {
                        HiroUtils.LogError(ex, "Exception.File.SaveTemplate");
                    }
                }).Start();
            }
        }

        private void ItemData_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ItemData.SelectedIndex != -1)
            {
                index = ItemData.SelectedIndex;
                ShowDetail(ItemData.SelectedIndex);
            }
        }


        private void ShowDetail(int index)
        {
            fflag = 0;
            var target = iusers[index];
            IDText.Text = target.Name;
            NameText.Text = target.Nickname;
            PwdText.Password = target.Password;
            DepartCombo.SelectedIndex = target.Depart.ID;
            var i = target.Privilege;
            RoleCombo.SelectedIndex = i >= 0 && i < HiroUtils.roles.Count ? i : i < 0 ? 0 : HiroUtils.roles.Count - 1;
            DetailGrid.Visibility = Visibility.Visible;
            ItemPanel.IsEnabled = false;
            HiroUtils.AddPowerAnimation(1, DetailGrid, null, 50).Begin();
        }


        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            HideDetail();
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


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HiroUtils.AddPowerAnimation(0, BaseGrid, null, 50).Begin();
        }

        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Excel 文件|*.xls;*.xlsx;*.xlsm|所有文件|*.*",
                ValidateNames = true, // 验证用户输入是否是一个有效的Windows文件名
                CheckFileExists = true, //验证路径的有效性
                CheckPathExists = true,//验证路径的有效性
                Title = "打开文件 - 库存管理"
            };
            if (ofd.ShowDialog() == true) //用户点击确认按钮，发送确认消息
            {
                if (File.Exists(@ofd.FileName))
                {
                    new System.Threading.Thread(() =>
                    {
                        ImportFromFile(@ofd.FileName);
                    }).Start();
                }
            }
        }

        internal bool ImportFromFile(string path)
        {
            Dispatcher.Invoke(() =>
            {
                iusers.Clear();
            });
            try
            {
                var excel = WorkbookFactory.Create(path);
                if (excel.NumberOfSheets < 2)
                {
                    return false;
                }
                var versheet = excel.GetSheetAt(1);
                if (versheet.LastRowNum < 0)
                {
                    return false;
                }
                var verrow = versheet.GetRow(0);
                if (verrow.LastCellNum < 0)
                {
                    return false;
                }
                if (GetCellValue(verrow.GetCell(0)).Equals("1.0"))
                {
                    versheet = excel.GetSheetAt(0);
                    if (versheet.LastRowNum < 2)
                    {
                        return false;//no data found
                    }
                    for (int i = 2; i <= versheet.LastRowNum; i++)
                    {
                        var row = versheet.GetRow(i);
                        if (row.LastCellNum < 5)
                        {
                            break;
                        }
                        Dispatcher.Invoke(() =>
                        {
                            var i = -1;
                            int.TryParse(GetCellValue(row.GetCell(4)).ToString(), out i);
                            if (i > 3)
                                i = 3;
                            var d = -1;
                            int.TryParse(GetCellValue(row.GetCell(3)).ToString(), out d);
                            var r = i >= 0 && i <= HiroUtils.roles.Count ? HiroUtils.roles[i] : i < 0 ? HiroUtils.roles[0] : HiroUtils.roles.Last();
                            iusers.Add(new UserClass()
                            {
                                Name = GetCellValue(row.GetCell(0)),
                                Password = GetCellValue(row.GetCell(1)),
                                Nickname = GetCellValue(row.GetCell(2)),
                                Depart = HiroUtils.depart_all.Where(x => x.ID == d).FirstOrDefault(),
                                Privilege = i + 1,
                                Role = r
                            });
                        });

                    }
                    Dispatcher.Invoke(() =>
                    {
                        Load_Part();
                    });
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                HiroUtils.LogError(ex, "Exception.ImportFromFile.Parse");
                return false;
            }
        }

        private void ImportClipboard_Click(object sender, RoutedEventArgs e)
        {
            ImportClipboard.IsEnabled = false;
            new System.Threading.Thread(() =>
            {
                ImportFromClipboard();
            }).Start();
        }

        private void ImportFromClipboard()
        {
            string data = string.Empty;
            Dispatcher.Invoke(() =>
            {
                data = Clipboard.GetData(DataFormats.Text).ToString();
            });
            foreach (var item in data.Split(Environment.NewLine))
            {
                var isp = item.Split("\t");
                if (isp.Length >= 5)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var i = -1;
                        int.TryParse(isp[4], out i);
                        if (i > 3)
                            i = 3;
                        var d = -1;
                        int.TryParse(isp[3], out d);
                        var r = i >= 0 && i <= HiroUtils.roles.Count ? HiroUtils.roles[i] : i < 0 ? HiroUtils.roles[0] : HiroUtils.roles.Last();
                        iusers.Add(new UserClass()
                        {
                            Name = isp[0],
                            Password = isp[1],
                            Nickname = isp[2],
                            Depart = HiroUtils.depart_all.Where(x => x.ID == d).FirstOrDefault(),
                            Privilege = i,
                            Role = r
                        });
                    });
                }
            }
            Dispatcher.Invoke(() =>
            {
                ImportClipboard.IsEnabled = true;
                Load_Part();
            });
        }

        private void Load_Page(int p)
        {
            if (iusers.Count == 0)
            {
                iusersp.Clear();
                StatusLabel.Content = string.Format("第 {0}/{1} 页 共计{2}项", 1, 1, iusers.Count);
            }
            if (p * 20 <= HiroUtils.GetPage(iusers.Count))
            {
                page = p;
                iusersp.Clear();
                for (var i = 0; i < 20; i++)
                {
                    if (p * 20 + i >= iusers.Count)
                        break;
                    var ui = iusers[p * 20 + i];
                    iusersp.Add(ui);
                }
                StatusLabel.Content = string.Format("第 {0}/{1} 页 共计{2}项", p + 1, HiroUtils.GetPage(iusers.Count), iusers.Count);
            }
        }


        private void Load_Part()
        {
            Load_Page(page);
        }

        private string GetCellValue(ICell? cell)
        {
            if (cell == null)
                return string.Empty;
            switch (cell.CellType)
            {
                case CellType.Numeric:
                    return cell.NumericCellValue.ToString();
                case CellType.Formula:
                    return cell.CellFormula.ToString();
                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();
                case CellType.String:
                    return cell.StringCellValue;
                case CellType.Blank:
                    return string.Empty;
                case CellType.Error:
                    return cell.ErrorCellValue.ToString();
                default:
                    return string.Empty;
            };
        }


        private void NewBtn_Click(object sender, RoutedEventArgs e)
        {
            fflag = 1;
            IDText.Text = string.Empty;
            NameText.Text = string.Empty;
            PwdText.Password = string.Empty;
            DepartCombo.SelectedIndex = -1;
            RoleCombo.SelectedIndex = -1;
            DetailGrid.Visibility = Visibility.Visible;
            ItemPanel.IsEnabled = false;
            HiroUtils.AddPowerAnimation(1, DetailGrid, null, 50).Begin();
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ItemData.SelectedIndex != -1)
                DeleteIndex(ItemData.SelectedIndex);
        }

        private void DeleteIndex(int index)
        {
            iusers.RemoveAt(page * 20 + index);
            Load_Part();
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            iusers.Clear();
            iusersp.Clear();
            page = 0;
            index = -1;
        }


        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {

            if (iusers.Count > 0)
                ShowMsg("正在导入", "正在将数据上传到服务器……", true);
            //分段导入
            if (iusers.Count > 20)
            {
                new System.Threading.Thread(() =>
                {
                    for (var i = 0; i * 20 < iusers.Count; i++)
                    {
                        var ja = new JsonArray();
                        for (var j = 0; j < 20; j++)
                        {
                            if (i * 20 + j >= iusers.Count)
                                break;
                            ja.Add(iusers[i * 20 + j].toJson());
                            var jo = HiroUtils.ParseJson(HiroUtils.SendRequest("/user", new List<string>() { "action", "username", "token", "device", "users" },
                            new List<string>() { "4", HiroUtils.userName, HiroUtils.userToken, "PC", ja.ToString().Replace(Environment.NewLine, " ") }));
                            if (jo != null)
                            {
                                continue;
                            }
                            else
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    HideMsg();
                                    HiroUtils.Notify($"导入数据时出错，出错的内容位于{i * 20} - {i + 1 * 20}项数据之间", "导入出错");
                                });
                                break;
                            }
                        }
                    }
                    Dispatcher.Invoke(() =>
                    {
                        HiroUtils.Notify("上传成功！");
                    });
                }).Start();

            }
            else
            {
                new System.Threading.Thread(() =>
                {
                    //生成json数据
                    var ja = new JsonArray();
                    foreach (var i in iusers)
                    {
                        ja.Add(i.toJson());
                    };
                    var jo = HiroUtils.ParseJson(HiroUtils.SendRequest("/user", new List<string>() { "action", "username", "token", "device", "users" },
                            new List<string>() { "4", HiroUtils.userName, HiroUtils.userToken, "PC", ja.ToString() }));
                    Dispatcher.Invoke(() =>
                    {
                        if (jo != null)
                        {
                            HideMsg();
                            HiroUtils.Notify("上传成功！");
                            iusers.Clear();
                            iusersp.Clear();
                        }
                        else
                        {
                            HideMsg();
                        }
                    });

                }).Start();
            }
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

        private void EnsureBtn_Click(object sender, RoutedEventArgs e)
        {
            var i = RoleCombo.SelectedIndex;
            i = i >= 0 && i <= HiroUtils.roles.Count ? i : i < 0 ? 0 : HiroUtils.roles.Count;
            var target = new UserClass
            {
                Name = IDText.Text,
                Password = PwdText.Password,
                Nickname = NameText.Text,
                Privilege = i,
                Role = HiroUtils.roles[i],
                Depart = HiroUtils.depart_all.Where(x => x.ID == DepartCombo.SelectedIndex).FirstOrDefault()

            };
            if (fflag == 0)
                iusers[page * 20 + index] = target;
            else
                iusers.Add(target);
            HideDetail();
            Load_Part();
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
            if (page <= HiroUtils.GetPage(iusers.Count))
            {
                page++;
                Load_Page(page);
            }
        }
    }
}
