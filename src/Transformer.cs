using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;

namespace AvaloniaGraphControl {
  class Transformer {
    private readonly Microsoft.Msagl.Core.Geometry.Point origin;
    private readonly double zoom;

    public Transformer(Microsoft.Msagl.Core.Geometry.Point origin, double zoom) {
      this.origin = origin;
      this.zoom = zoom;
    }

    public Point Transform(Microsoft.Msagl.Core.Geometry.Point pt) {
      return new Point(zoom*(pt.X - origin.X), zoom*(origin.Y - pt.Y));
    }

    public Rect Transform(Microsoft.Msagl.Core.Geometry.Rectangle rect) {
      return new Rect(Transform(rect.LeftTop), Transform(rect.RightBottom));
    }
  }
}
