using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Utilities;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Warehouser_NET
{
    /// <summary>
    /// Page_Code.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_Code : Page
    {
        internal System.Collections.ObjectModel.ObservableCollection<UIDClass> code_all = new System.Collections.ObjectModel.ObservableCollection<UIDClass>();
        internal System.Collections.ObjectModel.ObservableCollection<UIDClass> code_p = new System.Collections.ObjectModel.ObservableCollection<UIDClass>();
        internal System.Collections.ObjectModel.ObservableCollection<UIDClass> code_search = new System.Collections.ObjectModel.ObservableCollection<UIDClass>();
        internal int flag = 0;
        internal bool isolated = false;
        internal int page = 0;
        internal int index = 0;
        private FunWindow? parent = null;
        public Page_Code(FunWindow parent)
        {
            InitializeComponent();
            ItemData.ItemsSource = code_p;
            new Thread(() =>
            {
                GetUIDs();
            }).Start();
            this.parent = parent;
        }

        private bool GetUIDs()
        {
            Dispatcher.Invoke(() =>
            {
                ItemPanel.IsEnabled = false;
                ShowMsg("服务器通信", "正在从服务器上获取最新数据……", true);
                code_all.Clear();
            });
            try
            {
                var jo = HiroUtils.ParseJson(HiroUtils.SendRequest("/uid", new List<string>() { "action" }, new List<string>() { "3" }));
                if (jo != null)
                {
                    var ja = jo["msg"].AsArray();
                    Dispatcher.Invoke(() =>
                    {
                        var ap = flag == 0 ? code_all.Count % 20 == 0 ? code_all.Count / 20 + 1 : code_all.Count / 20 : code_search.Count % 20 == 0 ? code_search.Count / 20 + 1 : code_search.Count / 20;
                        StatusLabel.Content = string.Format("第 {0}/{1} 页共计{2}项", 1, ap, ja.Count);
                    });
                    for (int i = 0; i < ja.Count; i++)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            code_all.Add(UIDClass.Parse(ja[i]));
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
                    HiroUtils.LogError(ex, "Exception.UID.Get");
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

        private void ItemData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ItemData.SelectedIndex != -1)
            {
                index = ItemData.SelectedIndex;
                ShowDetail(ItemData.SelectedIndex);
            }
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (parent != null)
            {
                parent.ico ??= new Import_Code();
                if (!isolated)
                    parent.MainExplorer.Navigate(parent.ico);
                else if (parent.ico.isolated == false)
                {
                    if (parent.MainExplorer.Content == parent.ico)
                    {
                        parent.ppp ??= new Page_Popped();
                        parent.MainExplorer.Navigate(parent.ppp);
                    }
                    new Explorer(parent.ico, parent).Show();
                    parent.ico.isolated = true;
                }

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

        private void ShowDetail(int index)
        {
            Title_Label.Content = "详细信息";
            DetailPrgBar.Visibility = Visibility.Collapsed;
            var target = flag == 0 ? code_all[index] : code_search[index];
            UIDText.Content = target.UID;
            NameText.Text = target.Name;
            ModelText.Text = target.Model;
            UnitText.Text = target.Unit;
            PriceText.Text = target.Price;
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
        private void StatusLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StatusLabel.Visibility = Visibility.Collapsed;
            PageBox.Visibility = Visibility.Visible;
        }

        private void MsgTitle_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            HideMsg();
        }

        private void RefreashBtn_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                GetUIDs();
            }).Start();
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            Title_Label.Content = "与服务器同步数据……";
            HiroUtils.AddPowerAnimation(0, Title_Label, null, 50).Begin();
            DetailPrgBar.Visibility = Visibility.Visible;
            new Thread(() =>
            {
                var uid = new UIDClass();
                Dispatcher.Invoke(() =>
                {
                    uid = new UIDClass()
                    {
                        UID = UIDText.Content.ToString(),
                        Name = NameText.Text,
                        Model = ModelText.Text,
                        Unit = UnitText.Text,
                        Price = PriceText.Text
                    };
                    code_p[index] = uid;
                    code_all[page * 20 + index] = uid;
                });
                var jn = HiroUtils.ParseJson(HiroUtils.SendRequest("/uid", new List<string>() { "action", "uid", "username", "token", "device" },
                        new List<string>() { "2", uid.toJson().ToString(), HiroUtils.userName, HiroUtils.userToken, "PC" }));
                if (jn != null)
                    Dispatcher.Invoke(() =>
                    {
                        HideDetail();
                    });
            }).Start();
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
                code_search.Clear();
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
                                code_search.Add(UIDClass.Parse(ja[i].AsObject()));
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
                            HideMsg();
                            flag = 1;
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
                    code_search.Clear();
                    if (flag != 0)
                    {
                        flag = 0;
                        new Thread(() =>
                        {
                            GetUIDs();
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
                        var m = SearchMethod.SelectedIndex;
                        new Thread(() =>
                        {
                            Search(k, m);
                        }).Start();
                    }
                }
                e.Handled = true;
            }
        }

        private void Load_Page(int p)
        {
            var ap = flag == 0 ? HiroUtils.GetPage(code_all.Count) : HiroUtils.GetPage(code_search.Count);
            var ac = flag == 0 ? code_all.Count : code_search.Count;
            if (p <= ap && p >= 1)
            {
                p--;
                code_p.Clear();
                page = p;
                for (var i = 0; i < 20; i++)
                {
                    if (p * 20 + i >= ac)
                        break;
                    var ui = flag == 0 ? code_all[p * 20 + i] : code_search[p * 20 + i];
                    code_p.Add(ui);
                }
                StatusLabel.Content = string.Format("第 {0}/{1} 页共计{2}项", p + 1, ap, ac);
            }
        }

        private void PageBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PageBox.Visibility = Visibility.Collapsed;
                StatusLabel.Visibility = Visibility.Visible;
                if (int.TryParse(PageBox.Text, out int r))
                    Load_Page(r);
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
                    var u = new UIDClass();
                    Dispatcher.Invoke(() =>
                    {
                        u = code_p[a];
                    });
                    var jn = HiroUtils.ParseJson(HiroUtils.SendRequest("/uid", new List<string>() { "action", "uid", "username", "token", "device" },
                        new List<string>() { "1", u.toJson().ToString(), HiroUtils.userName, HiroUtils.userToken, "PC" }));
                    Dispatcher.Invoke(() =>
                    {
                        if (jn != null)
                        {
                            code_p.RemoveAt(a);
                            if (flag == 0)
                            {
                                code_all.RemoveAt(page * 20 + a);
                                if (page * 20 + code_p.Count < code_all.Count)
                                {
                                    code_p.Add(code_all[page * 20 + code_p.Count]);
                                }
                            }
                            else
                            {
                                code_search.RemoveAt(page * 20 + a);
                                if (page * 20 + code_p.Count < code_search.Count)
                                {
                                    code_p.Add(code_search[page * 20 + code_p.Count]);
                                }
                            }
                        }
                    });
                }).Start();
            }
        }
    }
}
