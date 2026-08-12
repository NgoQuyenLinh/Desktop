using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using QuanLyKhachHang.Helpers;
using QuanLyKhachHang.Models;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Views
{
    /// <summary>
    /// Màn hình Thống kê: chọn khoảng thời gian (từ ngày -> đến ngày), hiển thị các chỉ số
    /// tổng hợp và bảng Top khách hàng có điểm tích luỹ cao nhất (toàn bộ dữ liệu hiện có).
    /// </summary>
    public class ThongKeView : UserControl
    {
        private readonly DataService _data;
        private readonly DatePicker _dpTu = new();
        private readonly DatePicker _dpDen = new();

        private readonly TextBlock _lblSoDon = new();
        private readonly TextBlock _lblDoanhThu = new();
        private readonly TextBlock _lblDiemCong = new();
        private readonly TextBlock _lblDiemDung = new();

        private readonly ListBox _listBoxTop = new();

        private record KhachHangXepHang(int Hang, KhachHang KhachHang);

        public ThongKeView(DataService data)
        {
            _data = data;

            var goc = new StackPanel { Spacing = 14 };
            goc.Children.Add(new TextBlock { Text = "Thống kê", FontSize = 20, FontWeight = FontWeight.Bold });

            _dpTu.SelectedDate = _data.DanhSachDonHang.Count > 0
                ? _data.DanhSachDonHang.Min(d => d.NgayTao)
                : DateTime.Now.AddMonths(-1);
            _dpDen.SelectedDate = DateTime.Now;

            var hangLoc = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, VerticalAlignment = VerticalAlignment.Center };
            hangLoc.Children.Add(new TextBlock { Text = "Từ ngày:", VerticalAlignment = VerticalAlignment.Center });
            hangLoc.Children.Add(_dpTu);
            hangLoc.Children.Add(new TextBlock { Text = "Đến ngày:", VerticalAlignment = VerticalAlignment.Center });
            hangLoc.Children.Add(_dpDen);

            var btnLoc = new Button { Content = "🔎 Xem thống kê", Background = new SolidColorBrush(Color.Parse("#2563EB")), Foreground = Brushes.White };
            btnLoc.Click += (s, e) => ThongKe();
            hangLoc.Children.Add(btnLoc);
            goc.Children.Add(hangLoc);

            var hangThe = new WrapPanel { Orientation = Orientation.Horizontal };
            hangThe.Children.Add(TaoThe("🧾 Số đơn hàng", _lblSoDon, "#2563EB"));
            hangThe.Children.Add(TaoThe("💰 Doanh thu", _lblDoanhThu, "#EA580C"));
            hangThe.Children.Add(TaoThe("⭐ Điểm đã cộng", _lblDiemCong, "#10B981"));
            hangThe.Children.Add(TaoThe("🎁 Điểm đã dùng", _lblDiemDung, "#9333EA"));
            goc.Children.Add(hangThe);

            goc.Children.Add(new TextBlock { Text = "🏆 Top khách hàng có điểm tích luỹ cao nhất", FontSize = 15, FontWeight = FontWeight.Bold });

            var khungBang = new Border { Background = Brushes.White, Height = 300 };
            khungBang.Child = UiHelpers.TaoBang<KhachHangXepHang>(
                new List<KhachHangXepHang>(),
                new List<ColDef<KhachHangXepHang>>
                {
                    new("Hạng", 0.5, x => x.Hang.ToString()),
                    new("Mã KH", 0.8, x => x.KhachHang.MaKH),
                    new("Họ tên", 1.6, x => x.KhachHang.HoTen),
                    new("Số điện thoại", 1.3, x => x.KhachHang.SoDienThoai),
                    new("Điểm tích luỹ", 1, x => x.KhachHang.DiemTichLuy.ToString())
                },
                _listBoxTop);
            goc.Children.Add(khungBang);

            Content = goc;
            ThongKe();
        }

        private Border TaoThe(string tieuDe, TextBlock lblGiaTri, string maMau)
        {
            var mau = new SolidColorBrush(Color.Parse(maMau));
            var noiDung = new StackPanel { Margin = new Thickness(14, 10, 14, 10), Spacing = 8 };
            noiDung.Children.Add(new TextBlock { Text = tieuDe, FontSize = 13, Foreground = Brushes.DimGray });

            lblGiaTri.FontSize = 18;
            lblGiaTri.FontWeight = FontWeight.Bold;
            lblGiaTri.Foreground = mau;
            noiDung.Children.Add(lblGiaTri);

            return new Border
            {
                Width = 225,
                Height = 82,
                Background = Brushes.White,
                BorderBrush = mau,
                BorderThickness = new Thickness(4, 0, 0, 0),
                Margin = new Thickness(0, 0, 14, 14),
                Child = noiDung
            };
        }

        private async void ThongKe()
        {
            DateTime tuNgay = _dpTu.SelectedDate?.DateTime ?? DateTime.Now.AddMonths(-1);
            DateTime denNgay = _dpDen.SelectedDate?.DateTime ?? DateTime.Now;

            if (tuNgay > denNgay)
            {
                await ThongBaoWindow.ThongBao(TopLevel.GetTopLevel(this) as Window,
                    "Khoảng thời gian không hợp lệ", "'Từ ngày' phải nhỏ hơn hoặc bằng 'Đến ngày'.");
                return;
            }

            _lblSoDon.Text = _data.SoLuongDonHang(tuNgay, denNgay).ToString();
            _lblDoanhThu.Text = $"{_data.TongDoanhThu(tuNgay, denNgay):N0} đ";
            _lblDiemCong.Text = _data.TongDiemDaTichLuy(tuNgay, denNgay).ToString();
            _lblDiemDung.Text = _data.TongDiemDaSuDung(tuNgay, denNgay).ToString();

            _listBoxTop.ItemsSource = _data.TopKhachHangDiemCao(10)
                .Select((kh, idx) => new KhachHangXepHang(idx + 1, kh))
                .ToList();
        }
    }
}
