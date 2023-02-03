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
        internal bool lflag = false;
        public MainWindow()
        {
            InitializeComponent();
            new Thread(() =>
            {
                try
                {
                    HiroUtils.InitializeConfig();
                    if (HiroUtils.getUsages())
                    {
                        Thread.Sleep(2000);
                        //まるて読み込み中みたい
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
                            HiroUtils.userDepart = jn2["depart"]["name"].ToString();
                            HiroUtils.userNickname = jn2["nickname"].ToString();
                            HiroUtils.Write_Ini(HiroUtils.ConfigFilePath, "User", "UserDepart", HiroUtils.userDepart);
                            HiroUtils.Write_Ini(HiroUtils.ConfigFilePath, "User", "NickName", HiroUtils.userNickname);
                            App.funWindow = new FunWindow();
                            App.funWindow.Show();
                            Close();
                        }
                        else
                        {
                            lflag = true;
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
            if (Application.Current.Windows.Count <= 0 && !lflag)
            {
                HiroUtils.ExitApp();
            }
        }
    }
}
