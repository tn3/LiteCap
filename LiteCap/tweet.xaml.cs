using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CoreTweet;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace LiteCap
{
    /// <summary>
    /// tweet.xaml の相互作用ロジック
    /// </summary>
    public partial class tweet : Window
    {
        public tweet()
        {
            InitializeComponent();
        }
        
        //CoreTweet初期化
        public OAuth.OAuthSession session;
        public Tokens token;
        //取得
        public string filepath { get; internal set; }

        private void tweetWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //インターネット接続確認
            if (Internet_Check())
            {
                //bmp読み込み
                //http://neareal.net/index.php?Programming%2F.NetFramework%2FWPF%2FWriteableBitmap%2FLoadReleaseableBitmapImage
                MemoryStream data = new MemoryStream(File.ReadAllBytes(filepath));
                WriteableBitmap wbmp = new WriteableBitmap(BitmapFrame.Create(data));
                data.Close();

                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = wbmp;
                imageBrush.Stretch = Stretch.Uniform;
                tweetWindow.Background = imageBrush;

                if (Properties.Settings.Default.AccessToken == "")
                {
                    Window f = new auth();
                    f.ShowDialog();
                }
                update_account();
            }
            else
            {
                MessageBox.Show("このアプリケーションをオフラインで使用することはできません。終了します。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                Application.Current.Shutdown();
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window f = new auth();
            f.ShowDialog();
            update_account();
        }
        private void update_account()
        {
            comboBox.Items.Clear();
            Button newA = new Button();
            newA.Click += this.Button_Click;
            newA.Name = "add";
            newA.Content = "Add Account";
            newA.Background = null;
            newA.BorderBrush = null;
            comboBox.Items.Add(newA);

            string[] AT = Properties.Settings.Default.AccessToken.Split(',');
            string[] TS = Properties.Settings.Default.TokenSecret.Split(',');
            //Console.WriteLine(AT[0]);
            for (int i = 0; i < AT.Length; i++)
            {
                if (AT[i] != "")
                {
                    token = Tokens.Create(twitter.Consumer_Key, twitter.Consumer_Secret, AT[i], TS[i]);
                    var name = token.Account.VerifyCredentials();
                    //Console.WriteLine(name.ScreenName);
                    Button newB = new Button();
                    newB.Click += this.Button2_Click;
                    newB.Content = name.ScreenName;
                    newB.Tag = i;
                    newB.Background = null;
                    newB.BorderBrush = null;
                    comboBox.Items.Add(newB);
                }
                else
                {
                    Application.Current.Shutdown();
                    return;
                }
            }
            comboBox.SelectedItem = comboBox.Items[1];
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            comboBox_SelectionChanged(sender,null);
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox.SelectedItem != null)
            {
                var selectedItem = (Button)comboBox.SelectedItem;
                if (selectedItem.Name == "add")
                {
                    Button_Click(sender, e);
                }
                else
                {

                    string[] AT = Properties.Settings.Default.AccessToken.Split(',');
                    string[] TS = Properties.Settings.Default.TokenSecret.Split(',');
                    int i = Convert.ToInt32(selectedItem.Tag);
                    token = Tokens.Create(twitter.Consumer_Key, twitter.Consumer_Secret, AT[i], TS[i]);
                    //var test = token.Account.VerifyCredentials();
                    //Console.WriteLine(test.Name);
                    textBox.Focus();
                }
            }
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            FileInfo fileinfo = new FileInfo(filepath);
            fileinfo.Refresh();
            if (fileinfo.Length > 3000000)
            {
                if (MessageBox.Show("ファイルサイズが3MBを超えています。JPEGに変換して投稿しますか？", "Info", MessageBoxButton.YesNo,
                MessageBoxImage.Information) == MessageBoxResult.No)
                {
                    Close();
                }
                else
                {
                    Bitmap bmp = new Bitmap(filepath);
                    string s = System.IO.Path.GetTempFileName();
                    bmp.Save(s, ImageFormat.Jpeg);
                    bmp.Dispose();
                    fileinfo.Delete();
                    filepath = s;
                    update();
                }
            }
            else
            {
                update();
            }
        }
        async void update()
        {
            button.IsEnabled = false;
            button.Content = "Uploading...";
            textBox.IsEnabled = false;
            comboBox.IsEnabled = false;
            var mediaUploadTask = token.Media.UploadAsync(
                    media => new FileInfo(filepath));
            string statusText = textBox.Text;

            await mediaUploadTask.ContinueWith((x) =>
            {
                if (x.IsCompleted)
                {
                    token.Statuses.Update(
                        status => statusText,
                        media_ids => x.Result.MediaId);
                    return;
                }
            });
            Application.Current.Shutdown();
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        button_Click_1(sender,null);
                        break;
                }
            }
        }
        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            try
            {
                var f = new FileInfo(filepath);
                f.Delete();
            }
            catch(IOException)
            {
                return;
            }
            //ApplicationExitイベントハンドラを削除
            Application.Current.Exit -= new ExitEventHandler(Application_ApplicationExit);
        }
        static bool Internet_Check()
        {
            //http://dobon.net/vb/dotnet/internet/ping.html
            System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
            //Pingを送信する
            try
            {
                System.Net.NetworkInformation.PingReply reply = p.Send("api.twitter.com", 3000);
                //解放
                p.Dispose();
                //結果を取得しreturn
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    return true;
                else
                    return false;
            }
            //もしpingを送信できない場合はfalseを返す
            catch (System.Net.NetworkInformation.PingException)
            {
                return false;
            }
        }
    }
}
