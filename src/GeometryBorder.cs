
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaGraphControl
{
  public class GeometryBorder : Decorator
  {
    public static readonly StyledProperty<IBrush> BackgroundProperty = AvaloniaProperty.Register<Border, IBrush>(nameof(Background));
    public static readonly StyledProperty<IBrush> BorderBrushProperty = AvaloniaProperty.Register<Border, IBrush>(nameof(BorderBrush));
    public static readonly StyledProperty<Thickness> BorderThicknessProperty = AvaloniaProperty.Register<Border, Thickness>(nameof(BorderThickness));
    public static readonly StyledProperty<Geometry> GeometryProperty = AvaloniaProperty.Register<Border, Geometry>(nameof(Geometry));

    static GeometryBorder()
    {
      AffectsMeasure<GeometryBorder>(GeometryProperty);
      AffectsRender<GeometryBorder>(BackgroundProperty, BorderBrushProperty, BorderThicknessProperty);
      BackgroundProperty.Changed.AddClassHandler<GeometryBorder>((gb, ea) => gb.Drawing.Brush = (IBrush)ea.NewValue);
      BorderBrushProperty.Changed.AddClassHandler<GeometryBorder>((gb, ea) => gb.Drawing.Pen = new Pen((IBrush)ea.NewValue));
      GeometryProperty.Changed.AddClassHandler<GeometryBorder>((gb, ea) => gb.Drawing.Geometry = (Geometry)ea.NewValue);
    }

    public IBrush Background
    {
      get => GetValue(BackgroundProperty);
      set => SetValue(BackgroundProperty, value);
    }

    public IBrush BorderBrush
    {
      get => GetValue(BorderBrushProperty);
      set => SetValue(BorderBrushProperty, value);
    }

    public Thickness BorderThickness
    {
      get => GetValue(BorderThicknessProperty);
      set => SetValue(BorderThicknessProperty, value);
    }
    public Geometry Geometry
    {
      get => GetValue(GeometryProperty);
      set => SetValue(GeometryProperty, value);
    }

    public GeometryBorder()
    {
      Drawing = new GeometryDrawing();
    }

    public override void Render(DrawingContext context)
    {
      Drawing.Draw(context);
    }

    public readonly GeometryDrawing Drawing;

    protected override Size MeasureOverride(Size availableSize)
    {
      return LayoutHelper.MeasureChild(Child, availableSize, Padding, BorderThickness);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
      return LayoutHelper.ArrangeChild(Child, finalSize, Padding, BorderThickness);
    }

  }
}