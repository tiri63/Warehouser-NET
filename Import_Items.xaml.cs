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
    /// Import_Items.xaml の相互作用ロジック
    /// </summary>
    public partial class Import_Items : Page
    {
        public Import_Items()
        {
            InitializeComponent();
        }
        internal void ExportTemplate(string path)
        {
            var excel = new XSSFWorkbook();
            var sheet = excel.CreateSheet("在库项目导入模板");
            excel.CreateSheet("附加信息").CreateRow(0).CreateCell(0).SetCellValue("1.0");
            var row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("货架ID");
            row.CreateCell(1).SetCellValue("物料编码");
            row.CreateCell(2).SetCellValue("数量");
            row.CreateCell(3).SetCellValue("用途");
            row = sheet.CreateRow(1);
            row.CreateCell(0).SetCellValue("1-1-1");
            row.CreateCell(1).SetCellValue("10001");
            row.CreateCell(2).SetCellValue("20");
            row.CreateCell(3).SetCellValue("1,2");
            row.CreateCell(4).SetCellValue("此行为示例数据，导入时会直接忽略。请不要在此行填写真实数据！用途的填写：请填写用途表中的对应编号，多个用途用英文逗号分割");
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
