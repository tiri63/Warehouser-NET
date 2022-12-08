﻿using System;
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
                    JsonNode jn = JsonObject.Parse(HiroUtils.SendRequest("/usage", new List<string>() { "action" }, new List<string>() { "2" }));
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
                        HiroUtils.usages.Add(new UsageClass(int.Parse(jo["id"].ToString()), jo["name"].ToString(), info));
                    }
                    Thread.Sleep(1000);

                    JsonNode jn2 = JsonObject.Parse(HiroUtils.SendRequest("/user", new List<string>() { "action", "username", "token", "device" },
                        new List<string>() { "0", HiroUtils.userName, HiroUtils.userToken, "PC" }));
                    Dispatcher.Invoke(() =>
                    {
                        if (jn2["ret"].ToString() != "0")
                        {
                            new Login().Show();
                            Close();
                        }
                        else
                        {
                            new FunWindow().Show();
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
