using System.Diagnostics.CodeAnalysis;
using F0.Analyzers.Example.Tests.Services;
using NUnit.Framework;

namespace F0.Analyzers.Example.Tests.CodeAnalysis.Suppressors
{
	[TestFixture]
	[SuppressMessage("Style", "IDE0022:Use expression body for methods", Justification = "Example")]
	public class NUnit
	{
		private PrimeService primeService;

		[SetUp]
		public void SetUp()
		{
			primeService = new PrimeService();
		}

		[Test]
		public void IsPrime_InputIs1_ReturnFalse()
		{
			var result = primeService.IsPrime(1);

			Assert.IsFalse(result, "1 should not be prime");
		}

		[TestCase(-1)]
		[TestCase(0)]
		[TestCase(1)]
		public void IsPrime_ValuesLessThan2_ReturnFalse(int value)
		{
			var result = primeService.IsPrime(value);

			Assert.IsFalse(result, $"{value} should not be prime");
		}

		[TestCaseSource(nameof(TestCaseSource))]
		public void TestCaseSource_Attribute(int value)
		{
			var result = primeService.IsPrime(value);

			Assert.IsFalse(result, $"{value} should not be prime");
		}

		private static int[] TestCaseSource => new int[] { -2, -3 };

		[Combinatorial]
		public void Combinatorial_Attribute([Values(1, 2)] int number, [Values("A", "B")] string text)
		{
			var actual = number + text;

			Assert.That(actual, Is.AnyOf("1A", "1B", "2A", "2B"));
		}

		[Pairwise]
		public void Pairwise_Attribute([Values("a", "b", "c")] string left, [Values("+", "-")] string middle, [Values("x", "y")] string right)
		{
			var actual = left + middle + right;

			Assert.That(actual, Is.AnyOf("a-x", "a+y", "b+x", "b-y", "c-x", "c+y"));
		}

		[Sequential]
		public void Sequential_Attribute([Values(1, 2, 3)] int number, [Values("A", "B")] string text)
		{
			var actual = number + text;

			Assert.That(actual, Is.AnyOf("1A", "2B", "3"));
		}

		[Theory]
		public void Theory_Attribute(int number)
		{
			Assume.That(number != 0);

			var @delegate = () => 0 / number;

			Assert.That(@delegate, Throws.Nothing);
		}

		[DatapointSource]
		public int[] DatapointSource => new int[] { -1, 0, 1 };
	}
}
