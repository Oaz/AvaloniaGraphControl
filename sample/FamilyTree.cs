using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using AvaloniaGraphControl;

namespace AvaloniaGraphControlSample
{
  class FamilyMember
  {
    public FamilyMember(string name, IBrush backgroungColor, string url)
    {
      Name = name;
      BackgroundColor = backgroungColor;
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
    public IBrush BackgroundColor { get; private set; }
    public string URL { get; private set; }
  }

  class Male : FamilyMember
  {
    public Male(string name, string url) : base(name, Brushes.LightSkyBlue, url) { }
  }
  class Female : FamilyMember
  {
    public Female(string name, string url) : base(name, Brushes.LightPink, url) { }
  }
  class Family
  {
  }

  class FamilyTree : NamedGraph
  {
    public FamilyTree() : base("Family Tree")
    {
      Orientation = Orientations.Vertical;
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
      static Edge CreateEdge(object x, object y) => new Edge(
        x, y, tailSymbol: Edge.Symbol.None, headSymbol: Edge.Symbol.None
      );
      Edges.Add(CreateEdge(abraham, f1));
      Edges.Add(CreateEdge(mona, f1));
      Edges.Add(CreateEdge(f1, homer));
      Edges.Add(CreateEdge(clancy, f2));
      Edges.Add(CreateEdge(jackie, f2));
      Edges.Add(CreateEdge(f2, marge));
      Edges.Add(CreateEdge(f2, patty));
      Edges.Add(CreateEdge(f2, selma));
      Edges.Add(CreateEdge(homer, f3));
      Edges.Add(CreateEdge(marge, f3));
      Edges.Add(CreateEdge(f3, bart));
      Edges.Add(CreateEdge(f3, lisa));
      Edges.Add(CreateEdge(f3, maggie));
      Edges.Add(CreateEdge(selma, ling));
      HorizontalOrder = (p1, p2) => !ParentsOfSameFamily(p1, p2) ? 0 : (p1 is Male) ? -1 : 1;
    }

    private bool ParentsOfSameFamily(object p1, object p2)
    {
      var fp1 = Edges.FirstOrDefault(e => e.Tail == p1)?.Head;
      var fp2 = Edges.FirstOrDefault(e => e.Tail == p2)?.Head;
      return fp1 != null && fp2 != null && fp1 == fp2;
    }
  }
}
