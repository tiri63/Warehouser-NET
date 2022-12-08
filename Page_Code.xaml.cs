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

namespace Warehouser_NET
{
    /// <summary>
    /// Page_Code.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_Code : Page
    {
        internal List<UIDClass> code_all = new List<UIDClass>();
        internal List<UIDClass> code_search = new List<UIDClass>();
        internal int flag = 0;
        public Page_Code()
        {
            InitializeComponent();
            ItemData.ItemsSource = code_all;
            code_all.Add(new UIDClass()
            {
                Name = "测试物品",
                UID = "1001",
                Model = "X-1",
                Unit = "m",
                Price = "2.33"
            });
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HiroUtils.AddPowerAnimation(0, BaseGrid, null, 50).Begin();
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox.Text.Length != 0)
                SearchTextLabel.Visibility = Visibility.Hidden;
            else
                SearchTextLabel.Visibility = Visibility.Visible;
        }

        private void ItemData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ItemData.SelectedIndex != -1)
            {
                if (flag == 0)
                    HiroUtils.Notify("物品种类信息 - 库存管理", code_all[ItemData.SelectedIndex].ToString());
                else
                    HiroUtils.Notify("物品种类信息 - 库存管理", code_search[ItemData.SelectedIndex].ToString());
            }
        }
    }
}
