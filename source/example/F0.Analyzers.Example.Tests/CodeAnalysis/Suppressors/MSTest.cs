using F0.Analyzers.Example.Tests.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace F0.Analyzers.Example.Tests.CodeAnalysis.Suppressors
{
	[TestClass]
	public class MSTest
	{
		private readonly PrimeService primeService;

		public MSTest()
		{
			primeService = new PrimeService();
		}

		[TestMethod]
		public void IsPrime_InputIs1_ReturnFalse()
		{
			var result = primeService.IsPrime(1);

			Assert.IsFalse(result, "1 should not be prime");
		}

		[DataTestMethod]
		[DataRow(-1)]
		[DataRow(0)]
		[DataRow(1)]
		public void IsPrime_ValuesLessThan2_ReturnFalse(int value)
		{
			var result = primeService.IsPrime(value);

			Assert.IsFalse(result, $"{value} should not be prime");
		}
	}
}
