using Intersect.Client.Entities.Events;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UnityGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game
{

    public class EventWindow : Window
    {
        [SerializeField]
        private Button mEventResponse1 = default;
        [SerializeField]
        private TextMeshProUGUI mEventResponseText1 = default;
        [SerializeField]
        private Button mEventResponse2 = default;
        [SerializeField]
        private TextMeshProUGUI mEventResponseText2 = default;
        [SerializeField]
        private Button mEventResponse3 = default;
        [SerializeField]
        private TextMeshProUGUI mEventResponseText3 = default;
        [SerializeField]
        private Button mEventResponse4 = default;
        [SerializeField]
        private TextMeshProUGUI mEventResponseText4 = default;

        [SerializeField]
        private Image mEventFace = default;

        [SerializeField]
        private TextMeshProUGUI mEventDialogLabel = default;

        [SerializeField]
        private ScrollRect scrollViewText = default;

        private void Start()
        {
            mEventResponse1.onClick.AddListener(() => EventButtonClick(1));
            mEventResponse2.onClick.AddListener(() => EventButtonClick(2));
            mEventResponse3.onClick.AddListener(() => EventButtonClick(3));
            mEventResponse4.onClick.AddListener(() => EventButtonClick(4));
        }

        private void EventButtonClick(byte response)
        {
            Dialog dialog = Globals.EventDialogs[0];
            if (dialog.ResponseSent != 0)
            {
                return;
            }

            Hide();
            PacketSender.SendEventResponse(response, dialog);
            dialog.ResponseSent = 1;
        }

        internal void Draw()
        {

            if (Globals.EventDialogs.Count == 0)
            {
                return;
            }

            if (IsHidden)
            {
                Show();
                scrollViewText.verticalNormalizedPosition = 0;

                Dialog dialog = Globals.EventDialogs[0];
                GameTexture faceTex = Globals.ContentManager.GetTexture(
                    GameContentManager.TextureType.Face, dialog.Face
                );

                int responseCount = 0;
                if (dialog.Opt1.Length > 0)
                {
                    responseCount++;
                }

                if (dialog.Opt2.Length > 0)
                {
                    responseCount++;
                }

                if (dialog.Opt3.Length > 0)
                {
                    responseCount++;
                }

                if (dialog.Opt4.Length > 0)
                {
                    responseCount++;
                }

                if (faceTex != null)
                {
                    mEventFace.gameObject.SetActive(true);
                    mEventFace.sprite = faceTex.GetSpriteDefault();
                }
                else
                {
                    mEventFace.gameObject.SetActive(false);
                }

                if (responseCount == 0)
                {
                    mEventResponse1.gameObject.SetActive(true);
                    mEventResponseText1.text = Strings.EventWindow.Continue;
                    mEventResponse2.gameObject.SetActive(false);
                    mEventResponse3.gameObject.SetActive(false);
                    mEventResponse4.gameObject.SetActive(false);
                }
                else
                {
                    if (!string.IsNullOrEmpty(dialog.Opt1))
                    {
                        mEventResponse1.gameObject.SetActive(true);
                        mEventResponseText1.text = dialog.Opt1;
                    }
                    else
                    {
                        mEventResponse1.gameObject.SetActive(true);
                    }

                    if (!string.IsNullOrEmpty(dialog.Opt2))
                    {
                        mEventResponse2.gameObject.SetActive(true);
                        mEventResponseText2.text = dialog.Opt2;
                    }
                    else
                    {
                        mEventResponse2.gameObject.SetActive(false);
                    }

                    if (!string.IsNullOrEmpty(dialog.Opt3))
                    {
                        mEventResponse3.gameObject.SetActive(true);
                        mEventResponseText3.text = dialog.Opt3;
                    }
                    else
                    {
                        mEventResponse3.gameObject.SetActive(false);
                    }

                    if (!string.IsNullOrEmpty(dialog.Opt4))
                    {
                        mEventResponse4.gameObject.SetActive(true);
                        mEventResponseText4.text = dialog.Opt4;
                    }
                    else
                    {
                        mEventResponse4.gameObject.SetActive(false);
                    }
                }
                scrollViewText.gameObject.SetActive(!string.IsNullOrEmpty(dialog.Prompt));
                mEventDialogLabel.text = dialog.Prompt;
            }
        }
    }
}