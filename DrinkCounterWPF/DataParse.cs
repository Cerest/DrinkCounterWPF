using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;

namespace DrinkCounterWPF
{
	public static class DataParse
	{
		/// <summary>
		///     Name of the Records File
		/// </summary>
		private const string StrRecords = @"\Records.txt";

		/// <summary>
		///     File is small enough that we can save the information without using a DataReader/DataAdapter
		/// </summary>
		internal static void SaveData(DateTime DateOfProgram, int LimitDrinks, double HoursBeforeReset, int AgeReq, DataTable datPeople)
		{
			try
			{
				// Create an instance of StreamReader to read from a file.
				// The using statement also closes the StreamReader.
				using var WriteMe = new StreamWriter(Environment.CurrentDirectory + StrRecords, false);

				WriteMe.WriteLine(DateOfProgram);
				WriteMe.WriteLine($"LimitDrinks = {LimitDrinks}");
				WriteMe.WriteLine($"HoursBeforeReset = {HoursBeforeReset}");
				WriteMe.WriteLine($"AgeReq = {AgeReq}");

				foreach (DataRow person in datPeople.Rows)
				{
					WriteMe.WriteLine($"{person["DoDID32"]}|{person["Count"]}");
				}
			}
			catch
			{
				MessageBox.Show(ProjStrings.CloseConfigFile);
			}
		}

		/// <summary>
		///     File is small enough to load the information in memory without too much issue and
		///     without using a DataReader/DataAdapter
		/// </summary>
		internal static void LoadFile(ref int LimitDrinks, ref double HoursBeforeReset, ref int AgeReq, ref DateTime DateOfProgram, ref DataTable datPeople)
		{
			try
			{
				using var UserConfigFile = new StreamReader(Environment.CurrentDirectory + StrRecords);
				DateTime  DateInRecord   = DateTime.Parse(UserConfigFile.ReadLine()!);
				LimitDrinks      = Convert.ToInt32(UserConfigFile.ReadLine().Split('=')[1].Trim());
				HoursBeforeReset = Convert.ToDouble(UserConfigFile.ReadLine().Split('=')[1].Trim());
				AgeReq           = Convert.ToInt32(UserConfigFile.ReadLine().Split('=')[1].Trim());

				if ((DateTime.Now - DateInRecord).TotalHours < HoursBeforeReset)
				{
					DateOfProgram = DateInRecord;

					while (UserConfigFile.Peek() != -1)
					{
						var strArrayPerson = UserConfigFile.ReadLine().Split('|');

						datPeople.Rows.Add(strArrayPerson);
					}
				}
			}
			catch
			{
				// If it can't read the file, it probably doesn't exist. So make it.
				SaveData(DateOfProgram, LimitDrinks, HoursBeforeReset, AgeReq, datPeople);
			}
		}
	}
}
