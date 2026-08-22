using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using QuanLyKhachHang.Models;

namespace QuanLyKhachHang.Views
{
    /// <summary>
    /// Cửa sổ popup dùng chung cho cả 2 chức năng Thêm mới và Sửa khách hàng.
    /// Khi Sửa: mã KH bị khoá. Khi Thêm: mã KH được sinh tự động và truyền vào từ ngoài.
    /// </summary>
    public class KhachHangEditWindow : Window
    {
        private readonly TextBox _txtMaKH = new() { IsReadOnly = true };
        private readonly TextBox _txtHoTen = new();
        private readonly TextBox _txtSoDienThoai = new();
        private readonly NumericUpDown _numDiem = new() { Maximum = 1_000_000, Minimum = 0, FormatString = "0" };

        private readonly bool _laSua;
        private readonly DateTime _ngayTaoGoc;

        public KhachHang? KetQua { get; private set; }

        /// <summary>
        /// Cửa sổ popup Thêm/Sửa khách hàng.
        /// </summary>
        /// <param name="maKHMoi">Mã KH tự sinh, dùng khi thêm mới.</param>
        /// <param name="khDangSua">Khách hàng đang sửa (null nếu là thêm mới).</param>
        /// <param name="sdtMacDinh">
        /// Số điện thoại điền sẵn khi mở popup Thêm mới (ví dụ: số vừa gõ ở ô "Tạo hoá đơn nhanh"
        /// bên Trang chủ mà chưa khớp khách hàng nào) — giúp người dùng không phải gõ lại lần 2.
        /// </param>
        public KhachHangEditWindow(string maKHMoi, KhachHang? khDangSua = null, string? sdtMacDinh = null)
        {
            _laSua = khDangSua != null;
            _ngayTaoGoc = _laSua ? khDangSua!.NgayTao : DateTime.Now;

            Title = _laSua ? "Sửa thông tin khách hàng" : "Thêm khách hàng mới";
            Width = 420;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _txtMaKH.Text = _laSua ? khDangSua!.MaKH : maKHMoi;
            _txtHoTen.Text = _laSua ? khDangSua!.HoTen : string.Empty;
            _txtSoDienThoai.Text = _laSua ? khDangSua!.SoDienThoai : (sdtMacDinh ?? string.Empty);
            _numDiem.Value = _laSua ? khDangSua!.DiemTichLuy : 0;

            var form = new StackPanel { Margin = new Thickness(20), Spacing = 14 };
            form.Children.Add(TaoDong("Mã khách hàng:", _txtMaKH));
            form.Children.Add(TaoDong("Họ tên:", _txtHoTen));
            form.Children.Add(TaoDong("Số điện thoại:", _txtSoDienThoai));
            form.Children.Add(TaoDong("Điểm tích luỹ:", _numDiem));

            var panelNut = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10
            };

            var btnHuy = new Button { Content = "Huỷ", Width = 90 };
            btnHuy.Click += (s, e) => { KetQua = null; Close(); };

            var btnLuu = new Button
            {
                Content = "💾 Lưu",
                Width = 100,
                Background = new SolidColorBrush(Color.Parse("#2563EB")),
                Foreground = Brushes.White
            };
            btnLuu.Click += BtnLuu_Click;

            panelNut.Children.Add(btnHuy);
            panelNut.Children.Add(btnLuu);
            form.Children.Add(panelNut);

            Content = form;
        }

        private StackPanel TaoDong(string nhan, Control input)
        {
            input.Width = 260;
            var dong = new StackPanel { Spacing = 5 };
            dong.Children.Add(new TextBlock { Text = nhan, FontSize = 13 });
            dong.Children.Add(input);
            return dong;
        }

        private async void BtnLuu_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtHoTen.Text))
            {
                await ThongBaoWindow.ThongBao(this, "Thiếu thông tin", "Vui lòng nhập họ tên khách hàng.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtSoDienThoai.Text))
            {
                await ThongBaoWindow.ThongBao(this, "Thiếu thông tin", "Vui lòng nhập số điện thoại.");
                return;
            }

            KetQua = new KhachHang
            {
                MaKH = _txtMaKH.Text ?? string.Empty,
                HoTen = _txtHoTen.Text!.Trim(),
                SoDienThoai = _txtSoDienThoai.Text!.Trim(),
                DiemTichLuy = (int)(_numDiem.Value ?? 0),
                NgayTao = _ngayTaoGoc
            };

            Close();
        }
    }
}
