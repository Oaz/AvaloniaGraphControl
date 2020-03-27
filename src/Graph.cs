
using System;
using System.Collections.Generic;

namespace AvaloniaGraphControl
{
  public class Graph
  {
    public Graph()
    {
      Edges = new List<Edge>();
      Parent = new Indexer<object, object>(k => hierarchy.GetValueOrDefault(k), (k,v) => hierarchy[k] = v);
    }
    public readonly ICollection<Edge> Edges;
    public readonly Indexer<object, object> Parent;

    private readonly Dictionary<object, object> hierarchy = new Dictionary<object, object>();
  }
}