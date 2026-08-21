using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace QuanLyKhachHang.Views
{
    public class PhanBieuDoModel
    {
        public string TenDanhMuc { get; set; } = string.Empty;
        public double TyLe { get; set; }
        public string MaMau { get; set; } = "#2563EB";
    }

    public class BieuDoDoanhThuView : UserControl
    {
        private readonly PieChart _pieChart = new();
        private readonly StackPanel _panelChuThich = new() { Spacing = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
        private readonly TextBlock _txtTongDoanhThu = new() 
        { 
            Text = "12,450K đ", 
            FontWeight = FontWeight.Bold, 
            FontSize = 18,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        public BieuDoDoanhThuView()
        {
            var khungBieuDo = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 10, 0, 0),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };

            var panelBieuDoChinh = new StackPanel { Spacing = 14 };
            panelBieuDoChinh.Children.Add(new TextBlock { Text = "Doanh thu theo danh mục", FontWeight = FontWeight.Bold, FontSize = 16 });

            // Cấu hình Grid chia thành 2 cột (Cột trái: Biểu đồ, Cột phải: Chú thích %)
            var gridBieuDo = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("260, *"),
                VerticalAlignment = VerticalAlignment.Center
            };

            // --- CỘT TRÁI: Biểu đồ Donut ---
            _pieChart.Height = 200;
            _pieChart.IsClockwise = true;
            
            var panelTrai = new Grid();
            panelTrai.Children.Add(_pieChart);
            
            // Chữ tổng doanh thu ở tâm
            var panelTam = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 2,
                IsHitTestVisible = false
            };
            panelTam.Children.Add(_txtTongDoanhThu);
            panelTam.Children.Add(new TextBlock 
            { 
                Text = "DOANH THU", 
                FontSize = 10, 
                FontWeight = FontWeight.Bold, 
                Foreground = Brushes.Gray, 
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
            
            panelTrai.Children.Add(panelTam);
            Grid.SetColumn(panelTrai, 0);
            gridBieuDo.Children.Add(panelTrai);

            // --- CỘT PHẢI: Chú thích tỉ lệ % ---
            Grid.SetColumn(_panelChuThich, 1);
            gridBieuDo.Children.Add(_panelChuThich);

            panelBieuDoChinh.Children.Add(gridBieuDo);
            khungBieuDo.Child = panelBieuDoChinh;
            Content = khungBieuDo;

            LoadDuLieuMau();
        }

        public void LoadDuLieuMau()
        {
            var danhSach = new List<PhanBieuDoModel>
            {
                new() { TenDanhMuc = "Thuốc kê đơn", TyLe = 45.0, MaMau = "#2563EB" },
                new() { TenDanhMuc = "Thuốc không kê đơn", TyLe = 30.0, MaMau = "#10B981" },
                new() { TenDanhMuc = "Thực phẩm chức năng", TyLe = 15.0, MaMau = "#F97316" },
                new() { TenDanhMuc = "Thiết bị y tế", TyLe = 10.0, MaMau = "#06B6D4" }
            };

            CapNhatDuLieu(danhSach, "12,450K đ");
        }

        public void CapNhatDuLieu(List<PhanBieuDoModel> data, string tongTien)
        {
            _txtTongDoanhThu.Text = tongTien;
            _panelChuThich.Children.Clear();

            var seriesList = new List<ISeries>();

            foreach (var item in data)
            {
                // 1. Thêm vào biểu đồ Donut
                seriesList.Add(new PieSeries<double>
                {
                    Values = new[] { item.TyLe },
                    Name = item.TenDanhMuc,
                    Fill = new SolidColorPaint(SKColor.Parse(item.MaMau)),
                    InnerRadius = 55
                });

                // 2. Tạo dòng chú thích bên cột phải (chấm màu, tên danh mục, tỷ lệ %)
                var hang = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Margin = new Thickness(0, 4, 0, 4) };
                
                hang.Children.Add(new Border 
                { 
                    Width = 12, 
                    Height = 12, 
                    CornerRadius = new CornerRadius(6), 
                    Background = new SolidColorBrush(Color.Parse(item.MaMau)),
                    VerticalAlignment = VerticalAlignment.Center 
                });

                hang.Children.Add(new TextBlock 
                { 
                    Text = item.TenDanhMuc, 
                    FontSize = 14, 
                    Foreground = new SolidColorBrush(Color.Parse("#4B5563")),
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 170
                });

                hang.Children.Add(new TextBlock 
                { 
                    Text = $"{item.TyLe}%", 
                    FontSize = 15, 
                    FontWeight = FontWeight.Bold, 
                    Foreground = new SolidColorBrush(Color.Parse("#1F2937")),
                    VerticalAlignment = VerticalAlignment.Center 
                });

                _panelChuThich.Children.Add(hang);
            }

            _pieChart.Series = seriesList.ToArray();
        }
    }
}