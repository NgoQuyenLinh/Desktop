using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using QuanLyKhachHang.Services;
using SkiaSharp;

namespace QuanLyKhachHang.Views
{
    public class PhanBieuDoModel
    {
        public string TenDanhMuc { get; set; } = string.Empty;
        public double TyLe { get; set; }
        public string MaMau { get; set; } = "#2563EB";
    }

    public class BieuDoTron : UserControl
    {
        private readonly DataService? _data;
        private readonly PieChart _pieChart = new();
        private readonly StackPanel _panelChuThich = new() { Spacing = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
        private readonly TextBlock _txtTongDoanhThu = new()
        {
            Text = "0 đ",
            FontWeight = FontWeight.Bold,
            FontSize = 18,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        public BieuDoTron() : this(null) { }

        public BieuDoTron(DataService? data)
        {
            _data = data;

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

            // Đọc và render dữ liệu
            CapNhatDuLieuThucTe();
        }

        public void CapNhatDuLieuThucTe()
        {
            if (_data == null) return;

            var danhSachDon = _data.DanhSachDonHang ?? new List<Models.DonHang>();
            var danhSachThuoc = _data.DanhSachThuoc ?? new List<Models.Thuoc>();

            // 1. Tạo Tra cứu MaThuoc -> LoaiThuoc (Bỏ qua phân biệt hoa thường)
            var dicLoaiThuoc = danhSachThuoc.ToDictionary(
                t => t.MaThuoc,
                t => t.LoaiThuoc,
                StringComparer.OrdinalIgnoreCase
            );

            // 2. Khởi tạo nhóm danh mục (Bỏ qua phân biệt hoa thường để chống lỗi text)
            var doanhThuTheoGroup = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                { "Thuốc kê đơn", 0m },
                { "Thuốc không kê đơn", 0m },
                { "Thực phẩm chức năng", 0m },
                { "Thiết bị y tế", 0m },
                { "Khác", 0m } // Hứng toàn bộ tiền nếu không xác định được loại
            };

            // 3. Quét tiền từ danh sách đơn hàng
            foreach (var don in danhSachDon)
            {
                if (don.DanhSachThuoc == null) continue;

                foreach (var chiTiet in don.DanhSachThuoc)
                {
                    // Lấy số tiền (Ưu tiên ThanhTien, nếu = 0 thì lấy SoLuong * DonGia)
                    decimal tienChiTiet = chiTiet.ThanhTien > 0 
                        ? chiTiet.ThanhTien 
                        : (chiTiet.SoLuong * chiTiet.DonGia);

                    if (dicLoaiThuoc.TryGetValue(chiTiet.MaThuoc, out string? loaiThuoc) && !string.IsNullOrWhiteSpace(loaiThuoc))
                    {
                        string loaiDaTrim = loaiThuoc.Trim();
                        if (doanhThuTheoGroup.ContainsKey(loaiDaTrim))
                        {
                            doanhThuTheoGroup[loaiDaTrim] += tienChiTiet;
                        }
                        else
                        {
                            doanhThuTheoGroup["Khác"] += tienChiTiet;
                        }
                    }
                    else
                    {
                        doanhThuTheoGroup["Khác"] += tienChiTiet;
                    }
                }
            }

            decimal tongDoanhThu = doanhThuTheoGroup.Values.Sum();

            // 4. Bảng màu giao diện
            var mauGiaoDien = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Thuốc kê đơn", "#2563EB" },
                { "Thuốc không kê đơn", "#10B981" },
                { "Thực phẩm chức năng", "#F97316" },
                { "Thiết bị y tế", "#06B6D4" },
                { "Khác", "#8B5CF6" }
            };

            var danhSachModel = new List<PhanBieuDoModel>();

            foreach (var kvp in doanhThuTheoGroup)
            {
                // Chỉ hiển thị các mục có doanh thu > 0
                if (kvp.Value > 0)
                {
                    double tyLe = tongDoanhThu > 0 ? (double)Math.Round((kvp.Value / tongDoanhThu) * 100, 1) : 0;
                    
                    danhSachModel.Add(new PhanBieuDoModel
                    {
                        TenDanhMuc = kvp.Key,
                        TyLe = tyLe,
                        MaMau = mauGiaoDien.ContainsKey(kvp.Key) ? mauGiaoDien[kvp.Key] : "#CCCCCC"
                    });
                }
            }

            string chuoiDoanhThu = tongDoanhThu >= 1000000
                ? $"{tongDoanhThu / 1000:N0}K đ"
                : $"{tongDoanhThu:N0} đ";

            // Hiển thị mảng xám nếu thực sự tổng doanh thu mọi đơn hàng = 0
            if (tongDoanhThu == 0)
            {
                danhSachModel.Add(new PhanBieuDoModel { TenDanhMuc = "Chưa có dữ liệu", TyLe = 100, MaMau = "#E5E7EB" });
            }

            CapNhatGiaoDien(danhSachModel, chuoiDoanhThu);
        }

        private void CapNhatGiaoDien(List<PhanBieuDoModel> data, string tongTien)
        {
            _txtTongDoanhThu.Text = tongTien;
            _panelChuThich.Children.Clear();

            var seriesList = new List<ISeries>();

            foreach (var item in data)
            {
                seriesList.Add(new PieSeries<double>
                {
                    Values = new[] { item.TyLe },
                    Name = item.TenDanhMuc,
                    Fill = new SolidColorPaint(SKColor.Parse(item.MaMau)),
                    InnerRadius = 55,
                    HoverPushout = 5 // Hiệu ứng nổi nhẹ
                });

                if (item.TenDanhMuc != "Chưa có dữ liệu")
                {
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
            }

            _pieChart.Series = seriesList.ToArray();
        }
    }
}