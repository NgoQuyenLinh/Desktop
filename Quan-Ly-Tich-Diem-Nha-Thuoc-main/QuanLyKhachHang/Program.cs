using Avalonia;

namespace QuanLyKhachHang
{
    internal static class Program
    {
        // Điểm khởi động ứng dụng. Avalonia không yêu cầu STAThread như WinForms
        // nhưng để thuộc tính này cũng không gây hại và giữ quen thuộc với WinForms.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp() => AppBuilder
            .Configure<App>()
            .UsePlatformDetect()   // tự nhận diện macOS / Windows / Linux để chọn backend vẽ phù hợp
            .WithInterFont()
            .LogToTrace();
    }
}
