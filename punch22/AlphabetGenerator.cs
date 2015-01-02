using System;
using System.Collections.Generic;
using System.Linq;
using CxSoftware.HexSharp;

public class AlphabetGenerator
{
	// Fields

	private List <Letter> letters;
	private List <uint> options;



	// Constructor

	public AlphabetGenerator ()
	{
	}



	// Methods

	public void Generate ()
	{

		var random = new Random ();
		while (true) {
			this.letters = new [] {
				"A", "B", "C", "D", "E", "F",
				"G", "H", "I", "J", "K", "L",
				"M", "N", "O", "P", "Q", "R",
				"S", "T", "U", "V", "W", "X",
				"Y", "Z" }
				.Select (l => new Letter (l))
				.ToList ();
			this.options = Enumerable.Range (0x00, 0x80)
				.Select (x => (uint) x)
				.ToList ();

			var bestOptions =
				this.options
				.OrderByDescending (o => this.options.Count (
						o2 => o != o2 && CanConvert (o, o2)))
				.ThenBy (o => random.Next ())
					.Take (26)
					.ToList ();

			foreach (var letter in this.letters)
			{
				var nextValue = bestOptions.First ();
				this.AddValueToLetter (
					letter,
					nextValue);
				bestOptions.Remove (nextValue);
			}

			Console.WriteLine ("Looking up...");

			while (true) {
				var nextOptions =
					from letter in this.letters
					where letter.Convertions.Count < (this.letters.Count - 1)
					from option in this.options
					where !letter.Values.Any (v => CanConvert (option, v))
					let missingLetters = this.letters
						.Where (l => letter.Convertions.All (c => c != l))
						.ToList ()
					let data = new
					{
						letter = letter,
						option = option,
						amount = missingLetters.Count (
						        ml => CanConvert (ml.Values [0], (uint)option))
					}
					orderby
						data.amount descending,
						random.Next ()
					select data;

				var found = nextOptions.FirstOrDefault ();
				if (found == null)
					break;

				var unnecessaryValues = found.letter.Values
					.Skip (1)
					.Where (v => CanConvert (v, found.option))
					.ToList ();

				foreach (var unnecessaryValue in unnecessaryValues) {
					Console.WriteLine (
						"Removing value {0} from letter {1} because it will be replaced by {2}",
						PrintBits (unnecessaryValue),
						found.letter.Name,
						PrintBits (found.option));

					RemoveValueFromLetter (
						found.letter,
						unnecessaryValue);
				}

				AddValueToLetter (
					found.letter,
					(uint)found.option);
			}

			var totalConversions = this.letters.Sum (x => x.Convertions.Count);
			var remainingConversions =
				this.letters.Count * (this.letters.Count - 1)
				- totalConversions;
			Console.WriteLine (
				"Summary: {0} conversions, {1} left, {2} options unassigned",
				totalConversions,
				remainingConversions,
				this.options.Count);

			if (remainingConversions < 2)
			{
				foreach (var option in this.options)
					Console.WriteLine (
						"UNASSIGNED: {0}",
						PrintBits (option));

				var unnecessaryList =
					from letter in this.letters
					from val1 in letter.Values
					where val1 != letter.Values [0]
					from val2 in letter.Values
					where val2 != letter.Values [0]
					where val1 != val2
					where CanConvert (val1, val2)
					select new { Letter = letter, Value1 = val1, Value2 = val2 };
				foreach (var unnecessary in unnecessaryList)
					Console.WriteLine (
						"UNNECESSARY: {0} {1} {2}",
						unnecessary.Letter.Name,
						PrintBits (unnecessary.Value1),
						PrintBits (unnecessary.Value2));

				var missingList =
					from letterFrom in this.letters
					from letterTo in this.letters
					where letterFrom != letterTo
					where !CanConvert (letterFrom, letterTo)
					select new { From = letterFrom, To = letterTo };
				foreach (var missing in missingList)
					Console.WriteLine (
						"MISSING {0} -> {1}",
						missing.From.Name,
						missing.To.Name);

				foreach (var letter in this.letters)
					Console.WriteLine (
						"      {1}, -- {0} ({2})",
						letter.Name,
						string.Join (
							", ",
							letter.Values
								.Select (v => PrintBits (v))),
						letter.Convertions.Count);
				Console.ReadLine ();
			}
		}
	}



	// Private methods

	private void AddValueToLetter (Letter letter, uint newValue)
	{
		options.Remove (newValue);
		letter.AddValue (newValue);
		this.RecalculateConversions (letter);
	}

	private void RemoveValueFromLetter (Letter letter, uint oldValue)
	{
		options.Add (oldValue);
		letter.RemoveValue (oldValue);
	}

	private string PrintBits (uint val)
	{
		return string.Format (
			"0x{0} {1}",
			HexConvert.ToString (new [] { (byte)val }),
			new string (
				Enumerable.Range (1, 7)
				.Select (x => ((val >> (7 - x)) & 1) == 1 ? '1' : '0')
				.ToArray ()));
	}

	private void RecalculateConversions (Letter letter)
	{
		var convertableLetters = this.letters
			.Where (l => l.Values.Count != 0)
			.Where (l => l != letter)
			.Where (l => !letter.Convertions.Contains (l))
			.Where (l => CanConvert (l, letter));

		foreach (var letter2 in convertableLetters)
			letter.AddConvertion (letter2);
	}

	private static bool CanConvert (Letter letterFrom, Letter letterTo)
	{
		return CanConvert (letterFrom.Values[0], letterTo);
	}

	private static bool CanConvert (uint valueFrom, Letter letterTo)
	{
		return letterTo.Values.Any (x => CanConvert (valueFrom, x));
	}

	private static bool CanConvert (uint valueFrom, uint valueTo)
	{
		return (valueFrom & valueTo) == valueFrom;
	}
}
