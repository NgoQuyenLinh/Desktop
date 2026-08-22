// Services/DataService.cs
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
    /// - Đọc / ghi dữ liệu JSON.
    /// - Quản lý khách hàng.
    /// - Quản lý thuốc.
    /// - Quản lý đơn hàng.
    /// - Quản lý quà tặng.
    /// - Tìm kiếm, tính điểm và thống kê.
    /// </summary>
    public class DataService
    {

        
        private readonly string _dataFolder;
        private readonly string _khFile;
        private readonly string _donFile;
        private readonly string _quaFile;
        private readonly string _thuocFile;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        // ================== DANH SÁCH DỮ LIỆU ==================

        public List<KhachHang> DanhSachKhachHang { get; private set; } = new();
        public List<DonHang> DanhSachDonHang { get; private set; } = new();
        public List<QuaTang> DanhSachQuaTang { get; private set; } = new();
        public List<Thuoc> DanhSachThuoc { get; private set; } = new();


        // ================== KHỞI TẠO ==================

        public DataService()
        {
            
            _dataFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data"
            );

            Directory.CreateDirectory(_dataFolder);

            _khFile = Path.Combine(_dataFolder, "khachhang.json");
            _donFile = Path.Combine(_dataFolder, "donhang.json");
            _quaFile = Path.Combine(_dataFolder, "quatang.json");
            _thuocFile = Path.Combine(_dataFolder, "thuoc.json");

            TaiDuLieu();

            KhoiTaoQuaTangMacDinh();

            MigrateNgayTaoQuaTang();
        }


        // =========================================================
        // ĐỌC / GHI FILE
        // =========================================================

        public void TaiDuLieu()
        {
            DanhSachKhachHang = DocFile<KhachHang>(_khFile);

            DanhSachDonHang = DocFile<DonHang>(_donFile);

            DanhSachQuaTang = DocFile<QuaTang>(_quaFile);

            DanhSachThuoc = DocFile<Thuoc>(_thuocFile);
        }


        private List<T> DocFile<T>(string duongDan)
        {
            try
            {
                if (!File.Exists(duongDan))
                    return new List<T>();

                string noiDung = File.ReadAllText(duongDan);

                if (string.IsNullOrWhiteSpace(noiDung))
                    return new List<T>();

                // Thêm cấu hình case-insensitive để tránh lỗi không khớp hoa/thường
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<List<T>>(noiDung, options)
                       ?? new List<T>();
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ra cửa sổ Output hoặc hiển thị thông báo để debug
                System.Diagnostics.Debug.WriteLine($"Lỗi đọc file {duongDan}: {ex.Message}");

                // Trả về danh sách rỗng thay vì làm crash toàn bộ ứng dụng
                return new List<T>();
            }
        }


        private void GhiFile<T>(
            string duongDan,
            List<T> danhSach)
        {
            string noiDung =
                JsonSerializer.Serialize(
                    danhSach,
                    _jsonOptions
                );

            File.WriteAllText(
                duongDan,
                noiDung
            );
        }


        public void LuuKhachHang()
        {
            GhiFile(
                _khFile,
                DanhSachKhachHang
            );
        }


        public void LuuDonHang()
        {
            GhiFile(
                _donFile,
                DanhSachDonHang
            );
        }


        public void LuuQuaTang()
        {
            GhiFile(
                _quaFile,
                DanhSachQuaTang
            );
        }


        public void LuuThuoc()
        {
            GhiFile(
                _thuocFile,
                DanhSachThuoc
            );
        }


        // =========================================================
        // KHỞI TẠO QUÀ TẶNG
        // =========================================================

        private void KhoiTaoQuaTangMacDinh()
        {
            if (DanhSachQuaTang.Count == 0)
            {
                DanhSachQuaTang.Add(
                    new QuaTang
                    {
                        MaQua = "Q01",
                        TenQua = "Áo mưa",
                        DiemQuyDoi = 1000,
                        SoLuong = 10,
                        NgayTao = DateTime.Now
                    }
                );

                DanhSachQuaTang.Add(
                    new QuaTang
                    {
                        MaQua = "Q02",
                        TenQua = "Khẩu trang",
                        DiemQuyDoi = 100,
                        SoLuong = 50,
                        NgayTao = DateTime.Now
                    }
                );

                DanhSachQuaTang.Add(
                    new QuaTang
                    {
                        MaQua = "Q03",
                        TenQua = "Nước muối",
                        DiemQuyDoi = 50,
                        SoLuong = 100,
                        NgayTao = DateTime.Now
                    }
                );

                DanhSachQuaTang.Add(
                    new QuaTang
                    {
                        MaQua = "Q04",
                        TenQua = "Giấy",
                        DiemQuyDoi = 50,
                        SoLuong = 100,
                        NgayTao = DateTime.Now
                    }
                );

                LuuQuaTang();
            }
        }


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
            {
                LuuQuaTang();
            }
        }


        // =========================================================
        // KHÁCH HÀNG - CRUD
        // =========================================================

        public string TaoMaKhachHangMoi()
        {
            int soThuTu =
                DanhSachKhachHang.Count == 0
                    ? 1
                    : DanhSachKhachHang
                        .Select(
                            kh =>
                                int.TryParse(
                                    kh.MaKH.Replace("KH", ""),
                                    out int n
                                )
                                    ? n
                                    : 0
                        )
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
            var kh =
                DanhSachKhachHang.FirstOrDefault(
                    x => x.MaKH == khMoi.MaKH
                );

            if (kh == null)
                return false;

            kh.HoTen = khMoi.HoTen;
            kh.SoDienThoai = khMoi.SoDienThoai;
            kh.DiemTichLuy = khMoi.DiemTichLuy;

            LuuKhachHang();

            return true;
        }


        public bool XoaKhachHang(string maKH)
        {
            var kh =
                DanhSachKhachHang.FirstOrDefault(
                    x => x.MaKH == maKH
                );

            if (kh == null)
                return false;

            DanhSachKhachHang.Remove(kh);

            LuuKhachHang();

            return true;
        }


        public List<KhachHang> TimKiemKhachHang(
            string tuKhoa)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa))
                return DanhSachKhachHang;

            tuKhoa = tuKhoa
                .Trim()
                .ToLower();

            return DanhSachKhachHang
                .Where(
                    kh =>
                        kh.HoTen.ToLower().Contains(tuKhoa)
                        || kh.SoDienThoai
                            .ToLower()
                            .Contains(tuKhoa)
                        || kh.MaKH
                            .ToLower()
                            .Contains(tuKhoa)
                )
                .ToList();
        }


        public List<KhachHang> TimKhachHangTheoSoDienThoai(
            string chuoiSo)
        {
            if (string.IsNullOrWhiteSpace(chuoiSo))
                return new List<KhachHang>();

            chuoiSo = chuoiSo.Trim();

            return DanhSachKhachHang
                .Where(
                    kh =>
                        !string.IsNullOrEmpty(
                            kh.SoDienThoai
                        )
                        && kh.SoDienThoai.Contains(chuoiSo)
                )
                .OrderBy(
                    kh =>
                        kh.SoDienThoai.IndexOf(
                            chuoiSo,
                            StringComparison.Ordinal
                        )
                )
                .ThenBy(kh => kh.HoTen)
                .ToList();
        }


        // =========================================================
        // THUỐC - CRUD
        // =========================================================

        public string TaoMaThuocMoi()
        {
            int soThuTu =
                DanhSachThuoc.Count == 0
                    ? 1
                    : DanhSachThuoc
                        .Select(
                            t =>
                                int.TryParse(
                                    t.MaThuoc.Replace("T", ""),
                                    out int n
                                )
                                    ? n
                                    : 0
                        )
                        .DefaultIfEmpty(0)
                        .Max() + 1;

            return $"T{soThuTu:D3}";
        }


        public void ThemThuoc(Thuoc thuoc)
        {
            DanhSachThuoc.Add(thuoc);

            LuuThuoc();
        }


        public bool SuaThuoc(Thuoc thuocMoi)
        {
            var thuoc =
                DanhSachThuoc.FirstOrDefault(
                    x =>
                        x.MaThuoc
                        == thuocMoi.MaThuoc
                );

            if (thuoc == null)
                return false;

            thuoc.TenThuoc = thuocMoi.TenThuoc;

            thuoc.DonGia = thuocMoi.DonGia;

            thuoc.ConHang = thuocMoi.ConHang;

            LuuThuoc();

            return true;
        }


        public bool XoaThuoc(string maThuoc)
        {
            var thuoc =
                DanhSachThuoc.FirstOrDefault(
                    x =>
                        x.MaThuoc
                        == maThuoc
                );

            if (thuoc == null)
                return false;

            DanhSachThuoc.Remove(thuoc);

            LuuThuoc();

            return true;
        }


        public List<Thuoc> TimKiemThuoc(
            string? tuKhoa = null)
        {
            var ds =
                DanhSachThuoc.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tk =
                    tuKhoa
                        .Trim()
                        .ToLower();

                ds =
                    ds.Where(
                        t =>
                            t.TenThuoc
                                .ToLower()
                                .Contains(tk)
                            || t.MaThuoc
                                .ToLower()
                                .Contains(tk)
                    );
            }

            return ds
                .OrderBy(t => t.TenThuoc)
                .ToList();
        }


        public List<Thuoc> ThuocConHang()
        {
            return DanhSachThuoc
                .Where(t => t.ConHang)
                .OrderBy(t => t.TenThuoc)
                .ToList();
        }


        // =========================================================
        // ĐƠN HÀNG
        // =========================================================

        public string TaoMaDonHangMoi()
        {
            int soThuTu =
                DanhSachDonHang.Count == 0
                    ? 1
                    : DanhSachDonHang
                        .Select(
                            d =>
                                int.TryParse(
                                    d.MaDon.Replace("DH", ""),
                                    out int n
                                )
                                    ? n
                                    : 0
                        )
                        .DefaultIfEmpty(0)
                        .Max() + 1;

            return $"DH{soThuTu:D3}";
        }


        /// <summary>
        /// Tạo đơn hàng có nhiều loại thuốc.
        /// </summary>
        public (
            bool ThanhCong,
            string ThongBao,
            DonHang? Don
        ) TaoDonHang(
            string maKH,
            List<ChiTietDonHang> danhSachThuoc,
            int diemSuDung,
            QuaTang? quaTang = null)
        {
            var kh =
                DanhSachKhachHang.FirstOrDefault(
                    x => x.MaKH == maKH
                );

            if (kh == null)
            {
                return (
                    false,
                    "Không tìm thấy khách hàng.",
                    null
                );
            }


            if (
                danhSachThuoc == null
                || danhSachThuoc.Count == 0
            )
            {
                return (
                    false,
                    "Đơn hàng phải có ít nhất 1 loại thuốc.",
                    null
                );
            }


            if (
                danhSachThuoc.Any(
                    t => t.SoLuong <= 0
                )
            )
            {
                return (
                    false,
                    "Số lượng thuốc không hợp lệ.",
                    null
                );
            }


            decimal soTien =
                danhSachThuoc.Sum(
                    t => t.ThanhTien
                );


            if (soTien <= 0)
            {
                return (
                    false,
                    "Số tiền đơn hàng phải lớn hơn 0.",
                    null
                );
            }


            if (diemSuDung < 0)
            {
                return (
                    false,
                    "Điểm sử dụng không hợp lệ.",
                    null
                );
            }


            if (
                diemSuDung
                > kh.DiemTichLuy
            )
            {
                return (
                    false,
                    $"Khách hàng chỉ có {kh.DiemTichLuy} điểm, không đủ để sử dụng {diemSuDung} điểm.",
                    null
                );
            }


            // 1 điểm = giảm 1.000đ
            if (
                diemSuDung * 1000
                > soTien
            )
            {
                return (
                    false,
                    "Số điểm sử dụng vượt quá giá trị đơn hàng.",
                    null
                );
            }


            int tongDiemTru =
                diemSuDung
                + (
                    quaTang?.DiemQuyDoi
                    ?? 0
                );


            if (
                tongDiemTru
                > kh.DiemTichLuy
            )
            {
                return (
                    false,
                    $"Khách hàng không đủ điểm. Cần {tongDiemTru} điểm, hiện có {kh.DiemTichLuy} điểm.",
                    null
                );
            }


            int diemCong =
                (int)(soTien / 1000);


            kh.DiemTichLuy =
                kh.DiemTichLuy
                - tongDiemTru
                + diemCong;


            // Xử lý quà
            if (quaTang != null)
            {
                quaTang.SoLuong -= 1;

                quaTang.DangBan = true;

                LuuQuaTang();
            }


            // Trừ số lượng thuốc
            foreach (
                var chiTiet
                in danhSachThuoc
            )
            {
                var thuoc =
                    DanhSachThuoc.FirstOrDefault(
                        t =>
                            t.MaThuoc
                            == chiTiet.MaThuoc
                    );

                if (thuoc != null)
                {
                    /*
                     * Nếu model Thuoc có thuộc tính số lượng,
                     * có thể xử lý trừ tồn kho tại đây.
                     *
                     * Hiện tại theo model trong DataService,
                     * thuốc chỉ có trạng thái ConHang.
                     */
                }
            }


            var don =
                new DonHang
                {
                    MaDon =
                        TaoMaDonHangMoi(),

                    MaKH =
                        kh.MaKH,

                    TenKH =
                        kh.HoTen,

                    DanhSachThuoc =
                        danhSachThuoc,

                    SoTien =
                        soTien,

                    NgayTao =
                        DateTime.Now,

                    DiemCong =
                        diemCong,

                    DiemSuDung =
                        diemSuDung,

                    QuaTangDoi =
                        quaTang?.TenQua
                        ?? string.Empty,

                    DiemDoiQua =
                        quaTang?.DiemQuyDoi
                        ?? 0,

                    TongDiemSauGiaoDich =
                        kh.DiemTichLuy
                };


            DanhSachDonHang.Add(don);

            LuuDonHang();

            LuuKhachHang();

            return (
                true,
                "Tạo đơn hàng thành công.",
                don
            );
        }


        public bool XoaDonHang(
            string maDon)
        {
            var don =
                DanhSachDonHang.FirstOrDefault(
                    x =>
                        x.MaDon
                        == maDon
                );

            if (don == null)
                return false;

            DanhSachDonHang.Remove(don);

            LuuDonHang();

            return true;
        }


        // =========================================================
        // QUÀ TẶNG - CRUD
        // =========================================================

        public string TaoMaQuaTangMoi()
        {
            int soThuTu =
                DanhSachQuaTang.Count == 0
                    ? 1
                    : DanhSachQuaTang
                        .Select(
                            q =>
                                int.TryParse(
                                    q.MaQua.Replace("Q", ""),
                                    out int n
                                )
                                    ? n
                                    : 0
                        )
                        .DefaultIfEmpty(0)
                        .Max() + 1;

            return $"Q{soThuTu:D2}";
        }


        public void ThemQuaTang(
            QuaTang qua)
        {
            if (qua.NgayTao == default)
                qua.NgayTao =
                    DateTime.Now;

            DanhSachQuaTang.Add(qua);

            LuuQuaTang();
        }


        public bool SuaQuaTang(
            QuaTang quaMoi)
        {
            var qua =
                DanhSachQuaTang.FirstOrDefault(
                    x =>
                        x.MaQua
                        == quaMoi.MaQua
                );

            if (qua == null)
                return false;

            qua.TenQua =
                quaMoi.TenQua;

            qua.DiemQuyDoi =
                quaMoi.DiemQuyDoi;

            qua.SoLuong =
                quaMoi.SoLuong;

            qua.DangBan =
                quaMoi.DangBan;

            LuuQuaTang();

            return true;
        }


        public bool XoaQuaTang(
            string maQua)
        {
            var qua =
                DanhSachQuaTang.FirstOrDefault(
                    x =>
                        x.MaQua
                        == maQua
                );

            if (qua == null)
                return false;

            DanhSachQuaTang.Remove(qua);

            LuuQuaTang();

            return true;
        }


        public List<QuaTang> QuaTangTrongThang(
            string? tuKhoa = null)
        {
            var now =
                DateTime.Now;

            var ds =
                DanhSachQuaTang.Where(
                    q =>
                        q.NgayTao.Year
                            == now.Year
                        && q.NgayTao.Month
                            == now.Month
                );


            if (
                !string.IsNullOrWhiteSpace(
                    tuKhoa
                )
            )
            {
                var tk =
                    tuKhoa
                        .Trim()
                        .ToLower();

                ds =
                    ds.Where(
                        q =>
                            q.TenQua
                                .ToLower()
                                .Contains(tk)
                            || q.MaQua
                                .ToLower()
                                .Contains(tk)
                    );
            }


            return ds
                .OrderByDescending(
                    q => q.NgayTao
                )
                .ToList();
        }


        public List<QuaTang> QuaTangChuaBan(
            string? tuKhoa = null)
        {
            var ds =
                DanhSachQuaTang.Where(
                    q =>
                        !q.DangBan
                        && q.SoLuong > 0
                );


            if (
                !string.IsNullOrWhiteSpace(
                    tuKhoa
                )
            )
            {
                var tk =
                    tuKhoa
                        .Trim()
                        .ToLower();

                ds =
                    ds.Where(
                        q =>
                            q.TenQua
                                .ToLower()
                                .Contains(tk)
                            || q.MaQua
                                .ToLower()
                                .Contains(tk)
                    );
            }


            return ds
                .OrderByDescending(
                    q => q.NgayTao
                )
                .ToList();
        }


        public List<QuaTang> QuaTangDangBan(
            string? tuKhoa = null)
        {
            var ds =
                DanhSachQuaTang.Where(
                    q =>
                        q.DangBan
                        && q.SoLuong > 0
                );


            if (
                !string.IsNullOrWhiteSpace(
                    tuKhoa
                )
            )
            {
                var tk =
                    tuKhoa
                        .Trim()
                        .ToLower();

                ds =
                    ds.Where(
                        q =>
                            q.TenQua
                                .ToLower()
                                .Contains(tk)
                            || q.MaQua
                                .ToLower()
                                .Contains(tk)
                    );
            }


            return ds
                .OrderByDescending(
                    q => q.NgayTao
                )
                .ToList();
        }


        public List<QuaTang> QuaTangDaHetHang(
            string? tuKhoa = null)
        {
            var ds =
                DanhSachQuaTang.Where(
                    q => q.SoLuong <= 0
                );


            if (
                !string.IsNullOrWhiteSpace(
                    tuKhoa
                )
            )
            {
                var tk =
                    tuKhoa
                        .Trim()
                        .ToLower();

                ds =
                    ds.Where(
                        q =>
                            q.TenQua
                                .ToLower()
                                .Contains(tk)
                            || q.MaQua
                                .ToLower()
                                .Contains(tk)
                    );
            }


            return ds
                .OrderByDescending(
                    q => q.NgayTao
                )
                .ToList();
        }


        public List<QuaTang> QuaTangCoTheDoi()
        {
            return DanhSachQuaTang
                .Where(
                    q =>
                        q.DangBan
                        && q.SoLuong > 0
                )
                .ToList();
        }


        public bool ChuyenTrangThaiQuaTang(
            string maQua)
        {
            var qua =
                DanhSachQuaTang.FirstOrDefault(
                    x =>
                        x.MaQua
                        == maQua
                );

            if (qua == null)
                return false;

            qua.DangBan =
                !qua.DangBan;

            LuuQuaTang();

            return qua.DangBan;
        }


        // =========================================================
        // THỐNG KÊ ĐƠN HÀNG
        // =========================================================

        public List<DonHang> LocDonHangTheoNgay(
            DateTime tuNgay,
            DateTime denNgay)
        {
            denNgay =
                denNgay.Date
                    .AddDays(1)
                    .AddTicks(-1);

            return DanhSachDonHang
                .Where(
                    d =>
                        d.NgayTao
                            >= tuNgay.Date
                        && d.NgayTao
                            <= denNgay
                )
                .ToList();
        }


        public int SoLuongDonHang(
            DateTime tuNgay,
            DateTime denNgay)
        {
            return LocDonHangTheoNgay(
                tuNgay,
                denNgay
            ).Count;
        }


        public decimal TongDoanhThu(
            DateTime tuNgay,
            DateTime denNgay)
        {
            return LocDonHangTheoNgay(
                tuNgay,
                denNgay
            ).Sum(
                d => d.ThanhTien
            );
        }


        public int TongDiemDaTichLuy(
            DateTime tuNgay,
            DateTime denNgay)
        {
            return LocDonHangTheoNgay(
                tuNgay,
                denNgay
            ).Sum(
                d => d.DiemCong
            );
        }


        public int TongDiemDaSuDung(
            DateTime tuNgay,
            DateTime denNgay)
        {
            return LocDonHangTheoNgay(
                tuNgay,
                denNgay
            ).Sum(
                d => d.DiemSuDung
            );
        }


        public List<KhachHang> TopKhachHangDiemCao(
            int soLuong = 5)
        {
            return DanhSachKhachHang
                .OrderByDescending(
                    kh => kh.DiemTichLuy
                )
                .Take(soLuong)
                .ToList();
        }


        // =========================================================
        // THỐNG KÊ TỔNG QUAN
        // =========================================================

        public int TongSoDonHang()
        {
            return DanhSachDonHang.Count;
        }


        public decimal TongDoanhThuTuThuoc()
        {
            return DanhSachDonHang
                .Sum(
                    d => d.ThanhTien
                );
        }


        public int TongSoThuocDaBan()
        {
            return DanhSachDonHang
                .Sum(
                    d =>
                        d.DanhSachThuoc?
                            .Sum(
                                ct => ct.SoLuong
                            )
                        ?? 0
                );
        }


        public int TongSoQuaDaTang()
        {
            return DanhSachDonHang
                .Count(
                    d =>
                        !string.IsNullOrEmpty(
                            d.QuaTangDoi
                        )
                );
        }


        public string QuaDuocTangNhieuNhat()
        {
            var top =
                DanhSachDonHang
                    .Where(
                        d =>
                            !string.IsNullOrEmpty(
                                d.QuaTangDoi
                            )
                    )
                    .GroupBy(
                        d => d.QuaTangDoi
                    )
                    .OrderByDescending(
                        g => g.Count()
                    )
                    .FirstOrDefault();


            return top != null
                ? $"{top.Key} ({top.Count()} lần)"
                : "-";
        }


        public int TongDiemDaCongToanBo()
        {
            return DanhSachDonHang
                .Sum(
                    d => d.DiemCong
                );
        }


        public int TongDiemDaSuDungToanBo()
        {
            return DanhSachDonHang
                .Sum(
                    d => d.DiemSuDung
                );
        }


        // =========================================================
        // GHI CHÚ
        // =========================================================

        public List<GhiChu> LayDanhSachGhiChu()
        {
            string path =
                Path.Combine(
                    _dataFolder,
                    "ghichu.json"
                );

            if (!File.Exists(path))
                return new List<GhiChu>();

            string json =
                File.ReadAllText(path);

            return JsonSerializer.Deserialize<List<GhiChu>>(
                       json
                   )
                   ?? new List<GhiChu>();
        }


        public void LuuDanhSachGhiChu(
            List<GhiChu> danhSach)
        {
            string path =
                Path.Combine(
                    _dataFolder,
                    "ghichu.json"
                );

            string json =
                JsonSerializer.Serialize(
                    danhSach,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );

            File.WriteAllText(
                path,
                json
            );
        }
        
    }
    
}