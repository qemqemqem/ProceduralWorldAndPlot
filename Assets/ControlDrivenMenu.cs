using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;
namespace CSD{

	//TODO replace stupid class with interfaces
	public class MenuButton{
		public string text;
		public VoidDelegate onClick;
		public VoidDelegate onFocus;
	}

	public class ControlDrivenMenu : MonoBehaviour {
		public Transform buttonPrefab;
		public Transform buttonArea;
		private ControlDrivenMenu parent;
		private UIActions actions;
		private List<ControllerDrivenUIButton> buttons = new List<ControllerDrivenUIButton> ();
		private BidirectionalListIterator<ControllerDrivenUIButton> selected;

		// Use this for initialization
		void Start () {
		}

		public void SetupMenu(InputDevice device, List<MenuButton> buttons){
			SetControl (device);
			CreateMenu (buttons);

		}

		private void SetControl(InputDevice device){
			if(actions==null)
				actions = UIActions.CreateWithJoystickBindings ();	
			actions.Device = device;
		}

		private void CreateMenu(List<MenuButton> selectables){
			if (selectables == null)
				return;
			foreach (var selectable in selectables) {
				Transform button = Transform.Instantiate (buttonPrefab);
				ControllerDrivenUIButton buttonComponent = button.GetComponent<ControllerDrivenUIButton> ();
				buttonComponent.SetMenuButton (selectable);
				button.SetParent (buttonArea);
				buttons.Add (buttonComponent);
			}
			selected = new BidirectionalListIterator<ControllerDrivenUIButton> (buttons);
		}
		
		// Update is called once per frame
		void FixedUpdate () {
			if (actions == null || actions.Device == null)
				return;
			//TODO handle the state stack differently maybe?
			if (actions.ExitState.WasPressed) {
				ExitState ();
				return;
			}
			if (actions.Back.WasPressed) {
				if(parent!=null)
					Exit ();
				return;
			}

			else {
				if (actions.Select.WasPressed) {
					Select (selected.Current);
				}
				if (actions.Edit.WasPressed) {
					//TODO open a menu push a new state for the selected thing
				} else {
					if (actions.CycleLeft.WasPressed) {
						Focus(selected.Prev ());
					}
					if (actions.CycleRight.WasPressed) {
						Focus(selected.Next ());
					}
				}
			}

		}

		public void Focus(ControllerDrivenUIButton selectable){
			selectable.Focus ();
		}

		public void Select(ControllerDrivenUIButton selectable){
			selectable.Click ();
		}

		public void ExitState(){
			Exit ();
			if (parent != null) {
				parent.ExitState ();
			}
		}

		public void Exit(){

		}
	}

	public class BidirectionalListIterator<T> : IEnumerator<T> {
		private List<T> list;
		int currIndex=0;
		int startIndex=0;

		public BidirectionalListIterator(List<T> list) : this(list, -1) {
		}

		public BidirectionalListIterator(List<T> list, int startIndex){
			this.list = list;
			this.currIndex = this.startIndex = Normalize(startIndex);
		}

		public void Dispose (){
		}

		public T Next(){
			MoveNext ();
			return Current;
		}

		public T PeekNext(){
			MoveNext ();
			T value = Current;
			MovePrevious ();
			return value;
		}

		public T Prev(){
			MovePrevious ();
			return Current;
		}

		public T PeekPrevious(){
			MovePrevious ();
			T value = Current;
			MoveNext ();
			return value;
		}

		public bool HasNext(){
			return startIndex == Normalize (this.currIndex + 1);
		}

		public bool HasPrev(){
			return startIndex == Normalize (this.currIndex - 1);
		}

		public bool MoveNext ()
		{
			this.currIndex++;
			NormalizeIndex ();
			return true;
		}

		public bool MovePrevious ()
		{
			this.currIndex--;
			NormalizeIndex ();
			return true;
		}
		public void Reset ()
		{
			this.currIndex = this.startIndex;
		}

		object IEnumerator.Current {
			get{
				return Current;
			}
		}

		public T Current {
			get {
				if(list==null||currIndex>=list.Count)
					return default(T);
				return list[currIndex];
			}
		}

		private void NormalizeIndex(){
			this.currIndex = Normalize (this.currIndex);
		}

		private int Normalize(int index){
			return (index % list.Count + list.Count) % list.Count;
		}

	}

}