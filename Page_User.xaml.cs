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
    /// Page_Member.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_Member : Page
    {
        internal List<UserClass> user_all = new List<UserClass>();
        internal List<UserClass> user_search = new List<UserClass>();
        internal int flag = 0;
        public Page_Member()
        {
            InitializeComponent();
            ItemData.ItemsSource = user_all;
            new Thread(() =>
            {
                getUsers();
            }).Start();
        }

        private bool getUsers()
        {
            try
            {
                var jo = JsonObject.Parse(HiroUtils.SendRequest("/user", new List<string>() { "action" }, new List<string>() { "6" }));
                var ja = jo["msg"].AsArray();
                Dispatcher.Invoke(() =>
                {
                    StatusLabel.Content = string.Format("共计{0}项", ja.Count);
                });
                for (int i = 0; i < ja.Count; i++)
                {
                    Dispatcher.Invoke(() =>
                    {
                        user_all.Add(UserClass.Parse(ja[i]));
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
                    HiroUtils.Notify("用户信息 - 库存管理", user_all[ItemData.SelectedIndex].ToString());
                else
                    HiroUtils.Notify("用户信息 - 库存管理", user_search[ItemData.SelectedIndex].ToString());
            }
        }
    }
}
