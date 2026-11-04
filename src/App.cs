using System.IO;
using System.Windows;

namespace MaaTutorialAssistant;

public sealed class App : Application
{
    [STAThread]
    public static int Main(string[] args)
    {
        var app = new App();
        try
        {
            if (args.Length == 2 && args[0] == "--probe")
            {
                ControlSession.Probe(args[1]);
                return 0;
            }
            foreach (var theme in new[] { "SkinDark.xaml", "Theme.xaml" })
                app.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri($"pack://application:,,,/HandyControl;component/Themes/{theme}")
                });
            app.Run(new MainWindow());
            return 0;
        }
        catch (Exception ex)
        {
            if (args.Length == 2 && args[0] == "--probe") File.WriteAllText(args[1] + ".error.txt", ex.ToString());
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
