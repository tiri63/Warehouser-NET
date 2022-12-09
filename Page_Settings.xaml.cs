using System.Windows;
using System.Windows.Controls;

namespace Warehouser_NET
{
    /// <summary>
    /// Page_Settings.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_Settings : Page
    {
        internal bool isolated = false;
        private FunWindow? parent = null;
        public Page_Settings(FunWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HiroUtils.AddPowerAnimation(0, BaseGrid, null, 50).Begin();
        }
    }
}
