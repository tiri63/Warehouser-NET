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
        internal FunWindow? fw = null;
        public Explorer(Page p, FunWindow fw)
        {
            InitializeComponent();
            Frame.Navigate(p);
            this.fw = fw;
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
                case Page_Log pse:
                    pse.isolated = false;
                    break;
                case Page_Shelf psh:
                    psh.isolated = false;
                    break;
                case Page_Usage pus:
                    pus.isolated = false;
                    break;
                case Page_Member pme:
                    pme.isolated = false;
                    break;
                case Import_Code ic:
                    ic.isolated = false;
                    break;
                case Import_Depart id:
                    id.isolated = false;
                    break;
                case Import_Items ii:
                    ii.isolated = false;
                    break;
                case Import_Shelf ish:
                    ish.isolated = false;
                    break;
                case Import_Usage ius:
                    ius.isolated = false;
                    break;
                case Import_User iur:
                    iur.isolated = false;
                    break;
                default:
                    break;
            }
            if (fw != null)
                fw.MainExplorer.Navigate(Frame.Content);
        }
    }
}
