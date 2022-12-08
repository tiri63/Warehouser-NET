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
    /// Page_Depart.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_Depart : Page
    {
        internal List<DepartClass> depart_all = new List<DepartClass>();
        internal List<DepartClass> depart_search = new List<DepartClass>();
        internal int flag = 0;
        public Page_Depart()
        {
            InitializeComponent();
            ItemData.ItemsSource = depart_all;
            depart_all.Add(new DepartClass()
            {
                ID = 1,
                Name = "熔炼二线"
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
                    HiroUtils.Notify("部门信息 - 库存管理", depart_all[ItemData.SelectedIndex].ToIDString());
                else
                    HiroUtils.Notify("部门信息 - 库存管理", depart_search[ItemData.SelectedIndex].ToIDString());
            }
        }
    }
}
