# AvaloniaGraphControl
A Graph display control for AvaloniaUI with model based on MSAGL

## Usage
Each individual graph is displayed through the GraphView control included in the assembly
```xml
<Window xmlns:agc="clr-namespace:AvaloniaGraphControl;assembly=AvaloniaGraphControl">
    <agc:GraphView Source="{Binding MyGraph}" Zoom="1.2" LayoutMethod="SugiyamaScheme" />
</Window>
```
The `Source` property is bound to a `Microsoft.Msagl.Drawing.Graph` instance from [MSAGL (Microsoft Automatic Graph Layout)](https://github.com/microsoft/automatic-graph-layout). Please have a look at this project for all information related to graph definition.

The following layout methods are available in MSAGL and can be set in the GraphView control independently of the graph model:
* [SugiyamaScheme](https://en.wikipedia.org/wiki/Layered_graph_drawing)
* [MDS](https://en.wikipedia.org/wiki/Stress_majorization)
* Ranking
* IncrementalLayout

## Example of graph definition

```C#
public static Microsoft.Msagl.Drawing.Graph MyGraph
{
  get
  {
    var graph = new Microsoft.Msagl.Drawing.Graph("graph");
    graph.AddEdge("A", "B");
    graph.AddEdge("A", "D");
    graph.AddEdge("A", "E");
    graph.AddEdge("B", "C");
    graph.AddEdge("B", "D");
    graph.AddEdge("D", "A");
    graph.AddEdge("D", "E");
    graph.LayerConstraints.AddUpDownConstraint(graph.FindNode("A"), graph.FindNode("D"));
    foreach (var node in graph.Nodes)
    {
      node.Attr.Shape = Shape.Ellipse;
      node.LabelText = "   " + node.LabelText + "   ";
    }
    return graph;
  }
}
```
![Outcome of graph example](doc/images/Simple_Graph.png?raw=true)

## Some graphs from the sample application

![Family Tree](doc/images/Family_Tree.png?raw=true)

![State Machine](doc/images/State_Machine.png?raw=true)
