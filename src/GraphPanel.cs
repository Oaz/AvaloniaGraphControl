using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace AvaloniaGraphControl
{
  public class GraphPanel : Panel
  {
    public static readonly StyledProperty<IEnumerable<Edge>> EdgesProperty = AvaloniaProperty.Register<GraphPanel, IEnumerable<Edge>>(nameof(Edges));

    public IEnumerable<Edge> Edges
    {
      get { return GetValue(EdgesProperty); }
      set { SetValue(EdgesProperty, value); }
    }
    public static readonly StyledProperty<Func<object, object>> HierarchyProperty = AvaloniaProperty.Register<GraphPanel, Func<object, object>>(nameof(Hierarchy));

    public Func<object, object> Hierarchy
    {
      get { return GetValue(HierarchyProperty); }
      set { SetValue(HierarchyProperty, value); }
    }

    public static readonly StyledProperty<double> ZoomProperty = AvaloniaProperty.Register<GraphPanel, double>(nameof(Zoom), 1.0);

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

    public static readonly StyledProperty<LayoutMethods> LayoutMethodProperty = AvaloniaProperty.Register<GraphPanel, LayoutMethods>(nameof(LayoutMethod), LayoutMethods.SugiyamaScheme);

    public LayoutMethods LayoutMethod
    {
      get { return GetValue(LayoutMethodProperty); }
      set { SetValue(LayoutMethodProperty, value); }
    }

    static GraphPanel()
    {
      EdgesProperty.Changed.AddClassHandler<GraphPanel>((gp, _) => gp.ComputeGraphDrawing());
      ZoomProperty.Changed.AddClassHandler<GraphPanel>((gp, _) => gp.RenderTransform = new ScaleTransform(gp.Zoom, gp.Zoom));
      LayoutMethodProperty.Changed.AddClassHandler<GraphPanel>((gp, _) => gp.ComputeGraphDrawing());
      AffectsMeasure<GraphPanel>(EdgesProperty, HierarchyProperty, LayoutMethodProperty);
      AffectsRender<GraphPanel>(ZoomProperty);
    }

    public GraphPanel()
    {
      RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Absolute);
      Hierarchy = x => null;
    }

    private void ComputeGraphDrawing()
    {
      Children.Clear();
      idGenerator = new ObjectIDGenerator();
      var edgeVMs = Edges.ToArray();
      var nodeVMs = edgeVMs.Select(e => e.Head).Concat(edgeVMs.Select(e => e.Tail)).Distinct().Select(nvm => new NodeWrapper(nvm, idGenerator)).ToDictionary(nw => nw.Node, nw => nw);
      var parentVMs = nodeVMs.Select(kv => Hierarchy(kv.Key)).Where(pvm => pvm != null).Distinct().Select(pvm => nodeVMs[pvm]).ToArray();
      var leafVMs = nodeVMs.Values.Except(parentVMs).ToArray();
      graph = new Microsoft.Msagl.Drawing.Graph();
      graph.LayoutAlgorithmSettings = CurrentLayoutSettings;
      graph.RootSubgraph.IsVisible = false;
      foreach (var evm in edgeVMs)
      {
        var dEdge = graph.AddEdge(nodeVMs[evm.Tail].ID, nodeVMs[evm.Head].ID);
        dEdge.Attr.ArrowheadAtSource = Edge.GetArrowStyle(evm.TailSymbol);
        dEdge.Attr.ArrowheadAtTarget = Edge.GetArrowStyle(evm.HeadSymbol);
        evm.DEdge = dEdge;
      }
      graph.CreateGeometryGraph();
      graph.GeometryGraph.RootCluster.RectangularBoundary = new Microsoft.Msagl.Core.Geometry.RectangularClusterBoundary();
      foreach (var evm in edgeVMs)
      {
        var ctrl = CreateControlForEdge(evm);
        ctrl.DataContext = evm;
        Children.Add(ctrl);
        ctrl.ZIndex = 1;
      }
      nodeOfCtrl = new Dictionary<IControl, NodeWrapper>();
      foreach (var nvm in nodeVMs.Values)
      {
        var dNode = graph.FindNode(nvm.ID);
        nvm.DNode = dNode;
        var ctrl = CreateControlForNode(nvm);
        ctrl.DataContext = nvm.Node;
        Children.Add(ctrl);
        ctrl.ZIndex = 2;
        nodeOfCtrl[ctrl] = nvm;
      }

      //var subgraphs = RecurseInto(Source.RootSubgraph, sg => sg.Subgraphs).Select(sg => new NodeView(sg, Source)).ToList();
      //Children.AddRange(subgraphs);
    }

    Microsoft.Msagl.Drawing.Graph graph;
    ObjectIDGenerator idGenerator;
    Dictionary<IControl, NodeWrapper> nodeOfCtrl;

    class NodeWrapper
    {
      public NodeWrapper(object node, ObjectIDGenerator idGen)
      {
        Node = node;
        ID = idGen.GetId(node, out bool _).ToString();
      }
      public readonly object Node;
      public readonly string ID;
      public Microsoft.Msagl.Drawing.Node DNode { get; set; }

      internal void UpdateBoundaryCurve(IControl ctrl)
      {
        var (shape, borderRadius) = ctrl is TextSticker ts ? (ts.Shape, ts.BorderRadius) : (TextSticker.Shapes.Rectangle, 0);
        DNode.GeometryNode.BoundaryCurve = AglCurveFactory.Create(shape, ctrl.DesiredSize, borderRadius);
      }
    }

    private IControl CreateControlForNode(NodeWrapper nvm)
    {
      var tpl = this.FindDataTemplate(nvm.Node);
      return tpl == null ? new TextSticker { Text = nvm.Node.ToString() } : tpl.Build(nvm.Node);
    }
    private Connection CreateControlForEdge(Edge evm)
    {
      var tpl = this.FindDataTemplate(evm);
      return tpl == null ? new Connection() : (Connection)tpl.Build(evm);
    }
    protected override Size MeasureOverride(Size constraint)
    {
      if (Edges == null)
        return constraint;
      foreach (var child in Children)
      {
        child.Measure(constraint);
        if (nodeOfCtrl.TryGetValue(child, out NodeWrapper nvm))
          nvm.UpdateBoundaryCurve(child);
      }
      var transformer = new AglToAvalonia(graph.BoundingBox.LeftTop);
      return transformer.Convert(graph.BoundingBox.Size);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
      if (Edges == null)
        return finalSize;
      Microsoft.Msagl.Miscellaneous.LayoutHelpers.CalculateLayout(graph.GeometryGraph, graph.LayoutAlgorithmSettings, null);
      var a2a = new AglToAvalonia(graph.BoundingBox.LeftTop);
      foreach (var child in Children)
      {
        var bbox = GetBoundingBox(child);
        if (!bbox.HasValue)
          continue;
        child.Arrange(a2a.Convert(bbox.Value));
      }
      return finalSize;
    }

    private Microsoft.Msagl.Core.Geometry.Rectangle? GetBoundingBox(IControl ctrl)
    {
      if (nodeOfCtrl.TryGetValue(ctrl, out NodeWrapper nw))
        return nw.DNode.BoundingBox;
      if (ctrl is Connection c)
        return ((Edge)c.DataContext).DEdge.BoundingBox;
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
