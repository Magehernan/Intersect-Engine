using Intersect.Client.Framework.Content;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.UnityGame.Audio;
using Intersect.Client.UnityGame.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Intersect.Client.UnityGame.FileManagement
{

    internal class UnityContentManager : GameContentManager
    {
        private readonly AssetReferences assetReferences;

        public UnityContentManager(AssetReferences assetReferences)
        {
            Current = this;
            this.assetReferences = assetReferences;
        }

        private void LoadDictionary(List<UnityTexture> textures, Dictionary<string, IAsset> dictionary)
        {
            foreach (UnityTexture texture in textures)
            {
                dictionary.Add(texture.Name, texture);
            }
        }

        public override void LoadAnimations()
        {
            LoadDictionary(assetReferences.animations, mAnimationDict);
        }

        public override void LoadEntities()
        {
            LoadDictionary(assetReferences.entities, mEntityDict);
        }

        public override void LoadFaces()
        {
            LoadDictionary(assetReferences.faces, mFaceDict);
        }

        public override void LoadFogs()
        {
            LoadDictionary(assetReferences.fogs, mFogDict);
        }

        public override void LoadResources()
        {
            LoadDictionary(assetReferences.resources, mResourceDict);
        }

        public override void LoadImages()
        {
            LoadDictionary(assetReferences.images, mImageDict);
        }

        public override void LoadItems()
        {
            LoadDictionary(assetReferences.items, mItemDict);
        }

        public override void LoadMisc()
        {
            LoadDictionary(assetReferences.misc, mMiscDict);
        }

        public override void LoadMusic()
        {
            foreach (UnityAudioClip music in assetReferences.musics)
            {
                mMusicDict.Add(music.Name, music);
            }
        }

        public override void LoadPaperdolls()
        {
            LoadDictionary(assetReferences.paperdolls, mPaperdollDict);
        }

        public override void LoadSounds()
        {
            foreach (UnityAudioClip sound in assetReferences.sounds)
            {
                mSoundDict.Add(sound.Name, sound);
            }
        }

        public override void LoadSpells()
        {
            LoadDictionary(assetReferences.spells, mSpellDict);
        }

        public override void LoadTilesets(string[] tilesetnames)
        {
            LoadDictionary(assetReferences.tilesets, mTilesetDict);
            TilesetsLoaded = true;
        }

        protected override TAsset Load<TAsset>(Dictionary<string, IAsset> lookup, ContentTypes contentType, string assetName, Func<Stream> createStream)
        {
            return null;
        }
    }
}