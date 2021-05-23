using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.UnityGame.Graphics {
	public class OverlayRenderer : MonoBehaviour {
		[SerializeField]
		private Image imageOverlay = default;
		[SerializeField]
		private Canvas canvas = default;

		private GameObject myGameObject;

		private void Awake() {
			myGameObject = gameObject;
			canvas.worldCamera = Core.Graphics.mainCamera;
			myGameObject.SetActive(false);
		}

		public void Draw(Sprite spriteOverlay, float alpha) {
			if (alpha == 0f) {
				myGameObject.SetActive(false);
				return;
			}
			myGameObject.SetActive(true);
			imageOverlay.sprite = spriteOverlay;
			imageOverlay.color = new UnityEngine.Color(1f, 1f, 1f, alpha);
		}
	}
}