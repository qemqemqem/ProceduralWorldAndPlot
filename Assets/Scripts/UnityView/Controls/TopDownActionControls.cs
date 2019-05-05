namespace CSD
{
	using InControl;

	//TODO make a generic action set instead
	public class TopDownActions : PlayerActionSet
	{
		public PlayerAction Jump;
		public PlayerAction Crouch;
		public PlayerAction Interact;
		public PlayerAction Command;
		public PlayerAction MoveLeft;
		public PlayerAction MoveRight;
		public PlayerAction MoveUp;
		public PlayerAction MoveDown;
		public PlayerTwoAxisAction Move;
		public PlayerAction FaceLeft;
		public PlayerAction FaceRight;
		public PlayerAction FaceUp;
		public PlayerAction FaceDown;
		public PlayerTwoAxisAction Rotate;
		public PlayerAction Primary;
		public PlayerAction Secondary;
		public PlayerAction TriggerAction;
		public PlayerAction LockOn;
		public PlayerAction SpeechUp;
		public PlayerAction SpeechDown;
		public PlayerAction SpeechLeft;
		public PlayerAction SpeechRight;
		public PlayerAction Back;
		public PlayerAction Start;
		public PlayerAction Bonus1;
		public PlayerAction Bonus2;
		//TODO Unused dpad, menu button, back button, joystick buttons, right trigger
		//TODO dpad for quick items???
		//TODO Primary2


		public TopDownActions()
		{
			Jump = CreatePlayerAction( "Jump" );
			Crouch = CreatePlayerAction( "Crouch" );
			Interact = CreatePlayerAction( "Interact" );
			Command = CreatePlayerAction( "Command" );
			MoveLeft = CreatePlayerAction( "MoveLeft" );
			MoveRight = CreatePlayerAction( "MoveRight" );
			MoveUp = CreatePlayerAction( "MoveUp" );
			MoveDown = CreatePlayerAction( "MoveDown" );
			Move = CreateTwoAxisPlayerAction( MoveLeft, MoveRight, MoveDown, MoveUp );
			FaceLeft = CreatePlayerAction( "FaceLeft" );
			FaceRight = CreatePlayerAction( "FaceRight" );
			FaceUp = CreatePlayerAction( "FaceUp" );
			FaceDown = CreatePlayerAction( "FaceDown" );
			Rotate = CreateTwoAxisPlayerAction( FaceLeft, FaceRight, FaceDown, FaceUp );
			Primary = CreatePlayerAction( "Primary" );
			Secondary = CreatePlayerAction( "Secondary" );
			TriggerAction = CreatePlayerAction ("TriggerAction");
			LockOn = CreatePlayerAction ("LockOn");
			SpeechUp = CreatePlayerAction ("SpeechUp");
			SpeechDown = CreatePlayerAction ("SpeechDown");
			SpeechLeft = CreatePlayerAction ("SpeechLeft");
			SpeechRight = CreatePlayerAction ("SpeechRight");
			Back = CreatePlayerAction ("Back");
			Start = CreatePlayerAction ("Start");
			Bonus1 = CreatePlayerAction ("Bonus1");
			Bonus2 = CreatePlayerAction ("Bonus2");

		}


		public static TopDownActions CreateWithKeyboardBindings()
		{
			var actions = new TopDownActions();

			actions.Jump.AddDefaultBinding( Key.Space );
			actions.Crouch.AddDefaultBinding( Key.LeftShift );
			actions.Interact.AddDefaultBinding( Key.E );
			actions.Command.AddDefaultBinding( Key.Q );

			actions.MoveUp.AddDefaultBinding( Key.W );
			actions.MoveDown.AddDefaultBinding( Key.S );
			actions.MoveLeft.AddDefaultBinding( Key.A );
			actions.MoveRight.AddDefaultBinding( Key.D );

			actions.FaceUp.AddDefaultBinding( Mouse.PositiveY );
			actions.FaceDown.AddDefaultBinding( Mouse.NegativeY );
			actions.FaceLeft.AddDefaultBinding( Mouse.NegativeX );
			actions.FaceRight.AddDefaultBinding( Mouse.PositiveX );

			actions.Primary.AddDefaultBinding (Mouse.LeftButton);
			actions.Secondary.AddDefaultBinding (Mouse.RightButton);
			actions.LockOn.AddDefaultBinding (Mouse.MiddleButton);

			return actions;
		}


		public static TopDownActions CreateWithJoystickBindings()
		{
			var actions = new TopDownActions();

			actions.Jump.AddDefaultBinding( InputControlType.Action1 );
			actions.Crouch.AddDefaultBinding( InputControlType.Action2 );
			actions.Interact.AddDefaultBinding( InputControlType.Action3 );
			actions.Command.AddDefaultBinding( InputControlType.Action4 );

			actions.MoveUp.AddDefaultBinding( InputControlType.LeftStickUp );
			actions.MoveDown.AddDefaultBinding( InputControlType.LeftStickDown );
			actions.MoveLeft.AddDefaultBinding( InputControlType.LeftStickLeft );
			actions.MoveRight.AddDefaultBinding( InputControlType.LeftStickRight );

			actions.FaceUp.AddDefaultBinding( InputControlType.RightStickUp );
			actions.FaceDown.AddDefaultBinding( InputControlType.RightStickDown );
			actions.FaceLeft.AddDefaultBinding( InputControlType.RightStickLeft );
			actions.FaceRight.AddDefaultBinding( InputControlType.RightStickRight );

			actions.Primary.AddDefaultBinding (InputControlType.RightBumper);
			actions.Secondary.AddDefaultBinding (InputControlType.LeftBumper);
			actions.TriggerAction.AddDefaultBinding (InputControlType.RightTrigger);
			actions.LockOn.AddDefaultBinding (InputControlType.LeftTrigger);

			actions.SpeechUp.AddDefaultBinding (InputControlType.DPadUp);
			actions.SpeechDown.AddDefaultBinding (InputControlType.DPadDown);
			actions.SpeechLeft.AddDefaultBinding (InputControlType.DPadLeft);
			actions.SpeechRight.AddDefaultBinding (InputControlType.DPadRight);

			actions.Back.AddDefaultBinding (InputControlType.Back);
			actions.Start.AddDefaultBinding (InputControlType.Start);

			actions.Bonus1.AddDefaultBinding (InputControlType.RightStickButton);
			actions.Bonus2.AddDefaultBinding (InputControlType.LeftStickButton);



			return actions;
		}
	}
}
