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
    /// Dialog.xaml の相互作用ロジック
    /// </summary>
    public partial class Dialog : Window
    {
        public Dialog()
        {
            InitializeComponent();
        }

        public Dialog(string str)
        {
            InitializeComponent();
            Info.Text = str;
        }

        public Dialog(string txt, string str)
        {
            InitializeComponent();
            Title = txt;
            Info.Text = str;
        }
    }
}
