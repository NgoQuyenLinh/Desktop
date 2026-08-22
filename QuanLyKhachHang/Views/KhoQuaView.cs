using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using QuanLyKhachHang.Helpers;
using QuanLyKhachHang.Models;
using QuanLyKhachHang.Services;
using Avalonia.Controls.Primitives;

namespace QuanLyKhachHang.Views
{
    public class KhoQuaView : UserControl
    {
        private readonly DataService _data;

        private readonly TextBox _txtTimThang = new() { Width = 260, Watermark = "Tìm trong tháng..." };
        private readonly TextBox _txtTimChuaBan = new() { Width = 220, Watermark = "Tìm quà chưa tặng..." };
        private readonly TextBox _txtTimDangBan = new() { Width = 220, Watermark = "Tìm quà đang tặng..." };
        private readonly TextBox _txtTimHetHang = new() { Width = 260, Watermark = "Tìm quà đã hết hàng..." };

        private readonly ListBox _listBoxThang = new();
        private readonly ListBox _listBoxChuaBan = new();
        private readonly ListBox _listBoxDangBan = new();
        private readonly ListBox _listBoxHetHang = new();

        private readonly TextBlock _lblDangChon = new() { FontSize = 12, Foreground = Brushes.DimGray };

        private readonly Button _btnChuyenSangDangBan = new()
        {
            Content = "➡️ Chuyển sang Đang tặng",
            Background = new SolidColorBrush(Color.Parse("#0EA5E9")),
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 6, 0, 0),
            Cursor = new Cursor(StandardCursorType.Hand)
        };

        private readonly Button _btnChuyenVeChuaBan = new()
        {
            Content = "⬅️ Chuyển về Chưa tặng",
            Background = new SolidColorBrush(Color.Parse("#64748B")),
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 6, 0, 0),
            Cursor = new Cursor(StandardCursorType.Hand)
        };

        private QuaTang? _dangChon;
        private bool _dangDongBoChon; 

        private static string TrangThaiText(QuaTang q) =>
            q.SoLuong <= 0 ? "🔴 Đã tặng hết" : (q.DangBan ? "🟢 Đang tặng" : "⚪ Chưa tặng");

        private static List<ColDef<QuaTang>> TaoCotQuaTang() => new()
        {
            new("Mã Quà", 0.8, q => q.MaQua),
            new("Tên Quà", 1.8, q => q.TenQua),
            new("Điểm Đổi", 1, q => q.DiemQuyDoi.ToString("N0")),
            new("Số Lượng", 1, q => q.SoLuong.ToString("N0")),
            new("Trạng thái", 1.1, q => TrangThaiText(q))
        };

        public KhoQuaView(DataService data)
        {
            _data = data;
            Background = new SolidColorBrush(Color.Parse("#F8FAFC"));

            var goc = new StackPanel { Spacing = 12, Margin = new Thickness(15) };

            // ---- Tiêu đề + thanh nút thao tác dùng chung (Dùng Image Icon) ----
            var hangTieuDe = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 15, VerticalAlignment = VerticalAlignment.Center };
            hangTieuDe.Children.Add(new TextBlock { Text = "Quản lý Kho Quà", FontSize = 22, FontWeight = FontWeight.Bold, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.Parse("#1E293B")) });

            var btnThem = TaoNut("Thêm", "docs/imagess/thêm.png", "#2563EB");
            var btnSua = TaoNut("Sửa", "docs/imagess/edit.png", "#EAB308");
            var btnXoa = TaoNut("Xoá", "docs/imagess/trash.png", "#DC2626");
            
            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;

            hangTieuDe.Children.Add(btnThem);
            hangTieuDe.Children.Add(btnSua);
            hangTieuDe.Children.Add(btnXoa);
            goc.Children.Add(hangTieuDe);

            _lblDangChon.Text = "Chưa chọn quà nào.";
            goc.Children.Add(_lblDangChon);

            // ---- 2 GroupBox đặt song song ----
            var hangGroup = new Grid { ColumnDefinitions = new ColumnDefinitions("*,16,*") };

            var groupThang = TaoGroupBoxDon("🗓️ Quà nhập trong tháng", _txtTimThang, _listBoxThang);
            Grid.SetColumn(groupThang, 0);

            var groupTrangThai = TaoGroupBoxTrangThai();
            Grid.SetColumn(groupTrangThai, 2);

            hangGroup.Children.Add(groupThang);
            hangGroup.Children.Add(groupTrangThai);
            goc.Children.Add(hangGroup);

            // ---- Khu vực riêng: "Đã tặng hết" ----
            var groupHetHang = TaoGroupBoxHetHang();
            goc.Children.Add(groupHetHang);

            _txtTimThang.TextChanged += (s, e) => TaiLaiDuLieu();
            _txtTimChuaBan.TextChanged += (s, e) => TaiLaiDuLieu();
            _txtTimDangBan.TextChanged += (s, e) => TaiLaiDuLieu();
            _txtTimHetHang.TextChanged += (s, e) => TaiLaiDuLieu();

            _listBoxThang.SelectionChanged += (s, e) => ChonTu(_listBoxThang, _listBoxChuaBan, _listBoxDangBan, _listBoxHetHang);
            _listBoxChuaBan.SelectionChanged += (s, e) => ChonTu(_listBoxChuaBan, _listBoxThang, _listBoxDangBan, _listBoxHetHang);
            _listBoxDangBan.SelectionChanged += (s, e) => ChonTu(_listBoxDangBan, _listBoxThang, _listBoxChuaBan, _listBoxHetHang);
            _listBoxHetHang.SelectionChanged += (s, e) => ChonTu(_listBoxHetHang, _listBoxThang, _listBoxChuaBan, _listBoxDangBan);

            _listBoxThang.DoubleTapped += (s, e) => { if (_dangChon != null) _ = HienThiPopup(_dangChon, isMoi: false); };
            _listBoxChuaBan.DoubleTapped += (s, e) => { if (_dangChon != null) _ = HienThiPopup(_dangChon, isMoi: false); };
            _listBoxDangBan.DoubleTapped += (s, e) => { if (_dangChon != null) _ = HienThiPopup(_dangChon, isMoi: false); };
            _listBoxHetHang.DoubleTapped += (s, e) => { if (_dangChon != null) _ = HienThiPopup(_dangChon, isMoi: false); };

            _btnChuyenSangDangBan.Click += (s, e) => ChuyenTrangThai(true);
            _btnChuyenVeChuaBan.Click += (s, e) => ChuyenTrangThai(false);

            Content = new ScrollViewer { Content = goc, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            TaiLaiDuLieu();
        }

        // ================= HÀM TẠO UI =================

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

        private Button TaoNut(string text, string imagePath, string maMau)
        {
            var stack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            var bmp = TaoBitmap(imagePath);
            if (bmp != null) stack.Children.Add(new Image { Source = bmp, Width = 18, Height = 18, VerticalAlignment = VerticalAlignment.Center });
            
            stack.Children.Add(new TextBlock { Text = text, FontSize = 13, FontWeight = FontWeight.SemiBold, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center });

            return new Button
            {
                Content = stack,
                Padding = new Thickness(12, 8),
                Background = new SolidColorBrush(Color.Parse(maMau)),
                CornerRadius = new CornerRadius(8),
                Cursor = new Cursor(StandardCursorType.Hand)
            };
        }

        private Border TaoGroupBoxDon(string tieuDe, TextBox oTim, ListBox listBox)
        {
            var noiDung = new StackPanel { Spacing = 10, Margin = new Thickness(14) };
            noiDung.Children.Add(new TextBlock { Text = tieuDe, FontSize = 16, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#1E293B")) });
            noiDung.Children.Add(oTim);

            var khungBang = new Border { Background = Brushes.White, Height = 420, ClipToBounds = true, BorderBrush = new SolidColorBrush(Color.Parse("#E2E8F0")), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6) };
            khungBang.Child = UiHelpers.TaoBang(new List<QuaTang>(), TaoCotQuaTang(), listBox);
            noiDung.Children.Add(khungBang);

            return new Border { Background = Brushes.White, BorderBrush = new SolidColorBrush(Color.Parse("#E2E8F0")), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Child = noiDung };
        }

        private Border TaoGroupBoxTrangThai()
        {
            var noiDung = new StackPanel { Spacing = 10, Margin = new Thickness(14) };
            noiDung.Children.Add(new TextBlock { Text = "🔁 Trạng thái tặng quà", FontSize = 16, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#1E293B")) });

            var panelChuaBan = new StackPanel { Spacing = 10, Margin = new Thickness(0, 10, 0, 0) };
            panelChuaBan.Children.Add(_txtTimChuaBan);
            var khungChuaBan = new Border { Background = Brushes.White, Height = 340, ClipToBounds = true, BorderBrush = new SolidColorBrush(Color.Parse("#E2E8F0")), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6) };
            khungChuaBan.Child = UiHelpers.TaoBang(new List<QuaTang>(), TaoCotQuaTang(), _listBoxChuaBan);
            panelChuaBan.Children.Add(khungChuaBan);
            panelChuaBan.Children.Add(_btnChuyenSangDangBan);
            var tabChuaBan = new TabItem { Header = "⚪ Chưa tặng", Content = panelChuaBan };

            var panelDangBan = new StackPanel { Spacing = 10, Margin = new Thickness(0, 10, 0, 0) };
            panelDangBan.Children.Add(_txtTimDangBan);
            var khungDangBan = new Border { Background = Brushes.White, Height = 340, ClipToBounds = true, BorderBrush = new SolidColorBrush(Color.Parse("#E2E8F0")), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6) };
            khungDangBan.Child = UiHelpers.TaoBang(new List<QuaTang>(), TaoCotQuaTang(), _listBoxDangBan);
            panelDangBan.Children.Add(khungDangBan);
            panelDangBan.Children.Add(_btnChuyenVeChuaBan);
            var tabDangBan = new TabItem { Header = "🟢 Đang tặng", Content = panelDangBan };

            var tabControl = new TabControl();
            tabControl.Items.Add(tabChuaBan);
            tabControl.Items.Add(tabDangBan);
            noiDung.Children.Add(tabControl);

            return new Border { Background = Brushes.White, BorderBrush = new SolidColorBrush(Color.Parse("#E2E8F0")), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Child = noiDung };
        }

        private Border TaoGroupBoxHetHang()
        {
            var noiDung = new StackPanel { Spacing = 10, Margin = new Thickness(14) };
            noiDung.Children.Add(new TextBlock { Text = "🔴 Đã tặng hết", FontSize = 16, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#991B1B")) });
            noiDung.Children.Add(new TextBlock { Text = "Những quà đã hết số lượng trong kho. Cần sửa và nhập thêm số lượng để tiếp tục tặng.", FontSize = 12.5, Foreground = new SolidColorBrush(Color.Parse("#EF4444")), TextWrapping = TextWrapping.Wrap });
            noiDung.Children.Add(_txtTimHetHang);

            var khungBang = new Border { Background = Brushes.White, Height = 260, ClipToBounds = true, BorderBrush = new SolidColorBrush(Color.Parse("#FECACA")), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6) };
            khungBang.Child = UiHelpers.TaoBang(new List<QuaTang>(), TaoCotQuaTang(), _listBoxHetHang);
            noiDung.Children.Add(khungBang);

            return new Border { Background = new SolidColorBrush(Color.Parse("#FEF2F2")), BorderBrush = new SolidColorBrush(Color.Parse("#FCA5A5")), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Child = noiDung };
        }

        // ================= XỬ LÝ LOGIC & ĐỌC DỮ LIỆU TỪ FILE =================

        private void ChonTu(ListBox nguon, params ListBox[] conLai)
        {
            if (_dangDongBoChon) return;
            var quaChon = nguon.SelectedItem as QuaTang;
            if (quaChon == null) return;

            _dangDongBoChon = true;
            foreach (var lb in conLai) lb.SelectedItem = null;
            _dangDongBoChon = false;

            _dangChon = quaChon;
            _lblDangChon.Text = $"Đang chọn: {quaChon.TenQua} (Mã {quaChon.MaQua}) — {TrangThaiText(quaChon)}";
        }

        /// <summary>
        /// TRỰC TIẾP LỌC DỮ LIỆU TỪ _data.DanhSachQuaTang ĐỂ ĐẢM BẢO HIỂN THỊ ĐÚNG FILE JSON
        /// </summary>
        private void TaiLaiDuLieu()
        {
            // Đảm bảo lấy danh sách mới nhất từ DataService
            var ds = _data.DanhSachQuaTang ?? new List<QuaTang>();
            var now = DateTime.Now;

            // 1. Quà trong tháng
            string tThang = _txtTimThang.Text?.ToLower() ?? "";
            _listBoxThang.ItemsSource = ds.Where(q => q.NgayTao.Month == now.Month && q.NgayTao.Year == now.Year &&
                                                (q.TenQua.ToLower().Contains(tThang) || q.MaQua.ToLower().Contains(tThang))).ToList();

            // 2. Quà chưa tặng (Còn hàng và Đang tắt DangBan)
            string tChua = _txtTimChuaBan.Text?.ToLower() ?? "";
            _listBoxChuaBan.ItemsSource = ds.Where(q => q.SoLuong > 0 && !q.DangBan &&
                                                  (q.TenQua.ToLower().Contains(tChua) || q.MaQua.ToLower().Contains(tChua))).ToList();

            // 3. Quà đang tặng (Còn hàng và Đang bật DangBan)
            string tDang = _txtTimDangBan.Text?.ToLower() ?? "";
            _listBoxDangBan.ItemsSource = ds.Where(q => q.SoLuong > 0 && q.DangBan &&
                                                  (q.TenQua.ToLower().Contains(tDang) || q.MaQua.ToLower().Contains(tDang))).ToList();

            // 4. Quà đã hết hàng (Số lượng <= 0)
            string tHet = _txtTimHetHang.Text?.ToLower() ?? "";
            _listBoxHetHang.ItemsSource = ds.Where(q => q.SoLuong <= 0 &&
                                                  (q.TenQua.ToLower().Contains(tHet) || q.MaQua.ToLower().Contains(tHet))).ToList();

            if (_dangChon != null)
            {
                var quaMoi = ds.FirstOrDefault(q => q.MaQua == _dangChon.MaQua);
                if (quaMoi != null)
                {
                    _dangChon = quaMoi;
                    _lblDangChon.Text = $"Đang chọn: {quaMoi.TenQua} (Mã {quaMoi.MaQua}) — {TrangThaiText(quaMoi)}";
                }
                else
                {
                    _dangChon = null;
                    _lblDangChon.Text = "Chưa chọn quà nào.";
                }
            }
        }

        private async void ChuyenTrangThai(bool sangDangBan)
        {
            if (_dangChon == null)
            {
                await ThongBaoWindow.ThongBao(TopLevel.GetTopLevel(this) as Window, "Thông báo", "Vui lòng chọn 1 quà tặng cần chuyển trạng thái.");
                return;
            }

            if (_dangChon.SoLuong <= 0)
            {
                await ThongBaoWindow.ThongBao(TopLevel.GetTopLevel(this) as Window, "Thông báo", "Quà này đã hết hàng, vui lòng Sửa số lượng trước khi chuyển trạng thái.");
                return;
            }

            _dangChon.DangBan = sangDangBan;
            _data.SuaQuaTang(_dangChon); // Lưu xuống file JSON thông qua DataService
            TaiLaiDuLieu();
        }

        private async void BtnThem_Click(object? sender, RoutedEventArgs e)
        {
            await HienThiPopup(new QuaTang { MaQua = _data.TaoMaQuaTangMoi() }, isMoi: true);
        }

        private async void BtnSua_Click(object? sender, RoutedEventArgs e)
        {
            if (_dangChon == null)
            {
                await ThongBaoWindow.ThongBao(TopLevel.GetTopLevel(this) as Window, "Thông báo", "Vui lòng chọn 1 quà tặng cần sửa.");
                return;
            }
            await HienThiPopup(_dangChon, isMoi: false);
        }

        private async void BtnXoa_Click(object? sender, RoutedEventArgs e)
        {
            var cuaSoCha = TopLevel.GetTopLevel(this) as Window;

            if (_dangChon == null)
            {
                await ThongBaoWindow.ThongBao(cuaSoCha, "Thông báo", "Vui lòng chọn 1 quà tặng cần xoá.");
                return;
            }

            bool dongY = await ThongBaoWindow.XacNhan(cuaSoCha, "Xác nhận xoá", $"Bạn có chắc muốn xoá quà \n'{_dangChon.TenQua}'?");
            if (dongY)
            {
                _data.XoaQuaTang(_dangChon.MaQua);
                _dangChon = null;
                _lblDangChon.Text = "Chưa chọn quà nào.";
                TaiLaiDuLieu();
            }
        }

        private async System.Threading.Tasks.Task HienThiPopup(QuaTang qua, bool isMoi)
        {
            var popup = new Window
            {
                Title = isMoi ? "Thêm Quà Tặng" : "Sửa Quà Tặng",
                Width = 350,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var panel = new StackPanel { Spacing = 10, Margin = new Thickness(15) };

            var txtTenQua = new TextBox { Text = qua.TenQua, Watermark = "Tên Quà" };
            var numDiem = new NumericUpDown { Value = qua.DiemQuyDoi, Minimum = 0, FormatString = "0" };
            var numSL = new NumericUpDown { Value = qua.SoLuong, Minimum = 0, FormatString = "0" };
            var chkDangBan = new CheckBox { Content = "Trạng thái: Đang tặng", IsChecked = qua.DangBan };

            var btnLuu = new Button
            {
                Content = "💾 Lưu thay đổi",
                Background = new SolidColorBrush(Color.Parse("#10B981")),
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Right,
                Cursor = new Cursor(StandardCursorType.Hand),
                Padding = new Thickness(12, 8)
            };

            panel.Children.Add(new TextBlock { Text = "Tên quà:" });
            panel.Children.Add(txtTenQua);
            panel.Children.Add(new TextBlock { Text = "Điểm quy đổi:" });
            panel.Children.Add(numDiem);
            panel.Children.Add(new TextBlock { Text = "Số lượng trong kho:" });
            panel.Children.Add(numSL);
            panel.Children.Add(chkDangBan);
            panel.Children.Add(btnLuu);

            popup.Content = panel;

            btnLuu.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtTenQua.Text)) return;

                qua.TenQua = txtTenQua.Text;
                qua.DiemQuyDoi = (int)(numDiem.Value ?? 0);
                qua.SoLuong = (int)(numSL.Value ?? 0);
                qua.DangBan = chkDangBan.IsChecked ?? false;

                if (isMoi) _data.ThemQuaTang(qua);
                else _data.SuaQuaTang(qua);

                TaiLaiDuLieu();
                popup.Close();
            };

            if (TopLevel.GetTopLevel(this) is Window parentWindow)
            {
                await popup.ShowDialog(parentWindow);
            }
            else
            {
                popup.Show();
            }
        }
    }
}