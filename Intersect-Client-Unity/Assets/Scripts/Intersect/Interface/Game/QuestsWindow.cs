using Intersect.Client.General;
using Intersect.Client.Interface.Game.Displayers;
using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
using Intersect.GameObjects;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game
{
    public class QuestsWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private GameObject gameobjectClose;
        [SerializeField]
        private QuestDisplayer questDisplayerPrefab;
        [SerializeField]
        private Transform questContainer;
        [SerializeField]
        private GameObject listGameObject;
        [SerializeField]
        private GameObject detailGameObject;
        [SerializeField]
        private QuestDetailDisplayer questDetailDisplayer;


        private QuestBase mSelectedQuest;

        private readonly List<QuestDisplayer> questList = new List<QuestDisplayer>();

        private void Start()
        {
            textTitle.text = Strings.QuestLog.title;
            buttonClose.onClick.AddListener(() => Hide());
        }


        internal void Draw(bool shouldUpdateList)
        {
            if (shouldUpdateList)
            {
                UpdateQuestList();
                UpdateSelectedQuest();
            }

            if (!IsVisible)
            {
                return;
            }

            if (mSelectedQuest != null)
            {
                if (Globals.Me.QuestProgress.ContainsKey(mSelectedQuest.Id))
                {
                    if (Globals.Me.QuestProgress[mSelectedQuest.Id].Completed &&
                        Globals.Me.QuestProgress[mSelectedQuest.Id].TaskId == Guid.Empty)
                    {
                        //Completed
                        if (!mSelectedQuest.LogAfterComplete)
                        {
                            mSelectedQuest = null;
                            UpdateSelectedQuest();
                        }

                        return;
                    }
                    else
                    {
                        if (Globals.Me.QuestProgress[mSelectedQuest.Id].TaskId == Guid.Empty)
                        {
                            //Not Started
                            if (!mSelectedQuest.LogBeforeOffer)
                            {
                                mSelectedQuest = null;
                                UpdateSelectedQuest();
                            }
                        }

                        return;
                    }
                }

                if (!mSelectedQuest.LogBeforeOffer)
                {
                    mSelectedQuest = null;
                    UpdateSelectedQuest();
                }
            }
        }

        private void UpdateQuestList()
        {
            foreach (QuestDisplayer displayer in questList)
            {
                displayer.Destroy();
            }
            questList.Clear();

            if (Globals.Me != null)
            {
                ICollection<Models.IDatabaseObject> quests = QuestBase.Lookup.Values;
                foreach (QuestBase quest in quests)
                {
                    if (quest != null)
                    {
                        if (Globals.Me.QuestProgress.ContainsKey(quest.Id))
                        {
                            if (Globals.Me.QuestProgress[quest.Id].TaskId != Guid.Empty)
                            {
                                AddQuestToList(quest.Name, Color.Yellow, quest.Id);
                            }
                            else
                            {
                                if (Globals.Me.QuestProgress[quest.Id].Completed)
                                {
                                    if (quest.LogAfterComplete)
                                    {
                                        AddQuestToList(quest.Name, Color.Green, quest.Id);
                                    }
                                }
                                else
                                {
                                    if (quest.LogBeforeOffer)
                                    {
                                        AddQuestToList(quest.Name, Color.Red, quest.Id);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (quest.LogBeforeOffer)
                            {
                                AddQuestToList(quest.Name, Color.Red, quest.Id);
                            }
                        }
                    }
                }
            }
        }

        private void AddQuestToList(string name, Color color, Guid questId)
        {
            QuestDisplayer questDisplayer = Instantiate(questDisplayerPrefab, questContainer, false);
            questList.Add(questDisplayer);
            questDisplayer.UpdateQuest(name, questId, color, QuestDisplayerClicked);
        }

        private void UpdateSelectedQuest()
        {
            if (mSelectedQuest == null)
            {
                detailGameObject.SetActive(false);
                listGameObject.SetActive(true);
                return;
            }
            detailGameObject.SetActive(true);
            listGameObject.SetActive(false);

            questDetailDisplayer.UpdateQuest(mSelectedQuest, OnBack);
        }


        private void QuestDisplayerClicked(Guid questId)
        {
            QuestBase quest = QuestBase.Get(questId);
            if (quest != null)
            {
                mSelectedQuest = quest;
                UpdateSelectedQuest();
            }
        }

        private void OnBack()
        {
            mSelectedQuest = null;
            UpdateSelectedQuest();
        }
    }
}
