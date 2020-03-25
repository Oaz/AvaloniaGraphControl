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
      var nodeVMs = edgeVMs.Select(e => e.Head).Concat(edgeVMs.Select(e => e.Tail)).Distinct().Select(nvm => new NodeWrapper(nvm, idGenerator)).ToDictionary(nw => nw.VM, nw => nw);
      var parentVMs = nodeVMs.Select(kv => Hierarchy(kv.Key)).Where(pvm => pvm != null).Distinct().Select(pvm => nodeVMs[pvm]).ToArray();
      var leafVMs = nodeVMs.Values.Except(parentVMs).ToArray();
      graph = new Microsoft.Msagl.Drawing.Graph();
      graph.LayoutAlgorithmSettings = CurrentLayoutSettings;
      graph.RootSubgraph.IsVisible = false;
      vmOfCtrl = new Dictionary<IControl, Wrapper>();
      foreach (var evm in edgeVMs)
      {
        var dEdge = graph.AddEdge(nodeVMs[evm.Tail].ID, nodeVMs[evm.Head].ID);
        dEdge.Attr.ArrowheadAtSource = Edge.GetArrowStyle(evm.TailSymbol);
        dEdge.Attr.ArrowheadAtTarget = Edge.GetArrowStyle(evm.HeadSymbol);
        dEdge.LabelText = "x";
        evm.DEdge = dEdge;
        CreateControl(evm, _ => new Connection(), 1);
      }
      graph.CreateGeometryGraph();
      graph.GeometryGraph.RootCluster.RectangularBoundary = new Microsoft.Msagl.Core.Geometry.RectangularClusterBoundary();

      foreach (var evm in edgeVMs)
      {
        if (!evm.Label.Equals(string.Empty))
        {
          var ctrl = CreateControl(evm.Label, l => new TextBlock { Text = l.ToString(), FontSize = 6 }, 2);
          vmOfCtrl[ctrl] = new LabelWrapper(evm.Label, idGenerator, evm.DEdge.Label);
        }
      }
      foreach (var nvm in nodeVMs.Values)
      {
        var dNode = graph.FindNode(nvm.ID);
        nvm.DNode = dNode;
        var ctrl = CreateControl(nvm.VM, n => new TextSticker { Text = n.ToString() }, 3);
        vmOfCtrl[ctrl] = nvm;
      }

      //var subgraphs = RecurseInto(Source.RootSubgraph, sg => sg.Subgraphs).Select(sg => new NodeView(sg, Source)).ToList();
      //Children.AddRange(subgraphs);
    }

    private IControl CreateControl(object vm, Func<object, IControl> getDefault, int zIndex)
    {
      var tpl = this.FindDataTemplate(vm);
      var ctrl = tpl == null ? getDefault(vm) : tpl.Build(vm);
      ctrl.DataContext = vm;
      Children.Add(ctrl);
      ctrl.ZIndex = zIndex;
      return ctrl;
    }

    Microsoft.Msagl.Drawing.Graph graph;
    ObjectIDGenerator idGenerator;
    Dictionary<IControl, Wrapper> vmOfCtrl;

    abstract class Wrapper
    {
      public Wrapper(object vm, ObjectIDGenerator idGen)
      {
        VM = vm;
        ID = idGen.GetId(vm, out bool _).ToString();
      }
      public readonly object VM;
      public readonly string ID;

      internal abstract Microsoft.Msagl.Core.Geometry.Rectangle GetBoundingBox();
      internal abstract void UpdateBounds(IControl ctrl);
    }

    class LabelWrapper : Wrapper
    {
      public LabelWrapper(object label, ObjectIDGenerator idGen, Microsoft.Msagl.Drawing.Label dLabel) : base(label, idGen)
      {
        DLabel = dLabel;
      }

      public readonly Microsoft.Msagl.Drawing.Label DLabel;
      internal override Microsoft.Msagl.Core.Geometry.Rectangle GetBoundingBox() => DLabel.BoundingBox;
      internal override void UpdateBounds(IControl ctrl)
      {
        DLabel.Width = ctrl.DesiredSize.Width;
        DLabel.Height = ctrl.DesiredSize.Height;
      }
    }

    class NodeWrapper : Wrapper
    {
      public NodeWrapper(object node, ObjectIDGenerator idGen) : base(node, idGen) { }

      public Microsoft.Msagl.Drawing.Node DNode { get; set; }

      internal override Microsoft.Msagl.Core.Geometry.Rectangle GetBoundingBox() => DNode.BoundingBox;
      internal override void UpdateBounds(IControl ctrl)
      {
        var (shape, borderRadius) = ctrl is TextSticker ts ? (ts.Shape, ts.BorderRadius) : (TextSticker.Shapes.Rectangle, 0);
        DNode.GeometryNode.BoundaryCurve = AglCurveFactory.Create(shape, ctrl.DesiredSize, borderRadius);
      }
    }

    protected override Size MeasureOverride(Size constraint)
    {
      if (Edges == null)
        return constraint;
      foreach (var child in Children)
      {
        child.Measure(constraint);
        if (vmOfCtrl.TryGetValue(child, out Wrapper w))
          w.UpdateBounds(child);
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
      if (vmOfCtrl.TryGetValue(ctrl, out Wrapper w))
        return w.GetBoundingBox();
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
