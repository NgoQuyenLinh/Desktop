using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Forms
{
    /// <summary>
    /// Form chính của ứng dụng: chứa 1 sidebar bên trái để điều hướng
    /// và 1 vùng nội dung bên phải (Panel pnlContent) để hiển thị từng
    /// màn hình (UserControl) tương ứng: Trang chủ, Khách hàng, Đơn hàng, Thống kê.
    /// Toàn bộ dữ liệu dùng chung được nạp 1 lần vào DataService và truyền
    /// xuống cho các UserControl con, tránh đọc file lặp lại nhiều lần.
    /// </summary>
    public class MainForm : Form
    {
        private readonly DataService _dataService = new();
        private readonly Panel _pnlSidebar = new();
        private readonly Panel _pnlContent = new();
        private readonly Label _lblTieuDe = new();

        private Button? _btnDangChon;

        public MainForm()
        {
            Text = "Phần mềm Quản lý Khách hàng & Tích điểm";
            Width = 1200;
            Height = 720;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f);
            MinimumSize = new Size(1000, 640);

            XayDungSidebar();
            XayDungContent();

            Controls.Add(_pnlContent);
            Controls.Add(_pnlSidebar);

            HienThi(new ucTrangChu(_dataService), null);
        }

        private void XayDungSidebar()
        {
            _pnlSidebar.Dock = DockStyle.Left;
            _pnlSidebar.Width = 210;
            _pnlSidebar.BackColor = Color.FromArgb(31, 41, 55); // xám than hiện đại

            _lblTieuDe.Text = "🏬 QLKH & Tích điểm";
            _lblTieuDe.ForeColor = Color.White;
            _lblTieuDe.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            _lblTieuDe.Dock = DockStyle.Top;
            _lblTieuDe.Height = 70;
            _lblTieuDe.TextAlign = ContentAlignment.MiddleCenter;
            _pnlSidebar.Controls.Add(_lblTieuDe);

            var btnTrangChu = TaoNutMenu("🏠  Trang chủ");
            var btnKhachHang = TaoNutMenu("👤  Khách hàng");
            var btnDonHang = TaoNutMenu("🧾  Đơn hàng / Tích điểm");
            var btnThongKe = TaoNutMenu("📊  Thống kê");
            var btnThoat = TaoNutMenu("🚪  Thoát");

            btnTrangChu.Click += (s, e) => HienThi(new ucTrangChu(_dataService), btnTrangChu);
            btnKhachHang.Click += (s, e) => HienThi(new ucKhachHang(_dataService), btnKhachHang);
            btnDonHang.Click += (s, e) => HienThi(new ucDonHang(_dataService), btnDonHang);
            btnThongKe.Click += (s, e) => HienThi(new ucThongKe(_dataService), btnThongKe);
            btnThoat.Click += (s, e) => Close();

            // Thêm theo thứ tự ngược vì Dock = Top xếp chồng từ trên xuống
            var danhSachNut = new[] { btnTrangChu, btnKhachHang, btnDonHang, btnThongKe, btnThoat };
            for (int i = danhSachNut.Length - 1; i >= 0; i--)
                _pnlSidebar.Controls.Add(danhSachNut[i]);

            _btnDangChon = btnTrangChu;
            DanhDauNutDangChon(btnTrangChu);
        }

        private Button TaoNutMenu(string text)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 48,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 0, 0),
                ForeColor = Color.Gainsboro,
                BackColor = Color.FromArgb(31, 41, 55),
                Font = new Font("Segoe UI", 10f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 65, 81);
            return btn;
        }

        private void XayDungContent()
        {
            _pnlContent.Dock = DockStyle.Fill;
            _pnlContent.BackColor = Color.FromArgb(243, 244, 246);
            _pnlContent.Padding = new Padding(20);
        }

        private void HienThi(UserControl man, Button? nutNguon)
        {
            _pnlContent.Controls.Clear();
            man.Dock = DockStyle.Fill;
            _pnlContent.Controls.Add(man);

            if (nutNguon != null)
                DanhDauNutDangChon(nutNguon);
        }

        private void DanhDauNutDangChon(Button nut)
        {
            if (_btnDangChon != null)
                _btnDangChon.BackColor = Color.FromArgb(31, 41, 55);

            nut.BackColor = Color.FromArgb(37, 99, 235); // xanh dương nổi bật cho mục đang chọn
            _btnDangChon = nut;
        }
    }
}
