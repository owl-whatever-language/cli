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
		[Operator] public static Bool LogicalAnd(Bool left, Bool right) => new(left.Value && right.Value);
		[Operator] public static Bool LogicalOr(Bool left, Bool right) => new(left.Value || right.Value);
		[Operator] public static Bool Equal(Bool left, Bool right) => new(left.Value == right.Value);
		[Operator] public static Bool NotEqual(Bool left, Bool right) => new(left.Value != right.Value);
		#endregion
	}

	[Name("text")]
	public sealed class Text
	{
		#region Properties
		[Ignore] public string Value { get; }
		#endregion

		#region Constructors
		public Text(string value) => Value = value;
		#endregion

		#region Methods
		[Ignore] public override string ToString() => Value;
		#endregion

		#region Operators
		[Operator] public static Bool Equal(Text left, Text right) => new(left.Value == right.Value);
		[Operator] public static Bool NotEqual(Text left, Text right) => new(left.Value != right.Value);
		[Operator] public static Text Add(Text left, Text right) => new(left.Value + right.Value);
		#endregion
	}

	[Name("int")]
	public sealed class Int
	{
		#region Properties
		[Ignore] public object Value { get; }
		#endregion

		#region Constructors
		public Int(object value) => Value = value;
		#endregion

		#region Methods
		[Ignore]
		public override string ToString() => Value.ToString() ?? "0";
		#endregion

		#region Operators
		[Ignore] private static int Compare(Int left, Int right) => ((IComparable)left.Value).CompareTo(right.Value);

		[Operator] public static Bool Equal(Int left, Int right) => new(Compare(left, right) == 0);
		[Operator] public static Bool NotEqual(Int left, Int right) => new(Compare(left, right) != 0);
		[Operator] public static Bool LessThan(Int left, Int right) => new(Compare(left, right) < 0);
		[Operator] public static Bool LessThanOrEqual(Int left, Int right) => new(Compare(left, right) <= 0);
		[Operator] public static Bool GreaterThan(Int left, Int right) => new(Compare(left, right) > 0);
		[Operator] public static Bool GreaterThanOrEqual(Int left, Int right) => new(Compare(left, right) >= 0);

		[Operator]
		public static Int Add(Int left, Int right)
		{
			long l = (long)left.Value;
			long r = (long)right.Value;

			return new(l + r);
		}

		[Operator]
		public static Int Subtract(Int left, Int right)
		{
			long l = (long)left.Value;
			long r = (long)right.Value;

			return new(l - r);
		}

		[Operator]
		public static Int Multiply(Int left, Int right)
		{
			long l = (long)left.Value;
			long r = (long)right.Value;

			return new(l * r);
		}

		[Operator]
		public static Int Divide(Int left, Int right)
		{
			long l = (long)left.Value;
			long r = (long)right.Value;

			return new(l / r);
		}

		[Operator]
		public static Int Modulo(Int left, Int right)
		{
			long l = (long)left.Value;
			long r = (long)right.Value;

			return new(l % r);
		}
		#endregion
	}

	#region Resolve functions
	public static void Resolve(BuiltinContext context)
	{
		BuiltinType boolType = context.AddType<bool, Bool>("bool", b => new(b));
		BuiltinType textType = context.AddType<string, Text>("text", b => new(b));
		BuiltinType intType = context.AddType<object, Int>("int", b => new(b));

		context.AddBinary<Bool, Bool, Bool>(boolType, OperatorKind.LogicalAnd, Bool.LogicalAnd);
		context.AddBinary<Bool, Bool, Bool>(boolType, OperatorKind.LogicalOr, Bool.LogicalOr);
		context.AddBinary<Bool, Bool, Bool>(boolType, OperatorKind.Equal, Bool.Equal);
		context.AddBinary<Bool, Bool, Bool>(boolType, OperatorKind.NotEqual, Bool.NotEqual);

		context.AddBinary<Text, Text, Bool>(textType, OperatorKind.Equal, Text.Equal);
		context.AddBinary<Text, Text, Bool>(textType, OperatorKind.NotEqual, Text.NotEqual);
		context.AddBinary<Text, Text, Text>(textType, OperatorKind.Add, Text.Add);

		context.AddBinary<Int, Int, Bool>(intType, OperatorKind.Equal, Int.Equal);
		context.AddBinary<Int, Int, Bool>(intType, OperatorKind.NotEqual, Int.NotEqual);
		context.AddBinary<Int, Int, Bool>(intType, OperatorKind.LessThan, Int.LessThan);
		context.AddBinary<Int, Int, Bool>(intType, OperatorKind.GreaterThan, Int.GreaterThan);
		context.AddBinary<Int, Int, Bool>(intType, OperatorKind.LessThanOrEqual, Int.LessThanOrEqual);
		context.AddBinary<Int, Int, Bool>(intType, OperatorKind.GreaterThanOrEqual, Int.GreaterThanOrEqual);

		context.AddBinary<Int, Int, Int>(intType, OperatorKind.Add, Int.Add);
		context.AddBinary<Int, Int, Int>(intType, OperatorKind.Subtract, Int.Subtract);
		context.AddBinary<Int, Int, Int>(intType, OperatorKind.Multiply, Int.Multiply);
		context.AddBinary<Int, Int, Int>(intType, OperatorKind.Divide, Int.Divide);
		context.AddBinary<Int, Int, Int>(intType, OperatorKind.Modulo, Int.Modulo);
	}
	#endregion
}
