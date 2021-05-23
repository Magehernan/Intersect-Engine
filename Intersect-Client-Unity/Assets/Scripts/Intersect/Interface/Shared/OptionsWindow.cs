using Intersect.Client.Core;
using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Shared
{
    public class OptionsWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private TextMeshProUGUI textResolution;
        [SerializeField]
        private TMP_Dropdown dropdownResolution;
        [SerializeField]
        private TextMeshProUGUI textFPS;
        [SerializeField]
        private TMP_Dropdown dropdownFPS;
        [SerializeField]
        private TextMeshProUGUI textSound;
        [SerializeField]
        private Slider sliderSound;
        [SerializeField]
        private TextMeshProUGUI textMusic;
        [SerializeField]
        private Slider sliderMusic;
        [SerializeField]
        private TextMeshProUGUI textFullscreen;
        [SerializeField]
        private Toggle toggleFullscreen;
        [SerializeField]
        private TextMeshProUGUI textEditControls;
        [SerializeField]
        private Button buttonEditControls;
        [SerializeField]
        private TextMeshProUGUI textApply;
        [SerializeField]
        private Button buttonApply;
        [SerializeField]
        private TextMeshProUGUI textCancel;
        [SerializeField]
        private Button buttonCancel;
        [SerializeField]
        private ControlsWindow controlWindow;

        private long mListeningTimer;

        private int mPreviousMusicVolume;
        private int mPreviousSoundVolume;

        protected override void Awake()
        {
            base.Awake();

            textTitle.text = Strings.Options.title;
            textResolution.text = Strings.Options.resolution;
            textFPS.text = Strings.Options.targetfps;
            textFullscreen.text = Strings.Options.fullscreen;
            textEditControls.text = Strings.Controls.edit;
            textApply.text = Strings.Options.apply;
            textCancel.text = Strings.Options.cancel;
            buttonApply.onClick.AddListener(OnClickApply);
            buttonCancel.onClick.AddListener(OnClickCancel);
            buttonEditControls.onClick.AddListener(OnClickEditControls);
            sliderSound.onValueChanged.AddListener(OnChangeSound);
            sliderMusic.onValueChanged.AddListener(OnChangeMusic);

            List<string> resolutions = Core.Graphics.Renderer.GetValidVideoModes();
            foreach (string resolution in resolutions)
            {
                dropdownResolution.options.Add(new TMP_Dropdown.OptionData(resolution));
            }

            dropdownFPS.options.Add(new TMP_Dropdown.OptionData(Strings.Options.vsync));
            dropdownFPS.options.Add(new TMP_Dropdown.OptionData(Strings.Options.fps30));
            dropdownFPS.options.Add(new TMP_Dropdown.OptionData(Strings.Options.fps60));
            dropdownFPS.options.Add(new TMP_Dropdown.OptionData(Strings.Options.fps90));
            dropdownFPS.options.Add(new TMP_Dropdown.OptionData(Strings.Options.fps120));
            dropdownFPS.options.Add(new TMP_Dropdown.OptionData(Strings.Options.unlimitedfps));
        }

        private void OnClickEditControls()
        {
            controlWindow.Show();
        }

        public override void Show(object obj = null)
        {
            base.Show(obj);
            mPreviousMusicVolume = Globals.Database.MusicVolume;
            mPreviousSoundVolume = Globals.Database.SoundVolume;

            List<string> resolutions = Core.Graphics.Renderer.GetValidVideoModes();
            if (resolutions.Count > 0)
            {
                dropdownResolution.value = resolutions.IndexOf($"{Core.Graphics.Renderer.GetScreenWidth()},{Core.Graphics.Renderer.GetScreenHeight()}");
            }
            dropdownResolution.RefreshShownValue();

            if (Globals.Database.TargetFps == -1)
            {
                dropdownFPS.value = dropdownFPS.options.Count - 1;
            }
            else
            {
                dropdownFPS.value = Globals.Database.TargetFps;
            }
            dropdownFPS.RefreshShownValue();


            toggleFullscreen.isOn = Globals.Database.FullScreen;
            sliderMusic.value = Globals.Database.MusicVolume;
            sliderSound.value = Globals.Database.SoundVolume;
            UpdateMusicText();
            UpdateSoundText();
        }

        public override void Hide(object obj = null)
        {
            if (IsHidden)
            {
                return;
            }

            if (Globals.GameState == GameStates.Menu)
            {
                Interface.MenuUi.MainMenu.Show();
            }

            base.Hide(obj);
        }

        private void UpdateMusicText()
        {
            textMusic.text = Strings.Options.musicvolume.ToString((int)sliderMusic.value);
        }

        private void UpdateSoundText()
        {
            textSound.text = Strings.Options.soundvolume.ToString((int)sliderSound.value);
        }

        private void OnChangeMusic(float arg0)
        {
            UpdateMusicText();
            Globals.Database.MusicVolume = (int)sliderMusic.value;
            Audio.UpdateGlobalVolume();
        }

        private void OnChangeSound(float arg0)
        {
            UpdateSoundText();
            Globals.Database.SoundVolume = (int)sliderSound.value;
            Audio.UpdateGlobalVolume();
        }

        private void OnClickCancel()
        {
            Globals.Database.MusicVolume = mPreviousMusicVolume;
            Globals.Database.SoundVolume = mPreviousSoundVolume;
            Audio.UpdateGlobalVolume();

            Hide();
        }

        private void OnClickApply()
        {
            bool shouldReset = false;
            string resolution = dropdownResolution.captionText.text;
            List<string> validVideoModes = Core.Graphics.Renderer.GetValidVideoModes();

            int targetResolution = validVideoModes?.FindIndex(videoMode => string.Equals(videoMode, resolution)) ?? -1;
            if (targetResolution > -1)
            {
                shouldReset = Globals.Database.TargetResolution != targetResolution || Core.Graphics.Renderer.HasOverrideResolution;
                Globals.Database.TargetResolution = targetResolution;
            }

            if (Globals.Database.FullScreen != toggleFullscreen.isOn)
            {
                Globals.Database.FullScreen = toggleFullscreen.isOn;
                shouldReset = true;
            }

            int newFps = 0;
            if (dropdownFPS.value == dropdownFPS.options.Count - 1)
            {
                newFps = -1;
            }
            else
            {
                newFps = dropdownFPS.value;
            }

            if (newFps != Globals.Database.TargetFps)
            {
                shouldReset = true;
                Globals.Database.TargetFps = newFps;
            }

            Globals.Database.SavePreferences();
            if (shouldReset)
            {
                Core.Graphics.Renderer.OverrideResolution = Framework.Graphics.Resolution.Empty;
                Core.Graphics.Renderer.Init();
            }

            Hide();
        }
    }
}
