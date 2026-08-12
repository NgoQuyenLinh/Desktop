using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using QuanLyKhachHang.Models;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Views
{
    public class TrangChuView : UserControl
    {
        private readonly DataService _data;
        // Callback: param 1 là tabIndex (1: Khách hàng, 2: Đơn hàng), param 2 là maKH (nếu có)
        private readonly Action<int, string?>? _moTabAction;

        private readonly ComboBox _cboThoiGian = new() { Width = 130 };
        private readonly TextBlock _txtTongKhach = new();
        private readonly TextBlock _txtTongDon = new();
        private readonly TextBlock _txtTongDoanhThu = new();
        private readonly TextBlock _txtTongDiem = new();

        private readonly TextBox _txtTimSdt = new() { Width = 200, Watermark = "Nhập 3 số đuôi SĐT..." };
        private readonly TextBlock _lblThongBao = new() { FontSize = 12, IsVisible = false };

        private readonly Button _btnThemKhach = new() 
        { 
            Content = "➕ Thêm khách hàng", 
            Background = new SolidColorBrush(Color.Parse("#2563EB")), 
            Foreground = Brushes.White,
            IsVisible = false 
        };

        private readonly Button _btnTaoHoaDon = new()
        {
            Content = "🧾 Tạo hoá đơn",
            Background = new SolidColorBrush(Color.Parse("#16A34A")),
            Foreground = Brushes.White,
            IsVisible = false
        };

        private KhachHang? _khachHangTimThay;

        public TrangChuView(DataService data, Action<int, string?>? moTabAction = null)
        {
            _data = data;
            _moTabAction = moTabAction;

            var goc = new StackPanel { Spacing = 16, Margin = new Thickness(10) };

            // ================= 1. BỘ LỌC VÀ THẺ THỐNG KÊ =================
            var hangTieuDe = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 15 };
            hangTieuDe.Children.Add(new TextBlock
            {
                Text = "Tổng quan hệ thống",
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                VerticalAlignment = VerticalAlignment.Center
            });

            _cboThoiGian.ItemsSource = new[] { "Tất cả", "Hôm nay", "Tuần này", "Tháng này", "Năm nay" };
            _cboThoiGian.SelectedIndex = 0;
            _cboThoiGian.SelectionChanged += (s, e) => CapNhatThongKe();
            hangTieuDe.Children.Add(_cboThoiGian);
            goc.Children.Add(hangTieuDe);

            var hangThe = new WrapPanel { Orientation = Orientation.Horizontal };
            hangThe.Children.Add(TaoTheNho("👤 Tổng khách hàng", _txtTongKhach, "#2563EB"));
            hangThe.Children.Add(TaoTheNho("🧾 Tổng đơn hàng", _txtTongDon, "#10B981"));
            hangThe.Children.Add(TaoTheNho("💰 Tổng doanh thu", _txtTongDoanhThu, "#EA580C"));
            hangThe.Children.Add(TaoTheNho("⭐ Tổng điểm hiện có", _txtTongDiem, "#9333EA"));
            goc.Children.Add(hangThe);

            // ================= 2. KHUNG TẠO HÓA ĐƠN NHANH / TÌM SĐT =================
            var khungNhanh = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 10, 0, 0),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };

            var panelNhanh = new StackPanel { Spacing = 10 };
            panelNhanh.Children.Add(new TextBlock { Text = "⚡ Tạo hoá đơn nhanh", FontWeight = FontWeight.Bold, FontSize = 15 });

            var hangNhap = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, VerticalAlignment = VerticalAlignment.Center };
            hangNhap.Children.Add(new TextBlock { Text = "Số đuôi SĐT:", VerticalAlignment = VerticalAlignment.Center });
            hangNhap.Children.Add(_txtTimSdt);
            hangNhap.Children.Add(_btnThemKhach);
            hangNhap.Children.Add(_btnTaoHoaDon);

            _txtTimSdt.TextChanged += TxtTimSdt_TextChanged;
            _btnThemKhach.Click += BtnThemKhach_Click;
            _btnTaoHoaDon.Click += BtnTaoHoaDon_Click;

            panelNhanh.Children.Add(hangNhap);
            panelNhanh.Children.Add(_lblThongBao);

            khungNhanh.Child = panelNhanh;
            goc.Children.Add(khungNhanh);

            Content = goc;
            CapNhatThongKe();
        }

        private void CapNhatThongKe()
        {
            DateTime ngayHienTai = DateTime.Now;
            var donHangLoc = _data.DanhSachDonHang.AsEnumerable();

            switch (_cboThoiGian.SelectedIndex)
            {
                case 1:
                    donHangLoc = donHangLoc.Where(d => d.NgayTao.Date == ngayHienTai.Date);
                    break;
                case 2:
                    var dauTuan = ngayHienTai.Date.AddDays(-(int)ngayHienTai.DayOfWeek + (int)DayOfWeek.Monday);
                    donHangLoc = donHangLoc.Where(d => d.NgayTao.Date >= dauTuan);
                    break;
                case 3:
                    donHangLoc = donHangLoc.Where(d => d.NgayTao.Month == ngayHienTai.Month && d.NgayTao.Year == ngayHienTai.Year);
                    break;
                case 4:
                    donHangLoc = donHangLoc.Where(d => d.NgayTao.Year == ngayHienTai.Year);
                    break;
            }

            var danhSachDon = donHangLoc.ToList();
            _txtTongKhach.Text = _data.DanhSachKhachHang.Count.ToString();
            _txtTongDon.Text = danhSachDon.Count.ToString();
            _txtTongDoanhThu.Text = $"{danhSachDon.Sum(d => d.ThanhTien):N0} đ";
            _txtTongDiem.Text = _data.DanhSachKhachHang.Sum(kh => kh.DiemTichLuy).ToString();
        }

        private void TxtTimSdt_TextChanged(object? sender, TextChangedEventArgs e)
        {
            string chuoiTim = _txtTimSdt.Text?.Trim() ?? string.Empty;

            if (chuoiTim.Length >= 3)
            {
                _khachHangTimThay = _data.DanhSachKhachHang.FirstOrDefault(k => k.SoDienThoai.EndsWith(chuoiTim));
                if (_khachHangTimThay == null)
                {
                    _lblThongBao.Foreground = Brushes.Red;
                    _lblThongBao.Text = "⚠️ Chưa có thông tin khách hàng, hãy đăng ký";
                    _lblThongBao.IsVisible = true;
                    _btnThemKhach.IsVisible = true;
                    _btnTaoHoaDon.IsVisible = false;
                }
                else
                {
                    _lblThongBao.Foreground = Brushes.Green;
                    _lblThongBao.Text = $"✓ Tìm thấy: {_khachHangTimThay.HoTen} - {_khachHangTimThay.SoDienThoai}";
                    _lblThongBao.IsVisible = true;
                    _btnThemKhach.IsVisible = false;
                    _btnTaoHoaDon.IsVisible = true;
                }
            }
            else
            {
                _khachHangTimThay = null;
                _lblThongBao.IsVisible = false;
                _btnThemKhach.IsVisible = false;
                _btnTaoHoaDon.IsVisible = false;
            }
        }

        private async void BtnThemKhach_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var cuaSo = new KhachHangEditWindow(_data.TaoMaKhachHangMoi());
            if (TopLevel.GetTopLevel(this) is Window parentWindow)
            {
                await cuaSo.ShowDialog(parentWindow);
            }
            else
            {
                cuaSo.Show();
            }

            if (cuaSo.KetQua != null)
            {
                _data.ThemKhachHang(cuaSo.KetQua);
                CapNhatThongKe();
                _moTabAction?.Invoke(1, null); // Chuyển sang Tab Khách hàng (Index 1)
            }
        }

        private void BtnTaoHoaDon_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_khachHangTimThay != null)
            {
                // Chuyển sang Tab Đơn hàng (Index 2) và truyền MaKH
                _moTabAction?.Invoke(2, _khachHangTimThay.MaKH); 
            }
        }

        private Border TaoTheNho(string tieuDe, TextBlock txtGiaTri, string maMau)
        {
            var mau = new SolidColorBrush(Color.Parse(maMau));
            txtGiaTri.FontSize = 18;
            txtGiaTri.FontWeight = FontWeight.Bold;
            txtGiaTri.Foreground = mau;

            var noiDung = new StackPanel { Margin = new Thickness(12, 8, 12, 8), Spacing = 6 };
            noiDung.Children.Add(new TextBlock { Text = tieuDe, FontSize = 12, Foreground = Brushes.DimGray });
            noiDung.Children.Add(txtGiaTri);

            return new Border
            {
                Width = 190,
                Height = 75,
                Background = Brushes.White,
                BorderBrush = mau,
                BorderThickness = new Thickness(3, 0, 0, 0),
                Margin = new Thickness(0, 0, 10, 10),
                Child = noiDung
            };
        }
    }
}