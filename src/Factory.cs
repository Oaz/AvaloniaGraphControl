using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Media;
using Microsoft.Msagl.Drawing;

namespace AvaloniaGraphControl {
  static class Factory {
    
    public static FormattedText CreateText(Microsoft.Msagl.Drawing.Label label)
    {
      if (label == null)
        return null;
      var fontFamily = CreateFontFamily(label);
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

    public static FontFamily CreateFontFamily(Label label)
    {
      return new FontFamily(label.FontName);
    }

    public static (Avalonia.Media.FontStyle, FontWeight) GetFontProps(Microsoft.Msagl.Drawing.FontStyle mStyle) =>
      mStyle switch
      {
        Microsoft.Msagl.Drawing.FontStyle.Regular => (Avalonia.Media.FontStyle.Normal, FontWeight.Regular),
        Microsoft.Msagl.Drawing.FontStyle.Bold => (Avalonia.Media.FontStyle.Normal, FontWeight.Bold),
        Microsoft.Msagl.Drawing.FontStyle.Italic => (Avalonia.Media.FontStyle.Italic, FontWeight.Regular),
        Microsoft.Msagl.Drawing.FontStyle.Underline => (Avalonia.Media.FontStyle.Normal, FontWeight.Regular),
        Microsoft.Msagl.Drawing.FontStyle.Strikeout => (Avalonia.Media.FontStyle.Normal, FontWeight.Regular),
        _ => (Avalonia.Media.FontStyle.Normal, FontWeight.Regular)
      };

    public static Avalonia.Media.Color CreateColor(Microsoft.Msagl.Drawing.Color color) => new Avalonia.Media.Color(color.A, color.R, color.G, color.B);
  }
}
