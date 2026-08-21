namespace QuanLyKhachHang.Models
{
    public class GhiChu
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string NoiDung { get; set; } = string.Empty;
        public bool DaHoanThanh { get; set; }
        public string MauSac { get; set; } = "#3B82F6";
    }
}