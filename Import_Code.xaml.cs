using Microsoft.Win32;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
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

namespace Warehouser_NET
{
    /// <summary>
    /// Import_Code.xaml の相互作用ロジック
    /// </summary>
    public partial class Import_Code : Page
    {
        internal class Code4Import
        {
            public string UID;
            public string Name;
            public string Model;
            public string Unit;
            public double Price;
        }
        public Import_Code()
        {
            InitializeComponent();
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
                ItemData.Items.Clear();
            });
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
            if (verrow.GetCell(0).StringCellValue.Equals("1.0"))
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
                    double price;
                    if (!double.TryParse(row.GetCell(4).StringCellValue, out price))
                        break;
                    Dispatcher.Invoke(() =>
                    {
                        ItemData.Items.Add(new Code4Import()
                        {
                            UID = row.GetCell(0).StringCellValue,
                            Name = row.GetCell(1).StringCellValue,
                            Model = row.GetCell(2).StringCellValue,
                            Unit = row.GetCell(3).StringCellValue,
                            Price = price
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
    }
}
