
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Microsoft.Msagl.Drawing;

namespace AvaloniaGraphControl
{
  public class EdgeView : Decorator
  {
    public readonly Edge DrawingEdge;
    private readonly IBrush brush;
    private readonly List<Drawing> Drawings;

    public EdgeView(Edge edge)
    {
      DrawingEdge = edge;
      this.Drawings = new List<Drawing>();
      this.brush = new SolidColorBrush(Factory.CreateColor(edge.Attr.Color));
      if (edge.Label != null && edge.Label.IsVisible)
      {
        var (fontStyle, fontWeight) = Factory.GetFontProps(edge.Label.FontStyle);
        Child = new TextBlock
        {
          Text = edge.LabelText,
          FontFamily = Factory.CreateFontFamily(edge.Label),
          FontSize = edge.Label.FontSize,
          FontWeight = fontWeight,
          FontStyle = fontStyle,
          Foreground = brush,
          HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
          VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
        };
      }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
      if (Child == null)
        return availableSize;
      Child.Measure(availableSize);
      var bounds = new Rect(Child.DesiredSize);
      DrawingEdge.Label.Width = bounds.Width;
      DrawingEdge.Label.Height = bounds.Height;
      return bounds.Size;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
      Drawings.Clear();
      var box = DrawingEdge.BoundingBox;
      var a2a = new AglToAvalonia(box.LeftTop);
      // curve
      var nofill = new SolidColorBrush();
      Drawings.Add(FigureToDrawing(CreateEdgePathFigure(DrawingEdge, a2a), brush, nofill));
      if (DrawingEdge.Attr.ArrowAtTarget)
        Drawings.Add(FigureToDrawing(CreateArrowHeadFigure(DrawingEdge.EdgeCurve.End, DrawingEdge.ArrowAtTargetPosition, a2a), brush, brush));
      // label
      if (Child != null)
        Child.Arrange(a2a.Convert(DrawingEdge.Label.BoundingBox));

      return a2a.Convert(box.Size);
    }
    public override void Render(DrawingContext context)
    {
      foreach (var drawing in Drawings)
      {
        drawing.Draw(context);
      }
    }
    private static Drawing FigureToDrawing(PathFigure figure, IBrush strokeBrush, IBrush fillBrush)
    {
      return new GeometryDrawing()
      {
        Pen = new Pen(strokeBrush),
        Brush = fillBrush,
        Geometry = new PathGeometry
        {
          Figures = new PathFigures
          {
            figure
          }
        }
      };
    }

    private static PathFigure CreateEdgePathFigure(Edge edge, AglToAvalonia a2a)
    {
      return a2a.Convert(edge.EdgeCurve);
    }

    private static PathFigure CreateArrowHeadFigure(Microsoft.Msagl.Core.Geometry.Point origin, Microsoft.Msagl.Core.Geometry.Point target, AglToAvalonia a2a)
    {
      var arrowHead = ComputeArrowHead(origin, target, 3.0).ToList();
      var segments = new PathSegments();
      segments.AddRange(arrowHead.Skip(1).Select(p => new LineSegment { Point = a2a.Convert(p) }));
      var figure = new PathFigure { IsFilled = true, IsClosed = true, StartPoint = a2a.Convert(arrowHead.First()), Segments = segments };
      return figure;
    }

    private static IEnumerable<Microsoft.Msagl.Core.Geometry.Point> ComputeArrowHead(
      Microsoft.Msagl.Core.Geometry.Point origin,
      Microsoft.Msagl.Core.Geometry.Point target,
      double width
    )
    {
      yield return target;
      var v = target - origin;
      if (v.Y == 0)
      {
        yield return origin + new Microsoft.Msagl.Core.Geometry.Point(0, width);
        yield return origin + new Microsoft.Msagl.Core.Geometry.Point(0, -width);
      }
      else
      {
        var a = -v.X / v.Y;
        var x = Math.Sqrt(width * width / (1 + a * a));
        yield return origin + new Microsoft.Msagl.Core.Geometry.Point(x, a * x);
        yield return origin + new Microsoft.Msagl.Core.Geometry.Point(-x, -a * x);
      }
    }
  }
}