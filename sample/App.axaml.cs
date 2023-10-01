using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace AvaloniaGraphControlSample
{
  public class App : Application
  {
    public override void Initialize()
    {
      AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
      Trace.Listeners.Add(new ConsoleTraceListener());
      if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
      {
        desktop.MainWindow = new MainWindow()
        {
          DataContext = new Model()
        };
      }
      else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
      {
        singleViewPlatform.MainView = new MainView
        {
          DataContext = new Model()
        };
      }
      
      base.OnFrameworkInitializationCompleted();
    }
  }
}
