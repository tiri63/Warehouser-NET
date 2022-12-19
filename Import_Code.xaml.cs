using Microsoft.Win32;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;

namespace Warehouser_NET
{
    /// <summary>
    /// Import_Code.xaml の相互作用ロジック
    /// </summary>
    public partial class Import_Code : Page
    {
        internal System.Collections.ObjectModel.ObservableCollection<UIDClass> iuids = new System.Collections.ObjectModel.ObservableCollection<UIDClass>();
        public Import_Code()
        {
            InitializeComponent();
            ItemData.ItemsSource = iuids;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HiroUtils.AddPowerAnimation(0, BaseGrid, null, 50).Begin();
        }

        internal void ExportTemplate(string path)
        {
            var excel = new XSSFWorkbook();
            var sheet = excel.CreateSheet("唯一编码导入模板");
            excel.CreateSheet("附加信息").CreateRow(0).CreateCell(0).SetCellValue("1.0");
            var row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("唯一编码");
            row.CreateCell(1).SetCellValue("物料名称");
            row.CreateCell(2).SetCellValue("型号");
            row.CreateCell(3).SetCellValue("计量单位");
            row.CreateCell(4).SetCellValue("单价");
            row = sheet.CreateRow(1);
            row.CreateCell(0).SetCellValue("10001");
            row.CreateCell(1).SetCellValue("螺丝");
            row.CreateCell(2).SetCellValue("15mm");
            row.CreateCell(3).SetCellValue("个");
            row.CreateCell(4).SetCellValue("1.5");
            row.CreateCell(5).SetCellValue("此行为示例数据，导入时会直接忽略。请不要在此行填写真实数据！");
            FileStream stream = File.OpenWrite(@path); ;
            excel.Write(stream);
            stream.Close();
        }

        internal bool ImportFromFile(string path)
        {
            Dispatcher.Invoke(() =>
            {
                iuids.Clear();
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
                            iuids.Add(new UIDClass()
                            {
                                UID = GetCellValue(row.GetCell(0)),
                                Name = GetCellValue(row.GetCell(1)),
                                Model = GetCellValue(row.GetCell(2)),
                                Unit = GetCellValue(row.GetCell(3)),
                                Price = GetCellValue(row.GetCell(4))
                            });
                        });

                    }
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
        private string GetCellValue(ICell cell)
        {
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
                Filter = "Xlsx 文件|*.xlsx",
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
                        iuids.Add(new UIDClass()
                        {
                            UID = isp[0],
                            Name = isp[1],
                            Model = isp[2],
                            Unit = isp[3],
                            Price = isp[4]
                        });
                    });
                }
            }
            Dispatcher.Invoke(() =>
            {
                ImportClipboard.IsEnabled = true;
            });
        }
    }
}
