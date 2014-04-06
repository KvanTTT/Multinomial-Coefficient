using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multinomial
{
	public class Summand
	{
		public ulong Coefficient;
		public uint[] Factors;

		public override string ToString()
		{
			return string.Format("Factors: {0}; Coef: {1}", string.Join(",", Factors), Coefficient);
		}
	}
}
