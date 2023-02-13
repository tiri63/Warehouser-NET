using System;
using System.Collections.Generic;
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
using static ICSharpCode.SharpZipLib.Zip.ExtendedUnixData;

namespace Warehouser_NET
{
    /// <summary>
    /// Page_About.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_About : Page
    {
        internal bool isolated = false;
        private FunWindow? parent = null;
        public Page_About(FunWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HiroUtils.AddPowerAnimation(0, BaseGrid, null, 50).Begin();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ResetBtn.IsEnabled = false;
            if (MessageBox.Show("重置数据会导致所有数据丢失！" + Environment.NewLine + "重置数据后需要手动建立新账户，是否继续？", "警告！！！ - 库存管理", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                new System.Threading.Thread(() =>
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
