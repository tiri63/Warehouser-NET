using Microsoft.Win32;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Import_Usage.xaml の相互作用ロジック
    /// </summary>
    public partial class Import_Usage : Page
    {
        internal bool isolated = false;
        private FunWindow? parent = null;
        internal int index = 0;
        internal int fflag = 0;
        internal System.Collections.ObjectModel.ObservableCollection<UsageClass> iues = new System.Collections.ObjectModel.ObservableCollection<UsageClass>();
        public Import_Usage()
        {
            InitializeComponent();
            ItemData.ItemsSource = iues;
            new System.Threading.Thread(() =>
            {
                HiroUtils.getDeparts();
            }).Start();
        }

        internal void ExportTemplate(string path)
        {
            var excel = new XSSFWorkbook();
            var sheet = excel.CreateSheet("用途导入模板");
            excel.CreateSheet("附加信息").CreateRow(0).CreateCell(0).SetCellValue("1.0");
            var row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("编号");
            row.CreateCell(1).SetCellValue("名称");
            row.CreateCell(2).SetCellValue("描述（可空）");
            row.CreateCell(3).SetCellValue("可视性");
            row = sheet.CreateRow(1);
            row.CreateCell(0).SetCellValue("1");
            row.CreateCell(1).SetCellValue("暂存");
            row.CreateCell(2).SetCellValue("标志用于暂时存放的各项货物");
            row.CreateCell(3).SetCellValue("0");
            row.CreateCell(4).SetCellValue("此行为示例数据，导入时会直接忽略。请不要在此行填写真实数据！可视性：部分不再可用的用途可以在手机APP上隐藏，录入时不可选择这些用途。0为隐藏，其它任何值均为显示。");
            FileStream stream = File.OpenWrite(@path); ;
            excel.Write(stream);
            stream.Close();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HiroUtils.AddPowerAnimation(0, BaseGrid, null, 50).Begin();
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
            var target = iues[index];
            IDText.Text = target.Code.ToString();
            NameText.Text = target.Alias;
            DescribeText.Text = target.Info;
            VisibilityBox.SelectedIndex = target.Hide ? 1 : 0;
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
                iues.Clear();
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
                        if (row.LastCellNum < 4)
                        {
                            break;
                        }
                        Dispatcher.Invoke(() =>
                        {
                            var i = -1;
                            int.TryParse(GetCellValue(row.GetCell(0)), out i);
                            iues.Add(new UsageClass()
                            {
                                Code = i,
                                Alias = GetCellValue(row.GetCell(1)),
                                Info = GetCellValue(row.GetCell(2)),
                                Hide = GetCellValue(row.GetCell(2)).Equals("0"),
                                HideStr = GetCellValue(row.GetCell(2)).Equals("0") ? "隐藏" : "可见"
                            });
                        });

                    }
                    Dispatcher.Invoke(() =>
                    {
                        StatusLabel.Content = string.Format("共计{0}项", iues.Count);
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
                if (isp.Length >= 4)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var i = -1;
                        int.TryParse(isp[0], out i);
                        iues.Add(new UsageClass()
                        {
                            Code = i,
                            Alias = isp[1],
                            Info = isp[2],
                            Hide = isp[3].Equals("0"),
                            HideStr = isp[3].Equals("0") ? "隐藏" : "可见"
                        });
                    });
                }
            }
            Dispatcher.Invoke(() =>
            {
                StatusLabel.Content = string.Format("共计{0}项", iues.Count);
                ImportClipboard.IsEnabled = true;
            });
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
            DescribeText.Text = string.Empty;
            ItemPanel.IsEnabled = false;
            DetailGrid.Visibility = Visibility.Visible;
            HiroUtils.AddPowerAnimation(1, DetailGrid, null, 50).Begin();
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ItemData.SelectedIndex != -1)
                DeleteIndex(ItemData.SelectedIndex);
        }

        private void DeleteIndex(int index)
        {
            iues.RemoveAt(index);
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            iues.Clear();
            index = -1;
        }


        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {

            if (iues.Count > 0)
                ShowMsg("正在导入", "正在将数据上传到服务器……", true);
            //分段导入
            if (iues.Count > 20)
            {
                new System.Threading.Thread(() =>
                {
                    for (var i = 0; i * 20 < iues.Count; i++)
                    {
                        var ja = new JsonArray();
                        for (var j = 0; j < 20; j++)
                        {
                            if (i * 20 + j >= iues.Count)
                                break;
                            ja.Add(iues[i * 20 + j].toJson());
                            var jo = HiroUtils.ParseJson(HiroUtils.SendRequest("/usage", new List<string>() { "action", "username", "token", "device", "usages" },
                            new List<string>() { "3", HiroUtils.userName, HiroUtils.userToken, "PC", ja.ToString().Replace(Environment.NewLine, " ") }));
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
                    foreach (var i in iues)
                    {
                        ja.Add(i.toJson());
                    };
                    var jo = HiroUtils.ParseJson(HiroUtils.SendRequest("/usage", new List<string>() { "action", "username", "token", "device", "usages" },
                            new List<string>() { "3", HiroUtils.userName, HiroUtils.userToken, "PC", ja.ToString() }));
                    Dispatcher.Invoke(() =>
                    {
                        if (jo != null)
                        {
                            HideMsg();
                            HiroUtils.Notify("上传成功！");
                            iues.Clear();
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
            var i = -1;
            int.TryParse(IDText.Text, out i);
            var target = new UsageClass
            {
                Code = i,
                Alias = NameText.Text,
                Info = DescribeText.Text,
                Hide = VisibilityBox.SelectedIndex == 1,
                HideStr = VisibilityBox.SelectedIndex == 1 ? "隐藏" : "可见"
            };
            if (fflag == 0)
                iues[index] = target;
            else
                iues.Add(target);
            HideDetail();
            StatusLabel.Content = string.Format("共计{0}项", iues.Count);
        }
    }
}
