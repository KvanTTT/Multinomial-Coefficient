using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multinomial
{
	enum ErrorType
	{
		NoError = 0,
		Overflow = 1,
		Rounding = 2,
	}

	enum ErrorMeasure
	{
		Absolute = 0,
		Relative = 1
	}

	class MultinomialResult
	{
		public int Number;

		public uint[] Permutation;

		public uint PermutationSum
		{
			get
			{
				uint result = 0;
				foreach (var permut in Permutation)
					result += permut;
				return result;
			}
		}

		public ulong Result;

		public ErrorType ErrorType;

		public ulong Error;
	}
}
