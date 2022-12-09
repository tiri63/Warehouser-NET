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
    /// Import_User.xaml の相互作用ロジック
    /// </summary>
    public partial class Import_User : Page
    {
        public Import_User()
        {
            InitializeComponent();
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HiroUtils.AddPowerAnimation(0, BaseGrid, null, 50).Begin();
        }
    }
}
