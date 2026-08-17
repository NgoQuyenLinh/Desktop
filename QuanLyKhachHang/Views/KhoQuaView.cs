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
    public class KhoQuaView : UserControl
    {
        private readonly DataService _data;
        private readonly TextBox _txtTimKiem = new() { Width = 320, Watermark = "Nhập để tìm tên quà..." };
        private readonly ListBox _listBox = new();
        private QuaTang? _dangChon;

        public KhoQuaView(DataService data)
        {
            _data = data;

            var goc = new StackPanel { Spacing = 12 };

            goc.Children.Add(new TextBlock
            {
                Text = "Quản lý Kho Quà",
                FontSize = 20,
                FontWeight = FontWeight.Bold
            });

            goc.Children.Add(new TextBlock { Text = "🔍 Tìm kiếm quà tặng:", FontSize = 13 });

            var hangTimKiem = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
            _txtTimKiem.TextChanged += (s, e) => TaiLaiDuLieu();

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

            _listBox.SelectionChanged += (s, e) => _dangChon = _listBox.SelectedItem as QuaTang;

            var khungBang = new Border { Background = Brushes.White, Height = 480, ClipToBounds = true };
            khungBang.Child = UiHelpers.TaoBang<QuaTang>(
                new List<QuaTang>(),
                new List<ColDef<QuaTang>>
                {
                    new("Mã Quà", 0.8, q => q.MaQua),
                    new("Tên Quà", 2, q => q.TenQua),
                    new("Điểm Đổi", 1, q => q.DiemQuyDoi.ToString()),
                    new("Số Lượng", 1, q => q.SoLuong.ToString())
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

        private void TaiLaiDuLieu()
        {
            var tk = _txtTimKiem.Text?.ToLower() ?? "";
            _listBox.ItemsSource = _data.DanhSachQuaTang
                .Where(q => q.TenQua.ToLower().Contains(tk) || q.MaQua.ToLower().Contains(tk))
                .ToList();
        }

        private async void BtnThem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            await HienThiPopup(new QuaTang { MaQua = _data.TaoMaQuaTangMoi() }, isMoi: true);
        }

        private async void BtnSua_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_dangChon == null)
            {
                await ThongBaoWindow.ThongBao(TopLevel.GetTopLevel(this) as Window, "Thông báo", "Vui lòng chọn 1 quà tặng cần sửa.");
                return;
            }
            await HienThiPopup(_dangChon, isMoi: false);
        }

        private async void BtnXoa_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
                TaiLaiDuLieu();
            }
        }

        private async System.Threading.Tasks.Task HienThiPopup(QuaTang qua, bool isMoi)
        {
            var popup = new Window
            {
                Title = isMoi ? "Thêm Quà Tặng" : "Sửa Quà Tặng",
                Width = 350,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var panel = new StackPanel { Spacing = 10, Margin = new Thickness(15) };
            
            var txtTenQua = new TextBox { Text = qua.TenQua, Watermark = "Tên Quà" };
            var numDiem = new NumericUpDown { Value = qua.DiemQuyDoi, Minimum = 0, FormatString = "0" };
            var numSL = new NumericUpDown { Value = qua.SoLuong, Minimum = 0, FormatString = "0" };
            
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
            panel.Children.Add(btnLuu);

            popup.Content = panel;

            btnLuu.Click += (s, ev) => 
            {
                if (string.IsNullOrWhiteSpace(txtTenQua.Text)) return;
                
                qua.TenQua = txtTenQua.Text;
                qua.DiemQuyDoi = (int)(numDiem.Value ?? 0);
                qua.SoLuong = (int)(numSL.Value ?? 0);

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
