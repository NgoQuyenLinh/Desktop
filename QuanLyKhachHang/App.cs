using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using QuanLyKhachHang.Views;

namespace QuanLyKhachHang
{
    /// <summary>
    /// Lớp Application gốc của Avalonia. Toàn bộ giao diện được dựng bằng code C#
    /// (không dùng file .axaml) để giữ phong cách gần giống WinForms code-behind,
    /// dễ đọc và dễ giải thích khi thuyết trình.
    /// </summary>
    public class App : Application
    {
        public override void Initialize()
        {
            // Áp dụng theme Fluent (giao diện hiện đại, có sẵn trong Avalonia)
            Styles.Add(new FluentTheme());
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
