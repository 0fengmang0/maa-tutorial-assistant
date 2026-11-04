using System.IO;
using System.Text.Json;

namespace MaaTutorialAssistant;

public sealed record SessionState(string Executable, int ProcessId, long StartedUtcTicks, string OriginalTheme)
{
    public bool TutorialCompleted { get; set; }
    public bool Restored { get; set; }

    public static string FileName(WindowTarget target) => $"theme-{target.ProcessId}-{target.StartedUtcTicks}.json";

    public static SessionState? Load(string directory, WindowTarget target)
    {
        var path = Path.Combine(directory, FileName(target));
        if (!File.Exists(path)) return null;
        var state = JsonSerializer.Deserialize<SessionState>(File.ReadAllText(path));
        if (state is null || state.Executable != target.Executable || state.ProcessId != target.ProcessId || state.StartedUtcTicks != target.StartedUtcTicks)
            throw new InvalidDataException("原主题记录与目标 MAA 不一致。");
        if (state.OriginalTheme is not ("Dark" or "Light" or "SyncWithOs")) throw new InvalidDataException("原主题记录格式无效。");
        return state;
    }

    public void Save(string directory, WindowTarget target)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, FileName(target));
        var staging = path + ".writing";
        File.WriteAllText(staging, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(staging, path, overwrite: true);
    }
}
