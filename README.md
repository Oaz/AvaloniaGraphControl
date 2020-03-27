# AvaloniaGraphControl
A graph layout panel for AvaloniaUI

## Usage
Each individual graph is displayed through the GraphPanel control included in the assembly
```xml
<Window xmlns:agc="clr-namespace:AvaloniaGraphControl;assembly=AvaloniaGraphControl">
    <agc:GraphPanel Graph="{Binding MyGraph}" Zoom="1.2" LayoutMethod="SugiyamaScheme" />
</Window>
```

The layout is internally implemented with [MSAGL (Microsoft Automatic Graph Layout)](https://github.com/microsoft/automatic-graph-layout).

The following layout methods are available in MSAGL and can be set in the GraphPanel control independently of the graph model:
* [SugiyamaScheme](https://en.wikipedia.org/wiki/Layered_graph_drawing)
* [MDS](https://en.wikipedia.org/wiki/Stress_majorization)
* Ranking
* IncrementalLayout

The GraphPanel control and the MSAGL assemblies are bundled in [a nuget package](https://www.nuget.org/packages/AvaloniaGraphControl/).
The existing MSAGL nuget packages are dedicated to the .NET Framework and do not include any netstandard assembly.

## Example of graph definition

```C#
public static Graph MyGraph
{
  get
  {
    var graph = new Graph();
    graph.Edges.Add(new Edge("A", "B"));
    graph.Edges.Add(new Edge("A", "D"));
    graph.Edges.Add(new Edge("A", "E"));
    graph.Edges.Add(new Edge("B", "C"));
    graph.Edges.Add(new Edge("B", "D"));
    graph.Edges.Add(new Edge("D", "A"));
    graph.Edges.Add(new Edge("D", "E"));
    return graph;
  }
}
```
![Outcome of graph example](doc/images/Simple_Graph.png?raw=true)

## Some graphs from the sample application

![Family Tree](doc/images/Family_Tree.png?raw=true)

![State Machine](doc/images/State_Machine.png?raw=true)
