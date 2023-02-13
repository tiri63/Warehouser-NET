using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ZXing.QrCode;
using ZXing;
using System.Diagnostics;
using NPOI.SS.Formula.Functions;
using System.Drawing;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Globalization;
using System.Reflection;

namespace Warehouser_NET
{
    /// <summary>
    /// Page_Shelf.xaml の相互作用ロジック
    /// </summary>
    public partial class Page_Shelf : Page
    {
        internal System.Collections.ObjectModel.ObservableCollection<ShelfClass> shelf_all = new System.Collections.ObjectModel.ObservableCollection<ShelfClass>();
        internal System.Collections.ObjectModel.ObservableCollection<ShelfClass> shelf_search = new System.Collections.ObjectModel.ObservableCollection<ShelfClass>();
        internal System.Collections.ObjectModel.ObservableCollection<ShelfClass> shelf_p = new System.Collections.ObjectModel.ObservableCollection<ShelfClass>();
        internal int flag = 0;
        internal bool isolated = false;
        internal int page = 0;
        internal int index = 0;
        private FunWindow? parent = null;
        public Page_Shelf(FunWindow parent)
        {
            InitializeComponent();
            ItemData.ItemsSource = shelf_p;
            new Thread(() =>
            {
                GetShelves();
            }).Start();
            this.parent = parent;
        }

        private bool GetShelves()
        {
            Dispatcher.Invoke(() =>
            {
                ItemPanel.IsEnabled = false;
                ShowMsg("服务器通信", "正在从服务器上获取最新数据……", true);
                shelf_all.Clear();
            });
            try
            {
                var jo = HiroUtils.ParseJson(HiroUtils.SendRequest("/shelf", new List<string>() { "action" }, new List<string>() { "3" }));
                if (jo != null)
                {
                    var ja = jo["msg"].AsArray();
                    Dispatcher.Invoke(() =>
                    {
                        var ap = flag == 0 ? shelf_all.Count % 20 == 0 ? shelf_all.Count / 20 + 1 : shelf_all.Count / 20 : shelf_search.Count % 20 == 0 ? shelf_search.Count / 20 + 1 : shelf_search.Count / 20;
                        StatusLabel.Content = string.Format("第 {0}/{1} 页 共计{2}项", 1, ap, ja.Count);
                    });
                    for (int i = 0; i < ja.Count; i++)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            shelf_all.Add(ShelfClass.Parse(ja[i]));
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
                    HiroUtils.LogError(ex, "Exception.Shelf.Get");
                });
                return false;
            }
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (parent != null)
            {
                parent.isf ??= new Import_Shelf();
                if (!isolated)
                    parent.MainExplorer.Navigate(parent.isf);
                else if (parent.isf.isolated == false)
                {
                    if (parent.MainExplorer.Content == parent.isf)
                    {
                        parent.ppp ??= new Page_Popped();
                        parent.MainExplorer.Navigate(parent.ppp);
                    }
                    new Explorer(parent.isf, parent).Show();
                    parent.isf.isolated = true;
                }

            }
        }

        private void Load_Page(int p)
        {
            var ap = flag == 0 ? HiroUtils.GetPage(shelf_all.Count) : HiroUtils.GetPage(shelf_search.Count);
            var ac = flag == 0 ? shelf_all.Count : shelf_search.Count;
            if ((p <= ap || ap == 0) && p >= 1)
            {
                p--;
                shelf_p.Clear();
                page = p;
                for (var i = 0; i < 20; i++)
                {
                    if (p * 20 + i >= ac)
                        break;
                    var ui = flag == 0 ? shelf_all[p * 20 + i] : shelf_search[p * 20 + i];
                    shelf_p.Add(ui);
                }
                StatusLabel.Content = string.Format("第 {0}/{1} 页 共计{2}项", p + 1, ap, ac);
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
            var target = flag == 0 ? shelf_all[index] : shelf_search[index];
            UIDText.Text = target.FID;
            DepartText.Text = target.Depart.Name;
            MainText.Text = target.MID.ToString();
            SubText.Text = target.SID.ToString();
            NameText.Text = target.Alias;
            DescribeText.Text = target.Info;
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
                GetShelves();
            }).Start();
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            Title_Label.Content = "与服务器同步数据……";
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
                    int.TryParse(MainText.Text, out m);
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
                    shelf_all[page * 20 + index] = shelf;
                });
                var jn = HiroUtils.ParseJson(HiroUtils.SendRequest("/shelf", new List<string>() { "action", "shelf", "username", "token", "device" },
                        new List<string>() { "2", shelf.toJson().ToString(), HiroUtils.userName, HiroUtils.userToken, "PC" }));
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
                shelf_search.Clear();
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
                                shelf_search.Add(ShelfClass.Parse(ja[i].AsObject()));
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
                    shelf_search.Clear();
                    if (flag != 0)
                    {
                        flag = 0;
                        new Thread(() =>
                        {
                            GetShelves();
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
                        var m = SearchMethod.SelectedIndex + 20;
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
                    var u = new ShelfClass();
                    Dispatcher.Invoke(() =>
                    {
                        u = shelf_p[a];
                    });
                    var jn = HiroUtils.ParseJson(HiroUtils.SendRequest("/uid", new List<string>() { "action", "uid", "username", "token", "device" },
                        new List<string>() { "1", u.toJson().ToString(), HiroUtils.userName, HiroUtils.userToken, "PC" }));
                    Dispatcher.Invoke(() =>
                    {
                        if (jn != null)
                        {
                            shelf_p.RemoveAt(a);
                            if (flag == 0)
                            {
                                shelf_all.RemoveAt(page * 20 + a);
                                if (page * 20 + shelf_p.Count < shelf_all.Count)
                                {
                                    shelf_p.Add(shelf_all[page * 20 + shelf_p.Count]);
                                }
                            }
                            else
                            {
                                shelf_search.RemoveAt(page * 20 + a);
                                if (page * 20 + shelf_p.Count < shelf_search.Count)
                                {
                                    shelf_p.Add(shelf_search[page * 20 + shelf_p.Count]);
                                }
                            }
                        }
                    });
                }).Start();
            }
        }

        private void QRCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            var a = ItemData.SelectedIndex;
            if (a != -1)
            {
                var u = shelf_p[a];
                Directory.CreateDirectory(HiroUtils.Path_Prepare("<current>\\qrcode\\"));
                GenerateQRCode($"{HiroUtils.baseURL}/qr?i={u.FID}", u.FID).Save(HiroUtils.Path_Prepare("<current>\\qrcode\\") + u.FID + ".bmp");
                HiroUtils.Notify("点此查看文件位置", "生成成功", 2, () =>
                {
                    ProcessStartInfo pinfo_ = new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = HiroUtils.Path_Prepare("<current>\\qrcode\\"),
                    };
                    try
                    {
                        _ = Process.Start(pinfo_);
                    }
                    catch
                    {
                        HiroUtils.Notify("无法打开二维码文件位置", "错误");
                    }
                });
            }
        }

        private System.Drawing.Bitmap GenerateQRCode(string txt, string title)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    CharacterSet = "UTF-8",
                    Height = 300,
                    Width = 300
                }
            };
            var qr = barcodeWriter.Write(txt);
            Bitmap bmpimg = new Bitmap(qr);
            using (Graphics g = Graphics.FromImage(bmpimg))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.DrawImage(qr, 0, 0, qr.Width, qr.Height);
                Font f = new Font("Microsoft YaHei UI", 14);
                RectangleF rect = new RectangleF(0, qr.Height - f.GetHeight(g), qr.Width, qr.Height);
                var stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                g.DrawString(title, f, System.Drawing.Brushes.Black, rect, stringFormat);
            }
            return bmpimg;
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
            var ap = flag == 0 ? HiroUtils.GetPage(shelf_all.Count) : HiroUtils.GetPage(shelf_search.Count);
            if (page <= ap)
            {
                page++;
                Load_Page(page);
            }
        }
    }
}
