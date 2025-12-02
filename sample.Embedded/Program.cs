using Avalonia;
using Avalonia.Media;

namespace AvaloniaGraphControlSample.Embedded;

class Program
{
  [STAThread]
  public static void Main(string[] args)
  {
    BuildAvaloniaApp().StartEmbedded(args);
  }
  
  public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
      .WithInterFont()
      .With(new FontManagerOptions
      {
        DefaultFamilyName = "avares://Avalonia.Fonts.Inter/Assets#Inter"
      })
      .LogToTrace()
      .SetUrlOpener(OpenUrl);
  
  private static void OpenUrl(string url)
  {
    Console.WriteLine($"Open {url}");
  }
}

public static class AppBuilderExtensions
{
  public static void StartEmbedded(this AppBuilder builder, string[] args)
  {
    var drmCard = Environment.GetEnvironmentVariable("AVALONIA_DRM_CARD");
    if (drmCard != null)
    {
      Console.WriteLine($"Using DRM card {drmCard}");
      builder.StartLinuxDrm(args, card: drmCard, scaling: 1.0);
    }
    else
    {
      Console.WriteLine("No DRM card configured, falling back to fbdev");
      builder.StartLinuxFbDev(args);
    }
  }
}
