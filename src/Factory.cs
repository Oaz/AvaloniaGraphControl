using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Media;

namespace AvaloniaGraphControl {
  static class Factory {
    
    public static FormattedText CreateText(Microsoft.Msagl.Drawing.Label label) {
      if(label == null)
        return null;
      var fontFamily = new FontFamily(label.FontName);
      var fontSize = label.FontSize;
      var (fontStyle, fontWeight) = GetFontProps(label.FontStyle);
      var ftext = new FormattedText
      {
        Constraint = new Size(10, 10),
        Typeface = new Typeface(fontFamily, fontSize, fontStyle, fontWeight),
        Text = label.Text
      };
      return ftext;
    }

    public static (FontStyle, FontWeight) GetFontProps(Microsoft.Msagl.Drawing.FontStyle mStyle) =>
      mStyle switch
      {
        Microsoft.Msagl.Drawing.FontStyle.Regular => (FontStyle.Normal, FontWeight.Regular),
        Microsoft.Msagl.Drawing.FontStyle.Bold => (FontStyle.Normal, FontWeight.Bold),
        Microsoft.Msagl.Drawing.FontStyle.Italic => (FontStyle.Italic, FontWeight.Regular),
        Microsoft.Msagl.Drawing.FontStyle.Underline => (FontStyle.Normal, FontWeight.Regular),
        Microsoft.Msagl.Drawing.FontStyle.Strikeout => (FontStyle.Normal, FontWeight.Regular),
        _ => (FontStyle.Normal, FontWeight.Regular)
      };

    public static Color CreateColor(Microsoft.Msagl.Drawing.Color color) => new Color(color.A, color.R, color.G, color.B);
  }
}
