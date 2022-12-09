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
using System.Windows.Shapes;

namespace Warehouser_NET
{
    /// <summary>
    /// Explorer.xaml の相互作用ロジック
    /// </summary>
    public partial class Explorer : Window
    {
        public Explorer(Page p)
        {
            InitializeComponent();
            Frame.Navigate(p);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            switch (Frame.Content)
            {
                case Page_About pab:
                    pab.isolated = false;
                    break;
                case Page_Code pco:
                    pco.isolated = false;
                    break;
                case Page_Depart pde:
                    pde.isolated = false;
                    break;
                case Page_Items pit:
                    pit.isolated = false;
                    break;
                case Page_Main pma:
                    pma.isolated = false;
                    break;
                case Page_Settings pse:
                    pse.isolated = false;
                    break;
                case Page_Shelf psh:
                    psh.isolated = false;
                    break;
                case Page_Usage pus:
                    pus.isolated = true;
                    break;
                case Page_Member pme:
                    pme.isolated = true;
                    break;
                default:
                    break;

            }
        }
    }
}
