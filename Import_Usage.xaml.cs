using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
    /// Import_Usage.xaml の相互作用ロジック
    /// </summary>
    public partial class Import_Usage : Page
    {
        public Import_Usage()
        {
            InitializeComponent();
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
    }
}
