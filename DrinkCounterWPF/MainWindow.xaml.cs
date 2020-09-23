using System;
using System.Data;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using MessageBox = System.Windows.MessageBox;

namespace DrinkCounterWPF
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public sealed partial class MainWindow
	{
		/// <summary>
		///     The DOB on the CAC is offset by a weird 900.59178 years for some reason. Apparently we have
		///     people in the military who need to display DOBs since the year 1000.
		/// </summary>
		private const double DblWeirdDOBOffset = 900.59178 * 365;

		/// <summary>
		///     Data Table name
		/// </summary>
		private readonly DataTable datPeople = new DataTable("People");

		// THE FOLLOWING ARE FIELDS THAT CAN BE/ARE OVERWRITTEN IN THE RECORDS FILE

		private DateTime DateOfProgram = DateTime.Now;

		private double HoursBeforeReset = 12;

		private int AgeReq = 21, LimitDrinks = 3, DrinksLeft;

		private DataRow PersonOfInterest;

		private string NameOfPerson;

		public MainWindow()
		{
			InitializeComponent();

			// Formats the data table.
			datPeople.Columns.Add("DoDID32", typeof(string));
			datPeople.Columns.Add("Count", typeof(int));
			datPeople.PrimaryKey = new[] {datPeople.Columns["DoDID32"]};

			// Loads user configuration + datafile
			DataParse.LoadFile(ref LimitDrinks, ref HoursBeforeReset, ref AgeReq, ref DateOfProgram, ref datPeople);

			txtInput.Focus();
		}

		/// <summary>
		///     Occurs when a CAC is scanned. The btnSubmit by default is the form's "Accept Button".
		///     After a successful scan, it will set focus,
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnSubmit_Click(object sender, EventArgs e)
		{
			// We use the groupbox header as the status message, this resets it.
			grpStatus.Header = string.Empty;

			// Get the input and then clear the input text box
			string strInput = txtInput.Password;
			txtInput.Clear();

			// Clear any fields that are native to that user.
			ResetFields();

			// If the program is left running randomly overnight w/o closing the program then we
			// need to do some work and reset fields
			if ((DateTime.Now - DateOfProgram).TotalHours > HoursBeforeReset)
			{
				DateOfProgram = DateTime.Now;

				datPeople.Rows.Clear();
				DataParse.SaveData(DateOfProgram, LimitDrinks, HoursBeforeReset, AgeReq, datPeople);
			}

			txtInput.IsEnabled  = false;
			btnSubmit.IsEnabled = false;
			btnCancel.IsEnabled = true;

			// This is in a try/catch block in case the user tries to scan in something other than
			// the Front side of the CAC...
			try
			{
				NameOfPerson = string.Format(ProjStrings.Name,
													strInput.Substring(15, 1), // First Initial
													strInput.Substring(35, 26).Trim()); // Entire Last Name

				// Name
				grpStatus.Header = NameOfPerson;

				DateTime DateOfBirth = DateTime.FromOADate(CalcProcess.ConvertToBase10(strInput.Substring(61, 4), 32) - DblWeirdDOBOffset);

				// There's no .TotalYears function, so we have to use .TotalDays.
				int intAge = (int) ((DateTime.Today - DateOfBirth).TotalDays / 365);
				txtAge.Text = string.Format(ProjStrings.Age, intAge);

				// If the Age is >= the AgeReq, then enable the add button.
				bool personIsLegal = intAge >= AgeReq;

				grpStatus.Header = personIsLegal ? "" : string.Format(ProjStrings.Underaged, NameOfPerson);

				ToggleIcon(personIsLegal);

				btnAdd.IsEnabled         = personIsLegal;
				txtDrinksInput.IsEnabled = personIsLegal;

				// If they are legal, then we proceed to do other checks
				if (personIsLegal)
				{
					ProcessPerson(strInput);

					grpStatus.Header = string.Format(ProjStrings.DrinksRemaining, NameOfPerson, DrinksLeft);
				}
				else
				{
					btnCancel.Focus();
				}
			}
			catch
			{
				ResetFields();

				grpStatus.Header  = ProjStrings.ReadyToScan;

				MessageBox.Show(ProjStrings.ErrorReading);

				txtInput.Focus();
			}
		}

		/// <summary>
		/// Changes the Status Icon's color and shape depending on the status bool
		/// </summary>
		/// <param name="status"></param>
		private void ToggleIcon(bool status)
		{
			if (status)
			{
				icoStatus.Foreground = new SolidColorBrush(Color.FromRgb(50, 100, 50));
				icoStatus.Kind       = PackIconKind.CheckBold;
			}
			else
			{
				icoStatus.Foreground = new SolidColorBrush(Color.FromRgb(100, 50, 50));
				icoStatus.Kind       = PackIconKind.AlertCircle;
			}
		}

		/// <summary>
		///     Method in which we check whether or not person can be found in database
		/// </summary>
		/// <param name="strInput"></param>
		private void ProcessPerson(string strInput)
		{
			// By Default, the CAC stores the DoDID as a base 32, that by itself is a unique
			// identifier and as such there's no need to convert it back into its base 10 equivalent
			DataRow Person = datPeople.NewRow();
			Person["DoDID32"] = CalcProcess.GetRawDoDID(strInput);

			// Try to find a person on the list with that DoDID number
			DataRow PersonMatch = datPeople.Rows.Find(Person["DoDID32"]);

			// Person match returns null if it doesn't find a person
			if (PersonMatch != null)
			{
				bool blnBelowLimit = (int) PersonMatch["Count"] < LimitDrinks;

				btnAdd.IsEnabled = blnBelowLimit;

				txtDrinksInput.IsEnabled = blnBelowLimit;

				ToggleIcon(blnBelowLimit);

				PersonOfInterest = PersonMatch;

				DrinksLeft = LimitDrinks - (int) PersonOfInterest["Count"];

				if (blnBelowLimit)
				{
					txtDrinksInput.Focus();
					txtDrinksInput.SelectAll();

					ToggleIncrementButtons();
				}
				else
				{
					btnCancel.Focus();
				}
			}
			else
			{
				// Add the new LEGAL person to the list;
				Person["Count"] = 0;
				datPeople.Rows.Add(Person);
				PersonOfInterest = Person;

				DataParse.SaveData(DateOfProgram, LimitDrinks, HoursBeforeReset, AgeReq, datPeople);

				ToggleIcon(true);

				txtDrinksInput.Focus();
				txtDrinksInput.SelectAll();

				DrinksLeft = LimitDrinks - (int) PersonOfInterest["Count"];

				grpStatus.Header = string.Format(ProjStrings.DrinksRemaining, NameOfPerson, DrinksLeft);

				ToggleIncrementButtons();
			}
		}

		/// <summary>
		/// Toggles Increment Buttons based on where they are.
		/// </summary>
		private void ToggleIncrementButtons()
		{
			ValidateDrinks(out int intInputDrink);

			// Increment should be disable if the current drinks is
			btnIncrement.IsEnabled = intInputDrink < DrinksLeft;
			btnDecrement.IsEnabled = intInputDrink > 1;
		}

		private bool ValidateDrinks(out int intInputDrink)
		{
			bool blnInputGood = true;
			if (int.TryParse(txtDrinksInput.Text, out intInputDrink) == false ||
				intInputDrink < 1 ||
				intInputDrink + (int) PersonOfInterest["Count"] > LimitDrinks)
			{
				blnInputGood      = false;
				ToggleIcon(false);
			}

			return blnInputGood;
		}

		/// <summary>
		///     Add Number of drinks into database
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnAdd_Click(object sender, EventArgs e)
		{
			// Validate
			bool blnInputGood = ValidateDrinks(out int intInputDrink);

			if (blnInputGood)
			{
				PersonOfInterest["Count"] = (int) PersonOfInterest["Count"] + intInputDrink;

				grpStatus.Header = string.Format(ProjStrings.DrinksRemaining, NameOfPerson, DrinksLeft);

				ToggleIcon((int) PersonOfInterest["Count"] < LimitDrinks);

				ResetFields();

				DataParse.SaveData(DateOfProgram, LimitDrinks, HoursBeforeReset, AgeReq, datPeople);

				txtInput.Focus();
			}
		}

		/// <summary>
		///     Clears fields after every entry
		/// </summary>
		private void ResetFields()
		{
			icoStatus.Kind       = PackIconKind.BarcodeScan;
			icoStatus.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

			grpStatus.Header = ProjStrings.ReadyToScan;

			txtInput.IsEnabled  = true;
			btnSubmit.IsEnabled = true;

			txtDrinksInput.Text = "1";
			txtDrinksInput.IsEnabled = false;

			btnAdd.IsEnabled       = false;
			btnCancel.IsEnabled    = false;
			btnDecrement.IsEnabled = false;
			btnIncrement.IsEnabled = false;

			txtAge.Clear();
		}

		/// <summary>
		///     Sets the btnSubmit as the Accept Button whenever the CAC input field has focus
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnSubmitAsAccept(object sender, EventArgs e)
		{
			btnSubmit.IsDefault = true;
			btnAdd.IsDefault    = false;
		}

		/// <summary>
		///     Sets the btnAdd as the Accept button whenever the Drinks input field has focus
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnAddAsAccept(object sender, EventArgs e)
		{
			btnSubmit.IsDefault = false;
			btnAdd.IsDefault    = true;
		}

		/// <summary>
		///     Cancel the addition of the drink
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnCancel_Click(object sender, EventArgs e)
		{
			ResetFields();

			txtInput.Focus();
		}

		/// <summary>
		///     Adds 1 to the currently displayed drink Input
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnDecrement_Click(object sender, EventArgs e)
		{
			txtDrinksInput.Text = ValidateDrinks(out int intInputDrink)
									  ? (intInputDrink - 1).ToString()
									  : DrinksLeft.ToString();

			ToggleIncrementButtons();
			//txtDrinksInput.Focus();
		}

		/// <summary>
		///     Subtracts 1 from the currently displayed drink Input
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnIncrement_Click(object sender, EventArgs e)
		{
			txtDrinksInput.Text = ValidateDrinks(out int intInputDrink)
									  ? (intInputDrink + 1).ToString()
									  : DrinksLeft.ToString();

			ToggleIncrementButtons();
			//txtDrinksInput.Focus();
		}
	}
}