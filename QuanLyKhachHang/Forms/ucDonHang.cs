using QuanLyKhachHang.Models;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Forms
{
    /// <summary>
    /// Màn hình Đơn hàng / Tích điểm:
    ///  - Bên trái: form tạo đơn hàng mới (chọn khách hàng, nhập số tiền,
    ///    tuỳ chọn dùng điểm để giảm giá). Điểm cộng tự tính = SoTien / 1000.
    ///  - Bên phải: danh sách toàn bộ đơn hàng đã tạo.
    /// </summary>
    public class ucDonHang : UserControl
    {
        private readonly DataService _data;
        private readonly DataGridView _grid = new();

        private readonly ComboBox _cboKhachHang = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown _numSoTien = new() { Maximum = 1000000000, Minimum = 0, ThousandsSeparator = true };
        private readonly NumericUpDown _numDiemSuDung = new() { Maximum = 1000000, Minimum = 0 };
        private readonly Label _lblDiemHienCo = new();
        private readonly Label _lblDiemSeCong = new();
        private readonly Label _lblThanhTien = new();

        public ucDonHang(DataService data)
        {
            _data = data;
            BackColor = Color.FromArgb(243, 244, 246);
            XayDungGiaoDien();
            NapDanhSachKhachHang();
            TaiLaiDuLieuDon();
        }

        private void XayDungGiaoDien()
        {
            var lblTieuDe = new Label
            {
                Text = "Đơn hàng & Tích điểm",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            Controls.Add(lblTieuDe);

            // ---- Khung tạo đơn hàng bên trái ----
            var panelTao = new Panel
            {
                Location = new Point(10, 50),
                Size = new Size(330, 400),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblTao = new Label { Text = "Tạo đơn hàng mới", Font = new Font("Segoe UI", 11f, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true };
            panelTao.Controls.Add(lblTao);

            int y = 55;
            AddDong(panelTao, "Khách hàng:", _cboKhachHang, ref y);
            _cboKhachHang.SelectedIndexChanged += (s, e) => CapNhatDiemHienCo();

            AddDong(panelTao, "Số tiền đơn hàng (đ):", _numSoTien, ref y);
            _numSoTien.ValueChanged += (s, e) => CapNhatUocTinh();

            _lblDiemSeCong.AutoSize = true;
            _lblDiemSeCong.ForeColor = Color.FromArgb(16, 185, 129);
            _lblDiemSeCong.Location = new Point(15, y);
            panelTao.Controls.Add(_lblDiemSeCong);
            y += 30;

            AddDong(panelTao, "Điểm hiện có:", _lblDiemHienCo, ref y, laLabel: true);

            AddDong(panelTao, "Điểm muốn sử dụng:", _numDiemSuDung, ref y);
            _numDiemSuDung.ValueChanged += (s, e) => CapNhatUocTinh();

            _lblThanhTien.AutoSize = true;
            _lblThanhTien.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            _lblThanhTien.Location = new Point(15, y);
            panelTao.Controls.Add(_lblThanhTien);
            y += 40;

            var btnTaoDon = new Button
            {
                Text = "✅ Tạo đơn & cộng điểm",
                Location = new Point(15, y),
                Size = new Size(290, 38),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnTaoDon.Click += BtnTaoDon_Click;
            panelTao.Controls.Add(btnTaoDon);

            Controls.Add(panelTao);

            // ---- Bảng danh sách đơn hàng bên phải ----
            var lblDanhSach = new Label { Text = "Danh sách đơn hàng", Font = new Font("Segoe UI", 11f, FontStyle.Bold), AutoSize = true, Location = new Point(355, 55) };
            Controls.Add(lblDanhSach);

            var btnXoaDon = new Button
            {
                Text = "🗑️ Xoá đơn đã chọn",
                Location = new Point(700, 50),
                Size = new Size(160, 32),
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnXoaDon.Click += BtnXoaDon_Click;
            Controls.Add(btnXoaDon);

            _grid.Location = new Point(355, 90);
            _grid.Size = new Size(600, 500);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.BackgroundColor = Color.White;
            _grid.RowHeadersVisible = false;
            Controls.Add(_grid);
        }

        private void AddDong(Panel cha, string nhan, Control input, ref int y, bool laLabel = false)
        {
            var lbl = new Label { Text = nhan, Location = new Point(15, y + 3), AutoSize = true };
            cha.Controls.Add(lbl);

            if (laLabel)
            {
                input.Location = new Point(160, y);
                input.ForeColor = Color.FromArgb(37, 99, 235);
                ((Label)input).Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            }
            else
            {
                input.Location = new Point(160, y);
                input.Width = 150;
            }
            cha.Controls.Add(input);
            y += 34;
        }

        private void NapDanhSachKhachHang()
        {
            _cboKhachHang.DataSource = null;
            _cboKhachHang.DisplayMember = "HienThi";
            _cboKhachHang.ValueMember = "MaKH";
            _cboKhachHang.DataSource = _data.DanhSachKhachHang
                .Select(kh => new { kh.MaKH, HienThi = $"{kh.MaKH} - {kh.HoTen}" })
                .ToList();

            CapNhatDiemHienCo();
        }

        private KhachHang? KhachHangDangChon()
        {
            if (_cboKhachHang.SelectedValue == null) return null;
            string maKH = _cboKhachHang.SelectedValue.ToString() ?? string.Empty;
            return _data.DanhSachKhachHang.FirstOrDefault(k => k.MaKH == maKH);
        }

        private void CapNhatDiemHienCo()
        {
            var kh = KhachHangDangChon();
            _lblDiemHienCo.Text = kh != null ? $"{kh.DiemTichLuy} điểm" : "-";
            CapNhatUocTinh();
        }

        private void CapNhatUocTinh()
        {
            decimal soTien = _numSoTien.Value;
            int diemCong = (int)(soTien / 1000);
            _lblDiemSeCong.Text = $"→ Điểm sẽ được cộng: {diemCong} điểm (Số tiền / 1000)";

            decimal thanhTien = soTien - (_numDiemSuDung.Value * 1000);
            if (thanhTien < 0) thanhTien = 0;
            _lblThanhTien.Text = $"Thành tiền phải trả: {thanhTien:N0} đ";
        }

        private void BtnTaoDon_Click(object? sender, EventArgs e)
        {
            var kh = KhachHangDangChon();
            if (kh == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var (thanhCong, thongBao, _) = _data.TaoDonHang(kh.MaKH, _numSoTien.Value, (int)_numDiemSuDung.Value);

            if (!thanhCong)
            {
                MessageBox.Show(thongBao, "Không thể tạo đơn hàng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show(thongBao, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

            _numSoTien.Value = 0;
            _numDiemSuDung.Value = 0;
            NapDanhSachKhachHang();
            TaiLaiDuLieuDon();
        }

        private void TaiLaiDuLieuDon()
        {
            _grid.DataSource = null;
            _grid.DataSource = _data.DanhSachDonHang
                .OrderByDescending(d => d.NgayTao)
                .Select(d => new
                {
                    d.MaDon,
                    d.TenKH,
                    SoTien = d.SoTien.ToString("N0"),
                    DiemCong = d.DiemCong,
                    DiemSuDung = d.DiemSuDung,
                    ThanhTien = d.ThanhTien.ToString("N0"),
                    NgayTao = d.NgayTao.ToString("dd/MM/yyyy HH:mm")
                }).ToList();

            if (_grid.Columns.Count > 0)
            {
                _grid.Columns["MaDon"].HeaderText = "Mã đơn";
                _grid.Columns["TenKH"].HeaderText = "Khách hàng";
                _grid.Columns["SoTien"].HeaderText = "Số tiền";
                _grid.Columns["DiemCong"].HeaderText = "Điểm cộng";
                _grid.Columns["DiemSuDung"].HeaderText = "Điểm dùng";
                _grid.Columns["ThanhTien"].HeaderText = "Thành tiền";
                _grid.Columns["NgayTao"].HeaderText = "Ngày tạo";
            }
        }

        private void BtnXoaDon_Click(object? sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn 1 đơn hàng cần xoá.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string maDon = _grid.SelectedRows[0].Cells["MaDon"].Value?.ToString() ?? string.Empty;

            var xacNhan = MessageBox.Show(
                "Xoá đơn hàng này sẽ KHÔNG tự động hoàn/trừ lại điểm cho khách hàng.\nBạn có chắc muốn xoá?",
                "Xác nhận xoá", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (xacNhan == DialogResult.Yes)
            {
                _data.XoaDonHang(maDon);
                TaiLaiDuLieuDon();
            }
        }
    }
}
