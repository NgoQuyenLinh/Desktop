using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using QuanLyKhachHang.Models;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Views
{
    public class GhiChuView : UserControl
    {
        private readonly DataService _dataService;
        private readonly StackPanel _stackDanhSach;
        private readonly TextBox _txtNhapMoi;
        private List<GhiChu> _danhSachGhiChu;

        public GhiChuView(DataService dataService)
        {
            _dataService = dataService;
            _danhSachGhiChu = _dataService.LayDanhSachGhiChu();

            // Khung card bên ngoài
            var card = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                BorderBrush = new SolidColorBrush(Color.Parse("#E5E7EB")),
                BorderThickness = new Thickness(1),
                Width = 320 // Kích thước gọn gàng để đặt cạnh các phần khác
            };

            var mainLayout = new StackPanel { Spacing = 12 };

            // Tiêu đề
            mainLayout.Children.Add(new TextBlock
            {
                Text = "Ghi chú / Nhắc nhở",
                FontWeight = FontWeight.Bold,
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.Parse("#1F2937"))
            });

            // Danh sách các mục ghi chú
            _stackDanhSach = new StackPanel { Spacing = 8 };
            HienThiDanhSach();
            mainLayout.Children.Add(_stackDanhSach);

            // Khu vực nhập ghi chú mới nhanh
            var panelThem = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Margin = new Thickness(0, 8, 0, 0) };
            _txtNhapMoi = new TextBox
            {
                Watermark = "Nhập ghi chú mới...",
                Width = 220,
                Height = 32,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            var btnThem = new Button
            {
                Content = "+ Thêm",
                Height = 32,
                Background = new SolidColorBrush(Color.Parse("#2563EB")),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(4)
            };
            btnThem.Click += (s, e) => ThemGhiChuMoi();

            panelThem.Children.Add(_txtNhapMoi);
            panelThem.Children.Add(btnThem);
            mainLayout.Children.Add(panelThem);

            card.Child = mainLayout;
            Content = card;
        }

        private void HienThiDanhSach()
        {
            _stackDanhSach.Children.Clear();
            foreach (var item in _danhSachGhiChu)
            {
                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // Chấm tròn màu + Checkbox
                row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star)); // Nội dung
                row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // Nút xóa

                // Checkbox để đánh dấu hoàn thành
                var chk = new CheckBox
                {
                    IsChecked = item.DaHoanThanh,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 8, 0)
                };

                // Nội dung ghi chú (Gạch ngang nếu hoàn thành)
                var txtNoiDung = new TextBlock
                {
                    Text = item.NoiDung,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.Parse(item.DaHoanThanh ? "#9CA3AF" : "#374151")),
                    TextDecorations = item.DaHoanThanh ? TextDecorations.Strikethrough : null
                };

                // Xử lý sự kiện khi bấm tích hoàn thành
                chk.IsCheckedChanged += (s, e) =>
                {
                    item.DaHoanThanh = chk.IsChecked ?? false;
                    txtNoiDung.Foreground = new SolidColorBrush(Color.Parse(item.DaHoanThanh ? "#9CA3AF" : "#374151"));
                    txtNoiDung.TextDecorations = item.DaHoanThanh ? TextDecorations.Strikethrough : null;
                    _dataService.LuuDanhSachGhiChu(_danhSachGhiChu);
                };

                // Nút xóa ghi chú
                var btnXoa = new Button
                {
                    Content = "✕",
                    Background = Brushes.Transparent,
                    Foreground = new SolidColorBrush(Color.Parse("#EF4444")),
                    BorderThickness = new Thickness(0),
                    Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
                };
                btnXoa.Click += (s, e) =>
                {
                    _danhSachGhiChu.Remove(item);
                    _dataService.LuuDanhSachGhiChu(_danhSachGhiChu);
                    HienThiDanhSach(); // Vẽ lại giao diện
                };

                Grid.SetColumn(chk, 0);
                Grid.SetColumn(txtNoiDung, 1);
                Grid.SetColumn(btnXoa, 2);

                row.Children.Add(chk);
                row.Children.Add(txtNoiDung);
                row.Children.Add(btnXoa);

                _stackDanhSach.Children.Add(row);
            }
        }

        private void ThemGhiChuMoi()
        {
            if (string.IsNullOrWhiteSpace(_txtNhapMoi.Text)) return;

            var newItem = new GhiChu
            {
                NoiDung = _txtNhapMoi.Text.Trim(),
                DaHoanThanh = false,
                MauSac = "#2563EB"
            };

            _danhSachGhiChu.Add(newItem);
            _dataService.LuuDanhSachGhiChu(_danhSachGhiChu);

            _txtNhapMoi.Text = string.Empty;
            HienThiDanhSach();
        }
    }
}