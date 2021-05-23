using Intersect.Client.Framework.Audio;
using System;
using UnityEngine;

namespace Intersect.Client.UnityGame.Audio {

	[Serializable]
	public class UnityAudioClip : GameAudioSource {
		[SerializeField]
		private string name = string.Empty;
		[SerializeField]
		private AudioClip clip = default;

		public override AudioClip Clip => clip;

		public string Name => name;

		public UnityAudioClip(AudioClip clip, string name) {
			this.clip = clip;
			this.name = name;
		}
	}
}
