using OwlDomain.Owl.Code.Execution.Builtins.Attributes;

namespace OwlDomain.Owl.Code.Execution.Builtins.Core;

internal partial class CoreBuiltins
{
	[Name("bool")]
	public sealed class Bool
	{
		#region Properties
		[Ignore]
		public bool Value { get; }
		#endregion

		#region Constructors
		public Bool(bool value) => Value = value;
		#endregion

		#region Methods
		[Ignore]
		public override string ToString() => Value ? "true" : "false";
		#endregion

		#region Operators
		[Operator]
		public static Bool LogicalAnd(Bool left, Bool right) => new(left.Value && right.Value);

		[Operator]
		public static Bool LogicalOr(Bool left, Bool right) => new(left.Value || right.Value);

		[Operator]
		public static Bool Equal(Bool left, Bool right) => new(left.Value == right.Value);

		[Operator]
		public static Bool NotEqual(Bool left, Bool right) => new(left.Value != right.Value);
		#endregion
	}

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

	[Name("int")]
	public sealed class Int
	{
		#region Properties
		[Ignore]
		public object Value { get; }
		#endregion

		#region Constructors
		public Int(object value) => Value = value;
		#endregion

		#region Methods
		[Ignore]
		public override string ToString() => Value.ToString() ?? "0";
		#endregion

		#region Operators
		[Ignore]
		private static int Compare(Int left, Int right) => ((IComparable)left.Value).CompareTo(right.Value);

		[Operator]
		public static Int Modulo(Int left, Int right)
		{
			long l = (long)left.Value;
			long r = (long)right.Value;

			return new(l % r);
		}

		[Operator]
		public static Bool Equal(Int left, Int right) => new(Compare(left, right) == 0);

		[Operator]
		public static Bool NotEqual(Int left, Int right) => new(Compare(left, right) != 0);

		[Operator]
		public static Bool LessThan(Int left, Int right) => new(Compare(left, right) < 0);

		[Operator]
		public static Bool LessThanOrEqual(Int left, Int right) => new(Compare(left, right) <= 0);

		[Operator]
		public static Bool GreaterThan(Int left, Int right) => new(Compare(left, right) > 0);

		[Operator]
		public static Bool GreaterThanOrEqual(Int left, Int right) => new(Compare(left, right) >= 0);

		[Operator]
		public static Int Add(Int left, Int right)
		{
			long l = (long)left.Value;
			long r = (long)right.Value;

			return new(l + r);
		}
		#endregion
	}
}
