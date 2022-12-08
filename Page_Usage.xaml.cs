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
    /// Page_Usage.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_Usage : Page
    {
        internal List<UsageClass> usage_all = new List<UsageClass>();
        //internal List<UsageClass> usage_search = new List<UsageClass>();
        public Page_Usage()
        {
            InitializeComponent();
            ItemData.ItemsSource = HiroUtils.usages;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HiroUtils.AddPowerAnimation(0, BaseGrid, null, 50).Begin();
        }

        private void ItemData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ItemData.SelectedIndex != -1)
                HiroUtils.Notify("用途信息 - 库存管理", usage_all[ItemData.SelectedIndex].ToString());
        }
    }
}
