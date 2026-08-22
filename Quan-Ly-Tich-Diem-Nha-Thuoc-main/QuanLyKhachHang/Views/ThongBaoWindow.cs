using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace QuanLyKhachHang.Views
{
    /// <summary>
    /// Avalonia không có sẵn MessageBox như WinForms, nên đây là 1 cửa sổ popup nhỏ
    /// dùng chung cho 2 việc: (1) hiển thị thông báo (nút OK), (2) hỏi xác nhận (Có/Không).
    /// </summary>
    public class ThongBaoWindow : Window
    {
        private bool _ketQua;

        private ThongBaoWindow(string tieuDe, string noiDung, bool coHaiNut)
        {
            Title = tieuDe;
            Width = 420;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var lblNoiDung = new TextBlock
            {
                Text = noiDung,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(24, 24, 24, 10),
                FontSize = 13
            };

            var panelNut = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 20, 20),
                Spacing = 10
            };

            if (coHaiNut)
            {
                var btnKhong = new Button { Content = "Không", Width = 90 };
                btnKhong.Click += (s, e) => { _ketQua = false; Close(); };

                var btnCo = new Button
                {
                    Content = "Có",
                    Width = 90,
                    Background = new SolidColorBrush(Color.Parse("#2563EB")),
                    Foreground = Brushes.White
                };
                btnCo.Click += (s, e) => { _ketQua = true; Close(); };

                panelNut.Children.Add(btnKhong);
                panelNut.Children.Add(btnCo);
            }
            else
            {
                var btnOk = new Button
                {
                    Content = "OK",
                    Width = 90,
                    Background = new SolidColorBrush(Color.Parse("#2563EB")),
                    Foreground = Brushes.White
                };
                btnOk.Click += (s, e) => { _ketQua = true; Close(); };
                panelNut.Children.Add(btnOk);
            }

            Content = new DockPanel
            {
                Children =
                {
                    lblNoiDung,
                    panelNut
                }
            };
            DockPanel.SetDock(panelNut, Dock.Bottom);
        }

        /// <summary>Hiển thị thông báo đơn giản, chỉ có nút OK.</summary>
        public static Task ThongBao(Window? cha, string tieuDe, string noiDung)
        {
            var cuaSo = new ThongBaoWindow(tieuDe, noiDung, coHaiNut: false);
            return cha != null ? cuaSo.ShowDialog(cha) : ShowKhongCha(cuaSo);
        }

        /// <summary>Hỏi xác nhận Có / Không, trả về true nếu người dùng chọn "Có".</summary>
        public static async Task<bool> XacNhan(Window? cha, string tieuDe, string noiDung)
        {
            var cuaSo = new ThongBaoWindow(tieuDe, noiDung, coHaiNut: true);
            if (cha != null)
                await cuaSo.ShowDialog(cha);
            else
                await ShowKhongCha(cuaSo);
            return cuaSo._ketQua;
        }

        private static Task ShowKhongCha(ThongBaoWindow cuaSo)
        {
            var tcs = new TaskCompletionSource();
            cuaSo.Closed += (s, e) => tcs.TrySetResult();
            cuaSo.Show();
            return tcs.Task;
        }
    }
}
