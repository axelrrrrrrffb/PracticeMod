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

		private readonly UISelectionHandler _selectionHandler;

		public PracticeView()
		{
			_selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
			// the max zero indexed entry (2 entries - 1 since zero indexed)
			_selectionHandler.MaxIdx = 1;
			// when the "selection" key is pressed (we set it to enter above)
			_selectionHandler.OnSelected += OnEntrySelected;
			// since you quite often want to have an indicator of the selected item
			// I added helper function for that.
			// Basically you specify the prefix and suffix added to the selected item
			// and an prefix and suffix if the item isn't selected
			_selectionHandler.ConfigureSelectionIndicator("> ", "", "  ", "");
		}

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
			// when your text function isn't that complex
			// you can use this method which creates a string builder
			// passes it via the specified callback function and sets the text at the end
			SetText(str =>
			{
				str.AppendLine(Header);
				str.Repeat("=", SCREEN_WIDTH).AppendLines(2);

				// get the item with the prefix and suffix configured above
				// see how this results in a lot less lines and logic
				str.AppendLine(_selectionHandler.GetIndicatedText(0, "[Infected Speed]"));
				str.AppendLine(_selectionHandler.GetIndicatedText(1, "[Survivor Speed]"));
			});
		}

		private void OnEntrySelected(int idx)
		{
			switch (idx)
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
			// check if the pressed key is already handled
			// by the selection handler (if yes returns true)
			// don't check for other buttons if it's handled
			// just update the screen
			if (_selectionHandler.HandleKeypress(key))
			{
				UpdateScreen();
				return;
			}

			switch (key)
			{
				case EKeyboardKey.Back:
					ReturnView();
					break;
			}
		}
	}
}
