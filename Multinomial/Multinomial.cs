using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace Multinomial
{
	public static class Multinomial
	{
		#region Naive

		public static ulong Naive(params uint[] numbers)
		{
			return NaiveAr(numbers);
		}

		public static ulong NaiveAr(params uint[] numbers)
		{
			uint numbersSum = 0;
			foreach (var number in numbers)
				numbersSum += number;

			ulong nominator = Factorial(numbersSum);

			ulong denominator = 1;
			foreach (var number in numbers)
				denominator *= Factorial(number);

			return nominator / denominator;
		}

		public static ulong Factorial(ulong n)
		{
			ulong result = 1;
			for (ulong i = 1; i <= n; i++)
				result = result * i;
			return result;
		}

		#endregion

		#region Big Integer

		public static ulong Big(params uint[] numbers)
		{
			return BigAr(numbers);
		}

		public static ulong BigAr(uint[] numbers)
		{
			BigInteger numbersSum = 0;
			foreach (var number in numbers)
				numbersSum += number;

			BigInteger nominator = Factorial(numbersSum);

			BigInteger denominator = 1;
			foreach (var number in numbers)
				denominator *= Factorial(new BigInteger(number));

			return (ulong)(nominator / denominator);
		}
		
		public static BigInteger Factorial(BigInteger n)
		{
			BigInteger result = 1;
			for (ulong i = 1; i <= n; i++)
				result = result * i;
			return result;
		}

		#endregion

		#region My Method
		
		public static ulong My(params uint[] numbers)
		{
			return MyAr(numbers);
		}

		public static ulong MyAr(uint[] numbers)
		{
			uint numbersSum = 0;
			foreach (var number in numbers)
				numbersSum += number;

			uint maxNumber = numbers.Max();
			var denomFactorPowers = new uint[maxNumber + 1];
			foreach (var number in numbers)
				for (int i = 2; i <= number; i++)
					denomFactorPowers[i]++;
			for (int i = 2; i < denomFactorPowers.Length; i++)
				denomFactorPowers[i]--; // reduce with nominator;

			uint currentFactor = 2;
			uint currentPower = 1;
			double result = 1;
			for (uint i = maxNumber + 1; i <= numbersSum; i++)
			{
				uint tempDenom = 1;
				while (tempDenom < result && currentFactor < denomFactorPowers.Length)
				{
					if (currentPower > denomFactorPowers[currentFactor])
					{
						currentFactor++;
						currentPower = 1;
					}
					else
					{
						tempDenom *= currentFactor;
						currentPower++;
					}
				}
				result = result / tempDenom * i;
			}

			return (ulong)Math.Round(result);
		}

		#endregion

		#region Decomposition on Binoms

		public static ulong Binom(params uint[] numbers)
		{
			return BinomAr(numbers);
		}

		public static ulong BinomAr(uint[] numbers)
		{
			ulong res = 1;
			uint sum = numbers[0];
			for (uint i = 1; i < numbers.Length; i++)
				for (uint j = 1; j <= numbers[i]; j++)
				{
					sum++;
					res = (res / j) * sum + (res % j) * sum / j;
				}
			return res;
		}

		#endregion

		#region Prime Numbers

		public static ulong PrimeNumbers(params uint[] numbers)
		{
			return PrimeNumbersAr(numbers);
		}

		public static ulong PrimeNumbersAr(uint[] numbers)
		{
			uint s = 0;
			foreach (uint a in numbers)
				s += a;
			ulong result = 1;
			for (uint p = 2; p <= s; p++)
			{
				bool br = false;
				for (uint q = 2; q * q <= p; q++)
					if (p % q == 0)
					{
						br = true;
						break;
					}
				if (!br)
				{
					uint pow = 0;
					for (uint h = s / p; h != 0; h /= p)
						pow += h;
					foreach (uint a in numbers)
						for (uint h = a / p; h != 0; h /= p)
							pow -= h;
					while (pow != 0)
					{
						result *= p;
						pow--;
					}
				}
			}
			return result;
		}

		#endregion
		
		#region Log Method

		static List<double> Logarithms = new List<double>();

		public static ulong Log(params uint[] numbers)
		{
			return LogAr(numbers);
		}

		public static ulong LogAr(uint[] numbers)
		{
			int maxNumber = (int)numbers.Max();

			uint numbersSum = 0;
			foreach (var number in numbers)
				numbersSum += number;

			double sum = 0;
			for (int i = 2; i <= numbersSum; i++)
			{
				if (i <= maxNumber)
				{
					if (i - 2 >= Logarithms.Count)
					{
						var log = Math.Log(i);
						Logarithms.Add(log);
					}
				}
				else
				{
					if (i - 2 < Logarithms.Count)
						sum += Logarithms[i - 2];
					else
					{
						var log = Math.Log(i);
						Logarithms.Add(log);
						sum += log;
					}
				}
			}

			var maxNumberFirst = false;
			foreach (var number in numbers)
				if (number == maxNumber && !maxNumberFirst)
					maxNumberFirst = true;
				else
					for (int i = 2; i <= number; i++)
						sum -= Logarithms[i - 2];

			return (ulong)Math.Round(Math.Exp(sum));
		}

		#endregion

		#region Log Gamma Method

		static Dictionary<uint, double> LnFacts = new Dictionary<uint, double>();

		public static ulong LogGamma(params uint[] numbers)
		{
			return LogGammaAr(numbers);
		}

		public static ulong LogGammaAr(uint[] numbers)
		{
			int maxNumber = (int)numbers.Max();
			
			double value;
			double denom = 0;
			uint numbersSum = 0;
			foreach (var number in numbers)
			{
				numbersSum += number;
				
				if (LnFacts.TryGetValue(number, out value))
					denom += value;
				else
				{
					value = LnGamma(number + 1);
					LnFacts.Add(number, value);
					denom += value;
				}
			}

			double numer;
			if (LnFacts.TryGetValue(numbersSum, out value))
				numer = value;
			else
			{
				value = LnGamma(numbersSum + 1);
				LnFacts.Add(numbersSum, value);
				numer = value;
			}

			return (ulong)Math.Round(Math.Exp(numer - denom));
		}

		public static double LnGamma(double x)
		{
			double sign = 1;
			return LnGamma(x, ref sign);
		}

		/*************************************************************************
		Natural logarithm of gamma function

		Input parameters:
			X       -   argument

		Result:
			logarithm of the absolute value of the Gamma(X).

		Output parameters:
			SgnGam  -   sign(Gamma(X))

		Domain:
			0 < X < 2.55e305
			-2.55e305 < X < 0, X is not an integer.

		ACCURACY:
		arithmetic      domain        # trials     peak         rms
		   IEEE    0, 3                 28000     5.4e-16     1.1e-16
		   IEEE    2.718, 2.556e305     40000     3.5e-16     8.3e-17
		The error criterion was relative when the function magnitude
		was greater than one but absolute when it was less than one.

		The following test used the relative error criterion, though
		at certain points the relative error could be much higher than
		indicated.
		   IEEE    -200, -4             10000     4.8e-16     1.3e-16

		Cephes Math Library Release 2.8:  June, 2000
		Copyright 1984, 1987, 1989, 1992, 2000 by Stephen L. Moshier
		Translated to AlgoPascal by Bochkanov Sergey (2005, 2006, 2007).
		*************************************************************************/
		public static double LnGamma(double x, ref double sgngam)
		{
			double result = 0;
			double a = 0;
			double b = 0;
			double c = 0;
			double p = 0;
			double q = 0;
			double u = 0;
			double w = 0;
			double z = 0;
			int i = 0;
			double logpi = 0;
			double ls2pi = 0;
			double tmp = 0;

			sgngam = 0;

			sgngam = 1;
			logpi = 1.14472988584940017414;
			ls2pi = 0.91893853320467274178;
			if ((double)(x) < (double)(-34.0))
			{
				q = -x;
				w = LnGamma(q, ref tmp);
				p = (int)Math.Floor(q);
				i = (int)Math.Round(p);
				if (i % 2 == 0)
				{
					sgngam = -1;
				}
				else
				{
					sgngam = 1;
				}
				z = q - p;
				if ((double)(z) > (double)(0.5))
				{
					p = p + 1;
					z = p - q;
				}
				z = q * Math.Sin(Math.PI * z);
				result = logpi - Math.Log(z) - w;
				return result;
			}
			if ((double)(x) < (double)(13))
			{
				z = 1;
				p = 0;
				u = x;
				while ((double)(u) >= (double)(3))
				{
					p = p - 1;
					u = x + p;
					z = z * u;
				}
				while ((double)(u) < (double)(2))
				{
					z = z / u;
					p = p + 1;
					u = x + p;
				}
				if ((double)(z) < (double)(0))
				{
					sgngam = -1;
					z = -z;
				}
				else
				{
					sgngam = 1;
				}
				if ((double)(u) == (double)(2))
				{
					result = Math.Log(z);
					return result;
				}
				p = p - 2;
				x = x + p;
				b = -1378.25152569120859100;
				b = -38801.6315134637840924 + x * b;
				b = -331612.992738871184744 + x * b;
				b = -1162370.97492762307383 + x * b;
				b = -1721737.00820839662146 + x * b;
				b = -853555.664245765465627 + x * b;
				c = 1;
				c = -351.815701436523470549 + x * c;
				c = -17064.2106651881159223 + x * c;
				c = -220528.590553854454839 + x * c;
				c = -1139334.44367982507207 + x * c;
				c = -2532523.07177582951285 + x * c;
				c = -2018891.41433532773231 + x * c;
				p = x * b / c;
				result = Math.Log(z) + p;
				return result;
			}
			q = (x - 0.5) * Math.Log(x) - x + ls2pi;
			if ((double)(x) > (double)(100000000))
			{
				result = q;
				return result;
			}
			p = 1 / (x * x);
			if ((double)(x) >= (double)(1000.0))
			{
				q = q + ((7.9365079365079365079365 * 0.0001 * p - 2.7777777777777777777778 * 0.001) * p + 0.0833333333333333333333) / x;
			}
			else
			{
				a = 8.11614167470508450300 * 0.0001;
				a = -(5.95061904284301438324 * 0.0001) + p * a;
				a = 7.93650340457716943945 * 0.0001 + p * a;
				a = -(2.77777777730099687205 * 0.001) + p * a;
				a = 8.33333333333331927722 * 0.01 + p * a;
				q = q + a / x;
			}
			result = q;
			return result;
		}

		#endregion

		public static ulong Diff(ulong number1, ulong number2)
		{
			return number1 >= number2 ? number1 - number2 : number2 - number1;
		}

		public static void ClearCache()
		{
			Logarithms = new List<double>();
			LnFacts = new Dictionary<uint, double>();
		}
	}
}
