using System.Drawing;

namespace OwlDomain.ParsingTools.Styling;

public static class StylingHtmlExtensions
{
	extension(Color color)
	{
		#region Properties
		public string ToHex
		{
			get
			{
				if (color.A is not 255)
					return $"#{color.R:x2}{color.G:x2}{color.B:x2}{color.A:x2}";

				return $"#{color.R:x2}{color.G:x2}{color.B:x2}";
			}
		}
		public string ToRgb => $"rgb({color.R},{color.G},{color.B})";
		public string ToHtml => ColorTranslator.ToHtml(color);
		public string ToInlineCss => $"color:{color.ToRgb}";
		#endregion
	}

	extension(StylingEffect effect)
	{
		#region Properties
		public string ToInlineCss
		{
			get
			{
				List<string> parts = [];

				if (effect.HasFlag(StylingEffect.Bold))
					parts.Add("font-weight:bold");

				if (effect.HasFlag(StylingEffect.Italic))
					parts.Add("font-style:italic");

				if (effect.HasFlag(StylingEffect.Wavy))
					parts.Add("text-decoration-style:wavy");
				else if (effect.HasFlag(StylingEffect.Dotted))
					parts.Add("text-decoration-style:dotted");
				else if (effect.HasFlag(StylingEffect.Underline))
					parts.Add("text-decoration-line:underline");

				if (effect.HasFlag(StylingEffect.Dim))
					parts.Add("opacity:0.33");

				return string.Join(";", parts);
			}
		}
		#endregion
	}

	extension(StyleInfo style)
	{
		#region Properties
		public string ToHtmlStyle
		{
			get
			{
				string css = style.ToInlineCss;
				if (string.IsNullOrEmpty(css))
					return string.Empty;

				return $"style=\"{css}\"";
			}
		}
		public string ToInlineCss
		{
			get
			{
				return (style.Color, style.Effect) switch
				{
					(Color color, StylingEffect effect) => string.Concat(color.ToInlineCss, ";", effect.ToInlineCss),
					(_, StylingEffect effect) => effect.ToInlineCss,
					(Color color, _) => color.ToInlineCss,
					_ => ""
				};
			}
		}
		#endregion
	}
}
