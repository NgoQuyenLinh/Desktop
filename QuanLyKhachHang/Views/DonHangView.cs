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
    /// Màn hình Đơn hàng / Tích điểm:
    ///  - Bên trái: form tạo đơn hàng mới (chọn khách hàng, nhập số tiền, tuỳ chọn dùng điểm).
    ///  - Bên phải: danh sách toàn bộ đơn hàng đã tạo có hỗ trợ sắp xếp.
    /// </summary>
    public class DonHangView : UserControl
    {
        private readonly DataService _data;

        private readonly ComboBox _cboKhachHang = new();
        private readonly NumericUpDown _numSoTien = new() { Minimum = 0, Maximum = 1_000_000_000, FormatString = "N0", Increment = 10000 };
        private readonly NumericUpDown _numDiemSuDung = new() { Minimum = 0, Maximum = 1_000_000, FormatString = "0" };
        private readonly TextBlock _lblDiemHienCo = new();
        private readonly TextBlock _lblDiemSeCong = new();
        private readonly TextBlock _lblThongBaoLoiDiem = new() 
        { 
            Text = "Số điểm không hợp lệ", 
            Foreground = Brushes.Red, 
            FontWeight = FontWeight.Bold, 
            FontSize = 13, 
            IsVisible = false 
        };
        private readonly TextBlock _lblThanhTien = new();
        private readonly Button _btnTaoDon = new()
        {
            Content = "✅ Tạo đơn & cộng điểm",
            Background = new SolidColorBrush(Color.Parse("#2563EB")),
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Height = 38
        };

        // UI Sắp xếp bên phải
        private readonly ComboBox _cboSapXep = new() { Width = 170 };
        private readonly ListBox _listBoxDon = new();

        private DonHang? _donDangChon;

        public DonHangView(DataService data)
        {
            _data = data;

            var goc = new StackPanel { Spacing = 12 };
            goc.Children.Add(new TextBlock { Text = "Đơn hàng & Tích điểm", FontSize = 20, FontWeight = FontWeight.Bold });

            var hang = new Grid { ColumnDefinitions = new ColumnDefinitions("330,25,*") };

            // ---- Khung tạo đơn hàng bên trái ----
            var panelTao = new StackPanel { Margin = new Thickness(16), Spacing = 12 };
            panelTao.Children.Add(new TextBlock { Text = "Tạo đơn hàng mới", FontSize = 15, FontWeight = FontWeight.Bold });

            panelTao.Children.Add(TaoDong("Khách hàng:", _cboKhachHang));
            _cboKhachHang.SelectionChanged += (s, e) => CapNhatDiemHienCo();

            panelTao.Children.Add(TaoDong("Số tiền đơn hàng (đ):", _numSoTien));
            _numSoTien.ValueChanged += (s, e) => CapNhatUocTinh();

            _lblDiemSeCong.Foreground = new SolidColorBrush(Color.Parse("#10B981"));
            _lblDiemSeCong.FontSize = 13;
            panelTao.Children.Add(_lblDiemSeCong);

            panelTao.Children.Add(TaoDong("Điểm hiện có:", _lblDiemHienCo));
            _lblDiemHienCo.Foreground = new SolidColorBrush(Color.Parse("#2563EB"));
            _lblDiemHienCo.FontWeight = FontWeight.Bold;

            panelTao.Children.Add(TaoDong("Điểm muốn sử dụng:", _numDiemSuDung));
            _numDiemSuDung.ValueChanged += (s, e) => CapNhatUocTinh();

            // Dòng thông báo lỗi điểm đỏ bôi đậm nằm dưới ô điểm muốn sử dụng và trên thành tiền
            panelTao.Children.Add(_lblThongBaoLoiDiem);

            _lblThanhTien.FontWeight = FontWeight.Bold;
            _lblThanhTien.FontSize = 14;
            panelTao.Children.Add(_lblThanhTien);

            _btnTaoDon.Click += BtnTaoDon_Click;
            panelTao.Children.Add(_btnTaoDon);

            var khungTao = new Border { Background = Brushes.White, BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(1), Child = panelTao };
            Grid.SetColumn(khungTao, 0);

            // ---- Danh sách đơn hàng bên phải ----
            var phaiGoc = new DockPanel();

            var hangTieuDe = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
            
            var panelTraiTieuDe = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, VerticalAlignment = VerticalAlignment.Center };
            panelTraiTieuDe.Children.Add(new TextBlock { Text = "Danh sách đơn hàng", FontSize = 15, FontWeight = FontWeight.Bold, VerticalAlignment = VerticalAlignment.Center });
            
            // Cấu hình bộ chọn Sắp xếp
            _cboSapXep.ItemsSource = new[] { "Mới nhất (Ngày)", "Cũ nhất (Ngày)", "Tên khách hàng (A-Z)", "Số tiền (Cao - Thấp)", "Số điểm đã dùng" };
            _cboSapXep.SelectedIndex = 0;
            _cboSapXep.SelectionChanged += (s, e) => TaiLaiDuLieuDon();
            
            panelTraiTieuDe.Children.Add(new TextBlock { Text = " ↕ Sắp xếp:", VerticalAlignment = VerticalAlignment.Center, FontSize = 13 });
            panelTraiTieuDe.Children.Add(_cboSapXep);

            var btnXoaDon = new Button { Content = "🗑️ Xoá đơn đã chọn", Background = new SolidColorBrush(Color.Parse("#DC2626")), Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Right };
            btnXoaDon.Click += BtnXoaDon_Click;
            
            DockPanel.SetDock(btnXoaDon, Dock.Right);
            hangTieuDe.Children.Add(btnXoaDon);
            hangTieuDe.Children.Add(panelTraiTieuDe);
            DockPanel.SetDock(hangTieuDe, Dock.Top);
            phaiGoc.Children.Add(hangTieuDe);

            _listBoxDon.SelectionChanged += (s, e) => _donDangChon = _listBoxDon.SelectedItem as DonHang;
            var khungBang = new Border { Background = Brushes.White, Height = 480 };
            khungBang.Child = UiHelpers.TaoBang<DonHang>(
                new List<DonHang>(),
                new List<ColDef<DonHang>>
                {
                    new("Mã đơn", 0.8, d => d.MaDon),
                    new("Khách hàng", 1.6, d => d.TenKH),
                    new("Số tiền", 1.1, d => d.SoTien.ToString("N0")),
                    new("Điểm cộng", 0.9, d => d.DiemCong.ToString()),
                    new("Điểm dùng", 0.9, d => d.DiemSuDung.ToString()),
                    new("Thành tiền", 1.1, d => d.ThanhTien.ToString("N0")),
                    new("Ngày tạo", 1.4, d => d.NgayTao.ToString("dd/MM/yyyy HH:mm"))
                },
                _listBoxDon);
            phaiGoc.Children.Add(khungBang);

            Grid.SetColumn(phaiGoc, 2);

            hang.Children.Add(khungTao);
            hang.Children.Add(phaiGoc);
            goc.Children.Add(hang);

            Content = goc;

            NapDanhSachKhachHang();
            TaiLaiDuLieuDon();
        }

        private StackPanel TaoDong(string nhan, Control input)
        {
            input.Width = double.NaN;
            input.HorizontalAlignment = HorizontalAlignment.Stretch;
            var dong = new StackPanel { Spacing = 5 };
            dong.Children.Add(new TextBlock { Text = nhan, FontSize = 13 });
            dong.Children.Add(input);
            return dong;
        }

        private void NapDanhSachKhachHang()
        {
            _cboKhachHang.ItemsSource = _data.DanhSachKhachHang
                .Select(kh => $"{kh.MaKH} - {kh.HoTen}")
                .ToList();
            if (_cboKhachHang.ItemsSource is List<string> ds && ds.Count > 0)
                _cboKhachHang.SelectedIndex = 0;

            CapNhatDiemHienCo();
        }

        private KhachHang? KhachHangDangChon()
        {
            if (_cboKhachHang.SelectedIndex < 0 || _cboKhachHang.SelectedIndex >= _data.DanhSachKhachHang.Count)
                return null;
            return _data.DanhSachKhachHang[_cboKhachHang.SelectedIndex];
        }

        private void CapNhatDiemHienCo()
        {
            var kh = KhachHangDangChon();
            _lblDiemHienCo.Text = kh != null ? $"{kh.DiemTichLuy} điểm" : "-";
            CapNhatUocTinh();
        }

        private void CapNhatUocTinh()
        {
            decimal soTien = _numSoTien.Value ?? 0;
            int diemCong = (int)(soTien / 1000);
            _lblDiemSeCong.Text = $"→ Điểm sẽ được cộng: {diemCong} điểm (Số tiền / 1000)";

            var kh = KhachHangDangChon();
            int diemHienCo = kh?.DiemTichLuy ?? 0;
            decimal diemSuDung = _numDiemSuDung.Value ?? 0;

            // Ràng buộc kiểm tra tính hợp lệ của điểm khi gõ phím
            bool laDiemAm = diemSuDung < 0;
            bool vuotDiemHienCo = diemSuDung > diemHienCo;
            bool vuotSoTienChoPhep = diemSuDung > diemCong; // 1 điểm = 1000đ

            if (laDiemAm || vuotDiemHienCo || vuotSoTienChoPhep)
            {
                _lblThongBaoLoiDiem.IsVisible = true;
                _btnTaoDon.IsEnabled = false; // Khoá nút tạo đơn khi dữ liệu sai
            }
            else
            {
                _lblThongBaoLoiDiem.IsVisible = false;
                _btnTaoDon.IsEnabled = true;
            }

            decimal thanhTien = soTien - (diemSuDung * 1000);
            if (thanhTien < 0) thanhTien = 0;
            _lblThanhTien.Text = $"Thành tiền phải trả: {thanhTien:N0} đ";
        }

        private async void BtnTaoDon_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var cuaSoCha = TopLevel.GetTopLevel(this) as Window;
            var kh = KhachHangDangChon();
            if (kh == null)
            {
                await ThongBaoWindow.ThongBao(cuaSoCha, "Thiếu thông tin", "Vui lòng chọn khách hàng.");
                return;
            }

            var (thanhCong, thongBao, _) = _data.TaoDonHang(kh.MaKH, _numSoTien.Value ?? 0, (int)(_numDiemSuDung.Value ?? 0));

            if (!thanhCong)
            {
                await ThongBaoWindow.ThongBao(cuaSoCha, "Không thể tạo đơn hàng", thongBao);
                return;
            }

            await ThongBaoWindow.ThongBao(cuaSoCha, "Thành công", thongBao);

            _numSoTien.Value = 0;
            _numDiemSuDung.Value = 0;
            NapDanhSachKhachHang();
            TaiLaiDuLieuDon();
        }

        private void TaiLaiDuLieuDon()
        {
            IEnumerable<DonHang> danhSach = _data.DanhSachDonHang;

            // Xử lý sắp xếp tự động dựa theo lựa chọn trong ComboBox
            switch (_cboSapXep.SelectedIndex)
            {
                case 0: // Mới nhất (Ngày)
                    danhSach = danhSach.OrderByDescending(d => d.NgayTao);
                    break;
                case 1: // Cũ nhất (Ngày)
                    danhSach = danhSach.OrderBy(d => d.NgayTao);
                    break;
                case 2: // Tên khách hàng (A-Z)
                    danhSach = danhSach.OrderBy(d => d.TenKH);
                    break;
                case 3: // Số tiền (Cao - Thấp)
                    danhSach = danhSach.OrderByDescending(d => d.SoTien);
                    break;
                case 4: // Số điểm đã dùng
                    danhSach = danhSach.OrderByDescending(d => d.DiemSuDung);
                    break;
                default:
                    danhSach = danhSach.OrderByDescending(d => d.NgayTao);
                    break;
            }

            _listBoxDon.ItemsSource = danhSach.ToList();
        }

        private async void BtnXoaDon_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var cuaSoCha = TopLevel.GetTopLevel(this) as Window;

            if (_donDangChon == null)
            {
                await ThongBaoWindow.ThongBao(cuaSoCha, "Thông báo", "Vui lòng chọn 1 đơn hàng cần xoá.");
                return;
            }

            bool dongY = await ThongBaoWindow.XacNhan(cuaSoCha, "Xác nhận xoá",
                "Xoá đơn hàng này sẽ KHÔNG tự động hoàn/trừ lại điểm cho khách hàng.\nBạn có chắc muốn xoá?");

            if (dongY)
            {
                _data.XoaDonHang(_donDangChon.MaDon);
                TaiLaiDuLieuDon();
            }
        }

        public void ChonKhachHang(string maKH)
        {
            for (int i = 0; i < _data.DanhSachKhachHang.Count; i++)
            {
                if (_data.DanhSachKhachHang[i].MaKH == maKH)
                {
                    _cboKhachHang.SelectedIndex = i;
                    break;
                }
            }
        }
    }
}