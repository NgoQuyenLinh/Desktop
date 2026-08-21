using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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
            Title = "PHẦN MỀM QUẢN LÍ NHÀ THUỐC";
            Width = 1200;
            Height = 720;
            MinWidth = 1000;
            MinHeight = 640;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var grid = new Grid();
            // Thu nhỏ chiều rộng sidebar từ 220 xuống 70 pixel cho giống mẫu
            grid.ColumnDefinitions.Add(new ColumnDefinition(70, GridUnitType.Pixel));
            grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            // ---- Sidebar ----
            var stackSidebar = new StackPanel
            {
                Spacing = 8,
                Margin = new Thickness(0, 15, 0, 0)
            };

            // 1. Logo / Avatar tròn ở trên cùng giống ảnh mẫu
            var borderLogo = new Border
            {
                Width = 44,
                Height = 44,
                CornerRadius = new CornerRadius(22),
                Background = new SolidColorBrush(Color.Parse("#3B82F6")),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10),
                Child = new Image
                {
                    Source = TaoBitmap("docs/imagess/Home-page.png"),
                    Width = 22,
                    Height = 22,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            // 2. Tạo các nút chỉ chứa icon căn giữa (không có chữ)
            var btnTrangChu = TaoNutMenuIconOnly("docs/imagess/app.png");
            var btnKhachHang = TaoNutMenuIconOnly("docs/imagess/khachHang.png");
            var btnKhoQua = TaoNutMenuIconOnly("docs/imagess/khoQua.png");
            var btnDonHang = TaoNutMenuIconOnly("docs/imagess/hoaDon.png");
            var btnThongKe = TaoNutMenuIconOnly("docs/imagess/thongKe.png");
            var btnThoat = TaoNutMenuIconOnly("docs/imagess/thoat.png");

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
            btnDonHang.Click += (s, e) => HienThi(new DonHangView(_data), btnDonHang);
            btnThongKe.Click += (s, e) => HienThi(new ThongKeView(_data), btnThongKe);
            btnThoat.Click += (s, e) => Close();

            stackSidebar.Children.Add(borderLogo);
            stackSidebar.Children.Add(btnTrangChu);
            stackSidebar.Children.Add(btnKhachHang);
            stackSidebar.Children.Add(btnKhoQua);
            stackSidebar.Children.Add(btnDonHang);
            stackSidebar.Children.Add(btnThongKe);
            stackSidebar.Children.Add(btnThoat);

            var khungSidebar = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#11182700")),
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

        // Hàm đọc ảnh thành Bitmap an toàn
       private Bitmap? TaoBitmap(string relativePath)
        {
            try
            {
                // Đường dẫn trực tiếp từ thư mục gốc của dự án/bin
                string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
                if (System.IO.File.Exists(fullPath))
                {
                    return new Bitmap(fullPath);
                }
                
                // Hoặc thử tìm trực tiếp đường dẫn tương đối
                if (System.IO.File.Exists(relativePath))
                {
                    return new Bitmap(relativePath);
                }
            }
            catch { }
            return null;
        }
        // Hàm tạo nút menu dạng hình vuông bo góc, chỉ chứa icon ở chính giữa giống mẫu
        private Button TaoNutMenuIconOnly(string imagePath)
        {
            var iconImg = new Image
            {
                Source = TaoBitmap(imagePath),
                Width = 20,
                Height = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            return new Button
            {
                Content = iconImg,
                Width = 48,
                Height = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.Parse("#11182700")),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(10), // Bo tròn góc nút bấm đẹp như mẫu
                Margin = new Thickness(0, 4, 0, 4),
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
                _btnDangChon.Background = new SolidColorBrush(Color.Parse("#11182700"));

            // Khi được chọn, nút sẽ chuyển sang màu xanh dương nổi bật giống hình mẫu
            nut.Background = new SolidColorBrush(Color.Parse("#2563EB"));
            _btnDangChon = nut;
        }
    }
}