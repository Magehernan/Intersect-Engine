using Intersect.Client.Framework.Audio;
using Intersect.Client.Framework.Content;
using Intersect.Client.Framework.Graphics;
using Intersect.Logging;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Intersect.Plugins;

namespace Intersect.Client.Framework.File_Management
{

    public abstract class GameContentManager : IContentManager
    {

        public enum TextureType
        {

            Tileset = 0,

            Item,

            Entity,

            Spell,

            Animation,

            Face,

            Image,

            Fog,

            Resource,

            Paperdoll,

            Misc,

        }

        public enum UI
        {

            Menu,

            InGame

        }

        public static GameContentManager Current;

        protected Dictionary<string, IAsset> mAnimationDict = new Dictionary<string, IAsset>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, IAsset> mEntityDict = new Dictionary<string, IAsset>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, IAsset> mFaceDict = new Dictionary<string, IAsset>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, IAsset> mFogDict = new Dictionary<string, IAsset>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, IAsset> mImageDict = new Dictionary<string, IAsset>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, IAsset> mItemDict = new Dictionary<string, IAsset>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, IAsset> mMiscDict = new Dictionary<string, IAsset>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, GameAudioSource> mMusicDict = new Dictionary<string, GameAudioSource>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, IAsset> mPaperdollDict = new Dictionary<string, IAsset>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, IAsset> mResourceDict = new Dictionary<string, IAsset>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, GameAudioSource> mSoundDict = new Dictionary<string, GameAudioSource>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, IAsset> mSpellDict = new Dictionary<string, IAsset>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, IAsset> mTilesetDict = new Dictionary<string, IAsset>(StringComparer.OrdinalIgnoreCase);

        public bool TilesetsLoaded = false;

        //Content Loading
        public void LoadAll()
        {
            LoadEntities();
            LoadItems();
            LoadAnimations();
            LoadSpells();
            LoadFaces();
            LoadImages();
            LoadFogs();
            LoadResources();
            LoadPaperdolls();
            LoadMisc();
        }

        public abstract void LoadTilesets(string[] tilesetnames);

        public abstract void LoadItems();

        public abstract void LoadEntities();

        public abstract void LoadSpells();

        public abstract void LoadAnimations();

        public abstract void LoadFaces();

        public abstract void LoadImages();

        public abstract void LoadFogs();

        public abstract void LoadResources();

        public abstract void LoadPaperdolls();

        public abstract void LoadMisc();

        //Audio Loading
        public void LoadAudio()
        {
            LoadSounds();
            LoadMusic();
        }

        public abstract void LoadSounds();

        public abstract void LoadMusic();

        public static string RemoveExtension(string fileName)
        {
            int fileExtPos = fileName.LastIndexOf(".");
            if (fileExtPos >= 0)
            {
                fileName = fileName.Substring(0, fileExtPos);
            }

            return fileName;
        }

        public string[] GetTextureNames(TextureType type)
        {
            switch (type)
            {
                case TextureType.Tileset:
                    return mTilesetDict.Keys.ToArray();
                case TextureType.Item:
                    return mItemDict.Keys.ToArray();
                case TextureType.Entity:
                    return mEntityDict.Keys.ToArray();
                case TextureType.Spell:
                    return mSpellDict.Keys.ToArray();
                case TextureType.Animation:
                    return mAnimationDict.Keys.ToArray();
                case TextureType.Face:
                    return mFaceDict.Keys.ToArray();
                case TextureType.Image:
                    return mImageDict.Keys.ToArray();
                case TextureType.Fog:
                    return mFogDict.Keys.ToArray();
                case TextureType.Resource:
                    return mResourceDict.Keys.ToArray();
                case TextureType.Paperdoll:
                    return mPaperdollDict.Keys.ToArray();
                case TextureType.Misc:
                    return mMiscDict.Keys.ToArray();
            }

            return null;
        }

        //Content Getters
        public virtual GameTexture GetTexture(TextureType type, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            IDictionary<string, IAsset> textureDict;
            switch (type)
            {
                case TextureType.Tileset:
                    textureDict = mTilesetDict;
                    break;

                case TextureType.Item:
                    textureDict = mItemDict;

                    break;

                case TextureType.Entity:
                    textureDict = mEntityDict;

                    break;

                case TextureType.Spell:
                    textureDict = mSpellDict;

                    break;

                case TextureType.Animation:
                    textureDict = mAnimationDict;

                    break;

                case TextureType.Face:
                    textureDict = mFaceDict;

                    break;

                case TextureType.Image:
                    textureDict = mImageDict;

                    break;

                case TextureType.Fog:
                    textureDict = mFogDict;

                    break;

                case TextureType.Resource:
                    textureDict = mResourceDict;

                    break;

                case TextureType.Paperdoll:
                    textureDict = mPaperdollDict;

                    break;

                case TextureType.Misc:
                    textureDict = mMiscDict;

                    break;

                default:
                    return null;
            }

            if (textureDict == null)
            {
                return null;
            }

            return textureDict.TryGetValue(name, out IAsset asset) ? asset as GameTexture : default;
        }

        public virtual GameAudioSource GetMusic(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (mMusicDict == null)
            {
                return null;
            }

            return mMusicDict.TryGetValue(name, out GameAudioSource music) ? music : null;
        }

        public virtual GameAudioSource GetSound(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (mSoundDict == null)
            {
                return null;
            }

            return mSoundDict.TryGetValue(name, out GameAudioSource sound) ? sound : null;
        }

        protected Dictionary<string, IAsset> GetAssetLookup(ContentTypes contentType)
        {
            switch (contentType)
            {
                case ContentTypes.Animation:
                    return mAnimationDict;

                case ContentTypes.Entity:
                    return mEntityDict;

                case ContentTypes.Face:
                    return mFaceDict;

                case ContentTypes.Fog:
                    return mFogDict;

                case ContentTypes.Image:
                    return mImageDict;

                case ContentTypes.Item:
                    return mItemDict;

                case ContentTypes.Miscellaneous:
                    return mMiscDict;

                case ContentTypes.Paperdoll:
                    return mPaperdollDict;

                case ContentTypes.Resource:
                    return mResourceDict;

                case ContentTypes.Spell:
                    return mSpellDict;

                case ContentTypes.TileSet:
                    return mTilesetDict;

                default:
                    throw new ArgumentOutOfRangeException(nameof(contentType), contentType, null);
            }
        }

        [NotNull]
        protected abstract TAsset Load<TAsset>(
            [NotNull] Dictionary<string, IAsset> lookup,
            ContentTypes contentType,
            [NotNull] string assetName,
            [NotNull] Func<Stream> createStream
        ) where TAsset : class, IAsset;

        /// <inheritdoc />
        public TAsset Load<TAsset>(ContentTypes contentType, string assetPath) where TAsset : class, IAsset
        {
            if (!File.Exists(assetPath))
            {
                throw new FileNotFoundException($@"Asset does not exist at '{assetPath}'.");
            }

            return Load<TAsset>(contentType, assetPath, () => File.OpenRead(assetPath));
        }

        /// <inheritdoc />
        public TAsset Load<TAsset>(ContentTypes contentType, string assetName, Func<Stream> createStream)
            where TAsset : class, IAsset
        {
            Dictionary<string, IAsset> assetLookup = GetAssetLookup(contentType);

            if (assetLookup.TryGetValue(assetName, out IAsset asset))
            {
                return asset as TAsset;
            }

            return Load<TAsset>(assetLookup, contentType, assetName, createStream);
        }

        /// <inheritdoc />
        public TAsset LoadEmbedded<TAsset>(IPluginContext context, ContentTypes contentType, string assetName)
            where TAsset : class, IAsset
        {
            string manifestResourceName = context.EmbeddedResources.Resolve(assetName);
            return Load<TAsset>(contentType, assetName, () => context.EmbeddedResources.Read(manifestResourceName));
        }

    }

}
