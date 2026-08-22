using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using QuanLyKhachHang.Helpers;
using QuanLyKhachHang.Models;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Views
{
    /// <summary>
    /// Màn hình Quản lý khách hàng: bảng danh sách, ô tìm kiếm tức thời
    /// (theo tên / SĐT / mã KH) và 3 nút Thêm / Sửa / Xoá.
    /// </summary>
    public class KhachHangView : UserControl
    {
        private readonly DataService _data;
        private readonly TextBox _txtTimKiem = new() { Width = 320, Watermark = "Nhập để tìm..." };
        private readonly ListBox _listBox = new();

        // Menu chuột phải trên 1 dòng khách hàng: chọn "Sửa" trực tiếp không cần bấm nút Sửa ở trên.
        private readonly MenuItem _menuSua = new() { Header = "✏️ Sửa khách hàng này" };
        private readonly MenuItem _menuXoa = new() { Header = "🗑️ Xoá khách hàng này" };

        private KhachHang? _dangChon;

        public KhachHangView(DataService data)
        {
            _data = data;

            var goc = new StackPanel { Spacing = 12 };

            goc.Children.Add(new TextBlock
            {
                Text = "Quản lý khách hàng",
                FontSize = 20,
                FontWeight = FontWeight.Bold
            });

            goc.Children.Add(new TextBlock { Text = "🔍 Tìm kiếm (theo tên / SĐT / mã KH):", FontSize = 13 });

            var hangTimKiem = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
            _txtTimKiem.TextChanged += (s, e) => TaiLaiDuLieu(); // tìm kiếm tức thời

            var btnThem = TaoNut("➕ Thêm", "#2563EB");
            var btnSua = TaoNut("✏️ Sửa", "#EAB308");
            var btnXoa = TaoNut("🗑️ Xoá", "#DC2626");
            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;

            hangTimKiem.Children.Add(_txtTimKiem);
            hangTimKiem.Children.Add(btnThem);
            hangTimKiem.Children.Add(btnSua);
            hangTimKiem.Children.Add(btnXoa);
            goc.Children.Add(hangTimKiem);

            _listBox.SelectionChanged += (s, e) => _dangChon = _listBox.SelectedItem as KhachHang;

            _listBox.DoubleTapped += ListBox_DoubleTapped;

            // ---- Chuột phải trên 1 dòng: tự động chọn dòng đó rồi mở menu Sửa/Xoá ----
            _menuSua.Click += BtnSua_Click;
            _menuXoa.Click += BtnXoa_Click;
            _listBox.ContextMenu = new ContextMenu { ItemsSource = new List<MenuItem> { _menuSua, _menuXoa } };
            _listBox.AddHandler(InputElement.PointerPressedEvent, ListBox_PointerPressed, Avalonia.Interactivity.RoutingStrategies.Tunnel);

            var khungBang = new Border { Background = Brushes.White, Height = 480, ClipToBounds = true };
            khungBang.Child = UiHelpers.TaoBang<KhachHang>(
                new List<KhachHang>(),
                new List<ColDef<KhachHang>>
                {
                    new("Mã KH", 0.8, kh => kh.MaKH),
                    new("Họ tên", 2, kh => kh.HoTen),
                    new("Số điện thoại", 1.3, kh => kh.SoDienThoai),
                    new("Điểm tích luỹ", 1, kh => kh.DiemTichLuy.ToString()),
                    new("Ngày tạo", 1.2, kh => kh.NgayTao.ToString("dd/MM/yyyy"))
                },
                _listBox);
            goc.Children.Add(khungBang);

            Content = goc;
            TaiLaiDuLieu();
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

        /// <summary>
        /// Khi người dùng bấm chuột phải, tự động trỏ (chọn) đúng dòng khách hàng nằm dưới
        /// con trỏ chuột trước khi hiện menu Sửa/Xoá — tránh trường hợp menu áp dụng nhầm
        /// lên dòng đang chọn cũ (nếu có) thay vì dòng vừa bấm chuột phải vào.
        /// </summary>
        private void ListBox_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(_listBox).Properties.IsRightButtonPressed) return;

            if (e.Source is Visual v)
            {
                var dong = v.FindAncestorOfType<ListBoxItem>();
                if (dong?.DataContext is KhachHang kh)
                {
                    _listBox.SelectedItem = kh;
                    _dangChon = kh;
                }
            }
        }

        private void TaiLaiDuLieu()
        {
            var ketQua = _data.TimKiemKhachHang(_txtTimKiem.Text ?? string.Empty);
            _listBox.ItemsSource = ketQua;
        }

        private async void BtnThem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var cuaSo = new KhachHangEditWindow(_data.TaoMaKhachHangMoi());
            //await cuaSo.ShowDialog(TopLevel.GetTopLevel(this) as Window);
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
                TaiLaiDuLieu();
            }
        }

        private async void BtnSua_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_dangChon == null)
            {
                await ThongBaoWindow.ThongBao(TopLevel.GetTopLevel(this) as Window, "Thông báo", "Vui lòng chọn 1 khách hàng cần sửa.");
                return;
            }

            var cuaSo = new KhachHangEditWindow(_dangChon.MaKH, _dangChon);
            //await cuaSo.ShowDialog(TopLevel.GetTopLevel(this) as Window);
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
                _data.SuaKhachHang(cuaSo.KetQua);
                TaiLaiDuLieu();
            }
        }

        private async void BtnXoa_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var cuaSoCha = TopLevel.GetTopLevel(this) as Window;

            if (_dangChon == null)
            {
                await ThongBaoWindow.ThongBao(cuaSoCha, "Thông báo", "Vui lòng chọn 1 khách hàng cần xoá.");
                return;
            }

            bool dongY = await ThongBaoWindow.XacNhan(cuaSoCha, "Xác nhận xoá", $"Bạn có chắc muốn xoá khách hàng \n'{_dangChon.HoTen}'?");
            if (dongY)
            {
                _data.XoaKhachHang(_dangChon.MaKH);

                TaiLaiDuLieu();
            }
            //TaiLaiDuLieu();
        }

        private async void ListBox_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (_dangChon == null) return;

            var container = _listBox.ContainerFromItem(_dangChon);
            if (container == null) return;

            var editPopup = new Window
            {
                Title = "Sửa nhanh khách hàng",
                Width = 350,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var panel = new StackPanel { Spacing = 10, Margin = new Thickness(15) };
            
            var txtHoTen = new TextBox { Text = _dangChon.HoTen, Watermark = "Họ tên" };
            var txtSdt = new TextBox { Text = _dangChon.SoDienThoai, Watermark = "Số điện thoại" };
            
            var btnLuu = new Button 
            { 
                Content = "💾 Lưu (Enter)", 
                Background = new SolidColorBrush(Color.Parse("#16A34A")), 
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            panel.Children.Add(new TextBlock { Text = "Chỉnh sửa thông tin nhanh:", FontWeight = FontWeight.Bold });
            panel.Children.Add(txtHoTen);
            panel.Children.Add(txtSdt);
            panel.Children.Add(btnLuu);

            editPopup.Content = panel;

            void LuuVaDong()
            {
                _dangChon.HoTen = txtHoTen.Text;
                _dangChon.SoDienThoai = txtSdt.Text;

                _data.SuaKhachHang(_dangChon);
                TaiLaiDuLieu();
                editPopup.Close();
            }

            btnLuu.Click += (s, ev) => LuuVaDong();
            txtHoTen.KeyDown += (s, ev) => { if (ev.Key == Key.Enter) LuuVaDong(); };
            txtSdt.KeyDown += (s, ev) => { if (ev.Key == Key.Enter) LuuVaDong(); };

            // Sửa cảnh báo null reference tại đây
            if (TopLevel.GetTopLevel(this) is Window parentWindow)
            {
                await editPopup.ShowDialog(parentWindow);
            }
            else
            {
                editPopup.Show();
            }
        }
    }
}
