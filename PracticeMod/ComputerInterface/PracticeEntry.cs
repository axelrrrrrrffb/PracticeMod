using System;
using System.Collections.Generic;
using System.Text;
using ComputerInterface;
using ComputerInterface.Interfaces;

namespace PracticeMod
{
	public class PracticeEntry : IComputerModEntry
	{
		public string EntryName => "Practice Mod";

		public Type EntryViewType => typeof(PracticeView);
	}
}
