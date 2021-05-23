using System;

namespace Intersect.Client.Interface.Menu {

	public class Character {

		public string Class = "";

		public string[] Equipment = new string[Options.EquipmentSlots.Count + 1];

		public bool Exists = false;

		public string Face = "";

		public Guid Id;

		public int Level = 1;

		public string Name = "";

		public string Sprite = "";

		public Character(Guid id) {
			Id = id;
		}

		public Character(
			Guid id,
			string name,
			string sprite,
			string face,
			int level,
			string charClass,
			string[] equipment
		) {
			Equipment = equipment;
			Id = id;
			Name = name;
			Sprite = sprite;
			Face = face;
			Level = level;
			Class = charClass;
			Exists = true;
		}

		public Character() {
			for (int i = 0; i < Options.EquipmentSlots.Count + 1; i++) {
				Equipment[i] = "";
			}

			Equipment[0] = "Player";
		}

	}
}
