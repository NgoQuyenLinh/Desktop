# Phần mềm Quản lý Khách hàng & Tích điểm (WinForms, .NET 8)

Ứng dụng desktop viết bằng **C# WinForms (.NET 8)**, không dùng Database — toàn bộ dữ liệu
đọc/ghi trực tiếp từ 2 file JSON trong thư mục `Data/`. Phù hợp làm bài tập lập trình cơ bản
để thuyết trình các kiến thức: CRUD, tìm kiếm, xử lý file, tính toán điểm, lọc dữ liệu, thống kê.

## 1. Cấu trúc thư mục

```
QuanLyKhachHang/
├── QuanLyKhachHang.sln
└── QuanLyKhachHang/
    ├── QuanLyKhachHang.csproj
    ├── Program.cs                  # Điểm khởi động ứng dụng
    ├── Models/
    │   ├── KhachHang.cs             # Model khách hàng
    │   └── DonHang.cs               # Model đơn hàng
    ├── Services/
    │   └── DataService.cs           # Đọc/ghi JSON + toàn bộ nghiệp vụ (CRUD, tìm kiếm, tính điểm, thống kê)
    ├── Forms/
    │   ├── MainForm.cs              # Form chính: sidebar điều hướng + panel nội dung
    │   ├── ucTrangChu.cs            # Màn hình Trang chủ (tổng quan)
    │   ├── ucKhachHang.cs           # Màn hình Quản lý khách hàng (CRUD + tìm kiếm)
    │   ├── KhachHangEditForm.cs     # Form popup Thêm/Sửa khách hàng
    │   ├── ucDonHang.cs             # Màn hình Đơn hàng & Tích điểm
    │   └── ucThongKe.cs             # Màn hình Thống kê
    └── Data/
        ├── khachhang.json           # Dữ liệu mẫu: 10 khách hàng
        └── donhang.json             # Dữ liệu mẫu: 10 đơn hàng
```

## 2. Cách chạy chương trình

Yêu cầu: **Windows** + **.NET 8 SDK** (WinForms chỉ chạy trên Windows).

1. Cài .NET 8 SDK: https://dotnet.microsoft.com/download
2. Mở Terminal/PowerShell tại thư mục `QuanLyKhachHang/QuanLyKhachHang/` rồi chạy:
   ```
   dotnet restore
   dotnet run
   ```
   Hoặc mở file `QuanLyKhachHang.sln` bằng **Visual Studio 2022** (chọn workload
   ".NET desktop development"), nhấn **F5** để chạy.
3. Khi chạy lần đầu, chương trình tự đọc dữ liệu mẫu có sẵn trong `Data/khachhang.json`
   và `Data/donhang.json` (2 file này được copy tự động vào thư mục build nhờ cấu hình
   `CopyToOutputDirectory` trong file `.csproj`).

> Lưu ý: đây là code nguồn C#/WinForms — cần biên dịch bằng .NET SDK trên Windows để chạy;
> không thể chạy trực tiếp trong môi trường duyệt/Linux.

## 3. Giải thích cấu trúc & luồng hoạt động

- **Models**: `KhachHang`, `DonHang` là các lớp dữ liệu thuần (POCO), ánh xạ trực tiếp
  sang/từ JSON bằng `System.Text.Json`.
- **Services/DataService.cs**: là "trái tim" của ứng dụng, đóng vai trò database giả lập:
  - `TaiDuLieu()`: đọc 2 file JSON vào bộ nhớ (`List<KhachHang>`, `List<DonHang>`) khi khởi động.
  - Thêm/Sửa/Xoá khách hàng hoặc đơn hàng → cập nhật `List` trong bộ nhớ → gọi `LuuKhachHang()`
    / `LuuDonHang()` để ghi đè lại file JSON ngay lập tức.
  - `TimKiemKhachHang()`: lọc theo tên/SĐT/mã KH bằng `string.Contains`, không phân biệt hoa thường.
  - `TaoDonHang()`: nghiệp vụ tích điểm — kiểm tra không cho dùng điểm vượt quá điểm hiện có,
    tính điểm cộng = `SoTien / 1000` (làm tròn xuống), cập nhật điểm khách hàng.
  - Các hàm `SoLuongDonHang`, `TongDoanhThu`, `TongDiemDaTichLuy`, `TongDiemDaSuDung`,
    `TopKhachHangDiemCao`: phục vụ màn hình Thống kê, lọc theo khoảng ngày.
- **Forms**: mỗi màn hình là 1 `UserControl` (Trang chủ / Khách hàng / Đơn hàng / Thống kê),
  được `MainForm` hoán đổi qua lại trong 1 `Panel` nội dung khi bấm menu ở sidebar bên trái —
  giúp code gọn, dễ giải thích, không cần nhiều Form riêng lẻ.
  - `ucKhachHang`: DataGridView + ô tìm kiếm tức thời (sự kiện `TextChanged` → lọc lại ngay) +
    3 nút Thêm/Sửa/Xoá (Thêm/Sửa mở `KhachHangEditForm` dạng popup).
  - `ucDonHang`: bên trái là form tạo đơn hàng mới (chọn khách hàng, nhập tiền, tuỳ chọn dùng
    điểm), có ước tính điểm cộng & thành tiền theo thời gian thực; bên phải là danh sách đơn.
  - `ucThongKe`: 2 `DateTimePicker` chọn khoảng ngày, 4 thẻ số liệu tổng hợp, và bảng Top khách
    hàng điểm cao nhất.

## 4. Quy tắc tích điểm (đúng theo yêu cầu đề bài)

- Điểm được cộng = **Số tiền thanh toán / 1000** (làm tròn xuống).
- Không cho phép sử dụng số điểm **lớn hơn** số điểm khách đang có.
- 1 điểm sử dụng = giảm 1.000đ trên hoá đơn (thành tiền = số tiền − điểm dùng × 1.000).

## 5. Dữ liệu mẫu

`Data/khachhang.json` có sẵn 10 khách hàng, `Data/donhang.json` có sẵn 10 đơn hàng với
nhiều ngày khác nhau (từ 01/08/2025 đến 10/08/2025) và điểm tích luỹ khác nhau — dùng ngay
để demo tìm kiếm, tạo đơn mới, và xem thống kê theo khoảng thời gian mà không cần nhập liệu
thủ công trước khi thuyết trình.
# Desktop
