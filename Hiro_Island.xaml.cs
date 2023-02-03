using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Warehouser_NET
{
    /// <summary>
    /// Hiro_Island.xaml の相互作用ロジック
    /// </summary>
    public partial class Hiro_Island : Window
    {
        internal double former_left = 0;
        internal double former_width = 0;
        internal double former_height = 0;
        internal int CD = 2;
        internal DispatcherTimer timer;
        internal List<Hiro_Notice> notifications = new List<Hiro_Notice>();
        internal Action? act = null;
        public Hiro_Island(Hiro_Notice h)
        {
            InitializeComponent();
            ContentLabel.MaxWidth = SystemParameters.FullPrimaryScreenWidth * 4 / 5;
            Title = "新消息 - 库存管理";
            notifications.Add(h);
            ContentLabel.Text = notifications[0].msg;
            TitleLabel.Content = notifications[0].title;
            CD = notifications[0].time;
            act = notifications[0].act;
            if (act != null)
            {
                ContentGrid.Cursor = Cursors.Hand;
            }
            notifications.RemoveAt(0);
            Loaded += delegate
            {
                Island_In();
                timer = new DispatcherTimer()
                {
                    Interval = new TimeSpan(10000000)
                };
                timer.Tick += delegate
                {
                    TimerTick();
                };
            };
        }

        private void TimerTick()
        {
            CD--;
            if (CD <= 0)
            {
                NextNotification();
            }
        }

        private void NextNotification()
        {
            if (notifications.Count > 0)
            {
                ContentLabel.Text = notifications[0].msg;
                TitleLabel.Content = notifications[0].title;
                CD = notifications[0].time;
                act = notifications[0].act;
                if (act != null)
                {
                    ContentGrid.Cursor = Cursors.Hand;
                }
                notifications.RemoveAt(0);
                SetAutoSize(ContentGrid);
                Island_Switch();
            }
            else
            {
                Island_Out();
                timer.Stop();
                HiroUtils.his = null;
            }
        }

        private void Island_In()
        {
            SetAutoSize(BaseGrid);
            former_width = BaseGrid.ActualWidth;
            former_height = BaseGrid.ActualHeight;
            former_left = SystemParameters.FullPrimaryScreenWidth / 2 - former_width / 2;

            Storyboard sb = new Storyboard();
            sb = HiroUtils.AddDoubleAnimaton(50, 850, this, TopProperty.Name, sb, -former_height, 0.7);
            sb = HiroUtils.AddDoubleAnimaton(former_left, 850, this, LeftProperty.Name, sb, SystemParameters.FullPrimaryScreenWidth / 2, 0.7);
            sb = HiroUtils.AddDoubleAnimaton(former_width, 850, BaseGrid, "Width", sb, 1, 0.7);
            sb = HiroUtils.AddDoubleAnimaton(former_height, 850, BaseGrid, "Height", sb, 1, 0.7);
            sb.Completed += delegate
            {
                Canvas.SetTop(this, 50);
                Canvas.SetLeft(this, former_left);
                SetAutoSize(BaseGrid);
                timer.Start();
            };
            sb.Begin();

        }

        private void Island_Out()
        {
            var w = ContentGrid.ActualWidth;
            var h = ContentGrid.ActualHeight;
            Storyboard sb = new Storyboard();
            sb = HiroUtils.AddDoubleAnimaton(-h, 700, this, TopProperty.Name, sb, 50, 0.7);
            sb = HiroUtils.AddDoubleAnimaton(SystemParameters.FullPrimaryScreenWidth / 2, 700, this, LeftProperty.Name, sb, SystemParameters.FullPrimaryScreenWidth / 2 - former_width / 2, 0.7);
            sb = HiroUtils.AddDoubleAnimaton(1, 700, BaseGrid, "Width", sb, former_width, 0.7);
            sb = HiroUtils.AddDoubleAnimaton(1, 700, BaseGrid, "Height", sb, former_height, 0.7);
            sb.Completed += delegate
            {
                Visibility = Visibility.Hidden;
                new Thread(() =>
                {
                    //weird...
                    Thread.Sleep(100);
                    Dispatcher.Invoke(() =>
                    {
                        Close();
                    });
                }).Start();
            };
            sb.Begin();
        }

        private void SetAutoSize(FrameworkElement fe)
        {
            fe.Width = double.NaN;
            fe.Height = double.NaN;
            fe.InvalidateVisual();
            fe.UpdateLayout();
        }

        private void Island_Switch()
        {
            BaseGrid.Width = former_width;
            BaseGrid.Height = former_height;
            var w = ContentGrid.ActualWidth;
            var h = ContentGrid.ActualHeight;
            var offset = Math.Min(w * 0.1, 50);
            var sb = new Storyboard();
            sb = HiroUtils.AddDoubleAnimaton(former_left + offset, 300, this, LeftProperty.Name, sb, former_left);
            sb = HiroUtils.AddDoubleAnimaton(former_width - offset * 2, 180, BaseGrid, "Width", sb, former_width);
            sb.Completed += delegate
            {
                former_left = SystemParameters.FullPrimaryScreenWidth / 2 - w / 2;
                var sb = new Storyboard();
                sb = HiroUtils.AddDoubleAnimaton(former_left, 300, this, LeftProperty.Name, sb, null);
                sb = HiroUtils.AddDoubleAnimaton(w + ContentGrid.Margin.Left * 2, 300, BaseGrid, "Width", sb, null);
                sb = HiroUtils.AddDoubleAnimaton(h + ContentGrid.Margin.Top * 2, 300, BaseGrid, "Height", sb, null);
                sb.Completed += delegate
                {
                    SetAutoSize(BaseGrid);
                    former_width = BaseGrid.ActualWidth;
                    former_height = BaseGrid.ActualHeight;
                };
                sb.Begin();
            };
            sb.Begin();

        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            NextNotification();
        }

        private void Label_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            NextNotification();
        }

        private void TitleLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (act != null)
                act.Invoke();
            act = null;
            ContentGrid.Cursor = null;
        }

        private void Content_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (act != null)
                act.Invoke();
            act = null;
            ContentGrid.Cursor = null;
        }

        private void BaseIconBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (act != null)
                act.Invoke();
            act = null;
            ContentGrid.Cursor = null;
        }
    }
}
