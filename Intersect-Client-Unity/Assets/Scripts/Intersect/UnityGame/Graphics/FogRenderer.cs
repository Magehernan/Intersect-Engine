using Intersect.Client.Utils;
using System;
using UnityEngine;

namespace Intersect.Client.UnityGame.Graphics {
	public class FogRenderer : MonoBehaviour {
		[SerializeField]
		private SpriteRenderer spriteRenderer = default;

		private Transform myTransform;

		private GameObject myGameObject;

		private Sprite currentFog = null;
		private Vector2 viewSize;

		private void Awake() {
			myTransform = transform;
			myGameObject = gameObject;
		}

		public void ChangeFog(Sprite sprite, byte alpha) {
			if (sprite == null || alpha == 0) {
				myGameObject.SetActive(false);
				return;
			}
			Vector2 currentSize = Core.Graphics.mainCamera.OrthographicBounds().size;
			if (currentFog == null || currentSize != viewSize) {
				currentFog = sprite;
				spriteRenderer.sprite = sprite;
				viewSize = currentSize;


				float width = sprite.rect.width / sprite.pixelsPerUnit;
				int xCount = (int)((Options.MapWidth + viewSize.x + width) / width) + 2;
				float height = sprite.rect.height / sprite.pixelsPerUnit;
				int yCount = (int)((Options.MapHeight + viewSize.y + height) / height) + 2;

				spriteRenderer.size = new Vector2(xCount * width, yCount * height);
			}
			myGameObject.SetActive(true);
			spriteRenderer.color = new Color32(255, 255, 255, alpha);
		}

		public void ChangePosition(float x, float y, float mapX, float mapY) {
			if (myGameObject.activeSelf && currentFog != null) {
				myTransform.position = new Vector2(mapX + x / currentFog.pixelsPerUnit, -mapY - y / currentFog.pixelsPerUnit);
			}
		}
	}
}