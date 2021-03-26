using System;
using System.Collections.Generic;
using System.Text;
using ComputerInterface;
using ComputerInterface.ViewLib;

namespace PracticeMod
{
	public class PracticeView : ComputerView
	{
		const string Header = "Practice Mod";
		// const string Checkmark = "✔";
		// const string Xmark = "✘";

		int pointer = 0;
		int lenght = 1;

		// This is called when you view is opened
		public override void OnShow(object[] args)
		{
			base.OnShow(args);
			// changing the Text property will fire an PropertyChanged event
			// which lets the computer know the text has changed and update it
			UpdateScreen();
		}

		void UpdateScreen()
		{
			StringBuilder str = new StringBuilder();

			str.AppendLine(Header);
			str.Repeat("=", SCREEN_WIDTH).AppendLine().AppendLine();

			int i = 0;
			if (pointer == i) str.Append("> "); else str.Append("  ");
			str.AppendLine("[Infected Speed]");
			
			i++;
			if (pointer == i) str.Append("> "); else str.Append("  ");
			str.AppendLine("[Survivor Speed]");

			SetText(str);
		}

		void HitButton(int id)
		{
			switch (id)
			{
				case 0: // Infected speed
					Plugin.SetInfectedSpeed();
					break;
				case 1: // Survivor speed
					Plugin.SetSurvivorSpeed();
					break;
			}
		}

		public override void OnKeyPressed(EKeyboardKey key)
		{
			switch (key)
			{
				case EKeyboardKey.Back:
					ReturnView();
					break;
				case EKeyboardKey.Down:
					pointer = Math.Min(pointer + 1, lenght);
					break;
				case EKeyboardKey.Up:
					pointer = Math.Max(pointer - 1, 0);
					break;
				case EKeyboardKey.Enter:
					HitButton(pointer);
					break;
			}

			UpdateScreen();
		}
	}
}
