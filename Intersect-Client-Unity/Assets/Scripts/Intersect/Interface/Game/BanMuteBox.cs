using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game
{
    public class BanMuteBox : Window
    {
        [SerializeField]
        private Image imageModal;
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private TextMeshProUGUI textPrompt;
        [SerializeField]
        private TextMeshProUGUI labelReason;
        [SerializeField]
        private TMP_InputField inputReason;
        [SerializeField]
        private TextMeshProUGUI labelDuration;
        [SerializeField]
        private TMP_Dropdown dropdownDuration;
        [SerializeField]
        private TextMeshProUGUI labelIpBan;
        [SerializeField]
        private Toggle ipBan;
        [SerializeField]
        private Button buttonOk;
        [SerializeField]
        private TextMeshProUGUI textButtonOk;
        [SerializeField]
        private Button buttonCancel;
        [SerializeField]
        private TextMeshProUGUI textButtonCancel;

        private event EventHandler OkayEventHandler;

        private readonly List<string> durationOptions = new List<string>
        {
            Strings.BanMute.oneday,
            Strings.BanMute.twodays,
            Strings.BanMute.threedays,
            Strings.BanMute.fourdays,
            Strings.BanMute.fivedays,
            Strings.BanMute.oneweek,
            Strings.BanMute.twoweeks,
            Strings.BanMute.onemonth,
            Strings.BanMute.twomonths,
            Strings.BanMute.sixmonths,
            Strings.BanMute.oneyear,
            Strings.BanMute.forever,
        };

        private readonly List<int> durationValues = new List<int>
        {
            1,
            2,
            3,
            4,
            5,
            7,
            14,
            30,
            60,
            180,
            365,
            999999,
        };

        private void Start()
        {
            buttonOk.onClick.AddListener(OnClickOk);
            buttonCancel.onClick.AddListener(OnClickCancel);
            dropdownDuration.AddOptions(durationOptions);
            labelReason.text = Strings.BanMute.reason;
            labelDuration.text = Strings.BanMute.duration;
            labelIpBan.text = Strings.BanMute.ip;
            textButtonOk.text = Strings.BanMute.ok;
            textButtonCancel.text = Strings.BanMute.cancel;
        }


        public void Show(string title, string prompt, bool modal, EventHandler okayClicked)
        {
            imageModal.raycastTarget = modal;
            textTitle.text = title;
            textPrompt.text = prompt;
            inputReason.text = string.Empty;
            dropdownDuration.value = 0;
            dropdownDuration.RefreshShownValue();
            ipBan.isOn = false;
            OkayEventHandler = okayClicked;

            Show();
        }

        private void OnClickCancel()
        {
            Hide();
        }

        private void OnClickOk()
        {

            OkayEventHandler?.Invoke(this, EventArgs.Empty);
            Hide();
        }

        public int GetDuration() //days by default
        {
            return durationValues[dropdownDuration.value];
        }

        public string GetReason()
        {
            return inputReason.text;
        }

        public bool BanIp()
        {
            return ipBan.isOn;
        }
    }
}
