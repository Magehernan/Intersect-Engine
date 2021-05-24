using Intersect.Client.Core;
using Intersect.Client.Entities.Projectiles;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Items;
using Intersect.Client.Localization;
using Intersect.Client.Maps;
using Intersect.Client.Spells;
using Intersect.Client.UnityGame;
using Intersect.Client.UnityGame.Graphics;
using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.GameObjects.Maps;
using Intersect.Logging;
using Intersect.Network.Packets.Server;
using Intersect.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Intersect.Client.Entities
{
    public class Entity
    {
        private const string PLAYER = "Player";
        public enum LabelType
        {

            Header = 0,

            Footer,

            Name,

            ChatBubble,

            Guild

        }

        public int AnimationFrame;

        //Entity Animations
        public List<Animation> Animations { get; } = new List<Animation>();

        //Animation Timer (for animated sprites)
        public long AnimationTimer;

        //Combat
        public long AttackTimer { get; set; } = 0;
        public int AttackTime { get; set; } = -1;

        public bool Blocking = false;

        //Combat Status
        public long CastTime = 0;

        //Dashing instance
        public Dash Dashing;

        public Queue<Dash> DashQueue = new Queue<Dash>();

        public long DashTimer;

        public float elapsedtime; //to be removed

        public Guid[] Equipment = new Guid[Options.EquipmentSlots.Count];

        public Animation[] EquipmentAnimations = new Animation[Options.EquipmentSlots.Count];

        //Extras
        public string Face = string.Empty;

        public Label FooterLabel;

        public Gender Gender = Gender.Male;

        public Label HeaderLabel;

        public bool HideEntity = false;

        public bool HideName;

        //Core Values
        public Guid Id;

        //Inventory/Spells/Equipment
        public Item[] Inventory = new Item[Options.MaxInvItems];

        public bool InView = true;

        public bool IsLocal = false;

        public bool IsMoving;

        //Caching
        public MapInstance LatestMap;

        public int Level = 1;

        //Vitals & Stats
        public int[] MaxVital = new int[(int)Vitals.VitalCount];

        protected Pointf worldPos = Pointf.Empty;

        private byte mDir;

        //private byte mDeplDir;

        protected bool mDisposed;

        private long mLastUpdate;

        protected string mMySprite = string.Empty;

        public Color Color = new Color(255, 255, 255, 255);

        public int MoveDir = -1;

        public long MoveTimer;

        protected byte mRenderPriority = 1;

        protected string mTransformedSprite = string.Empty;

        private long mWalkTimer;

        public int[] MyEquipment = new int[Options.EquipmentSlots.Count];

        public string Name = string.Empty;

        public Color NameColor = null;

        public float OffsetX;

        public float OffsetY;

        public bool Passable;

        //Rendering Variables
        public HashSet<Entity> RenderList;

        public Guid SpellCast;

        public Spell[] Spells = new Spell[Options.MaxPlayerSkills];

        public int[] Stat = new int[(int)Stats.StatCount];

        public int Target = -1;

        public GameTexture Texture;

        #region "Animation Textures and Timing"
        public SpriteAnimations SpriteAnimation = SpriteAnimations.Normal;

        public Dictionary<SpriteAnimations, GameTexture> AnimatedTextures = new Dictionary<SpriteAnimations, GameTexture>();

        public int SpriteFrame = 0;

        public long SpriteFrameTimer = -1;

        public long LastActionTime = -1;

        public const long TimeBeforeIdling = 100;

        public const long IdleFrameDuration = 200;
        #endregion

        public int Type;

        public int[] Vital = new int[(int)Vitals.VitalCount];

        public int WalkFrame;

        public FloatRect WorldRect = new FloatRect();

        //Location Info
        public byte X;

        public byte Y;

        public byte Z;

        protected EntityRenderer entityRender;

        private List<ChatBubbleRenderer> chatBubbleRenderers;

        private ConcurrentQueue<string> chatBubbles;

        public Entity(Guid id, EntityPacket packet, bool isEvent = false)
        {
            Id = id;
            CurrentMap = Guid.Empty;
            if (id != Guid.Empty && !isEvent)
            {
                for (int i = 0; i < Options.MaxInvItems; i++)
                {
                    Inventory[i] = new Item();
                }

                for (int i = 0; i < Options.MaxPlayerSkills; i++)
                {
                    Spells[i] = new Spell();
                }

                for (int i = 0; i < Options.EquipmentSlots.Count; i++)
                {
                    Equipment[i] = Guid.Empty;
                    MyEquipment[i] = -1;
                }
            }

            AnimationTimer = Globals.System.GetTimeMs() + Globals.Random.Next(0, 500);

            //TODO Remove because fixed orrrrr change the exception text
            if (Options.EquipmentSlots.Count == 0)
            {
                throw new Exception("What the fuck is going on!?!?!?!?!?!");
            }

            Load(packet);
        }

        //Status effects
        public List<Status> Status { get; private set; } = new List<Status>();

        public byte Dir
        {
            get => mDir;
            set => mDir = (byte)((value + 4) % 4);
        }

        public virtual string TransformedSprite
        {
            get => mTransformedSprite;
            set
            {
                mTransformedSprite = value;
                Texture = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Entity, mTransformedSprite);
                LoadAnimationTextures(mTransformedSprite);
                if (string.IsNullOrEmpty(value))
                {
                    MySprite = mMySprite;
                }
            }
        }

        public virtual string MySprite
        {
            get => mMySprite;
            set
            {
                mMySprite = value;
                Texture = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Entity, mMySprite);
                LoadAnimationTextures(mMySprite);
            }
        }

        public virtual int SpriteFrames
        {
            get
            {
                switch (SpriteAnimation)
                {
                    case SpriteAnimations.Normal:
                        return Options.Instance.Sprites.NormalFrames;
                    case SpriteAnimations.Idle:
                        return Options.Instance.Sprites.IdleFrames;
                    case SpriteAnimations.Attack:
                        return Options.Instance.Sprites.AttackFrames;
                    case SpriteAnimations.Shoot:
                        return Options.Instance.Sprites.ShootFrames;
                    case SpriteAnimations.Cast:
                        return Options.Instance.Sprites.CastFrames;
                    case SpriteAnimations.Weapon:
                        return Options.Instance.Sprites.WeaponFrames;
                }

                return Options.Instance.Sprites.NormalFrames;
            }
        }

        public MapInstance MapInstance => MapInstance.Get(CurrentMap);

        public virtual Guid CurrentMap { get; set; }

        public virtual EntityTypes GetEntityType()
        {
            return EntityTypes.GlobalEntity;
        }

        //Deserializing
        public virtual void Load(EntityPacket packet)
        {
            if (packet == null)
            {
                return;
            }

            CurrentMap = packet.MapId;
            Name = packet.Name;
            Log.Info($"Load Entity {Name}");

            if (entityRender == null)
            {
                switch (packet)
                {
                    case PlayerEntityPacket _:
                        entityRender = UnityFactory.GetEntityRenderer(EntityTypes.Player, Name);
                        break;
                    case EventEntityPacket _:
                        entityRender = UnityFactory.GetEntityRenderer(EntityTypes.Event, Name);
                        break;
                    case ProjectileEntityPacket _:
                        entityRender = UnityFactory.GetEntityRenderer(EntityTypes.Projectile, Name);
                        break;
                    case ResourceEntityPacket _:
                        entityRender = UnityFactory.GetEntityRenderer(EntityTypes.Resource, Name);
                        break;
                    case EntityPacket _:
                        entityRender = UnityFactory.GetEntityRenderer(EntityTypes.GlobalEntity, Name);
                        break;
                    default:
                        throw new NotImplementedException($"There isn't type for this entity: {packet.GetType()}");
                }
            }

            MySprite = packet.Sprite;
            Color = packet.Color;
            Face = packet.Face;
            Level = packet.Level;
            X = packet.X;
            Y = packet.Y;
            Z = packet.Z;
            Dir = packet.Dir;
            Passable = packet.Passable;
            HideName = packet.HideName;
            HideEntity = packet.HideEntity;
            NameColor = packet.NameColor;
            HeaderLabel = new Label(packet.HeaderLabel.Label, packet.HeaderLabel.Color);
            FooterLabel = new Label(packet.FooterLabel.Label, packet.FooterLabel.Color);

            List<Animation> animsToClear = new List<Animation>();
            List<AnimationBase> animsToAdd = new List<AnimationBase>();
            for (int i = 0; i < packet.Animations.Length; i++)
            {
                AnimationBase anim = AnimationBase.Get(packet.Animations[i]);
                if (anim != null)
                {
                    animsToAdd.Add(anim);
                }
            }

            foreach (Animation anim in Animations)
            {
                animsToClear.Add(anim);
                if (!anim.InfiniteLoop)
                {
                    animsToClear.Remove(anim);
                }
                else
                {
                    foreach (AnimationBase addedAnim in animsToAdd)
                    {
                        if (addedAnim.Id == anim.MyBase.Id)
                        {
                            animsToClear.Remove(anim);
                            animsToAdd.Remove(addedAnim);

                            break;
                        }
                    }

                    foreach (Animation equipAnim in EquipmentAnimations)
                    {
                        if (equipAnim == anim)
                        {
                            animsToClear.Remove(anim);
                        }
                    }
                }
            }

            ClearAnimations(animsToClear);
            AddAnimations(animsToAdd);

            Vital = packet.Vital;
            MaxVital = packet.MaxVital;

            //Update status effects
            Status.Clear();

            if (packet.StatusEffects == null)
            {
                Log.Warn($"'{nameof(packet)}.{nameof(packet.StatusEffects)}' is null.");
            }
            else
            {
                foreach (StatusPacket status in packet.StatusEffects)
                {
                    Status instance = new Status(
                        status.SpellId, status.Type, status.TransformSprite, status.TimeRemaining, status.TotalDuration
                    );

                    Status?.Add(instance);

                    if (instance.Type == StatusTypes.Shield)
                    {
                        instance.Shield = status.VitalShields;
                    }
                }
            }

            SortStatuses();
            Stat = packet.Stats;

            mDisposed = false;

            //Status effects box update
            if (Globals.Me == null)
            {
                Log.Warn($"'{nameof(Globals.Me)}' is null.");
            }
            else
            {
                if (Id == Globals.Me.Id)
                {
                    if (Interface.Interface.GameUi == null)
                    {
                        Log.Warn($"'{nameof(Interface.Interface.GameUi)}' is null.");
                    }
                    else
                    {
                        if (Interface.Interface.GameUi.playerBox == null)
                        {
                            Log.Warn($"'{nameof(Interface.Interface.GameUi.playerBox)}' is null.");
                        }
                        else
                        {
                            Interface.Interface.GameUi.playerBox.UpdateStatuses = true;
                        }
                    }
                }
                else if (Id != Guid.Empty && Id == Globals.Me.TargetIndex)
                {
                    Globals.Me.TargetBox.UpdateStatuses = true;
                }
            }
        }

        public void AddAnimations(List<AnimationBase> anims)
        {
            foreach (AnimationBase anim in anims)
            {
                Animations.Add(new Animation(anim, true, false, -1, this));
            }
        }

        public void ClearAnimations(List<Animation> anims)
        {
            if (anims == null)
            {
                anims = Animations;
            }

            if (anims.Count > 0)
            {
                for (int i = 0; i < anims.Count; i++)
                {
                    anims[i].Dispose();
                    Animations.Remove(anims[i]);
                }
            }
        }

        public virtual bool IsDisposed() => mDisposed;

        public virtual void Dispose()
        {
            if (mDisposed)
            {
                return;
            }

            if (RenderList != null)
            {
                RenderList.Remove(this);
            }

            if (entityRender != null)
            {
                entityRender.Destroy();
                entityRender = null;
            }

            //chat bubbles no hacer nada despues de esto que no sea relacionado con el chat bubble
            if (chatBubbleRenderers != null)
            {
                foreach (ChatBubbleRenderer chatBubble in chatBubbleRenderers)
                {
                    chatBubble.Destroy();
                }
                chatBubbleRenderers.Clear();
            }


            ClearAnimations(null);
            mDisposed = true;
        }

        //Returns the amount of time required to traverse 1 tile
        public virtual float GetMovementTime()
        {
            float time = 1000f / (float)(1 + Math.Log(Stat[(int)Stats.Speed]));
            if (Blocking)
            {
                time += time * Options.BlockingSlow;
            }

            return Math.Min(1000f, time);
        }

        //Movement Processing
        public virtual bool Update()
        {
            MapInstance map;
            if (mDisposed)
            {
                LatestMap = null;

                return false;
            }
            else
            {
                map = MapInstance.Get(CurrentMap);
                LatestMap = map;
                if (map == null || !map.InView())
                {
                    Globals.EntitiesToDispose.Add(Id);

                    return false;
                }
            }

            UpdateChatBubbles();

            RenderList = DetermineRenderOrder(RenderList, map);
            if (mLastUpdate == 0)
            {
                mLastUpdate = Globals.System.GetTimeMs();
            }

            float ecTime = Globals.System.GetTimeMs() - mLastUpdate;
            elapsedtime = ecTime;
            if (Dashing != null)
            {
                WalkFrame = Options.Instance.Sprites.NormalSheetDashFrame; //Fix the frame whilst dashing
            }
            else if (mWalkTimer < Globals.System.GetTimeMs())
            {
                if (!IsMoving && DashQueue.Count > 0)
                {
                    Dashing = DashQueue.Dequeue();
                    Dashing.Start(this);
                    OffsetX = 0;
                    OffsetY = 0;
                    DashTimer = Globals.System.GetTimeMs() + Options.MaxDashSpeed;
                }
                else
                {
                    if (IsMoving)
                    {
                        WalkFrame++;
                        if (WalkFrame >= SpriteFrames)
                        {
                            WalkFrame = 0;
                        }
                    }
                    else
                    {
                        if ((WalkFrame > 0 && WalkFrame / SpriteFrames < 0.7f))
                        {
                            WalkFrame = SpriteFrames / 2;
                        }
                        else
                        {
                            WalkFrame = 0;
                        }
                    }

                    mWalkTimer = Globals.System.GetTimeMs() + Options.Instance.Sprites.MovingFrameDuration;
                }
            }

            if (Dashing != null)
            {
                if (Dashing.Update(this))
                {
                    OffsetX = Dashing.GetXOffset();
                    OffsetY = Dashing.GetYOffset();
                }
                else
                {
                    OffsetX = 0;
                    OffsetY = 0;
                }
            }
            else if (IsMoving)
            {
                float deplacementTime = ecTime / GetMovementTime();

                // Dir = facing direction (only 4)
                // delta offset Must be more than 0 for movements. 0 = slowest
                // Direction is related to the sprite animation, I don't know how to set a sprite animation for eache direction
                // so I use DeplacementDir...
                switch (Dir)
                {
                    case 0: // Up
                        OffsetY -= deplacementTime;
                        OffsetX = 0;

                        if (OffsetY < 0)
                        {
                            OffsetY = 0;
                        }
                        break;

                    case 1: // Down
                        OffsetY += deplacementTime;
                        OffsetX = 0;
                        if (OffsetY > 0)
                        {
                            OffsetY = 0;
                        }

                        break;

                    case 2: // Left
                        OffsetX -= deplacementTime;
                        OffsetY = 0;

                        if (OffsetX < 0)
                        {
                            OffsetX = 0;
                        }

                        break;

                    case 3: // Right
                        OffsetX += deplacementTime;
                        OffsetY = 0;
                        if (OffsetX > 0)
                        {
                            OffsetX = 0;
                        }

                        break;
                }

                if (OffsetX == 0 && OffsetY == 0)
                {
                    IsMoving = false;
                }
            }

            //Check to see if we should start or stop equipment animations
            if (Equipment.Length == Options.EquipmentSlots.Count)
            {
                for (int z = 0; z < Options.EquipmentSlots.Count; z++)
                {
                    if (Equipment[z] != Guid.Empty && (this != Globals.Me || MyEquipment[z] < Options.MaxInvItems))
                    {
                        Guid itemId = Guid.Empty;
                        if (this == Globals.Me)
                        {
                            int slot = MyEquipment[z];
                            if (slot > -1)
                            {
                                itemId = Inventory[slot].ItemId;
                            }
                        }
                        else
                        {
                            itemId = Equipment[z];
                        }

                        ItemBase itm = ItemBase.Get(itemId);
                        AnimationBase anim = null;
                        if (itm != null)
                        {
                            anim = itm.EquipmentAnimation;
                        }

                        if (anim != null)
                        {
                            if (EquipmentAnimations[z] != null &&
                                (EquipmentAnimations[z].MyBase != anim || EquipmentAnimations[z].Disposed()))
                            {
                                EquipmentAnimations[z].Dispose();
                                Animations.Remove(EquipmentAnimations[z]);
                                EquipmentAnimations[z] = null;
                            }

                            if (EquipmentAnimations[z] == null)
                            {
                                EquipmentAnimations[z] = new Animation(anim, true, true, -1, this);
                                Animations.Add(EquipmentAnimations[z]);
                            }
                        }
                        else
                        {
                            if (EquipmentAnimations[z] != null)
                            {
                                EquipmentAnimations[z].Dispose();
                                Animations.Remove(EquipmentAnimations[z]);
                                EquipmentAnimations[z] = null;
                            }
                        }
                    }
                    else
                    {
                        if (EquipmentAnimations[z] != null)
                        {
                            EquipmentAnimations[z].Dispose();
                            Animations.Remove(EquipmentAnimations[z]);
                            EquipmentAnimations[z] = null;
                        }
                    }
                }
            }

            if (AnimationTimer < Globals.System.GetTimeMs())
            {
                AnimationTimer = Globals.System.GetTimeMs() + 200;
                AnimationFrame++;
                if (AnimationFrame >= SpriteFrames)
                {
                    AnimationFrame = 0;
                }
            }

            CalculateWorldPos();

            List<Animation> animsToRemove = null;
            foreach (Animation animInstance in Animations)
            {
                animInstance.Update();

                //If disposed mark to be removed and continue onward
                if (animInstance.Disposed())
                {
                    if (animsToRemove == null)
                    {
                        animsToRemove = new List<Animation>();
                    }

                    animsToRemove.Add(animInstance);

                    continue;
                }

                if (IsStealthed())
                {
                    animInstance.Hide();
                }
                else
                {
                    animInstance.Show();
                }

                float y = worldPos.Y;
                if (Texture != null)
                {

                    y -= Texture.SpriteHeight / 2f / Options.TileHeight;
                }
                else
                {
                    y -= .5f;
                }

                animInstance.SetPosition(worldPos.X + .5f, y, X, Y, CurrentMap, animInstance.AutoRotate ? Dir : -1, Z);
            }

            if (animsToRemove != null)
            {
                foreach (Animation anim in animsToRemove)
                {
                    Animations.Remove(anim);
                }
            }

            mLastUpdate = Globals.System.GetTimeMs();

            UpdateSpriteAnimation();

            return true;
        }

        public virtual int CalculateAttackTime()
        {
            //If this is an npc we don't know it's attack time. Luckily the server provided it!
            if (this != Globals.Me && AttackTime > -1)
            {
                return AttackTime;
            }

            //Otherwise return the legacy attack speed calculation
            return (int)(Options.MaxAttackRate +
                          (float)((Options.MinAttackRate - Options.MaxAttackRate) *
                                   (((float)Options.MaxStatValue - Stat[(int)Stats.Speed]) /
                                    (float)Options.MaxStatValue)));
        }

        public virtual bool IsStealthed()
        {
            //If the entity has transformed, apply that sprite instead.
            if (this == Globals.Me)
            {
                return false;
            }

            for (int n = 0; n < Status.Count; n++)
            {
                if (Status[n].Type == StatusTypes.Stealth)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual HashSet<Entity> DetermineRenderOrder(HashSet<Entity> renderList, MapInstance map)
        {
            if (renderList != null)
            {
                renderList.Remove(this);
            }

            if (map == null || Globals.Me == null || Globals.Me.MapInstance == null)
            {
                return null;
            }

            int gridX = Globals.Me.MapInstance.MapGridX;
            int gridY = Globals.Me.MapInstance.MapGridY;
            for (int x = gridX - 1; x <= gridX + 1; x++)
            {
                for (int y = gridY - 1; y <= gridY + 1; y++)
                {
                    if (x >= 0 &&
                        x < Globals.MapGridWidth &&
                        y >= 0 &&
                        y < Globals.MapGridHeight &&
                        Globals.MapGrid[x, y] != Guid.Empty)
                    {
                        if (Globals.MapGrid[x, y] == CurrentMap)
                        {
                            byte priority = mRenderPriority;
                            if (Z != 0)
                            {
                                priority += 3;
                            }

                            HashSet<Entity> renderSet;

                            if (y == gridY - 1)
                            {
                                renderSet = Graphics.RenderingEntities[priority, Options.MapHeight + Y];
                            }
                            else if (y == gridY)
                            {
                                renderSet = Graphics.RenderingEntities[priority, Options.MapHeight * 2 + Y];
                            }
                            else
                            {
                                renderSet = Graphics.RenderingEntities[priority, Options.MapHeight * 3 + Y];
                            }

                            renderSet.Add(this);
                            renderList = renderSet;

                            return renderList;
                        }
                    }
                }
            }

            return renderList;
        }

        //Rendering Functions
        public virtual void Draw()
        {
            if (HideEntity)
            {
                if (entityRender != null)
                {
                    entityRender.HideAll();
                }


                return; //Don't draw if the entity is hidden
            }

            DrawName(null);
            DrawHpBar();
            DrawCastingBar();


            WorldRect.Reset();
            MapInstance map = MapInstance.Get(CurrentMap);
            if (map == null || !Globals.GridMaps.Contains(CurrentMap))
            {
                return;
            }


            string sprite = string.Empty;
            byte alpha = 255;

            //If the entity has transformed, apply that sprite instead.
            for (int n = 0; n < Status.Count; n++)
            {
                if (Status[n].Type == StatusTypes.Transform)
                {
                    sprite = Status[n].Data;
                    TransformedSprite = sprite;
                }

                //If unit is stealthed, don't render unless the entity is the player.
                if (Status[n].Type == StatusTypes.Stealth)
                {
                    if (this != Globals.Me && !(this is Player player && Globals.Me.IsInMyParty(player)))
                    {
                        entityRender.HideAll();
                        return;
                    }
                    else
                    {
                        alpha = 125;
                    }
                }
            }

            bool isTransformed = true;
            //Check if there is no transformed sprite set
            if (string.IsNullOrEmpty(sprite))
            {
                isTransformed = false;
                sprite = MySprite;
                MySprite = sprite;
            }


            GameTexture texture = AnimatedTextures[SpriteAnimation] ?? Texture;

            if (texture == null)
            {
                entityRender.HideAll();
                return;
            }

            int spriteX;
            int spriteY = 1;
            switch (Dir)
            {
                case 0:
                    spriteY = 3;
                    break;
                case 1:
                    spriteY = 0;
                    break;
                case 3:
                    spriteY = 2;
                    break;
            }

            if (SpriteAnimation == SpriteAnimations.Normal)
            {
                if (AttackTimer - CalculateAttackTime() / 2 > Timing.Global.Ticks / TimeSpan.TicksPerMillisecond || Blocking)
                {
                    spriteX = 3;
                }
                else
                {
                    //Restore Original Attacking/Blocking Code
                    spriteX = WalkFrame;
                }
            }
            else
            {
                spriteX = SpriteFrame;
            }

            WorldRect.X = worldPos.X;
            WorldRect.Y = worldPos.Y - texture.SpriteHeight / texture.PixelPerUnits;
            WorldRect.Width = 1;
            WorldRect.Height = texture.SpriteHeight / texture.PixelPerUnits;

            Utils.Draw.Rectangle(WorldRect, Color.White);

            entityRender.SetPosition(worldPos.X, worldPos.Y);
            entityRender.SetHeight(texture.SpriteHeight);

            //Order the layers of paperdolls and sprites
            List<string> paperdollOrder = Options.PaperdollOrder[spriteY];
            for (int z = 0; z < paperdollOrder.Count; z++)
            {
                string paperdoll = paperdollOrder[z];
                //Check for player
                if (PLAYER.Equals(paperdoll, StringComparison.Ordinal))
                {
                    entityRender.Draw(z, texture.GetSprite(Options.AnimatedSprites.Contains(sprite) ? AnimationFrame : spriteX, spriteY), alpha);
                    continue;
                }

                //Don't render the paperdolls if they have transformed.
                if (isTransformed)
                {
                    entityRender.Hide(z);
                    continue;
                }

                int equipSlot = Options.EquipmentSlots.IndexOf(paperdoll);
                if (equipSlot == -1)
                {
                    entityRender.Hide(z);
                    continue;
                }

                if (Equipment.Length == Options.EquipmentSlots.Count
                    && Equipment[equipSlot] != Guid.Empty
                    && this != Globals.Me || MyEquipment[equipSlot] < Options.MaxInvItems)
                {
                    Guid itemId = Guid.Empty;
                    if (this == Globals.Me)
                    {
                        int slot = MyEquipment[equipSlot];
                        if (slot > -1)
                        {
                            itemId = Inventory[slot].ItemId;
                        }
                    }
                    else
                    {
                        itemId = Equipment[equipSlot];
                    }

                    ItemBase item = ItemBase.Get(itemId);
                    if (item != null)
                    {
                        string itemTex = Gender == 0 ? item.MalePaperdoll : item.FemalePaperdoll;
                        if (!string.IsNullOrEmpty(itemTex))
                        {
                            GameTexture paperdollTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Paperdoll, $"{Path.GetFileNameWithoutExtension(itemTex)}_{SpriteAnimation}.png");
                            if (paperdollTex == null)
                            {
                                paperdollTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Paperdoll, itemTex);
                            }

                            if (paperdollTex != null)
                            {
                                entityRender.Draw(z, paperdollTex.GetSprite(spriteX, spriteY), alpha);
                                continue;
                            }
                        }
                    }
                    entityRender.Hide(z);
                }
            }
        }

        protected virtual void CalculateWorldPos()
        {
            Pointf pos = new Pointf(
                LatestMap.GetX() + X + OffsetX,
                LatestMap.GetY() + Y + OffsetY
            );

            worldPos = pos;
        }

        //returns the point on the screen that is the center of the player sprite
        public Pointf GetCenterPos()
        {
            Pointf centerPos = worldPos;
            centerPos.X += Options.TileWidth * .5f / Texture.PixelPerUnits;
            if (Texture != null)
            {
                centerPos.Y -= Texture.SpriteHeight * .5f / Texture.PixelPerUnits;
            }
            else
            {
                centerPos.Y += Options.TileHeight * .5f / Texture.PixelPerUnits;
            }


            return centerPos;
        }

        public void DrawLabels(
            string label,
            int position,
            Color labelColor,
            Color textColor,
            Color borderColor = null,
            Color backgroundColor = null
        )
        {
            if (HideName || string.IsNullOrWhiteSpace(label))
            {
                return;
            }


            if (borderColor == null)
            {
                borderColor = Color.Transparent;
            }

            if (backgroundColor == null)
            {
                backgroundColor = Color.Transparent;
            }

            //If we have a non-transparent label color then use it, otherwise use the players name color
            if (labelColor != null && labelColor.A != 0)
            {
                textColor = labelColor;
            }

            //Check for stealth amoungst status effects.
            for (int n = 0; n < Status.Count; n++)
            {
                //If unit is stealthed, don't render unless the entity is the player.
                if (Status[n].Type == StatusTypes.Stealth)
                {
                    if (this != Globals.Me && !(this is Player player && Globals.Me.IsInMyParty(player)))
                    {
                        return;
                    }
                }
            }

            MapInstance map = MapInstance;
            if (map == null)
            {
                return;
            }

            //int x = (int)Math.Ceiling(GetWorldPos().X);
            //float y = position == 0 ? GetLabelLocation(LabelType.Header) : GetLabelLocation(LabelType.Footer);

            //Graphics.Renderer.DrawString(
            //    label, null, x - (int)Math.Ceiling(textSize.X / 2f), (int)y, 1,
            //    Color.FromArgb(textColor.ToArgb()), true, null, Color.FromArgb(borderColor.ToArgb())
            //);
        }

        public virtual void DrawName(Color textColor, Color borderColor = null, Color backgroundColor = null)
        {
            if (HideName || string.IsNullOrWhiteSpace(Name))
            {
                entityRender.HideName();
                return;
            }

            //if (borderColor == null) {
            //	borderColor = Color.Transparent;
            //}

            //if (backgroundColor == null) {
            //	backgroundColor = Color.Transparent;
            //}

            //Check for npc colors
            if (textColor == null)
            {
                LabelColor? color;
                switch (Type)
                {
                    case -1: //When entity has a target (showing aggression)
                        color = CustomColors.Names.Npcs["Aggressive"];

                        break;
                    case 0: //Attack when attacked
                        color = CustomColors.Names.Npcs["AttackWhenAttacked"];

                        break;
                    case 1: //Attack on sight
                        color = CustomColors.Names.Npcs["AttackOnSight"];

                        break;
                    case 3: //Guard
                        color = CustomColors.Names.Npcs["Guard"];

                        break;
                    case 2: //Neutral
                    default:
                        color = CustomColors.Names.Npcs["Neutral"];

                        break;
                }

                if (color != null)
                {
                    textColor = color?.Name;
                    //backgroundColor = color?.Background;
                    //borderColor = color?.Outline;
                }
            }

            ////Check for stealth amoungst status effects.
            //for (int n = 0; n < Status.Count; n++)
            //{
            //    //If unit is stealthed, don't render unless the entity is the player.
            //    if (Status[n].Type == StatusTypes.Stealth)
            //    {
            //        if (this != Globals.Me && !(this is Player player && Globals.Me.IsInMyParty(player)))
            //        {
            //            entityRender.HideName();
            //            return;
            //        }
            //    }
            //}

            MapInstance map = MapInstance;
            if (map == null)
            {
                return;
            }

            string name = Name;
            if ((this is Player && Options.Player.ShowLevelByName) || (!(this is Player) && Options.Npc.ShowLevelByName))
            {
                name = Strings.GameWindow.EntityNameAndLevel.ToString(Name, Level);
            }

            entityRender.DrawName(name, textColor);
        }

        //public float GetLabelLocation(LabelType type)
        //{
        //    float y = 100;
        //    switch (type)
        //    {
        //        case LabelType.Header:
        //            if (string.IsNullOrWhiteSpace(HeaderLabel.Text))
        //            {
        //                return GetLabelLocation(LabelType.Name);
        //            }

        //            y = GetLabelLocation(LabelType.Name);

        //            break;
        //        case LabelType.Footer:
        //            if (string.IsNullOrWhiteSpace(FooterLabel.Text))
        //            {
        //                break;
        //            }

        //            break;
        //        case LabelType.Name:
        //        case LabelType.Guild:
        //            if (Texture != null)
        //            {
        //                y = Texture.SpriteHeight;
        //            }
        //            y += 30f;
        //            break;
        //        case LabelType.ChatBubble:
        //            y = GetLabelLocation(LabelType.Header) - 4;

        //            break;
        //    }

        //    return y;
        //}

        public int GetShieldSize()
        {
            int shieldSize = 0;
            foreach (Status status in Status)
            {
                if (status.Type == StatusTypes.Shield)
                {
                    shieldSize += status.Shield[(int)Vitals.Health];
                }
            }
            return shieldSize;
        }

        public void DrawHpBar()
        {
            if (HideName && HideEntity)
            {
                entityRender.HideAll();
                return;
            }

            if (Vital[(int)Vitals.Health] <= 0)
            {
                entityRender.HideHp();
                return;
            }

            int maxVital = MaxVital[(int)Vitals.Health];
            int shieldSize = 0;

            //Check for shields
            foreach (Status status in Status)
            {
                if (status.Type == StatusTypes.Shield)
                {
                    shieldSize += status.Shield[(int)Vitals.Health];
                    maxVital += status.Shield[(int)Vitals.Health];
                }
            }

            if (shieldSize + Vital[(int)Vitals.Health] > maxVital)
            {
                maxVital = shieldSize + Vital[(int)Vitals.Health];
            }

            if (Vital[(int)Vitals.Health] == MaxVital[(int)Vitals.Health] && shieldSize <= 0)
            {
                entityRender.HideHp();
                return;
            }

            //Check for stealth amoungst status effects.
            for (int n = 0; n < Status.Count; n++)
            {
                //If unit is stealthed, don't render unless the entity is the player.
                if (Status[n].Type == StatusTypes.Stealth)
                {
                    if (this != Globals.Me && !(this is Player player && Globals.Me.IsInMyParty(player)))
                    {
                        entityRender.HideHp();
                        return;
                    }
                }
            }

            MapInstance map = MapInstance.Get(CurrentMap);
            if (map == null)
            {
                entityRender.HideHp();
                return;
            }

            float hpfillRatio = (float)Vital[(int)Vitals.Health] / maxVital;
            hpfillRatio = Math.Min(1, Math.Max(0, hpfillRatio));

            float shieldfillRatio = (float)shieldSize / maxVital;
            shieldfillRatio = Math.Min(1, Math.Max(0, shieldfillRatio));

            entityRender.ChangeHp(hpfillRatio);

            //int y = (int)Math.Ceiling(GetWorldPos().Y);

            //GameTexture hpBackground = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Misc, "hpbackground.png");
            //GameTexture hpForeground = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Misc, "hpbar.png");
            //GameTexture shieldForeground = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Misc, "shieldbar.png");

            //if (hpBackground != null) {
            //	Graphics.DrawGameTexture(
            //		hpBackground, new FloatRect(0, 0, hpBackground.GetWidth(), hpBackground.GetHeight()),
            //		new FloatRect(x - width / 2, y - 1, width, 6), Color.White
            //	);
            //}

            //if (hpForeground != null) {
            //	Graphics.DrawGameTexture(
            //		hpForeground, new FloatRect(0, 0, hpfillWidth, hpForeground.GetHeight()),
            //		new FloatRect(x - width / 2, y - 1, hpfillWidth, 6), Color.White
            //	);
            //}

            //if (shieldSize > 0 && shieldForeground != null) //Check for a shield to render
            //{
            //	Graphics.DrawGameTexture(
            //		shieldForeground,
            //		new FloatRect((float)(width - shieldfillWidth), 0, shieldfillWidth, shieldForeground.GetHeight()),
            //		new FloatRect(x - width / 2 + hpfillWidth, y - 1, shieldfillWidth, 6), Color.White
            //	);
            //}
        }

        public void DrawCastingBar()
        {
            if (CastTime < Globals.System.GetTimeMs())
            {
                entityRender.HideCastBar();
                return;
            }

            if (MapInstance.Get(CurrentMap) == null)
            {
                entityRender.HideCastBar();
                return;
            }

            SpellBase castSpell = SpellBase.Get(SpellCast);
            if (castSpell != null)
            {
                float fillratio = (castSpell.CastDuration - (CastTime - Globals.System.GetTimeMs())) / (float)castSpell.CastDuration;
                entityRender.ChangeCast(fillratio);
            }
        }

        public void DrawTarget(int priority)
        {
            if (this is Projectile)
            {
                return;
            }

            MapInstance map = MapInstance.Get(CurrentMap);
            if (map == null)
            {
                return;
            }

            GameTexture targetTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Misc, "target.png");
            if (targetTex != null)
            {
                entityRender.DrawTarget(targetTex.GetSprite(priority, 0));
            }
        }

        public virtual bool CanBeAttacked()
        {
            return true;
        }

        //Chatting
        public void AddChatBubble(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (chatBubbles is null)
            {
                chatBubbles = new ConcurrentQueue<string>();
                chatBubbleRenderers = new List<ChatBubbleRenderer>();
            }

            chatBubbles.Enqueue(text);
        }

        public void UpdateChatBubbles()
        {
            if (chatBubbles is null)
            {
                return;
            }

            for (int i = chatBubbleRenderers.Count - 1; i >= 0; i--)
            {
                ChatBubbleRenderer chatBubble = chatBubbleRenderers[i];
                if (chatBubble.TimeOut())
                {
                    chatBubble.Destroy();
                    chatBubbleRenderers.RemoveAt(i);
                }
            }

            while (chatBubbles.TryDequeue(out string text))
            {
                ChatBubbleRenderer chatBubble = UnityFactory.GetChatBubble(Name, entityRender.ChatBubbleParent);
                chatBubble.Set(text);
                chatBubbleRenderers.Add(chatBubble);
            }
        }

        //Statuses
        public bool StatusActive(Guid guid)
        {
            foreach (Status status in Status)
            {
                if (status.SpellId == guid && status.IsActive())
                {
                    return true;
                }
            }

            return false;
        }

        public Status GetStatus(Guid guid)
        {
            foreach (Status status in Status)
            {
                if (status.SpellId == guid && status.IsActive())
                {
                    return status;
                }
            }

            return null;
        }

        public void SortStatuses()
        {
            //Sort Status effects by remaining time
            Status = Status.OrderByDescending(x => x.RemainingMs()).ToList();
        }

        public void UpdateSpriteAnimation()
        {
            //Exit if textures haven't been loaded yet
            if (AnimatedTextures.Count == 0)
            {
                return;
            }

            SpriteAnimation = AnimatedTextures[SpriteAnimations.Idle] != null && LastActionTime + Options.Instance.Sprites.TimeBeforeIdle < Globals.System.GetTimeMs() ? SpriteAnimations.Idle : SpriteAnimations.Normal;
            if (IsMoving)
            {
                SpriteAnimation = SpriteAnimations.Normal;
                LastActionTime = Globals.System.GetTimeMs();
            }
            else if (AttackTimer > Timing.Global.Ticks / TimeSpan.TicksPerMillisecond) //Attacking
            {
                long timeIn = CalculateAttackTime() - (AttackTimer - Timing.Global.Ticks / TimeSpan.TicksPerMillisecond);
                LastActionTime = Globals.System.GetTimeMs();

                if (AnimatedTextures[SpriteAnimations.Attack] != null)
                {
                    SpriteAnimation = SpriteAnimations.Attack;
                }

                if (Options.WeaponIndex > -1 && Options.WeaponIndex < Equipment.Length)
                {
                    if (Equipment[Options.WeaponIndex] != Guid.Empty && this != Globals.Me ||
                        MyEquipment[Options.WeaponIndex] < Options.MaxInvItems)
                    {
                        Guid itemId = Guid.Empty;
                        if (this == Globals.Me)
                        {
                            int slot = MyEquipment[Options.WeaponIndex];
                            if (slot > -1)
                            {
                                itemId = Inventory[slot].ItemId;
                            }
                        }
                        else
                        {
                            itemId = Equipment[Options.WeaponIndex];
                        }

                        ItemBase item = ItemBase.Get(itemId);
                        if (item != null)
                        {
                            if (AnimatedTextures[SpriteAnimations.Weapon] != null)
                            {
                                SpriteAnimation = SpriteAnimations.Weapon;
                            }

                            if (AnimatedTextures[SpriteAnimations.Shoot] != null && item.ProjectileId != Guid.Empty)
                            {
                                SpriteAnimation = SpriteAnimations.Shoot;
                            }
                        }
                    }
                }

                if (SpriteAnimation != SpriteAnimations.Normal && SpriteAnimation != SpriteAnimations.Idle)
                {
                    SpriteFrame = (int)Math.Floor(timeIn / (CalculateAttackTime() / (float)SpriteFrames));
                }
            }
            else if (CastTime > Globals.System.GetTimeMs())
            {
                SpellBase spell = SpellBase.Get(SpellCast);
                if (spell != null)
                {
                    int duration = spell.CastDuration;
                    long timeIn = duration - (CastTime - Globals.System.GetTimeMs());

                    if (AnimatedTextures[SpriteAnimations.Cast] != null)
                    {
                        SpriteAnimation = SpriteAnimations.Cast;
                    }

                    if (spell.SpellType == SpellTypes.CombatSpell &&
                        spell.Combat.TargetType == SpellTargetTypes.Projectile && AnimatedTextures[SpriteAnimations.Shoot] != null)
                    {
                        SpriteAnimation = SpriteAnimations.Shoot;
                    }
                    SpriteFrame = (int)Math.Floor((timeIn / (duration / (float)SpriteFrames)));
                }
                LastActionTime = Globals.System.GetTimeMs();
            }

            if (SpriteAnimation == SpriteAnimations.Normal)
            {
                ResetSpriteFrame();
            }
            else if (SpriteAnimation == SpriteAnimations.Idle)
            {
                if (SpriteFrameTimer + Options.Instance.Sprites.IdleFrameDuration < Globals.System.GetTimeMs())
                {
                    SpriteFrame++;
                    if (SpriteFrame >= SpriteFrames)
                    {
                        SpriteFrame = 0;
                    }
                    SpriteFrameTimer = Globals.System.GetTimeMs();
                }
            }
        }

        public void ResetSpriteFrame()
        {
            SpriteFrame = 0;
            SpriteFrameTimer = Globals.System.GetTimeMs();
        }

        public void LoadAnimationTextures(string tex)
        {
            string file = Path.GetFileNameWithoutExtension(tex);

            AnimatedTextures.Clear();
            foreach (object anim in Enum.GetValues(typeof(SpriteAnimations)))
            {
                AnimatedTextures.Add((SpriteAnimations)anim, Globals.ContentManager.GetTexture(GameContentManager.TextureType.Entity, $"{file}_{anim}.png"));
            }
        }

        //Movement
        /// <summary>
        ///     Returns -6 if the tile is blocked by a global (non-event) entity
        ///     Returns -5 if the tile is completely out of bounds.
        ///     Returns -4 if a tile is blocked because of a local event.
        ///     Returns -3 if a tile is blocked because of a Z dimension tile
        ///     Returns -2 if a tile does not exist or is blocked by a map attribute.
        ///     Returns -1 is a tile is passable.
        ///     Returns any value zero or greater matching the entity index that is in the way.
        /// </summary>
        /// <returns></returns>
        public int IsTileBlocked(
            int x,
            int y,
            int z,
            Guid mapId,
            ref Entity blockedBy,
            bool ignoreAliveResources = true,
            bool ignoreDeadResources = true,
            bool ignoreNpcAvoids = true
        )
        {
            var mapInstance = MapInstance.Get(mapId);
            if (mapInstance == null)
            {
                return -2;
            }

            var gridX = mapInstance.MapGridX;
            var gridY = mapInstance.MapGridY;
            try
            {
                var tmpX = x;
                var tmpY = y;
                var tmpMapId = Guid.Empty;
                if (x < 0)
                {
                    gridX--;
                    tmpX = Options.MapWidth - x * -1;
                }

                if (y < 0)
                {
                    gridY--;
                    tmpY = Options.MapHeight - y * -1;
                }

                if (x > Options.MapWidth - 1)
                {
                    gridX++;
                    tmpX = x - Options.MapWidth;
                }

                if (y > Options.MapHeight - 1)
                {
                    gridY++;
                    tmpY = y - Options.MapHeight;
                }

                if (gridX < 0 || gridY < 0 || gridX >= Globals.MapGridWidth || gridY >= Globals.MapGridHeight)
                {
                    return -2;
                }

                tmpMapId = Globals.MapGrid[gridX, gridY];

                foreach (var en in Globals.Entities)
                {
                    if (en.Value == null)
                    {
                        continue;
                    }

                    if (en.Value == Globals.Me)
                    {
                        continue;
                    }
                    else
                    {
                        if (en.Value.CurrentMap == tmpMapId &&
                            en.Value.X == tmpX &&
                            en.Value.Y == tmpY &&
                            en.Value.Z == Z)
                        {
                            if (en.Value.GetType() != typeof(Projectile))
                            {
                                if (en.Value.GetType() == typeof(Resource))
                                {
                                    var resourceBase = ((Resource)en.Value).GetResourceBase();
                                    if (resourceBase != null)
                                    {
                                        if (!ignoreAliveResources && !((Resource)en.Value).IsDead)
                                        {
                                            blockedBy = en.Value;

                                            return -6;
                                        }

                                        if (!ignoreDeadResources && ((Resource)en.Value).IsDead)
                                        {
                                            blockedBy = en.Value;

                                            return -6;
                                        }

                                        if (resourceBase.WalkableAfter && ((Resource)en.Value).IsDead ||
                                            resourceBase.WalkableBefore && !((Resource)en.Value).IsDead)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                else if (en.Value.GetType() == typeof(Player))
                                {
                                    //Return the entity key as this should block the player.  Only exception is if the MapZone this entity is on is passable.
                                    var entityMap = MapInstance.Get(en.Value.CurrentMap);
                                    if (Options.Instance.Passability.Passable[(int)entityMap.ZoneType])
                                    {
                                        continue;
                                    }
                                }

                                blockedBy = en.Value;

                                return -6;
                            }
                        }
                    }
                }

                if (MapInstance.Get(tmpMapId) != null)
                {
                    foreach (var en in MapInstance.Get(tmpMapId).LocalEntities)
                    {
                        if (en.Value == null)
                        {
                            continue;
                        }

                        if (en.Value.CurrentMap == tmpMapId &&
                            en.Value.X == tmpX &&
                            en.Value.Y == tmpY &&
                            en.Value.Z == Z &&
                            !en.Value.Passable)
                        {
                            blockedBy = en.Value;

                            return -4;
                        }
                    }

                    foreach (var en in MapInstance.Get(tmpMapId).Critters)
                    {
                        if (en.Value == null)
                        {
                            continue;
                        }

                        if (en.Value.CurrentMap == tmpMapId &&
                            en.Value.X == tmpX &&
                            en.Value.Y == tmpY &&
                            en.Value.Z == Z &&
                            !en.Value.Passable)
                        {
                            blockedBy = en.Value;

                            return -4;
                        }
                    }
                }

                var gameMap = MapInstance.Get(Globals.MapGrid[gridX, gridY]);
                if (gameMap != null)
                {
                    if (gameMap.Attributes[tmpX, tmpY] != null)
                    {
                        if (gameMap.Attributes[tmpX, tmpY].Type == MapAttributes.Blocked)
                        {
                            return -2;
                        }
                        else if (gameMap.Attributes[tmpX, tmpY].Type == MapAttributes.ZDimension)
                        {
                            if (((MapZDimensionAttribute)gameMap.Attributes[tmpX, tmpY]).BlockedLevel - 1 == z)
                            {
                                return -3;
                            }
                        }
                        else if (gameMap.Attributes[tmpX, tmpY].Type == MapAttributes.NpcAvoid)
                        {
                            if (!ignoreNpcAvoids)
                            {
                                return -2;
                            }
                        }
                    }
                }
                else
                {
                    return -5;
                }

                return -1;
            }
            catch
            {
                return -2;
            }
        }

        ~Entity()
        {
            Dispose();
        }

        internal void HideTarget()
        {
            entityRender.HideTarget();
        }
    }

}
