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
    /// Page_Shelf.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_Shelf : Page
    {
        internal System.Collections.ObjectModel.ObservableCollection<ShelfClass> shelf_all = new System.Collections.ObjectModel.ObservableCollection<ShelfClass>();
        internal System.Collections.ObjectModel.ObservableCollection<ShelfClass> shelf_search = new System.Collections.ObjectModel.ObservableCollection<ShelfClass>();
        internal int flag = 0;
        internal bool isolated = false;
        private FunWindow? parent = null;
        public Page_Shelf(FunWindow parent)
        {
            InitializeComponent();
            ItemData.ItemsSource = shelf_all;
            new Thread(() =>
            {
                getShelves();
            }).Start();
            this.parent = parent;
        }

        private bool getShelves()
        {
            try
            {
                var jo = HiroUtils.ParseJson(HiroUtils.SendRequest("/shelf", new List<string>() { "action" }, new List<string>() { "3" }));
                if (jo != null)
                {
                    var ja = jo["msg"].AsArray();
                    Dispatcher.Invoke(() =>
                    {
                        StatusLabel.Content = string.Format("共计{0}项", ja.Count);
                    });
                    for (int i = 0; i < ja.Count; i++)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            shelf_all.Add(ShelfClass.Parse(ja[i]));
                        });

                    };
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    HiroUtils.LogError(ex, "Exception.UID.Get");
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
                    HiroUtils.Notify("货架信息 - 库存管理", shelf_all[ItemData.SelectedIndex].ToString());
                else
                    HiroUtils.Notify("货架信息 - 库存管理", shelf_search[ItemData.SelectedIndex].ToString());
            }
        }
    }
}
