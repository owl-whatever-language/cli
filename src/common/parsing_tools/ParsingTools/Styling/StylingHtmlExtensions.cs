using System.Drawing;

namespace OwlDomain.ParsingTools.Styling;

public static class StylingHtmlExtensions
{
	extension(Color color)
	{
		#region Properties
		public string ToHtml => ColorTranslator.ToHtml(color);
		public string ToCss => $"color:{color.ToHtml};";
		#endregion
	}

	extension(StylingEffect effect)
	{
		#region Properties
		public string ToCss
		{
			get
			{
				List<string> parts = [];

				if (effect.HasFlag(StylingEffect.Bold))
					parts.Add("font-weight:bold;");

				if (effect.HasFlag(StylingEffect.Italic))
					parts.Add("font-style:italic;");

				if (effect.HasFlag(StylingEffect.Wavy))
					parts.Add("text-decoration-style:wavy;");
				else if (effect.HasFlag(StylingEffect.Dotted))
					parts.Add("text-decoration-style:dotted;");
				else if (effect.HasFlag(StylingEffect.Underline))
					parts.Add("text-decoration-line:underline;");

				return string.Concat(parts);
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
				string css = style.ToCss;
				if (string.IsNullOrEmpty(css))
					return string.Empty;

				return $"style=\"{css}\"";
			}
		}
		public string ToCss
		{
			get
			{
				return (style.Color, style.Effect) switch
				{
					(Color color, StylingEffect effect) => string.Concat(color.ToCss, effect.ToCss),
					(_, StylingEffect effect) => effect.ToCss,
					(Color color, _) => color.ToCss,
					_ => ""
				};
			}
		}
		#endregion
	}
}
