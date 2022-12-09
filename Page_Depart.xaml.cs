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
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties;

namespace Warehouser_NET
{
    /// <summary>
    /// Page_Depart.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_Depart : Page
    {
        internal System.Collections.ObjectModel.ObservableCollection<DepartClass> depart_all = new System.Collections.ObjectModel.ObservableCollection<DepartClass>();
        internal System.Collections.ObjectModel.ObservableCollection<DepartClass> depart_search = new System.Collections.ObjectModel.ObservableCollection<DepartClass>();
        internal int flag = 0;
        internal bool isolated = false;
        private FunWindow? parent = null;
        public Page_Depart(FunWindow parent)
        {
            InitializeComponent();
            ItemData.ItemsSource = depart_all;
            new Thread(() =>
            {
                getDeparts();
            }).Start();
            this.parent = parent;
        }

        private bool getDeparts()
        {
            try
            {
                var jo = JsonObject.Parse(HiroUtils.SendRequest("/depart", new List<string>() { "action" }, new List<string>() { "2" }));
                var ja = jo["msg"].AsArray();
                Dispatcher.Invoke(() =>
                {
                    StatusLabel.Content = string.Format("共计{0}项", ja.Count);
                });
                for (int i = 0; i < ja.Count; i++)
                {
                    Dispatcher.Invoke(() =>
                    {
                        depart_all.Add(DepartClass.Parse(ja[i]));
                    });

                };
                return true;
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    HiroUtils.LogError(ex, "Exception.Depart.Get");
                });
                return false;
            }
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HiroUtils.AddPowerAnimation(0, BaseGrid, null, 50).Begin();
        }
        private void ItemData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ItemData.SelectedIndex != -1)
            {
                if (flag == 0)
                    HiroUtils.Notify("部门信息 - 库存管理", depart_all[ItemData.SelectedIndex].ToIDString());
                else
                    HiroUtils.Notify("部门信息 - 库存管理", depart_search[ItemData.SelectedIndex].ToIDString());
            }
        }
    }
}
