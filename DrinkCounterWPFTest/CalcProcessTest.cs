using System;
using System.Data;
using DrinkCounterWPF;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DrinkCounterWPFTest
{
	[TestClass]
	public class CalcProcessTest
	{
		/// <summary>
		/// After a scan, TestGetDODID() should return John's Smith DOD of 1656809966
		/// </summary>
		[TestMethod]
		public void GetDODIDTest()
		{
			// SETUP
			const string inputString = "NG3EISKS1HC1QFEJohn                Smith                     B2OPAF00AMN   ME03BBPNBCRUJ";

			string rawDoD = CalcProcess.GetRawDoDID(inputString);

			// CALCULATION
			double actualString = CalcProcess.ConvertToBase10(rawDoD, 32);

			// VERIFY
			const double expectedString = 1656809966;
			Assert.AreEqual(expectedString, actualString);
		}
	}

	[TestClass]
	public class DataParseTest
	{
		/// <summary>
		/// After a scan, TestGetDODID() should return John's Smith DOD of 1656809966
		/// </summary>
		[TestMethod]
		public void SaveDataTest()
		{
			// SETUP
			DateTime  dateOfProgram    = DateTime.Now;
			const int limitDrinks      = 4;
			const int hoursBeforeReset = 12;
			const int ageReq           = 21;

			var peopleTable = new DataTable("People");
			peopleTable.Columns.Add("DoDID32", typeof(string));
			peopleTable.Columns.Add("Count", typeof(int));
			peopleTable.PrimaryKey = new[] {peopleTable.Columns["DoDID32"]};

			DataRow newPerson = peopleTable.NewRow();
			newPerson["DoDID32"] = 123;
			newPerson["Count"] = 1;

			peopleTable.Rows.Add(newPerson);

			// SAVE THE DATA
			DataParse.SaveData(dateOfProgram, limitDrinks, hoursBeforeReset, ageReq, peopleTable);

			// Initialize the actual values
			int actualLimit = 0;


			DataParse.LoadFile(ref actualLimit, ref hoursBeforeReset, ref int AgeReq, ref DateTime DateOfProgram, ref DataTable datPeople)

		}
	}
}
