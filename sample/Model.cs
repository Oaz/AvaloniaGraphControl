using System;
using System.Collections.Generic;
using Microsoft.Msagl.Drawing;

namespace AvaloniaGraphControlSample
{
  class NamedGraph
  {
    public string Name { get; set; }
    public Graph Graph { get; set; }
    public IEnumerable<AvaloniaGraphControl.Edge> Edges { get; set; }
    public override string ToString() => Name;
  }

  class StandardItem
  {
    public StandardItem(string name) { Name = name; }
    public string Name { get; private set; }
  }
  class InteractiveItem
  {
    public InteractiveItem(string name) { Name = name; }
    public string Name { get; private set; }
  }

  class FamilyMember
  {
    public FamilyMember(string name, Avalonia.Media.Color backgroungColor, string url)
    {
      Name = name;
      BackgroundColor = new Avalonia.Media.SolidColorBrush(backgroungColor);
      URL = url;
    }

    public void Navigate()
    {
      if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        System.Diagnostics.Process.Start("xdg-open", URL);
      else
        System.Diagnostics.Process.Start(URL);
    }
    public string Name { get; private set; }
    public Avalonia.Media.IBrush BackgroundColor { get; private set; }
    public string URL { get; private set; }
  }

  class Male : FamilyMember
  {
    public Male(string name, string url) : base(name, Avalonia.Media.Colors.LightSkyBlue, url) { }
  }
  class Female : FamilyMember
  {
    public Female(string name, string url) : base(name, Avalonia.Media.Colors.LightPink, url) { }
  }
  class Family
  {
  }

  class State
  {
    public State(string name) { Name = name; }
    public string Name { get; private set; }
  }
  class InitialState
  {
  }

  class Model
  {
    public IEnumerable<NamedGraph> SampleGraphs => new NamedGraph[] {
      new NamedGraph {Name="Simple Graph", Graph=SimpleGraph, Edges=SimpleGraphEdges},
      new NamedGraph {Name="Simple Interactive Graph", Graph=SimpleInteractiveGraph, Edges=SimpleInteractiveGraphEdges},
      new NamedGraph {Name="Family Tree", Graph=FamilyTree, Edges=FamilyTreeEdges},
      new NamedGraph {Name="State Machine", Graph=StateMachine, Edges=StateMachineEdges}
    };
    public static Graph SimpleGraph
    {
      get
      {
        var graph = new Microsoft.Msagl.Drawing.Graph("graph");
        graph.RootSubgraph.IsVisible = false;
        graph.AddEdge("A", "B");
        graph.AddEdge("A", "D");
        graph.AddEdge("A", "E");
        graph.AddEdge("B", "C");
        graph.AddEdge("B", "D");
        graph.AddEdge("D", "A");
        graph.AddEdge("D", "E");
        foreach (var node in graph.Nodes)
          node.UserData = new StandardItem(node.Id);
        return graph;
      }
    }

    public static IEnumerable<AvaloniaGraphControl.Edge> SimpleGraphEdges
    {
      get
      {
        var a = new StandardItem("A");
        var b = new StandardItem("B");
        var c = new StandardItem("C");
        var d = new StandardItem("D");
        var e = new StandardItem("E");
        yield return new AvaloniaGraphControl.Edge(a, b);
        yield return new AvaloniaGraphControl.Edge(a, d);
        yield return new AvaloniaGraphControl.Edge(a, e);
        yield return new AvaloniaGraphControl.Edge(b, c);
        yield return new AvaloniaGraphControl.Edge(b, d);
        yield return new AvaloniaGraphControl.Edge(d, a);
        yield return new AvaloniaGraphControl.Edge(d, e);
      }
    }

    public static Graph SimpleInteractiveGraph
    {
      get
      {
        var graph = SimpleGraph;
        foreach (var node in graph.Nodes)
          node.UserData = new InteractiveItem(node.Id);
        return graph;
      }
    }


    public static IEnumerable<AvaloniaGraphControl.Edge> SimpleInteractiveGraphEdges
    {
      get
      {
        var a = new InteractiveItem("A");
        var b = new InteractiveItem("B");
        var c = new InteractiveItem("C");
        var d = new InteractiveItem("D");
        var e = new InteractiveItem("E");
        yield return new AvaloniaGraphControl.Edge(a, b);
        yield return new AvaloniaGraphControl.Edge(a, d);
        yield return new AvaloniaGraphControl.Edge(a, e);
        yield return new AvaloniaGraphControl.Edge(b, c);
        yield return new AvaloniaGraphControl.Edge(b, d);
        yield return new AvaloniaGraphControl.Edge(d, a);
        yield return new AvaloniaGraphControl.Edge(d, e);
      }
    }

    public static Graph FamilyTree
    {
      get
      {
        var graph = new Microsoft.Msagl.Drawing.Graph("graph");
        graph.RootSubgraph.IsVisible = false;
        graph.Attr.LayerDirection = LayerDirection.BT;
        graph.AddEdge("f1", "Abraham");
        graph.AddEdge("f1", "Mona");
        graph.AddEdge("Homer", "f1");
        graph.AddEdge("f2", "Clancy");
        graph.AddEdge("f2", "Jackie");
        graph.AddEdge("Marge", "f2");
        graph.AddEdge("Patty", "f2");
        graph.AddEdge("Selma", "f2");
        graph.AddEdge("f3", "Homer");
        graph.AddEdge("f3", "Marge");
        graph.AddEdge("Bart", "f3");
        graph.AddEdge("Lisa", "f3");
        graph.AddEdge("Maggie", "f3");
        graph.AddEdge("Ling", "Selma");
        foreach (var node in graph.Nodes)
        {
          if (node.LabelText.StartsWith("f"))
          {
            node.LabelText = string.Empty;
            node.Attr.FillColor = Color.Gray;
          }
        }
        graph.FindNode("Abraham").UserData = new Male("Abraham", "https://en.wikipedia.org/wiki/Grampa_Simpson");
        graph.FindNode("Mona").UserData = new Female("Mona", "https://en.wikipedia.org/wiki/Mona_Simpson_(The_Simpsons)");
        graph.FindNode("Homer").UserData = new Male("Homer", "https://en.wikipedia.org/wiki/Homer_Simpson");
        graph.FindNode("Clancy").UserData = new Male("Clancy", "https://en.wikipedia.org/wiki/Simpson_family#Clancy_Bouvier");
        graph.FindNode("Jackie").UserData = new Female("Jackie", "https://en.wikipedia.org/wiki/Simpson_family#Jacqueline_Bouvier");
        graph.FindNode("Marge").UserData = new Female("Marge", "https://en.wikipedia.org/wiki/Marge_Simpson");
        graph.FindNode("Patty").UserData = new Female("Patty", "https://en.wikipedia.org/wiki/Patty_and_Selma");
        graph.FindNode("Selma").UserData = new Female("Selma", "https://en.wikipedia.org/wiki/Patty_and_Selma");
        graph.FindNode("Ling").UserData = new Female("Ling", "https://en.wikipedia.org/wiki/Simpson_family#Ling_Bouvier");
        graph.FindNode("Bart").UserData = new Male("Bart", "https://en.wikipedia.org/wiki/Bart_Simpson");
        graph.FindNode("Lisa").UserData = new Female("Lisa", "https://en.wikipedia.org/wiki/Lisa_Simpson");
        graph.FindNode("Maggie").UserData = new Female("Maggie", "https://en.wikipedia.org/wiki/Maggie_Simpson");
        foreach (var edge in graph.Edges)
        {
          edge.Attr.ArrowheadAtTarget = ArrowStyle.None;
        }
        return graph;
      }
    }


    public static IEnumerable<AvaloniaGraphControl.Edge> FamilyTreeEdges
    {
      get
      {
        var abraham = new Male("Abraham", "https://en.wikipedia.org/wiki/Grampa_Simpson");
        var mona = new Female("Mona", "https://en.wikipedia.org/wiki/Mona_Simpson_(The_Simpsons)");
        var homer = new Male("Homer", "https://en.wikipedia.org/wiki/Homer_Simpson");
        var clancy = new Male("Clancy", "https://en.wikipedia.org/wiki/Simpson_family#Clancy_Bouvier");
        var jackie = new Female("Jackie", "https://en.wikipedia.org/wiki/Simpson_family#Jacqueline_Bouvier");
        var marge = new Female("Marge", "https://en.wikipedia.org/wiki/Marge_Simpson");
        var patty = new Female("Patty", "https://en.wikipedia.org/wiki/Patty_and_Selma");
        var selma = new Female("Selma", "https://en.wikipedia.org/wiki/Patty_and_Selma");
        var ling = new Female("Ling", "https://en.wikipedia.org/wiki/Simpson_family#Ling_Bouvier");
        var bart = new Male("Bart", "https://en.wikipedia.org/wiki/Bart_Simpson");
        var lisa = new Female("Lisa", "https://en.wikipedia.org/wiki/Lisa_Simpson");
        var maggie = new Female("Maggie", "https://en.wikipedia.org/wiki/Maggie_Simpson");
        var f1 = new Family();
        var f2 = new Family();
        var f3 = new Family();
        AvaloniaGraphControl.Edge CreateEdge(object x, object y) => new AvaloniaGraphControl.Edge(
          x, y, tailSymbol: AvaloniaGraphControl.Edge.Symbol.None, headSymbol: AvaloniaGraphControl.Edge.Symbol.None
        );
        yield return CreateEdge(abraham, f1);
        yield return CreateEdge(mona, f1);
        yield return CreateEdge(f1, homer);
        yield return CreateEdge(clancy, f2);
        yield return CreateEdge(jackie, f2);
        yield return CreateEdge(f2, marge);
        yield return CreateEdge(f2, patty);
        yield return CreateEdge(f2, selma);
        yield return CreateEdge(homer, f3);
        yield return CreateEdge(marge, f3);
        yield return CreateEdge(f3, bart);
        yield return CreateEdge(f3, lisa);
        yield return CreateEdge(f3, maggie);
        yield return CreateEdge(selma, ling);
      }
    }

    public static Graph StateMachine
    {
      get
      {
        var graph = new Microsoft.Msagl.Drawing.Graph("graph");
        graph.RootSubgraph.IsVisible = false;

        var mainGraph = new Subgraph("State Machine");
        mainGraph.Attr.FillColor = Color.LightGray;
        graph.RootSubgraph.AddSubgraph(mainGraph);

        var onGraph = new Subgraph("On");
        onGraph.Attr.FillColor = Color.LightSkyBlue;
        mainGraph.AddSubgraph(onGraph);

        var availableGraph = new Subgraph("Available");
        availableGraph.Attr.FillColor = Color.LightGreen;
        onGraph.AddSubgraph(availableGraph);

        void SetInitialNode(Subgraph sugbgraph, string nodeID)
        {
          With(graph.FindNode(nodeID), n =>
          {
            sugbgraph.AddNode(n);
            n.Label.Text = string.Empty;
            n.Label.IsVisible = false;
            n.Attr.FillColor = Color.Black;
            n.Attr.Shape = Shape.Circle;
          });
        }
        void SetRegularNode(Subgraph subgraph, Color color, string nodeID)
        {
          With(graph.FindNode(nodeID), n =>
          {
            subgraph.AddNode(n);
            n.Attr.FillColor = color;
          });
        }

        graph.AddEdge("initAvailable", "Paused");
        graph.AddEdge("Paused", "Starting").LabelText = "Start";
        graph.AddEdge("Starting", "Started").LabelText = "StartingComplete";
        graph.AddEdge("Started", "Pausing").LabelText = "Pause";
        graph.AddEdge("Pausing", "Paused").LabelText = "PausingComplete";
        SetInitialNode(availableGraph, "initAvailable");
        void SetAvailableNode(string nodeID) => SetRegularNode(availableGraph, Color.Yellow, nodeID);
        SetAvailableNode("Paused");
        SetAvailableNode("Starting");
        SetAvailableNode("Started");
        SetAvailableNode("Pausing");

        graph.AddEdge("initOn", availableGraph.Id);
        graph.AddEdge(availableGraph.Id, "Failure").LabelText = "FailureDetected";
        graph.AddEdge("Failure", availableGraph.Id).LabelText = "Reset";
        SetInitialNode(onGraph, "initOn");
        void SetOnNode(string nodeID) => SetRegularNode(onGraph, Color.LimeGreen, nodeID);
        SetOnNode("Failure");

        graph.AddEdge("initMain", "Off");
        graph.AddEdge("Off", onGraph.Id).LabelText = "SwitchOn";
        graph.AddEdge(onGraph.Id, "Cleaning").LabelText = "SwitchOff";
        graph.AddEdge("Cleaning", "Off").LabelText = "CleaningComplete";
        SetInitialNode(mainGraph, "initMain");
        void SetMainNode(string nodeID) => SetRegularNode(mainGraph, Color.CornflowerBlue, nodeID);
        SetMainNode("Off");
        SetMainNode("Cleaning");

        foreach (var edge in graph.Edges)
        {
          if (edge.Label != null)
            edge.Label.FontSize = 6;
        }
        return graph;
      }
    }

    public static IEnumerable<AvaloniaGraphControl.Edge> StateMachineEdges
    {
      get
      {
        var initAvailable = new InitialState();
        var paused = new State("Paused");
        var starting = new State("Starting");
        var started = new State("Started");
        var pausing = new State("Pausing");
        AvaloniaGraphControl.Edge CreateEdge(object x, object y, object label = null) => new AvaloniaGraphControl.Edge(x, y, label);
        yield return CreateEdge(initAvailable, paused);
        yield return CreateEdge(paused, starting, "Start");
        yield return CreateEdge(starting, started, "StartingComplete");
        yield return CreateEdge(started, pausing, "Pause");
        yield return CreateEdge(pausing, paused, "PausingComplete");
      }
    }
    private static void With<T>(T t, Action<T> a) => a(t);

  }
}
