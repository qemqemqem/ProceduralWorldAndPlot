using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace CSD{

	public class ControllerDrivenUIButton : MonoBehaviour {
		public TextMeshProUGUI text;
		public UnityEngine.UI.Image image;
		public UnityEngine.Color focusColor;
		public UnityEngine.Color baseColor;
		public MenuButton menuButton;

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		public void SetMenuButton(MenuButton button){
			this.menuButton=button;
			if (this.menuButton == null)
				return;
			text.text = button.text;
			image.material.color = baseColor;
		}

		public void Focus(){
			if (menuButton == null)
				return;
			image.material.color = focusColor;
			if(menuButton.onFocus!=null)
				menuButton.onFocus.Invoke ();
		}

		public void Click(){
			if (menuButton == null)
				return;
			if(menuButton.onClick!=null)
				menuButton.onClick.Invoke ();
		}
	}

}