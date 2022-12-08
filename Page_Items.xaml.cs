using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
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
    /// Page_Items.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_Items : Page
    {
        internal List<ItemClass> item_all = new List<ItemClass>();
        internal List<ItemClass> item_search = new List<ItemClass>();
        internal int flag = 0;
        public Page_Items()
        {
            InitializeComponent();
            ItemData.ItemsSource = item_all;
            new Thread(() =>
            {
                getItems();
            }).Start();
        }

        private bool getItems()
        {
            try
            {
                var jo = JsonObject.Parse(HiroUtils.SendRequest("/all", new List<string>() { "action" }, new List<string>() { "1" }));
                var ja = jo["msg"].AsArray();
                for (int i = 0; i < ja.Count; i++)
                {
                    Dispatcher.Invoke(() =>
                    {
                        item_all.Add(ItemClass.Parse(ja[i]));
                    });
                    
                };
                return true;
            }
            catch(Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    HiroUtils.LogError(ex, "Exception.Items.Get");
                });
                return false;
            }
            
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
                    HiroUtils.Notify("项目信息 - 库存管理", item_all[ItemData.SelectedIndex].ToString());
                else
                    HiroUtils.Notify("项目信息 - 库存管理", item_search[ItemData.SelectedIndex].ToString());
            }
        }
    }
}
