using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AvaloniaGraphControlSample;

public partial class MainView : UserControl
{
  public MainView()
  {
    InitializeComponent();
    zoomBorderHelper = new ZoomBorderHelper(this.Find<ZoomBorder>("ZoomBorder")!);
  }

  private readonly ZoomBorderHelper zoomBorderHelper;
  
  private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

  private void ResetZoom(object? sender, RoutedEventArgs e) => zoomBorderHelper.Reset();
}

