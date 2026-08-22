using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Views
{
    public class MainWindow : Window
    {
        private readonly DataService _data = new();
        private readonly ContentControl _content = new();
        private Button? _btnDangChon;

        public MainWindow()
        {
            Title = "Phần mềm Quản lý Khách hàng & Tích điểm";
            Width = 1200;
            Height = 720;
            MinWidth = 1000;
            MinHeight = 640;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // ---- Style riêng cho menu sidebar để phần tô sáng (hover / đang chọn) hiển thị
            //      ĐỀU MÀU, PHỦ KÍN TOÀN BỘ NÚT — không bị theme mặc định (FluentTheme) chèn thêm
            //      lớp phủ hover/pressed với màu và bo góc khác, gây cảm giác tô sáng không đều. ----
            Styles.Add(new Style(x => x.OfType<Button>().Class("menuSidebar"))
            {
                Setters = { new Setter(Button.MarginProperty, new Thickness(0)) }
            });
            Styles.Add(new Style(x => x.OfType<Button>().Class("menuSidebar").Class(":pointerover"))
            {
                Setters = { new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.Parse("#374151"))) }
            });
            Styles.Add(new Style(x => x.OfType<Button>().Class("menuSidebar").Class(":pressed"))
            {
                Setters = { new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.Parse("#2563EB"))) }
            });

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition(210, GridUnitType.Pixel));
            grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            // ---- Sidebar ----
            var stackSidebar = new StackPanel();

            var lblTieuDe = new TextBlock
            {
                Text = "🏬 QLKH & Tích điểm",
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeight.Bold,
                Height = 70,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var btnTrangChu = TaoNutMenu("🏠  Màn hình chính");
            var btnKhachHang = TaoNutMenu("👤  Khách hàng");
            var btnKhoQua = TaoNutMenu("🎁  Kho quà");
            var btnThuoc = TaoNutMenu("💊  Thuốc");
            var btnDonHang = TaoNutMenu("🧾  Đơn hàng / Tích điểm");
            var btnThongKe = TaoNutMenu("📊  Thống kê");
            var btnThoat = TaoNutMenu("🚪  Thoát");

            // Hàm tạo TrangChuView có hỗ trợ Callback chuyển tab
            TrangChuView TaoTrangChuView() => new TrangChuView(_data, (tabIndex, maKH) =>
            {
                if (tabIndex == 1) // Tab Khách hàng
                {
                    HienThi(new KhachHangView(_data), btnKhachHang);
                }
                else if (tabIndex == 2) // Tab Đơn hàng / Tích điểm
                {
                    var donHangView = new DonHangView(_data);
                    if (!string.IsNullOrEmpty(maKH))
                    {
                        donHangView.ChonKhachHang(maKH);
                    }
                    HienThi(donHangView, btnDonHang);
                }
            });

            btnTrangChu.Click += (s, e) => HienThi(TaoTrangChuView(), btnTrangChu);
            btnKhachHang.Click += (s, e) => HienThi(new KhachHangView(_data), btnKhachHang);
            btnKhoQua.Click += (s, e) => HienThi(new KhoQuaView(_data), btnKhoQua);
            btnThuoc.Click += (s, e) => HienThi(new ThuocView(_data), btnThuoc);
            btnDonHang.Click += (s, e) => HienThi(new DonHangView(_data), btnDonHang);
            btnThongKe.Click += (s, e) => HienThi(new ThongKeView(_data), btnThongKe);
            btnThoat.Click += (s, e) => Close();

            stackSidebar.Children.Add(lblTieuDe);
            stackSidebar.Children.Add(btnTrangChu);
            stackSidebar.Children.Add(btnKhachHang);
            stackSidebar.Children.Add(btnKhoQua);
            stackSidebar.Children.Add(btnThuoc);
            stackSidebar.Children.Add(btnDonHang);
            stackSidebar.Children.Add(btnThongKe);
            stackSidebar.Children.Add(btnThoat);

            var khungSidebar = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#1F2937")),
                Child = stackSidebar
            };
            Grid.SetColumn(khungSidebar, 0);

            // ---- Vùng nội dung ----
            var khungNoiDung = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#F3F4F6")),
                Padding = new Thickness(20),
                Child = _content
            };
            Grid.SetColumn(khungNoiDung, 1);

            grid.Children.Add(khungSidebar);
            grid.Children.Add(khungNoiDung);
            Content = grid;

            // Hiển thị Trang chủ khi ứng dụng vừa khởi chạy
            HienThi(TaoTrangChuView(), btnTrangChu);
        }

        private Button TaoNutMenu(string text)
        {
            return new Button
            {
                Content = text,
                Height = 48,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(18, 0, 0, 0),
                Margin = new Thickness(0),
                Background = new SolidColorBrush(Color.Parse("#1F2937")),
                Foreground = new SolidColorBrush(Color.Parse("#D1D5DB")),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                FontSize = 14,
                Classes = { "menuSidebar" },
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
            };
        }

        private void HienThi(UserControl man, Button? nutNguon)
        {
            _content.Content = man;
            if (nutNguon != null) DanhDauNutDangChon(nutNguon);
        }

        private void DanhDauNutDangChon(Button nut)
        {
            if (_btnDangChon != null)
                _btnDangChon.Background = new SolidColorBrush(Color.Parse("#1F2937"));

            nut.Background = new SolidColorBrush(Color.Parse("#2563EB"));
            _btnDangChon = nut;
        }
    }
}