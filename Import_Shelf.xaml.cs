using Microsoft.Win32;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;

namespace Warehouser_NET
{
    /// <summary>
    /// Import_Shelf.xaml の相互作用ロジック
    /// </summary>
    public partial class Import_Shelf : Page
    {
        internal System.Collections.ObjectModel.ObservableCollection<ShelfClass> ishelves = new System.Collections.ObjectModel.ObservableCollection<ShelfClass>();
        internal System.Collections.ObjectModel.ObservableCollection<ShelfClass> ishelvesp = new System.Collections.ObjectModel.ObservableCollection<ShelfClass>();
        internal bool isolated = false;
        private FunWindow? parent = null;
        internal int page = 0;
        internal int index = 0;
        internal int fflag = 0;
        public Import_Shelf()
        {
            InitializeComponent();
            ItemData.ItemsSource = ishelvesp;
            new System.Threading.Thread(() =>
            {
                HiroUtils.getDeparts();
            }).Start();
        }

        internal void ExportTemplate(string path)
        {
            var excel = new XSSFWorkbook();
            var sheet = excel.CreateSheet("货架导入模板");
            excel.CreateSheet("附加信息").CreateRow(0).CreateCell(0).SetCellValue("1.0");
            var row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("部门ID");
            row.CreateCell(1).SetCellValue("货架ID");
            row.CreateCell(2).SetCellValue("层ID");
            row.CreateCell(3).SetCellValue("名称");
            row.CreateCell(4).SetCellValue("描述（可空）");
            row = sheet.CreateRow(1);
            row.CreateCell(0).SetCellValue("1");
            row.CreateCell(1).SetCellValue("1");
            row.CreateCell(2).SetCellValue("1");
            row.CreateCell(3).SetCellValue("北侧货架");
            row.CreateCell(4).SetCellValue("用于存放临时货物");
            row.CreateCell(5).SetCellValue("此行为示例数据，导入时会直接忽略。请不要在此行填写真实数据！");
            FileStream stream = File.OpenWrite(@path); ;
            excel.Write(stream);
            stream.Close();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HiroUtils.AddPowerAnimation(0, BaseGrid, null, 50).Begin();
        }

        internal bool ImportFromFile(string path)
        {
            Dispatcher.Invoke(() =>
            {
                ishelves.Clear();
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
                            var d = 0;
                            int.TryParse(GetCellValue(row.GetCell(0)), out d);
                            var m = 0;
                            int.TryParse(GetCellValue(row.GetCell(1)), out m);
                            var s = 0;
                            int.TryParse(GetCellValue(row.GetCell(2)), out s);
                            ishelves.Add(new ShelfClass()
                            {
                                Depart = HiroUtils.depart_all.Where(x => x.ID == d).FirstOrDefault(),
                                FID = $"{d}-{m}-{s}",
                                MID = m,
                                SID = s,
                                Alias = GetCellValue(row.GetCell(3)),
                                Info = GetCellValue(row.GetCell(4))
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
                        var d = -1;
                        int.TryParse(isp[0], out d);
                        var m = -1;
                        int.TryParse(isp[1], out m);
                        var s = -1;
                        int.TryParse(isp[2], out s);
                        ishelves.Add(new ShelfClass()
                        {
                            Depart = HiroUtils.depart_all.Where(x => x.ID == d).FirstOrDefault(),
                            FID = $"{d}-{m}-{s}",
                            MID = m,
                            SID = s,
                            Alias = isp[3],
                            Info = isp[4]
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

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {

            if (ishelves.Count > 0)
                ShowMsg("正在导入", "正在将数据上传到服务器……", true);
            //分段导入
            if (ishelves.Count > 20)
            {
                new System.Threading.Thread(() =>
                {
                    for (var i = 0; i * 20 < ishelves.Count; i++)
                    {
                        var ja = new JsonArray();
                        for (var j = 0; j < 20; j++)
                        {
                            if (i * 20 + j >= ishelves.Count)
                                break;
                            ja.Add(ishelves[i * 20 + j].toJson());
                            var jo = HiroUtils.ParseJson(HiroUtils.SendRequest("/shelf", new List<string>() { "action", "username", "token", "device", "shelves", "shelf" },
                            new List<string>() { "4", HiroUtils.userName, HiroUtils.userToken, "PC", ja.ToString().Replace(Environment.NewLine, " "), ja[0].ToString() }));
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
                    foreach (var i in ishelves)
                    {
                        ja.Add(i.toJson());
                    };
                    var jo = HiroUtils.ParseJson(HiroUtils.SendRequest("/shelf", new List<string>() { "action", "username", "token", "device", "shelves", "shelf" },
                            new List<string>() { "4", HiroUtils.userName, HiroUtils.userToken, "PC", ja.ToString(), ja[0].ToString() }));
                    Dispatcher.Invoke(() =>
                    {
                        if (jo != null)
                        {
                            HideMsg();
                            HiroUtils.Notify("上传成功！");
                            ishelves.Clear();
                            ishelvesp.Clear();
                        }
                        else
                        {
                            HideMsg();
                        }
                    });

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
            var target = ishelves[index];
            UIDText.Text = target.FID;
            DepartText.Text = target.Depart.Name;
            MainText.Text = target.MID.ToString();
            SubText.Text = target.SID.ToString();
            NameText.Text = target.Alias;
            DescribeText.Text = target.Info;
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
        private void OKBtn1_Click(object sender, RoutedEventArgs e)
        {
            var m = -1;
            var s = -1;
            int.TryParse(MainText.Text, out m);
            int.TryParse(SubText.Text, out s);
            var target = new ShelfClass()
            {
                FID = UIDText.Text,
                Depart = HiroUtils.depart_all.Where(x => x.Name.Equals(DepartText.Text)).FirstOrDefault(),
                MID = m,
                SID = s,
                Alias = NameText.Text,
                Info = DescribeText.Text
            };
            if (fflag == 0)
                ishelves[page * 20 + index] = target;
            else
                ishelves.Add(target);
            HideDetail();
            Load_Part();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            HideDetail();
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

        private void NewBtn_Click(object sender, RoutedEventArgs e)
        {
            fflag = 1;
            DepartText.Text = string.Empty;
            MainText.Text = string.Empty;
            SubText.Text = string.Empty;
            NameText.Text = string.Empty;
            DescribeText.Text = string.Empty;
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
            ishelves.RemoveAt(page * 20 + index);
            Load_Part();
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            ishelves.Clear();
            ishelvesp.Clear();
            page = 0;
            index = -1;
        }

        private void Load_Page(int p)
        {
            if (ishelves.Count == 0)
            {
                ishelvesp.Clear();
                StatusLabel.Content = string.Format("第 {0}/{1} 页 共计{2}项", 1, 1, ishelves.Count);
            }
            if (p * 20 <= HiroUtils.GetPage(ishelves.Count))
            {
                page = p;
                ishelvesp.Clear();
                for (var i = 0; i < 20; i++)
                {
                    if (p * 20 + i >= ishelves.Count)
                        break;
                    var ui = ishelves[p * 20 + i];
                    ishelvesp.Add(ui);
                }
                StatusLabel.Content = string.Format("第 {0}/{1} 页 共计{2}项", p + 1, HiroUtils.GetPage(ishelves.Count), ishelves.Count);
            }
        }

        private void Load_Part()
        {
            Load_Page(page);
        }
    }
}
