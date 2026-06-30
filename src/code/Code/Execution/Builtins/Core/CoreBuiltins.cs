using OwlDomain.Owl.Code.Execution.Builtins.Attributes;

namespace OwlDomain.Owl.Code.Execution.Builtins.Core;

internal partial class CoreBuiltins
{
	[Name("text")]
	public sealed class Text
	{
		#region Properties
		[Ignore]
		public string Value { get; }
		#endregion

		#region Constructors
		public Text(string value) => Value = value;
		#endregion

		#region Methods
		[Ignore]
		public override string ToString() => Value;
		#endregion
	}
}
