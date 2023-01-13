using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows;

namespace Warehouser_NET
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            new Thread(() =>
            {
                try
                {
                    HiroUtils.InitializeConfig();
                    HiroUtils.usages.Clear();
                    var jn = HiroUtils.ParseJson(HiroUtils.SendRequest("/usage", new List<string>() { "action" }, new List<string>() { "2" }));
                    if (jn != null)
                    {
                        JsonArray ja = jn["msg"].AsArray();
                        foreach (var jo in ja)
                        {
                            string? info = null;
                            try
                            {
                                info = jo["info"].ToString();
                            }
                            catch
                            {

                            }
                            Thread.Sleep(100);
                            HiroUtils.usages.Add(new UsageClass()
                            {
                                Code = int.Parse(jo["id"].ToString()),
                                Alias = jo["name"].ToString(),
                                Info = info,
                                Hide = jo["hide"].ToString().ToLower().Equals("true") || jo["hide"].ToString().Equals("1"),
                                HideStr = jo["hide"].ToString().ToLower().Equals("true") || jo["hide"].ToString().Equals("1") ? "隐藏" : "可见"
                            });
                        }
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Close();
                        });
                        return;
                    }


                    var jn2 = HiroUtils.ParseJson(HiroUtils.SendRequest("/user", new List<string>() { "action", "username", "token", "device" },
                        new List<string>() { "0", HiroUtils.userName, HiroUtils.userToken, "PC" }));
                    Dispatcher.Invoke(() =>
                    {
                        if (jn2 != null)
                        {
                            App.funWindow = new FunWindow();
                            App.funWindow.Show();
                            Close();

                        }
                        else
                        {
                            new Login().Show();
                            Close();
                        }
                    });


                }
                catch (Exception ex)
                {
                    HiroUtils.LogError(ex, "Exception.StartWin.Initialize");
                    HiroUtils.ExitApp();
                }
            }).Start();
        }

        private void StartWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Application.Current.Windows.Count <= 0)
            {
                HiroUtils.ExitApp();
            }
        }
    }
}
