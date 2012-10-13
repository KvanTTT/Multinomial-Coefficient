﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;


namespace Multinominal
{
	public static class Multinominal
	{
		#region Naive

		public static ulong Mutinomonal(params uint[] numbers)
		{
			return MutinomonalAr(numbers);
		}

		public static ulong MutinomonalAr(params uint[] numbers)
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

		public static BigInteger Big(params uint[] numbers)
		{
			return BigAr(numbers);
		}

		public static BigInteger BigAr(uint[] numbers)
		{
			BigInteger numbersSum = 0;
			foreach (var number in numbers)
				numbersSum += number;

			BigInteger nominator = Factorial(numbersSum);

			BigInteger denominator = 1;
			foreach (var number in numbers)
				denominator *= Factorial(new BigInteger(number));

			return nominator / denominator;
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
			var denomFactorPowers = new uint[numbers.Max() + 1];
			foreach (var number in numbers)
				for (int i = 2; i <= number; i++)
					denomFactorPowers[i]++;
			for (int i = 2; i < denomFactorPowers.Length; i++)
				denomFactorPowers[i]--; // reduce with niminator;

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
			if (numbers.Length == 1)
				return 1;

			ulong result = 1;
			uint sum = numbers[0];

			for (int i = 1; i < numbers.Length; i++)
			{
				sum += numbers[i];
				result *= Binominal(sum, numbers[i]);
			}

			return result;
		}

		public static ulong Binominal(ulong n, ulong k)
		{
			ulong r = 1;
			ulong d;
			if (k > n) return 0;
			for (d = 1; d <= k; d++)
			{
				r *= n--;
				r /= d;
			}
			return r;
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
					value = lngamma(number + 1);
					LnFacts.Add(number, value);
					denom += value;
				}
			}

			double numer;
			if (LnFacts.TryGetValue(numbersSum, out value))
				numer = value;
			else
			{
				value = lngamma(numbersSum + 1);
				LnFacts.Add(numbersSum, value);
				numer = value;
			}

			return (ulong)Math.Round(Math.Exp(numer - denom));
		}

		public static double lngamma(double x)
		{
			double sign = 1;
			return lngamma(x, ref sign);
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
		public static double lngamma(double x, ref double sgngam)
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
				w = lngamma(q, ref tmp);
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

		public static double GammaLn(double z)
		{
			double result;
			if (z < 0.5)
			{
				double s = Gamma_dk[0];
				for (int i = 1; i <= 10; i++)
				{
					s += Gamma_dk[i] / ((double)i - z);
				}
				result = 1.1447298858494002 - Math.Log(Math.Sin(3.1415926535897931 * z)) - Math.Log(s) - 0.6207822376352452 - (0.5 - z) * Math.Log((0.5 - z + 10.900511) / 2.7182818284590451);
			}
			else
			{
				double s = Gamma_dk[0];
				for (int i = 1; i <= 10; i++)
				{
					s += Gamma_dk[i] / (z + (double)i - 1.0);
				}
				result = Math.Log(s) + 0.6207822376352452 + (z - 0.5) * Math.Log((z - 0.5 + 10.900511) / 2.7182818284590451);
			}
			return result;
		}

		static double[] Gamma_dk = new double[]
		{
			2.4857408913875355E-05,
			1.0514237858172197,
			-3.4568709722201625,
			4.5122770946689483,
			-2.9828522532357664,
			1.056397115771267,
			-0.19542877319164587,
			0.017097054340444121,
			-0.00057192611740430573,
			4.6339947335990567E-06,
			-2.7199490848860772E-09
		};
	}
}