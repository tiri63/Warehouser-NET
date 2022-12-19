using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Warehouser_NET
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static FunWindow? funWindow = null;
        private void Hiro_We_Go(object sender, StartupEventArgs e)
        {
            new MainWindow().Show();
        }
    }
}
