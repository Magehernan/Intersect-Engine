using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Intersect.Client.UnityGame.Graphics;
using static Intersect.Client.Framework.File_Management.GameContentManager;
using Intersect.Client.UnityGame.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Intersect.Client.UnityGame.FileManagement
{
    [CreateAssetMenu(fileName = "AssetReferences", menuName = "AssetReferences", order = 390)]
    public class AssetReferences : ScriptableObject
    {
        public List<UnityTexture> tilesets;
        public List<UnityTexture> entities;
        public List<UnityTexture> paperdolls;
        public List<UnityTexture> resources;
        public List<UnityTexture> images;
        public List<UnityTexture> fogs;
        public List<UnityTexture> animations;
        public List<UnityTexture> items;
        public List<UnityTexture> misc;
        public List<UnityTexture> spells;
        public List<UnityTexture> faces;


        public List<UnityAudioClip> musics;
        public List<UnityAudioClip> sounds;
        public const string BASE_PATH = "Assets/IntersectResources/";
        public const string TILESETS_PATH = BASE_PATH + "tilesets";
        public const string ENTITIES_PATH = BASE_PATH + "entities";
        public const string PAPERDOLL_PATH = BASE_PATH + "paperdolls";
        public const string RESOURCES_PATH = BASE_PATH + "resource";
        public const string IMAGES_PATH = BASE_PATH + "images";
        public const string FOGS_PATH = BASE_PATH + "fogs";
        public const string ANIMATIONS_PATH = BASE_PATH + "animations";
        public const string ITEMS_PATH = BASE_PATH + "items";
        public const string MISC_PATH = BASE_PATH + "misc";
        public const string SPELLS_PATH = BASE_PATH + "spells";
        public const string FACES_PATH = BASE_PATH + "faces";

        public const string MUSICS_PATH = BASE_PATH + "musics";
        public const string SOUNDS_PATH = BASE_PATH + "sounds";

#if UNITY_EDITOR
        private void OnValidate()
        {
            //No queremos que recarte cuando ponemos play
            if (Application.isPlaying)
            {
                return;
            }

            LoadTexture(ref tilesets, TILESETS_PATH, TextureType.Tileset);
            LoadTexture(ref entities, ENTITIES_PATH, TextureType.Entity, 16);
            LoadTexture(ref paperdolls, PAPERDOLL_PATH, TextureType.Paperdoll, 16);
            LoadTexture(ref resources, RESOURCES_PATH, TextureType.Resource);
            LoadTexture(ref images, IMAGES_PATH, TextureType.Image);
            LoadTexture(ref fogs, FOGS_PATH, TextureType.Fog);
            LoadTexture(ref animations, ANIMATIONS_PATH, TextureType.Animation);
            LoadTexture(ref items, ITEMS_PATH, TextureType.Item);
            LoadTexture(ref misc, MISC_PATH, TextureType.Misc);
            LoadTexture(ref spells, SPELLS_PATH, TextureType.Spell);
            LoadTexture(ref faces, FACES_PATH, TextureType.Face);

            LoadAudioClip(ref musics, MUSICS_PATH, "*.ogg");
            LoadAudioClip(ref sounds, SOUNDS_PATH, "*.wav");

        }

        private void LoadAudioClip(ref List<UnityAudioClip> audioclips, string path, string extension)
        {
            if (audioclips is null)
            {
                audioclips = new List<UnityAudioClip>();
            }
            audioclips.Clear();
            if (!Directory.Exists(path))
            {
                return;
            }

            string[] filenames = Directory.GetFiles(path, extension);
            if (filenames != null)
            {
                foreach (string filename in filenames)
                {
                    AudioClip asset = AssetDatabase.LoadAssetAtPath<AudioClip>(filename);
                    audioclips.Add(new UnityAudioClip(asset, Path.GetFileName(filename)));
                }
            }
        }

        private void LoadTexture(ref List<UnityTexture> textures, string path, TextureType textureType, int spriteAmount = 0)
        {
            if (textures is null)
            {
                textures = new List<UnityTexture>();
            }
            textures.Clear();
            if (!Directory.Exists(path))
            {
                return;
            }
            string[] filenames = Directory.GetFiles(path, "*.png");
            if (filenames != null)
            {
                foreach (string filename in filenames)
                {
                    Object[] assets = AssetDatabase.LoadAllAssetsAtPath(filename);
                    List<Sprite> sprites = new List<Sprite>();
                    Texture2D mainTexture = default;
                    foreach (Object asset in assets)
                    {
                        switch (asset)
                        {
                            case Texture2D texture:
                                mainTexture = texture;
                                break;
                            case Sprite sprite:
                                sprites.Add(sprite);
                                break;
                            default:
                                Debug.LogError($"se esta intentando cargar algo que no es valid: {asset}");
                                break;
                        }
                    }
                    sprites.Sort((a, b) => (b.rect.y * 10000 - b.rect.x).CompareTo(a.rect.y * 10000 - a.rect.x));

                    if (spriteAmount > 0 && sprites.Count != spriteAmount)
                    {
                        Debug.LogError($"Sprite amount in {textureType} file {filename} has {sprites.Count} and need to be {spriteAmount}", mainTexture);
                    }

                    textures.Add(new UnityTexture(Path.GetFileName(filename), mainTexture, sprites, textureType));
                }
            }
        }
#endif
    }
}