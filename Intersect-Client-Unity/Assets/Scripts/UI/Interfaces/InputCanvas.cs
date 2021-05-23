using Intersect.Client.General;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.UnityGame {

	public class InputCanvas : Window {
		[SerializeField]
		private Button buttonUp = default;
		[SerializeField]
		private Button buttonLeft = default;
		[SerializeField]
		private Button buttonRight = default;
		[SerializeField]
		private Button buttonDown = default;

		//protected override MessageTypes ShowMessage => MessageTypes.JoinGamePacket;

		protected override void Awake() {
			base.Awake();

			buttonUp.onClick.AddListener(() => Move(0, 1));
			buttonDown.onClick.AddListener(() => Move(0, -1));
			buttonLeft.onClick.AddListener(() => Move(-1, 0));
			buttonRight.onClick.AddListener(() => Move(1, 0));
		}

		private void Move(int x, int y) {
			if (Globals.Me is null) {
				return;
			}
			if (x != 0) {
				//Globals.Me.movex = x;
			}
			if (y != 0) {
				//Globals.Me.movey = y;
			}
		}

		private void Update() {
			if (Globals.Me is null) {
				return;
			}

			if (UnityEngine.Input.GetKey(KeyCode.UpArrow)) {
				//Globals.Me.movey = 1;
			}

			if (UnityEngine.Input.GetKey(KeyCode.DownArrow)) {
				//Globals.Me.movey = -1;
			}

			if (UnityEngine.Input.GetKey(KeyCode.LeftArrow)) {
				//Globals.Me.movex = -1;
			}

			if (UnityEngine.Input.GetKey(KeyCode.RightArrow)) {
				//Globals.Me.movex = 1;
			}

		}
	}
}