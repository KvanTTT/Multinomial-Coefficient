using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Facet.Combinatorics;
using System.Numerics;
using System.Diagnostics;

namespace Multinominal
{
	class Program
	{
		static void Main(string[] args)
		{
			TestBigNumberMultinominal();

			Console.WriteLine();

			PerformanceTest();

			Console.ReadLine();
		}

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
					var curPermut = enumerator.Current.ToArray();

					result = Multinominal.BigAr(curPermut);
					if (result <= ulong.MaxValue)
					{
						count++;
						prevResult = result;
						prevComb = enumerator.Current;

						MultinomCoefMethodTest(curPermut, Multinominal.BinomAr, ref binomIsCorrect, (ulong)result, 0);
						MultinomCoefMethodTest(curPermut, Multinominal.LogAr, ref logIsCorrect, (ulong)result, 0);
						MultinomCoefMethodTest(curPermut, Multinominal.LogGammaAr, ref lnGammaIsCorrect, (ulong)result, 0);
						MultinomCoefMethodTest(curPermut, Multinominal.MyAr, ref optIsCorrect, (ulong)result, 0);
					}
					else
					{
						if (prevComb != null)
						{
							var prevPermut = prevComb.ToArray();
							var prevResultULong = (ulong)prevResult;
							PrintIfCorrect(Multinominal.BinomAr, prevPermut, prevResultULong, binomIsCorrect);
							PrintIfCorrect(Multinominal.LogAr, prevPermut, prevResultULong, logIsCorrect);
							PrintIfCorrect(Multinominal.LogGammaAr, prevPermut, prevResultULong, lnGammaIsCorrect);
							PrintIfCorrect(Multinominal.MyAr, prevPermut, prevResultULong, optIsCorrect);

							Console.WriteLine(string.Format("BigNumber({0}) = {1}", string.Join(",", prevPermut), prevResult));
						}
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
					if (Multinominal.Diff(temp, correctResult) > maxError)
						throw new Exception();
				}
				catch
				{
					if (temp != 0)
						Console.WriteLine(string.Format("{0}({1}) = {2}; error = {3}", 
							method.Method.Name, string.Join(",", args), temp, Multinominal.Diff(temp, correctResult)));
					else
						Console.WriteLine(string.Format("{0}({1}); error = {2}", 
							method.Method.Name, string.Join(",", args), correctResult));

					methodIsCorrect = false;
				}
			}
		}

		static void PrintIfCorrect(Func<uint[], ulong> method, uint[] args, ulong result, bool methodIsCorrect)
		{
			if (methodIsCorrect)
				Console.WriteLine(string.Format("{0}({1}) = {2}", method.Method.Name, string.Join(",", args), result));
		}

		static void PerformanceTest()
		{
			var argsSets = new List<uint[]>()
			{
				new uint[] { 1, 1 },
				new uint[] { 1, 2 },
				new uint[] { 2, 1 },
				new uint[] { 2, 2 },
				new uint[] { 5, 5, 5 },
				new uint[] { 10, 10, 10 },
				new uint[] { 5, 10, 15 },
				new uint[] { 6, 6, 6, 6 },
				new uint[] { 5, 6, 7, 8 },
				new uint[] { 2, 3, 4, 5, 7 },

			};

			var methods = new List<Func<uint[], ulong>>()
			{
				Multinominal.BinomAr,
				Multinominal.LogAr,
				Multinominal.LogGammaAr,
				Multinominal.MyAr
			};

			List<ulong> results;
			Stopwatch watch;
			foreach (var method in methods)
			{
				results = new List<ulong>();
				foreach (var argSet in argsSets)
					results.Add(method(argSet));
			}

			foreach (var method in methods)
			{
				results = new List<ulong>();
				watch = new Stopwatch();
				watch.Start();
				for (int i = 0; i < 100; i++)
					foreach (var argSet in argsSets)
						results.Add(method(argSet));
				watch.Stop();

				Console.WriteLine("{0}: {1}", method.Method.Name, watch.Elapsed);
			}
		}
	}
}