using QuanLyKhachHang.Models;

namespace QuanLyKhachHang.Forms
{
    /// <summary>
    /// Form popup dùng chung cho cả 2 chức năng Thêm mới và Sửa khách hàng.
    /// Khi Sửa: mã KH bị khoá (không cho đổi), điểm tích luỹ có thể chỉnh tay
    /// (phòng trường hợp cần hiệu chỉnh thủ công). Khi Thêm: mã KH được sinh tự động.
    /// </summary>
    public class KhachHangEditForm : Form
    {
        private readonly TextBox _txtMaKH = new() { ReadOnly = true };
        private readonly TextBox _txtHoTen = new();
        private readonly TextBox _txtSoDienThoai = new();
        private readonly NumericUpDown _numDiem = new() { Maximum = 1000000, Minimum = 0 };

        public KhachHang KetQua { get; private set; } = new();
        private readonly bool _laSua;
        private readonly DateTime _ngayTaoGoc;

        public KhachHangEditForm(string maKHMoi, KhachHang? khDangSua = null)
        {
            _laSua = khDangSua != null;

            Text = _laSua ? "Sửa thông tin khách hàng" : "Thêm khách hàng mới";
            Width = 420;
            Height = 320;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9.5f);

            _txtMaKH.Text = _laSua ? khDangSua!.MaKH : maKHMoi;
            _txtHoTen.Text = _laSua ? khDangSua!.HoTen : string.Empty;
            _txtSoDienThoai.Text = _laSua ? khDangSua!.SoDienThoai : string.Empty;
            _numDiem.Value = _laSua ? khDangSua!.DiemTichLuy : 0;
            _ngayTaoGoc = _laSua ? khDangSua!.NgayTao : DateTime.Now;

            XayDungGiaoDien();
        }

        private void XayDungGiaoDien()
        {
            int y = 20;
            AddDong("Mã khách hàng:", _txtMaKH, ref y);
            AddDong("Họ tên:", _txtHoTen, ref y);
            AddDong("Số điện thoại:", _txtSoDienThoai, ref y);
            AddDong("Điểm tích luỹ:", _numDiem, ref y);

            var btnLuu = new Button
            {
                Text = "💾 Lưu",
                Location = new Point(110, y + 20),
                Size = new Size(90, 34),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLuu.Click += BtnLuu_Click;

            var btnHuy = new Button
            {
                Text = "Huỷ",
                Location = new Point(210, y + 20),
                Size = new Size(90, 34),
                FlatStyle = FlatStyle.Flat
            };
            btnHuy.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(btnLuu);
            Controls.Add(btnHuy);
        }

        private void AddDong(string nhan, Control input, ref int y)
        {
            var lbl = new Label { Text = nhan, Location = new Point(20, y + 4), AutoSize = true };
            input.Location = new Point(160, y);
            input.Width = 220;
            Controls.Add(lbl);
            Controls.Add(input);
            y += 40;
        }

        private void BtnLuu_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtHoTen.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên khách hàng.", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtSoDienThoai.Text))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại.", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            KetQua = new KhachHang
            {
                MaKH = _txtMaKH.Text,
                HoTen = _txtHoTen.Text.Trim(),
                SoDienThoai = _txtSoDienThoai.Text.Trim(),
                DiemTichLuy = (int)_numDiem.Value,
                NgayTao = _ngayTaoGoc
            };

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
