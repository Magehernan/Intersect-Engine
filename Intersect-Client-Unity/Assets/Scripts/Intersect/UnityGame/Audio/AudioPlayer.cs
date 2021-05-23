using System;
using UnityEngine;

namespace Intersect.Client.UnityGame.Audio {

	public class AudioPlayer : MonoBehaviour {
		[SerializeField]
		private AudioSource audioSource = default;

		public bool IsPlaying => audioSource.isPlaying;

		internal void Destroy() {
			Destroy(gameObject);
		}

		internal void SetVolume(int soundVolume) {
			audioSource.volume = soundVolume / 100f;
		}

		internal void Play() {
			audioSource.Play();
		}

		internal void SetClip(AudioClip clip) {
			audioSource.clip = clip;
		}

		internal void SetLoop(bool loop) {
			audioSource.loop = loop;
		}
	}
}