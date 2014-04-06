using Multinomial;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultinomialTests
{
	[TestFixture]
	public class CombinationWithRepetitionTests
	{
		[Test]
		public void Test()
		{
			var combs = CombinationWithRepetition.Generate(4, 3);
			var expected = new int[][]
			{
				new int[] { 0, 0, 0 },
				new int[] { 0, 0, 1 },
				new int[] { 0, 0, 2 },
				new int[] { 0, 0, 3 },
				new int[] { 0, 1, 1 },
				new int[] { 0, 1, 2 },
				new int[] { 0, 1, 3 },
				new int[] { 0, 2, 2 },
				new int[] { 0, 2, 3 },
				new int[] { 0, 3, 3 },
				new int[] { 1, 1, 1 },
				new int[] { 1, 1, 2 },
				new int[] { 1, 1, 3 },
				new int[] { 1, 2, 2 },
				new int[] { 1, 2, 3 },
				new int[] { 1, 3, 3 },
				new int[] { 2, 2, 2 },
				new int[] { 2, 2, 3 },
				new int[] { 2, 3, 3 },
				new int[] { 3, 3, 3 }
			};

			for (int i = 0; i < expected.Length; i++)
				CollectionAssert.AreEqual(expected[i], combs[i]);
		}

		[Test]
		public void GenerateSumands()
		{
			var result = CombinationWithRepetition.GenerateSummands(4, 4);
			var expectedCoefs = new ulong[] { 1, 4, 6, 12, 24 };
			foreach (var comb in result)
				CollectionAssert.Contains(expectedCoefs, comb.Coefficient);
		}
	}
}
