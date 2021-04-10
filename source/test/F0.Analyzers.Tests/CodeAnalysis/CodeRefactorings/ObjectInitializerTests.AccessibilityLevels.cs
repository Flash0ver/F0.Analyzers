using System;
using System.Threading.Tasks;
using F0.Testing.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace F0.Tests.CodeAnalysis.CodeRefactorings
{
	public partial class ObjectInitializerTests
	{
		[Fact]
		public async Task ComputeRefactoringsAsync_AccessModifiersInternal_AssignsAccessibleMembers()
		{
			var initialCode =
				@"using System;

				class AccessModifiers
				{
					public int Public_Field;
					protected int Protected_Field;
					internal int Internal_Field;
					protected internal int Protected_Internal_Field;
					private int Private_Field;
					private protected int Private_Protected_Field;

					public int Public_Property { get; set; }
					protected int Protected_Property { get; set; }
					internal int Internal_Property { get; set; }
					protected internal int Protected_Internal_Property { get; set; }
					private int Private_Property { get; set; }
					private protected int Private_Protected_Property { get; set; }
				}

				class C
				{
					void Test()
					{
						var accessibility = [|new AccessModifiers()|];
					}
				}";

			var expectedCode =
				@"using System;

				class AccessModifiers
				{
					public int Public_Field;
					protected int Protected_Field;
					internal int Internal_Field;
					protected internal int Protected_Internal_Field;
					private int Private_Field;
					private protected int Private_Protected_Field;

					public int Public_Property { get; set; }
					protected int Protected_Property { get; set; }
					internal int Internal_Property { get; set; }
					protected internal int Protected_Internal_Property { get; set; }
					private int Private_Property { get; set; }
					private protected int Private_Protected_Property { get; set; }
				}

				class C
				{
					void Test()
					{
						var accessibility = new AccessModifiers()
						{
							Public_Field = default,
							Internal_Field = default,
							Protected_Internal_Field = default,
							Public_Property = default,
							Internal_Property = default,
							Protected_Internal_Property = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Theory]
		[MemberData(nameof(TestData_AccessModifiers_AssignsAccessibleMembers))]
		public async Task ComputeRefactoringsAsync_AccessModifiersExternal_AssignsAccessibleMembers(string displayName, string externalCode, string initialCode, string expectedCode)
		{
			displayName.Should().NotBeNullOrWhiteSpace();
			await VerifyAsync(initialCode, expectedCode, new string[][] { new[] { externalCode } });
		}

		public static TheoryData<string, string, string, string> TestData_AccessModifiers_AssignsAccessibleMembers()
		{
			var Type_Fields =
				@"namespace F0.Analyzers.Tests
				{
					public class AccessModifiers
					{
						public int Public;
						protected int Protected;
						internal int Internal;
						protected internal int Protected_Internal;
						private int Private;
						private protected int Private_Protected;
					}
				}";

			var Type_Properties =
				@"namespace F0.Analyzers.Tests
				{
					public class AccessModifiers
					{
						public int Public { get; set; }
						protected int Protected { get; set; }
						internal int Internal { get; set; }
						protected internal int Protected_Internal { get; set; }
						private int Private { get; set; }
						private protected int Private_Protected { get; set; }
					}
				}";

			var markup =
				@"using System;
				using F0.Analyzers.Tests;

				class C
				{
					void Test()
					{
						var accessibility = [|new AccessModifiers()|];
					}
				}";

			var ObjectInitializer_InternalsInaccessible =
				@"using System;
				using F0.Analyzers.Tests;

				class C
				{
					void Test()
					{
						var accessibility = new AccessModifiers()
						{
							Public = default
						};
					}
				}";

			var ObjectInitializer_InternalsAccessible =
				@"using System;
				using F0.Analyzers.Tests;

				class C
				{
					void Test()
					{
						var accessibility = new AccessModifiers()
						{
							Public = default,
							Internal = default,
							Protected_Internal = default
						};
					}
				}";

			return new TheoryData<string, string, string, string>
			{
				{ "Fields no internals", Type_Fields, markup, ObjectInitializer_InternalsInaccessible },
				{ "Properties no internals", Type_Properties, markup, ObjectInitializer_InternalsInaccessible },
				{ "Fields with internals", WithFriend(Type_Fields), markup, ObjectInitializer_InternalsAccessible },
				{ "Properties with internals", WithFriend(Type_Properties), markup, ObjectInitializer_InternalsAccessible },
			};

			static string WithFriend(string code)
			{
				var Friend =
					$@"using System.Runtime.CompilerServices;
					[assembly: InternalsVisibleTo(""{Projects.AssemblyName}"")]";
				return Friend + Environment.NewLine + Environment.NewLine + code;
			}
		}
	}
}
