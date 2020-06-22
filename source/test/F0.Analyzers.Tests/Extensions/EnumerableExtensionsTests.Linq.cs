using System.Linq;
using F0.Extensions;
using Xunit;

namespace F0.Tests.Extensions
{
	public class EnumerableExtensionsTests
	{
		[Fact]
		public void SoleOrDefault_SequenceContainsNoElements_ReturnsNull()
		{
			var sequence = Enumerable.Empty<string>();

			var result = sequence.SoleOrDefault();

			Assert.Null(result);
		}

		[Fact]
		public void SoleOrDefault_SequenceContainsOneSingleElement_ReturnsTheOnlyElement()
		{
			var element = "one";
			var sequence = new[] { element };

			var result = sequence.SoleOrDefault();

			Assert.Same(element, result);
		}

		[Fact]
		public void SoleOrDefault_SequenceContainsMoreThanOneElement_ReturnsNull()
		{
			var sequence = new string[] { "one", "two" };

			var result = sequence.SoleOrDefault();

			Assert.Null(result);
		}
	}
}
