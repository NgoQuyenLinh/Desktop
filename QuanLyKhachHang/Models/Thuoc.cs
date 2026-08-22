using System;

namespace QuanLyKhachHang.Models
{
    /// <summary>
    /// Model đại diện cho 1 loại thuốc trong kho.
    /// Đây là lớp dữ liệu thuần (POCO), được (de)serialize trực tiếp
    /// từ/ra file Data/thuoc.json bằng System.Text.Json.
    /// </summary>
    public class Thuoc
    {
        public string MaThuoc { get; set; } = string.Empty;
        public string TenThuoc { get; set; } = string.Empty;
        public string LoaiThuoc { get; set; } = string.Empty;

        /// <summary>Đơn giá / số tiền của 1 đơn vị thuốc.</summary>
        public decimal DonGia { get; set; }

        /// <summary>
        /// Tình trạng hàng THỦ CÔNG: true = "Còn hàng" (có thể chọn để bán trong Đơn hàng),
        /// false = "Hết hàng" (không cho phép chọn thuốc này khi tạo đơn hàng mới).
        /// Mặc định thuốc mới thêm luôn ở trạng thái "Còn hàng".
        /// </summary>
        public bool ConHang { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public override string ToString() => $"{TenThuoc} - {DonGia:N0}đ ({(ConHang ? "Còn hàng" : "Hết hàng")})";
    }
}