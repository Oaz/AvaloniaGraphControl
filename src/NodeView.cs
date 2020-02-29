
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Microsoft.Msagl.Drawing;

namespace AvaloniaGraphControl
{
  public class NodeView : Control
  {
    public static NodeView Create(Node dNode, Graph graph) => new NodeView(dNode, graph, true);
    public static NodeView CreateSubgraph(Node dNode, Graph graph) => new NodeView(dNode, graph, false);
    private Action<DrawingContext> render;

    private NodeView(Node dNode, Graph graph, bool setBoundary)
    {
      render = GetRenderer(dNode,graph,setBoundary);
    }

    public override void Render(DrawingContext context)
    {
      render(context);
    }

    private static Action<DrawingContext> GetRenderer(Node drawingNode, Graph graph, bool setBoundary)
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

      void drawLabel(DrawingContext context, AglToAvalonia a2a, Rect bounds)
      {
        if (text == null || !drawingNode.Label.IsVisible)
          return;
        var position = (drawingNode is Subgraph)
          ? bounds.TopLeft + margin
          : a2a.Convert(drawingNode.Label.Center) - text.Bounds.Center;
        context.DrawText(frontBrush, position, text);
      }

      void drawNode(DrawingContext context)
      {
        if (!drawingNode.IsVisible)
          return;
        if (drawingNode.GeometryNode.BoundaryCurve == null)
          return;
        var a2a = new AglToAvalonia(graph.BoundingBox.LeftTop);
        var bounds = a2a.Convert(drawingNode.BoundingBox);
        drawShape(context, bounds);
        drawLabel(context, a2a, bounds);
      }

      if(setBoundary)
        SetBoundary(graph, drawingNode, text);
      return drawNode;
    }

    private static void SetBoundary(Graph graph, Node drawingNode, FormattedText text)
    {
      var width = Math.Max(text.Bounds.Width + 2 * drawingNode.Attr.LabelMargin, graph.Attr.MinNodeWidth);
      var height = Math.Max(text.Bounds.Height + 2 * drawingNode.Attr.LabelMargin, graph.Attr.MinNodeHeight);
      drawingNode.GeometryNode.BoundaryCurve = NodeBoundaryCurves.GetNodeBoundaryCurve(drawingNode, width, height);
    }
  }
}