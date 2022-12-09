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
    /// FunWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class FunWindow : Window
    {
        internal Page_Main? pma = null;
        internal Page_Items? pit = null;
        internal Page_Shelf? psh = null;
        internal Page_Member? pme = null;
        internal Page_Code? pco = null;
        internal Page_Depart? pde = null;
        internal Page_Usage? pus = null;
        internal Page_Settings? pse = null;
        internal Page_About? pab = null;
        internal Import_Code? ico = null;
        public FunWindow()
        {
            InitializeComponent();
            UserName.Content = HiroUtils.userNickname + " (" + HiroUtils.userDepart + ")";
            pma = new Page_Main();
            MainExplorer.Navigate(pma);
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            new Login().Show();
            Close();
        }

        private void FunWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Application.Current.Windows.Count <= 0)
            {
                HiroUtils.ExitApp();
            }
        }

        private void Label_About_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pab ??= new Page_About();
            MainExplorer.Navigate(pab);
        }

        private void Label_Home_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pma ??= new Page_Main();
            MainExplorer.Navigate(pma);
        }

        private void Label_Items_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pit ??= new Page_Items();
            MainExplorer.Navigate(pit);
        }

        private void Label_Shelf_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            psh ??= new Page_Shelf();
            MainExplorer.Navigate(psh);
        }

        private void Label_Member_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pme ??= new Page_Member();
            MainExplorer.Navigate(pme);
        }

        private void Label_Code_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pco ??= new Page_Code(this);
            MainExplorer.Navigate(pco);
        }

        private void Label_Depart_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pde ??= new Page_Depart();
            MainExplorer.Navigate(pde);
        }

        private void Label_Usage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pus ??= new Page_Usage();
            MainExplorer.Navigate(pus);
        }

        private void Label_Settings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pse ??= new Page_Settings();
            MainExplorer.Navigate(pse);
        }
    }
}
