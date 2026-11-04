using System.Windows;

namespace MaaTutorialAssistant;

public partial class MainWindow : HandyControl.Controls.Window
{
    public MainWindow()
    {
        InitializeComponent();
        LogText.Text = "框架接入开发中。尚未执行教程操作。";
    }
    private void AddTask(object sender, RoutedEventArgs e) => AddButton.ContextMenu.IsOpen = true;
    private void AddRestore(object sender, RoutedEventArgs e)
    {
        RestoreRow.Visibility = Visibility.Visible;
        AddButton.IsEnabled = false;
        LogText.AppendText("\n已添加恢复原主题任务。");
    }
    private void Stop(object sender, RoutedEventArgs e) => StatusText.Text = "已停止";
    private void Retry(object sender, RoutedEventArgs e) => StatusText.Text = "框架接入开发中";
}
