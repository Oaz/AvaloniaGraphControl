
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Microsoft.Msagl.Drawing;

namespace AvaloniaGraphControl
{
  public class NodeView : Decorator
  {
    public readonly Node DrawingNode;

    public NodeView(Node drawingNode, Graph graph)
    {
      DrawingNode = drawingNode;
      IsVisible = DrawingNode.IsVisible;

      var fillBrush = new SolidColorBrush(Factory.CreateColor(DrawingNode.Attr.FillColor));
      var frontBrush = new SolidColorBrush(Factory.CreateColor(DrawingNode.Attr.Color));
      var (fontStyle, fontWeight) = Factory.GetFontProps(DrawingNode.Label.FontStyle);
      var isSubGraph = DrawingNode is Subgraph;

      Child = new GeometryBorder
      {
        BorderBrush = frontBrush,
        Background = fillBrush,
        BorderThickness = new Thickness(1),
        MinHeight = graph.Attr.MinNodeHeight,
        MinWidth = graph.Attr.MinNodeWidth,
        Child = new TextBlock
        {
          Margin = new Thickness(DrawingNode.Attr.LabelMargin),
          Text = DrawingNode.LabelText,
          FontFamily = Factory.CreateFontFamily(DrawingNode.Label),
          FontSize = DrawingNode.Label.FontSize,
          FontWeight = fontWeight,
          FontStyle = fontStyle,
          Foreground = frontBrush,
          HorizontalAlignment = isSubGraph ? Avalonia.Layout.HorizontalAlignment.Left : Avalonia.Layout.HorizontalAlignment.Center,
          VerticalAlignment = isSubGraph ? Avalonia.Layout.VerticalAlignment.Top : Avalonia.Layout.VerticalAlignment.Center
        }
      };
    }

    protected override Size MeasureOverride(Size availableSize)
    {
      Child.Measure(availableSize);
      var bounds = new Rect(Child.DesiredSize);
      DrawingNode.GeometryNode.BoundaryCurve = Microsoft.Msagl.Drawing.NodeBoundaryCurves.GetNodeBoundaryCurve(DrawingNode, bounds.Size.Width, bounds.Size.Height);
      return bounds.Size;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
      ((GeometryBorder)Child).Geometry = ComputeGeometry(new Rect(finalSize));
      return base.ArrangeOverride(finalSize);
    }

    private Geometry ComputeGeometry(Rect bounds)
    {
      return DrawingNode.Attr.Shape switch
      {
        Shape.Circle => new EllipseGeometry(bounds),
        Shape.Ellipse => new EllipseGeometry(bounds),
        Shape.Diamond => new DiamondGeometry(bounds),
        _ => new RoundedRectangleGeometry(bounds, (DrawingNode.Attr.XRadius + DrawingNode.Attr.YRadius) / 2)
      };
    }

  }
}