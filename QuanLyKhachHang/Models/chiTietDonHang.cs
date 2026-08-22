namespace QuanLyKhachHang.Models
{
    /// <summary>
    /// Model đại diện cho 1 dòng thuốc bên trong 1 đơn hàng (1 đơn hàng có thể
    /// chứa nhiều dòng ChiTietDonHang, mỗi dòng ứng với 1 loại thuốc đã chọn).
    /// Lưu lại TenThuoc và DonGia tại thời điểm bán (không tham chiếu ngược lại
    /// Thuoc gốc) để lịch sử đơn hàng không bị đổi theo nếu sau này sửa/xoá thuốc.
    /// </summary>
    public class ChiTietDonHang
    {
        public string MaThuoc { get; set; } = string.Empty;
        public string TenThuoc { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }

        /// <summary>Thành tiền của dòng thuốc này = Số lượng × Đơn giá.</summary>
        public decimal ThanhTien => SoLuong * DonGia;
    }
}