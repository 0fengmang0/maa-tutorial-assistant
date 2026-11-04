using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace MaaTutorialAssistant;

public partial class MainWindow : HandyControl.Controls.Window
{
    private CancellationTokenSource? _cancellation;
    private TutorialRunner? _runner;
    private bool _busy;
    private bool _restoreRequested;
    private bool _tutorialCompleted;
    private bool _closing;
    private nint _handle;
    private const int StopHotkeyId = 0x4D54;
    private readonly string _stateDirectory = Path.Combine(AppContext.BaseDirectory, "data");
    private readonly string _evidenceDirectory = Environment.GetEnvironmentVariable("MAA_TUTORIAL_LOG_DIR")
        ?? Path.Combine(AppContext.BaseDirectory, "data", "logs", DateTime.Now.ToString("yyyyMMdd-HHmmss"));

    public MainWindow(bool autoRun = true)
    {
        InitializeComponent();
        SourceInitialized += (_, _) =>
        {
            _handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(_handle)?.AddHook(HandleHotkey);
        };
        Closing += (_, _) => { _closing = true; _cancellation?.Cancel(); UnregisterHotKey(_handle, StopHotkeyId); };
        Loaded += async (_, _) =>
        {
            Log("自动完成教程；需要恢复原主题时，可通过左侧加号添加任务。运行中按 F8 停止。");
            if (autoRun) { await Task.Delay(700); await StartRun(); }
            else { StatusText.Text = "界面预览"; StopButton.IsEnabled = false; }
        };
    }
    private void AddTask(object sender, RoutedEventArgs e) => AddButton.ContextMenu.IsOpen = true;
    private void AddRestore(object sender, RoutedEventArgs e)
    {
        RestoreRow.Visibility = Visibility.Visible;
        AddButton.IsEnabled = false;
        _restoreRequested = true;
        if (_runner is not null) _runner.RestoreRequested = true;
        Log("已添加恢复原主题任务，将在教程之后执行。");
        if (!_busy && _tutorialCompleted) _ = StartRun(restoreOnly: true);
    }
    private void Stop(object sender, RoutedEventArgs e) => _cancellation?.Cancel();
    private async void Retry(object sender, RoutedEventArgs e) => await StartRun(restoreOnly: _tutorialCompleted && _restoreRequested);

    private async Task StartRun(bool restoreOnly = false)
    {
        if (_busy || _closing) return;
        _busy = true;
        RetryButton.IsEnabled = false;
        StopButton.IsEnabled = true;
        TutorialStatus.Text = restoreOnly ? "已完成" : "执行中";
        _cancellation?.Dispose();
        _cancellation = new CancellationTokenSource();
        if (!RegisterHotKey(_handle, StopHotkeyId, 0, 0x77)) Log("F8 已被其他程序占用，仍可使用停止按钮。");
        Directory.CreateDirectory(_evidenceDirectory);
        _runner = new TutorialRunner(_stateDirectory, _evidenceDirectory,
            message => Dispatcher.BeginInvoke(() => { if (!_closing) { StatusText.Text = message; Log(message); } })) { RestoreRequested = _restoreRequested };
        try
        {
            var runner = _runner;
            var token = _cancellation.Token;
            await Task.Run(() => { if (restoreOnly) runner.RunRestore(token); else runner.Run(token); });
            _tutorialCompleted = restoreOnly || runner.TutorialCompleted;
            TutorialStatus.Text = _tutorialCompleted ? "已完成" : "未完成";
            if (runner.ThemeRestored) RestoreStatus.Text = "已恢复";
            StatusText.Text = runner.ThemeRestored ? "教程完成，原主题已恢复" : "教程已完成";
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "已停止";
            TutorialStatus.Text = _tutorialCompleted ? "已完成" : "已停止，可重试";
            Log("已停止操作。原主题记录已保留，点击重试可继续。");
        }
        catch (Exception ex)
        {
            StatusText.Text = "需要处理后重试";
            TutorialStatus.Text = _tutorialCompleted ? "已完成" : "未完成";
            Log(ex.Message);
            File.WriteAllText(Path.Combine(_evidenceDirectory, "last-error.txt"), ex.ToString());
        }
        finally
        {
            UnregisterHotKey(_handle, StopHotkeyId);
            _busy = false;
            RetryButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }
    }

    private void Log(string message)
    {
        LogText.AppendText($"{DateTime.Now:HH:mm:ss}  {message}\n");
        LogText.ScrollToEnd();
        Directory.CreateDirectory(_evidenceDirectory);
        File.AppendAllText(Path.Combine(_evidenceDirectory, "hud.log"), $"{DateTimeOffset.Now:O} {message}\n");
    }

    private nint HandleHotkey(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == 0x0312 && wParam.ToInt32() == StopHotkeyId) { _cancellation?.Cancel(); handled = true; }
        return nint.Zero;
    }

    [DllImport("user32.dll")] private static extern bool RegisterHotKey(nint hwnd, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(nint hwnd, int id);
}
