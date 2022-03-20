using System;

namespace F0.Analyzers.Example.Tests.Services
{
	internal sealed class PrimeService
	{
		public bool IsPrime(int candidate)
		{
			if (candidate < 2)
			{
				return false;
			}

			throw new NotImplementedException("Please create a test first.");
		}
	}
}
