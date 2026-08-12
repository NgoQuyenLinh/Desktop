using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Views
{
    /// <summary>
    /// Màn hình Trang chủ: hiển thị nhanh vài con số tổng quan
    /// (tổng số khách hàng, tổng số đơn hàng, tổng doanh thu, tổng điểm hiện có).
    /// </summary>
    public class TrangChuView : UserControl
    {
        public TrangChuView(DataService data)
        {
            var goc = new StackPanel { Spacing = 20 };

            goc.Children.Add(new TextBlock
            {
                Text = "Tổng quan hệ thống",
                FontSize = 22,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#1F2937"))
            });

            int tongKhachHang = data.DanhSachKhachHang.Count;
            int tongDonHang = data.DanhSachDonHang.Count;
            decimal tongDoanhThu = data.DanhSachDonHang.Sum(d => d.ThanhTien);
            int tongDiemHienCo = data.DanhSachKhachHang.Sum(kh => kh.DiemTichLuy);

            var hangThe = new WrapPanel { Orientation = Orientation.Horizontal };
            hangThe.Children.Add(TaoThe("👤 Tổng khách hàng", tongKhachHang.ToString(), "#2563EB"));
            hangThe.Children.Add(TaoThe("🧾 Tổng đơn hàng", tongDonHang.ToString(), "#10B981"));
            hangThe.Children.Add(TaoThe("💰 Tổng doanh thu", $"{tongDoanhThu:N0} đ", "#EA580C"));
            hangThe.Children.Add(TaoThe("⭐ Tổng điểm hiện có", tongDiemHienCo.ToString(), "#9333EA"));
            goc.Children.Add(hangThe);

            goc.Children.Add(new TextBlock
            {
                Text = "Chọn mục trong menu bên trái để quản lý Khách hàng, tạo Đơn hàng / Tích điểm, hoặc xem Thống kê chi tiết.",
                Foreground = Brushes.Gray,
                FontStyle = FontStyle.Italic,
                FontSize = 13
            });

            Content = goc;
        }

        private Border TaoThe(string tieuDe, string giaTri, string maMau)
        {
            var mau = new SolidColorBrush(Color.Parse(maMau));

            var noiDung = new StackPanel { Margin = new Thickness(16, 12, 16, 12), Spacing = 10 };
            noiDung.Children.Add(new TextBlock { Text = tieuDe, FontSize = 13, Foreground = Brushes.DimGray });
            noiDung.Children.Add(new TextBlock { Text = giaTri, FontSize = 22, FontWeight = FontWeight.Bold, Foreground = mau });

            return new Border
            {
                Width = 240,
                Height = 100,
                Background = Brushes.White,
                BorderBrush = mau,
                BorderThickness = new Thickness(4, 0, 0, 0),
                Margin = new Thickness(0, 0, 14, 14),
                Child = noiDung
            };
        }
    }
}
