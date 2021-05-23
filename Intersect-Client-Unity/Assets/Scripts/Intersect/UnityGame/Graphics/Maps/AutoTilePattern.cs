using System;
using UnityEngine;

namespace Intersect.Client.UnityGame.Graphics.Maps {
	public class AutoTilePattern {
		public string name = string.Empty;
		public int[] Pattern = new int[4];
		public int Mask = 0x00000000;
		public int Combination;
		public Sprite[] Frames;
	}
}