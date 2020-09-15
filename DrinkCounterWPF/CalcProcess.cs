using System;
using System.Linq;

namespace DrinkCounterWPF
{
	public static class CalcProcess
	{
		/// <summary>
		///     Converts a string of a value of an arbitrary base into its base 10 form. We need to
		///     make this function ourselves because C#'s Convert.ToInt32 doesn't support base 32. It
		///     only supports up to base 16. The CAC DOB is stored in base 32.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="inputBase"></param>
		/// <returns></returns>
		public static double ConvertToBase10(string input, int inputBase)
		{
			int intLength = input.Length;

			return Enumerable.Range(0, intLength)
							 .Select(position => GetValue(input[position]) * Math.Pow(inputBase, intLength - position - 1))
							 .Sum();
		}

		/// <summary>
		/// Grabs the RawDODID from a input scan
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string GetRawDoDID(string input)
		{
			return input.Substring(8, 7);
		}

		/// <summary>
		///     Converts a character into its integer equivalent.
		///     Supports 0 -> 9 and A -> Z such that
		///     A = 10 & Z = 35,  a = 36 and so forth
		/// </summary>
		/// <param name="convertThis"></param>
		/// <returns></returns>
		private static int GetValue(char convertThis)
		{
			return char.IsDigit(convertThis)
					   ? convertThis - '0'
					   : char.IsUpper(convertThis)
						   ? convertThis - 'A' + 10
						   : convertThis - 'a' + 36;
		}
	}
}
