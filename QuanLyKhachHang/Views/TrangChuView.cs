using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
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

        // ---- Khung "Tạo hoá đơn nhanh": tìm khách hàng theo SĐT kiểu real-time ----
        private readonly TextBox _txtTimSdt = new() { Width = 220, Watermark = "Nhập số điện thoại..." };
        private readonly TextBlock _lblThongBao = new() { FontSize = 12, IsVisible = false };

        private readonly ListBox _lbGoiY = new()
        {
            IsVisible = false,
            MaxHeight = 180,
            Width = 420,
            HorizontalAlignment = HorizontalAlignment.Left
        };

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
            hangNhap.Children.Add(new TextBlock { Text = "Số điện thoại:", VerticalAlignment = VerticalAlignment.Center });
            hangNhap.Children.Add(_txtTimSdt);
            hangNhap.Children.Add(_btnThemKhach);
            hangNhap.Children.Add(_btnTaoHoaDon);

            // Hiển thị mỗi dòng gợi ý dạng "Mã KH - Họ tên - SĐT - Điểm" thay vì ToString() mặc định của KhachHang
            _lbGoiY.ItemTemplate = new FuncDataTemplate<KhachHang>((kh, ns) =>
                new TextBlock
                {
                    Text = kh == null ? string.Empty : $"{kh.MaKH}  •  {kh.HoTen}  •  {kh.SoDienThoai}  ({kh.DiemTichLuy} điểm)",
                    Padding = new Thickness(8, 6, 8, 6)
                });

            _txtTimSdt.TextChanged += TxtTimSdt_TextChanged;
            _lbGoiY.SelectionChanged += LbGoiY_SelectionChanged;
            _lbGoiY.DoubleTapped += LbGoiY_DoubleTapped;
            _btnThemKhach.Click += BtnThemKhach_Click;
            _btnTaoHoaDon.Click += BtnTaoHoaDon_Click;

            panelNhanh.Children.Add(hangNhap);
            panelNhanh.Children.Add(_lblThongBao);
            panelNhanh.Children.Add(_lbGoiY);

            khungNhanh.Child = panelNhanh;
            goc.Children.Add(khungNhanh);

            Content = goc;
            CapNhatThongKe();
        }

        /// <summary>
        /// [COMMAND: XỬ LÝ GIAO DIỆN PHẦN THỐNG KÊ & BỘ LỌC]
        /// Hàm này chạy khi ComboBox thời gian thay đổi hoặc khởi tạo form, 
        /// dùng để lọc dữ liệu và hiển thị lên 4 thẻ tổng quan (Tổng khách, Tổng đơn, Doanh thu, Điểm).
        /// </summary>
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

        /// <summary>
        /// [COMMAND: XỬ LÝ GIAO DIỆN Ô NHẬP SỐ ĐIỆN THOẠI (REAL-TIME SEARCH)]
        /// Hàm này tự động chạy mỗi khi người dùng gõ ký tự vào ô "Số điện thoại" (_txtTimSdt).
        /// Dùng để tìm kiếm khách hàng, hiển thị kết quả gợi ý hoặc bật nút "Thêm khách hàng" nếu chưa có.
        /// </summary>
        private void TxtTimSdt_TextChanged(object? sender, TextChangedEventArgs e)
        {
            string chuoiTim = _txtTimSdt.Text?.Trim() ?? string.Empty;

            // Mỗi lần gõ lại coi như chưa chọn khách hàng nào, tránh tạo nhầm hoá đơn cho lựa chọn cũ
            _khachHangTimThay = null;
            _btnTaoHoaDon.IsVisible = false;

            if (chuoiTim.Length == 0)
            {
                _lbGoiY.IsVisible = false;
                _lbGoiY.ItemsSource = null;
                _lblThongBao.IsVisible = false;
                _btnThemKhach.IsVisible = false;
                return;
            }

            var ketQua = _data.TimKhachHangTheoSoDienThoai(chuoiTim);

            if (ketQua.Count == 0)
            {
                _lbGoiY.IsVisible = false;
                _lbGoiY.ItemsSource = null;

                _lblThongBao.Foreground = Brushes.Red;
                _lblThongBao.Text = "⚠️ Chưa có khách hàng nào khớp số này, hãy đăng ký";
                _lblThongBao.IsVisible = true;
                _btnThemKhach.IsVisible = true;
            }
            else
            {
                _lblThongBao.Foreground = Brushes.Green;
                _lblThongBao.Text = $"✓ Tìm thấy {ketQua.Count} khách hàng khớp \"{chuoiTim}\" — chọn 1 người bên dưới:";
                _lblThongBao.IsVisible = true;
                _btnThemKhach.IsVisible = false;

                _lbGoiY.ItemsSource = ketQua;
                _lbGoiY.IsVisible = true;
            }
        }

        /// <summary>
        /// [COMMAND: XỬ LÝ GIAO DIỆN CHỌN DÒNG TRONG DANH SÁCH GỢI Ý]
        /// Hàm này chạy khi người dùng click chọn 1 khách hàng cụ thể trong khung danh sách gợi ý bên dưới ô SĐT,
        /// giúp kích hoạt hiển thị nút "Tạo hoá đơn".
        /// </summary>
        private void LbGoiY_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            _khachHangTimThay = _lbGoiY.SelectedItem as KhachHang;

            if (_khachHangTimThay != null)
            {
                _lblThongBao.Foreground = Brushes.Green;
                _lblThongBao.Text = $"✓ Đã chọn: {_khachHangTimThay.HoTen} - {_khachHangTimThay.SoDienThoai}";
                _lblThongBao.IsVisible = true;
                _btnTaoHoaDon.IsVisible = true;
            }
            else
            {
                _btnTaoHoaDon.IsVisible = false;
            }
        }

        /// <summary>
        /// [COMMAND: XỬ LÝ GIAO DIỆN NHẤP ĐÚP (DOUBLE-CLICK) VÀO GỢI Ý]
        /// Hàm này chạy khi người dùng nhấp đúp chuột vào một khách hàng trong danh sách gợi ý,
        /// giúp chuyển thẳng qua màn hình/tab tạo hoá đơn cho khách hàng đó.
        /// </summary>
        private void LbGoiY_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (_lbGoiY.SelectedItem is KhachHang kh)
            {
                _khachHangTimThay = kh;
                _moTabAction?.Invoke(2, kh.MaKH);
            }
        }

        /// <summary>
        /// [COMMAND: XỬ LÝ GIAO DIỆN NÚT "THÊM KHÁCH HÀNG"]
        /// Hàm này chạy khi người dùng bấm nút "➕ Thêm khách hàng" (hiện lên lúc không tìm thấy SĐT),
        /// mở cửa sổ popup/dialog để thêm mới khách hàng vào hệ thống.
        /// </summary>
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

        /// <summary>
        /// [COMMAND: XỬ LÝ GIAO DIỆN NÚT "TẠO HOÁ ĐƠN"]
        /// Hàm này chạy khi người dùng bấm nút "🧾 Tạo hoá đơn" sau khi đã chọn khách hàng thành công từ gợi ý,
        /// giúp điều hướng sang Tab Đơn hàng kèm theo mã khách hàng vừa chọn.
        /// </summary>
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