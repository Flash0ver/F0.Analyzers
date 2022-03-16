using F0.Extensions;

namespace F0.Tests.Extensions;

public class StringExtensionsTests
{
	[Fact]
	public void EqualsOrdinal_EqualStrings_True()
	{
		// Arrange
		var left = "Straße";
		var right = "Straße";

		// Act
		var equals = left.EqualsOrdinal(right);

		// Assert
		equals.Should().BeTrue();
	}

	[Fact]
	public void EqualsOrdinal_NotEqualStrings_False()
	{
		// Arrange
		var left = "Straße";
		var right = "Strasse";

		// Act
		var equals = left.EqualsOrdinal(right);

		// Assert
		equals.Should().BeFalse();
	}

	[Theory]
	[InlineData("Straße", "Straße")]
	[InlineData("Straßenname", "Straße")]
	public void StartsWithOrdinal_EqualStrings_True(string left, string right)
	{
		// Arrange

		// Act
		var equals = left.StartsWithOrdinal(right);

		// Assert
		equals.Should().BeTrue();
	}

	[Theory]
	[InlineData("Strasse", "Straße")]
	[InlineData("Strassenname", "Straße")]
	[InlineData("die Straße", "Straße")]
	[InlineData("die Strasse", "Straße")]
	public void StartsWithOrdinal_NotEqualStrings_False(string left, string right)
	{
		// Arrange

		// Act
		var equals = left.StartsWithOrdinal(right);

		// Assert
		equals.Should().BeFalse();
	}
}
