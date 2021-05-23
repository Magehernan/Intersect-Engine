using Intersect.Client.General;
using Intersect.Client.Interface.Shared;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.Utils;
using Intersect.Enums;
using Intersect.GameObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Displayers
{
    public class QuestDetailDisplayer : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textName;
        [SerializeField]
        private TextMeshProUGUI textStatus;
        [SerializeField]
        private TextMeshProUGUI textDescription;
        [SerializeField]
        private Button buttonBack;
        [SerializeField]
        private TextMeshProUGUI labelBack;
        [SerializeField]
        private Button buttonAbandon;
        [SerializeField]
        private TextMeshProUGUI labelAbandon;

        private QuestBase quest;

        private Action onBack;

        private void Awake()
        {
            buttonBack.onClick.AddListener(OnClickBack);
            buttonAbandon.onClick.AddListener(OnClickAbandon);

            labelBack.text = Strings.QuestLog.back;
            labelAbandon.text = Strings.QuestLog.abandon;
        }

        internal void UpdateQuest(QuestBase quest, Action onBack)
        {
            this.onBack = onBack;
            this.quest = quest;
            textName.text = quest.Name;

            if (Globals.Me.QuestProgress.TryGetValue(quest.Id, out QuestProgress questProgress))
            {
                if (questProgress.TaskId != Guid.Empty)
                {
                    //In Progress
                    textStatus.text = Strings.QuestLog.inprogress;
                    textStatus.color = Color.Yellow.ToColor32();
                    string desc = string.Empty;
                    if (quest.InProgressDescription.Length > 0)
                    {
                        desc = $"{quest.InProgressDescription}\n\n";
                    }

                    desc = $"{desc}{Strings.QuestLog.currenttask}\n";

                    for (int i = 0; i < quest.Tasks.Count; i++)
                    {
                        if (quest.Tasks[i].Id == Globals.Me.QuestProgress[quest.Id].TaskId)
                        {
                            if (quest.Tasks[i].Description.Length > 0)
                            {
                                desc = $"{desc}{quest.Tasks[i].Description}\n\n";
                            }

                            if (quest.Tasks[i].Objective == QuestObjective.GatherItems) //Gather Items
                            {
                                desc = $"{desc}{Strings.QuestLog.taskitem.ToString(Globals.Me.QuestProgress[quest.Id].TaskProgress, quest.Tasks[i].Quantity, ItemBase.GetName(quest.Tasks[i].TargetId))}";
                            }
                            else if (quest.Tasks[i].Objective == QuestObjective.KillNpcs) //Kill Npcs
                            {
                                desc = $"{desc}{Strings.QuestLog.tasknpc.ToString(Globals.Me.QuestProgress[quest.Id].TaskProgress, quest.Tasks[i].Quantity, NpcBase.GetName(quest.Tasks[i].TargetId))}";
                            }
                        }

                        buttonAbandon.interactable = quest.Quitable;
                    }

                    textDescription.text = desc;
                    return;
                }

                if (Globals.Me.QuestProgress[quest.Id].Completed)
                {
                    //Completed
                    if (quest.LogAfterComplete)
                    {
                        textStatus.text = Strings.QuestLog.completed;
                        textStatus.color = Color.Green.ToColor32();
                        textDescription.text = quest.EndDescription;
                    }
                    return;
                }
            }

            //Not Started
            if (quest.LogBeforeOffer)
            {
                textStatus.text = Strings.QuestLog.notstarted;
                textStatus.color = Color.Red.ToColor32();
                textDescription.text = quest.BeforeDescription;
            }
        }

        private void OnClickAbandon()
        {
            Interface.InputBox.Show(Strings.QuestLog.abandontitle.ToString(quest.Name), Strings.QuestLog.abandonprompt.ToString(quest.Name), true, InputBox.InputType.YesNo, AbandonQuest, null, null);
        }

        private void AbandonQuest(object sender, EventArgs e)
        {
            PacketSender.SendAbandonQuest(quest.Id);
        }

        private void OnClickBack()
        {
            onBack?.Invoke();
        }
    }
}