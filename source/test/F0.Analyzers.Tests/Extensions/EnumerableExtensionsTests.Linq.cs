using System.Linq;
using F0.Extensions;
using FluentAssertions;
using Xunit;

namespace F0.Tests.Extensions
{
	public class EnumerableExtensionsTests
	{
		[Fact]
		public void SoleOrDefault_SequenceContainsNoElements_ReturnsNull()
		{
			var sequence = Enumerable.Empty<object>();

			var result = sequence.SoleOrDefault();

			result.Should().BeNull();
		}

		[Fact]
		public void SoleOrDefault_SequenceContainsOneSingleElement_ReturnsTheOnlyElement()
		{
			var element = new object();
			var sequence = new[] { element };

			var result = sequence.SoleOrDefault();

			result.Should().BeSameAs(element);
		}

		[Fact]
		public void SoleOrDefault_SequenceContainsMoreThanOneElement_ReturnsNull()
		{
			var sequence = new[] { new object(), new object() };

			var result = sequence.SoleOrDefault();

			result.Should().BeNull();
		}
	}
}
