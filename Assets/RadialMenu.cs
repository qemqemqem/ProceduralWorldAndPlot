using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

namespace CSD{

	/*Radial menu
	 * 
	 * I think we don't want nested layers we just want tap the button once to open the menu
	 * The menu then will several items evenly spaced around a center point, joystick and hit A to select an item B to exit the menu
	 * */


	public interface UIDisplayable{
		UnityEngine.UI.Image GetImage ();
	}

	public interface UISelectable{
		void OnSelect();
		void OnUnSelect();
		void OnHighlight();
	}

	public class InterfaceButton : UIDisplayable, UISelectable {
		UIDisplayable displayabe;
		UISelectable selectable;
		public InterfaceButton(UIDisplayable displayable, UISelectable selectable){
			this.displayabe=displayable;
			this.selectable=selectable;
		}

		public UnityEngine.UI.Image GetImage(){
			return displayabe.GetImage ();
		}

		public void OnSelect(){
			selectable.OnSelect ();
		}

		public void OnUnSelect(){
			selectable.OnUnSelect ();
		}

		public void OnHighlight(){
			selectable.OnHighlight ();
		}

	}

	public class RadialMenu : MonoBehaviour, UIDisplayable, UISelectable {

		public UnityEngine.UI.Image image;
		public Transform buttomPrefab;
		public RadialMenu radialMenuPrefab;
		public Vector3 centerOffset = Vector3.zero;
		private List<Vector3> buttonPositions = new List<Vector3> ();
		private List<InterfaceButton> buttons = new List<InterfaceButton>();
		public float arcLow = 0f;
		public float arcHigh = 360f;
		public float minSpacing = .1f;
		public float maxSize = 100f;
		private List<Object> options = new List<Object> ();
		private float openAnimationDuration=.2f;
		private bool isFocus=false;
		private bool isChildFocus=false;
		private float radius;

		public bool expandable;
		private float hoverExpandDuration=.5f;
		private PlayerTwoAxisAction selector;
		private PlayerAction choose;
		private PlayerAction back;
		private PlayerAction cycleLeft;
		private PlayerAction cycleRight;


		public void Initialize(PlayerTwoAxisAction selector, PlayerAction choose, PlayerAction back, PlayerAction cycleLeft, PlayerAction cycleRight){
			this.selector = selector;
			this.choose = choose;
			this.back = back;
			this.cycleLeft = cycleLeft;
			this.cycleRight = cycleRight;
		}

		// Use this for initialization
		void Start () {
		}
		
		// Update is called once per frame
		void Update () {
			if (selector == null || back == null || choose == null) {
				Close ();
				return;
			}
			if (!isFocus) {
				if(!isChildFocus)
					Close ();
				return;
			}
			if (selector.Angle > arcHigh || selector.Angle < arcLow)
				return;
			if (back.IsPressed)
				Close ();
			
			
		}

		public void Open(){
			//create the buttons for each of the respective radial positions and have them interpolate into their respective positions
		}

		public void Close(){
			//create the buttons for each of the respective radial positions and have them interpolate into their respective positions
		}

		private int DirectionToItem(float angle){
			float arcLength=Mathf.PI*2*radius;
			if (arcLength / (maxSize * minSpacing) > options.Count) {
			}
			float spacing = arcLength / (options.Count * minSpacing);
			if (spacing > 0)
				return 1;
			return -1;
		}

		public void AddInterfaceButton<T>(T content) where T : UISelectable, UIDisplayable{
			InterfaceButton button = new InterfaceButton (content, content);
			buttons.Add (button);
		}

		public void AddInterfaceButtons<T>(List<T> contents) where T : UISelectable, UIDisplayable{
			foreach (var content in contents) {
				AddInterfaceButton (content);
			}
		}

		public UnityEngine.UI.Image GetImage(){
			return null;
		}

		public void OnSelect(){
			Open ();
		}

		public void OnUnSelect(){
			Close ();
		}

		public void OnHighlight(){
			
		}


	}

}