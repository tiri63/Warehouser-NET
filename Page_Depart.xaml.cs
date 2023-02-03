using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace Warehouser_NET
{
    /// <summary>
    /// Page_Depart.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_Depart : Page
    {
        //internal System.Collections.ObjectModel.ObservableCollection<DepartClass> depart_search = new System.Collections.ObjectModel.ObservableCollection<DepartClass>();
        //internal int flag = 0;
        internal bool isolated = false;
        private FunWindow? parent = null;
        internal int index = 0;
        public Page_Depart(FunWindow parent)
        {
            InitializeComponent();
            ItemData.ItemsSource = HiroUtils.depart_all;
            new Thread(() =>
            {
                HiroUtils.getDeparts();
                Dispatcher.Invoke(() =>
                {
                    StatusLabel.Content = string.Format("共计{0}项", HiroUtils.depart_all.Count);
                });
            }).Start();
            this.parent = parent;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HiroUtils.AddPowerAnimation(0, BaseGrid, null, 50).Begin();
        }
        private void ItemData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ItemData.SelectedIndex != -1)
            {
                index = ItemData.SelectedIndex;
                ShowDetail(ItemData.SelectedIndex);
            }
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            Title_Label.Content = "与服务器同步数据……";
            HiroUtils.AddPowerAnimation(0, Title_Label, null, 50).Begin();
            DetailPrgBar.Visibility = Visibility.Visible;
            new Thread(() =>
            {
                var dc = new DepartClass();
                Dispatcher.Invoke(() =>
                {
                    dc = new DepartClass()
                    {
                        ID = Convert.ToInt32(IDText.Content.ToString()),
                        Name = NameText.Text
                    };
                });
                var jn = HiroUtils.ParseJson(HiroUtils.SendRequest("/depart", new List<string>() { "action", "depart", "username", "token", "device" },
                        new List<string>() { "1", dc.toJson().ToString(), HiroUtils.userName, HiroUtils.userToken, "PC" }));
                HiroUtils.LogtoFile(dc.toJson().ToString());
                if (jn != null)
                    Dispatcher.Invoke(() =>
                    {
                        HiroUtils.depart_all[index].Name = dc.Name;
                        HideDetail();
                    });
            }).Start();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            HideDetail();
        }
        private void ShowDetail(int index)
        {
            Title_Label.Content = "详细信息";
            DetailPrgBar.Visibility = Visibility.Collapsed;
            var target = HiroUtils.depart_all[index];
            IDText.Content = target.ID.ToString();
            NameText.Text = target.Name;
            DetailGrid.Visibility = Visibility.Visible;
            ItemPanel.IsEnabled = false;
            HiroUtils.AddPowerAnimation(1, DetailGrid, null, 50).Begin();
        }

        private void HideDetail()
        {
            var sb = HiroUtils.AddDoubleAnimaton(0, 150, DetailGrid, "Opacity", null);
            sb.Completed += delegate
            {
                DetailGrid.Visibility = Visibility.Collapsed;
                ItemPanel.IsEnabled = true;
            };
            sb.Begin();
        }

        private void RefreashBtn_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                HiroUtils.getDeparts();
                Dispatcher.Invoke(() =>
                {
                    StatusLabel.Content = string.Format("共计{0}项",HiroUtils.depart_all.Count);
                });
            }).Start();
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (parent != null)
            {
                parent.idt ??= new Import_Depart();
                if (!isolated)
                    parent.MainExplorer.Navigate(parent.idt);
                else if (parent.idt.isolated == false)
                {
                    if (parent.MainExplorer.Content == parent.idt)
                    {
                        parent.ppp ??= new Page_Popped();
                        parent.MainExplorer.Navigate(parent.ppp);
                    }
                    new Explorer(parent.idt, parent).Show();
                    parent.idt.isolated = true;
                }

            }
        }
    }
}
