using System;

namespace Intersect.Client.Interface.Menu
{
	public class Character
	{
		private const string player = "Player";

		public string Class { get; set; } = string.Empty;

		public string[] Equipment { get; set; } = new string[Options.EquipmentSlots.Count + 1];

		public bool Exists { get; set; } = false;

		public string Face { get; set; } = string.Empty;

		public Guid Id { get; set; }

		public int Level { get; set; } = 1;

		public string Name { get; set; } = string.Empty;

		public string Sprite { get; set; } = string.Empty;

		public Character(Guid id)
		{
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
		)
		{
			Equipment = equipment;
			Id = id;
			Name = name;
			Sprite = sprite;
			Face = face;
			Level = level;
			Class = charClass;
			Exists = true;
		}

		public Character()
		{
			for (int i = 0; i < Options.EquipmentSlots.Count + 1; i++)
			{
				Equipment[i] = string.Empty;
			}

			Equipment[0] = player;
		}
	}
}
