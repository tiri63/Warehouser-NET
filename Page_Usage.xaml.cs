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
        internal System.Collections.ObjectModel.ObservableCollection<UsageClass> usage_all = new System.Collections.ObjectModel.ObservableCollection<UsageClass>();
        //internal List<UsageClass> usage_search = new List<UsageClass>();
        internal bool isolated = false;
        private FunWindow? parent = null;
        public Page_Usage(FunWindow parent)
        {
            InitializeComponent();
            ItemData.ItemsSource = HiroUtils.usages;
            StatusLabel.Content = string.Format("共计{0}项", HiroUtils.usages.Count);
            this.parent = parent;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HiroUtils.AddPowerAnimation(0, BaseGrid, null, 50).Begin();
        }

        private void ItemData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ItemData.SelectedIndex != -1)
                HiroUtils.Notify("用途信息 - 库存管理", HiroUtils.usages[ItemData.SelectedIndex].ToString());
        }
    }
}
