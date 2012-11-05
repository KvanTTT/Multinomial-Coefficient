using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Facet.Combinatorics;
using System.Numerics;
using System.Diagnostics;
using System.IO;

namespace Multinomial
{
	class Program
	{
		static StringBuilder Results;

		static void Main(string[] args)
		{
			Results = new StringBuilder();

			TestBigNumberMultinomial(0, ErrorMeasure.Absolute);

			Console.WriteLine();

			int iterationCount = 20000;

			Multinomial.ClearCache();
			Console.WriteLine("With prefetch");
			PerformanceTest(iterationCount, true);
			Console.WriteLine();
			Results.AppendLine();

			Multinomial.ClearCache();
			PerformanceTest2(iterationCount, true);
			Console.WriteLine();
			Results.AppendLine();

			Multinomial.ClearCache();
			Console.WriteLine("Without prefetch");
			PerformanceTest(iterationCount, false);
			Console.WriteLine();
			Results.AppendLine();

			Multinomial.ClearCache();
			PerformanceTest2(iterationCount, false);
			Console.WriteLine();
			Results.AppendLine();

			File.WriteAllText("result.txt", Results.ToString());
			Console.ReadLine();
		}

		static void TestBigNumberMultinomial(uint maxError, ErrorMeasure errorMeasure)
		{
			Dictionary<string, MultinomialResult> tempResults = new Dictionary<string, MultinomialResult>();
			Results.AppendLine("Number of arguments	Naive	Binom	Log	LogGamma	My	BigNumber	Table");

			const uint maxNumber = 1000;

			var ar = new uint[maxNumber];
			for (uint i = 0; i < ar.Length; i++)
				ar[i] = i + 1;

			int iter = 2;

			ulong result;
			int prevPermutNumber;
			ulong prevResult;
			uint[] prevPermut;
			int count = 0;
			do
			{
				Results.Append(iter + "\t");
				tempResults.Clear();

				var combinations = new Combinations<uint>(ar, iter, GenerateOption.WithRepetition);

				prevPermut = null;
				prevResult = 0;
				prevPermutNumber = -1;
				var enumerator = combinations.GetEnumerator();
				bool bigIsCorrect = true;
				bool naiveIsCorrect = true;
				bool binomIsCorrect = true;
				bool logIsCorrect = true;
				bool optIsCorrect = true;
				bool lnGammaIsCorrect = true;

				Console.WriteLine("Arg count: " + iter);
				while (enumerator.MoveNext())
				{
					var curPermut = enumerator.Current.ToArray();

					count++;

					result = 0;
					try
					{
						result = Multinomial.Big(curPermut);
					}
					catch
					{
						bigIsCorrect = false;
					}
					
					if (bigIsCorrect)
					{
						MultinomCoefMethodTest(curPermut, prevPermut, prevPermutNumber, Multinomial.Naive, ref naiveIsCorrect, result, maxError, errorMeasure, tempResults);
						MultinomCoefMethodTest(curPermut, prevPermut, prevPermutNumber, Multinomial.Binom, ref binomIsCorrect, result, maxError, errorMeasure, tempResults);
						MultinomCoefMethodTest(curPermut, prevPermut, prevPermutNumber, Multinomial.Log, ref logIsCorrect, result, maxError, errorMeasure, tempResults);
						MultinomCoefMethodTest(curPermut, prevPermut, prevPermutNumber, Multinomial.LogGamma, ref lnGammaIsCorrect, result, maxError, errorMeasure, tempResults);
						MultinomCoefMethodTest(curPermut, prevPermut, prevPermutNumber, Multinomial.My, ref optIsCorrect, result, maxError, errorMeasure, tempResults);

						prevPermut = enumerator.Current.ToArray();
						prevResult = result;
						prevPermutNumber++;
					}
					else
					{
						if (prevPermut != null)
						{
							PrintIfCorrect(Multinomial.Naive, prevPermut, prevPermutNumber, prevResult, naiveIsCorrect, tempResults);
							PrintIfCorrect(Multinomial.Binom, prevPermut, prevPermutNumber, prevResult, binomIsCorrect, tempResults);
							PrintIfCorrect(Multinomial.Log, prevPermut, prevPermutNumber, prevResult, logIsCorrect, tempResults);
							PrintIfCorrect(Multinomial.LogGamma, prevPermut, prevPermutNumber, prevResult, lnGammaIsCorrect, tempResults);
							PrintIfCorrect(Multinomial.My, prevPermut, prevPermutNumber, prevResult, optIsCorrect, tempResults);
							PrintIfCorrect(Multinomial.Big, prevPermut, prevPermutNumber, prevResult, true, tempResults);

							PrintTempResult("Naive", tempResults);
							PrintTempResult("Binom", tempResults);
							PrintTempResult("Log", tempResults);
							PrintTempResult("LogGamma", tempResults);
							PrintTempResult("My", tempResults);
							PrintTempResult("Big", tempResults);
							Results.Remove(Results.Length - 1, 1);
						}

						Console.WriteLine();
						break;
					}
				}
				iter++;

				Results.AppendLine();
			}
			while (prevPermut != null);
			Console.WriteLine("Count: {0}", count);
		}

		static ulong MultinomCoefMethodTest(uint[] args, uint[] prevArgs, int prevPermutNumber, Func<uint[], ulong> method, 
			ref bool methodIsCorrect, ulong correctResult, ulong maxError, ErrorMeasure errorMeasure,
			Dictionary<string, MultinomialResult> tempResults)
		{
			ulong result = 0;
			if (methodIsCorrect)
			{
				try
				{
					result = method(args);
				}
				catch
				{
					if (prevArgs != null)
					{
						result = method(prevArgs);
						Console.WriteLine(string.Format("{0}({1}) = {2}; #{3}; overflow",
							method.Method.Name, string.Join(",", prevArgs), result, prevPermutNumber));
						tempResults[method.Method.Name] = new MultinomialResult
						{
							Number = prevPermutNumber,
							Permutation = prevArgs,
							Result = result,
							ErrorType = ErrorType.Overflow,
							Error = 0
						};
					}
					else
					{
						Console.WriteLine(string.Format("{0}(x); #{1}; overflow", method.Method.Name, prevPermutNumber));
						tempResults[method.Method.Name] = new MultinomialResult
						{
							Number = -1,
							Permutation = prevArgs,
							Result = 0,
							ErrorType = ErrorType.Overflow,
							Error = 0
						};
					}

					methodIsCorrect = false;
				}

				var error = Multinomial.Diff(result, correctResult);
				if (methodIsCorrect && ((errorMeasure == ErrorMeasure.Absolute && error > maxError) ||
					(errorMeasure == ErrorMeasure.Relative && ((double)error * 10000000000000 / correctResult) > maxError)))
				{
					if (prevArgs != null)
					{
						Console.WriteLine(string.Format("{0}({1}) = {2}; #{3}; rounding(error = {4}{5})",
							method.Method.Name, string.Join(",", prevArgs), method(prevArgs), 
							prevPermutNumber, error,
							errorMeasure == ErrorMeasure.Relative ? "%" : string.Empty));
						tempResults[method.Method.Name] = new MultinomialResult
						{
							Number = prevPermutNumber,
							Permutation = prevArgs,
							Result = result,
							ErrorType = ErrorType.Rounding,
							Error = error
						};
					}
					else
					{
						Console.WriteLine(string.Format("{0}(x); #{1}; rounding(error = {2}{3})",
							method.Method.Name, prevPermutNumber, error,
							errorMeasure == ErrorMeasure.Relative ? "%" : string.Empty));
						tempResults[method.Method.Name] = new MultinomialResult
						{
							Number = -1,
							Permutation = prevArgs,
							Result = 0,
							ErrorType = ErrorType.Rounding,
							Error = error
						};
					}

					methodIsCorrect = false;
				}
			}
			return result;
		}

		static void PrintIfCorrect(Func<uint[], ulong> method, uint[] args, int number, ulong result, bool methodIsCorrect,
			Dictionary<string, MultinomialResult> tempResults)
		{
			if (methodIsCorrect)
			{
				Console.WriteLine(string.Format("{0}({1}) = {2}; #{3}", method.Method.Name, 
					string.Join(",", args), result, number));
				tempResults[method.Method.Name] = new MultinomialResult
				{
					Number = number,
					Permutation = args,
					Result = result,
					ErrorType = ErrorType.NoError,
					Error = 0
				};
			}
		}

		static void PrintTempResult(string methodName, Dictionary<string, MultinomialResult> tempResult)
		{
			var t = tempResult[methodName];
			if (t.ErrorType == ErrorType.NoError)
				Results.Append((t.Result == 0 ? "x" : t.Number.ToString()) + "	");
			else
				Results.AppendFormat("{0}({1})	", 
					t.Result == 0 ? "x" : t.Number.ToString(),
					t.ErrorType == ErrorType.Overflow ? "o" : "r");
		}

		static ulong PerformanceTest(int repeatCount, bool prefetch)
		{
			var argsSets = new List<uint[]>()
			{
				new uint[] { 1, 1 },
				new uint[] { 1, 2 },
				new uint[] { 2, 1 },
				new uint[] { 2, 2 },
				new uint[] { 5, 5, 5 },
				new uint[] { 1, 1, 1, 1, 1, 1, 1, 1, 2 },
				new uint[] { 1, 2, 3, 4, 5, 4 },
			};

			var methods = new List<Func<uint[], ulong>>()
			{
				Multinomial.Naive,
				Multinomial.Binom,
				Multinomial.Log,
				Multinomial.LogGamma,
				Multinomial.My,
				Multinomial.Big
			};

			List<ulong> results;
			Stopwatch watch;

			results = new List<ulong>(repeatCount);

			if (prefetch)
				foreach (var method in methods)
					foreach (var argSet in argsSets)
						results.Add(method(argSet));

			foreach (var method in methods)
			{
				results.Clear();
				watch = new Stopwatch();
				watch.Start();
				for (int i = 0; i < repeatCount; i++)
					foreach (var argSet in argsSets)
						results.Add(method(argSet));
				watch.Stop();

				Console.WriteLine("{0,10}: {1}", method.Method.Name, watch.Elapsed.ToString());
				Results.AppendFormat("{0,-10}:	{1}{2}", method.Method.Name, watch.Elapsed.ToString(), Environment.NewLine);
			}
			return results[42];
		}

		static ulong PerformanceTest2(int repeatCount, bool prefetch)
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
				new uint[] { 2, 3, 4, 5, 7 }
			};

			var methods = new List<Func<uint[], ulong>>()
			{
				Multinomial.Binom,
				Multinomial.Log,
				Multinomial.LogGamma,
				Multinomial.My,
				Multinomial.Big
			};

			List<ulong> results;
			Stopwatch watch;

			results = new List<ulong>(repeatCount);

			if (prefetch)
				foreach (var method in methods)
					foreach (var argSet in argsSets)
						results.Add(method(argSet));

			foreach (var method in methods)
			{
				results.Clear();
				watch = new Stopwatch();
				watch.Start();
				for (int i = 0; i < repeatCount; i++)
					foreach (var argSet in argsSets)
						results.Add(method(argSet));
				watch.Stop();

				Console.WriteLine("{0,10}: {1}", method.Method.Name, watch.Elapsed.ToString());
				Results.AppendFormat("{0,-10}:	{1}{2}", method.Method.Name, watch.Elapsed.ToString(), Environment.NewLine);
			}
			return results[42];
		}
	}
}