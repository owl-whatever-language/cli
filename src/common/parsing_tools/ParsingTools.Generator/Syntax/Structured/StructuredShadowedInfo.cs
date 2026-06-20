namespace OwlDomain.ParsingTools.Generator.Syntax.Structured;

internal interface IStructuredShadowedInfo<T>
	where T : notnull, IStructuredShadowedInfo<T>
{
	#region Properties
	T? Shadows { get; }
	T? ShadowedBy { get; }
	#endregion
}
