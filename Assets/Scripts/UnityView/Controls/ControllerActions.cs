namespace CSD
{
	using InControl;

	//TODO make a generic action set instead
	public class ControllerActions : PlayerActionSet
	{
		public PlayerAction A;
		public PlayerAction B;
		public PlayerAction X;
		public PlayerAction Y;
		public PlayerAction LeftJoyLeft;
		public PlayerAction LeftJoyRight;
		public PlayerAction LeftJoyUp;
		public PlayerAction LeftJoyDown;
		public PlayerTwoAxisAction LeftJoy;
		public PlayerAction RightJoyLeft;
		public PlayerAction RightJoyRight;
		public PlayerAction RightJoyUp;
		public PlayerAction RightJoyDown;
		public PlayerTwoAxisAction RightJoy;
		public PlayerAction RightBumper;
		public PlayerAction LeftBumper;
		public PlayerAction RightTrigger;
		public PlayerAction LeftTrigger;
		public PlayerAction DPadUp;
		public PlayerAction DPadDown;
		public PlayerAction DPadLeft;
		public PlayerAction DPadRight;
		public PlayerAction Back;
		public PlayerAction Start;
		public PlayerAction LeftJoyButton;
		public PlayerAction RightJoyButton;

		public ControllerActions()
		{
			A = CreatePlayerAction ("A");
			B = CreatePlayerAction ("B");
			X = CreatePlayerAction ("X");
			Y = CreatePlayerAction ("Y");
			LeftJoyLeft = CreatePlayerAction ("LeftJoyLeft");
			LeftJoyRight = CreatePlayerAction ("LeftJoyRight");
			LeftJoyUp = CreatePlayerAction ("LeftJoyUp");
			LeftJoyDown = CreatePlayerAction ("LeftJoyDown");
			LeftJoy = CreateTwoAxisPlayerAction (LeftJoyLeft, LeftJoyRight, LeftJoyDown, LeftJoyUp);
			RightJoyLeft = CreatePlayerAction ("RightJoyLeft");
			RightJoyRight = CreatePlayerAction ("RightJoyRight");
			RightJoyUp = CreatePlayerAction ("RightJoyUp");
			RightJoyDown = CreatePlayerAction ("RightJoyDown");
			RightJoy = CreateTwoAxisPlayerAction (RightJoyLeft, RightJoyRight, RightJoyDown, RightJoyUp);
			RightBumper = CreatePlayerAction ("RightBumper");
			LeftBumper = CreatePlayerAction ("LeftBumper");
			RightTrigger = CreatePlayerAction ("RightTrigger");
			LeftTrigger = CreatePlayerAction ("LeftTrigger");
			DPadUp = CreatePlayerAction ("DPadUp");
			DPadDown = CreatePlayerAction ("DPadDown");
			DPadLeft = CreatePlayerAction ("DPadLeft");
			DPadRight = CreatePlayerAction ("DPadRight");
			Back = CreatePlayerAction ("Back");
			Start = CreatePlayerAction ("Start");
			LeftJoyButton = CreatePlayerAction ("LeftJoyButton");
			RightJoyButton = CreatePlayerAction ("RightJoyButton");
		}


		public static ControllerActions CreateWithJoystickBindings()
		{
			var actions = new ControllerActions();

			actions.A.AddDefaultBinding( InputControlType.Action1 );
			actions.B.AddDefaultBinding( InputControlType.Action2 );
			actions.X.AddDefaultBinding( InputControlType.Action3 );
			actions.Y.AddDefaultBinding( InputControlType.Action4 );

			actions.LeftJoyUp.AddDefaultBinding( InputControlType.LeftStickUp );
			actions.LeftJoyDown.AddDefaultBinding( InputControlType.LeftStickDown );
			actions.LeftJoyLeft.AddDefaultBinding( InputControlType.LeftStickLeft );
			actions.LeftJoyRight.AddDefaultBinding( InputControlType.LeftStickRight );

			actions.RightJoyUp.AddDefaultBinding( InputControlType.RightStickUp );
			actions.RightJoyDown.AddDefaultBinding( InputControlType.RightStickDown );
			actions.RightJoyLeft.AddDefaultBinding( InputControlType.RightStickLeft );
			actions.RightJoyRight.AddDefaultBinding( InputControlType.RightStickRight );

			actions.RightBumper.AddDefaultBinding (InputControlType.RightBumper);
			actions.LeftBumper.AddDefaultBinding (InputControlType.LeftBumper);
			actions.RightTrigger.AddDefaultBinding (InputControlType.RightTrigger);
			actions.LeftTrigger.AddDefaultBinding (InputControlType.LeftTrigger);

			actions.DPadUp.AddDefaultBinding (InputControlType.DPadUp);
			actions.DPadDown.AddDefaultBinding (InputControlType.DPadDown);
			actions.DPadLeft.AddDefaultBinding (InputControlType.DPadLeft);
			actions.DPadRight.AddDefaultBinding (InputControlType.DPadRight);

			actions.Back.AddDefaultBinding (InputControlType.Back);
			actions.Start.AddDefaultBinding (InputControlType.Start);

			actions.LeftJoyButton.AddDefaultBinding (InputControlType.RightStickButton);
			actions.RightJoyButton.AddDefaultBinding (InputControlType.LeftStickButton);

			return actions;
		}
	}
}
