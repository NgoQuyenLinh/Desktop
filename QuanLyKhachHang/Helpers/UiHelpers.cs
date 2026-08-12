using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;

namespace QuanLyKhachHang.Helpers
{
    /// <summary>
    /// Định nghĩa 1 cột trong bảng: tiêu đề, tỉ lệ độ rộng (Star) và hàm lấy giá trị hiển thị từ đối tượng T.
    /// </summary>
    public record ColDef<T>(string Header, double Width, Func<T, string> GetText);

    /// <summary>
    /// Helper dựng 1 "bảng dữ liệu" đơn giản (header cố định + ListBox cuộn được bên dưới),
    /// dùng thay cho DataGridView của WinForms vì Avalonia core không có sẵn control này.
    /// Cách dùng: gọi TaoBang(...) truyền vào 1 ListBox rỗng, hàm sẽ gán ItemsSource + ItemTemplate
    /// cho ListBox đó và trả về Control (DockPanel) gồm header + ListBox để add vào giao diện.
    /// </summary>
    public static class UiHelpers
    {
        public static Control TaoBang<T>(IReadOnlyList<T> duLieu, IReadOnlyList<ColDef<T>> cot, ListBox listBox)
        {
            var goc = new DockPanel();

            var header = new Grid
            {
                Background = new SolidColorBrush(Color.Parse("#E5E7EB")),
                Height = 34
            };
            foreach (var c in cot)
                header.ColumnDefinitions.Add(new ColumnDefinition(c.Width, GridUnitType.Star));

            for (int i = 0; i < cot.Count; i++)
            {
                var tb = new TextBlock
                {
                    Text = cot[i].Header,
                    FontWeight = FontWeight.Bold,
                    FontSize = 13,
                    Margin = new Thickness(10, 0, 4, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.Parse("#374151"))
                };
                Grid.SetColumn(tb, i);
                header.Children.Add(tb);
            }
            DockPanel.SetDock(header, Dock.Top);
            goc.Children.Add(header);

            listBox.Background = Brushes.White;
            listBox.ItemsSource = duLieu;
            listBox.ItemTemplate = new FuncDataTemplate<T>((item, _) =>
            {
                var row = new Grid();
                foreach (var c in cot)
                    row.ColumnDefinitions.Add(new ColumnDefinition(c.Width, GridUnitType.Star));

                for (int i = 0; i < cot.Count; i++)
                {
                    var tb = new TextBlock
                    {
                        Text = item != null ? cot[i].GetText(item) : string.Empty,
                        FontSize = 13,
                        Margin = new Thickness(10, 7, 4, 7),
                        VerticalAlignment = VerticalAlignment.Center,
                        TextWrapping = TextWrapping.NoWrap,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                    Grid.SetColumn(tb, i);
                    row.Children.Add(tb);
                }
                return row;
            });

            DockPanel.SetDock(listBox, Dock.Bottom);
            goc.Children.Add(listBox);

            return goc;
        }
    }
}
