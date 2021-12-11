using F0.CodeAnalysis;

namespace F0.Tests.CodeAnalysis
{
	public class SymbolNameComparerTests
	{
		[Fact]
		public void Instance_AccessTwice_ReturnsSameSingletonInstance()
		{
			// Arrange
			IEqualityComparer<ISymbol> first;
			IEqualityComparer<ISymbol> second;

			// Act
			first = SymbolNameComparer.Instance;
			second = SymbolNameComparer.Instance;

			// Assert
			first.Should().BeSameAs(second);
		}

		[Fact]
		public void Equals_EqualSymbolNames_True()
		{
			// Arrange
			var comparer = SymbolNameComparer.Instance;

			var left = A.Fake<ISymbol>(o => o.Strict());
			var right = A.Fake<ISymbol>(o => o.Strict());
			A.CallTo(() => left.Name).Returns("SymbolName");
			A.CallTo(() => right.Name).Returns("SymbolName");

			// Act
			var areEqual = comparer.Equals(left, right);

			// Assert
			areEqual.Should().BeTrue();
			A.CallTo(() => left.Name).MustHaveHappenedOnceExactly();
			A.CallTo(() => right.Name).MustHaveHappenedOnceExactly();
		}

		[Fact]
		public void Equals_NotEqualSymbolNames_False()
		{
			// Arrange
			var comparer = SymbolNameComparer.Instance;

			var left = A.Fake<ISymbol>(o => o.Strict());
			var right = A.Fake<ISymbol>(o => o.Strict());
			A.CallTo(() => left.Name).Returns("SymbolName");
			A.CallTo(() => right.Name).Returns("symbolName");

			// Act
			var areEqual = comparer.Equals(left, right);

			// Assert
			areEqual.Should().BeFalse();
			A.CallTo(() => left.Name).MustHaveHappenedOnceExactly();
			A.CallTo(() => right.Name).MustHaveHappenedOnceExactly();
		}

		[Fact]
		public void GetHashCode_CompareToHashCodeForSymbolName_AreEqual()
		{
			// Arrange
			var comparer = SymbolNameComparer.Instance;

			var name = "SymbolName";
			var symbol = A.Fake<ISymbol>(o => o.Strict());
			A.CallTo(() => symbol.Name).Returns(name);

			// Act
			var hashCode = comparer.GetHashCode(symbol);

			// Assert
			hashCode.Should().Be(name.GetHashCode());
			A.CallTo(() => symbol.Name).MustHaveHappenedOnceExactly();
		}
	}
}
