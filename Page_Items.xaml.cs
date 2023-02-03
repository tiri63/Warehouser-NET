using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Warehouser_NET
{
    /// <summary>
    /// Page_Items.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_Items : Page
    {
        internal System.Collections.ObjectModel.ObservableCollection<ItemClass> item_all = new System.Collections.ObjectModel.ObservableCollection<ItemClass>();
        internal System.Collections.ObjectModel.ObservableCollection<ItemClass> item_search = new System.Collections.ObjectModel.ObservableCollection<ItemClass>();
        internal System.Collections.ObjectModel.ObservableCollection<ItemClass> item_p = new System.Collections.ObjectModel.ObservableCollection<ItemClass>();
        internal int flag = 0;
        internal bool isolated = false;
        internal int page = 0;
        internal int index = 0;
        private FunWindow? parent = null;
        public Page_Items(FunWindow parent)
        {
            InitializeComponent();
            ItemData.ItemsSource = item_p;
            new Thread(() =>
            {
                GetItems();
            }).Start();
            this.parent = parent;
        }

        private bool GetItems()
        {
            Dispatcher.Invoke(() =>
            {
                ItemPanel.IsEnabled = false;
                ShowMsg("服务器通信", "正在从服务器上获取最新数据……", true);
                item_all.Clear();
            });
            try
            {
                var jo = HiroUtils.ParseJson(HiroUtils.SendRequest("/all", new List<string>() { "action" }, new List<string>() { "1" }));
                if (jo != null)
                {
                    var ja = jo["msg"].AsArray();
                    Dispatcher.Invoke(() =>
                    {
                        var ap = flag == 0 ? item_all.Count % 20 == 0 ? item_all.Count / 20 + 1 : item_all.Count / 20 : item_search.Count % 20 == 0 ? item_search.Count / 20 + 1 : item_search.Count / 20;
                        StatusLabel.Content = string.Format("第 {0}/{1} 页 共计{2}项", 1, ap, ja.Count);
                    });
                    for (int i = 0; i < ja.Count; i++)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            item_all.Add(ItemClass.Parse(ja[i]));
                        });
                    };
                    Dispatcher.Invoke(() =>
                    {
                        ItemPanel.IsEnabled = false;
                        Load_Page(1);
                        HideMsg();
                    });
                    flag = 0;
                    return true;
                }
                Dispatcher.Invoke(() =>
                {
                    ItemPanel.IsEnabled = false;
                    HideMsg();
                });
                flag = 0;
                return false;

            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    ItemPanel.IsEnabled = false;
                    HideMsg();
                    HiroUtils.LogError(ex, "Exception.Item.Get");
                });
                return false;
            }

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HiroUtils.AddPowerAnimation(0, BaseGrid, null, 50).Begin();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox.Text.Length != 0)
                SearchTextLabel.Visibility = Visibility.Hidden;
            else
                SearchTextLabel.Visibility = Visibility.Visible;
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (parent != null)
            {
                parent.iis ??= new Import_Items();
                if (!isolated)
                    parent.MainExplorer.Navigate(parent.iis);
                else if (parent.iis.isolated == false)
                {
                    if (parent.MainExplorer.Content == parent.iis)
                    {
                        parent.ppp ??= new Page_Popped();
                        parent.MainExplorer.Navigate(parent.ppp);
                    }
                    new Explorer(parent.iis, parent).Show();
                    parent.iis.isolated = true;
                }

            }
        }

        private void Load_Page(int p)
        {
            var ap = flag == 0 ? HiroUtils.GetPage(item_all.Count) : HiroUtils.GetPage(item_search.Count);
            var ac = flag == 0 ? item_all.Count : item_search.Count;
            if ((p <= ap || ap == 0) && p >= 1)
            {
                p--;
                item_p.Clear();
                page = p;
                for (var i = 0; i < 20; i++)
                {
                    if (p * 20 + i >= ac)
                        break;
                    var ui = flag == 0 ? item_all[p * 20 + i] : item_search[p * 20 + i];
                    item_p.Add(ui);
                }
                StatusLabel.Content = string.Format("第 {0}/{1} 页 共计{2}项", p + 1, ap, ac);
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Height > 60)
                sv.MaxHeight = Height - 60;
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

        private void ItemData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ItemData.SelectedIndex != -1)
            {
                index = ItemData.SelectedIndex;
                ShowDetail(ItemData.SelectedIndex);
            }
        }

        private void ShowDetail(int index)
        {
            Title_Label.Content = "详细信息";
            DetailPrgBar.Visibility = Visibility.Collapsed;
            var target = flag == 0 ? item_all[index] : item_search[index];
            ShelfText.Text = target.Shelf.FID;
            DepartText.Text = target.Shelf.Depart.Name;
            NameText.Text = target.UID.Name;
            UIDText.Text = target.UID.UID;
            ModelText.Text = target.UID.Model;
            CountText.Text = target.Count.ToString();
            PriceText.Text = target.UID.Price.ToString();
            UsageText.Text = target.FunctionString;
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

        private void RefreashBtn_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                GetItems();
            }).Start();
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            /*Title_Label.Content = "与服务器同步数据……";
            HiroUtils.AddPowerAnimation(0, Title_Label, null, 50).Begin();
            DetailPrgBar.Visibility = Visibility.Visible;
            new Thread(() =>
            {
                HiroUtils.getDeparts();
                var shelf = new ShelfClass();
                var m = -1;
                var s = -1;
                Dispatcher.Invoke(() =>
                {
                    *//*int.TryParse(MainText.Text, out m);
                    int.TryParse(SubText.Text, out s);
                    shelf = new ShelfClass()
                    {
                        FID = UIDText.Text,
                        Depart = HiroUtils.depart_all.Where(x => x.Name.Equals(DepartText.Text)).FirstOrDefault(),
                        MID = m,
                        SID = s,
                        Alias = NameText.Text,
                        Info = DescribeText.Text
                    };
                    shelf_p[index] = shelf;
                    shelf_all[page * 20 + index] = shelf;*//*
                });
                var jn = HiroUtils.ParseJson(HiroUtils.SendRequest("/shelf", new List<string>() { "action", "shelf", "username", "token", "device" },
                        new List<string>() { "2", shelf.toJson().ToString(), HiroUtils.userName, HiroUtils.userToken, "PC" }));
                if (jn != null)
                    Dispatcher.Invoke(() =>
                    {
                        HideDetail();
                    });
            }).Start();*/
            HideDetail();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            HideDetail();
        }

        private void Search(string keyword, int method)
        {
            keyword = HiroUtils.UrlEncode(keyword, Encoding.UTF8);
            Dispatcher.Invoke(() =>
            {
                ShowMsg("搜索", "正在根据条件查找相关内容……", true);
                item_search.Clear();
                new Thread(() =>
                {
                    var jn = HiroUtils.ParseJson(HiroUtils.SendRequest("/search", new List<string>() { "keyword", "method", "username", "token", "device" },
                        new List<string>() { keyword, method.ToString(), HiroUtils.userName, HiroUtils.userToken, "PC" }));
                    if (jn != null)
                    {
                        var ja = jn["msg"].AsArray();
                        Dispatcher.Invoke(() =>
                        {
                            StatusLabel.Content = string.Format("共计{0}项", ja.Count);
                        });
                        for (int i = 0; i < ja.Count; i++)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                item_search.Add(ItemClass.Parse(ja[i].AsObject()));
                            });
                        };
                        Dispatcher.Invoke(() =>
                        {
                            ItemPanel.IsEnabled = true;
                            flag = 1;
                            HideMsg();
                            Load_Page(1);
                        });
                    }
                    else
                        Dispatcher.Invoke(() =>
                        {
                            ItemPanel.IsEnabled = true;
                            flag = 1;
                            HideMsg();
                            Load_Page(1);
                        });

                }).Start();
            });
            //
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (SearchTextBox.Text.Equals(string.Empty))
                {
                    item_search.Clear();
                    if (flag != 0)
                    {
                        flag = 0;
                        new Thread(() =>
                        {
                            GetItems();
                        }).Start();
                    }
                    else
                    {
                        HiroUtils.Notify("请输入关键字，支持正则表达式\n清空搜索栏后，按下 Enter 回到总览模式", "提示");
                    }
                }
                else
                {
                    if (SearchMethod.SelectedIndex != -1)
                    {
                        var k = SearchTextBox.Text;
                        var m = SearchMethod.SelectedIndex + 10;
                        new Thread(() =>
                        {
                            Search(k, m);
                        }).Start();
                    }
                }
                e.Handled = true;
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var a = ItemData.SelectedIndex;
            if (a != -1)
            {
                new Thread(() =>
                {
                    var u = new ItemClass();
                    Dispatcher.Invoke(() =>
                    {
                        u = item_p[a];
                    });
                    var jn = HiroUtils.ParseJson(HiroUtils.SendRequest("/uid", new List<string>() { "action", "uid", "username", "token", "device" },
                        new List<string>() { "1", u.toJson().ToString(), HiroUtils.userName, HiroUtils.userToken, "PC" }));
                    Dispatcher.Invoke(() =>
                    {
                        if (jn != null)
                        {
                            item_p.RemoveAt(a);
                            if (flag == 0)
                            {
                                item_all.RemoveAt(page * 20 + a);
                                if (page * 20 + item_p.Count < item_all.Count)
                                {
                                    item_p.Add(item_all[page * 20 + item_p.Count]);
                                }
                            }
                            else
                            {
                                item_search.RemoveAt(page * 20 + a);
                                if (page * 20 + item_p.Count < item_search.Count)
                                {
                                    item_p.Add(item_search[page * 20 + item_p.Count]);
                                }
                            }
                        }
                    });
                }).Start();
            }
        }
    }
}
