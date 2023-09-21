using System.Diagnostics.CodeAnalysis;
using F0.Analyzers.Example.Tests.Services;
using Xunit;

namespace F0.Analyzers.Example.Tests.CodeAnalysis.Suppressors
{
	[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "xUnit.net")]
	public class xUnit
	{
		private readonly PrimeService primeService;

		public xUnit()
		{
			primeService = new PrimeService();
		}

		[Fact]
		public void IsPrime_InputIs1_ReturnFalse()
		{
			var result = primeService.IsPrime(1);

			Assert.False(result, "1 should not be prime");
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(0)]
		[InlineData(1)]
		public void IsPrime_ValuesLessThan2_ReturnFalse(int value)
		{
			var result = primeService.IsPrime(value);

			Assert.False(result, $"{value} should not be prime");
		}
	}
}
