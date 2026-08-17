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
    /// tổng hợp và bảng Top khách hàng có điểm tích luỹ cao nhất.
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

        private record KhachHangXepHang(int Hang, string MaKH, string HoTen, string SoDienThoai, decimal TongTien);

        public ThongKeView(DataService data)
        {
            _data = data;

            var goc = new StackPanel { Spacing = 14 };
            goc.Children.Add(new TextBlock { Text = "Thống kê", FontSize = 20, FontWeight = FontWeight.Bold });

            // Thiết lập giá trị mặc định dạng DateTimeOffset
            DateTimeOffset tuNgayMacDinh = _data.DanhSachDonHang.Count > 0
                ? new DateTimeOffset(_data.DanhSachDonHang.Min(d => d.NgayTao))
                : new DateTimeOffset(DateTime.Now.AddMonths(-1));

            _dpTu.SelectedDate = tuNgayMacDinh;
            _dpDen.SelectedDate = DateTimeOffset.Now;

            var hangLoc = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, VerticalAlignment = VerticalAlignment.Center };
            hangLoc.Children.Add(new TextBlock { Text = "Từ ngày:", VerticalAlignment = VerticalAlignment.Center });
            hangLoc.Children.Add(_dpTu);
            hangLoc.Children.Add(new TextBlock { Text = "Đến ngày:", VerticalAlignment = VerticalAlignment.Center });
            hangLoc.Children.Add(_dpDen);

            _dpTu.SelectedDateChanged += (s, e) => ThongKe();
            _dpDen.SelectedDateChanged += (s, e) => ThongKe();

            goc.Children.Add(hangLoc);

            var hangThe = new WrapPanel { Orientation = Orientation.Horizontal };
            hangThe.Children.Add(TaoThe("🧾 Số đơn hàng", _lblSoDon, "#2563EB"));
            hangThe.Children.Add(TaoThe("💰 Doanh thu", _lblDoanhThu, "#EA580C"));
            hangThe.Children.Add(TaoThe("⭐ Điểm đã cộng", _lblDiemCong, "#10B981"));
            hangThe.Children.Add(TaoThe("🎁 Điểm đã dùng", _lblDiemDung, "#9333EA"));
            goc.Children.Add(hangThe);

            goc.Children.Add(new TextBlock { Text = "🏆 Top khách hàng tiêu tiền nhiều nhất", FontSize = 15, FontWeight = FontWeight.Bold });

            var khungBang = new Border { Background = Brushes.White, Height = 300 };
            khungBang.Child = UiHelpers.TaoBang<KhachHangXepHang>(
                new List<KhachHangXepHang>(),
                new List<ColDef<KhachHangXepHang>>
                {
                    new("Hạng", 0.5, x => x.Hang.ToString()),
                    new("Mã KH", 0.8, x => x.MaKH),
                    new("Họ tên", 1.6, x => x.HoTen),
                    new("Số điện thoại", 1.3, x => x.SoDienThoai),
                    new("Số tiền", 1, x => x.TongTien.ToString("N0"))
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
            // Chuyển đổi chính xác từ SelectedDate (DateTimeOffset?) sang DateTime
            DateTime tuNgay = _dpTu.SelectedDate?.Date ?? DateTime.Now.AddMonths(-1).Date;
            
            // Đặt mốc Đến ngày về cuối ngày (23:59:59) để lấy trọn vẹn hóa đơn phát sinh trong ngày đó
            DateTime denNgay = _dpDen.SelectedDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.Now;

            if (tuNgay > denNgay)
            {
                await ThongBaoWindow.ThongBao(TopLevel.GetTopLevel(this) as Window,
                    "Khoảng thời gian không hợp lệ", "'Từ ngày' phải nhỏ hơn hoặc bằng 'Đến ngày'.");
                return;
            }

            var donHangs = _data.LocDonHangTheoNgay(tuNgay, denNgay);
            
            _lblSoDon.Text = donHangs.Count.ToString();
            _lblDoanhThu.Text = $"{donHangs.Sum(d => d.ThanhTien):N0} đ";
            _lblDiemCong.Text = donHangs.Sum(d => d.DiemCong).ToString();
            _lblDiemDung.Text = donHangs.Sum(d => d.DiemSuDung).ToString();

            var topKh = donHangs
                .GroupBy(d => d.MaKH)
                .Select(g => new
                {
                    MaKH = g.Key,
                    TongTien = g.Sum(d => d.ThanhTien),
                    TenKH = g.First().TenKH
                })
                .OrderByDescending(x => x.TongTien)
                .Take(10)
                .ToList();

            var ketQuaXepHang = new List<KhachHangXepHang>();
            for (int i = 0; i < topKh.Count; i++)
            {
                var x = topKh[i];
                var kh = _data.DanhSachKhachHang.FirstOrDefault(k => k.MaKH == x.MaKH);
                ketQuaXepHang.Add(new KhachHangXepHang(
                    i + 1, 
                    x.MaKH, 
                    kh?.HoTen ?? x.TenKH, 
                    kh?.SoDienThoai ?? "", 
                    x.TongTien));
            }

            _listBoxTop.ItemsSource = null;
            _listBoxTop.ItemsSource = ketQuaXepHang;
        }
    }
}