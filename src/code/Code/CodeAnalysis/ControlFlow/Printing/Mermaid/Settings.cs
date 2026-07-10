using OwlDomain.Owl.Code.Styling;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Printing.Mermaid;

partial class MermaidControlFlow
{
	#region Nested types
	public sealed class Settings : IControlFlowPrinterSettings
	{
		#region Constants
		/// <summary>This is the maximum text size that Mermaid will allow.</summary>
		/// <remarks>
		/// 	It was picked from
		/// 	<see href="https://developer.mozilla.org/docs/Web/JavaScript/Reference/Global_Objects/Number/MAX_SAFE_INTEGER">
		/// 		<c>Number.MAX_SAFE_INTEGER</c>
		/// 	</see>
		/// 	from JavaScript, since that is what Mermaid uses.
		/// </remarks>
		public const long DefaultMaxTextSize = 9007199254740991;
		#endregion

		#region Properties
		public IClassificationStyling? Styles { get; set; } = OwlStyling.Default;
		public long MaxTextSize { get; set; } = DefaultMaxTextSize;
		public bool UseInlineCss { get; set; } = true;
		public bool Optimise { get; set; } = true;
		public bool IncludePlainSource { get; set; } = true;
		public bool IncludeSourceReference { get; set; } = true;
		public bool IncludeSymbolData { get; set; } = true;
		public bool IncludeLineData { get; set; } = true;
		public bool IncludeLineNavigation { get; set; } = true;
		public int LineNavigationPadding { get; set; } = 2;
		public int? TabSize { get; set; } = 3;
		#endregion
	}
	#endregion
}
