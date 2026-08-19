using System;

namespace QuanLyKhachHang.Models
{
    public class QuaTang
    {
        public string MaQua { get; set; } = string.Empty;
        public string TenQua { get; set; } = string.Empty;
        public int DiemQuyDoi { get; set; }
        public int SoLuong { get; set; }

        /// <summary>
        /// Ngày quà được thêm vào kho. Dùng để xác định quà có thuộc nhóm
        /// "Quà trong tháng" (tháng/năm hiện tại) hay không, hiển thị bên KhoQuaView.
        /// </summary>
        public DateTime NgayTao { get; set; } = DateTime.Now;

        /// <summary>
        /// Trạng thái THỦ CÔNG: true = "Đang bán" (đang đưa ra để đổi cho khách),
        /// false = "Chưa bán" (còn nằm trong kho, chưa đem ra dùng).
        /// Người dùng có thể bấm nút để chuyển qua lại giữa 2 trạng thái bất cứ lúc nào
        /// tại màn hình Kho Quà. Mặc định quà mới thêm luôn ở trạng thái "Chưa bán".
        /// Khi quà được dùng để đổi trong 1 đơn hàng, hệ thống cũng tự đánh dấu
        /// "Đang bán" giúp người dùng đỡ phải bấm tay, nhưng vẫn chuyển lại được bất kỳ lúc nào.
        /// </summary>
        public bool DangBan { get; set; } = false;

        public override string ToString() => $"{TenQua} ({DiemQuyDoi} điểm) - Còn: {SoLuong}";
    }
}