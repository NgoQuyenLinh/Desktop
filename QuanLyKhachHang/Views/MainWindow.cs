using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Views
{
    /// <summary>
    /// Cửa sổ chính của ứng dụng: sidebar bên trái để điều hướng (giống layout
    /// bản WinForms trước đây) và vùng nội dung bên phải hoán đổi giữa các UserControl:
    /// Trang chủ, Khách hàng, Đơn hàng, Thống kê.
    /// </summary>
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

            var btnTrangChu = TaoNutMenu("🏠  Trang chủ");
            var btnKhachHang = TaoNutMenu("👤  Khách hàng");
            var btnDonHang = TaoNutMenu("🧾  Đơn hàng / Tích điểm");
            var btnThongKe = TaoNutMenu("📊  Thống kê");
            var btnThoat = TaoNutMenu("🚪  Thoát");

            btnTrangChu.Click += (s, e) => HienThi(new TrangChuView(_data), btnTrangChu);
            btnKhachHang.Click += (s, e) => HienThi(new KhachHangView(_data), btnKhachHang);
            btnDonHang.Click += (s, e) => HienThi(new DonHangView(_data), btnDonHang);
            btnThongKe.Click += (s, e) => HienThi(new ThongKeView(_data), btnThongKe);
            btnThoat.Click += (s, e) => Close();

            stackSidebar.Children.Add(lblTieuDe);
            stackSidebar.Children.Add(btnTrangChu);
            stackSidebar.Children.Add(btnKhachHang);
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

            HienThi(new TrangChuView(_data), btnTrangChu);
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
                Background = new SolidColorBrush(Color.Parse("#1F2937")),
                Foreground = new SolidColorBrush(Color.Parse("#D1D5DB")),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                FontSize = 14,
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
