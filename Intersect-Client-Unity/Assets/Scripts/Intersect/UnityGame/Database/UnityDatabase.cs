using Intersect.Client.Framework.Database;
using Intersect.Configuration;
using System;

namespace Intersect.Client.UnityGame.Database {

	internal class UnityDatabase : GameDatabase {

		//Saving password, other stuff we don't want in the games directory
		public override void SavePreference(string key, object value) {
			UnityEngine.PlayerPrefs.SetString($"{ClientConfiguration.Instance.Host}:{ClientConfiguration.Instance.Port}:{key}", value.ToString());
		}

		public override string LoadPreference(string key) {
			return UnityEngine.PlayerPrefs.GetString($"{ClientConfiguration.Instance.Host}:{ClientConfiguration.Instance.Port}:{key}", string.Empty);
		}

		public override bool LoadConfig() {
			ClientConfiguration.LoadAndSave();

			return true;
		}
	}
}