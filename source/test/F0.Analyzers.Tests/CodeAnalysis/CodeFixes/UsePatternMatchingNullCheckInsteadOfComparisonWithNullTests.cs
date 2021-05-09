using System.Threading.Tasks;
using F0.CodeAnalysis;
using F0.CodeAnalysis.CodeFixes;
using F0.CodeAnalysis.Diagnostics;
using F0.Testing.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace F0.Tests.CodeAnalysis.CodeFixes
{
	public class UsePatternMatchingNullCheckInsteadOfComparisonWithNullTests
	{
		private const string EqualityComparisonMessage = "Prefer '{0}' pattern over calling the (potentially) {1} '{2}' {3} to check for {4}";
		private const string IdentityComparisonMessage = "Prefer '{0}' pattern over calling the '{1}' {2} to check for {3}";

		[Fact]
		public void UsePatternMatchingNullCheckInsteadOfComparisonWithNull_CheckType()
			=> Verify.CodeFix<UsePatternMatchingNullCheckInsteadOfComparisonWithNull>().Type();

		[Fact]
		public async Task RegisterCodeFixesAsync_PatternMatching_RegisterNoCodeFix()
		{
			var code =
@"using System;

record Record();

class Test
{
	void Method(Record record)
	{
		_ = record is null;
		_ = record is not null;
	}
}";

			await VerifyNoOpAsync(code);
		}

		[Fact]
		public async Task RegisterCodeFixesAsync_EqualityOperator_FixIfNullCheck()
		{
			var code =
@"using System;

record Record();

class Test
{
	void Method(Record record, object obj)
	{
		_ = {|#0:record == null|};
		_ = {|#1:null == record|};
		_ = null == null;
		_ = record == record;

		_ = {|#2:(Record)obj == null|};
		_ = {|#3:null == (Record)obj|};
		_ = (Record)obj == (Record)obj;
	}
}";

			var fix =
@"using System;

record Record();

class Test
{
	void Method(Record record, object obj)
	{
		_ = record is null;
		_ = record is null;
		_ = null == null;
		_ = record == record;

		_ = obj is null;
		_ = obj is null;
		_ = (Record)obj == (Record)obj;
	}
}";

			var expected = new[]
			{
				CreateEqualityComparisonDiagnostic(0, "is", "overloaded", "==", "operator", "null"),
				CreateEqualityComparisonDiagnostic(1, "is", "overloaded", "==", "operator", "null"),

				CreateEqualityComparisonDiagnostic(2, "is", "overloaded", "==", "operator", "null"),
				CreateEqualityComparisonDiagnostic(3, "is", "overloaded", "==", "operator", "null"),
			};

			await VerifyAsync(code, expected, fix);
		}

		[Fact]
		public async Task RegisterCodeFixesAsync_InequalityOperator_FixIfNullCheck()
		{
			var code =
@"using System;

record Record();

class Test
{
	void Method(Record record, object obj)
	{
		_ = {|#0:record != null|};
		_ = {|#1:null != record|};
		_ = null != null;
		_ = record != record;

		_ = {|#2:(Record)obj != null|};
		_ = {|#3:null != (Record)obj|};
		_ = (Record)obj != (Record)obj;
	}
}";

			var fix =
@"using System;

record Record();

class Test
{
	void Method(Record record, object obj)
	{
		_ = record is not null;
		_ = record is not null;
		_ = null != null;
		_ = record != record;

		_ = obj is not null;
		_ = obj is not null;
		_ = (Record)obj != (Record)obj;
	}
}";

			var expected = new[]
			{
				CreateEqualityComparisonDiagnostic(0, "is not", "overloaded", "!=", "operator", "non-null"),
				CreateEqualityComparisonDiagnostic(1, "is not", "overloaded", "!=", "operator", "non-null"),

				CreateEqualityComparisonDiagnostic(2, "is not", "overloaded", "!=", "operator", "non-null"),
				CreateEqualityComparisonDiagnostic(3, "is not", "overloaded", "!=", "operator", "non-null"),
			};

			await VerifyAsync(code, expected, fix);
		}

		[Fact]
		public async Task RegisterCodeFixesAsync_Comparison_FixIfNullCheck()
		{
			var code =
@"using System;

record Record();

class Test
{
	void Method(Record record, object obj)
	{
		_ = {|#0:(object)record == null|};
		_ = {|#1:null == (object)record|};
		_ = (object)record == (object)record;
		_ = {|#2:obj == null|};
		_ = {|#3:null == obj|};
		_ = obj == obj;

		_ = {|#4:(object)record != null|};
		_ = {|#5:null != (object)record|};
		_ = (object)record != (object)record;
		_ = {|#6:obj != null|};
		_ = {|#7:null != obj|};
		_ = obj != obj;
	}
}";

			var fix =
@"using System;

record Record();

class Test
{
	void Method(Record record, object obj)
	{
		_ = record is null;
		_ = record is null;
		_ = (object)record == (object)record;
		_ = obj is null;
		_ = obj is null;
		_ = obj == obj;

		_ = record is not null;
		_ = record is not null;
		_ = (object)record != (object)record;
		_ = obj is not null;
		_ = obj is not null;
		_ = obj != obj;
	}
}";

			var expected = new[]
			{
				CreateIdentityComparisonDiagnostic(0, "is", "==", "operator", "null"),
				CreateIdentityComparisonDiagnostic(1, "is", "==", "operator", "null"),
				CreateIdentityComparisonDiagnostic(2, "is", "==", "operator", "null"),
				CreateIdentityComparisonDiagnostic(3, "is", "==", "operator", "null"),

				CreateIdentityComparisonDiagnostic(4, "is not", "!=", "operator", "non-null"),
				CreateIdentityComparisonDiagnostic(5, "is not", "!=", "operator", "non-null"),
				CreateIdentityComparisonDiagnostic(6, "is not", "!=", "operator", "non-null"),
				CreateIdentityComparisonDiagnostic(7, "is not", "!=", "operator", "non-null"),
			};

			await VerifyAsync(code, expected, fix);
		}

		[Fact]
		public async Task RegisterCodeFixesAsync_ObjectEquals_FixIfNullCheck()
		{
			var code =
@"using System;

record Record();

class Test
{
	void Method(Record record, object obj)
	{
		_ = {|#0:record.Equals(null)|};
		_ = {|#1:!record.Equals(null)|};
		_ = record.Equals(record);
		_ = record.Equals(obj);

		_ = {|#2:obj.Equals(null)|};
		_ = {|#3:!obj.Equals(null)|};
		_ = obj.Equals(obj);
		_ = obj.Equals(record);

		_ = {|#4:((Object)record).Equals(null)|};
		_ = {|#5:!((Object)record).Equals(null)|};
		_ = ((Object)record).Equals(((Object)record));
		_ = ((Object)record).Equals(obj);
	}
}";

			var fix =
@"using System;

record Record();

class Test
{
	void Method(Record record, object obj)
	{
		_ = record is null;
		_ = record is not null;
		_ = record.Equals(record);
		_ = record.Equals(obj);

		_ = obj is null;
		_ = obj is not null;
		_ = obj.Equals(obj);
		_ = obj.Equals(record);

		_ = record is null;
		_ = record is not null;
		_ = ((Object)record).Equals(((Object)record));
		_ = ((Object)record).Equals(obj);
	}
}";

			var expected = new[]
			{
				CreateEqualityComparisonDiagnostic(0, "is", "overridden", "Equals", "method", "null"),
				CreateEqualityComparisonDiagnostic(1, "is not", "overridden", "Equals", "method", "non-null"),
				CreateEqualityComparisonDiagnostic(2, "is", "overridden", "Equals", "method", "null"),
				CreateEqualityComparisonDiagnostic(3, "is not", "overridden", "Equals", "method", "non-null"),
				CreateEqualityComparisonDiagnostic(4, "is", "overridden", "Equals", "method", "null"),
				CreateEqualityComparisonDiagnostic(5, "is not", "overridden", "Equals", "method", "non-null"),
			};

			await VerifyAsync(code, expected, fix);
		}

		[Fact]
		public async Task RegisterCodeFixesAsync_StaticObjectEquals_FixIfNullCheck()
		{
			var code =
@"using System;

record Record();

class Test
{
	void Method(Record record)
	{
		_ = {|#0:Object.Equals(record, null)|};
		_ = {|#1:!Object.Equals(record, null)|};

		_ = {|#2:Object.Equals(null, record)|};
		_ = {|#3:!Object.Equals(null, record)|};

		_ = Object.Equals(null, null);
		_ = !Object.Equals(null, null);

		_ = Object.Equals(record, record);
		_ = !Object.Equals(record, record);
	}
}";

			var fix =
@"using System;

record Record();

class Test
{
	void Method(Record record)
	{
		_ = record is null;
		_ = record is not null;

		_ = record is null;
		_ = record is not null;

		_ = Object.Equals(null, null);
		_ = !Object.Equals(null, null);

		_ = Object.Equals(record, record);
		_ = !Object.Equals(record, record);
	}
}";

			var expected = new[]
			{
				CreateIdentityComparisonDiagnostic(0, "is", "Equals", "method", "null"),
				CreateIdentityComparisonDiagnostic(1, "is not", "Equals", "method", "non-null"),
				CreateIdentityComparisonDiagnostic(2, "is", "Equals", "method", "null"),
				CreateIdentityComparisonDiagnostic(3, "is not", "Equals", "method", "non-null"),
			};

			await VerifyAsync(code, expected, fix);
		}

		[Fact]
		public async Task RegisterCodeFixesAsync_ObjectReferenceEquals_FixIfNullCheck()
		{
			var code =
@"using System;

record Record();

class Test
{
	void Method(Record record)
	{
		_ = {|#0:Object.ReferenceEquals(record, null)|};
		_ = {|#1:!Object.ReferenceEquals(record, null)|};

		_ = {|#2:Object.ReferenceEquals(null, record)|};
		_ = {|#3:!Object.ReferenceEquals(null, record)|};

		_ = Object.ReferenceEquals(null, null);
		_ = !Object.ReferenceEquals(null, null);

		_ = Object.ReferenceEquals(record, record);
		_ = !Object.ReferenceEquals(record, record);
	}
}";

			var fix =
@"using System;

record Record();

class Test
{
	void Method(Record record)
	{
		_ = record is null;
		_ = record is not null;

		_ = record is null;
		_ = record is not null;

		_ = Object.ReferenceEquals(null, null);
		_ = !Object.ReferenceEquals(null, null);

		_ = Object.ReferenceEquals(record, record);
		_ = !Object.ReferenceEquals(record, record);
	}
}";

			var expected = new[]
			{
				CreateIdentityComparisonDiagnostic(0, "is", "ReferenceEquals", "method", "null"),
				CreateIdentityComparisonDiagnostic(1, "is not", "ReferenceEquals", "method", "non-null"),
				CreateIdentityComparisonDiagnostic(2, "is", "ReferenceEquals", "method", "null"),
				CreateIdentityComparisonDiagnostic(3, "is not", "ReferenceEquals", "method", "non-null"),
			};

			await VerifyAsync(code, expected, fix);
		}

		[Fact]
		public async Task RegisterCodeFixesAsync_EquatableEquals_FixIfNullCheck()
		{
			var code =
@"using System;

record Record();

class Test
{
	void Method(IEquatable<Record> equatable)
	{
		_ = {|#0:equatable.Equals(null)|};
		_ = {|#1:!equatable.Equals(null)|};
		_ = equatable.Equals(new Record());
	}
}";

			var fix =
@"using System;

record Record();

class Test
{
	void Method(IEquatable<Record> equatable)
	{
		_ = equatable is null;
		_ = equatable is not null;
		_ = equatable.Equals(new Record());
	}
}";

			var expected = new[]
			{
				CreateEqualityComparisonDiagnostic(0, "is", "overridden", "Equals", "method", "null"),
				CreateEqualityComparisonDiagnostic(1, "is not", "overridden", "Equals", "method", "non-null"),
			};

			await VerifyAsync(code, expected, fix);
		}

		[Fact]
		public async Task RegisterCodeFixesAsync_CastEquatable_FixIfNullCheck()
		{
			var code =
@"using System;

record Record();

class Test
{
	void Method(Record record)
	{
		_ = {|#0:((IEquatable<Record>)record).Equals(null)|};
		_ = {|#1:!((IEquatable<Record>)record).Equals(null)|};
		_ = ((IEquatable<Record>)record).Equals(new Record());
	}
}";

			var fix =
@"using System;

record Record();

class Test
{
	void Method(Record record)
	{
		_ = record is null;
		_ = record is not null;
		_ = ((IEquatable<Record>)record).Equals(new Record());
	}
}";

			var expected = new[]
			{
				CreateEqualityComparisonDiagnostic(0, "is", "overridden", "Equals", "method", "null"),
				CreateEqualityComparisonDiagnostic(1, "is not", "overridden", "Equals", "method", "non-null"),
			};

			await VerifyAsync(code, expected, fix);
		}

		[Fact]
		public async Task RegisterCodeFixesAsync_EqualityComparerEquals_FixIfNullCheck()
		{
			var code =
@"using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

record Record();

class RecordEqualityComparer : EqualityComparer<Record>
{
	internal static RecordEqualityComparer Instance { get; } = new RecordEqualityComparer();
	public override bool Equals(Record? x, Record? y) => throw null;
	public override int GetHashCode([DisallowNull] Record obj) => throw null;
}

class Test
{
	void Method(Record record, IEqualityComparer<Record> comparer, EqualityComparer<Record> defaultComparer, RecordEqualityComparer customComparer)
	{
		_ = {|#0:comparer.Equals(record, null)|};
		_ = {|#1:!comparer.Equals(record, null)|};
		_ = {|#2:comparer.Equals(null, record)|};
		_ = {|#3:!comparer.Equals(null, record)|};
		_ = comparer.Equals(null, null);
		_ = !comparer.Equals(null, null);
		_ = comparer.Equals(record, record);
		_ = !comparer.Equals(record, record);

		_ = {|#4:defaultComparer.Equals(record, null)|};
		_ = {|#5:!defaultComparer.Equals(record, null)|};
		_ = {|#6:defaultComparer.Equals(null, record)|};
		_ = {|#7:!defaultComparer.Equals(null, record)|};
		_ = defaultComparer.Equals(null, null);
		_ = !defaultComparer.Equals(null, null);
		_ = defaultComparer.Equals(record, record);
		_ = !defaultComparer.Equals(record, record);

		_ = {|#8:customComparer.Equals(record, null)|};
		_ = {|#9:!customComparer.Equals(record, null)|};
		_ = {|#10:customComparer.Equals(null, record)|};
		_ = {|#11:!customComparer.Equals(null, record)|};
		_ = customComparer.Equals(null, null);
		_ = !customComparer.Equals(null, null);
		_ = customComparer.Equals(record, record);
		_ = !customComparer.Equals(record, record);
	}
}";

			var fix =
@"using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

record Record();

class RecordEqualityComparer : EqualityComparer<Record>
{
	internal static RecordEqualityComparer Instance { get; } = new RecordEqualityComparer();
	public override bool Equals(Record? x, Record? y) => throw null;
	public override int GetHashCode([DisallowNull] Record obj) => throw null;
}

class Test
{
	void Method(Record record, IEqualityComparer<Record> comparer, EqualityComparer<Record> defaultComparer, RecordEqualityComparer customComparer)
	{
		_ = record is null;
		_ = record is not null;
		_ = record is null;
		_ = record is not null;
		_ = comparer.Equals(null, null);
		_ = !comparer.Equals(null, null);
		_ = comparer.Equals(record, record);
		_ = !comparer.Equals(record, record);

		_ = record is null;
		_ = record is not null;
		_ = record is null;
		_ = record is not null;
		_ = defaultComparer.Equals(null, null);
		_ = !defaultComparer.Equals(null, null);
		_ = defaultComparer.Equals(record, record);
		_ = !defaultComparer.Equals(record, record);

		_ = record is null;
		_ = record is not null;
		_ = record is null;
		_ = record is not null;
		_ = customComparer.Equals(null, null);
		_ = !customComparer.Equals(null, null);
		_ = customComparer.Equals(record, record);
		_ = !customComparer.Equals(record, record);
	}
}";

			var expected = new[]
			{
				CreateEqualityComparisonDiagnostic(00, "is", "overridden", "Equals", "method", "null"),
				CreateEqualityComparisonDiagnostic(01, "is not", "overridden", "Equals", "method", "non-null"),
				CreateEqualityComparisonDiagnostic(02, "is", "overridden", "Equals", "method", "null"),
				CreateEqualityComparisonDiagnostic(03, "is not", "overridden", "Equals", "method", "non-null"),

				CreateEqualityComparisonDiagnostic(04, "is", "overridden", "Equals", "method", "null"),
				CreateEqualityComparisonDiagnostic(05, "is not", "overridden", "Equals", "method", "non-null"),
				CreateEqualityComparisonDiagnostic(06, "is", "overridden", "Equals", "method", "null"),
				CreateEqualityComparisonDiagnostic(07, "is not", "overridden", "Equals", "method", "non-null"),

				CreateEqualityComparisonDiagnostic(08, "is", "overridden", "Equals", "method", "null"),
				CreateEqualityComparisonDiagnostic(09, "is not", "overridden", "Equals", "method", "non-null"),
				CreateEqualityComparisonDiagnostic(10, "is", "overridden", "Equals", "method", "null"),
				CreateEqualityComparisonDiagnostic(11, "is not", "overridden", "Equals", "method", "non-null"),
			};

			await VerifyAsync(code, expected, fix, ReferenceAssemblies.NetStandard.NetStandard21);
		}

		[Fact]
		public async Task RegisterCodeFixesAsync_ReferenceEqualityComparerEquals_FixIfNullCheck()
		{
			var code =
@"using System.Collections.Generic;

record Record();

class Test
{
	void Method(Record record, ReferenceEqualityComparer referenceComparer)
	{
		_ = {|#0:referenceComparer.Equals(record, null)|};
		_ = {|#1:!referenceComparer.Equals(record, null)|};
		_ = {|#2:referenceComparer.Equals(null, record)|};
		_ = {|#3:!referenceComparer.Equals(null, record)|};
		_ = referenceComparer.Equals(null, null);
		_ = !referenceComparer.Equals(null, null);
		_ = referenceComparer.Equals(record, record);
		_ = !referenceComparer.Equals(record, record);
	}
}";

			var fix =
@"using System.Collections.Generic;

record Record();

class Test
{
	void Method(Record record, ReferenceEqualityComparer referenceComparer)
	{
		_ = record is null;
		_ = record is not null;
		_ = record is null;
		_ = record is not null;
		_ = referenceComparer.Equals(null, null);
		_ = !referenceComparer.Equals(null, null);
		_ = referenceComparer.Equals(record, record);
		_ = !referenceComparer.Equals(record, record);
	}
}";

			var expected = new[]
			{
				CreateIdentityComparisonDiagnostic(0, "is", "Equals", "method", "null"),
				CreateIdentityComparisonDiagnostic(1, "is not", "Equals", "method", "non-null"),
				CreateIdentityComparisonDiagnostic(2, "is", "Equals", "method", "null"),
				CreateIdentityComparisonDiagnostic(3, "is not", "Equals", "method", "non-null"),
			};

			await VerifyAsync(code, expected, fix, ReferenceAssemblies.Net.Net50);
		}

		[Fact]
		public async Task RegisterCodeFixesAsync_CastEqualityComparer_FixIfNullCheck()
		{
			var code =
@"using System.Collections.Generic;

record Record();

class Test
{
	void Method(Record record, ReferenceEqualityComparer referenceComparer)
	{
		_ = {|#0:((IEqualityComparer<object>)referenceComparer).Equals(record, null)|};
		_ = {|#1:!((IEqualityComparer<object>)referenceComparer).Equals(record, null)|};
		_ = {|#2:((IEqualityComparer<object>)referenceComparer).Equals(null, record)|};
		_ = {|#3:!((IEqualityComparer<object>)referenceComparer).Equals(null, record)|};
		_ = ((IEqualityComparer<object>)referenceComparer).Equals(null, null);
		_ = !((IEqualityComparer<object>)referenceComparer).Equals(null, null);
		_ = ((IEqualityComparer<object>)referenceComparer).Equals(record, record);
		_ = !((IEqualityComparer<object>)referenceComparer).Equals(record, record);
	}
}";

			var fix =
@"using System.Collections.Generic;

record Record();

class Test
{
	void Method(Record record, ReferenceEqualityComparer referenceComparer)
	{
		_ = record is null;
		_ = record is not null;
		_ = record is null;
		_ = record is not null;
		_ = ((IEqualityComparer<object>)referenceComparer).Equals(null, null);
		_ = !((IEqualityComparer<object>)referenceComparer).Equals(null, null);
		_ = ((IEqualityComparer<object>)referenceComparer).Equals(record, record);
		_ = !((IEqualityComparer<object>)referenceComparer).Equals(record, record);
	}
}";

			var expected = new[]
			{
				CreateEqualityComparisonDiagnostic(0, "is", "overridden", "Equals", "method", "null"),
				CreateEqualityComparisonDiagnostic(1, "is not", "overridden", "Equals", "method", "non-null"),
				CreateEqualityComparisonDiagnostic(2, "is", "overridden", "Equals", "method", "null"),
				CreateEqualityComparisonDiagnostic(3, "is not", "overridden", "Equals", "method", "non-null"),
			};

			await VerifyAsync(code, expected, fix, ReferenceAssemblies.Net.Net50);
		}

		[Fact]
		public async Task RegisterCodeFixesAsync_Compatibility_FixIfNullCheck()
		{
			var code =
@"using System;

record Record();

class Test
{
	void Method(Record record)
	{
		_ = {|#0:record.Equals(null)|} == true;
		_ = {|#1:!record.Equals(null)|} == false;
	}
}";

			var fix =
@"using System;

record Record();

class Test
{
	void Method(Record record)
	{
		_ = record is null == true;
		_ = record is not null == false;
	}
}";

			var expected = new[]
			{
				CreateEqualityComparisonDiagnostic(0, "is", "overridden", "Equals", "method", "null"),
				CreateEqualityComparisonDiagnostic(1, "is not", "overridden", "Equals", "method", "non-null"),
			};

			await VerifyAsync(code, expected, fix);
		}

		[Fact]
		public async Task RegisterCodeFixesAsync_Other_FixIfNullCheck()
		{
			var code =
@"using System;

class Test
{
	void Method(Object obj, Nullable<int> value)
	{
		_ = obj.GetHashCode() == null;
		_ = {|#0:obj.GetType() == null|};
		_ = {|#1:obj.ToString() == null|};

		_ = value.HasValue != null;
		_ = value.Value != null;
		_ = value.GetHashCode() != null;
		_ = value.GetValueOrDefault() != null;
		_ = value.GetValueOrDefault(default) != null;
		_ = {|#2:value.ToString() != null|};
	}
}";

			var fix =
@"using System;

class Test
{
	void Method(Object obj, Nullable<int> value)
	{
		_ = obj.GetHashCode() == null;
		_ = obj.GetType() is null;
		_ = obj.ToString() is null;

		_ = value.HasValue != null;
		_ = value.Value != null;
		_ = value.GetHashCode() != null;
		_ = value.GetValueOrDefault() != null;
		_ = value.GetValueOrDefault(default) != null;
		_ = value.ToString() is not null;
	}
}";

			var expected = new[]
			{
				CreateEqualityComparisonDiagnostic(0, "is", "overloaded", "==", "operator", "null"),
				CreateEqualityComparisonDiagnostic(1, "is", "overloaded", "==", "operator", "null"),

				CreateEqualityComparisonDiagnostic(2, "is not", "overloaded", "!=", "operator", "non-null"),
			};

			await VerifyAsync(code, expected, fix);
		}

		private static DiagnosticResult CreateEqualityComparisonDiagnostic(int markupKey, string pattern, string modifier, string memberName, string memberKind, string test)
			=> Verify.Diagnostic<F0100xPreferPatternMatchingNullCheckOverComparisonWithNull, UsePatternMatchingNullCheckInsteadOfComparisonWithNull>(DiagnosticIds.F01001)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithMessageFormat(EqualityComparisonMessage)
				.WithArguments(new object[] { pattern, modifier, memberName, memberKind, test })
				.WithLocation(markupKey);

		private static DiagnosticResult CreateIdentityComparisonDiagnostic(int markupKey, string pattern, string memberName, string memberKind, string test)
			=> Verify.Diagnostic<F0100xPreferPatternMatchingNullCheckOverComparisonWithNull, UsePatternMatchingNullCheckInsteadOfComparisonWithNull>(DiagnosticIds.F01002)
				.WithSeverity(DiagnosticSeverity.Info)
				.WithMessageFormat(IdentityComparisonMessage)
				.WithArguments(new object[] { pattern, memberName, memberKind, test })
				.WithLocation(markupKey);

		private static Task VerifyAsync(string code, DiagnosticResult[] diagnostics, string fix)
			=> Verify.CodeFix<F0100xPreferPatternMatchingNullCheckOverComparisonWithNull, UsePatternMatchingNullCheckInsteadOfComparisonWithNull>().CodeActionAsync(code, diagnostics, fix);

		private static Task VerifyAsync(string code, DiagnosticResult[] diagnostics, string fix, ReferenceAssemblies referenceAssemblies)
			=> Verify.CodeFix<F0100xPreferPatternMatchingNullCheckOverComparisonWithNull, UsePatternMatchingNullCheckInsteadOfComparisonWithNull>().CodeActionAsync(code, diagnostics, fix, referenceAssemblies);

		private static Task VerifyNoOpAsync(string code)
			=> Verify.CodeFix<F0100xPreferPatternMatchingNullCheckOverComparisonWithNull, UsePatternMatchingNullCheckInsteadOfComparisonWithNull>().NoOpAsync(code);
	}
}
