namespace OwlDomain.ParsingTools.Generator.Syntax.Structured;

internal class StructuredMemberInfo
{
	#region Nested types
	public sealed class Comparer(bool includeType) : EqualityComparer<StructuredMemberInfo>
	{
		#region Properties
		public static Comparer WithType { get; } = new(true);
		public static Comparer JustName { get; } = new(false);
		public override bool Equals(StructuredMemberInfo x, StructuredMemberInfo y)
		{
			if (includeType)
			{
				return
					x.Name == y.Name &&
					x.Type.TypeName == y.Type.TypeName;
			}

			return x.Name == y.Name;
		}
		public override int GetHashCode(StructuredMemberInfo obj)
		{
			string text;
			if (includeType)
				text = $"{obj.Type.TypeName} {obj.Name.Original}";
			else
				text = obj.Name.Original;

			return text.GetHashCode();
		}
		#endregion
	}
	#endregion

	#region Properties
	public Name Name { get; }
	public IStructuredTypeInfo Type { get; }
	public IReadOnlyList<StructuredMemberInfo> Shadows { get; }
	public string Owner { get; }
	#endregion

	#region Constructors
	public StructuredMemberInfo(Name name, string owner, IStructuredTypeInfo type, IEnumerable<StructuredMemberInfo?> shadows)
	{
		Name = name;
		Owner = owner;
		Type = type;

		Shadows = shadows.Where(s => s is not null).ToArray()!;
	}
	#endregion
}
