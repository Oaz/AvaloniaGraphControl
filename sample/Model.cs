using System;
using System.Collections.Generic;
using Avalonia.Media;
using Microsoft.Msagl.Drawing;

namespace AvaloniaGraphControlSample
{
  class NamedGraph
  {
    public NamedGraph()
    {
      Hierarchy = _ => null;
    }
    public string Name { get; set; }
    public Graph Graph { get; set; }
    public (IEnumerable<AvaloniaGraphControl.Edge>, Func<object, object>) Definition
    {
      set
      {
        Edges = value.Item1;
        Hierarchy = value.Item2;
      }
    }
    public IEnumerable<AvaloniaGraphControl.Edge> Edges { get; set; }
    public Func<object, object> Hierarchy { get; set; }
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
    public State(string name, IBrush color) { Name = name; Color = color; }
    public string Name { get; private set; }
    public IBrush Color { get; private set; }
  }
  class InitialState
  {
  }
  class CompositeState
  {
    public CompositeState(string name, IBrush color) { Name = name; Color = color; }
    public string Name { get; private set; }
    public IBrush Color { get; private set; }
  }

  class Model
  {
    public IEnumerable<NamedGraph> SampleGraphs => new NamedGraph[] {
      new NamedGraph {Name="Simple Graph", Definition=SimpleGraphEdges},
      new NamedGraph {Name="Simple Interactive Graph", Definition=SimpleInteractiveGraphEdges},
      new NamedGraph {Name="Family Tree", Definition=FamilyTreeEdges},
      new NamedGraph {Name="State Machine", Definition=StateMachineEdges}
    };

    public static (IEnumerable<AvaloniaGraphControl.Edge>, Func<object, object>) SimpleGraphEdges
    {
      get
      {
        var a = new StandardItem("A");
        var b = new StandardItem("B");
        var c = new StandardItem("C");
        var d = new StandardItem("D");
        var e = new StandardItem("E");
        var edges = new List<AvaloniaGraphControl.Edge>();
        edges.Add(new AvaloniaGraphControl.Edge(a, b));
        edges.Add(new AvaloniaGraphControl.Edge(a, d));
        edges.Add(new AvaloniaGraphControl.Edge(a, e));
        edges.Add(new AvaloniaGraphControl.Edge(b, c));
        edges.Add(new AvaloniaGraphControl.Edge(b, d));
        edges.Add(new AvaloniaGraphControl.Edge(d, a));
        edges.Add(new AvaloniaGraphControl.Edge(d, e));
        return (edges, _ => null);
      }
    }

    public static (IEnumerable<AvaloniaGraphControl.Edge>, Func<object, object>) SimpleInteractiveGraphEdges
    {
      get
      {
        var a = new InteractiveItem("A");
        var b = new InteractiveItem("B");
        var c = new InteractiveItem("C");
        var d = new InteractiveItem("D");
        var e = new InteractiveItem("E");
        var edges = new List<AvaloniaGraphControl.Edge>();
        edges.Add(new AvaloniaGraphControl.Edge(a, b));
        edges.Add(new AvaloniaGraphControl.Edge(a, d));
        edges.Add(new AvaloniaGraphControl.Edge(a, e));
        edges.Add(new AvaloniaGraphControl.Edge(b, c));
        edges.Add(new AvaloniaGraphControl.Edge(b, d));
        edges.Add(new AvaloniaGraphControl.Edge(d, a));
        edges.Add(new AvaloniaGraphControl.Edge(d, e));
        return (edges, _ => null);
      }
    }

    public static (IEnumerable<AvaloniaGraphControl.Edge>, Func<object, object>) FamilyTreeEdges
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
        var edges = new List<AvaloniaGraphControl.Edge>();
        edges.Add(CreateEdge(abraham, f1));
        edges.Add(CreateEdge(mona, f1));
        edges.Add(CreateEdge(f1, homer));
        edges.Add(CreateEdge(clancy, f2));
        edges.Add(CreateEdge(jackie, f2));
        edges.Add(CreateEdge(f2, marge));
        edges.Add(CreateEdge(f2, patty));
        edges.Add(CreateEdge(f2, selma));
        edges.Add(CreateEdge(homer, f3));
        edges.Add(CreateEdge(marge, f3));
        edges.Add(CreateEdge(f3, bart));
        edges.Add(CreateEdge(f3, lisa));
        edges.Add(CreateEdge(f3, maggie));
        edges.Add(CreateEdge(selma, ling));
        return (edges, _ => null);
      }
    }

    public static (IEnumerable<AvaloniaGraphControl.Edge>, Func<object, object>) StateMachineEdges
    {
      get
      {
        var initMain = new InitialState();
        var on = new CompositeState("On",Brushes.LightSkyBlue);
        var off = new State("Off",Brushes.CornflowerBlue);
        var cleaning = new State("Cleaning",Brushes.CornflowerBlue);
        var initOn = new InitialState();
        var available = new CompositeState("Available",Brushes.LightGreen);
        var failure = new State("Failure",Brushes.LimeGreen);
        var initAvailable = new InitialState();
        var paused = new State("Paused",Brushes.Yellow);
        var starting = new State("Starting",Brushes.Yellow);
        var started = new State("Started",Brushes.Yellow);
        var pausing = new State("Pausing",Brushes.Yellow);
        AvaloniaGraphControl.Edge CreateEdge(object x, object y, object label = null) => new AvaloniaGraphControl.Edge(x, y, label);
        var edges = new List<AvaloniaGraphControl.Edge>();
        edges.Add(CreateEdge(initMain, off));
        edges.Add(CreateEdge(off, on, "SwitchOn"));
        edges.Add(CreateEdge(on, cleaning, "SwitchOff"));
        edges.Add(CreateEdge(cleaning, off, "CleaningComplete"));
        edges.Add(CreateEdge(initOn, available));
        edges.Add(CreateEdge(available, failure, "FailureDetected"));
        edges.Add(CreateEdge(failure, available, "Reset"));
        edges.Add(CreateEdge(initAvailable, paused));
        edges.Add(CreateEdge(paused, starting, "Start"));
        edges.Add(CreateEdge(starting, started, "StartingComplete"));
        edges.Add(CreateEdge(started, pausing, "Pause"));
        edges.Add(CreateEdge(pausing, paused, "PausingComplete"));
        var parent = new Dictionary<object, object>();
        parent[initOn] = on;
        parent[available] = on;
        parent[failure] = on;
        parent[initAvailable] = available;
        parent[paused] = available;
        parent[starting] = available;
        parent[started] = available;
        parent[pausing] = available;
        return (edges, x => parent.GetValueOrDefault(x));
      }
    }

    private static void With<T>(T t, Action<T> a) => a(t);

  }
}
