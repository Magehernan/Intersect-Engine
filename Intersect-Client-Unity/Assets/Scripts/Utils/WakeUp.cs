using Intersect.Client.UnityGame;
using System.Collections.Generic;
using UnityEngine;

namespace Intersect.Client.Utils {
	public class WakeUp : MonoBehaviour {
		[SerializeField]
		private List<Window> windows = default;


		private void OnValidate() {
			windows.Clear();

			GetComponentsInChildren(true, windows);
		}

		private void Awake() {
			foreach (Window window in windows) {
				window.InitWindow();
			}

		}
	}
}
