using System.Text;
using OwlDomain.Owl.Code.Execution.Builtins.Attributes;

namespace OwlDomain.Owl.Code.Execution.Builtins.Core;

internal partial class CoreBuiltins
{
	#region Core types
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

		#region Conversion methods
		[Method("toInt")] public static Int ToInt(Bool value) => new(value.Value ? 1 : 0);
		[Method("toUInt")] public static UInt ToUInt(Bool value) => new(value.Value ? 1u : 0);
		#endregion

		#region Methods
		[Method("negate")] public static Bool Negate(Bool value) => new(value.Value is false);
		#endregion
	}

	[Name("text")]
	public sealed class Text
	{
		#region Properties
		[Ignore] public string Value { get; }
		[Property("Length")] public Int Length => new(Value.EnumerateTextElements().Count());
		[Property("IsEmpty")] public Bool IsEmpty => new(Value.Length is 0);
		[Property("IsNotEmpty")] public Bool IsNotEmpty => new(Value.Length is not 0);
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

		#region Conversion methods
		[Method("parseBool")] public static Bool ParseBool(Text value) => new(bool.Parse(value.Value));
		[Method("parseInt")] public static Int ParseInt(Text value) => new(long.Parse(value.Value));
		[Method("parseUInt")] public static UInt ParseUInt(Text value) => new(ulong.Parse(value.Value));
		[Method("parseNum")] public static Num ParseNum(Text value) => new(decimal.Parse(value.Value));
		#endregion

		#region Methods
		[Method("getAt")]
		public static Text GetAt(Text value, Int index)
		{
			TextElement element = value.Value.EnumerateTextElements().ElementAt((int)index.Value);
			return new(element.Value);
		}

		[Method("getPart")]
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

		[Method("reverse")]
		public static Text Reverse(Text value)
		{
			char[] ch = value.Value.ToCharArray();
			ch.Reverse();

			return new(new string(ch));
		}
		#endregion
	}

	[Name("int")]
	public sealed class Int
	{
		#region Properties
		[Ignore] public long Value { get; }
		#endregion

		#region Constructors
		public Int(long value) => Value = value;
		#endregion

		#region Methods
		[Ignore] public override string ToString() => Value.ToString();
		#endregion

		#region Comparison operators
		[Ignore] private static int Compare(Int left, Int right) => left.Value.CompareTo(right.Value);
		[Ignore] private static int Compare(Int left, UInt right) => left.Value.CompareTo(right.Value);
		[Ignore] private static int Compare(Int left, Num right) => left.Value.CompareTo(right.Value);

		[Operator] public static Bool Equal(Int left, Int right) => new(Compare(left, right) is 0);
		[Operator] public static Bool NotEqual(Int left, Int right) => new(Compare(left, right) is not 0);
		[Operator] public static Bool LessThan(Int left, Int right) => new(Compare(left, right) < 0);
		[Operator] public static Bool LessThanOrEqual(Int left, Int right) => new(Compare(left, right) <= 0);
		[Operator] public static Bool GreaterThan(Int left, Int right) => new(Compare(left, right) > 0);
		[Operator] public static Bool GreaterThanOrEqual(Int left, Int right) => new(Compare(left, right) >= 0);

		[Operator] public static Bool Equal(Int left, UInt right) => new(Compare(left, right) is 0);
		[Operator] public static Bool NotEqual(Int left, UInt right) => new(Compare(left, right) is not 0);
		[Operator] public static Bool LessThan(Int left, UInt right) => new(Compare(left, right) < 0);
		[Operator] public static Bool LessThanOrEqual(Int left, UInt right) => new(Compare(left, right) <= 0);
		[Operator] public static Bool GreaterThan(Int left, UInt right) => new(Compare(left, right) > 0);
		[Operator] public static Bool GreaterThanOrEqual(Int left, UInt right) => new(Compare(left, right) >= 0);

		[Operator] public static Bool Equal(Int left, Num right) => new(Compare(left, right) is 0);
		[Operator] public static Bool NotEqual(Int left, Num right) => new(Compare(left, right) is not 0);
		[Operator] public static Bool LessThan(Int left, Num right) => new(Compare(left, right) < 0);
		[Operator] public static Bool LessThanOrEqual(Int left, Num right) => new(Compare(left, right) <= 0);
		[Operator] public static Bool GreaterThan(Int left, Num right) => new(Compare(left, right) > 0);
		[Operator] public static Bool GreaterThanOrEqual(Int left, Num right) => new(Compare(left, right) >= 0);
		#endregion

		#region Math operations
		[Operator] public static Int Add(Int left, Int right) => new(left.Value + right.Value);
		[Operator] public static Int Subtract(Int left, Int right) => new(left.Value - right.Value);
		[Operator] public static Int Multiply(Int left, Int right) => new(left.Value * right.Value);
		[Operator] public static Int Divide(Int left, Int right) => new(left.Value / right.Value);
		[Operator] public static Int Modulo(Int left, Int right) => new(left.Value % right.Value);
		#endregion

		#region Conversion methods
		[Method("toUInt")] public static UInt ToUInt(Int value) => new((ulong)value.Value);
		[Method("toNum")] public static Num ToNum(Int value) => new(value.Value);
		#endregion

		#region Properties
		[Property("IsZero")] public static Bool IsZero(Int value) => new(value.Value is 0);
		[Property("IsOdd")] public static Bool IsOdd(Int value) => new(value.Value % 2 is 1);
		[Property("IsEven")] public static Bool IsEven(Int value) => new(value.Value % 2 is 0);
		[Property("Sign")] public static Int Sign(Int num) => new(long.Sign(num.Value));
		#endregion

		#region Methods
		[Method("abs")] public static Int Abs(Int num) => new(long.Abs(num.Value));
		#endregion
	}

	[Name("uint")]
	public sealed class UInt
	{
		#region Properties
		[Ignore] public ulong Value { get; }
		#endregion

		#region Constructors
		public UInt(ulong value) => Value = value;
		#endregion

		#region Methods
		[Ignore] public override string ToString() => Value.ToString();
		#endregion

		#region Comparison operators
		[Ignore] private static int Compare(UInt left, UInt right) => left.Value.CompareTo(right.Value);
		[Ignore] private static int Compare(UInt left, Int right) => left.Value.CompareTo(right.Value);
		[Ignore] private static int Compare(UInt left, Num right) => left.Value.CompareTo(right.Value);

		[Operator] public static Bool Equal(UInt left, UInt right) => new(Compare(left, right) is 0);
		[Operator] public static Bool NotEqual(UInt left, UInt right) => new(Compare(left, right) is not 0);
		[Operator] public static Bool LessThan(UInt left, UInt right) => new(Compare(left, right) < 0);
		[Operator] public static Bool LessThanOrEqual(UInt left, UInt right) => new(Compare(left, right) <= 0);
		[Operator] public static Bool GreaterThan(UInt left, UInt right) => new(Compare(left, right) > 0);
		[Operator] public static Bool GreaterThanOrEqual(UInt left, UInt right) => new(Compare(left, right) >= 0);

		[Operator] public static Bool Equal(UInt left, Int right) => new(Compare(left, right) is 0);
		[Operator] public static Bool NotEqual(UInt left, Int right) => new(Compare(left, right) is not 0);
		[Operator] public static Bool LessThan(UInt left, Int right) => new(Compare(left, right) < 0);
		[Operator] public static Bool LessThanOrEqual(UInt left, Int right) => new(Compare(left, right) <= 0);
		[Operator] public static Bool GreaterThan(UInt left, Int right) => new(Compare(left, right) > 0);
		[Operator] public static Bool GreaterThanOrEqual(UInt left, Int right) => new(Compare(left, right) >= 0);

		[Operator] public static Bool Equal(UInt left, Num right) => new(Compare(left, right) is 0);
		[Operator] public static Bool NotEqual(UInt left, Num right) => new(Compare(left, right) is not 0);
		[Operator] public static Bool LessThan(UInt left, Num right) => new(Compare(left, right) < 0);
		[Operator] public static Bool LessThanOrEqual(UInt left, Num right) => new(Compare(left, right) <= 0);
		[Operator] public static Bool GreaterThan(UInt left, Num right) => new(Compare(left, right) > 0);
		[Operator] public static Bool GreaterThanOrEqual(UInt left, Num right) => new(Compare(left, right) >= 0);
		#endregion

		#region Math operations
		[Operator] public static UInt Add(UInt left, UInt right) => new(left.Value + right.Value);
		[Operator] public static UInt Subtract(UInt left, UInt right) => new(left.Value - right.Value);
		[Operator] public static UInt Multiply(UInt left, UInt right) => new(left.Value * right.Value);
		[Operator] public static UInt Divide(UInt left, UInt right) => new(left.Value / right.Value);
		[Operator] public static UInt Modulo(UInt left, UInt right) => new(left.Value % right.Value);
		#endregion

		#region Conversion methods
		[Method("toUInt")] public static Int ToInt(UInt value) => new((long)value.Value);
		[Method("toNum")] public static Num ToNum(UInt value) => new(value.Value);
		#endregion

		#region Properties
		[Property("IsZero")] public static Bool IsZero(UInt value) => new(value.Value is 0);
		[Property("IsOdd")] public static Bool IsOdd(UInt value) => new(value.Value % 2 is 1);
		[Property("IsEven")] public static Bool IsEven(UInt value) => new(value.Value % 2 is 0);
		[Property("Sign")] public static Int Sign(UInt num) => new(ulong.Sign(num.Value));
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

		#region Equality operators
		[Ignore] private static int Compare(Num left, Num right) => left.Value.CompareTo(right.Value);
		[Ignore] private static int Compare(Num left, Int right) => left.Value.CompareTo(right.Value);
		[Ignore] private static int Compare(Num left, UInt right) => left.Value.CompareTo(right.Value);

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

		[Operator] public static Bool Equal(Num left, UInt right) => new(Compare(left, right) == 0);
		[Operator] public static Bool NotEqual(Num left, UInt right) => new(Compare(left, right) != 0);
		[Operator] public static Bool LessThan(Num left, UInt right) => new(Compare(left, right) < 0);
		[Operator] public static Bool LessThanOrEqual(Num left, UInt right) => new(Compare(left, right) <= 0);
		[Operator] public static Bool GreaterThan(Num left, UInt right) => new(Compare(left, right) > 0);
		[Operator] public static Bool GreaterThanOrEqual(Num left, UInt right) => new(Compare(left, right) >= 0);
		#endregion

		#region Math operators
		[Operator] public static Num Add(Num left, Num right) => new(left.Value + right.Value);
		[Operator] public static Num Subtract(Num left, Num right) => new(left.Value - right.Value);
		[Operator] public static Num Multiply(Num left, Num right) => new(left.Value * right.Value);
		[Operator] public static Num Divide(Num left, Num right) => new(left.Value / right.Value);
		[Operator] public static Num Modulo(Num left, Num right) => new(left.Value % right.Value);

		[Operator] public static Num Add(Num left, Int right) => new(left.Value + right.Value);
		[Operator] public static Num Subtract(Num left, Int right) => new(left.Value - right.Value);
		[Operator] public static Num Multiply(Num left, Int right) => new(left.Value * right.Value);
		[Operator] public static Num Divide(Num left, Int right) => new(left.Value / right.Value);
		[Operator] public static Num Modulo(Num left, Int right) => new(left.Value % right.Value);

		[Operator] public static Num Add(Num left, UInt right) => new(left.Value + right.Value);
		[Operator] public static Num Subtract(Num left, UInt right) => new(left.Value - right.Value);
		[Operator] public static Num Multiply(Num left, UInt right) => new(left.Value * right.Value);
		[Operator] public static Num Divide(Num left, UInt right) => new(left.Value / right.Value);
		[Operator] public static Num Modulo(Num left, UInt right) => new(left.Value % right.Value);
		#endregion

		#region Conversion methods
		[Method("toInt")] public static Int ToInt(Num num) => new((long)num.Value);
		[Method("toUInt")] public static UInt ToUInt(Num num) => new((ulong)num.Value);
		#endregion

		#region Properties
		[Property("Sign")] public static Int Sign(Num num) => new(decimal.Sign(num.Value));
		#endregion

		#region Methods
		[Method("abs")] public static Num Abs(Num num) => new(decimal.Abs(num.Value));
		#endregion
	}
	#endregion

	#region Resolve functions
	[Ignore]
	public static void Resolve(BuiltinContext context)
	{
		context.AddType<bool, Bool>("bool", b => new(b));
		context.AddType<string, Text>("text", b => new(b));
		context.AddType<long, Int>("int", b => new(b));
		context.AddType<ulong, UInt>("uint", b => new(b));
		context.AddType<decimal, Num>("num", b => new(b));

		ResolveBool(context);
		ResolveInt(context);
		ResolveUInt(context);
		ResolveNum(context);
		ResolveText(context);
	}

	private static void ResolveBool(BuiltinContext context)
	{
		BuiltinType type = context[typeof(Bool)];

		#region Operators
		context.AddBinary<Bool, Bool, Bool>(type, OperatorKind.LogicalAnd, Bool.LogicalAnd);
		context.AddBinary<Bool, Bool, Bool>(type, OperatorKind.LogicalOr, Bool.LogicalOr);
		context.AddBinary<Bool, Bool, Bool>(type, OperatorKind.Equal, Bool.Equal);
		context.AddBinary<Bool, Bool, Bool>(type, OperatorKind.NotEqual, Bool.NotEqual);
		#endregion

		#region Methods
		context.AddMethod<Bool, Bool>("negate", Bool.Negate);
		#endregion

		#region Conversion methods
		context.AddMethod<Bool, Int>("toInt", Bool.ToInt);
		context.AddMethod<Bool, UInt>("toUInt", Bool.ToUInt);
		#endregion
	}
	private static void ResolveInt(BuiltinContext context)
	{
		BuiltinType type = context[typeof(Int)];

		#region Equality operators
		context.AddBinary<Int, Int, Bool>(type, OperatorKind.Equal, Int.Equal);
		context.AddBinary<Int, Int, Bool>(type, OperatorKind.NotEqual, Int.NotEqual);
		context.AddBinary<Int, Int, Bool>(type, OperatorKind.LessThan, Int.LessThan);
		context.AddBinary<Int, Int, Bool>(type, OperatorKind.LessThanOrEqual, Int.LessThanOrEqual);
		context.AddBinary<Int, Int, Bool>(type, OperatorKind.GreaterThan, Int.GreaterThan);
		context.AddBinary<Int, Int, Bool>(type, OperatorKind.GreaterThanOrEqual, Int.GreaterThanOrEqual);

		context.AddBinary<Int, UInt, Bool>(type, OperatorKind.Equal, Int.Equal);
		context.AddBinary<Int, UInt, Bool>(type, OperatorKind.NotEqual, Int.NotEqual);
		context.AddBinary<Int, UInt, Bool>(type, OperatorKind.LessThan, Int.LessThan);
		context.AddBinary<Int, UInt, Bool>(type, OperatorKind.LessThanOrEqual, Int.LessThanOrEqual);
		context.AddBinary<Int, UInt, Bool>(type, OperatorKind.GreaterThan, Int.GreaterThan);
		context.AddBinary<Int, UInt, Bool>(type, OperatorKind.GreaterThanOrEqual, Int.GreaterThanOrEqual);

		context.AddBinary<Int, Num, Bool>(type, OperatorKind.Equal, Int.Equal);
		context.AddBinary<Int, Num, Bool>(type, OperatorKind.NotEqual, Int.NotEqual);
		context.AddBinary<Int, Num, Bool>(type, OperatorKind.LessThan, Int.LessThan);
		context.AddBinary<Int, Num, Bool>(type, OperatorKind.LessThanOrEqual, Int.LessThanOrEqual);
		context.AddBinary<Int, Num, Bool>(type, OperatorKind.GreaterThan, Int.GreaterThan);
		context.AddBinary<Int, Num, Bool>(type, OperatorKind.GreaterThanOrEqual, Int.GreaterThanOrEqual);
		#endregion

		#region Math operators
		context.AddBinary<Int, Int, Int>(type, OperatorKind.Add, Int.Add);
		context.AddBinary<Int, Int, Int>(type, OperatorKind.Subtract, Int.Subtract);
		context.AddBinary<Int, Int, Int>(type, OperatorKind.Multiply, Int.Multiply);
		context.AddBinary<Int, Int, Int>(type, OperatorKind.Divide, Int.Divide);
		context.AddBinary<Int, Int, Int>(type, OperatorKind.Modulo, Int.Modulo);
		#endregion

		#region Conversion methods
		context.AddMethod<Int, UInt>("toUInt", Int.ToUInt);
		context.AddMethod<Int, Num>("toNum", Int.ToNum);
		#endregion

		#region Properties
		context.AddProperty<Int, Bool>("IsZero", Int.IsZero);
		context.AddProperty<Int, Bool>("IsOdd", Int.IsOdd);
		context.AddProperty<Int, Bool>("IsEven", Int.IsEven);
		context.AddProperty<Int, Int>("Sign", Int.Sign);
		#endregion

		#region Methods
		context.AddMethod<Int, Int>("abs", Int.Abs);
		#endregion
	}
	private static void ResolveUInt(BuiltinContext context)
	{
		BuiltinType type = context[typeof(UInt)];

		#region Equality operators
		context.AddBinary<UInt, UInt, Bool>(type, OperatorKind.Equal, UInt.Equal);
		context.AddBinary<UInt, UInt, Bool>(type, OperatorKind.NotEqual, UInt.NotEqual);
		context.AddBinary<UInt, UInt, Bool>(type, OperatorKind.LessThan, UInt.LessThan);
		context.AddBinary<UInt, UInt, Bool>(type, OperatorKind.LessThanOrEqual, UInt.LessThanOrEqual);
		context.AddBinary<UInt, UInt, Bool>(type, OperatorKind.GreaterThan, UInt.GreaterThan);
		context.AddBinary<UInt, UInt, Bool>(type, OperatorKind.GreaterThanOrEqual, UInt.GreaterThanOrEqual);

		context.AddBinary<UInt, Int, Bool>(type, OperatorKind.Equal, UInt.Equal);
		context.AddBinary<UInt, Int, Bool>(type, OperatorKind.NotEqual, UInt.NotEqual);
		context.AddBinary<UInt, Int, Bool>(type, OperatorKind.LessThan, UInt.LessThan);
		context.AddBinary<UInt, Int, Bool>(type, OperatorKind.LessThanOrEqual, UInt.LessThanOrEqual);
		context.AddBinary<UInt, Int, Bool>(type, OperatorKind.GreaterThan, UInt.GreaterThan);
		context.AddBinary<UInt, Int, Bool>(type, OperatorKind.GreaterThanOrEqual, UInt.GreaterThanOrEqual);

		context.AddBinary<UInt, Num, Bool>(type, OperatorKind.Equal, UInt.Equal);
		context.AddBinary<UInt, Num, Bool>(type, OperatorKind.NotEqual, UInt.NotEqual);
		context.AddBinary<UInt, Num, Bool>(type, OperatorKind.LessThan, UInt.LessThan);
		context.AddBinary<UInt, Num, Bool>(type, OperatorKind.LessThanOrEqual, UInt.LessThanOrEqual);
		context.AddBinary<UInt, Num, Bool>(type, OperatorKind.GreaterThan, UInt.GreaterThan);
		context.AddBinary<UInt, Num, Bool>(type, OperatorKind.GreaterThanOrEqual, UInt.GreaterThanOrEqual);
		#endregion

		#region Math operators
		context.AddBinary<UInt, UInt, UInt>(type, OperatorKind.Add, UInt.Add);
		context.AddBinary<UInt, UInt, UInt>(type, OperatorKind.Subtract, UInt.Subtract);
		context.AddBinary<UInt, UInt, UInt>(type, OperatorKind.Multiply, UInt.Multiply);
		context.AddBinary<UInt, UInt, UInt>(type, OperatorKind.Divide, UInt.Divide);
		context.AddBinary<UInt, UInt, UInt>(type, OperatorKind.Modulo, UInt.Modulo);
		#endregion

		#region Conversion methods
		context.AddMethod<UInt, Int>("toInt", UInt.ToInt);
		context.AddMethod<UInt, Num>("toNum", UInt.ToNum);
		#endregion

		#region Properties
		context.AddProperty<UInt, Bool>("IsZero", UInt.IsZero);
		context.AddProperty<UInt, Bool>("IsOdd", UInt.IsOdd);
		context.AddProperty<UInt, Bool>("IsEven", UInt.IsEven);
		context.AddProperty<UInt, Int>("Sign", UInt.Sign);
		#endregion
	}
	private static void ResolveNum(BuiltinContext context)
	{
		BuiltinType type = context[typeof(Num)];

		#region Equality operators
		context.AddBinary<Num, Num, Bool>(type, OperatorKind.Equal, Num.Equal);
		context.AddBinary<Num, Num, Bool>(type, OperatorKind.NotEqual, Num.NotEqual);
		context.AddBinary<Num, Num, Bool>(type, OperatorKind.LessThan, Num.LessThan);
		context.AddBinary<Num, Num, Bool>(type, OperatorKind.LessThanOrEqual, Num.LessThanOrEqual);
		context.AddBinary<Num, Num, Bool>(type, OperatorKind.GreaterThan, Num.GreaterThan);
		context.AddBinary<Num, Num, Bool>(type, OperatorKind.GreaterThanOrEqual, Num.GreaterThanOrEqual);

		context.AddBinary<Num, Int, Bool>(type, OperatorKind.Equal, Num.Equal);
		context.AddBinary<Num, Int, Bool>(type, OperatorKind.NotEqual, Num.NotEqual);
		context.AddBinary<Num, Int, Bool>(type, OperatorKind.LessThan, Num.LessThan);
		context.AddBinary<Num, Int, Bool>(type, OperatorKind.LessThanOrEqual, Num.LessThanOrEqual);
		context.AddBinary<Num, Int, Bool>(type, OperatorKind.GreaterThan, Num.GreaterThan);
		context.AddBinary<Num, Int, Bool>(type, OperatorKind.GreaterThanOrEqual, Num.GreaterThanOrEqual);

		context.AddBinary<Num, UInt, Bool>(type, OperatorKind.Equal, Num.Equal);
		context.AddBinary<Num, UInt, Bool>(type, OperatorKind.NotEqual, Num.NotEqual);
		context.AddBinary<Num, UInt, Bool>(type, OperatorKind.LessThan, Num.LessThan);
		context.AddBinary<Num, UInt, Bool>(type, OperatorKind.LessThanOrEqual, Num.LessThanOrEqual);
		context.AddBinary<Num, UInt, Bool>(type, OperatorKind.GreaterThan, Num.GreaterThan);
		context.AddBinary<Num, UInt, Bool>(type, OperatorKind.GreaterThanOrEqual, Num.GreaterThanOrEqual);
		#endregion

		#region Math operators
		context.AddBinary<Num, Num, Num>(type, OperatorKind.Add, Num.Add);
		context.AddBinary<Num, Num, Num>(type, OperatorKind.Subtract, Num.Subtract);
		context.AddBinary<Num, Num, Num>(type, OperatorKind.Multiply, Num.Multiply);
		context.AddBinary<Num, Num, Num>(type, OperatorKind.Divide, Num.Divide);
		context.AddBinary<Num, Num, Num>(type, OperatorKind.Modulo, Num.Modulo);

		context.AddBinary<Num, Int, Num>(type, OperatorKind.Add, Num.Add);
		context.AddBinary<Num, Int, Num>(type, OperatorKind.Subtract, Num.Subtract);
		context.AddBinary<Num, Int, Num>(type, OperatorKind.Multiply, Num.Multiply);
		context.AddBinary<Num, Int, Num>(type, OperatorKind.Divide, Num.Divide);
		context.AddBinary<Num, Int, Num>(type, OperatorKind.Modulo, Num.Modulo);

		context.AddBinary<Num, UInt, Num>(type, OperatorKind.Add, Num.Add);
		context.AddBinary<Num, UInt, Num>(type, OperatorKind.Subtract, Num.Subtract);
		context.AddBinary<Num, UInt, Num>(type, OperatorKind.Multiply, Num.Multiply);
		context.AddBinary<Num, UInt, Num>(type, OperatorKind.Divide, Num.Divide);
		context.AddBinary<Num, UInt, Num>(type, OperatorKind.Modulo, Num.Modulo);
		#endregion

		#region Conversion methods
		context.AddMethod<Num, Int>("toInt", Num.ToInt);
		context.AddMethod<Num, UInt>("toUInt", Num.ToUInt);
		#endregion

		#region Properties
		context.AddProperty<Num, Int>("Sign", Num.Sign);
		#endregion

		#region Methods
		context.AddMethod<Num, Num>("abs", Num.Abs);
		#endregion
	}
	private static void ResolveText(BuiltinContext context)
	{
		BuiltinType type = context[typeof(Text)];

		#region Properties
		context.AddProperty("Length", static (Text text) => text.Length);
		context.AddProperty("IsEmpty", static (Text text) => text.IsEmpty);
		context.AddProperty("IsNotEmpty", static (Text text) => text.IsNotEmpty);
		#endregion

		#region Methods
		context.AddMethod<Text, Int, Text>("getAt", "index", Text.GetAt);
		context.AddMethod<Text, Int, Int, Text>("getPart", "index", "amount", Text.GetPart);
		context.AddMethod<Text, Text>("reverse", Text.Reverse);
		#endregion

		#region Operators
		context.AddBinary<Text, Text, Bool>(type, OperatorKind.Equal, Text.Equal);
		context.AddBinary<Text, Text, Bool>(type, OperatorKind.NotEqual, Text.NotEqual);
		context.AddBinary<Text, Text, Text>(type, OperatorKind.Add, Text.Add);
		#endregion

		#region Parsing methods
		context.AddMethod<Text, Bool>("parseBool", Text.ParseBool);
		context.AddMethod<Text, Int>("parseInt", Text.ParseInt);
		context.AddMethod<Text, UInt>("parseUInt", Text.ParseUInt);
		context.AddMethod<Text, Num>("parseNum", Text.ParseNum);
		#endregion
	}
	#endregion
}
