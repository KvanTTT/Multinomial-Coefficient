using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Facet.Combinatorics;
using System.Numerics;

namespace Multinominal
{
	class Program
	{
		static void TestBigNumberMultinominal()
		{
			const uint maxNumber = 1000;

			var ar = new uint[maxNumber];
			for (uint i = 0; i < ar.Length; i++)
				ar[i] = i + 1;

			int iter = 2;

			BigInteger result;
			BigInteger prevResult = 0;
			IList<uint> prevComb;
			int count = 0;
			do
			{
				var combinations = new Combinations<uint>(ar, iter, GenerateOption.WithRepetition);

				prevComb = null;
				var enumerator = combinations.GetEnumerator();
				bool binomIsCorrect = true;
				bool logIsCorrect = true;
				bool optIsCorrect = true;
				bool lnGammaIsCorrect = true;
				while (enumerator.MoveNext())
				{
					result = Multinominal.BigAr(enumerator.Current.ToArray());
					if (result <= ulong.MaxValue)
					{
						count++;
						prevResult = result;
						prevComb = enumerator.Current;

						MultinomCoefMethodTest(enumerator.Current.ToArray(), Multinominal.BinomAr, ref binomIsCorrect, (ulong)result, 0);
						MultinomCoefMethodTest(enumerator.Current.ToArray(), Multinominal.LogAr, ref logIsCorrect, (ulong)result, 0);
						MultinomCoefMethodTest(enumerator.Current.ToArray(), Multinominal.LogGammaAr, ref lnGammaIsCorrect, (ulong)result, 0);
						MultinomCoefMethodTest(enumerator.Current.ToArray(), Multinominal.MyAr, ref optIsCorrect, (ulong)result, 0);
					}
					else
					{
						if (prevComb != null)
							Console.WriteLine(string.Format("args: ({0}); max value = {1}", string.Join(",", prevComb.ToArray()), prevResult));
						Console.WriteLine();
						break;
					}
				}
				iter++;
			}
			while (prevComb != null);
			Console.WriteLine("Count: {0}", count);
		}

		static void MultinomCoefMethodTest(uint[] args, Func<uint[], ulong> method, ref bool methodIsCorrect, ulong correctResult, ulong maxError)
		{
			if (methodIsCorrect)
			{
				ulong temp = 0;
				try
				{
					temp = 0;
					temp = method(args);
					if (temp - correctResult > maxError)
						throw new Exception();
				}
				catch
				{
					if (temp != 0)
						Console.WriteLine(string.Format("{0} failed at: args ({1}); value = {2} real value = {3}", method.Method.Name, string.Join(",", args), temp, correctResult));
					else
						Console.WriteLine(string.Format("{0} failed at: args ({1}); real value = {2}", method.Method.Name, string.Join(",", args), correctResult));
					methodIsCorrect = false;
				}
			}
		}

		static void TestMultinominal(Func<uint[], ulong> func)
		{
			const uint maxNumber = 1000;

			var ar = new uint[maxNumber];
			for (uint i = 0; i < ar.Length; i++)
				ar[i] = i + 1;
			
			int iter = 2;

			ulong result;
			ulong prevResult = 0;
			IList<uint> prevComb;
			do
			{
				var combinations = new Combinations<uint>(ar, iter, GenerateOption.WithRepetition);

				prevComb = null;
				var enumerator = combinations.GetEnumerator();
				while (enumerator.MoveNext())
				{
					try
					{
						result = func(enumerator.Current.ToArray());
						prevResult = result;
						prevComb = enumerator.Current;
					}
					catch (Exception ex)
					{
						if (prevComb != null)
							Console.WriteLine(string.Format("args: ({0}),{1}value = {2}", string.Join(",", prevComb.ToArray()), Environment.NewLine, prevResult));
						break;
					}
				}
				iter++;
			}
			while (prevComb != null);
		}

		static void Main(string[] args)
		{
			Console.WriteLine("BigNumbers: ");
			TestBigNumberMultinominal();
			Console.WriteLine();

		/*	Console.WriteLine("Binominal: ");
			TestMultinominal(Multinominal.MultinominalBinomAr);
			Console.WriteLine();

			Console.WriteLine("Logarithm: ");
			TestMultinominal(Multinominal.MultinominalLogAr);
			Console.WriteLine();

			Console.WriteLine("Opt: ");
			TestMultinominal(Multinominal.MutinomonalOptAr);
			Console.WriteLine();*/

			Console.ReadLine();
		}
	}
}
