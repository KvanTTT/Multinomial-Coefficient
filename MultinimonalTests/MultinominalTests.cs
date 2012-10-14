using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MultinimonalTests
{
	[TestFixture]
	public class MultinominalTests
	{
		[Test]
		public void Test()
		{
			var argsSets = new List<uint[]>()
			{
				new uint[] { 1, 1 },
				new uint[] { 1, 2 },
				new uint[] { 2, 1 },
				new uint[] { 2, 2 },
				new uint[] { 10, 10 },
				new uint[] { 5, 5, 5 },
				new uint[] { 10, 10, 10 },
				new uint[] { 5, 10, 15 },
				new uint[] { 6, 6, 6, 6 },
				new uint[] { 5, 6, 7, 8 },
				new uint[] { 2, 3, 4, 5, 7 },
				new uint[] { 1, 2, 1, 2, 1, 2, 1, 2 },
			};

			var methods = new List<Func<uint[], ulong>>()
			{
				Multinominal.Multinominal.BinomAr,
				Multinominal.Multinominal.LogAr,
				Multinominal.Multinominal.LogGammaAr,
				Multinominal.Multinominal.MyAr
			};

			ulong maxError = 0;

			foreach (var argSet in argsSets)
			{
				var bigNumbersResult = (ulong)Multinominal.Multinominal.BigAr(argSet);
				foreach (var method in methods)
					Assert.LessOrEqual(Multinominal.Multinominal.Diff(bigNumbersResult, method(argSet)), maxError);
			}
		}
	}
}
