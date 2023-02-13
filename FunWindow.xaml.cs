using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
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
        internal Page_About? pab = null;
        internal Page_Popped? ppp = null;
        internal Import_Code? ico = null;
        internal Import_User? iur = null;
        internal Import_Depart? idt = null;
        internal Import_Usage? iue = null;
        internal Import_Shelf? isf = null;
        internal Import_Items? iis = null;
        internal Page_Log? plg = null;
        internal int Notification_CD = 0;

        public FunWindow()
        {
            InitializeComponent();
            try
            {
                File.Delete(HiroUtils.Path_Prepare(HiroUtils.LogFilePath) + "log.log");
            }
            catch { }
            UserName.Content = HiroUtils.userNickname + " (" + HiroUtils.userDepart + ")";
            pma = new Page_Main(this);
            MainExplorer.Navigate(pma);
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            HiroUtils.Write_Ini(HiroUtils.ConfigFilePath, "User", "UserName", "");
            HiroUtils.Write_Ini(HiroUtils.ConfigFilePath, "User", "UserToken", "");
            HiroUtils.Write_Ini(HiroUtils.ConfigFilePath, "User", "UserDepart", "");
            HiroUtils.Write_Ini(HiroUtils.ConfigFilePath, "User", "NickName", "");
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
            pab ??= new Page_About(this);
            if (!pab.isolated)
                MainExplorer.Navigate(pab);
            else
                SwitchTo(pab);
        }

        private void Label_Home_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pma ??= new Page_Main(this);
            if (!pma.isolated)
                MainExplorer.Navigate(pma);
            else
                SwitchTo(pma);
        }

        private void Label_Items_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pit ??= new Page_Items(this);
            if (!pit.isolated)
                MainExplorer.Navigate(pit);
            else
                SwitchTo(pit);
        }

        private void Label_Shelf_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            psh ??= new Page_Shelf(this);
            if (!psh.isolated)
                MainExplorer.Navigate(psh);
            else
                SwitchTo(psh);
        }

        private void Label_Member_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pme ??= new Page_Member(this);
            if (!pme.isolated)
                MainExplorer.Navigate(pme);
            else
                SwitchTo(pme);
        }

        private void Label_Code_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pco ??= new Page_Code(this);
            if (!pco.isolated)
                MainExplorer.Navigate(pco);
            else
                SwitchTo(pco);
        }

        private void Label_Depart_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pde ??= new Page_Depart(this);
            if (!pde.isolated)
                MainExplorer.Navigate(pde);
            else
                SwitchTo(pde);
        }

        private void Label_Usage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pus ??= new Page_Usage(this);
            if (!pus.isolated)
                MainExplorer.Navigate(pus);
            else
                SwitchTo(pus);
        }

        private void Label_Settings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            plg ??= new Page_Log(this);
            if (!plg.isolated)
                MainExplorer.Navigate(plg);
            else
                SwitchTo(plg);
        }

        private void Label_Items_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            pit ??= new Page_Items(this);
            pit.isolated = true;
            new Explorer(pit, this).Show();
            ppp ??= new Page_Popped();
            MainExplorer.Navigate(ppp);
        }

        private void Label_Code_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            pco ??= new Page_Code(this);
            pco.isolated = true;
            new Explorer(pco, this).Show();
            ppp ??= new Page_Popped();
            MainExplorer.Navigate(ppp);
        }

        private void Label_Shelf_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            psh ??= new Page_Shelf(this);
            psh.isolated = true;
            new Explorer(psh, this).Show();
            ppp ??= new Page_Popped();
            MainExplorer.Navigate(ppp);
        }

        private void Label_Home_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            pma ??= new Page_Main(this);
            pma.isolated = true;
            new Explorer(pma, this).Show();
            ppp ??= new Page_Popped();
            MainExplorer.Navigate(ppp);
        }

        private void Label_Member_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            pme ??= new Page_Member(this);
            pme.isolated = true;
            new Explorer(pme, this).Show();
            ppp ??= new Page_Popped();
            MainExplorer.Navigate(ppp);
        }

        private void Label_Depart_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            pde ??= new Page_Depart(this);
            pde.isolated = true;
            new Explorer(pde, this).Show();
            ppp ??= new Page_Popped();
            MainExplorer.Navigate(ppp);
        }

        private void Label_Usage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            pus ??= new Page_Usage(this);
            pus.isolated = true;
            new Explorer(pus, this).Show();
            ppp ??= new Page_Popped();
            MainExplorer.Navigate(ppp);
        }

        private void Label_Settings_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            plg ??= new Page_Log(this);
            plg.isolated = true;
            new Explorer(plg, this).Show();
            ppp ??= new Page_Popped();
            MainExplorer.Navigate(ppp);
        }

        private void Label_About_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            pab ??= new Page_About(this);
            pab.isolated = true;
            new Explorer(pab, this).Show();
            ppp ??= new Page_Popped();
            MainExplorer.Navigate(ppp);
        }

        private void SwitchTo(Page p)
        {
            foreach (var win in Application.Current.Windows)
            {
                if (win is Explorer)
                {
                    var ew = win as Explorer;
                    if (ew.Content == p)
                    {
                        ew.Focus();
                        break;
                    }
                }
            }
            ppp ??= new Page_Popped();
            MainExplorer.Navigate(ppp);
        }
    }
}
