using CoreTweet;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LiteCap
{
    /// <summary>
    /// auth.xaml の相互作用ロジック
    /// </summary>
    public partial class auth : Window
    {
        public auth()
        {
            InitializeComponent();
        }
        //CoreTweet初期化
        public OAuth.OAuthSession session;
        public Tokens token;

        private void pin_gen_Click(object sender, RoutedEventArgs e)
        {
            //token発行
            session = OAuth.Authorize(twitter.Consumer_Key, twitter.Consumer_Secret);
            string url = session.AuthorizeUri.ToString();
            System.Diagnostics.Process.Start(url);
            pin_box.IsEnabled = true;
        }

        private void pin_box_TextChanged(object sender, TextChangedEventArgs e)
        {
            var code = pin_box.Text;
            int n;
            if (code.Length == 7 && int.TryParse(code, out n) && auth1.IsEnabled == false)
            {
                auth1.IsEnabled = true;
            }
            else if (auth1.IsEnabled == true)
            {
                auth1.IsEnabled = false;
            }
        }

        private void auth1_Click(object sender, RoutedEventArgs e)
        {
            var code = pin_box.Text;
            try
            {
                token = session.GetTokens(code);
            }
            catch (TwitterException ex)
            {
                MessageBox.Show("エラーが発生しました、もう一度やり直してください\n" + ex.Message);
                return;
            }
            catch (System.Net.WebException ex)
            {
                MessageBox.Show("エラーが発生しました、もう一度やり直してください\n" + ex.Message);
                return;
            }
            if(Properties.Settings.Default.AccessToken=="")
            {
                Properties.Settings.Default.AccessToken = token.AccessToken;
                Properties.Settings.Default.TokenSecret = token.AccessTokenSecret;
                //Console.WriteLine(token.AccessToken + "/" + token.AccessTokenSecret);
            }
            else
            {
                Properties.Settings.Default.AccessToken = Properties.Settings.Default.AccessToken + "," + token.AccessToken;
                Properties.Settings.Default.TokenSecret = Properties.Settings.Default.TokenSecret + "," + token.AccessTokenSecret;
            }
            Properties.Settings.Default.Save();
            Close();
        }
    }
}
