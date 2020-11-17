using System.Threading.Tasks;
using Xunit;

namespace F0.Tests.CodeAnalysis.CodeRefactorings
{
	public partial class ObjectInitializerTests
	{
		[Fact]
		public async Task ComputeRefactoringsAsync_ValueType_StructsDoNotSupportInheritanceButTheyCanImplementInterfaces()
		{
			var initialCode =
				@"using System;

				interface Interface { int Property { get; set; } }
				struct Struct : Interface { public int Property { get; set; } }

				class C
				{
					void Test()
					{
						var instance = [|new Struct()|];
					}
				}";

			var expectedCode =
				@"using System;

				interface Interface { int Property { get; set; } }
				struct Struct : Interface { public int Property { get; set; } }

				class C
				{
					void Test()
					{
						var instance = new Struct()
						{
							Property = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_ClassImplementsInterfaceImplicitly_AssignsImplementedMembers()
		{
			var initialCode =
				@"using System;

				interface Interface { int Property { get; set; } }
				class Class : Interface { public int Property { get; set; } }

				class C
				{
					void Test()
					{
						var instance = [|new Class()|];
					}
				}";

			var expectedCode =
				@"using System;

				interface Interface { int Property { get; set; } }
				class Class : Interface { public int Property { get; set; } }

				class C
				{
					void Test()
					{
						var instance = new Class()
						{
							Property = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_ClassImplementsInterfaceExplicitly_AnExplicitlyImplementedMemberCannotBeAccessedThroughTheClassInstance()
		{
			var initialCode =
				@"using System;

				interface Interface { int Property { get; set; } }
				class Class : Interface { private int field; int Interface.Property { get => field; set => field = value; } }

				class C
				{
					void Test()
					{
						var instance = [|new Class()|];
					}
				}";

			var expectedCode =
				@"using System;

				interface Interface { int Property { get; set; } }
				class Class : Interface { private int field; int Interface.Property { get => field; set => field = value; } }

				class C
				{
					void Test()
					{
						var instance = new Class() { };
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_ClassImplementsInterfaceWithoutSetAccessor_InaccessibleMembersAreNotAssigned()
		{
			var initialCode =
				@"using System;

				interface Interface { int Property { get; } }
				class Class : Interface { public int Property { get; } }

				class C
				{
					void Test()
					{
						var instance = [|new Class()|];
					}
				}";

			var expectedCode =
				@"using System;

				interface Interface { int Property { get; } }
				class Class : Interface { public int Property { get; } }

				class C
				{
					void Test()
					{
						var instance = new Class() { };
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_ClassImplementsInterfaceWithSetAccessor_AccessibleMembersAreAssigned()
		{
			var initialCode =
				@"using System;

				interface Interface { int Property { get; } }
				class Class : Interface { public int Property { get; set; } }

				class C
				{
					void Test()
					{
						var instance = [|new Class()|];
					}
				}";

			var expectedCode =
				@"using System;

				interface Interface { int Property { get; } }
				class Class : Interface { public int Property { get; set; } }

				class C
				{
					void Test()
					{
						var instance = new Class()
						{
							Property = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_NoAccessibleMembersInherited_NoMemberAssignments()
		{
			var initialCode =
				@"using System;

				class Base { public int Property { get; } }
				class Derived : Base { }

				class C
				{
					void Test()
					{
						var instance = [|new Derived()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Base { public int Property { get; } }
				class Derived : Base { }

				class C
				{
					void Test()
					{
						var instance = new Derived() { };
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_AccessibleMembersInherited_MembersAreAssigned()
		{
			var initialCode =
				@"using System;

				class Base { public int Property { get; set; } }
				class Derived : Base { }

				class C
				{
					void Test()
					{
						var instance = [|new Derived()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Base { public int Property { get; set; } }
				class Derived : Base { }

				class C
				{
					void Test()
					{
						var instance = new Derived()
						{
							Property = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_NoBaseMembersDeclared_AssignsOnlyDeclaredMembers()
		{
			var initialCode =
				@"using System;

				class Base { }
				class Derived : Base { public int Property { get; set; } }

				class C
				{
					void Test()
					{
						var instance = [|new Derived()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Base { }
				class Derived : Base { public int Property { get; set; } }

				class C
				{
					void Test()
					{
						var instance = new Derived()
						{
							Property = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_InheritanceIsTransitive_AssignBaseMembersBeforeDerivedMembers()
		{
			var initialCode =
				@"using System;

				class ClassA { public string Text { get; set; } }
				class ClassB : ClassA { public int Number { get; set; } }
				class ClassC : ClassB { public bool Condition { get; set; } }

				class C
				{
					void Test()
					{
						var instance = [|new ClassC()|];
					}
				}";

			var expectedCode =
				@"using System;

				class ClassA { public string Text { get; set; } }
				class ClassB : ClassA { public int Number { get; set; } }
				class ClassC : ClassB { public bool Condition { get; set; } }

				class C
				{
					void Test()
					{
						var instance = new ClassC()
						{
							Text = default,
							Number = default,
							Condition = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_DoNotOverrideVirtualMember_AssignsMember()
		{
			var initialCode =
				@"using System;

				class Base { public virtual int Property { get; set; } }
				class Derived : Base { }

				class C
				{
					void Test()
					{
						var instance = [|new Derived()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Base { public virtual int Property { get; set; } }
				class Derived : Base { }

				class C
				{
					void Test()
					{
						var instance = new Derived()
						{
							Property = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_OverrideVirtualMember_AssignsMember()
		{
			var initialCode =
				@"using System;

				class Base { public virtual int Property { get; set; } }
				class Derived : Base { public override int Property { get => base.Property; set => base.Property = value; } }

				class C
				{
					void Test()
					{
						var instance = [|new Derived()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Base { public virtual int Property { get; set; } }
				class Derived : Base { public override int Property { get => base.Property; set => base.Property = value; } }

				class C
				{
					void Test()
					{
						var instance = new Derived()
						{
							Property = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_MustOverrideAbstractMember_AssignsMember()
		{
			var initialCode =
				@"using System;

				abstract class Base { public abstract int Property { get; set; } }
				sealed class Derived : Base { public override int Property { get; set; } }

				class C
				{
					void Test()
					{
						var instance = [|new Derived()|];
					}
				}";

			var expectedCode =
				@"using System;

				abstract class Base { public abstract int Property { get; set; } }
				sealed class Derived : Base { public override int Property { get; set; } }

				class C
				{
					void Test()
					{
						var instance = new Derived()
						{
							Property = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_HideInheritedMemberWithoutSetAccessor_InaccessibleMembersAreNotAssigned()
		{
			var initialCode =
				@"using System;

				class Base { public int Property { get; set; } }
				class Derived : Base { public new int Property { get; } }

				class C
				{
					void Test()
					{
						var instance = [|new Derived()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Base { public int Property { get; set; } }
				class Derived : Base { public new int Property { get; } }

				class C
				{
					void Test()
					{
						var instance = new Derived() { };
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_HideInheritedMemberWithSetAccessor_AccessibleMembersAreAssigned()
		{
			var initialCode =
				@"using System;

				class Base { public int Property { get; } }
				class Derived : Base { public new int Property { get; set; } }

				class C
				{
					void Test()
					{
						var instance = [|new Derived()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Base { public int Property { get; } }
				class Derived : Base { public new int Property { get; set; } }

				class C
				{
					void Test()
					{
						var instance = new Derived()
						{
							Property = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_Order_AssignDerivedMembersAfterBaseMembers()
		{
			var initialCode =
				@"using System;

				interface Interface
				{
					int PropertyI { get; set; }
				}
				abstract class ClassA : Interface
				{
					public int FieldA;
					public int PropertyA { get; set; }
					public abstract int PropertyI { get; set; }
				}
				class ClassB : ClassA
				{
					public int FieldB;
					public int PropertyB { get; set; }
					public new int PropertyA { get; set; }
					public override int PropertyI { get; set; }
				}
				sealed class ClassC : ClassB
				{
					public int FieldC;
					public int PropertyC { get; set; }
					public new int PropertyB { get; set; }
					public sealed override int PropertyI { get; set; }
				}

				class C
				{
					void Test()
					{
						var instance = [|new ClassC()|];
					}
				}";

			var expectedCode =
				@"using System;

				interface Interface
				{
					int PropertyI { get; set; }
				}
				abstract class ClassA : Interface
				{
					public int FieldA;
					public int PropertyA { get; set; }
					public abstract int PropertyI { get; set; }
				}
				class ClassB : ClassA
				{
					public int FieldB;
					public int PropertyB { get; set; }
					public new int PropertyA { get; set; }
					public override int PropertyI { get; set; }
				}
				sealed class ClassC : ClassB
				{
					public int FieldC;
					public int PropertyC { get; set; }
					public new int PropertyB { get; set; }
					public sealed override int PropertyI { get; set; }
				}

				class C
				{
					void Test()
					{
						var instance = new ClassC()
						{
							FieldA = default,
							FieldB = default,
							PropertyA = default,
							FieldC = default,
							PropertyC = default,
							PropertyB = default,
							PropertyI = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode);
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_InternalBaseMembers_CannotBeReferencedOutsideTheAssemblyWithinWhichTheyWereDefined()
		{
			var externalCode =
				@"public class Base
				{
					internal string BaseField;
					internal int BaseProperty { get; set; }
				}";

			var initialCode =
				@"using System;

				class Derived : Base
				{
					internal string DerivedField;
					internal int DerivedProperty { get; set; }
				}

				class C
				{
					void Test()
					{
						var instance = [|new Derived()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Derived : Base
				{
					internal string DerivedField;
					internal int DerivedProperty { get; set; }
				}

				class C
				{
					void Test()
					{
						var instance = new Derived()
						{
							DerivedField = default,
							DerivedProperty = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode, new string[][] { new[] { externalCode } });
		}

		[Fact]
		public async Task ComputeRefactoringsAsync_InternalBaseMembers_AreVisibleToFriendAssemblies()
		{
			var externalCode =
				@"using System.Runtime.CompilerServices;
				[assembly: InternalsVisibleTo(""TestProject0"")]

				public class Base
				{
					internal string BaseField;
					internal int BaseProperty { get; set; }
				}";

			var initialCode =
				@"using System;

				class Derived : Base
				{
					internal string DerivedField;
					internal int DerivedProperty { get; set; }
				}

				class C
				{
					void Test()
					{
						var instance = [|new Derived()|];
					}
				}";

			var expectedCode =
				@"using System;

				class Derived : Base
				{
					internal string DerivedField;
					internal int DerivedProperty { get; set; }
				}

				class C
				{
					void Test()
					{
						var instance = new Derived()
						{
							BaseField = default,
							BaseProperty = default,
							DerivedField = default,
							DerivedProperty = default
						};
					}
				}";

			await VerifyAsync(initialCode, expectedCode, new string[][] { new[] { externalCode } });
		}
	}
}
