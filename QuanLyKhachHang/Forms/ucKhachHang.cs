using QuanLyKhachHang.Models;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Forms
{
    /// <summary>
    /// Màn hình Quản lý khách hàng: hiển thị danh sách bằng DataGridView,
    /// có ô tìm kiếm tức thời (gõ tới đâu lọc tới đó) và các nút Thêm / Sửa / Xoá.
    /// </summary>
    public class ucKhachHang : UserControl
    {
        private readonly DataService _data;
        private readonly DataGridView _grid = new();
        private readonly TextBox _txtTimKiem = new();

        public ucKhachHang(DataService data)
        {
            _data = data;
            BackColor = Color.FromArgb(243, 244, 246);
            XayDungGiaoDien();
            TaiLaiDuLieu();
        }

        private void XayDungGiaoDien()
        {
            var lblTieuDe = new Label
            {
                Text = "Quản lý khách hàng",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            Controls.Add(lblTieuDe);

            var lblTimKiem = new Label { Text = "🔍 Tìm kiếm (theo tên / SĐT / mã KH):", AutoSize = true, Location = new Point(10, 50) };
            _txtTimKiem.Location = new Point(10, 72);
            _txtTimKiem.Width = 320;
            _txtTimKiem.TextChanged += (s, e) => TaiLaiDuLieu(); // tìm kiếm tức thời khi gõ
            Controls.Add(lblTimKiem);
            Controls.Add(_txtTimKiem);

            var btnThem = TaoNut("➕ Thêm", 350, 68, Color.FromArgb(37, 99, 235));
            var btnSua = TaoNut("✏️ Sửa", 460, 68, Color.FromArgb(234, 179, 8));
            var btnXoa = TaoNut("🗑️ Xoá", 570, 68, Color.FromArgb(220, 38, 38));
            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;
            Controls.Add(btnThem);
            Controls.Add(btnSua);
            Controls.Add(btnXoa);

            _grid.Location = new Point(10, 115);
            _grid.Size = new Size(950, 480);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.BackgroundColor = Color.White;
            _grid.BorderStyle = BorderStyle.None;
            _grid.RowHeadersVisible = false;
            Controls.Add(_grid);
        }

        private Button TaoNut(string text, int x, int y, Color mau)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(100, 34),
                BackColor = mau,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        private void TaiLaiDuLieu()
        {
            var ketQua = _data.TimKiemKhachHang(_txtTimKiem.Text);

            _grid.DataSource = null;
            _grid.DataSource = ketQua.Select(kh => new
            {
                kh.MaKH,
                kh.HoTen,
                kh.SoDienThoai,
                DiemTichLuy = kh.DiemTichLuy,
                NgayTao = kh.NgayTao.ToString("dd/MM/yyyy")
            }).ToList();

            if (_grid.Columns.Count > 0)
            {
                _grid.Columns["MaKH"].HeaderText = "Mã KH";
                _grid.Columns["HoTen"].HeaderText = "Họ tên";
                _grid.Columns["SoDienThoai"].HeaderText = "Số điện thoại";
                _grid.Columns["DiemTichLuy"].HeaderText = "Điểm tích luỹ";
                _grid.Columns["NgayTao"].HeaderText = "Ngày tạo";
            }
        }

        private KhachHang? LayKhachHangDangChon()
        {
            if (_grid.SelectedRows.Count == 0) return null;
            string maKH = _grid.SelectedRows[0].Cells["MaKH"].Value?.ToString() ?? string.Empty;
            return _data.DanhSachKhachHang.FirstOrDefault(k => k.MaKH == maKH);
        }

        private void BtnThem_Click(object? sender, EventArgs e)
        {
            using var form = new KhachHangEditForm(_data.TaoMaKhachHangMoi());
            if (form.ShowDialog(FindForm()) == DialogResult.OK)
            {
                _data.ThemKhachHang(form.KetQua);
                TaiLaiDuLieu();
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            var kh = LayKhachHangDangChon();
            if (kh == null)
            {
                MessageBox.Show("Vui lòng chọn 1 khách hàng cần sửa.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new KhachHangEditForm(kh.MaKH, kh);
            if (form.ShowDialog(FindForm()) == DialogResult.OK)
            {
                _data.SuaKhachHang(form.KetQua);
                TaiLaiDuLieu();
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            var kh = LayKhachHangDangChon();
            if (kh == null)
            {
                MessageBox.Show("Vui lòng chọn 1 khách hàng cần xoá.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var xacNhan = MessageBox.Show($"Bạn có chắc muốn xoá khách hàng '{kh.HoTen}'?",
                "Xác nhận xoá", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (xacNhan == DialogResult.Yes)
            {
                _data.XoaKhachHang(kh.MaKH);
                TaiLaiDuLieu();
            }
        }
    }
}
