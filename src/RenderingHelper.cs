using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Media;
using Microsoft.Msagl.Drawing;

namespace AvaloniaGraphControl
{
  static class RenderingHelper
  {
    public static Action<DrawingContext, Transformer> GetNode(Node drawingNode, Graph graph = null)
    {
      var fillBrush = new SolidColorBrush(Factory.CreateColor(drawingNode.Attr.FillColor));
      var frontBrush = new SolidColorBrush(Factory.CreateColor(drawingNode.Attr.Color));
      var frontPen = new Pen(frontBrush);
      var cornerRadius = (float)(drawingNode.Attr.XRadius + drawingNode.Attr.YRadius) / 2;
      var text = Factory.CreateText(drawingNode.Label);
      var margin = new Point(drawingNode.Attr.LabelMargin, drawingNode.Attr.LabelMargin);

      Action<DrawingContext, Rect> drawShape = drawingNode.Attr.Shape switch
      {
        Shape.Circle => (context, bounds) => context.DrawGeometry(fillBrush, frontPen, new EllipseGeometry(bounds)),
        Shape.Ellipse => (context, bounds) => context.DrawGeometry(fillBrush, frontPen, new EllipseGeometry(bounds)),
        Shape.Diamond => (context, bounds) => context.DrawGeometry(fillBrush, frontPen, new DiamondGeometry(bounds)),
        _ => (context, bounds) =>
        {
          context.FillRectangle(fillBrush, bounds, cornerRadius);
          context.DrawRectangle(frontPen, bounds, cornerRadius);
        }
      };

      void drawLabel(DrawingContext context, Transformer transformer, Rect bounds)
      {
        if (text == null || !drawingNode.Label.IsVisible)
          return;
        var position = (drawingNode is Subgraph)
          ? bounds.TopLeft + margin
          : transformer.Transform(drawingNode.Label.Center) - text.Bounds.Center;
        context.DrawText(frontBrush, position, text);
      }

      void drawNode(DrawingContext context, Transformer transformer)
      {
        if (!drawingNode.IsVisible)
          return;
        if (drawingNode.GeometryNode.BoundaryCurve == null)
          return;
        var bounds = transformer.Transform(drawingNode.BoundingBox);
        drawShape(context, bounds);
        drawLabel(context, transformer, bounds);
      }

      if (graph != null)
        SetBoundary(graph, drawingNode, text);
      return drawNode;
    }

    private static void SetBoundary(Graph graph, Node drawingNode, FormattedText text)
    {
      var width = Math.Max(text.Bounds.Width + 2 * drawingNode.Attr.LabelMargin, graph.Attr.MinNodeWidth);
      var height = Math.Max(text.Bounds.Height + 2 * drawingNode.Attr.LabelMargin, graph.Attr.MinNodeHeight);
      drawingNode.GeometryNode.BoundaryCurve = NodeBoundaryCurves.GetNodeBoundaryCurve(drawingNode, width, height);
    }

    public static Action<DrawingContext, Transformer> GetEdge(Edge edge)
    {
      var brush = new SolidColorBrush(Factory.CreateColor(edge.Attr.Color));
      var text = Factory.CreateText(edge.Label);
      if (edge.Label != null)
      {
        edge.Label.Width = text.Bounds.Width;
        edge.Label.Height = text.Bounds.Height;
      }

      Action<DrawingContext, Transformer> drawArrowAtTarget = edge.Attr.ArrowheadAtTarget switch
      {
        ArrowStyle.None => (context, transformer) => {},
        _ => (context, transformer) =>
        {
          var arrowHead = ComputeArrowHead(edge.EdgeCurve.End, edge.ArrowAtTargetPosition, 3.0);
          var poly = new PolylineGeometry(arrowHead.Select(p => transformer.Transform(p)).ToArray(), false);
          context.DrawGeometry(brush, new Pen(brush), poly);
        }
      };

      void drawEdge(DrawingContext context, Transformer transformer)
      {
        var bounds = transformer.Transform(edge.BoundingBox);
        context.DrawLine(new Pen(brush), transformer.Transform(edge.EdgeCurve.Start), transformer.Transform(edge.EdgeCurve.End));
        drawArrowAtTarget(context,transformer);
        if (text != null && edge.Label.IsVisible)
          context.DrawText(brush, transformer.Transform(edge.Label.LeftTop), text);
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
