using F0.Extensions;

namespace F0.Tests.Extensions
{
	public class EnumerableExtensionsTests
	{
		[Fact]
		public void SoleOrDefault_SequenceContainsNoElements_ReturnsNull()
		{
			// Arrange
			var sequence = Enumerable.Empty<object>();

			// Act
			var result = sequence.SoleOrDefault();

			// Assert
			result.Should().BeNull();
		}

		[Fact]
		public void SoleOrDefault_SequenceContainsOneSingleElement_ReturnsTheOnlyElement()
		{
			// Arrange
			var element = new object();
			var sequence = new[] { element };

			// Act
			var result = sequence.SoleOrDefault();

			// Assert
			result.Should().BeSameAs(element);
		}

		[Fact]
		public void SoleOrDefault_SequenceContainsMoreThanOneElement_ReturnsNull()
		{
			// Arrange
			var sequence = new[] { new object(), new object() };

			// Act
			var result = sequence.SoleOrDefault();

			// Assert
			result.Should().BeNull();
		}

		[Fact]
		public void SoleOrDefaultWithPredicate_SequenceContainsNoMatch_ReturnsNull()
		{
			// Arrange
			var sequence = new[] { "bowl", "of", "petunias" };

			// Act
			var result = sequence.SoleOrDefault(x => x == "42");

			// Assert
			result.Should().BeNull();
		}

		[Fact]
		public void SoleOrDefaultWithPredicate_SequenceContainsOneSingleMatch_ReturnsTheOnlyMatch()
		{
			// Arrange
			var element = "bowl";
			var sequence = new[] { element, "of", "petunias" };

			// Act
			var result = sequence.SoleOrDefault(x => x == "bowl");

			// Assert
			result.Should().BeSameAs(element);
		}

		[Fact]
		public void SoleOrDefaultWithPredicate_SequenceContainsMoreThanOneMatch_ReturnsNull()
		{
			// Arrange
			var sequence = new[] { "bowl", "of", "petunias" };

			// Act
			var result = sequence.SoleOrDefault(x => x.Contains('o'));

			// Assert
			result.Should().BeNull();
		}
	}
}
