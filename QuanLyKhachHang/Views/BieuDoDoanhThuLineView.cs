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
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using QuanLyKhachHang.Services;
using SkiaSharp;

namespace QuanLyKhachHang.Views
{
    public class BieuDoDoanhThuLineView : UserControl
    {
        private readonly DataService? _data;
        private readonly CartesianChart _lineChart = new();
        private readonly ComboBox _cboBoLoc = new();

        public BieuDoDoanhThuLineView() : this(null) { }

        public BieuDoDoanhThuLineView(DataService? data)
        {
            _data = data;

            var khungCard = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 10, 0, 0),
                BorderBrush = new SolidColorBrush(Color.Parse("#E5E7EB")),
                BorderThickness = new Thickness(1)
            };

            var mainStack = new StackPanel { Spacing = 16 };

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

            var txtTieuDe = new TextBlock
            {
                Text = "Biểu đồ doanh thu",
                FontWeight = FontWeight.Bold,
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.Parse("#1F2937")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtTieuDe, 0);
            headerGrid.Children.Add(txtTieuDe);

            _cboBoLoc.ItemsSource = new[] { "7 ngày qua", "14 ngày qua", "30 ngày qua" };
            _cboBoLoc.SelectedIndex = 0;
            _cboBoLoc.FontSize = 12;
            _cboBoLoc.Height = 32;
            _cboBoLoc.SelectionChanged += (s, e) => CapNhatDuLieuBieuDo();

            Grid.SetColumn(_cboBoLoc, 1);
            headerGrid.Children.Add(_cboBoLoc);

            mainStack.Children.Add(headerGrid);

            _lineChart.Height = 240;

            mainStack.Children.Add(_lineChart);
            khungCard.Child = mainStack;
            Content = khungCard;

            CapNhatDuLieuBieuDo();
        }

        /// <summary>
        /// Đọc trực tiếp danh sách đơn hàng từ DataService, gom nhóm theo ngày,
        /// ngày nào không có đơn hàng sẽ gán giá trị = 0.
        /// </summary>
        public void CapNhatDuLieuBieuDo()
        {
            var danhSachDon = _data?.DanhSachDonHang ?? new List<Models.DonHang>();

            // 1. Xác định ngày mốc làm mốc kết thúc (Lấy ngày lớn nhất trong JSON hoặc Ngày hiện tại)
            DateTime ngayMoc;
            if (danhSachDon.Count > 0)
            {
                DateTime ngayDonMoiNhat = danhSachDon.Max(d => d.NgayTao).Date;
                ngayMoc = ngayDonMoiNhat > DateTime.Now.Date ? ngayDonMoiNhat : DateTime.Now.Date;
            }
            else
            {
                ngayMoc = DateTime.Now.Date;
            }

            // 2. Xác định số ngày cần vẽ
            int soNgay = 7;
            if (_cboBoLoc.SelectedIndex == 1) soNgay = 14;
            else if (_cboBoLoc.SelectedIndex == 2) soNgay = 30;

            DateTime ngayBatDau = ngayMoc.AddDays(-(soNgay - 1));

            // 3. Gom nhóm tổng tiền đơn hàng theo từng ngày (Date)
            var doanhThuTheoNgay = danhSachDon
                .Where(d => d.NgayTao.Date >= ngayBatDau && d.NgayTao.Date <= ngayMoc)
                .GroupBy(d => d.NgayTao.Date)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.ThanhTien));

            var danhSachLabelNgay = new List<string>();
            var danhSachDoanhThu = new List<double>();

            // 4. Lặp qua từng ngày trong khoảng thời gian. Nếu ngày nào không có trong Dictionary thì gán = 0
            for (DateTime date = ngayBatDau; date <= ngayMoc; date = date.AddDays(1))
            {
                danhSachLabelNgay.Add(date.ToString("dd/MM"));

                if (doanhThuTheoNgay.TryGetValue(date, out decimal tongTienInDay))
                {
                    danhSachDoanhThu.Add((double)tongTienInDay);
                }
                else
                {
                    danhSachDoanhThu.Add(0); // Ngày không có đơn hàng thì gán = 0
                }
            }

            // --- TRỤC X ---
            _lineChart.XAxes = new[]
            {
                new Axis
                {
                    Labels = danhSachLabelNgay.ToArray(),
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#9CA3AF")),
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#F3F4F6"))
                }
            };

            // --- TRỤC Y ---
            _lineChart.YAxes = new[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(new SKColor(0, 0, 0, 0)),
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB"))
                    {
                        StrokeThickness = 1,
                        PathEffect = new DashEffect(new float[] { 4f, 4f })
                    }
                }
            };

            // --- VẼ ĐƯỜNG BIỂU ĐỒ ---
            _lineChart.Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = danhSachDoanhThu.ToArray(),
                    Name = "Doanh thu (đ)",
                    Stroke = new SolidColorPaint(SKColor.Parse("#2563EB")) { StrokeThickness = 2.5f },
                    Fill = new SolidColorPaint(SKColor.Parse("#EFF6FF")),
                    GeometrySize = 8,
                    GeometryFill = new SolidColorPaint(SKColor.Parse("#FFFFFF")),
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#2563EB")) { StrokeThickness = 2.5f },
                    LineSmoothness = 0.2
                }
            };
        }
    }
}