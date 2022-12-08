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
    /// Page_Shelf.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_Shelf : Page
    {
        internal List<ShelfClass> shelf_all = new List<ShelfClass>();
        internal List<ShelfClass> shelf_search = new List<ShelfClass>();
        internal int flag = 0;
        public Page_Shelf()
        {
            InitializeComponent();
            ItemData.ItemsSource = shelf_all;
            shelf_all.Add(new ShelfClass()
            {
                FID = "1-1",
                Alias = "一号货架",
                Info = "无附加信息",
                Depart = new DepartClass()
                {
                    ID = 1,
                    Name = "熔炼二线"
                }
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
                    HiroUtils.Notify("货架信息 - 库存管理", shelf_all[ItemData.SelectedIndex].ToString());
                else
                    HiroUtils.Notify("货架信息 - 库存管理", shelf_search[ItemData.SelectedIndex].ToString());
            }
        }
    }
}
