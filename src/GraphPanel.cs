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
      EdgesProperty.Changed.AddClassHandler<GraphPanel>((gp, _) => gp.CreateMSAGLGraphAndPopulatePanelWithAssociatedControls());
      HierarchyProperty.Changed.AddClassHandler<GraphPanel>((gp, _) => gp.CreateMSAGLGraphAndPopulatePanelWithAssociatedControls());
      ZoomProperty.Changed.AddClassHandler<GraphPanel>((gp, _) => gp.RenderTransform = new ScaleTransform(gp.Zoom, gp.Zoom));
      LayoutMethodProperty.Changed.AddClassHandler<GraphPanel>((gp, _) => gp.CreateMSAGLGraphAndPopulatePanelWithAssociatedControls());
      AffectsMeasure<GraphPanel>(EdgesProperty, HierarchyProperty, LayoutMethodProperty);
      AffectsRender<GraphPanel>(ZoomProperty);
    }

    public GraphPanel()
    {
      RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Absolute);
      Hierarchy = x => null;
    }

    private void CreateMSAGLGraphAndPopulatePanelWithAssociatedControls()
    {
      if (Edges == null)
        return;
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
      foreach (var sgvm in parentVMs)
      {
        var sg = new Microsoft.Msagl.Drawing.Subgraph(sgvm.ID);
        sgvm.DNode = sg;
        var ctrl = CreateControl(sgvm.VM, n => new TextSticker
        {
          Text = n.ToString(),
          HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
          VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        }, 1);
        vmOfCtrl[ctrl] = sgvm;
      }
      foreach (var sgvm in parentVMs)
      {
        var parent = Hierarchy(sgvm.VM);
        var pGraph = (parent == null) ? graph.RootSubgraph : (Microsoft.Msagl.Drawing.Subgraph)nodeVMs[parent].DNode;
        pGraph.AddSubgraph((Microsoft.Msagl.Drawing.Subgraph)sgvm.DNode);
      }
      foreach (var evm in edgeVMs)
      {
        var dEdge = graph.AddEdge(nodeVMs[evm.Tail].ID, nodeVMs[evm.Head].ID);
        dEdge.Attr.ArrowheadAtSource = Edge.GetArrowStyle(evm.TailSymbol);
        dEdge.Attr.ArrowheadAtTarget = Edge.GetArrowStyle(evm.HeadSymbol);
        dEdge.LabelText = "x";
        evm.DEdge = dEdge;
        CreateControl(evm, _ => new Connection(), 2);
      }
      foreach (var nvm in leafVMs)
      {
        var dNode = graph.FindNode(nvm.ID);
        nvm.DNode = dNode;
        var ctrl = CreateControl(nvm.VM, n => new TextSticker { Text = n.ToString() }, 4);
        vmOfCtrl[ctrl] = nvm;
        var parent = Hierarchy(nvm.VM);
        if (parent != null)
        {
          var pw = nodeVMs[parent];
          ((Microsoft.Msagl.Drawing.Subgraph)pw.DNode).AddNode(dNode);
        }
      }
      graph.CreateGeometryGraph();
      graph.GeometryGraph.RootCluster.RectangularBoundary = new Microsoft.Msagl.Core.Geometry.RectangularClusterBoundary();

      foreach (var evm in edgeVMs)
      {
        if (!evm.Label.Equals(string.Empty))
        {
          var ctrl = CreateControl(evm.Label, l => new TextBlock { Text = l.ToString(), FontSize = 6 }, 3);
          vmOfCtrl[ctrl] = new LabelWrapper(evm.Label, idGenerator, evm.DEdge.Label);
        }
      }

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
        if (DNode.GeometryNode == null)
          return;
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
        var childFinalSize = a2a.Convert(bbox.Value);
        child.Arrange(childFinalSize);
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
