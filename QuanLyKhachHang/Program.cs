using QuanLyKhachHang.Forms;

namespace QuanLyKhachHang
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
