using System;
using System.Collections.Generic;

namespace QuanLyKhachHang.Models
{
    /// <summary>
    /// Model đại diện cho 1 đơn hàng. Mỗi đơn hàng gắn với 1 khách hàng
    /// (thông qua MaKH) và ghi lại số điểm được cộng / đã dùng cho đơn đó,
    /// cùng tổng điểm của khách sau khi giao dịch hoàn tất (để tiện xem lịch sử).
    /// </summary>
    public class DonHang
    {
        public string MaDon { get; set; } = string.Empty;
        public string MaKH { get; set; } = string.Empty;
        public string TenKH { get; set; } = string.Empty;

        /// <summary>
        /// Danh sách các loại thuốc trong đơn hàng này (1 đơn có thể chứa nhiều loại thuốc).
        /// Đơn hàng cũ (tạo trước khi có chức năng Quản lý thuốc) sẽ có danh sách rỗng;
        /// khi đó SoTien vẫn giữ nguyên giá trị đã lưu trước đó để không phá vỡ lịch sử cũ.
        /// </summary>
        public List<ChiTietDonHang> DanhSachThuoc { get; set; } = new();

        /// <summary>Tổng tiền đơn hàng (bằng tổng Thành tiền của các thuốc trong DanhSachThuoc).</summary>
        public decimal SoTien { get; set; }
        public DateTime NgayTao { get; set; }
        public int DiemCong { get; set; }
        public int DiemSuDung { get; set; }
        public int TongDiemSauGiaoDich { get; set; }
        public string QuaTangDoi { get; set; } = string.Empty;
        public int DiemDoiQua { get; set; }

        /// <summary>Số tiền thực khách phải trả sau khi trừ điểm sử dụng (1 điểm = 1.000đ).</summary>
        public decimal ThanhTien => SoTien - (DiemSuDung * 1000);
    }
}
