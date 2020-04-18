using System;
using System.Collections.Generic;
using Avalonia.Media;
using AvaloniaGraphControl;

namespace AvaloniaGraphControlSample
{

  class StandardItem
  {
    public StandardItem(string name) { Name = name; }
    public string Name { get; private set; }
  }

  class SimpleGraph : NamedGraph
  {
    public SimpleGraph() : base("Simple Graph")
    {
      var a = new StandardItem("A");
      var b = new StandardItem("B");
      var c = new StandardItem("C");
      var d = new StandardItem("D");
      var e = new StandardItem("E");
      Edges.Add(new Edge(a, b));
      Edges.Add(new Edge(a, d));
      Edges.Add(new Edge(a, e));
      Edges.Add(new Edge(b, c));
      Edges.Add(new Edge(b, d));
      Edges.Add(new Edge(d, a));
      Edges.Add(new Edge(d, e));
    }
  }

  class SimpleOrderedLayoutGraph : NamedGraph
  {
    public SimpleOrderedLayoutGraph() : base("Simple Graph (ordered layout)")
    {
      var a = new StandardItem("A");
      var b = new StandardItem("B");
      var c = new StandardItem("C");
      var d = new StandardItem("D");
      var e = new StandardItem("E");
      Edges.Add(new Edge(a, b));
      Edges.Add(new Edge(a, d));
      Edges.Add(new Edge(a, e));
      Edges.Add(new Edge(b, c));
      Edges.Add(new Edge(b, d));
      Edges.Add(new Edge(d, a));
      Edges.Add(new Edge(d, e));
      static string Name(dynamic o) => o.Name;
      VerticalOrder = (n1,n2) => Name(n1).CompareTo(Name(n2));
    }
  }

  class InteractiveItem
  {
    public InteractiveItem(string name) { Name = name; }
    public string Name { get; private set; }
  }
  class SimpleInteractiveGraph : NamedGraph
  {
    public SimpleInteractiveGraph() : base("Simple Interactive Graph")
    {
      var a = new InteractiveItem("A");
      var b = new InteractiveItem("B");
      var c = new InteractiveItem("C");
      var d = new InteractiveItem("D");
      var e = new InteractiveItem("E");
      Edges.Add(new Edge(a, b));
      Edges.Add(new Edge(a, d));
      Edges.Add(new Edge(a, e));
      Edges.Add(new Edge(b, c));
      Edges.Add(new Edge(b, d));
      Edges.Add(new Edge(d, a));
      Edges.Add(new Edge(d, e));
    }
  }



  class CompositeItem
  {
    public CompositeItem(string name) { Name = name; }
    public string Name { get; private set; }
  }

  class SimpleWithSubgraph : NamedGraph
  {
    public SimpleWithSubgraph() : base("Simple Graph (with subgraph)")
    {
      var a = new StandardItem("A");
      var b = new CompositeItem("B");
      var b1 = new StandardItem("B1");
      var b2 = new StandardItem("B2");
      var b3 = new StandardItem("B3");
      var b4 = new StandardItem("B4");
      var c = new StandardItem("C");
      var d = new StandardItem("D");
      Edges.Add(new Edge(a, b));
      Edges.Add(new Edge(a, c));
      Edges.Add(new Edge(b, d));
      Edges.Add(new Edge(c, d));
      Edges.Add(new Edge(b1, b2));
      Edges.Add(new Edge(b1, b3));
      Edges.Add(new Edge(b2, b4));
      Edges.Add(new Edge(b3, b4));
      Parent[b1]=b;
      Parent[b2]=b;
      Parent[b3]=b;
      Parent[b4]=b;
    }
  }

  class SimpleOrderedLayoutWithSubgraph : NamedGraph
  {
    public SimpleOrderedLayoutWithSubgraph() : base("Simple Graph (ordered layout with subgraph)")
    {
      var a = new StandardItem("A");
      var b = new CompositeItem("B");
      var b1 = new StandardItem("B1");
      var b2 = new StandardItem("B2");
      var b3 = new StandardItem("B3");
      var b4 = new StandardItem("B4");
      var c = new StandardItem("C");
      var d = new StandardItem("D");
      Edges.Add(new Edge(a, b));
      Edges.Add(new Edge(a, c));
      Edges.Add(new Edge(b, d));
      Edges.Add(new Edge(c, d));
      Edges.Add(new Edge(b1, b2));
      Edges.Add(new Edge(b1, b3));
      Edges.Add(new Edge(b2, b4));
      Edges.Add(new Edge(b3, b4));
      Parent[b1]=b;
      Parent[b2]=b;
      Parent[b3]=b;
      Parent[b4]=b;
      static string Name(dynamic o) => o.Name;
      VerticalOrder = (n1,n2) => Name(n1).CompareTo(Name(n2));
    }
  }
}
