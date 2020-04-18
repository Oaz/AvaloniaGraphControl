
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
      Orientation = Orientations.Vertical;
    }
    public readonly ICollection<Edge> Edges;
    public readonly Indexer<object, object> Parent;
    public enum Orientations
    {
      Vertical,
      Horizontal
    }
    public Orientations Orientation { get; set; }

    private readonly Dictionary<object, object> hierarchy = new Dictionary<object, object>();
  }
}