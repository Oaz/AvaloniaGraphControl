using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;

namespace AvaloniaGraphControl {
  class Transformer {
    private readonly Microsoft.Msagl.Core.Geometry.Point origin;

    public Transformer(Microsoft.Msagl.Core.Geometry.Point origin) {
      this.origin = origin;
    }

    public Point Transform(Microsoft.Msagl.Core.Geometry.Point pt) {
      return new Point(pt.X - origin.X, origin.Y - pt.Y);
    }

    public Rect Transform(Microsoft.Msagl.Core.Geometry.Rectangle rect) {
      return new Rect(Transform(rect.LeftTop), Transform(rect.RightBottom));
    }
  }
}
