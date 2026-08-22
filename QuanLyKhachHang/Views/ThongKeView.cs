using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using QuanLyKhachHang.Helpers;
using QuanLyKhachHang.Models;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Views
{
    /// <summary>
    /// Màn hình Thống kê:
    ///  - Phần trên: Top quà đã phát / tặng nhiều nhất theo Ngày / Tháng / Năm.
    ///  - Phần dưới: Lịch sử mua hàng của tất cả khách hàng, có sắp xếp và xóa đơn.
    /// </summary>
    public class ThongKeView : UserControl
    {
        private readonly DataService _data;

        // ---- Top quà ----
        private readonly RadioButton _rbNgay = new()
        {
            Content = "Ngày",
            GroupName = "kyThongKeQua",
            IsChecked = true
        };

        private readonly RadioButton _rbThang = new()
        {
            Content = "Tháng",
            GroupName = "kyThongKeQua"
        };

        private readonly RadioButton _rbNam = new()
        {
            Content = "Năm",
            GroupName = "kyThongKeQua"
        };

        private readonly ListBox _listBoxTopQua = new();

        // ---- Lịch sử tất cả đơn hàng ----
        private readonly ComboBox _cboSapXep = new() { Width = 200 };
        private readonly ListBox _listBoxDon = new();
        private readonly TextBlock _lblTrong = new()
        {
            Text = "Chưa có đơn hàng",
            FontSize = 14,
            Foreground = Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsVisible = false
        };

        private DonHang? _donDangChon;

        private record QuaXepHang(
            int Hang,
            string MaQua,
            string TenQua,
            int SoLanTang,
            int TongDiemDoi);

        public ThongKeView(DataService data)
        {
            _data = data;
            Background = new SolidColorBrush(Color.Parse("#F4F7FB"));

            var content = new StackPanel
            {
                Margin = new Thickness(18),
                Spacing = 14
            };

            content.Children.Add(new TextBlock
            {
                Text = "Thống kê",
                FontSize = 26,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#1F2937"))
            });

            // ================= TOP QUÀ =================
            var topCard = Card("🎁  Top quà đã phát / tặng nhiều nhất");
            var topStack = new StackPanel { Spacing = 10 };

            var hangTieuDeQua = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 15,
                VerticalAlignment = VerticalAlignment.Center
            };

            hangTieuDeQua.Children.Add(new TextBlock
            {
                Text = "Trong:",
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 13,
                Foreground = Brushes.DimGray
            });
            hangTieuDeQua.Children.Add(_rbNgay);
            hangTieuDeQua.Children.Add(_rbThang);
            hangTieuDeQua.Children.Add(_rbNam);
            topStack.Children.Add(hangTieuDeQua);

            _rbNgay.IsCheckedChanged += (_, _) => CapNhatTopQua();
            _rbThang.IsCheckedChanged += (_, _) => CapNhatTopQua();
            _rbNam.IsCheckedChanged += (_, _) => CapNhatTopQua();

            var khungBangQua = new Border
            {
                Height = 260,
                BorderBrush = new SolidColorBrush(Color.Parse("#E5E7EB")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Child = UiHelpers.TaoBang<QuaXepHang>(
                    new List<QuaXepHang>(),
                    new List<ColDef<QuaXepHang>>
                    {
                        new("Hạng", 0.5, x => x.Hang.ToString()),
                        new("Mã quà", 0.8, x => x.MaQua),
                        new("Tên quà", 1.8, x => x.TenQua),
                        new("Số lần tặng", 1, x => x.SoLanTang.ToString()),
                        new("Tổng điểm đổi", 1, x => x.TongDiemDoi.ToString())
                    },
                    _listBoxTopQua)
            };
            topStack.Children.Add(khungBangQua);
            ((StackPanel)topCard.Child!).Children.Add(topStack);
            content.Children.Add(topCard);

            // ================= LỊCH SỬ MUA HÀNG CỦA TẤT CẢ KHÁCH HÀNG =================
            var historyCard = Card("🕘  Lịch sử mua hàng của tất cả khách hàng");
            var historyStack = new StackPanel { Spacing = 10 };

            var hangDieuKhien = new DockPanel
            {
                Margin = new Thickness(0, 0, 0, 4)
            };

            var leftControls = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                VerticalAlignment = VerticalAlignment.Center
            };

            leftControls.Children.Add(new TextBlock
            {
                Text = "↕ Sắp xếp:",
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 13,
                Foreground = Brushes.DimGray
            });

            _cboSapXep.ItemsSource = new[]
            {
                "Mới nhất (Ngày)",
                "Cũ nhất (Ngày)",
                "Tên khách hàng (A-Z)",
                "Số tiền (Cao - Thấp)",
                "Số điểm đã dùng"
            };
            _cboSapXep.SelectedIndex = 0;
            _cboSapXep.SelectionChanged += (_, _) => TaiLaiDuLieuDon();
            leftControls.Children.Add(_cboSapXep);

            var btnXoaDon = new Button
            {
                Content = "🗑 Xóa đơn đã chọn",
                Background = new SolidColorBrush(Color.Parse("#DC2626")),
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            btnXoaDon.Click += BtnXoaDon_Click;

            DockPanel.SetDock(btnXoaDon, Dock.Right);
            hangDieuKhien.Children.Add(btnXoaDon);
            hangDieuKhien.Children.Add(leftControls);
            historyStack.Children.Add(hangDieuKhien);

            _listBoxDon.SelectionChanged += (_, _) =>
                _donDangChon = _listBoxDon.SelectedItem as DonHang;

            var gridBang = new Grid();
            var bang = UiHelpers.TaoBang<DonHang>(
                new List<DonHang>(),
                new List<ColDef<DonHang>>
                {
                    new("Mã đơn", 0.8, d => d.MaDon),
                    new("Khách hàng", 1.5, d => d.TenKH),
                    new("Tổng tiền", 1.1, d => $"{d.SoTien:N0} đ"),
                    new("Điểm cộng", 0.9, d => d.DiemCong.ToString()),
                    new("Quà đã đổi", 1.3, d => string.IsNullOrEmpty(d.QuaTangDoi) ? "-" : d.QuaTangDoi),
                    new("Điểm dùng", 0.9, d => d.DiemSuDung.ToString()),
                    new("Ngày tạo", 1.3, d => d.NgayTao.ToString("dd/MM/yyyy HH:mm"))
                },
                _listBoxDon);

            gridBang.Children.Add(bang);
            gridBang.Children.Add(_lblTrong);

            historyStack.Children.Add(new Border
            {
                Height = 340,
                BorderBrush = new SolidColorBrush(Color.Parse("#E5E7EB")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Child = gridBang
            });

            ((StackPanel)historyCard.Child!).Children.Add(historyStack);
            content.Children.Add(historyCard);

            Content = new ScrollViewer
            {
                Content = content,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            CapNhatTopQua();
            TaiLaiDuLieuDon();
        }

        private Border Card(string title)
        {
            var card = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.Parse("#DDE5F0")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(9),
                Padding = new Thickness(14)
            };

            var stack = new StackPanel { Spacing = 8 };
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#1F3B63"))
            });
            card.Child = stack;
            return card;
        }

        private void CapNhatTopQua()
        {
            DateTime hienTai = DateTime.Now;
            DateTime tu;
            DateTime den;

            if (_rbThang.IsChecked == true)
            {
                tu = new DateTime(hienTai.Year, hienTai.Month, 1);
                den = tu.AddMonths(1).AddTicks(-1);
            }
            else if (_rbNam.IsChecked == true)
            {
                tu = new DateTime(hienTai.Year, 1, 1);
                den = tu.AddYears(1).AddTicks(-1);
            }
            else
            {
                tu = hienTai.Date;
                den = tu.AddDays(1).AddTicks(-1);
            }

            var topQua = _data.DanhSachDonHang
                .Where(d => d.NgayTao >= tu && d.NgayTao <= den && !string.IsNullOrEmpty(d.QuaTangDoi))
                .GroupBy(d => d.QuaTangDoi)
                .Select(g => new
                {
                    TenQua = g.Key,
                    SoLan = g.Count(),
                    TongDiem = g.Sum(d => d.DiemDoiQua)
                })
                .OrderByDescending(x => x.SoLan)
                .ThenByDescending(x => x.TongDiem)
                .Take(10)
                .ToList();

            var ketQua = new List<QuaXepHang>();
            for (int i = 0; i < topQua.Count; i++)
            {
                var x = topQua[i];
                var maQua = _data.DanhSachQuaTang
                    .FirstOrDefault(q => q.TenQua == x.TenQua)?.MaQua ?? "-";

                ketQua.Add(new QuaXepHang(
                    i + 1,
                    maQua,
                    x.TenQua ?? "-",
                    x.SoLan,
                    x.TongDiem));
            }

            _listBoxTopQua.ItemsSource = null;
            _listBoxTopQua.ItemsSource = ketQua;
        }

        private void TaiLaiDuLieuDon()
        {
            // Hiển thị toàn bộ đơn hàng của tất cả khách hàng.
            IEnumerable<DonHang> danhSach = _data.DanhSachDonHang;

            switch (_cboSapXep.SelectedIndex)
            {
                case 0:
                    danhSach = danhSach.OrderByDescending(d => d.NgayTao);
                    break;
                case 1:
                    danhSach = danhSach.OrderBy(d => d.NgayTao);
                    break;
                case 2:
                    danhSach = danhSach.OrderBy(d => d.TenKH);
                    break;
                case 3:
                    danhSach = danhSach.OrderByDescending(d => d.SoTien);
                    break;
                case 4:
                    danhSach = danhSach.OrderByDescending(d => d.DiemSuDung);
                    break;
                default:
                    danhSach = danhSach.OrderByDescending(d => d.NgayTao);
                    break;
            }

            var dsKetQua = danhSach.ToList();
            _donDangChon = null;
            _lblTrong.IsVisible = dsKetQua.Count == 0;

            _listBoxDon.ItemsSource = null;
            _listBoxDon.ItemsSource = dsKetQua;
        }

        private async void BtnXoaDon_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var parent = TopLevel.GetTopLevel(this) as Window;

            if (_donDangChon == null)
            {
                await ThongBaoWindow.ThongBao(parent, "Chưa chọn đơn hàng", "Vui lòng chọn một đơn hàng cần xóa.");
                return;
            }

            bool dongY = await ThongBaoWindow.XacNhan(
                parent,
                "Xác nhận xóa",
                $"Bạn có chắc muốn xóa đơn {_donDangChon.MaDon} của khách hàng {_donDangChon.TenKH}?");

            if (!dongY)
                return;

            string maDon = _donDangChon.MaDon;
            if (_data.XoaDonHang(maDon))
            {
                await ThongBaoWindow.ThongBao(parent, "Thành công", "Đã xóa đơn hàng.");
                TaiLaiDuLieuDon();
                CapNhatTopQua();
            }
            else
            {
                await ThongBaoWindow.ThongBao(parent, "Không thể xóa", "Không tìm thấy đơn hàng cần xóa.");
            }
        }
    }
}