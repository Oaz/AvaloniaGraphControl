using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaGraphControlSample
{
  public class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      this.FindControl<ComboBox>("layoutMethods").Items = Enum.GetValues(typeof(AvaloniaGraphControl.GraphPanel.LayoutMethods));
#if DEBUG
      this.AttachDevTools();
#endif
    }


    private void InitializeComponent()
    {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
