using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using QuanLyKhachHang.Helpers;
using QuanLyKhachHang.Models;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Views
{
    public class DonHangView : UserControl
    {
        private readonly DataService _data;

        private readonly AutoCompleteBox _cboKhachHang = new()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Watermark = "Nhập mã KH, tên hoặc SĐT...",
            FilterMode = AutoCompleteFilterMode.Custom,
            MinimumPrefixLength = 0 // Cho phép hiện Dropdown khi chưa gõ gì
        };
        
        private readonly TextBlock _txtMaKH = new() { Text = "-", FontWeight = FontWeight.SemiBold };
        private readonly TextBlock _txtTenKH = new() { Text = "-", FontWeight = FontWeight.SemiBold };
        private readonly TextBlock _txtSdt = new() { Text = "-", FontWeight = FontWeight.SemiBold };
        private readonly TextBlock _txtDiem = new() { Text = "0 điểm", FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#D97706")) }; // Màu vàng cam dịu
        
        private readonly AutoCompleteBox _cboThuoc = new()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Watermark = "Nhập mã thuốc hoặc tên thuốc...",
            FilterMode = AutoCompleteFilterMode.Custom,
            MinimumPrefixLength = 0 // Cho phép hiện Dropdown khi chưa gõ gì
        };
        
        private readonly NumericUpDown _numSLThuoc = new() { Minimum = 1, Maximum = 10000, Value = 1, FormatString = "0" };
        private readonly TextBlock _txtDonGiaThuoc = new() { Text = "0 đ", FontWeight = FontWeight.SemiBold };
        private readonly ListBox _lbThuoc = new();

// DÒNG MỚI ĐÃ SỬA:
private readonly ComboBox _cboQua = new() { HorizontalAlignment = HorizontalAlignment.Stretch };        private readonly TextBlock _txtDiemQua = new() { Text = "0 điểm", FontWeight = FontWeight.SemiBold };
        private readonly ListBox _lbQuaDaChon = new();

        private readonly List<ChiTietDonHang> _thuocDangChon = new();
        private QuaTang? _quaDangChon;

        private readonly ListBox _lbLichSu = new();
        private readonly TextBlock _txtKhongCoLichSu = new()
        {
            Text = "Khách hàng chưa có lịch sử mua hàng. Chọn thời gian bên dưới để xem giao dịch.",
            Foreground = new SolidColorBrush(Color.Parse("#94A3B8")),
            TextWrapping = TextWrapping.Wrap,
            IsVisible = false,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(12)
        };

        private readonly RadioButton _rbNgay = new() { Content = "Ngày", GroupName = "lichsu", IsChecked = true };
        private readonly RadioButton _rbThang = new() { Content = "Tháng", GroupName = "lichsu" };
        private readonly RadioButton _rbNam = new() { Content = "Năm", GroupName = "lichsu" };

        private readonly TextBlock _txtTongTien = new() { FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#0F172A")) };
        private readonly TextBlock _txtDiemCong = new() { FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#10B981")) }; // Xanh lá ngọc
        private readonly TextBlock _txtTongDiemDoi = new() { FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#EF4444")) }; // Đỏ nhạt
        private readonly TextBlock _txtDiemSau = new() { FontWeight = FontWeight.Bold, FontSize = 18, Foreground = new SolidColorBrush(Color.Parse("#0EA5E9")) }; // Xanh dương ngọc
        private readonly TextBox _txtGhiChu = new() { Watermark = "Nhập ghi chú (nếu có)...", AcceptsReturn = true, MinHeight = 58 };

        private readonly Button _btnTaoDon;

        public DonHangView(DataService data)
        {
            _data = data;
            
            // Màu nền tổng thể dịu nhẹ, sang trọng
            Background = new SolidColorBrush(Color.Parse("#F8FAFC"));

            var content = new StackPanel { Margin = new Thickness(18), Spacing = 16 };

            // ===== HEADER =====
            var title = new StackPanel { Spacing = 4 };
            title.Children.Add(TaoHeaderIcon("docs/imagess/hoaDon.png", "Tạo đơn hàng", 24));
            title.Children.Add(new TextBlock { Text = "Tạo đơn mới, bán thuốc, cộng điểm và đổi quà cho khách hàng", Foreground = new SolidColorBrush(Color.Parse("#64748B")), Margin = new Thickness(36, 0, 0, 0) });
            content.Children.Add(title);

            // ===== THÔNG TIN KHÁCH HÀNG =====
            var customerCard = Card(TaoHeaderIcon("docs/imagess/person.png", "Thông tin khách hàng"));
            var customerGrid = new Grid { ColumnDefinitions = new ColumnDefinitions("2.1*,1*,1.4*,1.3*,1.2*"), Margin = new Thickness(0, 8, 0, 0) };
            
            // Xử lý sự kiện tự động Dropdown khi Click vào TextBox
            _cboKhachHang.GotFocus += (s, e) => { if (!_cboKhachHang.IsDropDownOpen) _cboKhachHang.IsDropDownOpen = true; };
            _cboThuoc.GotFocus += (s, e) => { if (!_cboThuoc.IsDropDownOpen) _cboThuoc.IsDropDownOpen = true; };

            customerGrid.Children.Add(Field("Chọn / tìm khách hàng", _cboKhachHang, 0));
            customerGrid.Children.Add(InfoField("Mã khách hàng", _txtMaKH, 1));
            customerGrid.Children.Add(InfoField("Họ và tên", _txtTenKH, 2));
            customerGrid.Children.Add(InfoField("Số điện thoại", _txtSdt, 3));
            customerGrid.Children.Add(InfoField("Điểm hiện có", _txtDiem, 4));
            ((StackPanel)customerCard.Child!).Children.Add(customerGrid);
            content.Children.Add(customerCard);

            var middle = new Grid { ColumnDefinitions = new ColumnDefinitions("1.25*,0.85*") };

            // ===== CHỌN THUỐC =====
            var medicineCard = Card(TaoHeaderIcon("docs/imagess/medicin.png", "1. Chọn thuốc đã mua"));
            var medStack = new StackPanel { Spacing = 12 };
            var medInput = new Grid { ColumnDefinitions = new ColumnDefinitions("2*,0.8*,0.9*,1.2*") };
            medInput.Children.Add(Field("Tên thuốc", _cboThuoc, 0));
            medInput.Children.Add(Field("Số lượng", _numSLThuoc, 1));
            medInput.Children.Add(InfoField("Đơn giá", _txtDonGiaThuoc, 2));
            
            var btnThemThuoc = TaoButtonIcon("Thêm vào danh sách", "docs/imagess/thêm.png", "#0EA5E9", "#FFFFFF");
            btnThemThuoc.VerticalAlignment = VerticalAlignment.Bottom;
            btnThemThuoc.Height = 36;
            btnThemThuoc.Click += BtnThemThuoc_Click;
            Grid.SetColumn(btnThemThuoc, 3);
            medInput.Children.Add(btnThemThuoc);
            medStack.Children.Add(medInput);

            var medTable = new List<ColDef<ChiTietDonHang>>
            {
                new("STT", .45, x => (_thuocDangChon.IndexOf(x) + 1).ToString()),
                new("Tên thuốc", 1.8, x => x.TenThuoc),
                new("Đơn giá", .9, x => $"{x.DonGia:N0} đ"),
                new("Số lượng", .8, x => x.SoLuong.ToString()),
                new("Thành tiền", 1, x => $"{x.ThanhTien:N0} đ"),
                new("Thao tác", .65, x => "Chọn để xoá")
            };
            var tableBorder = new Border { Height = 205, BorderBrush = new SolidColorBrush(Color.Parse("#E2E8F0")), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Child = UiHelpers.TaoBang(new List<ChiTietDonHang>(), medTable, _lbThuoc) };
            medStack.Children.Add(tableBorder);
            
            var btnXoaThuoc = TaoButtonIcon("Xóa thuốc đang chọn", "docs/imagess/trash.png", "#FFFFFF", "#EF4444", true);
            btnXoaThuoc.HorizontalAlignment = HorizontalAlignment.Left;
            btnXoaThuoc.Click += (_, _) => { if (_lbThuoc.SelectedItem is ChiTietDonHang ct) { _thuocDangChon.Remove(ct); CapNhatThuoc(); } };
            medStack.Children.Add(btnXoaThuoc);
            ((StackPanel)medicineCard.Child!).Children.Add(medStack);
            
            Grid.SetColumn(medicineCard, 0);
            middle.Children.Add(medicineCard);

            // ===== CHỌN QUÀ =====
            var giftCard = Card(TaoHeaderIcon("docs/imagess/gift.png", "2. Chọn quà muốn đổi (tùy chọn)"));
            var giftStack = new StackPanel { Spacing = 12 };
            var giftInput = new Grid { ColumnDefinitions = new ColumnDefinitions("2*,1*")};
            giftInput.Children.Add(Field("Tên quà", _cboQua, 0));
            giftInput.Children.Add(InfoField("Điểm cần đổi", _txtDiemQua, 1));
            giftStack.Children.Add(giftInput);
            
            var btnThemQua = TaoButtonIcon("Chọn quà", "docs/imagess/thêm.png", "#10B981", "#FFFFFF");
            btnThemQua.HorizontalAlignment = HorizontalAlignment.Left;
            btnThemQua.Click += BtnThemQua_Click;
            giftStack.Children.Add(btnThemQua);

            var giftTable = new List<ColDef<QuaTang>>
            {
                new("STT", .5, _ => "1"),
                new("Tên quà", 1.7, q => q.TenQua),
                new("Điểm đổi", 1, q => q.DiemQuyDoi.ToString()),
                new("Thao tác", .8, _ => "Chọn để xoá")
            };
            giftStack.Children.Add(new Border { Height = 170, BorderBrush = new SolidColorBrush(Color.Parse("#E2E8F0")), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Child = UiHelpers.TaoBang(new List<QuaTang>(), giftTable, _lbQuaDaChon) });
            
            var btnXoaQua = TaoButtonIcon("Bỏ quà đã chọn", "docs/imagess/trash.png", "#FFFFFF", "#EF4444", true);
            btnXoaQua.HorizontalAlignment = HorizontalAlignment.Left;
            btnXoaQua.Click += (_, _) => { _quaDangChon = null; CapNhatQua(); };
            giftStack.Children.Add(btnXoaQua);
            
            ((StackPanel)giftCard.Child!).Children.Add(giftStack);
            Grid.SetColumn(giftCard, 1);
            middle.Children.Add(giftCard);

            content.Children.Add(middle);

            // ===== LỊCH SỬ & TỔNG KẾT =====
            var bottom = new Grid { ColumnDefinitions = new ColumnDefinitions("1.65*,0.85*") };
            var historyCard = Card(TaoHeaderIcon("docs/imagess/clock.png", "Lịch sử mua hàng của khách hàng"));
            var historyStack = new StackPanel { Spacing = 10 };
            var historyCols = new List<ColDef<DonHang>>
            {
                new("STT", .45, x => (DanhSachLichSu().IndexOf(x) + 1).ToString()),
                new("Ngày mua", 1.1, x => x.NgayTao.ToString("dd/MM/yyyy")),
                new("Mã đơn", .85, x => x.MaDon),
                new("Tổng tiền", 1.05, x => $"{x.SoTien:N0} đ"),
                new("Điểm cộng", .85, x => x.DiemCong.ToString()),
                new("Quà đã đổi", 1.05, x => string.IsNullOrEmpty(x.QuaTangDoi) ? "-" : x.QuaTangDoi)
            };
            historyStack.Children.Add(new Border { Height = 230, BorderBrush = new SolidColorBrush(Color.Parse("#E2E8F0")), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Child = UiHelpers.TaoBang(new List<DonHang>(), historyCols, _lbLichSu) });
            historyStack.Children.Add(_txtKhongCoLichSu);
            var filters = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Spacing = 18 };
            filters.Children.Add(new TextBlock { Text = "Xem giao dịch theo:", VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.Parse("#64748B")) });
            filters.Children.Add(_rbNgay); filters.Children.Add(_rbThang); filters.Children.Add(_rbNam);
            historyStack.Children.Add(filters);
            ((StackPanel)historyCard.Child!).Children.Add(historyStack);
            Grid.SetColumn(historyCard, 0);
            bottom.Children.Add(historyCard);

            var summaryCard = Card(TaoHeaderIcon("docs/imagess/hoaDon.png", "Chi tiết giao dịch"));
            var summary = new StackPanel { Spacing = 12 };
            summary.Children.Add(SummaryRow("Tổng tiền thuốc", _txtTongTien));
            summary.Children.Add(SummaryRow("Tổng điểm được cộng", _txtDiemCong));
            summary.Children.Add(new TextBlock { Text = "(Tổng tiền / 1000)", FontSize = 11, Foreground = new SolidColorBrush(Color.Parse("#94A3B8")), Margin = new Thickness(0, -10, 0, 0) });
            summary.Children.Add(SummaryRow("Tổng điểm đổi quà", _txtTongDiemDoi));
            summary.Children.Add(new Border { Height = 1, Background = new SolidColorBrush(Color.Parse("#E2E8F0")), Margin = new Thickness(0, 3) });
            summary.Children.Add(SummaryRow("Điểm sau giao dịch", _txtDiemSau));
            summary.Children.Add(Field("Ghi chú", _txtGhiChu));
            summary.Children.Add(new Border { Height = 8, Background = Brushes.Transparent });
            
            // Nút tạo đơn & nút hủy
            _btnTaoDon = TaoButtonIcon("Xác nhận tạo đơn hàng", "docs/imagess/confirm.png", "#10B981", "#FFFFFF");
            _btnTaoDon.Height = 46;
            _btnTaoDon.HorizontalAlignment = HorizontalAlignment.Stretch;
            _btnTaoDon.Click += BtnTaoDon_Click;
            summary.Children.Add(_btnTaoDon);

            var btnHuy = TaoButtonIcon("Hủy bỏ", "docs/imagess/trash.png", "#F1F5F9", "#64748B", true);
            btnHuy.Height = 40;
            btnHuy.HorizontalAlignment = HorizontalAlignment.Stretch;
            btnHuy.Click += (_, _) => LamMoiDon();
            summary.Children.Add(btnHuy);
            
            ((StackPanel)summaryCard.Child!).Children.Add(summary);
            Grid.SetColumn(summaryCard, 1);
            bottom.Children.Add(summaryCard);
            
            content.Children.Add(bottom);

            Content = new ScrollViewer { Content = content, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };

            _cboKhachHang.SelectionChanged += (_, _) => CapNhatKhachHang();
            _cboThuoc.SelectionChanged += (_, _) => CapNhatDonGiaThuoc();
            _cboQua.SelectionChanged += (_, _) => CapNhatDiemQua();
            _rbNgay.IsCheckedChanged += (_, _) => CapNhatLichSu();
            _rbThang.IsCheckedChanged += (_, _) => CapNhatLichSu();
            _rbNam.IsCheckedChanged += (_, _) => CapNhatLichSu();

            NapDuLieu();
        }

        // ================= HÀM HỖ TRỢ VẼ UI (ẢNH, THẺ CARD, NÚT) =================

        private static Bitmap? TaoBitmap(string duongDan)
        {
            try
            {
                string pathFull = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, duongDan);
                if (File.Exists(pathFull)) return new Bitmap(pathFull);
                if (File.Exists(duongDan)) return new Bitmap(duongDan);
            }
            catch { }
            return null;
        }

        private StackPanel TaoHeaderIcon(string imagePath, string title, double fontSize = 16)
        {
            var stack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
            var bmp = TaoBitmap(imagePath);
            if (bmp != null)
            {
                stack.Children.Add(new Image { Source = bmp, Width = fontSize * 1.25, Height = fontSize * 1.25, VerticalAlignment = VerticalAlignment.Center });
            }
            stack.Children.Add(new TextBlock { Text = title, FontSize = fontSize, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#334155")), VerticalAlignment = VerticalAlignment.Center });
            return stack;
        }

        private Button TaoButtonIcon(string text, string imagePath, string bgHex, string fgHex, bool isGhost = false)
        {
            var stack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            var bmp = TaoBitmap(imagePath);
            if (bmp != null)
            {
                stack.Children.Add(new Image { Source = bmp, Width = 18, Height = 18, VerticalAlignment = VerticalAlignment.Center });
            }
            var txt = new TextBlock { Text = text, FontSize = 13.5, FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center };
            stack.Children.Add(txt);

            var btn = new Button
            {
                Content = stack,
                Padding = new Thickness(14, 8),
                CornerRadius = new CornerRadius(8),
                Cursor = new Cursor(StandardCursorType.Hand)
            };

            if (isGhost)
            {
                btn.Background = Brushes.Transparent;
                txt.Foreground = new SolidColorBrush(Color.Parse(fgHex));
            }
            else
            {
                btn.Background = new SolidColorBrush(Color.Parse(bgHex));
                txt.Foreground = new SolidColorBrush(Color.Parse(fgHex));
            }

            return btn;
        }

        private Border Card(StackPanel header)
        {
            var card = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.Parse("#E2E8F0")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(18)
            };
            var stack = new StackPanel { Spacing = 12 };
            stack.Children.Add(header);
            card.Child = stack;
            return card;
        }

        private static Control Field(string label, Control control, int column = -1)
        {
            var p = new StackPanel { Spacing = 6 };
            p.Children.Add(new TextBlock { Text = label, FontSize = 12.5, FontWeight = FontWeight.Medium, Foreground = new SolidColorBrush(Color.Parse("#64748B")) });
            p.Children.Add(control);
            if (column >= 0) Grid.SetColumn(p, column);
            return p;
        }

        private static Control InfoField(string label, TextBlock value, int column)
        {
            var box = new Border { Background = new SolidColorBrush(Color.Parse("#F8FAFC")), BorderBrush = new SolidColorBrush(Color.Parse("#E2E8F0")), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 10) };
            box.Child = value;
            return Field(label, box, column);
        }

        private static Control SummaryRow(string label, TextBlock value)
        {
            var g = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            g.Children.Add(new TextBlock { Text = label, FontSize = 13.5, Foreground = new SolidColorBrush(Color.Parse("#475569")) });
            Grid.SetColumn(value, 1);
            g.Children.Add(value);
            return g;
        }

        // ================= XỬ LÝ LOGIC NGHIỆP VỤ (GIỮ NGUYÊN) =================

        private void NapDuLieu()
        {
            _cboKhachHang.ItemsSource = _data.DanhSachKhachHang
                .OrderBy(k => k.HoTen)
                .Select(k => $"{k.MaKH} - {k.HoTen} - {k.SoDienThoai}")
                .ToList();

            _cboKhachHang.ItemFilter = (search, item) =>
            {
                if (string.IsNullOrWhiteSpace(search)) return true;
                return item?.ToString()?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false;
            };

            _cboThuoc.ItemsSource = _data.ThuocConHang();
            _cboThuoc.ItemTemplate = new FuncDataTemplate<Thuoc>((x, _) => new TextBlock { Text = x == null ? "" : $"{x.MaThuoc} - {x.TenThuoc}" });
            _cboThuoc.ItemFilter = (search, item) =>
            {
                if (string.IsNullOrWhiteSpace(search)) return true;
                if (item is not Thuoc thuoc) return false;
                return thuoc.MaThuoc.Contains(search, StringComparison.OrdinalIgnoreCase) || thuoc.TenThuoc.Contains(search, StringComparison.OrdinalIgnoreCase);
            };

            _cboQua.ItemsSource = _data.QuaTangCoTheDoi();
            _cboQua.ItemTemplate = new FuncDataTemplate<QuaTang>((x, _) => new TextBlock { Text = x == null ? "" : $"{x.TenQua} - {x.DiemQuyDoi} điểm" });

            CapNhatKhachHang();
            CapNhatThuoc();
            CapNhatQua();
        }

        private KhachHang? KhachHangDangChon()
        {
            if (_cboKhachHang.SelectedItem is not string s) return null;
            var ma = s.Split(" - ")[0];
            return _data.DanhSachKhachHang.FirstOrDefault(x => x.MaKH == ma);
        }

        private void CapNhatKhachHang()
        {
            var kh = KhachHangDangChon();
            _txtMaKH.Text = kh?.MaKH ?? "-";
            _txtTenKH.Text = kh?.HoTen ?? "-";
            _txtSdt.Text = kh?.SoDienThoai ?? "-";
            _txtDiem.Text = kh == null ? "0 điểm" : $"{kh.DiemTichLuy:N0} điểm";
            CapNhatLichSu();
            CapNhatTongKet();
        }

        private void CapNhatDonGiaThuoc()
        {
            _txtDonGiaThuoc.Text = _cboThuoc.SelectedItem is Thuoc t ? $"{t.DonGia:N0} đ" : "0 đ";
        }

        private void CapNhatDiemQua()
        {
            _txtDiemQua.Text = _cboQua.SelectedItem is QuaTang q ? $"{q.DiemQuyDoi:N0} điểm" : "0 điểm";
        }

        private void BtnThemThuoc_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_cboThuoc.SelectedItem is not Thuoc t || !t.ConHang) return;
            int sl = (int)(_numSLThuoc.Value ?? 0);
            if (sl <= 0) return;

            var old = _thuocDangChon.FirstOrDefault(x => x.MaThuoc == t.MaThuoc);
            if (old != null) old.SoLuong += sl;
            else _thuocDangChon.Add(new ChiTietDonHang { MaThuoc = t.MaThuoc, TenThuoc = t.TenThuoc, DonGia = t.DonGia, SoLuong = sl });

            _numSLThuoc.Value = 1;
            CapNhatThuoc();
        }

        private void CapNhatThuoc()
        {
            _lbThuoc.ItemsSource = null;
            _lbThuoc.ItemsSource = _thuocDangChon.ToList();
            CapNhatTongKet();
        }

        private void BtnThemQua_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_cboQua.SelectedItem is QuaTang q) { _quaDangChon = q; CapNhatQua(); }
        }

        private void CapNhatQua()
        {
            _lbQuaDaChon.ItemsSource = null;
            _lbQuaDaChon.ItemsSource = _quaDangChon == null ? new List<QuaTang>() : new List<QuaTang> { _quaDangChon };
            CapNhatTongKet();
        }

        private List<DonHang> DanhSachLichSu()
        {
            var kh = KhachHangDangChon();
            if (kh == null) return new List<DonHang>();
            var now = DateTime.Now;
            var ds = _data.DanhSachDonHang.Where(x => x.MaKH == kh.MaKH);
            if (_rbNgay.IsChecked == true) ds = ds.Where(x => x.NgayTao.Date == now.Date);
            else if (_rbThang.IsChecked == true) ds = ds.Where(x => x.NgayTao.Year == now.Year && x.NgayTao.Month == now.Month);
            else if (_rbNam.IsChecked == true) ds = ds.Where(x => x.NgayTao.Year == now.Year);
            return ds.OrderByDescending(x => x.NgayTao).ToList();
        }

        private void CapNhatLichSu()
        {
            var ds = DanhSachLichSu();
            _lbLichSu.ItemsSource = null;
            _lbLichSu.ItemsSource = ds;
            _txtKhongCoLichSu.IsVisible = KhachHangDangChon() != null && ds.Count == 0;
        }

        private void CapNhatTongKet()
        {
            var kh = KhachHangDangChon();
            decimal tongTien = _thuocDangChon.Sum(x => x.ThanhTien);
            int diemCong = (int)(tongTien / 1000);
            int diemDoi = _quaDangChon?.DiemQuyDoi ?? 0;
            int diemSau = (kh?.DiemTichLuy ?? 0) + diemCong - diemDoi;

            _txtTongTien.Text = $"{tongTien:N0} đ";
            _txtDiemCong.Text = $"{diemCong:N0} điểm";
            _txtTongDiemDoi.Text = $"{diemDoi:N0} điểm";
            _txtDiemSau.Text = $"{diemSau:N0} điểm";
            _btnTaoDon.IsEnabled = kh != null && _thuocDangChon.Count > 0 && diemDoi <= (kh?.DiemTichLuy ?? 0);
        }

        private async void BtnTaoDon_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var parent = TopLevel.GetTopLevel(this) as Window;
            var kh = KhachHangDangChon();
            if (kh == null)
            {
                await ThongBaoWindow.ThongBao(parent, "Thiếu thông tin", "Vui lòng chọn khách hàng.");
                return;
            }
            if (_thuocDangChon.Count == 0)
            {
                await ThongBaoWindow.ThongBao(parent, "Thiếu thông tin", "Vui lòng thêm ít nhất một loại thuốc.");
                return;
            }
            int diemQua = _quaDangChon?.DiemQuyDoi ?? 0;
            if (diemQua > kh.DiemTichLuy)
            {
                await ThongBaoWindow.ThongBao(parent, "Không đủ điểm", $"Khách hàng hiện có {kh.DiemTichLuy} điểm, chưa đủ đổi quà.");
                return;
            }

            var copy = _thuocDangChon.Select(x => new ChiTietDonHang { MaThuoc = x.MaThuoc, TenThuoc = x.TenThuoc, DonGia = x.DonGia, SoLuong = x.SoLuong }).ToList();
            var result = _data.TaoDonHang(kh.MaKH, copy, 0, _quaDangChon);
            if (!result.ThanhCong)
            {
                await ThongBaoWindow.ThongBao(parent, "Không thể tạo đơn", result.ThongBao);
                return;
            }

            await ThongBaoWindow.ThongBao(parent, "Thành công", result.ThongBao);
            LamMoiDon();
            NapDuLieu();
            _cboKhachHang.SelectedItem = _data.DanhSachKhachHang
                .Where(x => x.MaKH == kh.MaKH)
                .Select(x => $"{x.MaKH} - {x.HoTen} - {x.SoDienThoai}")
                .FirstOrDefault();
        }

        private void LamMoiDon()
        {
            _thuocDangChon.Clear();
            _quaDangChon = null;
            _txtGhiChu.Text = "";
            _numSLThuoc.Value = 1;
            _cboThuoc.SelectedItem = null;
            _cboQua.SelectedItem = null;
            CapNhatThuoc();
            CapNhatQua();
            CapNhatTongKet();
        }

        public void ChonKhachHang(string maKH)
        {
            var kh = _data.DanhSachKhachHang.FirstOrDefault(x => x.MaKH == maKH);
            if (kh != null) _cboKhachHang.SelectedItem = $"{kh.MaKH} - {kh.HoTen} - {kh.SoDienThoai}";
        }
    }
}