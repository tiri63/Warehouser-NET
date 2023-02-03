using Microsoft.Win32;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;

namespace Warehouser_NET
{
    /// <summary>
    /// Import_Depart.xaml の相互作用ロジック
    /// </summary>
    public partial class Import_Depart : Page
    {
        internal bool isolated = false;
        private FunWindow? parent = null;
        internal int index = 0;
        internal int fflag = 0;
        internal System.Collections.ObjectModel.ObservableCollection<DepartClass> idts = new System.Collections.ObjectModel.ObservableCollection<DepartClass>();
        public Import_Depart()
        {
            InitializeComponent();
            ItemData.ItemsSource = idts;
            new System.Threading.Thread(() =>
            {
                HiroUtils.getDeparts();
            }).Start();
        }
        internal void ExportTemplate(string path)
        {
            var excel = new XSSFWorkbook();
            var sheet = excel.CreateSheet("部门导入模板");
            excel.CreateSheet("附加信息").CreateRow(0).CreateCell(0).SetCellValue("1.0");
            var row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("ID");
            row.CreateCell(1).SetCellValue("名称");
            row = sheet.CreateRow(1);
            row.CreateCell(0).SetCellValue("0");
            row.CreateCell(1).SetCellValue("熔炼一线");
            row.CreateCell(2).SetCellValue("此行为示例数据，导入时会直接忽略。请不要在此行填写真实数据！");
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
            var target = idts[index];
            IDText.Text = target.ID.ToString();
            NameText.Text = target.Name;
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
                idts.Clear();
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
                        if (row.LastCellNum < 2)
                        {
                            break;
                        }
                        Dispatcher.Invoke(() =>
                        {
                            var i = -1;
                            int.TryParse(GetCellValue(row.GetCell(0)).ToString(), out i);
                            idts.Add(new DepartClass
                            {
                                ID = i,
                                Name = GetCellValue(row.GetCell(1)).ToString()
                            });
                        });

                    }
                    Dispatcher.Invoke(() =>
                    {
                        StatusLabel.Content = string.Format("共计{0}项", idts.Count);
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
                if (isp.Length >= 2)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var i = -1;
                        int.TryParse(isp[0], out i);
                        idts.Add(new DepartClass
                        {
                            ID = i,
                            Name = isp[1]
                        });
                    });
                }
            }
            Dispatcher.Invoke(() =>
            {
                ImportClipboard.IsEnabled = true;
                StatusLabel.Content = string.Format("共计{0}项", idts.Count);
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
            idts.RemoveAt(index);
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            idts.Clear();
            index = -1;
        }


        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {

            if (idts.Count > 0)
                ShowMsg("正在导入", "正在将数据上传到服务器……", true);
            //分段导入
            if (idts.Count > 20)
            {
                new System.Threading.Thread(() =>
                {
                    for (var i = 0; i * 20 < idts.Count; i++)
                    {
                        var ja = new JsonArray();
                        for (var j = 0; j < 20; j++)
                        {
                            if (i * 20 + j >= idts.Count)
                                break;
                            ja.Add(idts[i * 20 + j].toJson());
                            var jo = HiroUtils.ParseJson(HiroUtils.SendRequest("/depart", new List<string>() { "action", "username", "token", "device", "departs" },
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
                    foreach (var i in idts)
                    {
                        ja.Add(i.toJson());
                    };
                    var jo = HiroUtils.ParseJson(HiroUtils.SendRequest("/depart", new List<string>() { "action", "username", "token", "device", "departs" },
                            new List<string>() { "3", HiroUtils.userName, HiroUtils.userToken, "PC", ja.ToString() }));
                    Dispatcher.Invoke(() =>
                    {
                        if (jo != null)
                        {
                            HideMsg();
                            HiroUtils.Notify("上传成功！");
                            idts.Clear();
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
            var target = new DepartClass
            {
                Name = NameText.Text,
                ID = i
            };
            if (fflag == 0)
                idts[ index] = target;
            else
                idts.Add(target);
            HideDetail();
            StatusLabel.Content = string.Format("共计{0}项", idts.Count);
        }
    }
}
