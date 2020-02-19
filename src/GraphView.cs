using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;

namespace AvaloniaGraphControl
{
  public class GraphView : Control
  {
    public static readonly StyledProperty<Microsoft.Msagl.Drawing.Graph> SourceProperty = AvaloniaProperty.Register<GraphView, Microsoft.Msagl.Drawing.Graph>(nameof(Source));

    public Microsoft.Msagl.Drawing.Graph Source
    {
      get { return GetValue(SourceProperty); }
      set { SetValue(SourceProperty, value); }
    }

    public static readonly StyledProperty<double> ZoomProperty = AvaloniaProperty.Register<GraphView, double>(nameof(Zoom), 1.0);

    public double Zoom
    {
      get { return GetValue(ZoomProperty); }
      set { SetValue(ZoomProperty, value); }
    }

    public enum LayoutMethods
    {
      SugiyamaScheme,
      MDS,
      Ranking,
      IncrementalLayout
    }

    public static readonly StyledProperty<LayoutMethods> LayoutMethodProperty = AvaloniaProperty.Register<GraphView, LayoutMethods>(nameof(LayoutMethod), LayoutMethods.SugiyamaScheme);

    public LayoutMethods LayoutMethod
    {
      get { return GetValue(LayoutMethodProperty); }
      set { SetValue(LayoutMethodProperty, value); }
    }

    public GraphView()
    {
      RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Absolute);
      PropertyChanged += (s, args) => InvalidateVisual();
    }
    public override void Render(DrawingContext context)
    {
      if (Source == null)
        return;
      var viewPort = new Rect(Bounds.Size);
      RenderTransform = new ScaleTransform(Zoom, Zoom);
      var renderers = ComputeLayoutAndGetRenderers(Source);
      var transformer = new Transformer(Source.BoundingBox.LeftTop);
      foreach (var render in renderers)
        render(context, transformer);
    }

    private IEnumerable<Action<DrawingContext, Transformer>> ComputeLayoutAndGetRenderers(Graph graph)
    {
      var edges = Source.Edges.Select(edge => RenderingHelper.GetEdge(edge)).ToList();
      graph.LayoutAlgorithmSettings = CurrentLayoutSettings;
      graph.CreateGeometryGraph();
      var nodes = graph.Nodes.Select(dNode => RenderingHelper.GetNode(dNode, graph)).ToList();
      Microsoft.Msagl.Miscellaneous.LayoutHelpers.CalculateLayout(graph.GeometryGraph, graph.LayoutAlgorithmSettings, null);
      var subgraphs = RecurseInto(Source.RootSubgraph, sg => sg.Subgraphs).Select(sg => RenderingHelper.GetNode(sg));
      var allRenderers = subgraphs.Concat(nodes).Concat(edges).ToList();
      return allRenderers;
    }

    private static IEnumerable<T> RecurseInto<T>(T parent, Func<T, IEnumerable<T>> getChildren)
    {
      var children = getChildren(parent).SelectMany(c => RecurseInto(c, getChildren));
      return children.Prepend(parent);
    }

    private Microsoft.Msagl.Core.Layout.LayoutAlgorithmSettings CurrentLayoutSettings =>
       LayoutMethod switch
       {
         LayoutMethods.SugiyamaScheme => new Microsoft.Msagl.Layout.Layered.SugiyamaLayoutSettings(),
         LayoutMethods.MDS => new Microsoft.Msagl.Layout.MDS.MdsLayoutSettings(),
         LayoutMethods.Ranking => new Microsoft.Msagl.Prototype.Ranking.RankingLayoutSettings(),
         LayoutMethods.IncrementalLayout => new Microsoft.Msagl.Layout.Incremental.FastIncrementalLayoutSettings(),
         _ => new Microsoft.Msagl.Layout.Layered.SugiyamaLayoutSettings()
       };
  }
}
