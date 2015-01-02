using System;
using System.Collections.Generic;

public class Letter
{
	// Properties

	public List<Letter> Convertions { get; private set; }
	public string Name { get; private set; }
	public List<uint> Values { get; private set; }



	// Constructor

	public Letter (string name)
	{
		this.Name = name;
		this.Convertions = new List<Letter> ();
		this.Values = new List<uint> ();
	}



	// Methods

	public void AddConvertion (Letter letter)
	{
		this.Convertions.Add (letter);
	}

	public void AddValue (uint newValue)
	{
		this.Values.Add (newValue);
	}

	public void RemoveValue (uint oldValue)
	{
		this.Values.Remove (oldValue);
	}
}
