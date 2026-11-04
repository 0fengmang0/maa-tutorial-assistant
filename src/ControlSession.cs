using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using MaaFramework.Binding;
using MaaFramework.Binding.Buffers;

namespace MaaTutorialAssistant;

public record WindowTarget([property: JsonIgnore] nint Handle, int ProcessId, long StartedUtcTicks, string Executable, string Title)
{
    public long WindowHandle => Handle.ToInt64();
}
public record ScreenText(string Text, int X, int Y, int Width, int Height, double Score)
{
    public int CenterX => X + Width / 2;
    public int CenterY => Y + Height / 2;
    public string Normalized => string.Concat(Text.Where(c => !char.IsWhiteSpace(c)));
}

public sealed class ControlSession : IDisposable
{
    public WindowTarget Target { get; }
    public MaaTasker Tasker { get; }
    public MaaWin32Controller Controller { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public byte[] LastPng { get; private set; } = [];
    public string EvidenceDirectory { get; }

    public static IReadOnlyList<WindowTarget> FindTargets()
    {
        var targets = new List<WindowTarget>();
        using var windows = MaaToolkit.Shared.Desktop.Window.Find();
        foreach (var window in windows)
        {
            try
            {
                GetWindowThreadProcessId(window.Handle, out var id);
                using var process = Process.GetProcessById((int)id);
                if (!process.ProcessName.Equals("MAA", StringComparison.OrdinalIgnoreCase)) continue;
                if (process.MainWindowHandle != window.Handle || !window.Name.StartsWith("MAA")) continue;
                targets.Add(new(window.Handle, process.Id, process.StartTime.ToUniversalTime().Ticks,
                    process.MainModule?.FileName ?? "", window.Name));
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception) { }
        }
        return targets;
    }

    public ControlSession(WindowTarget target, string evidenceDirectory, CancellationToken token = default)
    {
        Target = target;
        EvidenceDirectory = evidenceDirectory;
        Directory.CreateDirectory(evidenceDirectory);
        MaaToolkit.Shared.Config.InitOption(evidenceDirectory);
        Controller = new MaaWin32Controller(target.Handle, Win32ScreencapMethods.ScreenDC,
            Win32InputMethod.LegacyEvent, Win32InputMethod.Seize, link: LinkOption.None);
        MaaResource? resource = null;
        MaaTasker? tasker = null;
        try
        {
            Check(Controller.LinkStart(), token);
            resource = new MaaResource();
            resource.SetOption_InferenceExecutionProvider(InferenceExecutionProvider.CPU);
            resource.SetOption_InferenceDevice(InferenceDevice.CPU);
            Check(resource.AppendBundle(Path.Combine(AppContext.BaseDirectory, "resources")), token);
            tasker = new MaaTasker { Controller = Controller, Resource = resource, DisposeOptions = DisposeOptions.All };
            if (!tasker.IsInitialized) throw new InvalidOperationException("MaaFramework 初始化失败。");
            Tasker = tasker;
        }
        catch
        {
            if (tasker is not null) tasker.Dispose();
            else { resource?.Dispose(); Controller.Dispose(); }
            throw;
        }
    }

    public void VerifyTarget()
    {
        if (!IsWindow(Target.Handle)) throw new InvalidOperationException("目标 MAA 窗口已经关闭。");
        GetWindowThreadProcessId(Target.Handle, out var id);
        using var process = Process.GetProcessById((int)id);
        if (process.Id != Target.ProcessId || process.StartTime.ToUniversalTime().Ticks != Target.StartedUtcTicks)
            throw new InvalidOperationException("目标窗口身份已变化，已停止输入。");
    }

    public IReadOnlyList<ScreenText> Read(CancellationToken token)
    {
        VerifyTarget();
        token.ThrowIfCancellationRequested();
        Check(Controller.Screencap(), token);
        using var image = new MaaImageBuffer();
        if (!Controller.GetCachedImage(image)) throw new InvalidOperationException("无法读取窗口截图。");
        Width = image.Width;
        Height = image.Height;
        if (image.TryGetEncodedData(out byte[]? png)) LastPng = png;
        var job = Tasker.AppendRecognition("OCR", "{\"expected\":[\".+\"],\"threshold\":0.5}", image);
        Check(job, token);
        var node = job.QueryNodeDetail() ?? throw new InvalidOperationException("OCR 没有返回识别节点。");
        if (!Tasker.GetRecognitionDetail<MaaImageBuffer>(node.RecognitionId, out _, out _, out _, null, out var detail, null, null))
            throw new InvalidOperationException("无法读取 OCR 识别结果。");
        using var json = JsonDocument.Parse(detail);
        var result = new List<ScreenText>();
        if (!json.RootElement.TryGetProperty("all", out var entries)) return result;
        foreach (var entry in entries.EnumerateArray())
        {
            var box = entry.GetProperty("box").EnumerateArray().Select(v => v.GetInt32()).ToArray();
            result.Add(new(entry.GetProperty("text").GetString() ?? "", box[0], box[1], box[2], box[3], entry.GetProperty("score").GetDouble()));
        }
        token.ThrowIfCancellationRequested();
        return result;
    }

    public void Click(int x, int y, CancellationToken token, bool right = false)
    {
        token.ThrowIfCancellationRequested();
        VerifyTarget();
        Check(Controller.Click(x, y, right ? 1 : 0), token);
    }

    public bool IsChecked(int x, int y, CancellationToken token)
    {
        using var image = new MaaImageBuffer();
        if (!Controller.GetCachedImage(image)) throw new InvalidOperationException("无法检查复选框状态。");
        var options = JsonSerializer.Serialize(new { roi = new[] { x - 6, y - 6, 12, 12 },
            lower = new[] { 20, 70, 160 }, upper = new[] { 100, 180, 255 }, count = 15 });
        var job = Tasker.AppendRecognition("ColorMatch", options, image);
        Check(job, token);
        var node = job.QueryNodeDetail() ?? throw new InvalidOperationException("复选框颜色识别失败。");
        if (!Tasker.GetRecognitionDetail<MaaImageBuffer>(node.RecognitionId, out _, out _, out var hit, null, out _, null, null))
            throw new InvalidOperationException("无法读取复选框识别结果。");
        return hit;
    }

    public void Key(int key, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); VerifyTarget(); Check(Controller.ClickKey(key), token);
    }

    public void Input(string text, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); VerifyTarget(); Check(Controller.InputText(text), token);
    }

    public void Chord(int modifier, int key, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); VerifyTarget();
        Check(Controller.KeyDown(modifier), token);
        try { Check(Controller.ClickKey(key), token); }
        finally { Controller.KeyUp(modifier).Wait(); }
    }

    public void Hover(int x, int y, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); VerifyTarget(); Check(Controller.TouchMove(0, x, y), token);
    }

    public void Stop() => Tasker.Stop();

    public void SaveEvidence(string name, IReadOnlyList<ScreenText> text)
    {
        File.WriteAllBytes(Path.Combine(EvidenceDirectory, name + ".png"), LastPng);
        File.WriteAllText(Path.Combine(EvidenceDirectory, name + ".json"), JsonSerializer.Serialize(text, new JsonSerializerOptions { WriteIndented = true }));
    }

    public ControlSession OpenOwnedDialog(string title, CancellationToken token)
    {
        using var windows = MaaToolkit.Shared.Desktop.Window.Find();
        var matches = new List<nint>();
        foreach (var window in windows)
        {
            GetWindowThreadProcessId(window.Handle, out var id);
            if (id == Target.ProcessId && window.Name.Contains(title) && GetWindow(window.Handle, 4) == Target.Handle)
                matches.Add(window.Handle);
        }
        if (matches.Count != 1) throw new InvalidOperationException("未确认唯一的 MAA 对话框：" + title);
        var dialogTarget = Target with { Handle = matches[0], Title = title };
        return new ControlSession(dialogTarget, Path.Combine(EvidenceDirectory, "rename-dialog"), token);
    }

    public static void Probe(string output)
    {
        var candidates = FindTargets();
        if (candidates.Count != 1) throw new InvalidOperationException($"需要唯一 MAA 窗口，当前找到 {candidates.Count} 个。");
        using var session = new ControlSession(candidates[0], Path.GetDirectoryName(Path.GetFullPath(output))!);
        var screen = session.Read(CancellationToken.None);
        File.WriteAllBytes(output + ".png", session.LastPng);
        GetWindowRect(candidates[0].Handle, out var rect);
        DwmGetWindowAttribute(candidates[0].Handle, 9, out var frame, Marshal.SizeOf<NativeRect>());
        File.WriteAllText(output, JsonSerializer.Serialize(new { Framework = MaaGlobal.Shared.Version, session.Target, session.Width, session.Height,
            Dpi = GetDpiForWindow(candidates[0].Handle), WindowBounds = new[] { rect.Left, rect.Top, rect.Right, rect.Bottom },
            PhysicalBounds = new[] { frame.Left, frame.Top, frame.Right, frame.Bottom }, Text = screen }, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void Check(MaaJob job, CancellationToken token = default)
    {
        var elapsed = Stopwatch.StartNew();
        var status = job.Status;
        while (status is MaaJobStatus.Pending or MaaJobStatus.Running)
        {
            token.ThrowIfCancellationRequested();
            if (elapsed.Elapsed > TimeSpan.FromSeconds(20)) throw new TimeoutException("MaaFramework 操作超过 20 秒，已停止后续输入。");
            Thread.Sleep(40);
            status = job.Status;
        }
        if (status != MaaJobStatus.Succeeded) throw new InvalidOperationException($"MaaFramework 操作失败：{status}");
    }

    public void Dispose() => Tasker.Dispose();

    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindow(nint hWnd);
    [DllImport("user32.dll")] private static extern uint GetDpiForWindow(nint hWnd);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(nint hWnd, out NativeRect rect);
    [DllImport("user32.dll")] private static extern nint GetWindow(nint hWnd, uint command);
    [DllImport("dwmapi.dll")] private static extern int DwmGetWindowAttribute(nint hWnd, int attribute, out NativeRect rect, int size);
    [StructLayout(LayoutKind.Sequential)] private struct NativeRect { public int Left, Top, Right, Bottom; }
}
