using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects; 
using SkiaSharp;

namespace QuanLyKhachHang.Views
{
    public class BieuDoDoanhThuLineView : UserControl
    {
        private readonly CartesianChart _lineChart = new();

        public BieuDoDoanhThuLineView()
        {
            // 1. Khung card bên ngoài giao diện
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

            // 2. Tiêu đề và nút "7 ngày qua"
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

            var btnBoLoc = new Border
            {
                Background = Brushes.Transparent,
                BorderBrush = new SolidColorBrush(Color.Parse("#D1D5DB")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 6),
                Child = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        new TextBlock { Text = "7 ngày qua", FontSize = 12, Foreground = new SolidColorBrush(Color.Parse("#374151")), VerticalAlignment = VerticalAlignment.Center },
                        new TextBlock { Text = "▼", FontSize = 10, Foreground = new SolidColorBrush(Color.Parse("#6B7280")), VerticalAlignment = VerticalAlignment.Center }
                    }
                }
            };
            Grid.SetColumn(btnBoLoc, 1);
            headerGrid.Children.Add(btnBoLoc);

            mainStack.Children.Add(headerGrid);

            // 3. Cấu hình Biểu đồ Đường
            _lineChart.Height = 240;

            // --- TRỤC X (NGÀY THÁNG) ---
            _lineChart.XAxes = new[]
            {
                new Axis
                {
                    // DỮ LIỆU CỨNG: Các mốc ngày (Thay đổi tùy ý)
                    Labels = new[] { "12/08", "13/08", "14/08", "15/08", "16/08", "17/08", "18/08" },
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#9CA3AF")),
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#F3F4F6"))
                }
            };

            // --- TRỤC Y (MỨC DOANH THU) ---
            _lineChart.YAxes = new[]
            {
                new Axis
                {
                    // Ẩn số liệu bên trái bằng cách để màu chữ trong suốt hoàn toàn
                    LabelsPaint = new SolidColorPaint(new SKColor(0, 0, 0, 0)), 
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")) 
                    { 
                        StrokeThickness = 1,
                        // Sử dụng DashEffect chuẩn của LiveCharts2 cho nét đứt ngang
                        PathEffect = new DashEffect(new float[] { 4f, 4f })
                    }
                }
            };

            // --- DỮ LIỆU CỨNG CHO ĐƯỜNG BIỂU ĐỒ ---
            _lineChart.Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    // DỮ LIỆU CỨNG: Số liệu doanh thu tương ứng qua các ngày
                    Values = new double[] { 4.5, 6.2, 8.0, 5.1, 7.0, 7.0, 9.5 },
                    Name = "Doanh thu",
                    
                    // Màu sắc đường line (Xanh dương đậm)
                    Stroke = new SolidColorPaint(SKColor.Parse("#2563EB")) { StrokeThickness = 2.5f },
                    
                    // Màu nền mờ bên dưới đường biểu đồ
                    Fill = new SolidColorPaint(SKColor.Parse("#EFF6FF")), 
                    
                    // Các chấm tròn tại các điểm mốc dữ liệu
                    GeometrySize = 8,
                    GeometryFill = new SolidColorPaint(SKColor.Parse("#FFFFFF")),
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#2563EB")) { StrokeThickness = 2.5f },
                    
                    // Độ mềm mại, uốn cong của đường nối
                    LineSmoothness = 0.2 
                }
            };

            mainStack.Children.Add(_lineChart);
            khungCard.Child = mainStack;
            Content = khungCard;
        }
    }
}