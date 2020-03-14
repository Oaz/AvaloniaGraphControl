using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
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
      var edges = graph.Edges.Select(edge => CreateControlForEdge(edge)).ToList();
      graph.LayoutAlgorithmSettings = CurrentLayoutSettings;
      graph.CreateGeometryGraph();
      customNodes = new Dictionary<IControl, Microsoft.Msagl.Drawing.Node>();
      var nodes = graph.Nodes.Select(dNode => CreateControlForNode(dNode)).ToList();
      Source.GeometryGraph.RootCluster.RectangularBoundary = new Microsoft.Msagl.Core.Geometry.RectangularClusterBoundary();
      var subgraphs = RecurseInto(Source.RootSubgraph, sg => sg.Subgraphs).Select(sg => new NodeView(sg, Source)).ToList();
      Children.AddRange(subgraphs);
    }

    public IControl CreateControlForEdge(Microsoft.Msagl.Drawing.Edge dEdge)
    {
      var edge = new EdgeView(dEdge);
      Children.Add(edge);
      ((Avalonia.VisualTree.IVisual)edge).ZIndex = 1;
      return edge;
    }

    public IControl CreateControlForNode(Microsoft.Msagl.Drawing.Node dNode)
    {
      var tpl = (dNode.UserData != null) ? this.FindDataTemplate(dNode.UserData) : null;
      if (tpl == null)
      {
        var n = new NodeView(dNode, Source);
        Children.Add(n);
        ((Avalonia.VisualTree.IVisual)n).ZIndex = 2;
        return n;
      }
      var ctrl = tpl.Build(dNode.UserData);
      ctrl.DataContext = dNode.UserData;
      ((Avalonia.Layout.Layoutable)ctrl).HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
      ((Avalonia.Layout.Layoutable)ctrl).VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
      Children.Add(ctrl);
      ((Avalonia.VisualTree.IVisual)ctrl).ZIndex = 2;
      customNodes[ctrl] = dNode;
      return ctrl;
    }

    Dictionary<IControl, Microsoft.Msagl.Drawing.Node> customNodes;

    protected override Size MeasureOverride(Size constraint)
    {
      if (Source == null)
        return constraint;
      foreach (var child in Children)
      {
        child.Measure(constraint);
        if (customNodes.TryGetValue(child, out Microsoft.Msagl.Drawing.Node dNode))
        {
          dNode.GeometryNode.BoundaryCurve = Microsoft.Msagl.Drawing.NodeBoundaryCurves.GetNodeBoundaryCurve(dNode, child.DesiredSize.Width, child.DesiredSize.Height);
        }
      }
      var transformer = new AglToAvalonia(Source.BoundingBox.LeftTop);
      return transformer.Convert(Source.BoundingBox.Size);
    }


    protected override Size ArrangeOverride(Size finalSize)
    {
      if (Source == null)
        return finalSize;
      Microsoft.Msagl.Miscellaneous.LayoutHelpers.CalculateLayout(Source.GeometryGraph, Source.LayoutAlgorithmSettings, null);
      var a2a = new AglToAvalonia(Source.BoundingBox.LeftTop);
      foreach (var child in Children)
      {
        var bbox = GetBoundingBox(child);
        if(!bbox.HasValue)
          continue;
        child.Arrange(a2a.Convert(bbox.Value));
      }
      return finalSize;
    }

    private Microsoft.Msagl.Core.Geometry.Rectangle? GetBoundingBox(IControl ctrl)
    {
      if (ctrl is NodeView nv && nv.DrawingNode.GeometryNode.BoundaryCurve != null)
        return nv.DrawingNode.BoundingBox;
      if (customNodes.TryGetValue(ctrl, out Microsoft.Msagl.Drawing.Node dNode))
        return dNode.BoundingBox;
      if (ctrl is EdgeView ev)
        return ev.DrawingEdge.BoundingBox;
      return null;
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
