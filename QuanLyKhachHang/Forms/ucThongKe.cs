using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Forms
{
    /// <summary>
    /// Màn hình Thống kê: cho phép chọn khoảng thời gian (từ ngày -> đến ngày),
    /// hiển thị các chỉ số tổng hợp (số đơn, doanh thu, điểm cộng/dùng trong khoảng đó)
    /// và bảng Top khách hàng có điểm tích luỹ cao nhất (tính trên toàn bộ dữ liệu hiện có).
    /// </summary>
    public class ucThongKe : UserControl
    {
        private readonly DataService _data;
        private readonly DateTimePicker _dtpTu = new() { Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker _dtpDen = new() { Format = DateTimePickerFormat.Short };

        private readonly Label _lblSoDon = new();
        private readonly Label _lblDoanhThu = new();
        private readonly Label _lblDiemCong = new();
        private readonly Label _lblDiemDung = new();

        private readonly DataGridView _gridTop = new();

        public ucThongKe(DataService data)
        {
            _data = data;
            BackColor = Color.FromArgb(243, 244, 246);
            XayDungGiaoDien();
            ThongKe();
        }

        private void XayDungGiaoDien()
        {
            var lblTieuDe = new Label
            {
                Text = "Thống kê",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            Controls.Add(lblTieuDe);

            var lblTu = new Label { Text = "Từ ngày:", AutoSize = true, Location = new Point(10, 55) };
            _dtpTu.Location = new Point(70, 50);
            _dtpTu.Width = 130;
            _dtpTu.Value = _data.DanhSachDonHang.Count > 0
                ? _data.DanhSachDonHang.Min(d => d.NgayTao)
                : DateTime.Now.AddMonths(-1);

            var lblDen = new Label { Text = "Đến ngày:", AutoSize = true, Location = new Point(220, 55) };
            _dtpDen.Location = new Point(285, 50);
            _dtpDen.Width = 130;
            _dtpDen.Value = DateTime.Now;

            var btnLoc = new Button
            {
                Text = "🔎 Xem thống kê",
                Location = new Point(430, 48),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLoc.Click += (s, e) => ThongKe();

            Controls.Add(lblTu);
            Controls.Add(_dtpTu);
            Controls.Add(lblDen);
            Controls.Add(_dtpDen);
            Controls.Add(btnLoc);

            var the1 = TaoThe("🧾 Số đơn hàng", _lblSoDon, Color.FromArgb(37, 99, 235));
            var the2 = TaoThe("💰 Doanh thu", _lblDoanhThu, Color.FromArgb(234, 88, 12));
            var the3 = TaoThe("⭐ Điểm đã cộng", _lblDiemCong, Color.FromArgb(16, 185, 129));
            var the4 = TaoThe("🎁 Điểm đã dùng", _lblDiemDung, Color.FromArgb(147, 51, 234));

            int x = 10, y = 100, khoangCach = 20, rong = 225;
            the1.Location = new Point(x, y);
            the2.Location = new Point(x + (rong + khoangCach), y);
            the3.Location = new Point(x + 2 * (rong + khoangCach), y);
            the4.Location = new Point(x + 3 * (rong + khoangCach), y);
            Controls.Add(the1);
            Controls.Add(the2);
            Controls.Add(the3);
            Controls.Add(the4);

            var lblTop = new Label
            {
                Text = "🏆 Top khách hàng có điểm tích luỹ cao nhất",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 225)
            };
            Controls.Add(lblTop);

            _gridTop.Location = new Point(10, 255);
            _gridTop.Size = new Size(950, 300);
            _gridTop.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _gridTop.ReadOnly = true;
            _gridTop.AllowUserToAddRows = false;
            _gridTop.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _gridTop.BackgroundColor = Color.White;
            _gridTop.RowHeadersVisible = false;
            Controls.Add(_gridTop);
        }

        private Panel TaoThe(string tieuDe, Label lblGiaTri, Color mau)
        {
            var panel = new Panel { Size = new Size(225, 90), BackColor = Color.White };
            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(mau, 4);
                e.Graphics.DrawLine(pen, 0, 0, 0, panel.Height);
            };

            var lbl = new Label { Text = tieuDe, AutoSize = true, Location = new Point(14, 12), ForeColor = Color.DimGray };
            lblGiaTri.AutoSize = true;
            lblGiaTri.Location = new Point(14, 36);
            lblGiaTri.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
            lblGiaTri.ForeColor = mau;

            panel.Controls.Add(lbl);
            panel.Controls.Add(lblGiaTri);
            return panel;
        }

        private void ThongKe()
        {
            DateTime tuNgay = _dtpTu.Value.Date;
            DateTime denNgay = _dtpDen.Value.Date;

            if (tuNgay > denNgay)
            {
                MessageBox.Show("'Từ ngày' phải nhỏ hơn hoặc bằng 'Đến ngày'.", "Khoảng thời gian không hợp lệ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _lblSoDon.Text = _data.SoLuongDonHang(tuNgay, denNgay).ToString();
            _lblDoanhThu.Text = $"{_data.TongDoanhThu(tuNgay, denNgay):N0} đ";
            _lblDiemCong.Text = _data.TongDiemDaTichLuy(tuNgay, denNgay).ToString();
            _lblDiemDung.Text = _data.TongDiemDaSuDung(tuNgay, denNgay).ToString();

            _gridTop.DataSource = null;
            _gridTop.DataSource = _data.TopKhachHangDiemCao(10)
                .Select((kh, idx) => new
                {
                    Hang = idx + 1,
                    kh.MaKH,
                    kh.HoTen,
                    kh.SoDienThoai,
                    DiemTichLuy = kh.DiemTichLuy
                }).ToList();

            if (_gridTop.Columns.Count > 0)
            {
                _gridTop.Columns["Hang"].HeaderText = "Hạng";
                _gridTop.Columns["MaKH"].HeaderText = "Mã KH";
                _gridTop.Columns["HoTen"].HeaderText = "Họ tên";
                _gridTop.Columns["SoDienThoai"].HeaderText = "Số điện thoại";
                _gridTop.Columns["DiemTichLuy"].HeaderText = "Điểm tích luỹ";
            }
        }
    }
}
