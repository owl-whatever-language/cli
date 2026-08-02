using System.IO;

namespace OwlDomain.Owl.Code.CodeAnalysis.Sources;

public static class CodeSourceExtensions
{
	extension(ISourceFile source)
	{
		#region Properties
		public bool IsCodeFile => Path.GetExtension(source.SimpleName) == ".owl";
		#endregion
	}
	extension(IEnumerable<ISourceFile> sources)
	{
		#region Methods
		public IEnumerable<ISourceFile> OnlyCode() => sources.Where(s => s.IsCodeFile);
		#endregion
	}
}
