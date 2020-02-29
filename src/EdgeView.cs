
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Microsoft.Msagl.Drawing;

namespace AvaloniaGraphControl
{
  public class EdgeView : Control
  {
    public static EdgeView Create(Edge edge, Graph graph) => new EdgeView(edge,graph);
    private Action<DrawingContext> render;

    private EdgeView(Edge edge, Graph graph)
    {
      render = GetRenderer(edge,graph);
    }

    public override void Render(DrawingContext context)
    {
      render(context);
    }

    private static Action<DrawingContext> GetRenderer(Edge edge, Graph graph)
    {
      var brush = new SolidColorBrush(Factory.CreateColor(edge.Attr.Color));
      var text = Factory.CreateText(edge.Label);
      if (edge.Label != null)
      {
        edge.Label.Width = text.Bounds.Width;
        edge.Label.Height = text.Bounds.Height;
      }

      Action<DrawingContext, AglToAvalonia> drawArrowAtTarget = edge.Attr.ArrowheadAtTarget switch
      {
        ArrowStyle.None => (context, a2a) => { }
        ,
        _ => (context, a2a) =>
        {
          var arrowHead = ComputeArrowHead(edge.EdgeCurve.End, edge.ArrowAtTargetPosition, 3.0);
          var poly = new PolylineGeometry(arrowHead.Select(p => a2a.Convert(p)).ToArray(), false);
          context.DrawGeometry(brush, new Pen(brush), poly);
        }
      };

      var nofill = new SolidColorBrush();
      void drawEdge(DrawingContext context)
      {
        var a2a = new AglToAvalonia(graph.BoundingBox.LeftTop);
        var bounds = a2a.Convert(edge.BoundingBox);
        var geom = new PathGeometry { Figures = new PathFigures { a2a.Convert(edge.EdgeCurve) } };
        context.DrawGeometry(nofill, new Pen(brush), geom);
        drawArrowAtTarget(context, a2a);
        if (text != null && edge.Label.IsVisible)
          context.DrawText(brush, a2a.Convert(edge.Label.LeftTop), text);
      }
      return drawEdge;
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