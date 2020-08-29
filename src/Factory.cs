using Avalonia;
using Avalonia.Media;
using MsaglDrawing = Microsoft.Msagl.Drawing;

namespace AvaloniaGraphControl
{
  static class Factory
  {
    public static FormattedText CreateText(MsaglDrawing.Label label)
    {
      if (label == null)
        return null;
      var fontFamily = CreateFontFamily(label);
      var fontSize = label.FontSize;
      var (fontStyle, fontWeight) = GetFontProps(label.FontStyle);
      var ftext = new FormattedText
      {
        Constraint = new Size(10, 10),
        Typeface = FontManager.Current.GetOrAddTypeface(fontFamily, fontStyle, fontWeight),
        Text = label.Text,
        FontSize = fontSize,
      };
      return ftext;
    }

    public static FontFamily CreateFontFamily(MsaglDrawing.Label label)
    {
      return new FontFamily(label.FontName);
    }

    public static (FontStyle, FontWeight) GetFontProps(MsaglDrawing.FontStyle mStyle) =>
      mStyle switch
      {
        MsaglDrawing.FontStyle.Regular => (FontStyle.Normal, FontWeight.Regular),
        MsaglDrawing.FontStyle.Bold => (FontStyle.Normal, FontWeight.Bold),
        MsaglDrawing.FontStyle.Italic => (FontStyle.Italic, FontWeight.Regular),
        MsaglDrawing.FontStyle.Underline => (FontStyle.Normal, FontWeight.Regular),
        MsaglDrawing.FontStyle.Strikeout => (FontStyle.Normal, FontWeight.Regular),
        _ => (FontStyle.Normal, FontWeight.Regular)
      };

    public static Color CreateColor(MsaglDrawing.Color color) =>
      new Color(color.A, color.R, color.G, color.B);
  }
}
