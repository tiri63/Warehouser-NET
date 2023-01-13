using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Threading;
using System.Text.Json.Nodes;

namespace Warehouser_NET
{
    /// <summary>
    /// Login.xaml の相互作用ロジック
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginBtn.IsEnabled = false;
            new Thread(() =>
            {
                try
                {
                    var userName = string.Empty;
                    var userPwd = string.Empty;
                    Dispatcher.Invoke(() =>
                    {
                        userName = UserName.Text;
                        userPwd = UserPwd.Password;
                    });
                    var jn = HiroUtils.ParseJson(HiroUtils.SendRequest("/user", new List<string>() { "action", "username", "password", "device" },
                        new List<string>() { "1", userName, userPwd, "PC" }));

                    Dispatcher.Invoke(() =>
                    {
                        if (jn != null)
                        {
                            HiroUtils.userName = userName;
                            HiroUtils.userDepart = jn["depart"]["name"].ToString();
                            HiroUtils.userToken = jn["token"].ToString();
                            HiroUtils.userNickname = jn["nickname"].ToString();
                            HiroUtils.Write_Ini(HiroUtils.ConfigFilePath, "User", "UserName", HiroUtils.userName);
                            HiroUtils.Write_Ini(HiroUtils.ConfigFilePath, "User", "UserToken", HiroUtils.userToken);
                            HiroUtils.Write_Ini(HiroUtils.ConfigFilePath, "User", "UserDepart", HiroUtils.userDepart);
                            HiroUtils.Write_Ini(HiroUtils.ConfigFilePath, "User", "NickName", HiroUtils.userNickname);
                            new FunWindow().Show();
                            Close();
                        }
                        LoginBtn.IsEnabled = true;
                    });

                }
                catch (Exception ex)
                {
                    HiroUtils.LogError(ex, "Exception.Login");
                }

            }).Start();
        }

        private void LoginWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Application.Current.Windows.Count <= 0)
            {
                HiroUtils.ExitApp();
            }
        }
    }
}
