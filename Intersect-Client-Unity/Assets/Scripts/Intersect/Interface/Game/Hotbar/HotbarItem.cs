using Intersect.Client.Core.Controls;
using Intersect.Client.Entities;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Interface.Game.Inventory;
using Intersect.Client.Interface.Game.Spells;
using Intersect.Client.Items;
using Intersect.Client.Localization;
using Intersect.Client.Spells;
using Intersect.GameObjects;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Intersect.Client.Interface.Game.Hotbar
{
    public class HotbarItem : UIBaseItem, IPointerClickHandler
    {
        private KeyCode mHotKey;
        private bool mTexLoaded;

        private Guid mCurrentId = Guid.Empty;

        private ItemBase mCurrentItem = null;
        private Item mInventoryItem = null;
        private int mInventoryItemIndex = -1;

        private SpellBase mCurrentSpell = null;
        private Spell mSpellBookItem = null;


        private bool mIsEquipped;
        private bool mIsFaded;

        public override void Setup(int index, Transform descTransformDisplay)
        {
            base.Setup(index, myTransform);
            displayer.SetTextTop(Strings.Inventory.equippedicon);
            displayer.TextTopVisible(false);
            displayer.TextCoolDownVisible(false);
        }

        internal void Activate()
        {
            if (mCurrentId != Guid.Empty)
            {
                if (mCurrentItem != null)
                {
                    if (mInventoryItemIndex > -1)
                    {
                        Globals.Me.TryUseItem(mInventoryItemIndex);
                    }
                }
                else if (mCurrentSpell != null)
                {
                    Globals.Me.TryUseSpell(mCurrentSpell.Id);
                }
            }
        }

        internal void Draw()
        {
            //See if Label Should be changed
            if (mHotKey != Controls.ActiveControls.ControlMapping[Control.Hotkey1 + Index].Key1)
            {
                displayer.SetTextBottom(Strings.Keys.keydict[Controls.ActiveControls.ControlMapping[Control.Hotkey1 + Index].Key1]);
                displayer.TextBottomVisible(true);
                mHotKey = Controls.ActiveControls.ControlMapping[Control.Hotkey1 + Index].Key1;
            }

            HotbarInstance slot = Globals.Me.Hotbar[Index];
            bool updateDisplay = mCurrentId != slot.ItemOrSpellId
                || mTexLoaded == false; //Update display if the hotbar item changes or we dont have a texture for the current item

            if (mCurrentId != slot.ItemOrSpellId)
            {
                mCurrentItem = null;
                mCurrentSpell = null;
                ItemBase itm = ItemBase.Get(slot.ItemOrSpellId);
                SpellBase spl = SpellBase.Get(slot.ItemOrSpellId);
                if (itm != null)
                {
                    mCurrentItem = itm;
                }

                if (spl != null)
                {
                    mCurrentSpell = spl;
                }

                mCurrentId = slot.ItemOrSpellId;
            }

            mSpellBookItem = null;
            mInventoryItem = null;
            mInventoryItemIndex = -1;

            if (mCurrentItem != null)
            {
                int itmIndex = Globals.Me.FindHotbarItem(slot);
                if (itmIndex > -1)
                {
                    mInventoryItemIndex = itmIndex;
                    mInventoryItem = Globals.Me.Inventory[itmIndex];
                }
            }
            else if (mCurrentSpell != null)
            {
                int splIndex = Globals.Me.FindHotbarSpell(slot);
                if (splIndex > -1)
                {
                    mSpellBookItem = Globals.Me.Spells[splIndex];
                }
            }

            if (mCurrentItem != null) //When it's an item
            {
                //We don't have it, and the icon isn't faded
                if (mInventoryItem == null && !mIsFaded)
                {
                    updateDisplay = true;
                }

                //We have it, and the equip icon doesn't match equipped status
                if (mInventoryItem != null && Globals.Me.IsEquipped(mInventoryItemIndex) != mIsEquipped)
                {
                    updateDisplay = true;
                }

                //We have it, and it's on cd
                if (mInventoryItem != null && Globals.Me.ItemOnCd(mInventoryItemIndex))
                {
                    updateDisplay = true;
                }

                //We have it, and it's on cd, and the fade is incorrect
                if (mInventoryItem != null && Globals.Me.ItemOnCd(mInventoryItemIndex) != mIsFaded)
                {
                    updateDisplay = true;
                }
            }

            if (mCurrentSpell != null) //When it's a spell
            {
                //We don't know it, and the icon isn't faded!
                if (mSpellBookItem == null && !mIsFaded)
                {
                    updateDisplay = true;
                }

                //Spell on cd
                if (mSpellBookItem != null &&
                    Globals.Me.GetSpellCooldown(mSpellBookItem.SpellId) > Globals.System.GetTimeMs())
                {
                    updateDisplay = true;
                }

                //Spell on cd and the fade is incorrect
                if (mSpellBookItem != null &&
                    Globals.Me.GetSpellCooldown(mSpellBookItem.SpellId) > Globals.System.GetTimeMs() != mIsFaded)
                {
                    updateDisplay = true;
                }
            }

            displayer.Set(mCurrentItem, mInventoryItem, mCurrentSpell);

            if (updateDisplay) //Item on cd and fade is incorrect
            {
                if (mCurrentItem != null)
                {
                    displayer.TextCoolDownVisible(false);
                    displayer.IconVisible(true);
                    GameTexture texture = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Item, mCurrentItem.Icon);
                    if (texture != null)
                    {
                        displayer.IconSprite(texture.GetSpriteDefault());
                        displayer.IconVisible(true);
                    }
                    else
                    {
                        displayer.IconVisible(false);
                    }

                    if (mInventoryItemIndex > -1)
                    {
                        displayer.TextTopVisible(Globals.Me.IsEquipped(mInventoryItemIndex));
                        mIsFaded = Globals.Me.ItemOnCd(mInventoryItemIndex);
                        if (mIsFaded)
                        {
                            displayer.TextCoolDownVisible(true);
                            float secondsRemaining = Globals.Me.ItemCdRemainder(mInventoryItemIndex) / 1000f;
                            if (secondsRemaining > 10f)
                            {
                                displayer.SetTextCoolDown(Strings.Inventory.cooldown.ToString(secondsRemaining.ToString("N0")));
                            }
                            else
                            {
                                displayer.SetTextCoolDown(Strings.Inventory.cooldown.ToString(secondsRemaining.ToString("N1").Replace(".", Strings.Numbers.dec)));
                            }
                        }

                        mIsEquipped = Globals.Me.IsEquipped(mInventoryItemIndex);
                    }
                    else
                    {
                        displayer.TextTopVisible(false);
                        mIsEquipped = false;
                        mIsFaded = true;
                    }

                    mTexLoaded = true;
                }
                else if (mCurrentSpell != null)
                {
                    displayer.TextCoolDownVisible(false);
                    displayer.IconVisible(true);
                    GameTexture gameTexture = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Spell, mCurrentSpell.Icon);
                    if (gameTexture != null)
                    {
                        displayer.IconSprite(gameTexture.GetSpriteDefault());
                        displayer.IconVisible(true);
                    }
                    else
                    {
                        displayer.IconVisible(false);
                    }


                    displayer.TextTopVisible(false);
                    if (mSpellBookItem != null)
                    {
                        mIsFaded = Globals.Me.GetSpellCooldown(mSpellBookItem.SpellId) > Globals.System.GetTimeMs();
                        if (mIsFaded)
                        {
                            displayer.TextCoolDownVisible(true);
                            float secondsRemaining = (Globals.Me.GetSpellCooldown(mSpellBookItem.SpellId) - Globals.System.GetTimeMs()) / 1000f;

                            if (secondsRemaining > 10f)
                            {
                                displayer.SetTextCoolDown(Strings.Spells.cooldown.ToString(secondsRemaining.ToString("N0")));
                            }
                            else
                            {
                                displayer.SetTextCoolDown(Strings.Spells.cooldown.ToString(secondsRemaining.ToString("N1").Replace(".", Strings.Numbers.dec)));
                            }
                        }
                    }
                    else
                    {
                        mIsFaded = true;
                    }

                    mTexLoaded = true;
                    mIsEquipped = false;
                }
                else
                {
                    displayer.IconVisible(false);
                    mTexLoaded = true;
                    mIsEquipped = false;
                    displayer.TextTopVisible(false);
                    displayer.TextCoolDownVisible(false);
                }

                if (mIsFaded)
                {
                    displayer.IconColor(new Color32(255, 255, 255, 100));
                }
                else
                {
                    displayer.IconColor(UnityEngine.Color.white);
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Guid.Empty.Equals(mCurrentId))
            {
                return;
            }

            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                {
                    Activate();
                }
                break;
                case PointerEventData.InputButton.Right:
                {
                    Globals.Me.AddToHotbar((byte)Index, -1, -1);
                }
                break;
                case PointerEventData.InputButton.Middle:
                    break;
            }
        }

        public override void Drop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
            {
                return;
            }

            InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            if (inventoryItem != null)
            {
                Globals.Me.AddToHotbar((byte)Index, 0, inventoryItem.Index);
                return;
            }

            SpellItem spellItem = eventData.pointerDrag.GetComponent<SpellItem>();
            if (spellItem != null)
            {
                Globals.Me.AddToHotbar((byte)Index, 1, spellItem.Index);
                return;
            }

            HotbarItem hotbarItem = eventData.pointerDrag.GetComponent<HotbarItem>();
            if (hotbarItem != null)
            {
                Globals.Me.SwapHotbar((byte)Index, (byte)hotbarItem.Index);
                return;
            }

        }
    }
}
