using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Forms
{
    /// <summary>
    /// Màn hình Trang chủ: hiển thị nhanh vài con số tổng quan
    /// (tổng số khách hàng, tổng số đơn hàng, tổng doanh thu, tổng điểm hiện có)
    /// giúp người dùng nắm tình hình ngay khi mở ứng dụng.
    /// </summary>
    public class ucTrangChu : UserControl
    {
        private readonly DataService _data;

        public ucTrangChu(DataService data)
        {
            _data = data;
            BackColor = Color.FromArgb(243, 244, 246);
            XayDungGiaoDien();
        }

        private void XayDungGiaoDien()
        {
            var lblTieuDe = new Label
            {
                Text = "Tổng quan hệ thống",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10),
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            Controls.Add(lblTieuDe);

            int tongKhachHang = _data.DanhSachKhachHang.Count;
            int tongDonHang = _data.DanhSachDonHang.Count;
            decimal tongDoanhThu = _data.DanhSachDonHang.Sum(d => d.ThanhTien);
            int tongDiemHienCo = _data.DanhSachKhachHang.Sum(kh => kh.DiemTichLuy);

            var the1 = TaoTheThongTin("👤 Tổng khách hàng", tongKhachHang.ToString(), Color.FromArgb(37, 99, 235));
            var the2 = TaoTheThongTin("🧾 Tổng đơn hàng", tongDonHang.ToString(), Color.FromArgb(16, 185, 129));
            var the3 = TaoTheThongTin("💰 Tổng doanh thu", $"{tongDoanhThu:N0} đ", Color.FromArgb(234, 88, 12));
            var the4 = TaoTheThongTin("⭐ Tổng điểm hiện có", tongDiemHienCo.ToString(), Color.FromArgb(147, 51, 234));

            int x = 10, y = 60, khoangCach = 20, rong = 250;
            the1.Location = new Point(x, y);
            the2.Location = new Point(x + (rong + khoangCach), y);
            the3.Location = new Point(x + 2 * (rong + khoangCach), y);
            the4.Location = new Point(x + 3 * (rong + khoangCach), y);

            Controls.Add(the1);
            Controls.Add(the2);
            Controls.Add(the3);
            Controls.Add(the4);

            var lblGhiChu = new Label
            {
                Text = "Chọn mục trong menu bên trái để quản lý Khách hàng, tạo Đơn hàng / Tích điểm, hoặc xem Thống kê chi tiết.",
                AutoSize = true,
                Location = new Point(10, 190),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Italic)
            };
            Controls.Add(lblGhiChu);
        }

        private Panel TaoTheThongTin(string tieuDe, string giaTri, Color mau)
        {
            var panel = new Panel
            {
                Size = new Size(250, 110),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(mau, 4);
                e.Graphics.DrawLine(pen, 0, 0, 0, panel.Height); // vạch màu bên trái làm điểm nhấn
            };

            var lblTieuDe = new Label
            {
                Text = tieuDe,
                AutoSize = true,
                Location = new Point(16, 14),
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 9.5f)
            };
            var lblGiaTri = new Label
            {
                Text = giaTri,
                AutoSize = true,
                Location = new Point(16, 40),
                ForeColor = mau,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold)
            };

            panel.Controls.Add(lblTieuDe);
            panel.Controls.Add(lblGiaTri);
            return panel;
        }
    }
}
