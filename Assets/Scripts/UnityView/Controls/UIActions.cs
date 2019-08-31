using System;

namespace CSD
{
	using InControl;

	public class UIActions : PlayerActionSet
	{
		public PlayerAction CycleLeft;
		public PlayerAction CycleRight;
		public PlayerAction Select;
		public PlayerAction Back;
		public PlayerAction Details;
		public PlayerAction Edit;
		public PlayerAction ExitState;
		public UIActions(){
			CycleLeft = CreatePlayerAction("CycleLeft");
			CycleRight = CreatePlayerAction("CycleRight");
			Select = CreatePlayerAction ("Select");
			Back = CreatePlayerAction ("Back");
			Details = CreatePlayerAction ("Details");
			Edit = CreatePlayerAction ("Edit");
			ExitState = CreatePlayerAction ("ExitMenu");
		}

		public static UIActions CreateWithJoystickBindings(){
			var actions = new UIActions();
			actions.CycleLeft.AddDefaultBinding (InputControlType.LeftBumper);
			actions.CycleRight.AddDefaultBinding (InputControlType.RightBumper);
			actions.Select.AddDefaultBinding (InputControlType.Action3);
			actions.Back.AddDefaultBinding (InputControlType.Action2);
			actions.Details.AddDefaultBinding (InputControlType.Action4);
			actions.Edit.AddDefaultBinding (InputControlType.Action1);
			actions.ExitState.AddDefaultBinding (InputControlType.Back);
			return actions;
		}
	}


}

