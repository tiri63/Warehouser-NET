using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Warehouser_NET
{
    /// <summary>
    /// Page_Member.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_Member : Page
    {
        internal System.Collections.ObjectModel.ObservableCollection<UserClass> user_all = new System.Collections.ObjectModel.ObservableCollection<UserClass>();
        internal System.Collections.ObjectModel.ObservableCollection<UserClass> user_p = new System.Collections.ObjectModel.ObservableCollection<UserClass>();
        internal System.Collections.ObjectModel.ObservableCollection<UserClass> user_search = new System.Collections.ObjectModel.ObservableCollection<UserClass>();
        internal int flag = 0;
        internal int page = 0;
        internal int index = 0;
        internal int findex = 0;
        internal bool isolated = false;
        private FunWindow? parent = null;
        public Page_Member(FunWindow parent)
        {
            InitializeComponent();
            ItemData.ItemsSource = user_p;
            foreach (var i in HiroUtils.roles)
            {
                RoleCombo.Items.Add(i);
            };
            new Thread(() =>
            {
                HiroUtils.getDeparts();
                GetUsers();
            }).Start();
            this.parent = parent;
        }

        private bool GetUsers()
        {
            flag = 0;
            Dispatcher.Invoke(() =>
            {
                ItemPanel.IsEnabled = false;
                ShowMsg("服务器通信", "正在从服务器上获取最新数据……", true);
                user_all.Clear();
                SearchMethod.Items.Clear();
                SearchMethod.SelectedIndex = -1;
            });
            try
            {
                var jo = JsonObject.Parse(HiroUtils.SendRequest("/user", new List<string>() { "action" }, new List<string>() { "6" }));
                var ja = jo["msg"].AsArray();
                Dispatcher.Invoke(() =>
                {
                    StatusLabel.Content = string.Format("共计{0}项", ja.Count);
                });
                Dictionary<string, string> dic = new Dictionary<string, string>();
                for (int i = 0; i < ja.Count; i++)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var a = UserClass.Parse(ja[i]);
                        user_all.Add(a);
                        dic.TryAdd(a.Depart.Name, "1");
                    });

                };
                Dispatcher.Invoke(() =>
                {
                    SearchMethod.Items.Add("全部");
                    foreach (var k in dic.Keys)
                    {
                        SearchMethod.Items.Add(k);
                    }
                    Load_Page(1);
                    ItemPanel.IsEnabled = false;
                    HideMsg();
                });
                return true;
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    ItemPanel.IsEnabled = false;
                    HideMsg();
                    HiroUtils.LogError(ex, "Exception.Depart.Get");
                });
                return false;
            }
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

        private void ShowDetail(int index)
        {
            Title_Label.Content = "详细信息";
            DetailPrgBar.Visibility = Visibility.Collapsed;
            var target = flag == 0 ? user_all[page * 20 + index] : user_search[page * 20 + index];
            IDText.Content = target.Name;
            NameText.Text = target.Nickname;
            Dispatcher.Invoke(() =>
            {
                DepartCombo.Items.Clear();
            });
            foreach (var i in HiroUtils.depart_all)
            {
                DepartCombo.Items.Add(i.Name);
                if (target.Depart.ID == i.ID)
                    DepartCombo.SelectedIndex = DepartCombo.Items.Count - 1;
            }
            if (target.Privilege < 0)
                RoleCombo.SelectedIndex = 0;
            else if (target.Privilege > HiroUtils.roles.Count - 1)
                RoleCombo.SelectedIndex = HiroUtils.roles.Count - 1;
            else
                RoleCombo.SelectedIndex = target.Privilege;
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

        private void Load_Page(int p)
        {
            var ap = flag == 0 ? HiroUtils.GetPage(user_all.Count) : HiroUtils.GetPage(user_search.Count);
            var ac = flag == 0 ? user_all.Count : user_search.Count;
            if ((p <= ap || ap == 0) && p >= 1)
            {
                p--;
                user_p.Clear();
                page = p;
                for (var i = 0; i < 20; i++)
                {
                    if (p * 20 + i >= ac)
                        break;
                    var ui = flag == 0 ? user_all[p * 20 + i] : user_search[p * 20 + i];
                    user_p.Add(ui);
                }
                StatusLabel.Content = string.Format("第 {0}/{1} 页 共计{2}项", p + 1, ap, ac);
            }
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            Title_Label.Content = "与服务器同步数据……";
            HiroUtils.AddPowerAnimation(0, Title_Label, null, 50).Begin();
            DetailPrgBar.Visibility = Visibility.Visible;
            new Thread(() =>
            {
                var uc = new UserClass();
                Dispatcher.Invoke(() =>
                {
                    var de = HiroUtils.depart_all.Where(x => x.ID == DepartCombo.SelectedIndex).FirstOrDefault();
                    uc = new UserClass()
                    {
                        Name = IDText.Content.ToString(),
                        Nickname = NameText.Text,
                        Depart = de,
                        Privilege = RoleCombo.SelectedIndex,
                        Role = HiroUtils.roles[RoleCombo.SelectedIndex]
                    };
                });
                var jn = HiroUtils.ParseJson(HiroUtils.SendRequest("/user", new List<string>() { "action", "user", "username", "token", "device" },
                        new List<string>() { "2", uc.toJson().ToString(), HiroUtils.userName, HiroUtils.userToken, "PC" }));
                if (jn != null)
                    Dispatcher.Invoke(() =>
                    {
                        user_p[index] = uc;
                        if (flag == 0)
                            user_all[page * 20 + index] = uc;
                        else
                            user_search[page * 20 + index] = uc;
                        HideDetail();
                    });
            }).Start();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            HideDetail();
        }

        private void ResetPwdBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ItemData.SelectedIndex != -1)
            {
                index = ItemData.SelectedIndex;
                ResetGrid.Visibility = Visibility.Visible;
                ResetTitle.Content = string.Format("重置 {0} 的密码", user_p[index].Nickname);
                ItemPanel.IsEnabled = false;
                HiroUtils.AddPowerAnimation(1, ResetGrid, null, 50).Begin();
            }
        }

        private void RefreashBtn_Click(object sender, RoutedEventArgs e)
        {
            GetUsers();
        }

        private void ResetCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            HideReset();
        }

        private void ResetOKBtn_Click(object sender, RoutedEventArgs e)
        {
            if (NewPwd.Password.Equals(RepeatPwd.Password))
            {
                new Thread(() =>
                {
                    var value = new List<string>();
                    Dispatcher.Invoke(() =>
                    {
                        value = new List<string>() { "3", NewPwd.Password, user_p[index].Name, HiroUtils.userName, HiroUtils.userToken, "PC" };
                    });
                    var jn = HiroUtils.ParseJson(HiroUtils.SendRequest("/user", new List<string>() { "action", "newPwd", "targetname", "username", "token", "device" }, value));
                    if (jn != null)
                        Dispatcher.Invoke(() =>
                        {
                            HideReset();
                        });
                }).Start();
            }
            else
            {
                HiroUtils.Notify("两次输入的密码不匹配", "不匹配");
            }


        }

        private void HideReset()
        {
            var sb = HiroUtils.AddDoubleAnimaton(0, 150, ResetGrid, "Opacity", null);
            sb.Completed += delegate
            {
                ResetGrid.Visibility = Visibility.Collapsed;
                ItemPanel.IsEnabled = true;
            };
            sb.Begin();
        }

        private void SearchMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchMethod.SelectedIndex > -1 && findex != SearchMethod.SelectedIndex)
            {
                findex = SearchMethod.SelectedIndex;
                if (findex == 0)
                {
                    GetUsers();
                }
                else
                {
                    flag = 1;
                    var de = SearchMethod.Items[findex].ToString();
                    user_search.Clear();
                    foreach (var i in user_all)
                    {
                        if (i.Depart.Name.Equals(de))
                        {
                            user_search.Add(i);
                        }
                    };
                    Load_Page(1);
                }
            }
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (parent != null)
            {
                parent.iur ??= new Import_User();
                if (!isolated)
                    parent.MainExplorer.Navigate(parent.iur);
                else if (parent.iur.isolated == false)
                {
                    if (parent.MainExplorer.Content == parent.iur)
                    {
                        parent.ppp ??= new Page_Popped();
                        parent.MainExplorer.Navigate(parent.ppp);
                    }
                    new Explorer(parent.iur, parent).Show();
                    parent.iur.isolated = true;
                }

            }
        }
        private void PageMinus_Click(object sender, RoutedEventArgs e)
        {
            if (page > 1)
            {
                page--;
                Load_Page(page);
            }
        }

        private void PagePlus_Click(object sender, RoutedEventArgs e)
        {
            var ap = flag == 0 ? HiroUtils.GetPage(user_all.Count) : HiroUtils.GetPage(user_search.Count);
            if (page <= ap)
            {
                page++;
                Load_Page(page);
            }
        }
    }
}
