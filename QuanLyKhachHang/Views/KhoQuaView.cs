using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using QuanLyKhachHang.Helpers;
using QuanLyKhachHang.Models;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Views
{
    /// <summary>
    /// Màn hình Quản lý Kho Quà - bố cục:
    ///  - Trái  : "🗓️ Quà trong tháng" -> quà có NgayTao thuộc tháng/năm hiện tại (chỉ để xem).
    ///  - Phải  : "🔁 Trạng thái tặng quà" -> TabControl 2 tab "Chưa tặng" / "Đang tặng" dựa trên
    ///            cờ thủ công QuaTang.DangBan. Người dùng chọn 1 quà rồi bấm nút chuyển để
    ///            đẩy quà đó qua lại giữa 2 trạng thái bất cứ lúc nào. Quà đã hết hàng
    ///            (SoLuong &lt;= 0) sẽ tự động không còn nằm ở 2 tab này nữa.
    ///  - Dưới  : "🔴 Đã tặng hết" -> khu vực riêng cho quà hết hàng (SoLuong &lt;= 0),
    ///            tách khỏi 2 trạng thái Chưa tặng / Đang tặng để dễ nhận biết cần nhập thêm.
    /// Nút Thêm / Sửa / Xoá dùng chung, thao tác trên quà đang được chọn ở BẤT KỲ bảng nào.
    /// </summary>
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
            Margin = new Thickness(0, 6, 0, 0)
        };

        private readonly Button _btnChuyenVeChuaBan = new()
        {
            Content = "⬅️ Chuyển về Chưa tặng",
            Background = new SolidColorBrush(Color.Parse("#64748B")),
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 6, 0, 0)
        };

        private QuaTang? _dangChon;
        private bool _dangDongBoChon; // cờ chống vòng lặp khi tự xoá lựa chọn ở bảng còn lại

        /// <summary>
        /// Nhãn trạng thái hiển thị dùng chung cho cột "Trạng thái" và dòng "Đang chọn".
        /// Quà hết hàng (SoLuong &lt;= 0) luôn hiện "Đã tặng hết", bất kể cờ DangBan là gì.
        /// </summary>
        private static string TrangThaiText(QuaTang q) =>
            q.SoLuong <= 0 ? "🔴 Đã tặng hết" : (q.DangBan ? "🟢 Đang tặng" : "⚪ Chưa tặng");

        private static List<ColDef<QuaTang>> TaoCotQuaTang() => new()
        {
            new("Mã Quà", 0.8, q => q.MaQua),
            new("Tên Quà", 1.8, q => q.TenQua),
            new("Điểm Đổi", 1, q => q.DiemQuyDoi.ToString()),
            new("Số Lượng", 1, q => q.SoLuong.ToString()),
            new("Trạng thái", 1.1, q => TrangThaiText(q))
        };

        public KhoQuaView(DataService data)
        {
            _data = data;

            var goc = new StackPanel { Spacing = 12 };

            // ---- Tiêu đề + thanh nút thao tác dùng chung ----
            var hangTieuDe = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 15, VerticalAlignment = VerticalAlignment.Center };
            hangTieuDe.Children.Add(new TextBlock { Text = "Quản lý Kho Quà", FontSize = 20, FontWeight = FontWeight.Bold, VerticalAlignment = VerticalAlignment.Center });

            var btnThem = TaoNut("➕ Thêm", "#2563EB");
            var btnSua = TaoNut("✏️ Sửa", "#EAB308");
            var btnXoa = TaoNut("🗑️ Xoá", "#DC2626");
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

            var groupThang = TaoGroupBoxDon(
                "🗓️ Quà trong tháng",
                _txtTimThang,
                _listBoxThang);
            Grid.SetColumn(groupThang, 0);

            var groupTrangThai = TaoGroupBoxTrangThai();
            Grid.SetColumn(groupTrangThai, 2);

            hangGroup.Children.Add(groupThang);
            hangGroup.Children.Add(groupTrangThai);
            goc.Children.Add(hangGroup);

            // ---- Khu vực riêng: "Đã tặng hết" (SoLuong <= 0), tách khỏi 2 trạng thái trên ----
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

            _btnChuyenSangDangBan.Click += (s, e) => ChuyenTrangThai();
            _btnChuyenVeChuaBan.Click += (s, e) => ChuyenTrangThai();

            Content = goc;
            TaiLaiDuLieu();
        }

        /// <summary>Border mô phỏng GroupBox đơn giản chỉ có 1 bảng (dùng cho "Quà trong tháng").</summary>
        private Border TaoGroupBoxDon(string tieuDe, TextBox oTim, ListBox listBox)
        {
            var noiDung = new StackPanel { Spacing = 10, Margin = new Thickness(14) };

            noiDung.Children.Add(new TextBlock { Text = tieuDe, FontSize = 15, FontWeight = FontWeight.Bold });
            noiDung.Children.Add(oTim);

            var khungBang = new Border { Background = Brushes.White, Height = 420, ClipToBounds = true };
            khungBang.Child = UiHelpers.TaoBang(new List<QuaTang>(), TaoCotQuaTang(), listBox);
            noiDung.Children.Add(khungBang);

            return new Border
            {
                Background = new SolidColorBrush(Color.Parse("#F9FAFB")),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Child = noiDung
            };
        }

        /// <summary>GroupBox "Trạng thái tặng quà" chứa TabControl 2 tab Chưa tặng / Đang tặng + nút chuyển đổi.</summary>
        private Border TaoGroupBoxTrangThai()
        {
            var noiDung = new StackPanel { Spacing = 10, Margin = new Thickness(14) };
            noiDung.Children.Add(new TextBlock { Text = "🔁 Trạng thái tặng quà", FontSize = 15, FontWeight = FontWeight.Bold });

            // ---- Tab "Chưa tặng" ----
            var panelChuaBan = new StackPanel { Spacing = 10, Margin = new Thickness(0, 10, 0, 0) };
            panelChuaBan.Children.Add(_txtTimChuaBan);
            var khungChuaBan = new Border { Background = Brushes.White, Height = 340, ClipToBounds = true };
            khungChuaBan.Child = UiHelpers.TaoBang(new List<QuaTang>(), TaoCotQuaTang(), _listBoxChuaBan);
            panelChuaBan.Children.Add(khungChuaBan);
            panelChuaBan.Children.Add(_btnChuyenSangDangBan);

            var tabChuaBan = new TabItem { Header = "⚪ Chưa tặng", Content = panelChuaBan };

            // ---- Tab "Đang tặng" ----
            var panelDangBan = new StackPanel { Spacing = 10, Margin = new Thickness(0, 10, 0, 0) };
            panelDangBan.Children.Add(_txtTimDangBan);
            var khungDangBan = new Border { Background = Brushes.White, Height = 340, ClipToBounds = true };
            khungDangBan.Child = UiHelpers.TaoBang(new List<QuaTang>(), TaoCotQuaTang(), _listBoxDangBan);
            panelDangBan.Children.Add(khungDangBan);
            panelDangBan.Children.Add(_btnChuyenVeChuaBan);

            var tabDangBan = new TabItem { Header = "🟢 Đang tặng", Content = panelDangBan };

            var tabControl = new TabControl();
            tabControl.Items.Add(tabChuaBan);
            tabControl.Items.Add(tabDangBan);
            noiDung.Children.Add(tabControl);

            return new Border
            {
                Background = new SolidColorBrush(Color.Parse("#F9FAFB")),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Child = noiDung
            };
        }

        /// <summary>
        /// Khu vực riêng "🔴 Đã tặng hết" — nằm tách biệt bên dưới, dành cho quà đã hết
        /// hàng trong kho (SoLuong &lt;= 0). Chỉ xem/tìm kiếm + Sửa (để nhập thêm hàng) /
        /// Xoá qua các nút dùng chung; không có nút chuyển trạng thái vì trạng thái này
        /// được tính tự động theo số lượng, không phải cờ thủ công.
        /// </summary>
        private Border TaoGroupBoxHetHang()
        {
            var noiDung = new StackPanel { Spacing = 10, Margin = new Thickness(14) };
            noiDung.Children.Add(new TextBlock { Text = "🔴 Đã tặng hết", FontSize = 15, FontWeight = FontWeight.Bold });
            noiDung.Children.Add(new TextBlock
            {
                Text = "Những quà đã hết số lượng trong kho. Sửa quà và nhập thêm số lượng để đưa quà trở lại 2 trạng thái Chưa tặng / Đang tặng.",
                FontSize = 12,
                Foreground = Brushes.DimGray,
                TextWrapping = TextWrapping.Wrap
            });
            noiDung.Children.Add(_txtTimHetHang);

            var khungBang = new Border { Background = Brushes.White, Height = 260, ClipToBounds = true };
            khungBang.Child = UiHelpers.TaoBang(new List<QuaTang>(), TaoCotQuaTang(), _listBoxHetHang);
            noiDung.Children.Add(khungBang);

            return new Border
            {
                Background = new SolidColorBrush(Color.Parse("#FEF2F2")),
                BorderBrush = new SolidColorBrush(Color.Parse("#FCA5A5")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Child = noiDung
            };
        }

        private Button TaoNut(string text, string maMau)
        {
            return new Button
            {
                Content = text,
                Width = 100,
                Background = new SolidColorBrush(Color.Parse(maMau)),
                Foreground = Brushes.White
            };
        }

        /// <summary>Khi chọn 1 dòng ở bảng này, bỏ chọn ở 2 bảng kia để tránh nhầm lẫn "đang chọn quà nào".</summary>
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

        private async void ChuyenTrangThai()
        {
            if (_dangChon == null)
            {
                await ThongBaoWindow.ThongBao(TopLevel.GetTopLevel(this) as Window, "Thông báo", "Vui lòng chọn 1 quà tặng cần chuyển trạng thái.");
                return;
            }

            _data.ChuyenTrangThaiQuaTang(_dangChon.MaQua);
            TaiLaiDuLieu();
        }

        private void TaiLaiDuLieu()
        {
            _listBoxThang.ItemsSource = _data.QuaTangTrongThang(_txtTimThang.Text);
            _listBoxChuaBan.ItemsSource = _data.QuaTangChuaBan(_txtTimChuaBan.Text);
            _listBoxDangBan.ItemsSource = _data.QuaTangDangBan(_txtTimDangBan.Text);
            _listBoxHetHang.ItemsSource = _data.QuaTangDaHetHang(_txtTimHetHang.Text);

            if (_dangChon != null)
            {
                // đồng bộ lại nhãn "đang chọn" phòng khi trạng thái vừa đổi
                var quaMoi = _data.DanhSachQuaTang.FirstOrDefault(q => q.MaQua == _dangChon.MaQua);
                if (quaMoi != null)
                {
                    _dangChon = quaMoi;
                    _lblDangChon.Text = $"Đang chọn: {quaMoi.TenQua} (Mã {quaMoi.MaQua}) — {TrangThaiText(quaMoi)}";
                }
            }
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
                Height = 280,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var panel = new StackPanel { Spacing = 10, Margin = new Thickness(15) };

            var txtTenQua = new TextBox { Text = qua.TenQua, Watermark = "Tên Quà" };
            var numDiem = new NumericUpDown { Value = qua.DiemQuyDoi, Minimum = 0, FormatString = "0" };
            var numSL = new NumericUpDown { Value = qua.SoLuong, Minimum = 0, FormatString = "0" };
            var chkDangBan = new CheckBox { Content = "Đang tặng", IsChecked = qua.DangBan };

            var btnLuu = new Button
            {
                Content = "💾 Lưu",
                Background = new SolidColorBrush(Color.Parse("#16A34A")),
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Right
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