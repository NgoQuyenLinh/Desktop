using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using QuanLyKhachHang.Models;

namespace QuanLyKhachHang.Services
{
    /// <summary>
    /// Lớp trung tâm chịu trách nhiệm:
    ///  - Đọc / ghi 2 file JSON (khachhang.json, donhang.json) đóng vai trò "database" giả lập.
    ///  - Cung cấp các thao tác CRUD, tìm kiếm, tính điểm, thống kê cho toàn bộ ứng dụng.
    /// Toàn bộ danh sách được nạp 1 lần khi khởi động (trong bộ nhớ), và mỗi lần
    /// Thêm/Sửa/Xoá sẽ ghi đè lại file tương ứng ngay lập tức để đảm bảo dữ liệu
    /// trên đĩa luôn đồng bộ với dữ liệu đang hiển thị.
    /// </summary>
    public class DataService
    {
        private readonly string _dataFolder;
        private readonly string _khFile;
        private readonly string _donFile;
        private readonly string _quaFile;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        public List<KhachHang> DanhSachKhachHang { get; private set; } = new();
        public List<DonHang> DanhSachDonHang { get; private set; } = new();
        public List<QuaTang> DanhSachQuaTang { get; private set; } = new();

        public DataService()
        {
            // Thư mục Data nằm cạnh file .exe khi build (đã cấu hình CopyToOutputDirectory trong .csproj)
            _dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(_dataFolder);

            _khFile = Path.Combine(_dataFolder, "khachhang.json");
            _donFile = Path.Combine(_dataFolder, "donhang.json");
            _quaFile = Path.Combine(_dataFolder, "quatang.json");

            TaiDuLieu();
            KhoiTaoQuaTangMacDinh();
            MigrateNgayTaoQuaTang();
        }

        private void KhoiTaoQuaTangMacDinh()
        {
            if (DanhSachQuaTang.Count == 0)
            {
                DanhSachQuaTang.Add(new QuaTang { MaQua = "Q01", TenQua = "Áo mưa", DiemQuyDoi = 1000, SoLuong = 10, NgayTao = DateTime.Now });
                DanhSachQuaTang.Add(new QuaTang { MaQua = "Q02", TenQua = "Khẩu trang", DiemQuyDoi = 100, SoLuong = 50, NgayTao = DateTime.Now });
                DanhSachQuaTang.Add(new QuaTang { MaQua = "Q03", TenQua = "Nước muối", DiemQuyDoi = 50, SoLuong = 100, NgayTao = DateTime.Now });
                DanhSachQuaTang.Add(new QuaTang { MaQua = "Q04", TenQua = "Giấy", DiemQuyDoi = 50, SoLuong = 100, NgayTao = DateTime.Now });
                LuuQuaTang();
            }
        }

        /// <summary>
        /// Dữ liệu quà tặng cũ (tạo trước khi có trường NgayTao) khi đọc lên từ JSON
        /// sẽ có NgayTao = mặc định (0001-01-01). Theo yêu cầu: coi các quà này như
        /// vừa được tạo hôm nay để chúng xuất hiện trong nhóm "Quà trong tháng".
        /// </summary>
        private void MigrateNgayTaoQuaTang()
        {
            bool coThayDoi = false;
            foreach (var qua in DanhSachQuaTang)
            {
                if (qua.NgayTao == default)
                {
                    qua.NgayTao = DateTime.Now;
                    coThayDoi = true;
                }
            }

            if (coThayDoi)
                LuuQuaTang();
        }

        // ================== ĐỌC / GHI FILE ==================

        public void TaiDuLieu()
        {
            DanhSachKhachHang = DocFile<KhachHang>(_khFile);
            DanhSachDonHang = DocFile<DonHang>(_donFile);
            DanhSachQuaTang = DocFile<QuaTang>(_quaFile);
        }

        private List<T> DocFile<T>(string duongDan)
        {
            if (!File.Exists(duongDan))
                return new List<T>();

            string noiDung = File.ReadAllText(duongDan);
            if (string.IsNullOrWhiteSpace(noiDung))
                return new List<T>();

            return JsonSerializer.Deserialize<List<T>>(noiDung) ?? new List<T>();
        }

        private void GhiFile<T>(string duongDan, List<T> danhSach)
        {
            string noiDung = JsonSerializer.Serialize(danhSach, _jsonOptions);
            File.WriteAllText(duongDan, noiDung);
        }

        public void LuuKhachHang() => GhiFile(_khFile, DanhSachKhachHang);
        public void LuuDonHang() => GhiFile(_donFile, DanhSachDonHang);
        public void LuuQuaTang() => GhiFile(_quaFile, DanhSachQuaTang);

        // ================== KHÁCH HÀNG: CRUD ==================

        public string TaoMaKhachHangMoi()
        {
            int soThuTu = DanhSachKhachHang.Count == 0
                ? 1
                : DanhSachKhachHang
                    .Select(kh => int.TryParse(kh.MaKH.Replace("KH", ""), out int n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;
            return $"KH{soThuTu:D3}";
        }

        public void ThemKhachHang(KhachHang kh)
        {
            DanhSachKhachHang.Add(kh);
            LuuKhachHang();
        }

        public bool SuaKhachHang(KhachHang khMoi)
        {
            var kh = DanhSachKhachHang.FirstOrDefault(x => x.MaKH == khMoi.MaKH);
            if (kh == null) return false;

            kh.HoTen = khMoi.HoTen;
            kh.SoDienThoai = khMoi.SoDienThoai;
            kh.DiemTichLuy = khMoi.DiemTichLuy;
            LuuKhachHang();
            return true;
        }

        public bool XoaKhachHang(string maKH)
        {
            var kh = DanhSachKhachHang.FirstOrDefault(x => x.MaKH == maKH);
            if (kh == null) return false;

            DanhSachKhachHang.Remove(kh);
            LuuKhachHang();
            return true;
        }

        /// <summary>Tìm kiếm tức thời theo tên / SĐT / mã KH, không phân biệt hoa thường, chỉ cần chứa từ khoá.</summary>
        public List<KhachHang> TimKiemKhachHang(string tuKhoa)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa))
                return DanhSachKhachHang;

            tuKhoa = tuKhoa.Trim().ToLower();

            return DanhSachKhachHang.Where(kh =>
                kh.HoTen.ToLower().Contains(tuKhoa) ||
                kh.SoDienThoai.ToLower().Contains(tuKhoa) ||
                kh.MaKH.ToLower().Contains(tuKhoa)
            ).ToList();
        }

        /// <summary>
        /// Tìm kiếm khách hàng theo số điện thoại kiểu real-time: chỉ cần số điện thoại
        /// CHỨA chuỗi số vừa nhập (không cần đúng vị trí đầu/cuối). Trả về TOÀN BỘ
        /// các khách hàng khớp để hiển thị dạng gợi ý, thay vì chỉ 1 kết quả duy nhất.
        /// Dùng cho khung "Tạo hoá đơn nhanh" ở Trang chủ.
        /// </summary>
        public List<KhachHang> TimKhachHangTheoSoDienThoai(string chuoiSo)
        {
            if (string.IsNullOrWhiteSpace(chuoiSo))
                return new List<KhachHang>();

            chuoiSo = chuoiSo.Trim();

            return DanhSachKhachHang
                .Where(kh => !string.IsNullOrEmpty(kh.SoDienThoai) && kh.SoDienThoai.Contains(chuoiSo))
                .OrderBy(kh => kh.SoDienThoai.IndexOf(chuoiSo, StringComparison.Ordinal))
                .ThenBy(kh => kh.HoTen)
                .ToList();
        }

        // ================== ĐƠN HÀNG & TÍCH ĐIỂM ==================

        public string TaoMaDonHangMoi()
        {
            int soThuTu = DanhSachDonHang.Count == 0
                ? 1
                : DanhSachDonHang
                    .Select(d => int.TryParse(d.MaDon.Replace("DH", ""), out int n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;
            return $"DH{soThuTu:D3}";
        }

        /// <summary>
        /// Tạo 1 đơn hàng mới cho khách hàng, tự động:
        ///  - Kiểm tra không cho dùng quá số điểm đang có.
        ///  - Trừ điểm đã dùng, cộng điểm mới (Điểm cộng = SoTien / 1000, làm tròn xuống).
        ///  - Cập nhật lại điểm tích luỹ của khách hàng và ghi file.
        /// </summary>
        public (bool ThanhCong, string ThongBao, DonHang? Don) TaoDonHang(string maKH, decimal soTien, int diemSuDung, QuaTang? quaTang = null)
        {
            var kh = DanhSachKhachHang.FirstOrDefault(x => x.MaKH == maKH);
            if (kh == null)
                return (false, "Không tìm thấy khách hàng.", null);

            if (soTien <= 0)
                return (false, "Số tiền đơn hàng phải lớn hơn 0.", null);

            if (diemSuDung < 0)
                return (false, "Điểm sử dụng không hợp lệ.", null);

            if (diemSuDung > kh.DiemTichLuy)
                return (false, $"Khách hàng chỉ có {kh.DiemTichLuy} điểm, không đủ để sử dụng {diemSuDung} điểm.", null);

            // 1 điểm tương ứng giảm 1.000đ, không cho dùng điểm vượt quá giá trị đơn hàng
            if (diemSuDung * 1000 > soTien)
                return (false, "Số điểm sử dụng vượt quá giá trị đơn hàng.", null);

            int tongDiemTru = diemSuDung + (quaTang?.DiemQuyDoi ?? 0);
            if (tongDiemTru > kh.DiemTichLuy)
                return (false, $"Khách hàng không đủ điểm. Cần {tongDiemTru} điểm, hiện có {kh.DiemTichLuy} điểm.", null);

            int diemCong = (int)(soTien / 1000);

            kh.DiemTichLuy = kh.DiemTichLuy - tongDiemTru + diemCong;

            if (quaTang != null)
            {
                quaTang.SoLuong -= 1;
                quaTang.DangBan = true; // đã có khách đổi -> tự chuyển sang "Đang tặng", vẫn chuyển tay lại được
                LuuQuaTang();
            }

            var don = new DonHang
            {
                MaDon = TaoMaDonHangMoi(),
                MaKH = kh.MaKH,
                TenKH = kh.HoTen,
                SoTien = soTien,
                NgayTao = DateTime.Now,
                DiemCong = diemCong,
                DiemSuDung = diemSuDung,
                QuaTangDoi = quaTang?.TenQua ?? string.Empty,
                DiemDoiQua = quaTang?.DiemQuyDoi ?? 0,
                TongDiemSauGiaoDich = kh.DiemTichLuy
            };

            DanhSachDonHang.Add(don);
            LuuDonHang();
            LuuKhachHang();

            return (true, "Tạo đơn hàng thành công.", don);
        }

        public bool XoaDonHang(string maDon)
        {
            var don = DanhSachDonHang.FirstOrDefault(x => x.MaDon == maDon);
            if (don == null) return false;

            DanhSachDonHang.Remove(don);
            LuuDonHang();
            return true;
        }

        // ================== QUÀ TẶNG: CRUD ==================

        public string TaoMaQuaTangMoi()
        {
            int soThuTu = DanhSachQuaTang.Count == 0
                ? 1
                : DanhSachQuaTang
                    .Select(q => int.TryParse(q.MaQua.Replace("Q", ""), out int n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;
            return $"Q{soThuTu:D2}";
        }

        public void ThemQuaTang(QuaTang qua)
        {
            if (qua.NgayTao == default)
                qua.NgayTao = DateTime.Now;

            DanhSachQuaTang.Add(qua);
            LuuQuaTang();
        }

        public bool SuaQuaTang(QuaTang quaMoi)
        {
            var qua = DanhSachQuaTang.FirstOrDefault(x => x.MaQua == quaMoi.MaQua);
            if (qua == null) return false;

            qua.TenQua = quaMoi.TenQua;
            qua.DiemQuyDoi = quaMoi.DiemQuyDoi;
            qua.SoLuong = quaMoi.SoLuong;
            qua.DangBan = quaMoi.DangBan;
            LuuQuaTang();
            return true;
        }

        public bool XoaQuaTang(string maQua)
        {
            var qua = DanhSachQuaTang.FirstOrDefault(x => x.MaQua == maQua);
            if (qua == null) return false;

            DanhSachQuaTang.Remove(qua);
            LuuQuaTang();
            return true;
        }

        /// <summary>
        /// Danh sách quà được thêm vào kho trong THÁNG HIỆN TẠI (theo NgayTao), có hỗ trợ
        /// lọc theo từ khoá tên/mã quà. Dùng cho GroupBox "Quà trong tháng" bên KhoQuaView.
        /// </summary>
        public List<QuaTang> QuaTangTrongThang(string? tuKhoa = null)
        {
            var now = DateTime.Now;
            var ds = DanhSachQuaTang.Where(q => q.NgayTao.Year == now.Year && q.NgayTao.Month == now.Month);

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim().ToLower();
                ds = ds.Where(q => q.TenQua.ToLower().Contains(tk) || q.MaQua.ToLower().Contains(tk));
            }

            return ds.OrderByDescending(q => q.NgayTao).ToList();
        }

        /// <summary>
        /// Danh sách quà đang ở trạng thái "Chưa tặng" (QuaTang.DangBan == false)
        /// VÀ vẫn còn hàng trong kho (SoLuong &gt; 0). Quà đã hết hàng không còn nằm
        /// ở đây nữa mà chuyển sang nhóm riêng <see cref="QuaTangDaHetHang"/>.
        /// </summary>
        public List<QuaTang> QuaTangChuaBan(string? tuKhoa = null)
        {
            var ds = DanhSachQuaTang.Where(q => !q.DangBan && q.SoLuong > 0);

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim().ToLower();
                ds = ds.Where(q => q.TenQua.ToLower().Contains(tk) || q.MaQua.ToLower().Contains(tk));
            }

            return ds.OrderByDescending(q => q.NgayTao).ToList();
        }

        /// <summary>
        /// Danh sách quà đang ở trạng thái "Đang tặng" (QuaTang.DangBan == true)
        /// VÀ vẫn còn hàng trong kho (SoLuong &gt; 0). Quà đã hết hàng không còn nằm
        /// ở đây nữa mà chuyển sang nhóm riêng <see cref="QuaTangDaHetHang"/>.
        /// </summary>
        public List<QuaTang> QuaTangDangBan(string? tuKhoa = null)
        {
            var ds = DanhSachQuaTang.Where(q => q.DangBan && q.SoLuong > 0);

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim().ToLower();
                ds = ds.Where(q => q.TenQua.ToLower().Contains(tk) || q.MaQua.ToLower().Contains(tk));
            }

            return ds.OrderByDescending(q => q.NgayTao).ToList();
        }

        /// <summary>
        /// Danh sách quà đã HẾT HÀNG trong kho (SoLuong &lt;= 0), bất kể trước đó đang
        /// ở trạng thái "Chưa tặng" hay "Đang tặng". Tách thành khu vực riêng để người
        /// dùng dễ nhận biết quà nào cần nhập thêm, không lẫn với 2 trạng thái còn hàng.
        /// </summary>
        public List<QuaTang> QuaTangDaHetHang(string? tuKhoa = null)
        {
            var ds = DanhSachQuaTang.Where(q => q.SoLuong <= 0);

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim().ToLower();
                ds = ds.Where(q => q.TenQua.ToLower().Contains(tk) || q.MaQua.ToLower().Contains(tk));
            }

            return ds.OrderByDescending(q => q.NgayTao).ToList();
        }

        /// <summary>
        /// Danh sách quà ĐỦ ĐIỀU KIỆN để khách đổi bằng điểm ở màn hình Đơn hàng:
        /// phải đang ở trạng thái "Đang tặng" (QuaTang.DangBan == true) bên Kho Quà
        /// VÀ còn số lượng trong kho. Đây là điểm đồng bộ giữa Kho Quà và khu đổi quà
        /// bằng điểm — quà chỉ hiện ra ở Đơn hàng khi đã được đưa vào "Đang tặng".
        /// </summary>
        public List<QuaTang> QuaTangCoTheDoi() =>
            DanhSachQuaTang.Where(q => q.DangBan && q.SoLuong > 0).ToList();

        /// <summary>
        /// Chuyển trạng thái Chưa tặng &lt;-&gt; Đang tặng cho 1 quà, lưu file ngay.
        /// Trả về trạng thái DangBan mới sau khi chuyển (true = vừa chuyển sang Đang tặng).
        /// Lưu ý: nếu quà đã hết hàng (SoLuong &lt;= 0), quà vẫn nằm ở nhóm "Đã tặng hết"
        /// bất kể cờ DangBan là gì, cho tới khi được nhập thêm hàng.
        /// </summary>
        public bool ChuyenTrangThaiQuaTang(string maQua)
        {
            var qua = DanhSachQuaTang.FirstOrDefault(x => x.MaQua == maQua);
            if (qua == null) return false;

            qua.DangBan = !qua.DangBan;
            LuuQuaTang();
            return qua.DangBan;
        }

        // ================== THỐNG KÊ ==================

        public List<DonHang> LocDonHangTheoNgay(DateTime tuNgay, DateTime denNgay)
        {
            denNgay = denNgay.Date.AddDays(1).AddTicks(-1); // lấy trọn ngày kết thúc
            return DanhSachDonHang
                .Where(d => d.NgayTao >= tuNgay.Date && d.NgayTao <= denNgay)
                .ToList();
        }

        public int SoLuongDonHang(DateTime tuNgay, DateTime denNgay) => LocDonHangTheoNgay(tuNgay, denNgay).Count;

        public decimal TongDoanhThu(DateTime tuNgay, DateTime denNgay) =>
            LocDonHangTheoNgay(tuNgay, denNgay).Sum(d => d.ThanhTien);

        public int TongDiemDaTichLuy(DateTime tuNgay, DateTime denNgay) =>
            LocDonHangTheoNgay(tuNgay, denNgay).Sum(d => d.DiemCong);

        public int TongDiemDaSuDung(DateTime tuNgay, DateTime denNgay) =>
            LocDonHangTheoNgay(tuNgay, denNgay).Sum(d => d.DiemSuDung);

        public List<KhachHang> TopKhachHangDiemCao(int soLuong = 5) =>
            DanhSachKhachHang.OrderByDescending(kh => kh.DiemTichLuy).Take(soLuong).ToList();

        public List<GhiChu> LayDanhSachGhiChu()
        {
            string path = "Data/ghichu.json";
            if (!File.Exists(path)) return new List<GhiChu>();
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<GhiChu>>(json) ?? new List<GhiChu>();
        }

        public void LuuDanhSachGhiChu(List<GhiChu> danhSach)
        {
            string path = "Data/ghichu.json";
            string json = JsonSerializer.Serialize(danhSach, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}