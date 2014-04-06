using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Diagnostics;
using System.IO;

namespace Multinomial
{
	class Program
	{
		static StringBuilder Results;
		static Func<uint[], ulong>[] MultinomialMethods = new Func<uint[], ulong>[]
		{
			Multinomial.Naive,
			Multinomial.Binom,
			Multinomial.Log,
			Multinomial.LogGamma,
			Multinomial.My,
			Multinomial.Naive,
			Multinomial.PrimeNumbers,
			Multinomial.Big
		};

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

			uint iteration = 2;

			ulong result;
			int prevPermutNumber;
			ulong prevResult;
			uint[] prevPermut;
			int count = 0;
			do
			{
				Results.Append(iteration + "\t");
				tempResults.Clear();

				prevPermut = null;
				prevResult = 0;
				prevPermutNumber = -1;
				bool bigIsCorrect = true;
				bool naiveIsCorrect = true;
				bool binomIsCorrect = true;
				bool logIsCorrect = true;
				bool optIsCorrect = true;
				bool lnGammaIsCorrect = true;
				bool primeNumbersIsCorrect = true;

				Console.WriteLine("Arg count: " + iteration);
				uint[] combination;
				while ((combination = CombinationWithRepetition.Next(maxNumber, iteration)) != null)
				{
					var currentCombintaion = combination.Select(a => a + 1).ToArray();
					count++;

					result = 0;
					try
					{
						result = Multinomial.Big(currentCombintaion);
					}
					catch
					{
						bigIsCorrect = false;
					}
					
					if (bigIsCorrect)
					{
						MultinomCoefMethodTest(currentCombintaion, prevPermut, prevPermutNumber, Multinomial.Naive, ref naiveIsCorrect, result, maxError, errorMeasure, tempResults);
						MultinomCoefMethodTest(currentCombintaion, prevPermut, prevPermutNumber, Multinomial.Binom, ref binomIsCorrect, result, maxError, errorMeasure, tempResults);
						MultinomCoefMethodTest(currentCombintaion, prevPermut, prevPermutNumber, Multinomial.Log, ref logIsCorrect, result, maxError, errorMeasure, tempResults);
						MultinomCoefMethodTest(currentCombintaion, prevPermut, prevPermutNumber, Multinomial.LogGamma, ref lnGammaIsCorrect, result, maxError, errorMeasure, tempResults);
						MultinomCoefMethodTest(currentCombintaion, prevPermut, prevPermutNumber, Multinomial.My, ref optIsCorrect, result, maxError, errorMeasure, tempResults);
						MultinomCoefMethodTest(currentCombintaion, prevPermut, prevPermutNumber, Multinomial.PrimeNumbers, ref primeNumbersIsCorrect, result, maxError, errorMeasure, tempResults);

						prevPermut = (uint[])currentCombintaion.Clone();
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
							PrintIfCorrect(Multinomial.PrimeNumbers, prevPermut, prevPermutNumber, prevResult, primeNumbersIsCorrect, tempResults);
							PrintIfCorrect(Multinomial.Big, prevPermut, prevPermutNumber, prevResult, true, tempResults);

							PrintTempResult("Naive", tempResults);
							PrintTempResult("Binom", tempResults);
							PrintTempResult("Log", tempResults);
							PrintTempResult("LogGamma", tempResults);
							PrintTempResult("My", tempResults);
							PrintTempResult("PrimeNumbers", tempResults);
							PrintTempResult("Big", tempResults);
							Results.Remove(Results.Length - 1, 1);
						}

						Console.WriteLine();
						break;
					}
				}
				iteration++;

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

			List<ulong> results;
			Stopwatch watch;

			results = new List<ulong>(repeatCount);

			if (prefetch)
				foreach (var method in MultinomialMethods)
					foreach (var argSet in argsSets)
						results.Add(method(argSet));

			foreach (var method in MultinomialMethods)
			{
				results.Clear();
				watch = new Stopwatch();
				watch.Start();
				for (int i = 0; i < repeatCount; i++)
					foreach (var argSet in argsSets)
						results.Add(method(argSet));
				watch.Stop();

				Console.WriteLine("{0,15}: {1}", method.Method.Name, watch.Elapsed.ToString());
				Results.AppendFormat("{0,-15}:	{1}{2}", method.Method.Name, watch.Elapsed.ToString(), Environment.NewLine);
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
				Multinomial.PrimeNumbers,
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

				Console.WriteLine("{0,15}: {1}", method.Method.Name, watch.Elapsed.ToString());
				Results.AppendFormat("{0,-15}:	{1}{2}", method.Method.Name, watch.Elapsed.ToString(), Environment.NewLine);
			}
			return results[42];
		}
	}
}