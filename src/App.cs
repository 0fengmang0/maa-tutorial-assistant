using System.IO;
using System.Windows;

namespace MaaTutorialAssistant;

public sealed class App : Application
{
    [STAThread]
    public static int Main(string[] args)
    {
        using var instance = new Mutex(true, "Local\\MaaTutorialAssistant", out var firstInstance);
        if (!firstInstance) { MessageBox.Show("教程助手已经运行。", "MAA 教程助手"); return 1; }
        var app = new App();
        try
        {
            if (args.Length == 2 && args[0] == "--probe")
            {
                ControlSession.Probe(args[1]);
                return 0;
            }
            if (args.Length >= 2 && args[0] is "--theme-test" or "--run-fixture")
            {
                var directory = Path.GetFullPath(args[1]);
                Directory.CreateDirectory(directory);
                var logPath = Path.Combine(directory, "run.log");
                var runner = new TutorialRunner(Path.Combine(directory, "state"), directory,
                    message => File.AppendAllText(logPath, $"{DateTimeOffset.Now:O} {message}\n")) { RestoreRequested = args.Contains("--restore") };
                runner.Run(CancellationToken.None, themeTestOnly: args[0] == "--theme-test", fixtureOnly: true);
                File.WriteAllText(Path.Combine(directory, "result.json"), System.Text.Json.JsonSerializer.Serialize(new { runner.TutorialCompleted, runner.ThemeRestored }));
                return 0;
            }
            foreach (var theme in new[] { "SkinDark.xaml", "Theme.xaml" })
                app.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri($"pack://application:,,,/HandyControl;component/Themes/{theme}")
                });
            app.Run(new MainWindow(preview: args.Contains("--preview-ui")));
            return 0;
        }
        catch (Exception ex)
        {
            if (args.Length >= 2 && args[0].StartsWith("--") && args[0] is "--probe" or "--theme-test" or "--run-fixture") File.WriteAllText(args[1] + ".error.txt", ex.ToString());
            else
            {
                var path = Path.Combine(AppContext.BaseDirectory, "data");
                Directory.CreateDirectory(path);
                File.WriteAllText(Path.Combine(path, "startup-error.txt"), ex.ToString());
                MessageBox.Show(ex.Message, "MAA 教程助手启动失败");
            }
            return 1;
        }
    }
}
