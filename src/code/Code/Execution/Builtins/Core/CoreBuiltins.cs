using System.Text;
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
		public Int(long value) => Value = value;
		public Int(ulong value) => Value = value;
		#endregion

		#region Methods
		[Ignore]
		public override string ToString() => Value.ToString() ?? "0";
		#endregion

		#region Operators
		[Ignore] private static int Compare(Int left, Int right) => ((IComparable)left.Value).CompareTo(right.Value);

		[Ignore]
		private static int Compare(Int left, Num right)
		{
			if (left.Value is long l)
				return l.CompareTo(l);

			return ((long)left.Value).CompareTo(right.Value);
		}

		[Operator] public static Bool Equal(Int left, Int right) => new(Compare(left, right) == 0);
		[Operator] public static Bool NotEqual(Int left, Int right) => new(Compare(left, right) != 0);
		[Operator] public static Bool LessThan(Int left, Int right) => new(Compare(left, right) < 0);
		[Operator] public static Bool LessThanOrEqual(Int left, Int right) => new(Compare(left, right) <= 0);
		[Operator] public static Bool GreaterThan(Int left, Int right) => new(Compare(left, right) > 0);
		[Operator] public static Bool GreaterThanOrEqual(Int left, Int right) => new(Compare(left, right) >= 0);

		[Operator] public static Bool Equal(Int left, Num right) => new(Compare(left, right) == 0);
		[Operator] public static Bool NotEqual(Int left, Num right) => new(Compare(left, right) != 0);
		[Operator] public static Bool LessThan(Int left, Num right) => new(Compare(left, right) < 0);
		[Operator] public static Bool LessThanOrEqual(Int left, Num right) => new(Compare(left, right) <= 0);
		[Operator] public static Bool GreaterThan(Int left, Num right) => new(Compare(left, right) > 0);
		[Operator] public static Bool GreaterThanOrEqual(Int left, Num right) => new(Compare(left, right) >= 0);

		[Operator] public static Num Add(Int left, Num right) => new((long)left.Value + right.Value);
		[Operator] public static Num Subtract(Int left, Num right) => new((long)left.Value - right.Value);
		[Operator] public static Num Multiply(Int left, Num right) => new((long)left.Value * right.Value);
		[Operator] public static Num Divide(Int left, Num right) => new((long)left.Value / right.Value);
		[Operator] public static Num Modulo(Int left, Num right) => new((long)left.Value % right.Value);

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
		public static Num Divide(Int left, Int right)
		{
			long l = (long)left.Value;
			long r = (long)right.Value;

			return new((decimal)l / r);
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

	[Name("num")]
	public sealed class Num
	{
		#region Properties
		[Ignore] public decimal Value { get; }
		#endregion

		#region Constructors
		public Num(decimal value) => Value = value;
		#endregion

		#region Methods
		[Ignore]
		public override string ToString() => Value.ToString("G29");
		#endregion

		#region Operators
		[Ignore] private static int Compare(Num left, Num right) => left.Value.CompareTo(right.Value);
		[Ignore]
		private static int Compare(Num left, Int right)
		{
			if (right.Value is long l)
				return left.Value.CompareTo(l);

			return left.Value.CompareTo((ulong)right.Value);
		}

		[Operator] public static Bool Equal(Num left, Num right) => new(Compare(left, right) == 0);
		[Operator] public static Bool NotEqual(Num left, Num right) => new(Compare(left, right) != 0);
		[Operator] public static Bool LessThan(Num left, Num right) => new(Compare(left, right) < 0);
		[Operator] public static Bool LessThanOrEqual(Num left, Num right) => new(Compare(left, right) <= 0);
		[Operator] public static Bool GreaterThan(Num left, Num right) => new(Compare(left, right) > 0);
		[Operator] public static Bool GreaterThanOrEqual(Num left, Num right) => new(Compare(left, right) >= 0);

		[Operator] public static Bool Equal(Num left, Int right) => new(Compare(left, right) == 0);
		[Operator] public static Bool NotEqual(Num left, Int right) => new(Compare(left, right) != 0);
		[Operator] public static Bool LessThan(Num left, Int right) => new(Compare(left, right) < 0);
		[Operator] public static Bool LessThanOrEqual(Num left, Int right) => new(Compare(left, right) <= 0);
		[Operator] public static Bool GreaterThan(Num left, Int right) => new(Compare(left, right) > 0);
		[Operator] public static Bool GreaterThanOrEqual(Num left, Int right) => new(Compare(left, right) >= 0);

		[Operator] public static Num Add(Num left, Num right) => new(left.Value + right.Value);
		[Operator] public static Num Subtract(Num left, Num right) => new(left.Value - right.Value);
		[Operator] public static Num Multiply(Num left, Num right) => new(left.Value * right.Value);
		[Operator] public static Num Divide(Num left, Num right) => new(left.Value / right.Value);
		[Operator] public static Num Modulo(Num left, Num right) => new(left.Value % right.Value);

		[Operator] public static Num Add(Num left, Int right) => new(left.Value + (long)right.Value);
		[Operator] public static Num Subtract(Num left, Int right) => new(left.Value - (long)right.Value);
		[Operator] public static Num Multiply(Num left, Int right) => new(left.Value * (long)right.Value);
		[Operator] public static Num Divide(Num left, Int right) => new(left.Value / (long)right.Value);
		[Operator] public static Num Modulo(Num left, Int right) => new(left.Value % (long)right.Value);
		#endregion
	}

	#region Conversion functions
	[Name("toInt")] public static Int ToInt(Num value) => new((long)value.Value);
	[Name("toInt")] public static Int ToInt(Bool value) => new(value.Value ? 1 : 0);

	[Name("toNum")] public static Num ToNum(Int value) => new((long)value.Value);
	[Name("toNum")] public static Num ToNum(Bool value) => new(value.Value ? 1 : 0);
	#endregion

	#region Parse functions
	[Name("toInt")] public static Int ToInt(Text value) => new(long.Parse(value.Value));
	[Name("toNum")] public static Num ToNum(Text value) => new(decimal.Parse(value.Value));
	[Name("toBool")] public static Bool ToBool(Text value) => new(bool.Parse(value.Value));
	#endregion

	#region String functions
	[Name("getLength")]
	public static Int GetLength(Text value)
	{
		long count = value.Value.EnumerateTextElements().Count();
		return new(count);
	}

	[Name("getAt")]
	public static Text GetAt(Text value, Int index)
	{
		TextElement element = value.Value.EnumerateTextElements().ElementAt((int)(long)index.Value);
		return new(element.Value);
	}

	[Name("getPart")]
	public static Text GetPart(Text value, Int index, Int amount)
	{
		StringBuilder builder = new();

		IEnumerable<TextElement> elements = value.Value
			.EnumerateTextElements()
			.Skip((int)index.Value)
			.Take((int)amount.Value);

		foreach (TextElement element in elements)
			builder.Append(element.Value);

		string result = builder.ToString();
		return new(result);
	}
	#endregion

	#region Resolve functions
	[Ignore]
	public static void Resolve(BuiltinContext context)
	{
		BuiltinType boolType = context.AddType<bool, Bool>("bool", b => new(b));
		BuiltinType textType = context.AddType<string, Text>("text", b => new(b));
		BuiltinType intType = context.AddType<object, Int>("int", b => b is long l ? new(l) : new((ulong)b));
		BuiltinType numType = context.AddType<decimal, Num>("num", b => new(b));

		#region Conversion functions
		context.AddFunction<Num, Int>("toInt", "value", ToInt);
		context.AddFunction<Bool, Int>("toInt", "value", ToInt);

		context.AddFunction<Int, Num>("toNum", "value", ToNum);
		context.AddFunction<Bool, Num>("toNum", "value", ToNum);
		#endregion

		#region Parse functions
		context.AddFunction<Text, Int>("toInt", "value", ToInt);
		context.AddFunction<Text, Num>("toNum", "value", ToNum);
		context.AddFunction<Text, Bool>("toBool", "value", ToBool);
		#endregion

		#region String functions
		context.AddFunction<Text, Int>("getLength", "value", GetLength);
		context.AddFunction<Text, Int, Text>("getAt", "value", "index", GetAt);
		context.AddFunction<Text, Int, Int, Text>("getPart", "value", "index", "amount", GetPart);
		#endregion

		#region Bool
		context.AddBinary<Bool, Bool, Bool>(boolType, OperatorKind.LogicalAnd, Bool.LogicalAnd);
		context.AddBinary<Bool, Bool, Bool>(boolType, OperatorKind.LogicalOr, Bool.LogicalOr);
		context.AddBinary<Bool, Bool, Bool>(boolType, OperatorKind.Equal, Bool.Equal);
		context.AddBinary<Bool, Bool, Bool>(boolType, OperatorKind.NotEqual, Bool.NotEqual);
		#endregion

		#region Text
		context.AddBinary<Text, Text, Bool>(textType, OperatorKind.Equal, Text.Equal);
		context.AddBinary<Text, Text, Bool>(textType, OperatorKind.NotEqual, Text.NotEqual);
		context.AddBinary<Text, Text, Text>(textType, OperatorKind.Add, Text.Add);
		#endregion

		#region Int
		context.AddBinary<Int, Int, Bool>(intType, OperatorKind.Equal, Int.Equal);
		context.AddBinary<Int, Int, Bool>(intType, OperatorKind.NotEqual, Int.NotEqual);
		context.AddBinary<Int, Int, Bool>(intType, OperatorKind.LessThan, Int.LessThan);
		context.AddBinary<Int, Int, Bool>(intType, OperatorKind.GreaterThan, Int.GreaterThan);
		context.AddBinary<Int, Int, Bool>(intType, OperatorKind.LessThanOrEqual, Int.LessThanOrEqual);
		context.AddBinary<Int, Int, Bool>(intType, OperatorKind.GreaterThanOrEqual, Int.GreaterThanOrEqual);

		context.AddBinary<Int, Int, Int>(intType, OperatorKind.Add, Int.Add);
		context.AddBinary<Int, Int, Int>(intType, OperatorKind.Subtract, Int.Subtract);
		context.AddBinary<Int, Int, Int>(intType, OperatorKind.Multiply, Int.Multiply);
		context.AddBinary<Int, Int, Num>(intType, OperatorKind.Divide, Int.Divide);
		context.AddBinary<Int, Int, Int>(intType, OperatorKind.Modulo, Int.Modulo);
		#endregion

		#region Num
		context.AddBinary<Num, Num, Bool>(numType, OperatorKind.Equal, Num.Equal);
		context.AddBinary<Num, Num, Bool>(numType, OperatorKind.NotEqual, Num.NotEqual);
		context.AddBinary<Num, Num, Bool>(numType, OperatorKind.LessThan, Num.LessThan);
		context.AddBinary<Num, Num, Bool>(numType, OperatorKind.GreaterThan, Num.GreaterThan);
		context.AddBinary<Num, Num, Bool>(numType, OperatorKind.LessThanOrEqual, Num.LessThanOrEqual);
		context.AddBinary<Num, Num, Bool>(numType, OperatorKind.GreaterThanOrEqual, Num.GreaterThanOrEqual);

		context.AddBinary<Num, Num, Num>(numType, OperatorKind.Add, Num.Add);
		context.AddBinary<Num, Num, Num>(numType, OperatorKind.Subtract, Num.Subtract);
		context.AddBinary<Num, Num, Num>(numType, OperatorKind.Multiply, Num.Multiply);
		context.AddBinary<Num, Num, Num>(numType, OperatorKind.Divide, Num.Divide);
		context.AddBinary<Num, Num, Num>(numType, OperatorKind.Modulo, Num.Modulo);
		#endregion

		#region Int - Num
		context.AddBinary<Int, Num, Bool>(intType, OperatorKind.Equal, Int.Equal);
		context.AddBinary<Int, Num, Bool>(intType, OperatorKind.NotEqual, Int.NotEqual);
		context.AddBinary<Int, Num, Bool>(intType, OperatorKind.LessThan, Int.LessThan);
		context.AddBinary<Int, Num, Bool>(intType, OperatorKind.GreaterThan, Int.GreaterThan);
		context.AddBinary<Int, Num, Bool>(intType, OperatorKind.LessThanOrEqual, Int.LessThanOrEqual);
		context.AddBinary<Int, Num, Bool>(intType, OperatorKind.GreaterThanOrEqual, Int.GreaterThanOrEqual);

		context.AddBinary<Int, Num, Num>(intType, OperatorKind.Add, Int.Add);
		context.AddBinary<Int, Num, Num>(intType, OperatorKind.Subtract, Int.Subtract);
		context.AddBinary<Int, Num, Num>(intType, OperatorKind.Multiply, Int.Multiply);
		context.AddBinary<Int, Num, Num>(intType, OperatorKind.Divide, Int.Divide);
		context.AddBinary<Int, Num, Num>(intType, OperatorKind.Modulo, Int.Modulo);
		#endregion

		#region Num - Int
		context.AddBinary<Num, Int, Bool>(numType, OperatorKind.Equal, Num.Equal);
		context.AddBinary<Num, Int, Bool>(numType, OperatorKind.NotEqual, Num.NotEqual);
		context.AddBinary<Num, Int, Bool>(numType, OperatorKind.LessThan, Num.LessThan);
		context.AddBinary<Num, Int, Bool>(numType, OperatorKind.GreaterThan, Num.GreaterThan);
		context.AddBinary<Num, Int, Bool>(numType, OperatorKind.LessThanOrEqual, Num.LessThanOrEqual);
		context.AddBinary<Num, Int, Bool>(numType, OperatorKind.GreaterThanOrEqual, Num.GreaterThanOrEqual);

		context.AddBinary<Num, Int, Num>(numType, OperatorKind.Add, Num.Add);
		context.AddBinary<Num, Int, Num>(numType, OperatorKind.Subtract, Num.Subtract);
		context.AddBinary<Num, Int, Num>(numType, OperatorKind.Multiply, Num.Multiply);
		context.AddBinary<Num, Int, Num>(numType, OperatorKind.Divide, Num.Divide);
		context.AddBinary<Num, Int, Num>(numType, OperatorKind.Modulo, Num.Modulo);
		#endregion
	}
	#endregion
}
