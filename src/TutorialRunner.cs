using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MaaTutorialAssistant;

public sealed class TutorialRunner(string stateDirectory, string evidenceDirectory, Action<string> report)
{
    private ControlSession? _session;
    private CancellationToken _token;
    private IReadOnlyList<ScreenText> _screen = [];
    private SessionState? _state;
    private int _evidenceIndex;
    public volatile bool RestoreRequested;
    public bool TutorialCompleted { get; private set; }
    public bool ThemeRestored { get; private set; }

    public void Run(CancellationToken token, bool themeTestOnly = false, bool fixtureOnly = false)
    {
        _token = token;
        var target = WaitForTarget();
        if (fixtureOnly && !File.Exists(Path.Combine(Path.GetDirectoryName(target.Executable)!, ".maa-tutorial-fixture.json")))
            throw new InvalidOperationException("命令行回归仅允许操作带项目标记的测试副本。");
        report("已找到 MAA，正在检查主题。");
        using var session = new ControlSession(target, evidenceDirectory, token);
        _session = session;
        try
        {
            if (!target.Title.Contains("v6.21.3")) throw new InvalidOperationException("当前版本仅验证了 MAA v6.21.3，请使用对应版本。");
            var saved = SessionState.Load(stateDirectory, target);
            Read();
            if (Has("请输入新的任务名称")) FinishRenameDialog();
            if (!Has("设置指引"))
            {
                Tap(Require("长草", t => t.Y < session.Height * .1));
                if (!Has("设置指引") && saved is { TutorialCompleted: true })
                {
                    _state = saved;
                    TutorialCompleted = true;
                    report("当前 MAA 的教程已由本助手完成。");
                    if (RestoreRequested && !saved.Restored) RestoreTheme();
                    return;
                }
                WaitFor(() => Has("设置指引"), "目标 MAA 当前不在新手教程中，请打开教程后重试。");
            }
            OpenThemeSettings();
            var currentTheme = ReadTheme();
            _state = saved is { Restored: false } ? saved : new(target.Executable, target.ProcessId, target.StartedUtcTicks, currentTheme);
            _state.Save(stateDirectory, target);
            report("已保存原主题：" + ThemeLabel(_state.OriginalTheme));
            SetTheme("Dark");
            if (themeTestOnly)
            {
                RestoreTheme();
                ReturnToTutorial();
                report("主题切换与恢复验证完成。");
                return;
            }
            ReturnToTutorial();
            CompleteTutorial();
            _state.TutorialCompleted = true;
            _state.Save(stateDirectory, target);
            TutorialCompleted = true;
            report("MAA 新手教程已完成。");
            if (RestoreRequested) RestoreTheme();
        }
        catch
        {
            if (session.LastPng.Length > 0) Capture("interrupted");
            session.Stop();
            throw;
        }
        finally { _session = null; }
    }

    public void RunRestore(CancellationToken token)
    {
        _token = token;
        var target = WaitForTarget();
        _state = SessionState.Load(stateDirectory, target) ?? throw new InvalidOperationException("没有当前 MAA 的原主题记录。");
        if (!_state.TutorialCompleted) throw new InvalidOperationException("教程尚未完成，请先完成或重试教程任务。");
        using var session = new ControlSession(target, evidenceDirectory, token);
        _session = session;
        try { OpenThemeSettings(); RestoreTheme(); }
        catch { session.Stop(); throw; }
        finally { _session = null; }
    }

    private WindowTarget WaitForTarget()
    {
        var timer = Stopwatch.StartNew();
        while (timer.Elapsed < TimeSpan.FromSeconds(30))
        {
            _token.ThrowIfCancellationRequested();
            var candidates = ControlSession.FindTargets();
            if (candidates.Count > 1) throw new InvalidOperationException("找到多个 MAA 窗口，请只保留一个需要操作的窗口。");
            if (candidates.Count == 1) return candidates[0];
            report("正在寻找 MAA 教程窗口……");
            Pause(1000);
        }
        throw new TimeoutException("未找到 MAA 窗口，请先打开 MAA 新手教程，再点击重试。");
    }

    private void Read()
    {
        var waiting = Stopwatch.StartNew();
        do
        {
            _token.ThrowIfCancellationRequested();
            _screen = _session!.Read(_token);
            if (_screen.Any(t => t.Y < _session.Height * .10 && t.Text.Contains("MAA"))) return;
            // A newly created WPF window can be discoverable before its first rendered frame.
            Pause(250);
        } while (waiting.Elapsed < TimeSpan.FromSeconds(5));
        throw new InvalidOperationException("截图中未确认 MAA 标题，已停止输入。请将目标窗口置于前台后重试。");
    }

    private bool Has(string text) => _screen.Any(t => t.Normalized.Contains(text));
    private ScreenText? Find(string text, Func<ScreenText, bool>? where = null, bool exact = false)
        => _screen.Where(t => (exact ? t.Normalized == text : t.Normalized.Contains(text)) && (where?.Invoke(t) ?? true))
            .OrderBy(t => t.Width).FirstOrDefault();

    private ScreenText Require(string text, Func<ScreenText, bool>? where = null, bool exact = false)
        => Find(text, where, exact) ?? throw new InvalidOperationException("未识别到「" + text + "」，已停止输入。");

    private void Tap(ScreenText text, bool right = false) => Tap(text.CenterX, text.CenterY, right);
    private void Tap(int x, int y, bool right = false)
    {
        _session!.Click(x, y, _token, right);
        Pause(450);
        Read();
    }

    private void Pause(int milliseconds)
    {
        if (_token.WaitHandle.WaitOne(milliseconds)) _token.ThrowIfCancellationRequested();
    }

    private void WaitFor(Func<bool> condition, string message)
    {
        var timeout = Stopwatch.StartNew();
        while (!condition())
        {
            if (timeout.Elapsed > TimeSpan.FromSeconds(5)) throw new InvalidOperationException(message);
            Pause(250);
            Read();
        }
    }

    private void Capture(string name)
        => _session!.SaveEvidence($"{++_evidenceIndex:D3}-{name}", _screen);

    private void OpenThemeSettings()
    {
        Read();
        Tap(Require("设置", t => t.Y < _session!.Height * .1, exact: true));
        WaitFor(() => Find("界面设置", t => t.X < _session!.Width * .25) is not null, "MAA 设置页未打开。");
        Tap(Require("界面设置", t => t.X < _session!.Width * .25));
        WaitFor(() => Has("界面主题"), "界面主题设置未显示。");
        _ = ReadTheme();
    }

    private string ReadTheme()
    {
        var label = Require("界面主题", t => t.X > _session!.Width * .2);
        foreach (var theme in new[] { "SyncWithOs", "Light", "Dark" })
            if (Find(ThemeLabel(theme), t => t.Y > label.Y && t.Y < label.Y + _session!.Height * .10, exact: true) is not null) return theme;
        throw new InvalidOperationException("无法读取 MAA 原始主题选项，未修改主题。");
    }

    private void SetTheme(string theme)
    {
        var current = ReadTheme();
        if (current == theme) { report("当前主题：" + ThemeLabel(theme)); return; }
        Tap(Require(ThemeLabel(current), exact: true));
        _session!.Key(0x24, _token); // Home: upstream order is Light, Dark, SyncWithOs.
        for (var i = 0; i < (theme == "Light" ? 0 : theme == "Dark" ? 1 : 2); i++) _session.Key(0x28, _token);
        _session.Key(0x0D, _token);
        Pause(600);
        Read();
        if (ReadTheme() != theme) throw new InvalidOperationException("主题切换未生效，已停止后续操作。");
        Capture("theme-" + theme);
        report("已切换为" + ThemeLabel(theme) + "。");
    }

    private void RestoreTheme()
    {
        OpenThemeSettings();
        SetTheme(_state!.OriginalTheme);
        _state.Restored = true;
        _state.Save(stateDirectory, _session!.Target);
        ThemeRestored = true;
        report("已恢复原主题：" + ThemeLabel(_state.OriginalTheme));
        Capture("theme-restored");
    }

    private void ReturnToTutorial()
    {
        Tap(Require("长草", t => t.Y < _session!.Height * .1));
        WaitFor(() => Has("设置指引"), "无法返回 MAA 新手教程。");
    }

    private void CompleteTutorial()
    {
        var timer = Stopwatch.StartNew();
        var previous = "";
        var unchanged = 0;
        for (var iteration = 0; iteration < 100 && timer.Elapsed < TimeSpan.FromMinutes(8); iteration++)
        {
            Read();
            Capture("tutorial");
            if (!Has("设置指引"))
            {
                if (Has("LinkStart") || Has("添加任务") || Has("添加新任务")) return;
                throw new InvalidOperationException("教程已退出，但未识别到正常任务界面。");
            }
            var signature = string.Join("|", _screen.Select(t => t.Normalized));
            unchanged = signature == previous ? unchanged + 1 : 0;
            previous = signature;
            if (unchanged >= 3) throw new InvalidOperationException("教程页面未按预期变化，已停止重复操作。");
            var next = NextButton();
            var done = Find("完成", t => t.X > _session!.Width * .82 && t.Y > _session.Height * .2);
            if (next is null && done is not null)
            {
                report("正在完成最后一页。");
                Tap(done);
                WaitFor(() => !Has("设置指引"), "点击完成后教程未退出。");
                continue;
            }
            if (next is null) throw new InvalidOperationException("未识别到教程的下一步按钮。");
            var gate = string.Concat(_screen.Where(t => t.X > _session!.Width * .80 && t.Y > next.Y + next.Height + 5)
                .OrderBy(t => t.Y).ThenBy(t => t.X).Select(t => t.Normalized));
            if (gate.Contains("协议") || gate.Contains("请先阅读"))
            {
                report("正在打开教程要求的用户协议。");
                var agreement = Require("用户协议", t => t.X < _session!.Width * .82 && t.Y > _session.Height * .25);
                // The final four characters of this OCR line are the agreement hyperlink.
                Tap(agreement.X + agreement.Width - Math.Min(agreement.Width / 2, agreement.Height * 2), agreement.CenterY);
                Pause(400);
                Read();
                continue;
            }
            if (_screen.Any(t => t.X < _session!.Width * .65 && t.Normalized.Contains("理智作战")) && !string.IsNullOrEmpty(gate))
            {
                CompleteTaskGate(gate);
                continue;
            }
            report("正在进入下一页。");
            Tap(next);
            Pause(250);
        }
        throw new TimeoutException("教程流程超过运行限制，已停止。");
    }

    private void CompleteTaskGate(string gate)
    {
        report("任务演示：" + gate);
        if ((gate.Contains("添加一个") || gate.Contains("重新添加")) && gate.Contains("理智"))
        {
            AddDemoTask("理智作战");
            return;
        }
        if (gate.Contains("关卡") && gate.Contains("剿灭"))
        {
            SelectDemoGear("理智作战", last: true);
            var stage = Find("当前/上次", t => t.X > _session!.Width * .35 && t.X < _session.Width * .80)
                ?? Find("当前", t => t.X > _session!.Width * .35 && t.X < _session.Width * .80);
            Tap(stage ?? throw new InvalidOperationException("未识别到演示关卡选择框。"));
            Tap(Require("当期剿灭", t => t.X < _session!.Width * .80));
            return;
        }
        if (gate.Contains("重命名") && !gate.Contains("移到"))
        {
            OpenDemoTaskMenu("理智作战", last: true);
            Tap(Require("重命名", t => t.X < _session!.Width * .8));
            Capture("rename-dialog");
            WaitFor(() => Has("请输入新的任务名称"), "重命名对话框未打开。");
            FinishRenameDialog();
            return;
        }
        if (gate.Contains("移到") && gate.Contains("理智"))
        {
            OpenDemoTaskMenu("教程任务");
            Tap(Require("上移", t => t.X < _session!.Width * .8));
            return;
        }
        if (gate.Contains("周计划") && !gate.Contains("周一") && !gate.Contains("周日"))
        {
            SelectDemoGear("教程任务");
            Tap(Require("高级设置", t => t.X < _session!.Width * .8));
            Tap(Require("启用周计划", t => t.X < _session!.Width * .8));
            return;
        }
        if (gate.Contains("周一") || gate.Contains("周日"))
        {
            var sunday = Require("星期日", t => t.X < _session!.Width * .8);
            var tuesday = Require("星期二", t => t.X < _session!.Width * .8);
            var x = sunday.X - (int)(_session!.Width * .014);
            var mondayY = (sunday.CenterY + tuesday.CenterY) / 2;
            if (!_session.IsChecked(x, sunday.CenterY, _token)) Tap(x, sunday.CenterY);
            if (!_session.IsChecked(x, mondayY, _token)) Tap(x, mondayY);
            return;
        }
        if (gate.Contains("添加") && gate.Contains("更换主题") && !gate.Contains("夜间"))
        {
            AddDemoTask("更换主题");
            return;
        }
        if (gate.Contains("移到") && gate.Contains("最后"))
        {
            OpenDemoTaskMenu("更换主题");
            Tap(Require("下移", t => t.X < _session!.Width * .8));
            return;
        }
        if (gate.Contains("夜间"))
        {
            SelectDemoGear("更换主题");
            Tap(Require("添加主题", t => t.X < _session!.Width * .8));
            return;
        }
        throw new InvalidOperationException("未适配的任务演示提示：" + gate);
    }

    private ScreenText? NextButton()
    {
        var update = Find("更新设置", t => t.X > _session!.Width * .82);
        var performance = Find("性能设置", t => t.X > _session!.Width * .82);
        if (update is null || performance is null) return null;
        var stepGap = update.CenterY - performance.CenterY;
        var buttonY = update.CenterY + (int)(stepGap * .84);
        bool InButton(ScreenText t) => t.X > _session!.Width * .82 && Math.Abs(t.CenterY - buttonY) < stepGap * .45;
        if (_screen.Any(t => InButton(t) && t.Normalized.Contains("完成"))) return null;
        var direct = _screen.FirstOrDefault(t => InButton(t) && Regex.IsMatch(t.Normalized, "下.*步"));
        if (direct is not null) return direct;
        var down = _screen.FirstOrDefault(t => InButton(t) && t.Normalized.Contains('下'));
        if (down is null) return new("下一步", update.CenterX, buttonY - 8, stepGap, 16, 1);
        var step = _screen.FirstOrDefault(t => t.Normalized.Contains('步') && Math.Abs(t.CenterY - down.CenterY) < Math.Max(down.Height, t.Height)
            && t.X >= down.X && t.X - down.X < down.Height * 5);
        return step is null ? new("下一步", update.CenterX, buttonY - 8, stepGap, 16, 1)
            : new("下一步", down.X, Math.Min(down.Y, step.Y), step.X + step.Width - down.X, Math.Max(down.Height, step.Height), Math.Min(down.Score, step.Score));
    }

    private void FinishRenameDialog()
    {
        using (var dialog = _session!.OpenOwnedDialog("重命名任务", _token))
        {
            var text = dialog.Read(_token);
            var prompt = text.FirstOrDefault(t => t.Normalized.Contains("请输入新的任务名称"))
                ?? throw new InvalidOperationException("未确认重命名输入框。");
            var field = text.FirstOrDefault(t => t.Y > prompt.Y + prompt.Height && t.Y < dialog.Height * .65)
                ?? throw new InvalidOperationException("未识别到重命名文本框中的现有任务名。");
            dialog.Click(field.CenterX, field.CenterY, _token);
            dialog.Chord(0x11, 0x41, _token);
            dialog.Input("教程任务", _token);
            Pause(200);
            text = dialog.Read(_token);
            dialog.SaveEvidence("typed", text);
            var okay = text.FirstOrDefault(t => t.Normalized == "好的")
                ?? throw new InvalidOperationException("未识别到重命名确认按钮。");
            dialog.Click(okay.CenterX, okay.CenterY, _token);
        }
        Pause(400); Read();
        WaitFor(() => !Has("请输入新的任务名称") && Has("教程任务"), "任务重命名未生效。");
    }

    private ScreenText DemoTask(string name, bool last = false)
    {
        var tasks = _screen.Where(t => t.X < _session!.Width * .45 && t.Y > _session.Height * .2 && t.Normalized.Contains(name))
            .OrderBy(t => t.Y).ToArray();
        if (tasks.Length == 0) throw new InvalidOperationException("未识别到演示任务「" + name + "」。");
        return last ? tasks[^1] : tasks[0];
    }

    private int DemoGearX()
    {
        var clear = Require("清空", t => t.X < _session!.Width * .5);
        return clear.CenterX + (int)(_session!.Width * .032);
    }

    private void SelectDemoGear(string name, bool last = false)
    {
        var task = DemoTask(name, last);
        Tap(DemoGearX(), task.CenterY);
    }

    private void OpenDemoTaskMenu(string name, bool last = false)
    {
        var task = DemoTask(name, last);
        Tap(DemoGearX(), task.CenterY, right: true);
        Capture("task-menu");
    }

    private void AddDemoTask(string name)
    {
        var all = Require("全选", t => t.X < _session!.Width * .5);
        var clear = Require("清空", t => t.X < _session!.Width * .5);
        var plusX = all.CenterX - (int)((clear.CenterX - all.CenterX) * .70);
        Tap(plusX, all.CenterY);
        var entries = _screen.Where(t => t.Normalized == name && t.X < _session!.Width * .55).OrderByDescending(t => t.Y).ToArray();
        if (entries.Length == 0) throw new InvalidOperationException("添加菜单中没有识别到「" + name + "」。");
        Tap(entries[0]);
    }

    public static string ThemeLabel(string theme) => theme switch
    {
        "Dark" => "暗色", "Light" => "亮色", "SyncWithOs" => "与系统同步",
        _ => throw new ArgumentOutOfRangeException(nameof(theme))
    };
}
