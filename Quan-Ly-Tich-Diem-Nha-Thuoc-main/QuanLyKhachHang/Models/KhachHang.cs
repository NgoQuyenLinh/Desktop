using System;

namespace QuanLyKhachHang.Models
{
    /// <summary>
    /// Model đại diện cho 1 khách hàng.
    /// Đây là lớp dữ liệu thuần (POCO), được (de)serialize trực tiếp
    /// từ/ra file Data/khachhang.json bằng System.Text.Json.
    /// </summary>
    public class KhachHang
    {
        public string MaKH { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public int DiemTichLuy { get; set; }
        public DateTime NgayTao { get; set; }

        public override string ToString() => $"{MaKH} - {HoTen}";
    }
}
