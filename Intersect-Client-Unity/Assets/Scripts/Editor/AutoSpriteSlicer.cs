using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

// This is only useful for spritesheets that need to be automatically sliced (Sprite Editor > Slice > Automatic)
public static class AutoSpriteSlicer
{
    [MenuItem("Tools/Slice Spritesheets(4x4)")]
    public static void Slice()
    {
        Texture2D[] textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);

        foreach (Texture2D texture in textures)
        {
            ProcessTexture(texture, new Vector2(texture.width / 4, texture.height / 4), new Vector2(.5f, 0f), SpriteAlignment.BottomCenter);
        }
    }

    [MenuItem("Tools/Slice Spritesheets Tile")]
    public static void SliceTile()
    {
        Texture2D[] textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);

        foreach (Texture2D texture in textures)
        {
            ProcessTexture(texture, new Vector2(32, 32), new Vector2(.5f, .5f), SpriteAlignment.Center);
        }
    }

    [MenuItem("Tools/Slice Spritesheets(Custom)")]
    public static void SliceCustom()
    {
        // Get existing open window or if none, make a new one:
        DialogWindow window = EditorWindow.GetWindow<DialogWindow>(true, "Slice Options");
        window.minSize = window.maxSize = new Vector2(400, 125);

        window.Configure(ProcessCustom);

        window.Show();
    }

    private static void ProcessCustom(Vector2Int slices, Vector2 pivot, SpriteAlignment alignment)
    {
        Texture2D[] textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);

        foreach (Texture2D texture in textures)
        {
            ProcessTexture(texture, new Vector2(texture.width / slices.x, texture.height / slices.y), pivot, alignment);
        }
    }

    private class DialogWindow : EditorWindow
    {
        private Vector2Int slices = new Vector2Int(1, 1);
        private Vector2 pivot = new Vector2(.5f, .5f);
        private SpriteAlignment alignment = SpriteAlignment.Center;
        private Action<Vector2Int, Vector2, SpriteAlignment> onConfirm;

        public void Configure(Action<Vector2Int, Vector2, SpriteAlignment> onConfirm)
        {
            Configure(onConfirm, slices);
        }

        public void Configure(Action<Vector2Int, Vector2, SpriteAlignment> onConfirm, Vector2Int slices)
        {
            Configure(onConfirm, slices, pivot, alignment);
        }

        public void Configure(Action<Vector2Int, Vector2, SpriteAlignment> onConfirm, Vector2Int slices, Vector2 pivot, SpriteAlignment alignment)
        {
            this.slices = slices;
            this.pivot = pivot;
            this.alignment = alignment;
            this.onConfirm = onConfirm;
        }

        void OnGUI()
        {
            slices = EditorGUILayout.Vector2IntField("Slice Parts", slices);
            pivot = EditorGUILayout.Vector2Field("Pivot", pivot);
            alignment = (SpriteAlignment)EditorGUILayout.EnumPopup("Alignament", alignment);

            if (GUILayout.Button("Confirm"))
            {
                onConfirm?.Invoke(slices, pivot, alignment);
            }
        }
    }

    [MenuItem("Tools/Delete SpriteSheet")]
    public static void DeleteSprites()
    {
        Texture2D[] textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);

        foreach (Texture2D texture in textures)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritesheet = null;
            AssetDatabase.ForceReserializeAssets(new List<string> { path }, ForceReserializeAssetsOptions.ReserializeMetadata);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        }
    }

    static void ProcessTexture(Texture2D texture, Vector2 size, Vector2 pivot, SpriteAlignment alignment)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = 32;

        TextureImporterSettings textureSettings = new TextureImporterSettings(); // need this stupid class because spriteExtrude and spriteMeshType aren't exposed on TextureImporter
        importer.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.Tight;
        textureSettings.spriteExtrude = 0;
        importer.SetTextureSettings(textureSettings);
        AssetDatabase.ImportAsset(path);

        Rect[] rects = InternalSpriteUtility.GenerateGridSpriteRectangles(texture, Vector2.zero, size, Vector2.zero, true);
        importer.textureCompression = TextureImporterCompression.Compressed;
        string filenameNoExtension = Path.GetFileNameWithoutExtension(path);
        List<SpriteMetaData> metas = new List<SpriteMetaData>();

        int rectNum = 0;
        foreach (Rect rect in rects)
        {
            SpriteMetaData meta = new SpriteMetaData
            {
                pivot = pivot,
                alignment = (int)alignment,
                rect = rect,
                name = $"{filenameNoExtension}_{rectNum++}"
            };
            metas.Add(meta);
        }

        importer.spritesheet = metas.ToArray();
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    //static List<Rect> SortRects(List<Rect> rects, float textureWidth) {
    //	List<Rect> list = new List<Rect>();
    //	while (rects.Count > 0) {
    //		Rect rect = rects[rects.Count - 1];
    //		Rect sweepRect = new Rect(0f, rect.yMin, textureWidth, rect.height);
    //		List<Rect> list2 = RectSweep(rects, sweepRect);
    //		if (list2.Count <= 0) {
    //			list.AddRange(rects);
    //			break;
    //		}
    //		list.AddRange(list2);
    //	}
    //	return list;
    //}

    //static List<Rect> RectSweep(List<Rect> rects, Rect sweepRect) {
    //	List<Rect> result;
    //	if (rects == null || rects.Count == 0) {
    //		result = new List<Rect>();
    //	} else {
    //		List<Rect> list = new List<Rect>();
    //		foreach (Rect current in rects) {
    //			if (current.Overlaps(sweepRect)) {
    //				list.Add(current);
    //			}
    //		}
    //		foreach (Rect current2 in list) {
    //			rects.Remove(current2);
    //		}
    //		list.Sort((a, b) => a.x.CompareTo(b.x));
    //		result = list;
    //	}
    //	return result;
    //}
}