using System;

namespace QuanLyKhachHang.Models
{
    public class QuaTang
    {
        public string MaQua { get; set; } = string.Empty;
        public string TenQua { get; set; } = string.Empty;
        public int DiemQuyDoi { get; set; }
        public int SoLuong { get; set; }

        public override string ToString() => $"{TenQua} ({DiemQuyDoi} điểm) - Còn: {SoLuong}";
    }
}
