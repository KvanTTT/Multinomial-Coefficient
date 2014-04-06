using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multinomial
{
	public class CombinationWithRepetition
	{
		private static Stack<uint> _n_inds = new Stack<uint>();
		private static Stack<uint> _inds = new Stack<uint>();
		private static uint[] _array;

		static CombinationWithRepetition()
		{
			Reset();
		}

		public static uint[][] GenerateRecursive(uint n, uint k)
		{
			uint[] array = new uint[k];
			uint[][] result = new uint[CombinationCount(n, k)][];
			int j = 0;
			RecursiveHelper(n, k, array, result, 0, 0, ref j);
			return result;
		}

		public static uint[][] Generate(uint n, uint k)
		{
			var n_inds = new Stack<uint>();
			var inds = new Stack<uint>();
			n_inds.Push(0);
			inds.Push(0);
			uint[] array = new uint[k];
			var result = new uint[CombinationCount(n, k)][];

			int j = 0;
			while (n_inds.Count > 0)
			{
				uint n_chosen = n_inds.Pop();
				uint ind = inds.Pop();

				if (n_chosen > 0)
					array[n_chosen - 1] = ind;
				if (n_chosen == k)
					result[j++] = (uint[])array.Clone();
				else
				{
					for (int i = (int)(n - 1); i >= ind; i--)
					{
						n_inds.Push(n_chosen + 1);
						inds.Push((uint)i);
					}
				}
			}

			return result;
		}

		public static Summand[] GenerateSummands(uint n, uint k)
		{
			var combs = Generate(n, k);

			// TODO: compute multinomial coeffs here (length of array = k)
			var multinomialCoefs = new List<Tuple<uint[], ulong>>();
			Summand[] result = new Summand[combs.Length];

			uint[] array = new uint[n];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = new Summand { Factors = (uint[])combs[i].Clone() };

				for (int j = 0; j < n; j++)
					array[j] = 0;
				for (int j = 0; j < k; j++)
					array[combs[i][j]]++;
				Array.Sort(array);

				bool finded = false;
				foreach (var coef in multinomialCoefs)
					if (Enumerable.SequenceEqual(array, coef.Item1))
					{
						result[i].Coefficient = coef.Item2;
						finded = true;
						break;
					}
				if (!finded)
				{
					result[i].Coefficient = Multinomial.PrimeNumbers(array);
					multinomialCoefs.Add(new Tuple<uint[], ulong>((uint[])array.Clone(), result[i].Coefficient));
				}
			}

			return result;
		}

		public static uint[] Next(uint n, uint k)
		{
			if (_array == null || _array.Length != k)
			{
				Reset();
				_array = new uint[k];
			}
			while (_n_inds.Count > 0)
			{
				uint n_chosen = _n_inds.Pop();
				uint ind = _inds.Pop();

				if (n_chosen > 0)
					_array[n_chosen - 1] = ind;
				if (n_chosen == k)
					return (uint[])_array.Clone();
				else
				{
					for (int i = (int)(n - 1); i >= ind; i--)
					{
						_n_inds.Push(n_chosen + 1);
						_inds.Push((uint)i);
					}
				}
			}
			return null;
		}

		public static void Reset()
		{
			_n_inds.Clear();
			_n_inds.Push(0);
			_inds.Clear();
			_inds.Push(0);
			_array = null;
		}

		private static void RecursiveHelper(uint n, uint k, uint[] array, uint[][] result, uint n_chosen, uint ind, ref int j)
		{
			if (n_chosen == k)
			{
				result[j++] = (uint[])array.Clone();
				return;
			}

			for (uint i = ind; i < n; i++)
			{
				array[n_chosen] = i;
				RecursiveHelper(n, k, array, result, n_chosen + 1, i, ref j);
			}
		}

		public static long CombinationCount(uint n, uint k)
		{
			return BinomCoefficient(n + k - 1, k);
		}

		public static long BinomCoefficient(uint n, uint k)
		{
			if (k > n)
				return 0;
			if (n == k)
				return 1;
			if (k > n - k)
				k = n - k;
			long c = 1;
			for (long i = 1; i <= k; i++)
			{
				c = (c / i) * n + (c % i) * n / i;
				n--;
			}
			return c;
		}
	}
}
