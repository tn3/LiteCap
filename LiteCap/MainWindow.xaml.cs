using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LiteCap
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        Point firstPoint,capPoint;
        public Shape redCaptureArea = new Rectangle();
        int fx;
        int fy;
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            firstPoint = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            //マージン用にローカルな座標を取得
            capPoint = e.GetPosition(this);

            redCaptureArea.Stroke = Brushes.Red;
            redCaptureArea.StrokeThickness = 1.5;
            redCaptureArea.Fill = new SolidColorBrush(Color.FromArgb(64, 96, 0, 0));
            redCaptureArea.HorizontalAlignment = HorizontalAlignment.Left;
            redCaptureArea.VerticalAlignment = VerticalAlignment.Top;
            var marginValue = new Thickness();
            marginValue.Left = capPoint.X;
            marginValue.Top = capPoint.Y;
            redCaptureArea.Margin = marginValue;
            mainGrid.Children.Add(redCaptureArea);
            fx = Convert.ToInt32(firstPoint.X);
            fy = Convert.ToInt32(firstPoint.Y);
            //Console.WriteLine(firstPoint);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var p = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                //マージン用にローカルな座標取得
                var cp = e.GetPosition(this);
                //Console.WriteLine(p);
                var height = p.Y - firstPoint.Y;
                var width = p.X - firstPoint.X;
                //Console.WriteLine(dpiX.ToString() + "," + p.Y.ToString());
                //if (height<=0||width<=0)
                //{
                    //Console.WriteLine(cp.Y.ToString());
                    var marginValue = new Thickness();
                    marginValue.Left = capPoint.X;
                    marginValue.Top = capPoint.Y;
                    if (height<=0)
                    {
                        height = (-(height));
                        marginValue.Top = cp.Y;
                        fy = Convert.ToInt32(p.Y);
                    }
                    if(width<=0)
                    {
                        width = (-(width));
                        marginValue.Left = cp.X;
                        fx = Convert.ToInt32(p.X);
                    }
                    redCaptureArea.Margin = marginValue;
                    //Console.WriteLine(marginValue.ToString());
                //}
                redCaptureArea.Height = height / dpiY;
                redCaptureArea.Width = width / dpiX;
            }
        }

        private async void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                windowmain.Background.Opacity = 0.0;
                redCaptureArea.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                redCaptureArea.StrokeThickness = 0;
                string s = System.IO.Path.GetTempFileName();
                //保存ファイル名取得
                DateTime time = DateTime.Now;
                string captime = time.ToString("yyyy-MM-dd_mmss-fff");
                string capfile = captime + ".png";
                string capdir = "saveimg";
                string s2 = capdir + "\\" + capfile;
                //画像が赤くなるバグを回避
                await Task.Run(() => System.Threading.Thread.Sleep(100));
                //Console.WriteLine(dpiX.ToString());
                using (var bmp = new System.Drawing.Bitmap(Convert.ToInt32(redCaptureArea.Width * dpiX), Convert.ToInt32(redCaptureArea.Height * dpiY), System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                using (var graph = System.Drawing.Graphics.FromImage(bmp))
                {
                    // 画面をコピーする
                    //Console.WriteLine(firstPoint.X+","+firstPoint.Y+","+ new System.Drawing.Point() + "," + bmp.Size);
                    graph.CopyFromScreen(new System.Drawing.Point(Convert.ToInt32(fx /* dpiX*/), Convert.ToInt32(fy /* dpiY*/)), new System.Drawing.Point(), bmp.Size);
                    bmp.SetPixel(0, 0, System.Drawing.Color.FromArgb(0, 0, 0, 0));
                    bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
                    bmp.Save(s2, System.Drawing.Imaging.ImageFormat.Png);
                }
                var w = new tweet();
                w.filepath = s;
                w.Show();
                Close();
            }
            catch(OverflowException)
            {
                Close();
            }
        }

        private void windowmain_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void windowmain_MouseLeave(object sender, MouseEventArgs e)
        {
            //マルチディスプレイ処理
            if (windowmain.IsMouseOver == false && e.LeftButton != MouseButtonState.Pressed)
            {
                //マルチディスプレイ処理
                windowmain.WindowState = WindowState.Normal;
                windowmain.Width = 1;
                windowmain.Height = 1;
                //Console.WriteLine(System.Windows.Forms.Cursor.Position.X + "," + System.Windows.Forms.Cursor.Position.Y);
                windowmain.Left = System.Windows.Forms.Cursor.Position.X/dpiX;
                windowmain.Top = System.Windows.Forms.Cursor.Position.Y/dpiY;
                windowmain.WindowState = WindowState.Maximized;
            }
        }
        double dpiX = 1;
        double dpiY = 1;

        private void windowmain_Loaded(object sender, RoutedEventArgs e)
        {
            dpiX = VisualTreeHelper.GetDpi(windowmain).DpiScaleX;
            dpiY = VisualTreeHelper.GetDpi(windowmain).DpiScaleY;
        }

        private void windowmain_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            dpiX = e.NewDpi.DpiScaleX;
            dpiY = e.NewDpi.DpiScaleY;
            //Console.WriteLine(dpiX.ToString());
        }
    }
}
