using System;
using System.Collections.Generic;
using System.Text;
using ComputerInterface;
using ComputerInterface.ViewLib;
using GorillaLocomotion;
using PracticeMod.Patches;

namespace PracticeMod
{
	public class PracticeView : ComputerView
	{
		const string highlightColor = "ffa500ff";

		private readonly UISelectionHandler _selectionHandler;

		int infectedPlayers = 1;
		int totalPlayers = 10;
		bool infected = true;

		public PracticeView()
		{
			_selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
			// the max zero indexed entry (2 entries - 1 since zero indexed)
			_selectionHandler.MaxIdx = 3;
			// when the "selection" key is pressed (we set it to enter above)
			_selectionHandler.OnSelected += OnEntrySelected;
			// since you quite often want to have an indicator of the selected item
			// I added helper function for that.
			// Basically you specify the prefix and suffix added to the selected item
			// and an prefix and suffix if the item isn't selected
			_selectionHandler.ConfigureSelectionIndicator($"<color=#{highlightColor}>></color> ", "", "  ", "");
		}

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
				str.BeginCenter();
				str.MakeBar('-', SCREEN_WIDTH, 0, "ffffff10");
				str.AppendClr("Practice Mod", highlightColor).EndColor().AppendLine();
				str.AppendLine("By Graic");
				str.MakeBar('-', SCREEN_WIDTH, 0, "ffffff10");
				str.EndAlign().AppendLines(1);

				if (Plugin.Allowed)
				{
					DrawBody(str);
				} else
				{
					DrawDisabled(str);
				}
			});
		}

		void DrawDisabled(StringBuilder str)
		{
			str.AppendLine();
			str.BeginCenter();
			str.BeginColor("ff0000");
			//str.AppendLine("You must be alone in a private");
			//str.AppendLine("to use this mod!");
			str.AppendLine("Join a private by yourself");
			str.AppendLine("to begin practicing!");
			str.EndColor();
			str.EndAlign();
		}

		void DrawBody(StringBuilder str)
		{
			str.AppendLine($"  Jump Multiplier: {(MovementSpeedPatch.ShouldOverride ? MovementSpeedPatch.OverrideJumpMultiplier : Player.Instance.jumpMultiplier):F2}");
			str.AppendLine($"  Max Jump Speed: {(MovementSpeedPatch.ShouldOverride ? MovementSpeedPatch.OverrideMaxJumpSpeed : Player.Instance.maxJumpSpeed):F2}");
			str.AppendLine();

			// get the item with the prefix and suffix configured above
			// see how this results in a lot less lines and logic
			str.AppendLine(_selectionHandler.GetIndicatedText(0, $"<color={(infected ? "#"+highlightColor: "white")}>[Infected]</color>"));
			str.AppendLine(_selectionHandler.GetIndicatedText(1, $"<color={(!infected ? "#"+highlightColor: "white")}>[Survivor]</color>"));
			str.AppendLine(_selectionHandler.GetIndicatedText(2, $"Infected Count: {infectedPlayers}"));
			str.AppendLine(_selectionHandler.GetIndicatedText(3, $"Player Count: {totalPlayers}"));

			str.AppendLine();
			str.BeginColor("ffffff10").AppendLine("  ▲/▼ Select  Enter/◀/▶ Adjust").EndColor();
		}

		private void OnEntrySelected(int index)
		{
			switch (index)
			{
				case 0: // Infected speed
					infectedPlayers = 1;
					totalPlayers = 10;
					infected = true;
					Plugin.SetInfectedSpeed();
					break;
				case 1: // Survivor speed
					infectedPlayers = 1;
					totalPlayers = 10;
					infected = false;
					Plugin.SetSurvivorSpeed();
					break;
			}
		}

		private void OnEntryAdjusted(int index, bool increase)
		{
			int offset = increase ? 1 : -1;
			switch (index)
			{
				case 2:
					infectedPlayers = UnityEngine.Mathf.Clamp(infectedPlayers + offset, 1, totalPlayers - 1);
					Plugin.SetSpecificSpeed(totalPlayers, infectedPlayers, infected);
					break;
				case 3:
					totalPlayers = UnityEngine.Mathf.Clamp(totalPlayers + offset, 2, 10);
					infectedPlayers = UnityEngine.Mathf.Clamp(infectedPlayers, 1, totalPlayers - 1);
					Plugin.SetSpecificSpeed(totalPlayers, infectedPlayers, infected);
					break;
			}
		}

		public override void OnKeyPressed(EKeyboardKey key)
		{
			if (Plugin.Allowed)
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
				
				// check if the pressed key is adjusting a setting
				if (key == EKeyboardKey.Left || key == EKeyboardKey.Right)
				{
					OnEntryAdjusted(_selectionHandler.CurrentSelectionIndex, key == EKeyboardKey.Right);
					UpdateScreen();
				}
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
