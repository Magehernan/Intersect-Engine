using System;
using System.Linq;
using System.Collections.Generic;

using Intersect.Client.Core;
using Intersect.Client.Core.Controls;
using Intersect.Client.Entities.Events;
using Intersect.Client.Entities.Projectiles;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Interface.Game;
using Intersect.Client.Interface.Game.EntityPanel;
using Intersect.Client.Localization;
using Intersect.Client.Maps;
using Intersect.Client.Networking;
using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.GameObjects.Maps;
using Intersect.Network.Packets.Server;

using Newtonsoft.Json;
using Intersect.Client.UnityGame.Graphics;
using Intersect.Utilities;
using Intersect.Client.Utils;
using Intersect.Client.Interface.Shared;
using Intersect.Config.Guilds;

namespace Intersect.Client.Entities
{

    public partial class Player : Entity
    {

        public delegate void InventoryUpdated();

        public Guid Class;

        public long Experience = 0;

        public long ExperienceToNextLevel = 0;

        public List<FriendInstance> Friends = new List<FriendInstance>();

        public HotbarInstance[] Hotbar = new HotbarInstance[Options.MaxHotbar];

        public InventoryUpdated InventoryUpdatedDelegate;

        public Dictionary<Guid, long> ItemCooldowns = new Dictionary<Guid, long>();

        private Entity mLastBumpedEvent = null;

        private List<PartyMember> mParty;

        public Dictionary<Guid, QuestProgress> QuestProgress = new Dictionary<Guid, QuestProgress>();
        
        public Guid[] HiddenQuests = new Guid[0];

        public Dictionary<Guid, long> SpellCooldowns = new Dictionary<Guid, long>();

        public int StatPoints = 0;

        public EntityBox TargetBox;

        public Guid TargetIndex;

        public int TargetType;

        public long CombatTimer { get; set; } = 0;

        // Target data
        private long mlastTargetScanTime = 0;

        Guid mlastTargetScanMap = Guid.Empty;

        Point mlastTargetScanLocation = new Point(-1, -1);

        Dictionary<Entity, TargetInfo> mlastTargetList = new Dictionary<Entity, TargetInfo>(); // Entity, Last Time Selected

        Entity mLastEntitySelected = null;

        private Dictionary<int, long> mLastHotbarUseTime = new Dictionary<int, long>();
        private int mHotbarUseDelay = 150;

        /// <summary>
        /// Name of our guild if we are in one.
        /// </summary>
        public string Guild;

        /// <summary>
        /// Index of our rank where 0 is the leader
        /// </summary>
        public int Rank;

        /// <summary>
        /// Returns whether or not we are in a guild by checking to see if we are assigned a guild name
        /// </summary>
        public bool InGuild => !string.IsNullOrWhiteSpace(Guild);

        /// <summary>
        /// Obtains our rank and permissions from the game config
        /// </summary>
        public GuildRank GuildRank => InGuild ? Options.Instance.Guild.Ranks[Math.Max(0, Math.Min(this.Rank, Options.Instance.Guild.Ranks.Length - 1))] : null;

        /// <summary>
        /// Contains a record of all members of this player's guild.
        /// </summary>
        public GuildMember[] GuildMembers = new GuildMember[0];

        public Player(Guid id, PlayerEntityPacket packet) : base(id, packet)
        {
            TargetBox = Interface.Interface.GameUi.targetBox;
            for (int i = 0; i < Options.MaxHotbar; i++)
            {
                Hotbar[i] = new HotbarInstance();
            }

            mRenderPriority = 2;
        }

        public List<PartyMember> Party
        {
            get
            {
                if (mParty == null)
                {
                    mParty = new List<PartyMember>();
                }

                return mParty;
            }
        }

        public override Guid CurrentMap
        {
            get => base.CurrentMap;
            set
            {
                if (value != base.CurrentMap)
                {
                    MapInstance oldMap = MapInstance.Get(base.CurrentMap);
                    MapInstance newMap = MapInstance.Get(value);
                    base.CurrentMap = value;
                    if (Globals.Me == this)
                    {
                        if (MapInstance.Get(Globals.Me.CurrentMap) != null)
                        {
                            Audio.PlayMusic(MapInstance.Get(Globals.Me.CurrentMap).Music, 3, 3, true);
                        }

                        if (newMap != null && oldMap != null)
                        {
                            newMap.CompareEffects(oldMap);
                        }
                    }
                }
            }
        }

        public bool IsInParty()
        {
            return Party.Count > 0;
        }

        public bool IsInMyParty(Player player) => IsInMyParty(player.Id);

        public bool IsInMyParty(Guid id) => Party.Any(member => member.Id == id);

        public bool IsBusy()
        {
            return !(Globals.EventHolds.Count == 0
                && !Globals.MoveRouteActive
                && !Globals.InShop
                && !Globals.InBank
                && !Globals.InCraft
                && !Globals.InTrade
                && !Interface.Interface.HasInputFocus());
        }

        public override bool Update()
        {
            if (Globals.Me == this)
            {
                HandleInput();
            }


            if (!IsBusy())
            {
                if (this == Globals.Me && IsMoving == false)
                {
                    ProcessDirectionalInput();
                }

                if (Controls.KeyDown(Control.AttackInteract))
                {
                    if (!Globals.Me.TryAttack())
                    {
                        if (Globals.Me.AttackTimer < Timing.Global.Ticks / TimeSpan.TicksPerMillisecond)
                        {
                            Globals.Me.AttackTimer = Timing.Global.Ticks / TimeSpan.TicksPerMillisecond + Globals.Me.CalculateAttackTime();
                        }
                    }
                }
            }

            TargetBox.Draw();

            return base.Update();
        }

        //Loading
        public override void Load(EntityPacket packet)
        {
            base.Load(packet);
            PlayerEntityPacket playerPacket = (PlayerEntityPacket)packet;
            Gender = playerPacket.Gender;
            Class = playerPacket.ClassId;
            Guild = playerPacket.Guild;
            Type = playerPacket.AccessLevel;
            CombatTimer = playerPacket.CombatTimeRemaining + Globals.System.GetTimeMs();

            if (playerPacket.Equipment != null)
            {
                if (this == Globals.Me && playerPacket.Equipment.InventorySlots != null)
                {
                    MyEquipment = playerPacket.Equipment.InventorySlots;
                }
                else if (playerPacket.Equipment.ItemIds != null)
                {
                    Equipment = playerPacket.Equipment.ItemIds;
                }
            }

            if (this == Globals.Me && string.IsNullOrEmpty(Guild) && Interface.Interface.GameUi != null)
            {
                Interface.Interface.GameUi.HideGuildWindow();
            }
        }

        public override EntityTypes GetEntityType()
        {
            return EntityTypes.Player;
        }

        //Item Processing
        private static void SwapItems(Items.Item[] items, int item1, int item2, Action<int, int> packetSender)
        {
            if (item1 == item2)
            {
                //si es el mismo me voy
                return;
            }

            Items.Item temp = items[item2];
            items[item2] = items[item1];
            items[item1] = temp;
            packetSender?.Invoke(item1, item2);
        }

        internal void SwapBagItems(int item1, int item2)
        {
            SwapItems(Globals.Bag, item1, item2, PacketSender.SendMoveBagItems);
        }


        public void SwapBankItems(int item1, int item2)
        {
            SwapItems(Globals.Bank, item1, item2, PacketSender.SendMoveBankItems);
        }

        public void SwapInventoryItems(int item1, int item2)
        {
            SwapItems(Inventory, item1, item2, PacketSender.SendSwapInvItems);
        }

        public void TryDropItem(int index)
        {
            if (ItemBase.Get(Inventory[index].ItemId) != null)
            {
                if (Inventory[index].Quantity > 1)
                {
                    Interface.Interface.InputBox.Show(
                        Strings.Inventory.dropitem,
                        Strings.Inventory.dropitemprompt.ToString(ItemBase.Get(Inventory[index].ItemId).Name), true,
                        InputBox.InputType.NumericInput, DropItemInputBoxOkay, null, index, Inventory[index].Quantity
                    );
                }
                else
                {
                    Interface.Interface.InputBox.Show(
                        Strings.Inventory.dropitem,
                        Strings.Inventory.dropprompt.ToString(ItemBase.Get(Inventory[index].ItemId).Name), true,
                        InputBox.InputType.YesNo, DropInputBoxOkay, null, index
                    );
                }
            }
        }

        private void DropItemInputBoxOkay(object sender, EventArgs e)
        {
            int value = (int)((InputBox)sender).Value;
            if (value > 0)
            {
                PacketSender.SendDropItem((int)((InputBox)sender).UserData, value);
            }
        }

        private void DropInputBoxOkay(object sender, EventArgs e)
        {
            PacketSender.SendDropItem((int)((InputBox)sender).UserData, 1);
        }

        public int FindItem(Guid itemId, int itemVal = 1)
        {
            for (int i = 0; i < Options.MaxInvItems; i++)
            {
                if (Inventory[i].ItemId == itemId && Inventory[i].Quantity >= itemVal)
                {
                    return i;
                }
            }

            return -1;
        }

        public void TryUseItem(int index)
        {
            if (Globals.GameShop == null && Globals.InBank == false && Globals.InTrade == false && !ItemOnCd(index)
                && index >= 0 && index < Globals.Me.Inventory.Length && Globals.Me.Inventory[index]?.Quantity > 0)
            {
                PacketSender.SendUseItem(index, TargetIndex);
            }
        }

        public long GetItemCooldown(Guid id)
        {
            if (ItemCooldowns.ContainsKey(id))
            {
                return ItemCooldowns[id];
            }

            return 0;
        }

        public int FindHotbarItem(HotbarInstance hotbarInstance)
        {
            int bestMatch = -1;

            if (hotbarInstance.ItemOrSpellId != Guid.Empty)
            {
                for (int i = 0; i < Inventory.Length; i++)
                {
                    Items.Item itm = Inventory[i];
                    if (itm != null && itm.ItemId == hotbarInstance.ItemOrSpellId)
                    {
                        bestMatch = i;
                        ItemBase itemBase = ItemBase.Get(itm.ItemId);
                        if (itemBase != null)
                        {
                            if (itemBase.ItemType == ItemTypes.Bag)
                            {
                                if (hotbarInstance.BagId == itm.BagId)
                                {
                                    break;
                                }
                            }
                            else if (itemBase.ItemType == ItemTypes.Equipment)
                            {
                                if (hotbarInstance.PreferredStatBuffs != null)
                                {
                                    bool statMatch = true;
                                    for (int s = 0; s < hotbarInstance.PreferredStatBuffs.Length; s++)
                                    {
                                        if (itm.StatBuffs[s] != hotbarInstance.PreferredStatBuffs[s])
                                        {
                                            statMatch = false;
                                        }
                                    }

                                    if (statMatch)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return bestMatch;
        }

        public bool IsEquipped(int slot)
        {
            for (int i = 0; i < Options.EquipmentSlots.Count; i++)
            {
                if (MyEquipment[i] == slot)
                {
                    return true;
                }
            }

            return false;
        }

        public bool ItemOnCd(int slot)
        {
            if (Inventory[slot] != null)
            {
                Items.Item itm = Inventory[slot];
                if (itm.ItemId != Guid.Empty)
                {
                    if (ItemCooldowns.ContainsKey(itm.ItemId) && ItemCooldowns[itm.ItemId] > Globals.System.GetTimeMs())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public long ItemCdRemainder(int slot)
        {
            if (Inventory[slot] != null)
            {
                Items.Item itm = Inventory[slot];
                if (itm.ItemId != Guid.Empty)
                {
                    if (ItemCooldowns.ContainsKey(itm.ItemId) && ItemCooldowns[itm.ItemId] > Globals.System.GetTimeMs())
                    {
                        return ItemCooldowns[itm.ItemId] - Globals.System.GetTimeMs();
                    }
                }
            }

            return 0;
        }

        public decimal GetCooldownReduction()
        {
            int cooldown = 0;

            for (int i = 0; i < Options.EquipmentSlots.Count; i++)
            {
                if (MyEquipment[i] > -1)
                {
                    if (Inventory[MyEquipment[i]].ItemId != Guid.Empty)
                    {
                        ItemBase item = ItemBase.Get(Inventory[MyEquipment[i]].ItemId);
                        if (item != null)
                        {
                            //Check for cooldown reduction
                            if (item.Effect.Type == EffectType.CooldownReduction)
                            {
                                cooldown += item.Effect.Percentage;
                            }
                        }
                    }
                }
            }

            return cooldown;
        }

        public void TrySellItem(int index)
        {
            if (ItemBase.Get(Inventory[index].ItemId) != null)
            {
                int foundItem = -1;
                for (int i = 0; i < Globals.GameShop.BuyingItems.Count; i++)
                {
                    if (Globals.GameShop.BuyingItems[i].ItemId == Inventory[index].ItemId)
                    {
                        foundItem = i;

                        break;
                    }
                }

                if (foundItem > -1 && Globals.GameShop.BuyingWhitelist ||
                    foundItem == -1 && !Globals.GameShop.BuyingWhitelist)
                {
                    if (Inventory[index].Quantity > 1)
                    {
                        Interface.Interface.InputBox.Show(
                            Strings.Shop.sellitem,
                            Strings.Shop.sellitemprompt.ToString(ItemBase.Get(Inventory[index].ItemId).Name), true,
                            InputBox.InputType.NumericInput, SellItemInputBoxOkay, null, index, Inventory[index].Quantity
                        );
                    }
                    else
                    {
                        Interface.Interface.InputBox.Show(
                            Strings.Shop.sellitem,
                            Strings.Shop.sellprompt.ToString(ItemBase.Get(Inventory[index].ItemId).Name), true,
                            InputBox.InputType.YesNo, SellInputBoxOkay, null, index
                        );
                    }
                }
                else
                {
                    Interface.Interface.InputBox.Show(
                        Strings.Shop.sellitem, Strings.Shop.cannotsell, true, InputBox.InputType.OkayOnly, null, null,
                        -1
                    );
                }
            }
        }

        private void SellItemInputBoxOkay(object sender, EventArgs e)
        {
            int value = (int)((InputBox)sender).Value;
            if (value > 0)
            {
                PacketSender.SendSellItem((int)((InputBox)sender).UserData, value);
            }
        }

        private void SellInputBoxOkay(object sender, EventArgs e)
        {
            PacketSender.SendSellItem((int)((InputBox)sender).UserData, 1);
        }

        //bank
        public void TryDepositItem(int index)
        {
            if (ItemBase.Get(Inventory[index].ItemId) != null)
            {
                if (Inventory[index].Quantity > 1)
                {
                    Interface.Interface.InputBox.Show(
                        Strings.Bank.deposititem,
                        Strings.Bank.deposititemprompt.ToString(ItemBase.Get(Inventory[index].ItemId).Name), true,
                        InputBox.InputType.NumericInput, DepositItemInputBoxOkay, null, index, Inventory[index].Quantity
                    );
                }
                else
                {
                    PacketSender.SendDepositItem(index, 1);
                }
            }
        }

        private void DepositItemInputBoxOkay(object sender, EventArgs e)
        {
            int value = (int)((InputBox)sender).Value;
            if (value > 0)
            {
                PacketSender.SendDepositItem((int)((InputBox)sender).UserData, value);
            }
        }

        public void TryWithdrawItem(int index)
        {
            if (ItemBase.Get(Globals.Bank[index].ItemId) != null)
            {
                if (Globals.Bank[index].Quantity > 1)
                {
                    Interface.Interface.InputBox.Show(
                        Strings.Bank.withdrawitem,
                        Strings.Bank.withdrawitemprompt.ToString(ItemBase.Get(Globals.Bank[index].ItemId).Name), true,
                        InputBox.InputType.NumericInput, WithdrawItemInputBoxOkay, null, index
                    );
                }
                else
                {
                    PacketSender.SendWithdrawItem(index, 1);
                }
            }
        }

        private void WithdrawItemInputBoxOkay(object sender, EventArgs e)
        {
            int value = (int)((InputBox)sender).Value;
            if (value > 0)
            {
                PacketSender.SendWithdrawItem((int)((InputBox)sender).UserData, value);
            }
        }

        //Bag
        public void TryStoreBagItem(int invSlot, int bagSlot)
        {
            if (ItemBase.Get(Inventory[invSlot].ItemId) != null)
            {
                if (Inventory[invSlot].Quantity > 1)
                {
                    int[] userData = new int[2] { invSlot, bagSlot };

                    Interface.Interface.InputBox.Show(
                        Strings.Bags.storeitem,
                        Strings.Bags.storeitemprompt.ToString(ItemBase.Get(Inventory[invSlot].ItemId).Name), true,
                        InputBox.InputType.NumericInput, StoreBagItemInputBoxOkay, null, userData, Inventory[invSlot].Quantity
                    );
                }
                else
                {
                    PacketSender.SendStoreBagItem(invSlot, 1, bagSlot);
                }
            }
        }

        private void StoreBagItemInputBoxOkay(object sender, EventArgs e)
        {
            int value = (int)((InputBox)sender).Value;
            if (value > 0)
            {
                int[] userData = (int[])((InputBox)sender).UserData;
                PacketSender.SendStoreBagItem(userData[0], value, userData[1]);
            }
        }

        public void TryRetreiveBagItem(int bagSlot, int invSlot)
        {
            if (Globals.Bag[bagSlot] != null && ItemBase.Get(Globals.Bag[bagSlot].ItemId) != null)
            {
                if (Globals.Bag[bagSlot].Quantity > 1)
                {
                    int[] userData = new int[2] { bagSlot, invSlot };

                    Interface.Interface.InputBox.Show(
                        Strings.Bags.retreiveitem,
                        Strings.Bags.retreiveitemprompt.ToString(ItemBase.Get(Globals.Bag[bagSlot].ItemId).Name), true,
                        InputBox.InputType.NumericInput, RetreiveBagItemInputBoxOkay, null, userData, Globals.Bag[bagSlot].Quantity
                    );
                }
                else
                {
                    PacketSender.SendRetrieveBagItem(bagSlot, 1, invSlot);
                }
            }
        }

        private void RetreiveBagItemInputBoxOkay(object sender, EventArgs e)
        {
            int value = (int)((InputBox)sender).Value;
            if (value > 0)
            {
                int[] userData = (int[])((InputBox)sender).UserData;
                PacketSender.SendRetrieveBagItem(userData[0], value, userData[1]);
            }
        }

        //Trade
        public void TryTradeItem(int index)
        {
            if (ItemBase.Get(Inventory[index].ItemId) != null)
            {
                if (Inventory[index].Quantity > 1)
                {
                    Interface.Interface.InputBox.Show(
                        Strings.Trading.offeritem,
                        Strings.Trading.offeritemprompt.ToString(ItemBase.Get(Inventory[index].ItemId).Name), true,
                        InputBox.InputType.NumericInput, TradeItemInputBoxOkay, null, index, Inventory[index].Quantity
                    );
                }
                else
                {
                    PacketSender.SendOfferTradeItem(index, 1);
                }
            }
        }

        private void TradeItemInputBoxOkay(object sender, EventArgs e)
        {
            int value = (int)((InputBox)sender).Value;
            if (value > 0)
            {
                PacketSender.SendOfferTradeItem((int)((InputBox)sender).UserData, value);
            }
        }

        public void TryRevokeItem(int index)
        {
            Items.Item item = Globals.Trade[0, index];
            if (item != null && ItemBase.Get(item.ItemId) != null)
            {
                if (item.Quantity > 1)
                {
                    Interface.Interface.InputBox.Show(
                        Strings.Trading.revokeitem,
                        Strings.Trading.revokeitemprompt.ToString(ItemBase.Get(item.ItemId).Name),
                        true, InputBox.InputType.NumericInput, RevokeItemInputBoxOkay, null, index
                    );
                }
                else
                {
                    PacketSender.SendRevokeTradeItem(index, 1);
                }
            }
        }

        private void RevokeItemInputBoxOkay(object sender, EventArgs e)
        {
            int value = (int)((InputBox)sender).Value;
            if (value > 0)
            {
                PacketSender.SendRevokeTradeItem((int)((InputBox)sender).UserData, value);
            }
        }

        //Spell Processing
        public void SwapSpells(int spell1, int spell2)
        {
            if (spell1 == spell2)
            {
                //si es el mismo me voy
                return;
            }

            Spells.Spell tmpInstance = Spells[spell2];
            Spells[spell2] = Spells[spell1];
            Spells[spell1] = tmpInstance;

            PacketSender.SendSwapSpells(spell1, spell2);
        }

        public void TryForgetSpell(int index)
        {
            if (SpellBase.Get(Spells[index].SpellId) != null)
            {
                Interface.Interface.InputBox.Show(
                    Strings.Spells.forgetspell,
                    Strings.Spells.forgetspellprompt.ToString(SpellBase.Get(Spells[index].SpellId).Name), true,
                    InputBox.InputType.YesNo, ForgetSpellInputBoxOkay, null, index
                );
            }
        }

        private void ForgetSpellInputBoxOkay(object sender, EventArgs e)
        {
            PacketSender.SendForgetSpell((int)((InputBox)sender).UserData);
        }

        public void TryUseSpell(int index)
        {
            if (Spells[index].SpellId != Guid.Empty &&
                (!Globals.Me.SpellCooldowns.ContainsKey(Spells[index].SpellId) ||
                 Globals.Me.SpellCooldowns[Spells[index].SpellId] < Globals.System.GetTimeMs()))
            {
                SpellBase spellBase = SpellBase.Get(Spells[index].SpellId);

                if (spellBase.CastDuration > 0 && Globals.Me.IsMoving)
                {
                    return;
                }

                PacketSender.SendUseSpell(index, TargetIndex);
            }
        }

        public long GetSpellCooldown(Guid id)
        {
            if (SpellCooldowns.ContainsKey(id))
            {
                return SpellCooldowns[id];
            }

            return 0;
        }

        public void TryUseSpell(Guid spellId)
        {
            if (spellId == Guid.Empty)
            {
                return;
            }

            for (int i = 0; i < Spells.Length; i++)
            {
                if (Spells[i].SpellId == spellId)
                {
                    TryUseSpell(i);

                    return;
                }
            }
        }

        public int FindHotbarSpell(HotbarInstance hotbarInstance)
        {
            if (hotbarInstance.ItemOrSpellId != Guid.Empty && SpellBase.Get(hotbarInstance.ItemOrSpellId) != null)
            {
                for (int i = 0; i < Spells.Length; i++)
                {
                    if (Spells[i].SpellId == hotbarInstance.ItemOrSpellId)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        //Hotbar Processing
        public void AddToHotbar(byte hotbarSlot, sbyte itemType, int itemSlot)
        {
            Hotbar[hotbarSlot].ItemOrSpellId = Guid.Empty;
            Hotbar[hotbarSlot].PreferredStatBuffs = new int[(int)Stats.StatCount];
            if (itemType == 0)
            {
                Items.Item item = Inventory[itemSlot];
                if (item != null)
                {
                    Hotbar[hotbarSlot].ItemOrSpellId = item.ItemId;
                    Hotbar[hotbarSlot].PreferredStatBuffs = item.StatBuffs;
                }
            }
            else if (itemType == 1)
            {
                Spells.Spell spell = Spells[itemSlot];
                if (spell != null)
                {
                    Hotbar[hotbarSlot].ItemOrSpellId = spell.SpellId;
                }
            }

            PacketSender.SendHotbarUpdate(hotbarSlot, itemType, itemSlot);
        }

        public void SwapHotbar(byte index, byte swapIndex)
        {
            if (index == swapIndex)
            {
                return;
            }

            Guid itemId = Hotbar[index].ItemOrSpellId;
            Guid bagId = Hotbar[index].BagId;
            int[] stats = Hotbar[index].PreferredStatBuffs;

            Hotbar[index].ItemOrSpellId = Hotbar[swapIndex].ItemOrSpellId;
            Hotbar[index].BagId = Hotbar[swapIndex].BagId;
            Hotbar[index].PreferredStatBuffs = Hotbar[swapIndex].PreferredStatBuffs;

            Hotbar[swapIndex].ItemOrSpellId = itemId;
            Hotbar[swapIndex].BagId = bagId;
            Hotbar[swapIndex].PreferredStatBuffs = stats;

            PacketSender.SendHotbarSwap(index, swapIndex);
        }

        // Change the dimension if the player is on a gateway
        private void TryToChangeDimension()
        {
            if (X < Options.MapWidth && X >= 0)
            {
                if (Y < Options.MapHeight && Y >= 0)
                {
                    if (MapInstance.Get(CurrentMap) != null && MapInstance.Get(CurrentMap).Attributes[X, Y] != null)
                    {
                        if (MapInstance.Get(CurrentMap).Attributes[X, Y].Type == MapAttributes.ZDimension)
                        {
                            if (((MapZDimensionAttribute)MapInstance.Get(CurrentMap).Attributes[X, Y]).GatewayTo > 0)
                            {
                                Z = (byte)(((MapZDimensionAttribute)MapInstance.Get(CurrentMap).Attributes[X, Y])
                                            .GatewayTo -
                                            1);
                            }
                        }
                    }
                }
            }
        }

        //Input Handling
        private void HandleInput()
        {
            int movex = 0;
            int movey = 0;

            if (Interface.Interface.HasInputFocus())
            {
                return;
            }

            if (Controls.KeyDown(Control.MoveUp))
            {
                movey = 1;
            }

            if (Controls.KeyDown(Control.MoveDown))
            {
                movey = -1;
            }

            if (Controls.KeyDown(Control.MoveLeft))
            {
                movex = -1;
            }

            if (Controls.KeyDown(Control.MoveRight))
            {
                movex = 1;
            }


            Globals.Me.MoveDir = -1;
            if (movex != 0 || movey != 0)
            {
                if (movey < 0)
                {
                    Globals.Me.MoveDir = 1;
                }

                if (movey > 0)
                {
                    Globals.Me.MoveDir = 0;
                }

                if (movex < 0)
                {
                    Globals.Me.MoveDir = 2;
                }

                if (movex > 0)
                {
                    Globals.Me.MoveDir = 3;
                }
            }

            int castInput = -1;
            for (int barSlot = 0; barSlot < Options.MaxHotbar; barSlot++)
            {
                if (!mLastHotbarUseTime.ContainsKey(barSlot))
                {
                    mLastHotbarUseTime.Add(barSlot, 0);
                }

                if (Controls.KeyDown((Control)barSlot + 9))
                {
                    castInput = barSlot;
                }
            }

            if (castInput != -1)
            {
                if (0 <= castInput && castInput < Interface.Interface.GameUi.hotbar.Items?.Count && mLastHotbarUseTime[castInput] < Timing.Global.Milliseconds)
                {
                    Interface.Game.Hotbar.HotbarItem item = Interface.Interface.GameUi.hotbar.Items[castInput];
                    if (item != null)
                    {
                        item.Activate();
                    }
                    mLastHotbarUseTime[castInput] = Timing.Global.Milliseconds + mHotbarUseDelay;
                }
            }
        }

        protected int GetDistanceTo(Entity target)
        {
            if (target != null)
            {
                MapInstance myMap = MapInstance.Get(CurrentMap);
                MapInstance targetMap = MapInstance.Get(target.CurrentMap);
                if (myMap != null && targetMap != null)
                {
                    //Calculate World Tile of Me
                    int x1 = X + myMap.MapGridX * Options.MapWidth;
                    int y1 = Y + myMap.MapGridY * Options.MapHeight;

                    //Calculate world tile of target
                    int x2 = target.X + targetMap.MapGridX * Options.MapWidth;
                    int y2 = target.Y + targetMap.MapGridY * Options.MapHeight;

                    return (int)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
                }
            }

            //Something is null.. return a value that is out of range :) 
            return 9999;
        }

        public void AutoTarget()
        {
            //Check for taunt status if so don't allow to change target
            for (int i = 0; i < Status.Count; i++)
            {
                if (Status[i].Type == StatusTypes.Taunt)
                {
                    return;
                }
            }

            // Do we need to account for players?
            // Depends on what type of map we're currently on.
            if (Globals.Me.MapInstance == null)
            {
                return;
            }

            bool canTargetPlayers = Globals.Me.MapInstance.ZoneType != MapZones.Safe;

            // Build a list of Entities to select from with positions if our list is either old, we've moved or changed maps somehow.
            if (mlastTargetScanTime < Timing.Global.Milliseconds
                || mlastTargetScanMap != Globals.Me.CurrentMap
                || mlastTargetScanLocation != new Point(X, Y)
                )
            {
                // Add new items to our list!
                foreach (KeyValuePair<Guid, Entity> en in Globals.Entities)
                {
                    // Check if this is a valid entity.
                    if (en.Value == null)
                    {
                        continue;
                    }

                    // Don't allow us to auto target ourselves.
                    if (en.Value == Globals.Me)
                    {
                        continue;
                    }

                    // Check if the entity has stealth status
                    if (en.Value.IsStealthed() && !Globals.Me.IsInMyParty(en.Value.Id))
                    {
                        continue;
                    }

                    // Check if we are allowed to target players here, if we're not and this is a player then skip!
                    // If we are, check to see if they're our party or nation member, then exclude them. We're friendly happy people here.
                    if (!canTargetPlayers && en.Value.GetEntityType() == EntityTypes.Player)
                    {
                        continue;
                    }
                    else if (canTargetPlayers && en.Value.GetEntityType() == EntityTypes.Player)
                    {
                        Player player = en.Value as Player;
                        if (IsInMyParty(player))
                        {
                            continue;
                        }
                    }

                    if (en.Value.GetEntityType() == EntityTypes.GlobalEntity || en.Value.GetEntityType() == EntityTypes.Player)
                    {
                        // Already in our list?
                        if (mlastTargetList.ContainsKey(en.Value))
                        {
                            mlastTargetList[en.Value].DistanceTo = GetDistanceTo(en.Value);
                        }
                        else
                        {
                            // Add entity with blank time. Never been selected.
                            mlastTargetList.Add(en.Value, new TargetInfo() { DistanceTo = GetDistanceTo(en.Value), LastTimeSelected = 0 });
                        }
                    }
                }

                // Remove old items.
                KeyValuePair<Entity, TargetInfo>[] toRemove = mlastTargetList.Where(en => !Globals.Entities.ContainsValue(en.Key)).ToArray();
                foreach (KeyValuePair<Entity, TargetInfo> en in toRemove)
                {
                    mlastTargetList.Remove(en.Key);
                }

                // Skip scanning for another second or so.. And set up other values.
                mlastTargetScanTime = Timing.Global.Milliseconds + 300;
                mlastTargetScanMap = CurrentMap;
                mlastTargetScanLocation = new Point(X, Y);
            }

            // Find all valid entities in the direction we are facing.
            KeyValuePair<Entity, TargetInfo>[] validEntities = Array.Empty<KeyValuePair<Entity, TargetInfo>>();

            // TODO: Expose option to users
            if (Globals.Database.TargetAccountDirection)
            {
                switch (Dir)
                {
                    case (byte)Directions.Up:
                        validEntities = mlastTargetList.Where(en =>
                            ((en.Key.CurrentMap == CurrentMap || en.Key.CurrentMap == MapInstance.Left || en.Key.CurrentMap == MapInstance.Right) && en.Key.Y < Y) || en.Key.CurrentMap == MapInstance.Down)
                            .ToArray();
                        break;

                    case (byte)Directions.Down:
                        validEntities = mlastTargetList.Where(en =>
                            ((en.Key.CurrentMap == CurrentMap || en.Key.CurrentMap == MapInstance.Left || en.Key.CurrentMap == MapInstance.Right) && en.Key.Y > Y) || en.Key.CurrentMap == MapInstance.Up)
                            .ToArray();
                        break;

                    case (byte)Directions.Left:
                        validEntities = mlastTargetList.Where(en =>
                            ((en.Key.CurrentMap == CurrentMap || en.Key.CurrentMap == MapInstance.Up || en.Key.CurrentMap == MapInstance.Down) && en.Key.X < X) || en.Key.CurrentMap == MapInstance.Left)
                            .ToArray();
                        break;

                    case (byte)Directions.Right:
                        validEntities = mlastTargetList.Where(en =>
                                    ((en.Key.CurrentMap == CurrentMap || en.Key.CurrentMap == MapInstance.Up || en.Key.CurrentMap == MapInstance.Down) && en.Key.X > X) || en.Key.CurrentMap == MapInstance.Right)
                                    .ToArray();
                        break;
                }
            }
            else
            {
                validEntities = mlastTargetList.ToArray();
            }

            // Reduce the number of targets down to what is in our allowed range.
            validEntities = validEntities.Where(en => en.Value.DistanceTo <= Options.Combat.MaxPlayerAutoTargetRadius).ToArray();

            int currentDistance = 9999;
            long currentTime = Timing.Global.Milliseconds;
            Entity currentEntity = mLastEntitySelected;
            foreach (KeyValuePair<Entity, TargetInfo> entity in validEntities)
            {
                if (currentEntity == entity.Key)
                {
                    continue;
                }

                // if distance is the same
                if (entity.Value.DistanceTo == currentDistance)
                {
                    if (entity.Value.LastTimeSelected < currentTime)
                    {
                        currentTime = entity.Value.LastTimeSelected;
                        currentDistance = entity.Value.DistanceTo;
                        currentEntity = entity.Key;
                    }
                }
                else if (entity.Value.DistanceTo < currentDistance)
                {
                    if (entity.Value.LastTimeSelected < currentTime || entity.Value.LastTimeSelected == currentTime)
                    {
                        currentTime = entity.Value.LastTimeSelected;
                        currentDistance = entity.Value.DistanceTo;
                        currentEntity = entity.Key;
                    }
                }
            }

            // We didn't target anything? Can we default to closest?
            if (currentEntity == null)
            {
                currentEntity = validEntities.Where(x => x.Value.DistanceTo == validEntities.Min(y => y.Value.DistanceTo)).FirstOrDefault().Key;

                // Also reset our target times so we can start auto targetting again.
                foreach (KeyValuePair<Entity, TargetInfo> entity in mlastTargetList)
                {
                    entity.Value.LastTimeSelected = 0;
                }
            }

            if (currentEntity == null)
            {
                mLastEntitySelected = null;
                return;
            }

            if (mlastTargetList.ContainsKey(currentEntity))
            {
                mlastTargetList[currentEntity].LastTimeSelected = Timing.Global.Milliseconds;
            }
            mLastEntitySelected = currentEntity;

            if (TargetIndex != currentEntity.Id)
            {
                SetTargetBox(currentEntity);
                TargetIndex = currentEntity.Id;
                TargetType = 0;
            }
        }
        private void SetTargetBox(Entity en)
        {
            if (en == null)
            {
                TargetBox.Hide();
                return;
            }

            if (en is Player)
            {
                TargetBox.SetEntity(en, EntityTypes.Player);
            }
            else if (en is Event)
            {
                TargetBox.SetEntity(en, EntityTypes.Event);
            }
            else
            {
                TargetBox.SetEntity(en, EntityTypes.GlobalEntity);
            }

            TargetBox.Show();
        }

        public bool TryBlock()
        {
            if (AttackTimer > Timing.Global.Ticks / TimeSpan.TicksPerMillisecond)
            {
                return false;
            }

            if (Options.ShieldIndex > -1 && Globals.Me.MyEquipment[Options.ShieldIndex] > -1)
            {
                ItemBase item = ItemBase.Get(Globals.Me.Inventory[Globals.Me.MyEquipment[Options.ShieldIndex]].ItemId);
                if (item != null)
                {
                    PacketSender.SendBlock(true);
                    Blocking = true;

                    return true;
                }
            }

            return false;
        }

        public void StopBlocking()
        {
            if (Blocking)
            {
                Blocking = false;
                PacketSender.SendBlock(false);
                AttackTimer = Timing.Global.Ticks / TimeSpan.TicksPerMillisecond + CalculateAttackTime();
            }
        }

        private bool TryAttack()
        {
            if (AttackTimer > Timing.Global.Ticks / TimeSpan.TicksPerMillisecond || Blocking || (IsMoving && !Options.Instance.PlayerOpts.AllowCombatMovement))
            {
                return false;
            }

            int x = Globals.Me.X;
            int y = Globals.Me.Y;
            Guid map = Globals.Me.CurrentMap;
            switch (Globals.Me.Dir)
            {
                case 0:
                    y--;
                    break;
                case 1:
                    y++;
                    break;
                case 2:
                    x--;
                    break;
                case 3:
                    x++;
                    break;
            }

            if (GetRealLocation(ref x, ref y, ref map))
            {
                foreach (KeyValuePair<Guid, Entity> en in Globals.Entities)
                {
                    if (en.Value == null)
                    {
                        continue;
                    }

                    if (en.Value != Globals.Me
                        && en.Value.CurrentMap == map
                        && en.Value.X == x && en.Value.Y == y
                        && en.Value.CanBeAttacked())
                    {
                        //ATTACKKKKK!!!
                        PacketSender.SendAttack(en.Key);
                        AttackTimer = Timing.Global.Ticks / TimeSpan.TicksPerMillisecond + CalculateAttackTime();

                        return true;
                    }
                }
            }

            foreach (MapInstance eventMap in MapInstance.Lookup.Values)
            {
                foreach (KeyValuePair<Guid, Entity> en in eventMap.LocalEntities)
                {
                    if (en.Value == null)
                    {
                        continue;
                    }

                    if (en.Value.CurrentMap == map && en.Value.X == x && en.Value.Y == y)
                    {
                        if (en.Value is Event)
                        {
                            //Talk to Event
                            PacketSender.SendActivateEvent(en.Key);
                            AttackTimer = Timing.Global.Ticks / TimeSpan.TicksPerMillisecond + CalculateAttackTime();

                            return true;
                        }
                    }
                }
            }

            //Projectile/empty swing for animations
            PacketSender.SendAttack(Guid.Empty);
            AttackTimer = Timing.Global.Ticks / TimeSpan.TicksPerMillisecond + CalculateAttackTime();

            return true;
        }

        public bool GetRealLocation(ref int x, ref int y, ref Guid mapId)
        {
            int tmpX = x;
            int tmpY = y;
            if (MapInstance.Get(mapId) != null)
            {
                int gridX = MapInstance.Get(mapId).MapGridX;
                int gridY = MapInstance.Get(mapId).MapGridY;

                if (x < 0)
                {
                    tmpX = Options.MapWidth - x * -1;
                    gridX--;
                }

                if (y < 0)
                {
                    tmpY = Options.MapHeight - y * -1;
                    gridY--;
                }

                if (y > Options.MapHeight - 1)
                {
                    tmpY = y - Options.MapHeight;
                    gridY++;
                }

                if (x > Options.MapWidth - 1)
                {
                    tmpX = x - Options.MapWidth;
                    gridX++;
                }

                if (gridX >= 0 && gridX < Globals.MapGridWidth && gridY >= 0 && gridY < Globals.MapGridHeight)
                {
                    if (MapInstance.Get(Globals.MapGrid[gridX, gridY]) != null)
                    {
                        x = (byte)tmpX;
                        y = (byte)tmpY;
                        mapId = Globals.MapGrid[gridX, gridY];

                        return true;
                    }
                }
            }

            return false;
        }

        public bool TryTarget()
        {
            //Check for taunt status if so don't allow to change target
            for (int i = 0; i < Status.Count; i++)
            {
                if (Status[i].Type == StatusTypes.Taunt)
                {
                    return false;
                }
            }

            int x = (int)Globals.InputManager.GetMousePosition().X;
            int y = (int)Globals.InputManager.GetMousePosition().Y;
            FloatRect targetRect = new FloatRect(x, y, 1, 1); //Adjust to allow more/less error

            Entity bestMatch = null;
            float bestAreaMatch = 0f;

            foreach (MapInstance map in MapInstance.Lookup.Values)
            {
                if (x >= map.GetX() && x <= map.GetX() + Options.MapWidth)
                {
                    if (y >= map.GetY() && y <= map.GetY() + Options.MapHeight)
                    {
                        //Remove the offsets to just be dealing with pixels within the map selected
                        x -= (int)map.GetX();
                        y -= (int)map.GetY();

                        //transform pixel format to tile format
                        Guid mapId = map.Id;
                        if (GetRealLocation(ref x, ref y, ref mapId))
                        {
                            foreach (KeyValuePair<Guid, Entity> en in Globals.Entities)
                            {
                                if (en.Value == null || en.Value.CurrentMap != mapId || en.Value is Projectile || en.Value is Resource || (en.Value.IsStealthed() && !Globals.Me.IsInMyParty(en.Value.Id)))
                                {
                                    continue;
                                }

                                FloatRect intersectRect = FloatRect.Intersect(en.Value.WorldRect, targetRect);
                                if (intersectRect.Width * intersectRect.Height > bestAreaMatch)
                                {
                                    bestAreaMatch = intersectRect.Width * intersectRect.Height;
                                    bestMatch = en.Value;
                                }
                            }

                            foreach (MapInstance eventMap in MapInstance.Lookup.Values)
                            {
                                foreach (KeyValuePair<Guid, Entity> en in eventMap.LocalEntities)
                                {
                                    if (en.Value == null || en.Value.CurrentMap != mapId || ((Event)en.Value).DisablePreview)
                                    {
                                        continue;
                                    }

                                    FloatRect intersectRect = FloatRect.Intersect(en.Value.WorldRect, targetRect);
                                    if (intersectRect.Width * intersectRect.Height > bestAreaMatch)
                                    {
                                        bestAreaMatch = intersectRect.Width * intersectRect.Height;
                                        bestMatch = en.Value;
                                    }
                                }
                            }

                            if (bestMatch != null && bestMatch.Id != TargetIndex)
                            {
                                int targetType = bestMatch is Event ? 1 : 0;


                                SetTargetBox(bestMatch);

                                if (bestMatch is Player)
                                {
                                    //Select in admin window if open
                                    if (Interface.Interface.GameUi.AdminWindowOpen())
                                    {
                                        Interface.Interface.GameUi.AdminWindowSelectName(bestMatch.Name);
                                    }
                                }

                                TargetType = targetType;
                                TargetIndex = bestMatch.Id;

                                return true;
                            }
                            else if (!Globals.Database.StickyTarget)
                            {
                                // We've clicked off of our target and are allowed to clear it!
                                ClearTarget();
                                return true;
                            }
                        }
                        return false;
                    }
                }
            }

            return false;
        }

        public bool TryTarget(Entity entity, bool force = false)
        {
            //Check for taunt status if so don't allow to change target
            for (int i = 0; i < Status.Count; i++)
            {
                if (Status[i].Type == StatusTypes.Taunt && !force)
                {
                    return false;
                }
            }

            if (entity == null)
            {
                return false;
            }

            // Are we already targetting this?
            if (TargetBox != null && TargetBox.MyEntity == entity)
            {
                return true;
            }

            int targetType = entity is Event ? 1 : 0;

            if (entity.GetType() == typeof(Player))
            {
                //Select in admin window if open
                if (Interface.Interface.GameUi.AdminWindowOpen())
                {
                    Interface.Interface.GameUi.AdminWindowSelectName(entity.Name);
                }
            }

            if (TargetIndex != entity.Id)
            {
                SetTargetBox(entity);
                TargetType = targetType;
                TargetIndex = entity.Id;
            }

            return true;

        }
        public void ClearTarget()
        {
            TargetBox.Dispose();

            TargetIndex = Guid.Empty;
            TargetType = -1;
        }

        /// <summary>
        /// Attempts to pick up an item at the specified location.
        /// </summary>
        /// <param name="mapId">The Id of the map we are trying to loot from.</param>
        /// <param name="x">The X location on the current map.</param>
        /// <param name="y">The Y location on the current map.</param>
        /// <param name="uniqueId">The Unique Id of the specific item we want to pick up, leave <see cref="Guid.Empty"/> to not specificy an item and pick up the first thing we can find.</param>
        /// <param name="firstOnly">Defines whether we only want to pick up the first item we can find when true, or all items when false.</param>
        /// <returns></returns>
        public bool TryPickupItem(Guid mapId, int tileIndex, Guid uniqueId = new Guid(), bool firstOnly = false)
        {
            MapInstance map = MapInstance.Get(mapId);
            if (map == null || tileIndex < 0 || tileIndex >= Options.MapWidth * Options.MapHeight)
            {
                return false;
            }

            // Are we trying to pick up anything in particular, or everything?
            if (uniqueId != Guid.Empty || firstOnly)
            {
                if (!map.MapItems.ContainsKey(tileIndex) || map.MapItems[tileIndex].Count < 1)
                {
                    return false;
                }

                foreach (Items.MapItemInstance item in map.MapItems[tileIndex])
                {
                    // Check if we are trying to pick up a specific item, and if this is the one.
                    if (uniqueId != Guid.Empty && item.UniqueId != uniqueId)
                    {
                        continue;
                    }

                    PacketSender.SendPickupItem(mapId, tileIndex, item.UniqueId);

                    return true;
                }
            }
            else
            {
                // Let the server worry about what we can and can not pick up.
                PacketSender.SendPickupItem(mapId, tileIndex, uniqueId);

                return true;
            }

            return false;
        }

        //Forumlas
        public long GetNextLevelExperience()
        {
            return ExperienceToNextLevel;
        }

        public override int CalculateAttackTime()
        {
            ItemBase weapon = null;
            int attackTime = base.CalculateAttackTime();

            ClassBase cls = ClassBase.Get(Class);
            if (cls != null && cls.AttackSpeedModifier == 1) //Static
            {
                attackTime = cls.AttackSpeedValue;
            }

            if (this == Globals.Me)
            {
                if (Options.WeaponIndex > -1 &&
                    Options.WeaponIndex < Equipment.Length &&
                    MyEquipment[Options.WeaponIndex] >= 0)
                {
                    weapon = ItemBase.Get(Inventory[MyEquipment[Options.WeaponIndex]].ItemId);
                }
            }
            else
            {
                if (Options.WeaponIndex > -1 &&
                    Options.WeaponIndex < Equipment.Length &&
                    Equipment[Options.WeaponIndex] != Guid.Empty)
                {
                    weapon = ItemBase.Get(Equipment[Options.WeaponIndex]);
                }
            }

            if (weapon != null)
            {
                if (weapon.AttackSpeedModifier == 1) // Static
                {
                    attackTime = weapon.AttackSpeedValue;
                }
                else if (weapon.AttackSpeedModifier == 2) //Percentage
                {
                    attackTime = (int)(attackTime * (100f / weapon.AttackSpeedValue));
                }
            }

            return attackTime;
        }

        //Movement Processing
        private void ProcessDirectionalInput()
        {
            //Check if player is crafting
            if (Globals.InCraft)
            {
                return;
            }

            //check if player is stunned or snared, if so don't let them move.
            for (int n = 0; n < Status.Count; n++)
            {
                if (Status[n].Type == StatusTypes.Stun ||
                    Status[n].Type == StatusTypes.Snare ||
                    Status[n].Type == StatusTypes.Sleep)
                {
                    return;
                }
            }

            //Check if the player is dashing, if so don't let them move.
            if (Dashing != null || DashQueue.Count > 0 || DashTimer > Globals.System.GetTimeMs())
            {
                return;
            }

            if (AttackTimer > Timing.Global.Ticks / TimeSpan.TicksPerMillisecond && !Options.Instance.PlayerOpts.AllowCombatMovement)
            {
                return;
            }

            sbyte tmpX = (sbyte)X;
            sbyte tmpY = (sbyte)Y;
            Entity blockedBy = null;

            if (MoveDir > -1 && Globals.EventDialogs.Count == 0)
            {
                //Try to move if able and not casting spells.
                if (!IsMoving && MoveTimer < Timing.Global.Ticks / TimeSpan.TicksPerMillisecond && (Options.Combat.MovementCancelsCast || CastTime < Globals.System.GetTimeMs()))
                {
                    if (Options.Combat.MovementCancelsCast)
                    {
                        CastTime = 0;
                    }

                    switch (MoveDir)
                    {
                        case 0: // Up
                            if (IsTileBlocked(X, Y - 1, Z, CurrentMap, ref blockedBy) == -1)
                            {
                                tmpY--;
                                Dir = 0;
                                IsMoving = true;
                                OffsetY = 1;
                                OffsetX = 0;
                            }

                            break;
                        case 1: // Down
                            if (IsTileBlocked(X, Y + 1, Z, CurrentMap, ref blockedBy) == -1)
                            {
                                tmpY++;
                                Dir = 1;
                                IsMoving = true;
                                OffsetY = -1;
                                OffsetX = 0;
                            }

                            break;
                        case 2: // Left
                            if (IsTileBlocked(X - 1, Y, Z, CurrentMap, ref blockedBy) == -1)
                            {
                                tmpX--;
                                Dir = 2;
                                IsMoving = true;
                                OffsetY = 0;
                                OffsetX = 1;
                            }

                            break;
                        case 3: // Right
                            if (IsTileBlocked(X + 1, Y, Z, CurrentMap, ref blockedBy) == -1)
                            {
                                tmpX++;
                                Dir = 3;
                                IsMoving = true;
                                OffsetY = 0;
                                OffsetX = -1;
                            }

                            break;
                    }

                    if (blockedBy != mLastBumpedEvent)
                    {
                        mLastBumpedEvent = null;
                    }

                    if (IsMoving)
                    {
                        if (tmpX < 0 || tmpY < 0 || tmpX > Options.MapWidth - 1 || tmpY > Options.MapHeight - 1)
                        {
                            int gridX = MapInstance.Get(Globals.Me.CurrentMap).MapGridX;
                            int gridY = MapInstance.Get(Globals.Me.CurrentMap).MapGridY;
                            if (tmpX < 0)
                            {
                                gridX--;
                                X = (byte)(Options.MapWidth - 1);
                            }
                            else if (tmpX >= Options.MapWidth)
                            {
                                X = 0;
                                gridX++;
                            }
                            else
                            {
                                X = (byte)tmpX;
                            }

                            if (tmpY < 0)
                            {
                                gridY--;
                                Y = (byte)(Options.MapHeight - 1);
                            }
                            else if (tmpY >= Options.MapHeight)
                            {
                                Y = 0;
                                gridY++;
                            }
                            else
                            {
                                Y = (byte)tmpY;
                            }

                            if (CurrentMap != Globals.MapGrid[gridX, gridY])
                            {
                                CurrentMap = Globals.MapGrid[gridX, gridY];
                                FetchNewMaps();
                            }
                        }
                        else
                        {
                            X = (byte)tmpX;
                            Y = (byte)tmpY;
                        }

                        TryToChangeDimension();
                        PacketSender.SendMove();
                        MoveTimer = (Timing.Global.Ticks / TimeSpan.TicksPerMillisecond) + (long)GetMovementTime();
                    }
                    else
                    {
                        if (MoveDir != Dir)
                        {
                            Dir = (byte)MoveDir;
                            PacketSender.SendDirection(Dir);
                        }

                        if (blockedBy != null && mLastBumpedEvent != blockedBy && blockedBy is Event)
                        {
                            PacketSender.SendBumpEvent(blockedBy.CurrentMap, blockedBy.Id);
                            mLastBumpedEvent = blockedBy;
                        }
                    }
                }
            }
        }

        public void FetchNewMaps()
        {
            if (Globals.MapGridWidth == 0 || Globals.MapGridHeight == 0)
            {
                return;
            }

            if (MapInstance.Get(Globals.Me.CurrentMap) != null)
            {
                int gridX = MapInstance.Get(Globals.Me.CurrentMap).MapGridX;
                int gridY = MapInstance.Get(Globals.Me.CurrentMap).MapGridY;
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
                            if (MapInstance.Get(Globals.MapGrid[x, y]) == null)
                            {
                                PacketSender.SendNeedMap(Globals.MapGrid[x, y]);
                            }
                        }
                    }
                }
            }
        }

        //Override of the original function, used for rendering the color of a player based on rank
        public override void DrawName(Color textColor, Color borderColor, Color backgroundColor)
        {
            if (textColor == null)
            {
                if (Type == 1) //Mod
                {
                    textColor = CustomColors.Names.Players["Moderator"].Name;
                    borderColor = CustomColors.Names.Players["Moderator"].Outline;
                    backgroundColor = CustomColors.Names.Players["Moderator"].Background;
                }
                else if (Type == 2) //Admin
                {
                    textColor = CustomColors.Names.Players["Admin"].Name;
                    borderColor = CustomColors.Names.Players["Admin"].Outline;
                    backgroundColor = CustomColors.Names.Players["Admin"].Background;
                }
                else //No Power
                {
                    textColor = CustomColors.Names.Players["Normal"].Name;
                    borderColor = CustomColors.Names.Players["Normal"].Outline;
                    backgroundColor = CustomColors.Names.Players["Normal"].Background;
                }
            }

            Color customColorOverride = NameColor;
            if (customColorOverride != null)
            {
                //We don't want to override the default colors if the color is transparent!
                if (customColorOverride.A != 0)
                {
                    textColor = customColorOverride;
                }
            }

            DrawNameAndLabels(textColor, borderColor, backgroundColor);
        }

        private void DrawNameAndLabels(Color textColor, Color borderColor, Color backgroundColor)
        {
            base.DrawName(textColor, borderColor, backgroundColor);
            DrawLabels(HeaderLabel.Text, 0, HeaderLabel.Color, textColor, borderColor, backgroundColor);
            DrawLabels(FooterLabel.Text, 1, FooterLabel.Color, textColor, borderColor, backgroundColor);
            DrawGuildName(textColor, borderColor, backgroundColor);
        }

        public virtual void DrawGuildName(Color textColor, Color borderColor = null, Color backgroundColor = null)
        {
            PlayerEntityRenderer playerRenderer = (PlayerEntityRenderer)entityRender;
            if (HideName || string.IsNullOrWhiteSpace(Guild))
            {
                playerRenderer.HideGuild();
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

            //Check for stealth amoungst status effects.
            for (int n = 0; n < Status.Count; n++)
            {
                //If unit is stealthed, don't render unless the entity is the player.
                if (Status[n].Type == StatusTypes.Stealth)
                {
                    if (this != Globals.Me && !(this is Player player && Globals.Me.IsInMyParty(player)))
                    {
                        playerRenderer.HideGuild();
                        return;
                    }
                }
            }

            MapInstance map = MapInstance;
            if (map == null)
            {
                playerRenderer.HideGuild();
                return;
            }

            playerRenderer.DrawGuildName(Guild, textColor);
            playerRenderer.SetGuildLabelPosition(0, GetLabelLocation(LabelType.Guild));
        }

        public void DrawTargets()
        {
            foreach (KeyValuePair<Guid, Entity> en in Globals.Entities)
            {
                if (en.Value == null)
                {
                    continue;
                }

                if (!en.Value.IsStealthed() || en.Value is Player player && Globals.Me.IsInMyParty(player))
                {
                    if (en.Value.GetType() != typeof(Projectile) && en.Value.GetType() != typeof(Resource))
                    {
                        if (TargetType == 0 && TargetIndex == en.Value.Id)
                        {
                            en.Value.DrawTarget((int)TargetTypes.Selected);
                        }
                        else
                        {
                            en.Value.HideTarget();
                        }
                    }
                }
                else
                {
                    //TODO: Completely wipe the stealthed player from memory and have server re-send once stealth ends.
                    ClearTarget();
                }
            }

            foreach (MapInstance eventMap in MapInstance.Lookup.Values)
            {
                foreach (KeyValuePair<Guid, Entity> en in eventMap.LocalEntities)
                {
                    if (en.Value == null)
                    {
                        continue;
                    }

                    if (en.Value.CurrentMap == eventMap.Id &&
                        !((Event)en.Value).DisablePreview &&
                        (!en.Value.IsStealthed() || en.Value is Player player && Globals.Me.IsInMyParty(player)))
                    {
                        if (TargetType == 1 && TargetIndex == en.Value.Id)
                        {
                            en.Value.DrawTarget((int)TargetTypes.Selected);
                        }
                        else
                        {
                            en.Value.HideTarget();
                        }
                    }
                }
            }

            Pointf mousePos = Globals.InputManager.GetMousePosition();

            Utils.Draw.Ellipse(mousePos, .1f, .1f, 10, UnityEngine.Color.white);

            foreach (MapInstance map in MapInstance.Lookup.Values)
            {
                if (mousePos.X >= map.GetX() && mousePos.X <= map.GetX() + Options.MapWidth)
                {
                    if (mousePos.Y >= map.GetY() && mousePos.Y <= map.GetY() + Options.MapHeight)
                    {
                        Guid mapId = map.Id;

                        foreach (KeyValuePair<Guid, Entity> en in Globals.Entities)
                        {
                            if (en.Value == null)
                            {
                                continue;
                            }

                            if (en.Value.CurrentMap == mapId &&
                                !en.Value.IsStealthed() &&
                                en.Value.WorldRect.Contains(mousePos.X, mousePos.Y))
                            {
                                if (en.Value.GetType() != typeof(Projectile) && en.Value.GetType() != typeof(Resource))
                                {
                                    if (TargetType != 0 || TargetIndex != en.Value.Id)
                                    {
                                        en.Value.DrawTarget((int)TargetTypes.Hover);
                                    }
                                }
                            }
                        }

                        foreach (MapInstance eventMap in MapInstance.Lookup.Values)
                        {
                            foreach (KeyValuePair<Guid, Entity> en in eventMap.LocalEntities)
                            {
                                if (en.Value == null)
                                {
                                    continue;
                                }

                                if (en.Value.CurrentMap == mapId &&
                                    !((Event)en.Value).DisablePreview &&
                                    !en.Value.IsStealthed() &&
                                    en.Value.WorldRect.Contains(mousePos.X, mousePos.Y))
                                {
                                    if (TargetType != 1 || TargetIndex != en.Value.Id)
                                    {
                                        en.Value.DrawTarget((int)TargetTypes.Hover);
                                    }
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }

        internal void DrawLight(float size, float intensity, float expand, UnityEngine.Color color)
        {
            PlayerEntityRenderer playerRenderer = (PlayerEntityRenderer)entityRender;
            playerRenderer.UpdateLight(size, intensity, expand, color);
        }

        private class TargetInfo
        {
            public long LastTimeSelected;

            public int DistanceTo;
        }
    }

    public class FriendInstance
    {

        public string Map;

        public string Name;

        public bool Online = false;

    }

    public class HotbarInstance
    {

        public Guid BagId = Guid.Empty;

        public Guid ItemOrSpellId = Guid.Empty;

        public int[] PreferredStatBuffs = new int[(int)Stats.StatCount];

        public void Load(string data)
        {
            JsonConvert.PopulateObject(data, this);
        }

    }

}
