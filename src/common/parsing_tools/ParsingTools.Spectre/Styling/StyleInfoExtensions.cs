namespace OwlDomain.ParsingTools.Styling;

public static class StyleInfoExtensions
{
	extension(StyleInfo? info)
	{
		#region Properties
		[NotNullIfNotNull(nameof(info))]
		public Style? AsSpectre => info?.AsSpectre ?? (Style?)null; // Note(Nightowl): This is dumb, why is C# complaining about this specific null but not the other ones in this file;
		#endregion
	}
	extension(StyleInfo info)
	{
		#region Properties
		public Style AsSpectre => new(foreground: info.Color.AsSpectre, decoration: info.Effect.AsSpectre);
		#endregion
	}

	extension(StylingEffect? effect)
	{
		#region Properties
		[NotNullIfNotNull(nameof(effect))]
		public Decoration? AsSpectre => effect?.AsSpectre ?? null;
		#endregion
	}
	extension(StylingEffect effect)
	{
		#region Properties
		public Decoration AsSpectre
		{
			get
			{
				Decoration decoration = Decoration.None;

				if (effect.HasFlag(StylingEffect.Bold))
					decoration |= Decoration.Bold;

				if (effect.HasFlag(StylingEffect.Italic))
					decoration |= Decoration.Italic;

				if (effect.HasFlag(StylingEffect.Underline))
					decoration |= Decoration.Underline;

				if (effect.HasFlag(StylingEffect.Dim))
					decoration |= Decoration.Dim;

				return decoration;
			}
		}
		#endregion
	}

	extension(System.Drawing.Color? color)
	{
		#region Properties
		[NotNullIfNotNull(nameof(color))]
		public Color? AsSpectre => color?.AsSpectre ?? null;
		#endregion
	}
	extension(System.Drawing.Color color)
	{
		#region Properties
		public Color AsSpectre => new(color.R, color.G, color.B);
		#endregion
	}
}
