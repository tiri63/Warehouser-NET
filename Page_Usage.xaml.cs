using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        internal int index = 0;
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
            {
                index = ItemData.SelectedIndex;
                ShowDetail(ItemData.SelectedIndex);
            }
        }

        private void RefreashBtn_Click(object sender, RoutedEventArgs e)
        {

            ItemPanel.IsEnabled = false;
            ShowMsg("服务器通信", "正在从服务器上获取最新数据……", true);
            new Thread(() =>
            {
                HiroUtils.getUsages();
                Dispatcher.Invoke(() =>
                {
                    StatusLabel.Content = string.Format("共计{0}项", HiroUtils.usages.Count);
                    HideMsg();
                });
            }).Start();
        }

        private void ShowMsg(string title, string? content, bool progress = false, bool button = false)
        {
            MsgProgressBar.Visibility = progress ? Visibility.Visible : Visibility.Collapsed;
            MsgOKButton.Visibility = button ? Visibility.Visible : Visibility.Collapsed;
            MsgGrid.Visibility = Visibility.Visible;
            MsgContent.Visibility = content == null ? Visibility.Collapsed : Visibility.Visible;
            ItemPanel.IsEnabled = false;
            MsgTitle.Content = title;
            if (content != null)
                MsgContent.Content = content;
            HiroUtils.AddPowerAnimation(1, MsgGrid, null, 50).Begin();
        }

        private void HideMsg()
        {
            var sb = HiroUtils.AddDoubleAnimaton(0, 150, MsgGrid, "Opacity", null);
            sb.Completed += delegate
            {
                MsgGrid.Visibility = Visibility.Collapsed;
                ItemPanel.IsEnabled = true;
            };
            sb.Begin();
        }


        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            Title_Label.Content = "与服务器同步数据……";
            HiroUtils.AddPowerAnimation(0, Title_Label, null, 50).Begin();
            DetailPrgBar.Visibility = Visibility.Visible;
            new Thread(() =>
            {
                var uc = new UsageClass();
                Dispatcher.Invoke(() =>
                {
                    var i = -1;
                    int.TryParse(IDText.Content.ToString(), out i);
                    uc.Code = i;
                    uc.Alias = NameText.Text;
                    uc.Info = DescribeText.Text;
                    uc.Hide = VisibilityBox.SelectedIndex == 1 ? true : false;
                    uc.HideStr = uc.Hide ? "隐藏" : "可见";
                });
                var jn = HiroUtils.ParseJson(HiroUtils.SendRequest("/usage", new List<string>() { "action", "function", "username", "token", "device" },
                        new List<string>() { "1", uc.toJson().ToString(), HiroUtils.userName, HiroUtils.userToken, "PC" }));
                if (jn != null)
                    Dispatcher.Invoke(() =>
                    {
                        HiroUtils.usages[index] = uc;
                        HideDetail();
                    });
            }).Start();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            HideDetail();
        }
        private void ShowDetail(int index)
        {
            Title_Label.Content = "详细信息";
            DetailPrgBar.Visibility = Visibility.Collapsed;
            var target = HiroUtils.usages[index];
            IDText.Content = target.Code.ToString();
            NameText.Text = target.Alias;
            DescribeText.Text = target.Info;
            VisibilityBox.SelectedIndex = target.Hide ? 1 : 0;
            DetailGrid.Visibility = Visibility.Visible;
            ItemPanel.IsEnabled = false;
            HiroUtils.AddPowerAnimation(1, DetailGrid, null, 50).Begin();
        }

        private void HideDetail()
        {
            var sb = HiroUtils.AddDoubleAnimaton(0, 150, DetailGrid, "Opacity", null);
            sb.Completed += delegate
            {
                DetailGrid.Visibility = Visibility.Collapsed;
                ItemPanel.IsEnabled = true;
            };
            sb.Begin();
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (parent != null)
            {
                parent.iue ??= new Import_Usage();
                if (!isolated)
                    parent.MainExplorer.Navigate(parent.iue);
                else if (parent.iue.isolated == false)
                {
                    if (parent.MainExplorer.Content == parent.iue)
                    {
                        parent.ppp ??= new Page_Popped();
                        parent.MainExplorer.Navigate(parent.ppp);
                    }
                    new Explorer(parent.iue, parent).Show();
                    parent.iue.isolated = true;
                }

            }
        }
    }
}
