using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaGraphControl
{
  public class GraphView : Panel
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

    static GraphView()
    {
      SourceProperty.Changed.AddClassHandler<GraphView>((gv, _) => gv.ComputeGraphDrawing());
      ZoomProperty.Changed.AddClassHandler<GraphView>((gv, _) => gv.RenderTransform = new ScaleTransform(gv.Zoom, gv.Zoom));
      LayoutMethodProperty.Changed.AddClassHandler<GraphView>((gv, _) => gv.ComputeGraphDrawing());
      AffectsMeasure<GraphView>(SourceProperty, LayoutMethodProperty);
      AffectsRender<GraphView>(SourceProperty, ZoomProperty, LayoutMethodProperty);
    }

    public GraphView()
    {
      RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Absolute);
    }

    private void ComputeGraphDrawing()
    {
      Children.Clear();
      var graph = Source;
      var edges = graph.Edges.Select(edge => EdgeView.Create(edge,graph)).ToList();
      graph.LayoutAlgorithmSettings = CurrentLayoutSettings;
      graph.CreateGeometryGraph();
      var nodes = graph.Nodes.Select(dNode => NodeView.Create(dNode, graph)).ToList();
      Microsoft.Msagl.Miscellaneous.LayoutHelpers.CalculateLayout(graph.GeometryGraph, graph.LayoutAlgorithmSettings, null);
      var subgraphs = RecurseInto(Source.RootSubgraph, sg => sg.Subgraphs).Select(sg => NodeView.CreateSubgraph(sg,graph));
      Children.AddRange(subgraphs);
      Children.AddRange(nodes);
      Children.AddRange(edges);
    }

    protected override Size MeasureOverride(Size constraint)
    {
      if(Source==null)
        return constraint;
      var transformer = new AglToAvalonia(Source.BoundingBox.LeftTop);
      return transformer.Convert(Source.BoundingBox.Size);
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
