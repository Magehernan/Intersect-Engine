using Intersect.Client.Entities;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Interface.Game.Displayers;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UI.Components;
using Intersect.Client.UnityGame;
using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.EntityPanel
{

    public class EntityBox : Window
    {
        [SerializeField]
        private SpellStatus spellStatusPrefab;
        [SerializeField]
        private Transform spellStatusContainer;
        [SerializeField]
        private TextMeshProUGUI textName;
        [SerializeField]
        private TextMeshProUGUI textMap;
        [SerializeField]
        private TextMeshProUGUI textEventDesc;
        [SerializeField]
        private TextMeshProUGUI labelHP;
        [SerializeField]
        private TextMeshProUGUI textHP;
        [SerializeField]
        private FillBar fillHP;
        [SerializeField]
        private TextMeshProUGUI labelMP;
        [SerializeField]
        private TextMeshProUGUI textMP;
        [SerializeField]
        private FillBar fillMP;
        [SerializeField]
        private TextMeshProUGUI labelXP;
        [SerializeField]
        private TextMeshProUGUI textXP;
        [SerializeField]
        private FillBar fillXP;
        [SerializeField]
        private EntityDisplayer entityDisplayer;
        [SerializeField]
        private Button buttonGuild;
        [SerializeField]
        private TextMeshProUGUI labelGuild;
        [SerializeField]
        private Button buttonTrade;
        [SerializeField]
        private TextMeshProUGUI labelTrade;
        [SerializeField]
        private Button buttonParty;
        [SerializeField]
        private TextMeshProUGUI labelParty;
        [SerializeField]
        private Button buttonFriend;
        [SerializeField]
        private TextMeshProUGUI labelFriend;
        [SerializeField]
        private Transform descTransform;


        private EntityTypes entityType;
        private bool mInitialized;
        private readonly Dictionary<Guid, SpellStatus> mActiveStatuses = new Dictionary<Guid, SpellStatus>();

        public bool UpdateStatuses { get; set; }
        public Entity MyEntity { get; private set; }


        private void Start()
        {
            labelHP.text = Strings.EntityBox.vital0;
            labelMP.text = Strings.EntityBox.vital1;
            labelXP.text = Strings.EntityBox.exp;
            labelGuild.text = Strings.Guilds.Guild;
            labelTrade.text = Strings.EntityBox.trade;
            labelParty.text = Strings.EntityBox.party;
            labelFriend.text = Strings.EntityBox.friend;

            buttonGuild.onClick.AddListener(OnClickGuild);
            buttonTrade.onClick.AddListener(OnClickTrade);
            buttonParty.onClick.AddListener(OnClickParty);
            buttonFriend.onClick.AddListener(OnClickFriend);
        }


        public void SetEntity(Entity myEntity, EntityTypes entityType)
        {
            this.entityType = entityType;
            MyEntity = myEntity;
            entityDisplayer.Set(MyEntity);
            SetupEntityElements();
        }

        internal void Draw()
        {
            if (MyEntity is null)
            {
                if (MyGameObject.activeSelf)
                {
                    Hide();
                }
                return;
            }

            if (!MyGameObject.activeSelf)
            {
                Show();
            }

            if (MyEntity.IsDisposed())
            {
                Dispose();
                return;
            }

            if (!mInitialized)
            {
                SetupEntityElements();
                UpdateSpellStatus();
                //if (entityType == EntityTypes.Event)
                //{
                //    EventDesc.AddText(((Event)MyEntity).Desc, Color.White);
                //}

                mInitialized = true;
            }


            //Update the event/entity face.
            entityDisplayer.UpdateImage();

            if (entityType != EntityTypes.Event)
            {
                UpdateLevel();
                UpdateMap();
                UpdateHpBar();
                UpdateMpBar();
            }
            else
            {
                if (textName.enabled)
                {
                    textName.text = MyEntity.Name;
                }
            }

            //If player draw exp bar
            if (MyEntity == Globals.Me)
            {
                UpdateXpBar();
            }
            else
            {
                UpdateGuildButton();
            }

            if (UpdateStatuses)
            {
                UpdateSpellStatus();
                UpdateStatuses = false;
            }

            foreach (KeyValuePair<Guid, SpellStatus> itm in mActiveStatuses)
            {
                itm.Value.Draw();
            }

        }

        internal void Dispose()
        {
            MyEntity = null;
            Hide();
        }

        private void SetupEntityElements()
        {
            buttonGuild.gameObject.SetActive(false);

            bool isOtherPlayer = entityType == EntityTypes.Player && Globals.Me != MyEntity;
            buttonTrade.gameObject.SetActive(isOtherPlayer);
            buttonParty.gameObject.SetActive(isOtherPlayer);
            buttonFriend.gameObject.SetActive(isOtherPlayer);

            bool isMe = Globals.Me == MyEntity;
            fillXP.gameObject.SetActive(isMe);
            labelXP.enabled = isMe;
            textXP.enabled = isMe;
            textMap.enabled = isMe;

            bool isEvent = entityType == EntityTypes.Event;
            fillMP.gameObject.SetActive(!isEvent);
            labelMP.enabled = !isEvent;
            textMP.enabled = !isEvent;
            fillHP.gameObject.SetActive(!isEvent);
            labelHP.enabled = !isEvent;
            textHP.enabled = !isEvent;
            textMap.enabled = !isEvent;

            textEventDesc.enabled = isEvent;
            if (isEvent)
            {
                textEventDesc.text = ((Entities.Events.Event)MyEntity).Desc;
            }

            textName.text = MyEntity.Name;
        }

        public void UpdateSpellStatus()
        {
            //Remove 'Dead' Statuses
            Guid[] statuses = mActiveStatuses.Keys.ToArray();
            foreach (Guid status in statuses)
            {
                if (!MyEntity.StatusActive(status))
                {
                    SpellStatus spellStatus = mActiveStatuses[status];
                    Destroy(spellStatus.gameObject);
                    mActiveStatuses.Remove(status);
                }
            }

            //Add all of the spell status effects
            for (int i = 0; i < MyEntity.Status.Count; i++)
            {
                Guid id = MyEntity.Status[i].SpellId;
                if (!mActiveStatuses.TryGetValue(id, out SpellStatus spellStatus))
                {
                    spellStatus = Instantiate(spellStatusPrefab, spellStatusContainer, false);
                    mActiveStatuses.Add(id, spellStatus);
                }

                spellStatus.UpdateStatus(MyEntity.Status[i], descTransform);
            }
        }

        private void UpdateLevel()
        {
            string levelString = Strings.EntityBox.level.ToString(MyEntity.Level);
            textName.text = Strings.EntityBox.NameAndLevel.ToString(MyEntity.Name, levelString);
        }

        private void UpdateMap()
        {
            if (Globals.Me.MapInstance != null)
            {
                textMap.text = Strings.EntityBox.map.ToString(Globals.Me.MapInstance.Name);
            }
            else
            {
                textMap.text = Strings.EntityBox.map.ToString(string.Empty);
            }
        }

        private void UpdateHpBar()
        {
            if (MyEntity.MaxVital[(int)Vitals.Health] > 0)
            {
                int maxVital = MyEntity.MaxVital[(int)Vitals.Health];
                int shieldSize = 0;

                //Check for shields
                foreach (Status status in MyEntity.Status)
                {
                    if (status.Type == StatusTypes.Shield)
                    {
                        shieldSize += status.Shield[(int)Vitals.Health];
                    }
                }

                if (shieldSize + MyEntity.Vital[(int)Vitals.Health] > maxVital)
                {
                    maxVital = shieldSize + MyEntity.Vital[(int)Vitals.Health];
                }

                float hpfillRatio = (float)MyEntity.Vital[(int)Vitals.Health] / maxVital;
                hpfillRatio = Math.Min(1, Math.Max(0, hpfillRatio));
                fillHP.ChangeValue(hpfillRatio);

                //float shieldfillRatio = (float)shieldSize / maxVital;
                //shieldfillRatio = Math.Min(1, Math.Max(0, shieldfillRatio));

                //Fix the Labels
                textHP.text = Strings.EntityBox.vital0val.ToString(MyEntity.Vital[(int)Vitals.Health], MyEntity.MaxVital[(int)Vitals.Health]);
            }
            else
            {
                textHP.text = Strings.EntityBox.vital0val.ToString(0, 0);
                fillHP.ChangeValue(0f);
            }
        }

        private void UpdateMpBar()
        {
            if (MyEntity.MaxVital[(int)Vitals.Mana] > 0)
            {
                float mpfillRatio = MyEntity.Vital[(int)Vitals.Mana] / (float)MyEntity.MaxVital[(int)Vitals.Mana];
                mpfillRatio = Math.Min(1, Math.Max(0, mpfillRatio));
                textMP.text = Strings.EntityBox.vital1val.ToString(MyEntity.Vital[(int)Vitals.Mana], MyEntity.MaxVital[(int)Vitals.Mana]);
                fillMP.ChangeValue(mpfillRatio);
            }
            else
            {
                textMP.text = Strings.EntityBox.vital1val.ToString(0, 0);
                fillMP.ChangeValue(0f);
            }
        }

        private void UpdateXpBar()
        {
            if (((Player)MyEntity).GetNextLevelExperience() > 0)
            {
                float targetExpFill = (float)((Player)MyEntity).Experience / ((Player)MyEntity).GetNextLevelExperience();
                textXP.text = Strings.EntityBox.expval.ToString(((Player)MyEntity)?.Experience, ((Player)MyEntity)?.GetNextLevelExperience());
                fillXP.ChangeValue(targetExpFill);
            }
            else
            {
                textXP.text = Strings.EntityBox.maxlevel;
                fillXP.ChangeValue(1f);
            }
        }

        private void UpdateGuildButton()
        {
            if (MyEntity is Player plyr && MyEntity != Globals.Me && string.IsNullOrWhiteSpace(plyr.Guild))
            {
                if (Globals.Me?.GuildRank?.Permissions?.Invite ?? false)
                {
                    buttonGuild.gameObject.SetActive(true);
                }
            }
        }

        private void OnClickTrade()
        {
            if (Globals.Me.TargetIndex != Guid.Empty && Globals.Me.TargetIndex != Globals.Me.Id)
            {
                PacketSender.SendTradeRequest(Globals.Me.TargetIndex);
            }
        }

        private void OnClickParty()
        {
            if (Globals.Me.TargetIndex != Guid.Empty && Globals.Me.TargetIndex != Globals.Me.Id)
            {
                PacketSender.SendPartyInvite(Globals.Me.TargetIndex);
            }
        }

        private void OnClickFriend()
        {
            if (Globals.Me.TargetIndex != Guid.Empty && Globals.Me.TargetIndex != Globals.Me.Id)
            {
                PacketSender.SendAddFriend(MyEntity.Name);
            }
        }

        private void OnClickGuild()
        {
            if (MyEntity is Player plyr && MyEntity != Globals.Me && string.IsNullOrWhiteSpace(plyr.Guild))
            {
                if (Globals.Me?.GuildRank?.Permissions?.Invite ?? false)
                {
                    if (Globals.Me.CombatTimer < Globals.System.GetTimeMs())
                    {
                        PacketSender.SendInviteGuild(MyEntity.Name);
                    }
                    else
                    {
                        PacketSender.SendChatMsg(Strings.Friends.infight.ToString(), 4);
                    }
                }
            }
        }
    }
}
