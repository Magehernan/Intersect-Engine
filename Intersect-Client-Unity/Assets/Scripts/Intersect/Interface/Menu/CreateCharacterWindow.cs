using Intersect.Client.Framework.File_Management;
using Intersect.Client.General;
using Intersect.Client.Interface.Game.Chat;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UnityGame;
using Intersect.GameObjects;
using Intersect.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Menu
{
    public class CreateCharacterWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private TextMeshProUGUI textCharacter;
        [SerializeField]
        private TextMeshProUGUI textDescription;
        [SerializeField]
        private Image imageCharacterBody;
        [SerializeField]
        private Button buttonLeftSprite;
        [SerializeField]
        private Button buttonRightSprite;
        [SerializeField]
        private Button buttonCreate;
        [SerializeField]
        private TextMeshProUGUI textCreate;
        [SerializeField]
        private Button buttonBack;
        [SerializeField]
        private TextMeshProUGUI textBack;
        [SerializeField]
        private TextMeshProUGUI textName;
        [SerializeField]
        private TMP_InputField inputName;
        [SerializeField]
        private TextMeshProUGUI textClass;
        [SerializeField]
        private TMP_Dropdown dropdownClass;
        [SerializeField]
        private TextMeshProUGUI textGender;
        [SerializeField]
        private TextMeshProUGUI textMale;
        [SerializeField]
        private Toggle toggleMale;
        [SerializeField]
        private TextMeshProUGUI textFemale;
        [SerializeField]
        private Toggle toggleFemale;

        private readonly List<KeyValuePair<int, ClassSprite>> mMaleSprites = new List<KeyValuePair<int, ClassSprite>>();
        private readonly List<KeyValuePair<int, ClassSprite>> mFemaleSprites = new List<KeyValuePair<int, ClassSprite>>();
        private int mDisplaySpriteIndex = -1;

        private void Start()
        {
            textTitle.text = Strings.CharacterCreation.title;
            textName.text = Strings.CharacterCreation.name;
            textClass.text = Strings.CharacterCreation.Class;
            textCharacter.text = Strings.CharacterCreation.hint;
            textDescription.text = Strings.CharacterCreation.hint2;
            textGender.text = Strings.CharacterCreation.gender;
            textMale.text = Strings.CharacterCreation.male;
            textFemale.text = Strings.CharacterCreation.female;
            textCreate.text = Strings.CharacterCreation.create;
            textBack.text = Strings.CharacterCreation.back;

            dropdownClass.onValueChanged.AddListener(OnChangeClass);
            toggleMale.onValueChanged.AddListener(OnChangeGender);
            toggleFemale.onValueChanged.AddListener(OnChangeGender);

            inputName.onSubmit.AddListener(OnSubmit);

            buttonLeftSprite.onClick.AddListener(OnClickLeftSprite);
            buttonRightSprite.onClick.AddListener(OnClickRightSprite);
            buttonCreate.onClick.AddListener(OnClickCreate);
            buttonBack.onClick.AddListener(OnClickBack);
        }

        public override void Show(object obj = null)
        {
            base.Show(obj);
            toggleMale.isOn = true;
            toggleFemale.isOn = false;
            inputName.text = string.Empty;
        }

        internal void Init()
        {
            dropdownClass.ClearOptions();
            foreach (ClassBase cls in ClassBase.Lookup.Values)
            {
                if (!cls.Locked)
                {
                    dropdownClass.options.Add(new TMP_Dropdown.OptionData(cls.Name));
                }
            }

            dropdownClass.value = 0;
            dropdownClass.RefreshShownValue();
            LoadClass();
            UpdateDisplay();
        }

        internal void Draw()
        {
            if (!Networking.Network.Connected)
            {
                Hide();
                Interface.MenuUi.MainMenu.Show();
                Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.Errors.lostconnection));
                return;
            }

            // Re-Enable our buttons if we're not waiting for the server anymore with it disabled.
            if (!Globals.WaitingOnServer && !buttonCreate.interactable)
            {
                buttonCreate.interactable = true;
            }

        }

        private void UpdateDisplay()
        {
            ClassBase currentClass = GetClass();
            if (currentClass != null && mDisplaySpriteIndex != -1)
            {
                if (currentClass.Sprites.Count > 0)
                {
                    ClassSprite classSprite;
                    if (toggleMale.isOn)
                    {
                        classSprite = mMaleSprites[mDisplaySpriteIndex].Value;
                    }
                    else
                    {
                        classSprite = mFemaleSprites[mDisplaySpriteIndex].Value;
                    }

                    imageCharacterBody.sprite = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Face, classSprite.Face)?.GetSpriteDefault();

                    if (imageCharacterBody.sprite == null)
                    {
                        imageCharacterBody.sprite = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Entity, classSprite.Sprite)?.GetSpriteDefault();
                    }
                }
            }
            else
            {
                imageCharacterBody.enabled = false;
            }
        }

        private ClassBase GetClass()
        {
            foreach (KeyValuePair<Guid, Models.IDatabaseObject> cls in ClassBase.Lookup)
            {
                if (!((ClassBase)cls.Value).Locked && dropdownClass.captionText.text.Equals(cls.Value.Name))
                {
                    return (ClassBase)cls.Value;
                }
            }

            return null;
        }

        private void LoadClass()
        {
            ClassBase cls = GetClass();
            mMaleSprites.Clear();
            mFemaleSprites.Clear();
            mDisplaySpriteIndex = -1;

            if (cls != null)
            {
                for (int i = 0; i < cls.Sprites.Count; i++)
                {
                    if (cls.Sprites[i].Gender == 0)
                    {
                        mMaleSprites.Add(new KeyValuePair<int, ClassSprite>(i, cls.Sprites[i]));
                    }
                    else
                    {
                        mFemaleSprites.Add(new KeyValuePair<int, ClassSprite>(i, cls.Sprites[i]));
                    }
                }
            }

            ResetSprite();
        }

        private void ResetSprite()
        {
            buttonLeftSprite.gameObject.SetActive(false);
            buttonRightSprite.gameObject.SetActive(false);

            List<KeyValuePair<int, ClassSprite>> sprites;

            if (toggleMale.isOn)
            {
                sprites = mMaleSprites;
            }
            else
            {
                sprites = mFemaleSprites;
            }

            // Sprite
            if (sprites.Count > 0)
            {
                mDisplaySpriteIndex = 0;
                if (sprites.Count > 1)
                {
                    buttonLeftSprite.gameObject.SetActive(true);
                    buttonRightSprite.gameObject.SetActive(true);
                }
            }
            else
            {
                mDisplaySpriteIndex = -1;
            }
        }

        private void OnClickBack()
        {
            Hide();
            if (Options.Player.MaxCharacters <= 1)
            {
                //Logout
                Interface.MenuUi.MainMenu.Show();
            }
            else
            {
                //Character Selection Screen
                Interface.MenuUi.MainMenu.ShowCharacterSelection();
            }
        }

        private void OnClickCreate()
        {
            TryCreateCharacter();
        }

        private void OnSubmit(string arg0)
        {
            TryCreateCharacter();
        }

        private void OnClickLeftSprite()
        {
            mDisplaySpriteIndex--;
            int count = toggleMale.isOn ? mMaleSprites.Count : mFemaleSprites.Count;
            if (count > 0)
            {
                if (mDisplaySpriteIndex == -1)
                {
                    mDisplaySpriteIndex = count - 1;
                }
            }
            else
            {
                mDisplaySpriteIndex = -1;
            }

            UpdateDisplay();
        }

        private void OnClickRightSprite()
        {
            mDisplaySpriteIndex++;
            int count = toggleMale.isOn ? mMaleSprites.Count : mFemaleSprites.Count;
            if (count > 0)
            {
                if (mDisplaySpriteIndex >= count)
                {
                    mDisplaySpriteIndex = 0;
                }
            }
            else
            {
                mDisplaySpriteIndex = -1;
            }

            UpdateDisplay();
        }

        private void OnChangeGender(bool arg0)
        {
            ResetSprite();
            UpdateDisplay();
        }

        private void OnChangeClass(int arg0)
        {
            LoadClass();
            UpdateDisplay();
        }

        private void TryCreateCharacter()
        {
            if (Globals.WaitingOnServer || mDisplaySpriteIndex == -1)
            {
                return;
            }

            if (!FieldChecking.IsValidUsername(inputName.text, Strings.Regex.username))
            {
                Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.CharacterCreation.invalidname));
                return;
            }

            int sprite;
            if (toggleMale.isOn)
            {
                sprite = mMaleSprites[mDisplaySpriteIndex].Key;
            }
            else
            {
                sprite = mFemaleSprites[mDisplaySpriteIndex].Key;
            }

            // Add our custom layers to the packet.
            PacketSender.SendCreateCharacter(inputName.text, GetClass().Id, sprite);

            Globals.WaitingOnServer = true;
            buttonCreate.enabled = false;
            ChatboxMsg.ClearMessages();
        }
    }
}
