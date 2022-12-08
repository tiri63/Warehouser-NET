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
            user_all.Add(new UserClass()
            {
                Name = "Hiro",
                Role = "超级管理员",
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
