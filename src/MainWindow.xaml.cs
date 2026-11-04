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
    private bool _restoreAdded;
    private bool _restoreRequested;
    private bool _tutorialCompleted;
    private bool _closing;
    private readonly bool _preview;
    private string _selectedTask = "tutorial";
    private readonly List<(DateTime Time, string Message, string? Task)> _logs = [];
    private nint _handle;
    private const int StopHotkeyId = 0x4D54;
    private readonly string _stateDirectory = Path.Combine(AppContext.BaseDirectory, "data");
    private readonly string _evidenceDirectory = Environment.GetEnvironmentVariable("MAA_TUTORIAL_LOG_DIR")
        ?? Path.Combine(AppContext.BaseDirectory, "data", "logs", DateTime.Now.ToString("yyyyMMdd-HHmmss"));

    public MainWindow(bool preview = false)
    {
        _preview = preview;
        InitializeComponent();
        SourceInitialized += (_, _) =>
        {
            _handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(_handle)?.AddHook(HandleHotkey);
        };
        Closing += (_, _) => { _closing = true; _cancellation?.Cancel(); UnregisterHotKey(_handle, StopHotkeyId); };
        Loaded += (_, _) =>
        {
            Log(preview ? "界面预览：不会操作 MAA。"
                : "勾选任务后点击开始。需要恢复原主题时，通过左下加号添加。");
            UpdateControls();
            UpdateDetails();
        };
    }

    private void AddTask(object sender, RoutedEventArgs e)
    {
        AddButton.ContextMenu.PlacementTarget = AddButton;
        AddButton.ContextMenu.IsOpen = true;
    }

    private void AddRestore(object sender, RoutedEventArgs e)
    {
        if (_restoreAdded) return;
        _restoreAdded = true;
        _restoreRequested = true;
        RestoreRow.Visibility = Visibility.Visible;
        RestoreCheck.IsChecked = true;
        AddRestoreItem.IsEnabled = false;
        RestoreSelect.IsChecked = true;
        _selectedTask = "restore";
        if (_runner is not null) _runner.RestoreRequested = true;
        RestoreStatus.Text = _busy ? "等待教程完成" : "等待开始";
        Log(_busy ? "已添加恢复原主题任务，将在教程之后执行。"
            : "已添加恢复原主题任务，点击开始后执行。", "restore");
        UpdateControls();
        UpdateDetails();
    }

    private void SelectTask(object sender, RoutedEventArgs e)
    {
        _selectedTask = sender == RestoreSelect ? "restore" : "tutorial";
        UpdateDetails();
    }

    private void ChangeDetailTab(object sender, RoutedEventArgs e) => UpdateDetails();
    private void ChangeLogTab(object sender, RoutedEventArgs e) => RenderLogs();

    private void TaskEnabledChanged(object sender, RoutedEventArgs e)
    {
        _restoreRequested = _restoreAdded && RestoreCheck.IsChecked == true;
        if (_runner is not null) _runner.RestoreRequested = _restoreRequested;
        UpdateControls();
    }

    private void SelectAll(object sender, RoutedEventArgs e)
    {
        TutorialCheck.IsChecked = true;
        RestoreCheck.IsChecked = _restoreAdded;
        TaskEnabledChanged(sender, e);
    }

    private void ClearTasks(object sender, RoutedEventArgs e)
    {
        TutorialCheck.IsChecked = RestoreCheck.IsChecked = false;
        TaskEnabledChanged(sender, e);
    }

    private async void StartOrStop(object sender, RoutedEventArgs e)
    {
        if (_busy) { RequestStop(); return; }
        if (_preview) { Log("界面预览不会执行任务，请使用正常启动入口。"); return; }
        await StartRun(restoreOnly: TutorialCheck.IsChecked != true || (_tutorialCompleted && _restoreRequested));
    }

    private void RequestStop()
    {
        if (!_busy || _cancellation?.IsCancellationRequested != false) return;
        StatusText.Text = "正在停止……";
        StartButton.Content = "正在停止";
        StartButton.IsEnabled = false;
        _cancellation.Cancel();
    }

    private async Task StartRun(bool restoreOnly)
    {
        if (_busy || _closing || (TutorialCheck.IsChecked != true && !_restoreRequested)) return;
        _busy = true;
        UpdateControls();
        if (!restoreOnly) TutorialStatus.Text = "执行中";
        if (_restoreRequested) RestoreStatus.Text = restoreOnly ? "执行中" : "等待教程完成";
        _cancellation?.Dispose();
        _cancellation = new CancellationTokenSource();
        if (!RegisterHotKey(_handle, StopHotkeyId, 0, 0x77)) Log("F8 已被其他程序占用，仍可使用停止按钮。");
        Directory.CreateDirectory(_evidenceDirectory);
        _runner = new TutorialRunner(_stateDirectory, _evidenceDirectory, message =>
        {
            var task = restoreOnly || (_runner?.TutorialCompleted == true && !message.Contains("教程已完成")) ? "restore" : "tutorial";
            Dispatcher.BeginInvoke(() =>
            {
                if (_closing) return;
                StatusText.Text = message;
                if (task == "restore" && _restoreRequested) RestoreStatus.Text = "执行中";
                Log(message, task);
            });
        }) { RestoreRequested = _restoreRequested };
        try
        {
            var runner = _runner;
            var token = _cancellation.Token;
            await Task.Run(() => { if (restoreOnly) runner.RunRestore(token); else runner.Run(token); });
            if (_restoreRequested && runner.TutorialCompleted && !runner.ThemeRestored)
                await Task.Run(() => runner.RunRestore(token));
            _tutorialCompleted = restoreOnly || runner.TutorialCompleted;
            TutorialStatus.Text = _tutorialCompleted ? "已完成" : "未完成";
            if (runner.ThemeRestored) RestoreStatus.Text = "已恢复";
            StatusText.Text = runner.ThemeRestored ? "教程完成，原主题已恢复" : "教程已完成";
        }
        catch (OperationCanceledException)
        {
            _tutorialCompleted |= _runner.TutorialCompleted;
            StatusText.Text = "已停止，点击开始可继续";
            TutorialStatus.Text = _tutorialCompleted ? "已完成" : "已停止，可继续";
            if (_restoreRequested) RestoreStatus.Text = _runner.ThemeRestored ? "已恢复" : "已停止，可继续";
            Log("已停止操作。原主题记录已保留，点击开始可继续。", restoreOnly ? "restore" : "tutorial");
        }
        catch (Exception ex)
        {
            _tutorialCompleted |= _runner.TutorialCompleted;
            StatusText.Text = "需要处理后重新开始";
            TutorialStatus.Text = _tutorialCompleted ? "已完成" : "未完成";
            if (_restoreRequested) RestoreStatus.Text = "未完成";
            Log(ex.Message, restoreOnly ? "restore" : "tutorial");
            File.WriteAllText(Path.Combine(_evidenceDirectory, "last-error.txt"), ex.ToString());
        }
        finally
        {
            UnregisterHotKey(_handle, StopHotkeyId);
            _busy = false;
            UpdateControls();
        }
    }

    private void UpdateControls()
    {
        StartButton.Content = _busy ? "停止" : "开始";
        StartButton.IsEnabled = _busy || TutorialCheck.IsChecked == true || _restoreRequested;
        TutorialCheck.IsEnabled = RestoreCheck.IsEnabled = !_busy;
        SelectAllButton.IsEnabled = ClearButton.IsEnabled = !_busy;
        ThemeAfterText.Text = _restoreRequested ? "恢复操作前的主题" : "保持深色";
    }

    private void UpdateDetails()
    {
        var restore = _selectedTask == "restore";
        TutorialDetails.Visibility = restore ? Visibility.Collapsed : Visibility.Visible;
        RestoreDetails.Visibility = restore ? Visibility.Visible : Visibility.Collapsed;
        DetailHelpText.Text = (restore, AdvancedTab.IsChecked == true) switch
        {
            (false, false) => "使用说明：\n先打开 MAA 新手教程，再点击左下方开始。\n自动记录原主题，切为深色后完成教程。\n运行中可点击停止，或按 F8。",
            (false, true) => "适配版本：MAA v6.21.3 简体中文。\n仅操作一个已打开的 MAA 窗口。\n运行时保持 MAA 可见，请勿同时操作鼠标键盘。\n异常或中断后，处理提示的问题，再点击开始继续。",
            (true, false) => "恢复的是目标 MAA 的主题。\n包含亮色、深色和与系统同步。\n勾选后会在教程完成后恢复；也可以完成教程后再添加并点击开始。\n不勾选则保持深色。",
            (true, true) => "只使用当前 MAA 进程的原主题记录。\n原选项在第一次切换前保存，中断后继续使用。\nMAA 重启后，不会将旧记录套用到新进程。\n恢复失败时保留记录，可处理后重新开始。"
        };
        RenderLogs();
    }

    private void Log(string message, string? task = null)
    {
        _logs.Add((DateTime.Now, message, task));
        RenderLogs();
        Directory.CreateDirectory(_evidenceDirectory);
        File.AppendAllText(Path.Combine(_evidenceDirectory, "hud.log"), $"{DateTimeOffset.Now:O} {message}\n");
    }

    private void RenderLogs()
    {
        var entries = TaskLogsTab.IsChecked == true ? _logs.Where(entry => entry.Task == _selectedTask) : _logs;
        LogText.Text = string.Join("\n", entries.Select(entry => $"{entry.Time:HH:mm:ss}  {entry.Message}"));
        if (LogText.Text.Length == 0) LogText.Text = "当前任务暂无日志。\n点击开始后在此显示执行记录。";
        LogText.ScrollToEnd();
    }

    private nint HandleHotkey(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == 0x0312 && wParam.ToInt32() == StopHotkeyId) { RequestStop(); handled = true; }
        return nint.Zero;
    }

    [DllImport("user32.dll")] private static extern bool RegisterHotKey(nint hwnd, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(nint hwnd, int id);
}
