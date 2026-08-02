namespace OwlDomain.Owl.Config.CodeAnalysis.Sources;

public static class ConfigSourceExtensions
{
	extension(ISourceFile source)
	{
		#region Properties
		public bool IsWorkspaceFile => source.SimpleName == "owl.workspace";
		public bool IsPackageFile => source.SimpleName == "owl.package";
		public bool IsConfigFile => source.SimpleName == "owl.config";
		public bool IsConfigFileGroup
		{
			get
			{
				return
					source.IsWorkspaceFile ||
					source.IsPackageFile ||
					source.IsConfigFile
				;
			}
		}
		#endregion
	}
	extension(IEnumerable<ISourceFile> sources)
	{
		#region Methods
		public IEnumerable<ISourceFile> OnlyWorkspace() => sources.Where(s => s.IsWorkspaceFile);
		public IEnumerable<ISourceFile> OnlyConfig() => sources.Where(s => s.IsConfigFile);
		public IEnumerable<ISourceFile> OnlyPackage() => sources.Where(s => s.IsPackageFile);
		#endregion
	}
}
