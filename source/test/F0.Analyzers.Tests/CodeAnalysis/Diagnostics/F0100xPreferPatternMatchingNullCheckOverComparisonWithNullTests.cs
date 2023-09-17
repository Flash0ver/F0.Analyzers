using System.ComponentModel;
using F0.CodeAnalysis.Diagnostics;
using F0.Testing.CodeAnalysis;

namespace F0.Tests.CodeAnalysis.Diagnostics;

public class F0100xPreferPatternMatchingNullCheckOverComparisonWithNullTests
{
	[Fact]
	public void F0100xPreferPatternMatchingNullCheckOverComparisonWithNull_CheckType()
		=> Verify.DiagnosticAnalyzer<F0100xPreferPatternMatchingNullCheckOverComparisonWithNull>().Type();

	[Fact]
	public async Task Initialize_CSharp8_ReportNoDiagnostics()
	{
		var code = """
			using System;
			using System.Collections.Generic;

			class Test
			{
				void Method(Type instance, IEqualityComparer<Type> comparer)
				{
					_ = (object)instance == null;
					_ = (object)instance != null;
					_ = instance == null;
					_ = instance != null;
					_ = Object.Equals(instance, null);
					_ = !Object.Equals(instance, null);
					_ = Object.ReferenceEquals(instance, null);
					_ = !Object.ReferenceEquals(instance, null);
					_ = ((object)instance).Equals(null);
					_ = !((object)instance).Equals(null);
					_ = ((IEquatable<Type>)instance).Equals(null);
					_ = !((IEquatable<Type>)instance).Equals(null);
					_ = instance.Equals(null);
					_ = !instance.Equals(null);
					_ = comparer.Equals(instance, null);
					_ = !comparer.Equals(instance, null);
					_ = ReferenceEqualityComparer.Instance.Equals(instance, null);
					_ = !ReferenceEqualityComparer.Instance.Equals(instance, null);
					_ = EqualityComparer<Type>.Default.Equals(instance, null);
					_ = !EqualityComparer<Type>.Default.Equals(instance, null);
				}
			}
			""";

		await VerifyNoOpAsync(code, LanguageVersion.CSharp8);
	}

	[Fact]
	public async Task Initialize_NullTests_FlagIfNullCheck()
	{
		var type = """
			using System;

			public class UserClass : IEquatable<UserClass>
			{
				public static bool operator ==(UserClass? left, UserClass? right) => throw null;
				public static bool operator !=(UserClass? left, UserClass? right) => throw null;
				bool IEquatable<UserClass>.Equals(UserClass? other) => throw null;
				public bool Equals(UserClass? other) => throw null;
				public override bool Equals(object? obj) => throw null;
				public override int GetHashCode() => throw null;
			}
			""";

		var comparer = """
			using System.Collections.Generic;

			public class UserClassEqualityComparer : IEqualityComparer<UserClass>
			{
				public static IEqualityComparer<UserClass> Instance { get; } = new UserClassEqualityComparer();
				bool IEqualityComparer<UserClass>.Equals(UserClass? x, UserClass? y) => throw null;
				int IEqualityComparer<UserClass>.GetHashCode(UserClass obj) => throw null;
			}
			""";

		var code = """
			using System;
			using System.Collections.Generic;

			class Test
			{
				void Method(UserClass instance, IEqualityComparer<UserClass> comparer)
				{
					_ = instance is null;
					_ = instance is not null;
					_ = {|#0:(object)instance == null|};
					_ = {|#1:(object)instance != null|};
					_ = {|#2:instance == null|};
					_ = {|#3:instance != null|};
					_ = {|#4:Object.Equals(instance, null)|};
					_ = {|#5:!Object.Equals(instance, null)|};
					_ = {|#6:Object.ReferenceEquals(instance, null)|};
					_ = {|#7:!Object.ReferenceEquals(instance, null)|};
					_ = {|#8:((object)instance).Equals(null)|};
					_ = {|#9:!((object)instance).Equals(null)|};
					_ = {|#10:((IEquatable<UserClass>)instance).Equals(null)|};
					_ = {|#11:!((IEquatable<UserClass>)instance).Equals(null)|};
					_ = {|#12:instance.Equals(null)|};
					_ = {|#13:!instance.Equals(null)|};
					_ = {|#14:comparer.Equals(instance, null)|};
					_ = {|#15:!comparer.Equals(instance, null)|};
					_ = {|#16:ReferenceEqualityComparer.Instance.Equals(instance, null)|};
					_ = {|#17:!ReferenceEqualityComparer.Instance.Equals(instance, null)|};
					_ = {|#18:EqualityComparer<UserClass>.Default.Equals(instance, null)|};
					_ = {|#19:!EqualityComparer<UserClass>.Default.Equals(instance, null)|};
					_ = {|#20:UserClassEqualityComparer.Instance.Equals(instance, null)|};
					_ = {|#21:!UserClassEqualityComparer.Instance.Equals(instance, null)|};
				}
			}
			""";

		var expected = new[]
		{
			CreateDiagnostic(00, NullComparisonRule.Identity, ComparisonExpression.EqualityOperator),
			CreateDiagnostic(01, NullComparisonRule.Identity, ComparisonExpression.InequalityOperator),
			CreateDiagnostic(02, NullComparisonRule.Equality, ComparisonExpression.EqualityOperator),
			CreateDiagnostic(03, NullComparisonRule.Equality, ComparisonExpression.InequalityOperator),
			CreateDiagnostic(04, NullComparisonRule.Identity, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(05, NullComparisonRule.Identity, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(06, NullComparisonRule.Identity, ComparisonExpression.ReferenceEqualsMethod),
			CreateDiagnostic(07, NullComparisonRule.Identity, ComparisonExpression.NotReferenceEqualsMethod),
			CreateDiagnostic(08, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(09, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(10, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(11, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(12, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(13, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(14, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(15, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(16, NullComparisonRule.Identity, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(17, NullComparisonRule.Identity, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(18, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(19, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(20, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(21, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
		};

		await VerifyAsync(code, expected, type, comparer);
	}

	[Fact]
	public async Task Initialize_PureNullTests_FlagIfNullCheck()
	{
		var type = """
			using System;

			public class DefaultClass
			{
			}
			""";

		var code = """
			using System;
			using System.Collections.Generic;

			class Test
			{
				void Method(DefaultClass instance, IEqualityComparer<DefaultClass> comparer)
				{
					_ = instance is null;
					_ = instance is not null;
					_ = {|#0:(object)instance == null|};
					_ = {|#1:(object)instance != null|};
					_ = {|#2:instance == null|};
					_ = {|#3:instance != null|};
					_ = {|#4:Object.Equals(instance, null)|};
					_ = {|#5:!Object.Equals(instance, null)|};
					_ = {|#6:Object.ReferenceEquals(instance, null)|};
					_ = {|#7:!Object.ReferenceEquals(instance, null)|};
					_ = {|#8:((object)instance).Equals(null)|};
					_ = {|#9:!((object)instance).Equals(null)|};
					_ = {|#10:((IEquatable<DefaultClass>)instance).Equals(null)|};
					_ = {|#11:!((IEquatable<DefaultClass>)instance).Equals(null)|};
					_ = {|#12:instance.Equals(null)|};
					_ = {|#13:!instance.Equals(null)|};
					_ = {|#14:comparer.Equals(instance, null)|};
					_ = {|#15:!comparer.Equals(instance, null)|};
					_ = {|#16:ReferenceEqualityComparer.Instance.Equals(instance, null)|};
					_ = {|#17:!ReferenceEqualityComparer.Instance.Equals(instance, null)|};
					_ = {|#18:EqualityComparer<DefaultClass>.Default.Equals(instance, null)|};
					_ = {|#19:!EqualityComparer<DefaultClass>.Default.Equals(instance, null)|};
				}
			}
			""";

		var expected = new[]
		{
			CreateDiagnostic(00, NullComparisonRule.Identity, ComparisonExpression.EqualityOperator),
			CreateDiagnostic(01, NullComparisonRule.Identity, ComparisonExpression.InequalityOperator),
			CreateDiagnostic(02, NullComparisonRule.Identity, ComparisonExpression.EqualityOperator),
			CreateDiagnostic(03, NullComparisonRule.Identity, ComparisonExpression.InequalityOperator),
			CreateDiagnostic(04, NullComparisonRule.Identity, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(05, NullComparisonRule.Identity, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(06, NullComparisonRule.Identity, ComparisonExpression.ReferenceEqualsMethod),
			CreateDiagnostic(07, NullComparisonRule.Identity, ComparisonExpression.NotReferenceEqualsMethod),
			CreateDiagnostic(08, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(09, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(10, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(11, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(12, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(13, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(14, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(15, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(16, NullComparisonRule.Identity, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(17, NullComparisonRule.Identity, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(18, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(19, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
		};

		await VerifyAsync(code, expected, type);
	}

	[Fact]
	public async Task Initialize_NonNullTests_ReportNoDiagnostics()
	{
		var type = """
			using System;

			public interface IEquatable
			{
				bool Equals(object obj);
				bool Equals(Class obj);
			}

			public interface IInterface
			{
				bool Equals(object obj);
			}

			public interface IInterface<T>
			{
				bool Equals(T? other);
			}

			public class Class : IEquatable, IInterface, IInterface<Class>
			{
				public new bool Equals(object? obj) => throw null;
				bool IEquatable.Equals(object obj) => throw null;
				bool IEquatable.Equals(Class obj) => throw null;
				bool IInterface<Class>.Equals(Class? other) => throw null;
			}
			""";

		var extensions = """
			using System;

			public static class Extensions
			{
				public static bool Equal(this Class instance, Class other) => throw null;
				public static bool Equal(this object instance, object other) => throw null;
				public static bool Equals(this Class instance, Class other) => throw null;
				public static bool Equals(this object instance, object other) => throw null;
				public static bool ReferenceEquals(this Class instance, Class other) => throw null;
				public static bool ReferenceEquals(this object instance, object other) => throw null;
			}
			""";

		var bad = """
			using System;

			public class Bad
			{
				public static new bool Equals(object? obj) => throw null;
				public static bool Equals(Bad? obj) => throw null;
				public static new bool ReferenceEquals(object? objA, object? objB) => throw null;

				public void Equals() => throw null;
				public static void ReferenceEquals(object? obj) => throw null;
			}
			""";

		var code = """
			using System;
			using System.Collections.Generic;

			class Test
			{
				void Method(Class instance, IEqualityComparer<Class> comparer)
				{
					_ = instance.Equals(null);
					_ = !instance.Equals(null);
					_ = ((IEquatable)instance).Equals(null);
					_ = !((IEquatable)instance).Equals(null);
					_ = ((IInterface)instance).Equals(null);
					_ = !((IInterface)instance).Equals(null);
					_ = ((IInterface<Class>)instance).Equals(null);
					_ = !((IInterface<Class>)instance).Equals(null);
					_ = instance.ReferenceEquals(null);
					_ = !instance.ReferenceEquals(null);
					_ = instance.Equal(null);
					_ = !instance.Equal(null);
					_ = Extensions.Equal(instance, null);
					_ = !Extensions.Equal(instance, null);
					_ = Extensions.Equals(instance, null);
					_ = !Extensions.Equals(instance, null);
					_ = Extensions.Equals(null, instance);
					_ = !Extensions.Equals(null, instance);
					_ = {|#0:((object)instance).Equals(null)|};
					_ = {|#1:!((object)instance).Equals(null)|};

					_ = Bad.Equals(null);
					_ = Bad.Equals((object?)null);
					_ = Bad.Equals((Bad?)null);
					_ = Bad.ReferenceEquals(instance, null);
					new Bad().Equals();
					Bad.ReferenceEquals(null);
				}
			}
			""";

		var expected = new[]
		{
			CreateDiagnostic(0, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(1, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
		};

		await VerifyAsync(code, expected, type, extensions, bad);
	}

	[Fact]
	public async Task Initialize_NullableValueType_FlagIfNullCheck()
	{
		var type = """
			using System;

			public struct UserStruct : IEquatable<UserStruct>
			{
				public static bool operator ==(UserStruct left, UserStruct right) => throw null;
				public static bool operator !=(UserStruct left, UserStruct right) => throw null;
				bool IEquatable<UserStruct>.Equals(UserStruct other) => throw null;
				public bool Equals(UserStruct other) => throw null;
				public override bool Equals(object? obj) => throw null;
				public override int GetHashCode() => throw null;
			}
			""";

		var comparer = """
			using System.Collections.Generic;

			public class UserStructEqualityComparer : IEqualityComparer<UserStruct?>
			{
				public static IEqualityComparer<UserStruct?> Instance { get; } = new UserStructEqualityComparer();
				bool IEqualityComparer<UserStruct?>.Equals(UserStruct? x, UserStruct? y) => throw null;
				int IEqualityComparer<UserStruct?>.GetHashCode(UserStruct? obj) => throw null;
			}
			""";

		var code = """
			using System;
			using System.Collections.Generic;

			class Test
			{
				void Method(UserStruct? instance, IEqualityComparer<UserStruct?> comparer)
				{
					_ = instance is null;
					_ = instance is not null;
					_ = {|#0:(object)instance == null|};
					_ = {|#1:(object)instance != null|};
					_ = {|#2:instance == null|};
					_ = {|#3:instance != null|};
					_ = {|#4:Object.Equals(instance, null)|};
					_ = {|#5:!Object.Equals(instance, null)|};
					_ = {|#6:Object.ReferenceEquals(instance, null)|};
					_ = {|#7:!Object.ReferenceEquals(instance, null)|};
					_ = {|#8:((object)instance).Equals(null)|};
					_ = {|#9:!((object)instance).Equals(null)|};
					_ = {|#10:((IEquatable<UserStruct>)instance).Equals(null)|};
					_ = {|#11:!((IEquatable<UserStruct>)instance).Equals(null)|};
					_ = {|#12:instance.Equals(null)|};
					_ = {|#13:!instance.Equals(null)|};
					_ = {|#14:comparer.Equals(instance, null)|};
					_ = {|#15:!comparer.Equals(instance, null)|};
					_ = {|#16:ReferenceEqualityComparer.Instance.Equals(instance, null)|};
					_ = {|#17:!ReferenceEqualityComparer.Instance.Equals(instance, null)|};
					_ = {|#18:EqualityComparer<UserStruct?>.Default.Equals(instance, null)|};
					_ = {|#19:!EqualityComparer<UserStruct?>.Default.Equals(instance, null)|};
					_ = {|#20:UserStructEqualityComparer.Instance.Equals(instance, null)|};
					_ = {|#21:!UserStructEqualityComparer.Instance.Equals(instance, null)|};
				}
			}
			""";

		var expected = new[]
		{
			CreateDiagnostic(00, NullComparisonRule.Identity, ComparisonExpression.EqualityOperator),
			CreateDiagnostic(01, NullComparisonRule.Identity, ComparisonExpression.InequalityOperator),
			CreateDiagnostic(02, NullComparisonRule.Identity, ComparisonExpression.EqualityOperator),
			CreateDiagnostic(03, NullComparisonRule.Identity, ComparisonExpression.InequalityOperator),
			CreateDiagnostic(04, NullComparisonRule.Identity, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(05, NullComparisonRule.Identity, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(06, NullComparisonRule.Identity, ComparisonExpression.ReferenceEqualsMethod),
			CreateDiagnostic(07, NullComparisonRule.Identity, ComparisonExpression.NotReferenceEqualsMethod),
			CreateDiagnostic(08, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(09, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(10, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(11, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(12, NullComparisonRule.Identity, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(13, NullComparisonRule.Identity, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(14, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(15, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(16, NullComparisonRule.Identity, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(17, NullComparisonRule.Identity, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(18, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(19, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
			CreateDiagnostic(20, NullComparisonRule.Equality, ComparisonExpression.EqualsMethod),
			CreateDiagnostic(21, NullComparisonRule.Equality, ComparisonExpression.NotEqualsMethod),
		};

		await VerifyAsync(code, expected, type, comparer);
	}

	[Fact]
	public async Task Initialize_NonNullableValueType_ReportNoDiagnostics()
	{
		var type = """
			using System;

			public struct DefaultStruct : IEquatable<DefaultStruct>
			{
				public bool Equals(DefaultStruct other) => throw null;
			}
			""";

		var code = """
			using System;
			using System.Collections.Generic;

			class Test
			{
				void Method(DefaultStruct instance, IEqualityComparer<DefaultStruct?> comparer)
				{
					_ = (object)instance == null;
					_ = (object)instance != null;
					_ = null == (object)instance;
					_ = null != (object)instance;
					_ = Object.Equals(instance, null);
					_ = !Object.Equals(instance, null);
					_ = Object.ReferenceEquals(instance, null);
					_ = !Object.ReferenceEquals(instance, null);
					_ = ((object)instance).Equals(null);
					_ = !((object)instance).Equals(null);
					_ = ((IEquatable<DefaultStruct>)instance).Equals(null);
					_ = !((IEquatable<DefaultStruct>)instance).Equals(null);
					_ = instance.Equals(null);
					_ = !instance.Equals(null);
					_ = comparer.Equals(instance, null);
					_ = !comparer.Equals(instance, null);
					_ = ReferenceEqualityComparer.Instance.Equals(instance, null);
					_ = !ReferenceEqualityComparer.Instance.Equals(instance, null);
					_ = EqualityComparer<DefaultStruct?>.Default.Equals(instance, null);
					_ = !EqualityComparer<DefaultStruct?>.Default.Equals(instance, null);
				}
			}
			""";

		await VerifyAsync(code, Array.Empty<DiagnosticResult>(), type);
	}

	[Fact]
	public async Task Initialize_IsOperator_ReportNoDiagnostics()
	{
		var code = """
			using System;

			class Test
			{
				void Method(object instance)
				{
					_ = instance is null;
					_ = instance is not null;

					_ = instance is object;
					_ = instance is not object;

					_ = instance is { };
					_ = instance is not { };

					_ = instance is { } objA;
					_ = instance is not { } objB;

					_ = instance is string;
					_ = instance is not string;

					_ = instance is Type type;
					_ = instance is not Type obj;

					_ = instance is var variable;
					_ = instance is var _;
				}
			}
			""";

		await VerifyNoOpAsync(code);
	}

	[Fact]
	public async Task Initialize_Other_ReportNoDiagnostics()
	{
		var code = """
			using System;

			class Test
			{
				void Method(Object obj, Nullable<int> value)
				{
					_ = obj.GetHashCode();
					_ = obj.GetType();
					_ = obj.ToString();

					_ = value.HasValue;
					_ = value.Value;
					_ = value.GetHashCode();
					_ = value.GetValueOrDefault();
					_ = value.GetValueOrDefault(default);
					_ = value.ToString();
				}
			}
			""";

		await VerifyNoOpAsync(code);
	}

	private static DiagnosticResult CreateDiagnostic(int markupKey, NullComparisonRule rule, ComparisonExpression expression)
	{
		var descriptor = rule switch
		{
			NullComparisonRule.Equality => F0100xPreferPatternMatchingNullCheckOverComparisonWithNull.EqualityComparisonRule,
			NullComparisonRule.Identity => F0100xPreferPatternMatchingNullCheckOverComparisonWithNull.IdentityComparisonRule,
			_ => throw new InvalidEnumArgumentException(nameof(rule), (int)rule, typeof(NullComparisonRule)),
		};

		var pattern = expression switch
		{
			ComparisonExpression.EqualityOperator or ComparisonExpression.EqualsMethod or ComparisonExpression.ReferenceEqualsMethod => "is",
			ComparisonExpression.InequalityOperator or ComparisonExpression.NotEqualsMethod or ComparisonExpression.NotReferenceEqualsMethod => "is not",
			_ => throw new InvalidEnumArgumentException(nameof(expression), (int)expression, typeof(ComparisonExpression)),
		};

		var modifier = expression switch
		{
			ComparisonExpression.EqualityOperator or ComparisonExpression.InequalityOperator => "overloaded",
			ComparisonExpression.EqualsMethod or ComparisonExpression.NotEqualsMethod or ComparisonExpression.ReferenceEqualsMethod or ComparisonExpression.NotReferenceEqualsMethod => "overridden",
			_ => throw new InvalidEnumArgumentException(nameof(expression), (int)expression, typeof(ComparisonExpression)),
		};

		var memberName = expression switch
		{
			ComparisonExpression.EqualityOperator => "==",
			ComparisonExpression.InequalityOperator => "!=",
			ComparisonExpression.EqualsMethod or ComparisonExpression.NotEqualsMethod => "Equals",
			ComparisonExpression.ReferenceEqualsMethod or ComparisonExpression.NotReferenceEqualsMethod => "ReferenceEquals",
			_ => throw new InvalidEnumArgumentException(nameof(expression), (int)expression, typeof(ComparisonExpression)),
		};

		var memberKind = expression switch
		{
			ComparisonExpression.EqualityOperator or ComparisonExpression.InequalityOperator => "operator",
			ComparisonExpression.EqualsMethod or ComparisonExpression.NotEqualsMethod or ComparisonExpression.ReferenceEqualsMethod or ComparisonExpression.NotReferenceEqualsMethod => "method",
			_ => throw new InvalidEnumArgumentException(nameof(expression), (int)expression, typeof(ComparisonExpression)),
		};

		var test = expression switch
		{
			ComparisonExpression.EqualityOperator or ComparisonExpression.EqualsMethod or ComparisonExpression.ReferenceEqualsMethod => "null",
			ComparisonExpression.InequalityOperator or ComparisonExpression.NotEqualsMethod or ComparisonExpression.NotReferenceEqualsMethod => "non-null",
			_ => throw new InvalidEnumArgumentException(nameof(expression), (int)expression, typeof(ComparisonExpression)),
		};

		var arguments = rule switch
		{
			NullComparisonRule.Equality => new object[] { pattern, modifier, memberName, memberKind, test },
			NullComparisonRule.Identity => new object[] { pattern, memberName, memberKind, test },
			_ => throw new InvalidEnumArgumentException(nameof(rule), (int)rule, typeof(NullComparisonRule)),
		};

		return Verify.Diagnostic<F0100xPreferPatternMatchingNullCheckOverComparisonWithNull>(descriptor)
			.WithLocation(markupKey)
			.WithArguments(arguments);
	}

	private static Task VerifyAsync(string code, DiagnosticResult[] diagnostics, params string[] additionalDocuments)
		=> Verify.DiagnosticAnalyzer<F0100xPreferPatternMatchingNullCheckOverComparisonWithNull>().DiagnosticAsync(code, diagnostics, new string[][] { additionalDocuments }, ReferenceAssemblies.Net.Net50, LanguageVersion.Latest);

	private static Task VerifyNoOpAsync(string code)
		=> Verify.DiagnosticAnalyzer<F0100xPreferPatternMatchingNullCheckOverComparisonWithNull>().NoOpAsync(code);

	private static Task VerifyNoOpAsync(string code, LanguageVersion languageVersion)
		=> Verify.DiagnosticAnalyzer<F0100xPreferPatternMatchingNullCheckOverComparisonWithNull>().NoOpAsync(code, ReferenceAssemblies.Net.Net50, languageVersion);

	private enum NullComparisonRule
	{
		Equality,
		Identity,
	}

	private enum ComparisonExpression
	{
		EqualityOperator,
		InequalityOperator,
		EqualsMethod,
		NotEqualsMethod,
		ReferenceEqualsMethod,
		NotReferenceEqualsMethod,
	}
}
