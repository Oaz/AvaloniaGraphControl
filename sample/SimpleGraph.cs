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
}
