using Intersect.Client.UnityGame;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Intersect.Client.Utils {
	public class Singleton : MonoBehaviour {
		private static Singleton instance;
		public static Singleton Instance => instance;

		public IntersectGame intersectGame;

#if UNITY_EDITOR
		private void OnValidate() {
			intersectGame = GetComponent<IntersectGame>();
		}
#endif

		private void Awake() {
			UnitySystemConsoleRedirector.Redirect();
			if (Interlocked.CompareExchange(ref instance, this, null) == null) {
				Debug.Log("Singleton Setted");
				DontDestroyOnLoad(gameObject);

			} else {
				Debug.LogError("Singleton Destroyed");
				Destroy(gameObject);
			}
		}

		[System.Diagnostics.Conditional("UNIMPLEMENTED")]
		public static void Unimplemented(string name) {
			Debug.LogWarning($"Unimplemented: {name}");
		}
	}


	/// <summary>
	/// Redirects writes to System.Console to Unity3D's Debug.Log.
	/// </summary>
	/// <author>
	/// Jackson Dunstan, http://jacksondunstan.com/articles/2986
	/// </author>
	public static class UnitySystemConsoleRedirector {
		private class UnityTextWriter : TextWriter {
			private StringBuilder buffer = new StringBuilder();
			private object lockString = new object();

			public override void Flush() {
				lock (lockString) {
					Debug.Log(buffer.ToString());
					buffer.Length = 0;
				}
			}

			public override void Write(string value) {
				lock (lockString) {
					buffer.Append(value);
					if (value != null) {
						int len = value.Length;
						if (len > 0) {
							char lastChar = value[len - 1];
							if (lastChar == '\n') {
								Flush();
							}
						}
					}
				}
			}

			public override void Write(char value) {
				lock (lockString) {
					buffer.Append(value);
					if (value == '\n') {
						Flush();
					}
				}
			}

			public override void Write(char[] value, int index, int count) {
				Write(new string(value, index, count));
			}

			public override Encoding Encoding {
				get { return Encoding.Default; }
			}
		}

		public static void Redirect() {
			Console.SetOut(new UnityTextWriter());
		}
	}


	public static class CameraExtensions {
		public static Bounds OrthographicBounds(this Camera camera) {
			float screenAspect = Screen.width / (float)Screen.height;
			float cameraHeight = camera.orthographicSize * 2;
			Bounds bounds = new Bounds(
				camera.transform.position,
				new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
			return bounds;
		}
	}
}