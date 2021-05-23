using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.Client.Interface.Game;
using Intersect.Client.Interface.Game.Chat;
using Intersect.Client.Interface.Shared;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UnityGame;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Menu
{

    public class SelectCharacterWindow : Window
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
        private Image imageCharacterHair;
        [SerializeField]
        private Image imageCharacterGuild;
        [SerializeField]
        private Button buttonLeft;
        [SerializeField]
        private Button buttonRight;
        [SerializeField]
        private Button buttonUse;
        [SerializeField]
        private TextMeshProUGUI textUse;
        [SerializeField]
        private Button buttonNew;
        [SerializeField]
        private TextMeshProUGUI textNew;
        [SerializeField]
        private Button buttonDelete;
        [SerializeField]
        private TextMeshProUGUI textDelete;
        [SerializeField]
        private Button buttonLogout;
        [SerializeField]
        private TextMeshProUGUI textLogout;

        private int mSelectedChar;

        private List<CharacterSlot> slots;


        private List<Character> characters;
        public List<Character> Characters
        {
            set
            {
                characters = value;
                slots = new List<CharacterSlot>(characters.Count);
                foreach (Character character in characters)
                {
                    slots.Add(new CharacterSlot(character));
                }

                mSelectedChar = 0;
                UpdateDisplay();
            }
        }

        //protected override MessageTypes ShowMessage => MessageTypes.CharactersPacket;

        private class CharacterSlot
        {
            private Guid id;
            private string name;
            private string description = string.Empty;
            private Sprite spriteBody;
            private Sprite spriteFace;
            private Sprite spriteGuild;
            public CharacterSlot(Character character)
            {
                if (character is null)
                {
                    name = Strings.CharacterSelection.empty;
                }
                else
                {
                    id = character.Id;
                    name = Strings.CharacterSelection.name.ToString(character.Name);
                    description = Strings.CharacterSelection.info.ToString(character.Level, character.Class);


                    GameTexture textureBody = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Entity, character.Sprite);
                    if (textureBody != null)
                    {
                        spriteBody = textureBody.GetSprite(0, 0);
                    }

                    if (character.Equipment.Length >= 8)
                    {
                        GameTexture texturePaperdoll = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Paperdoll, character.Equipment[7]);
                        if (texturePaperdoll != null)
                        {
                            spriteGuild = texturePaperdoll.GetSprite(0, 0);
                        }
                    }
                }
            }

            public void Show(
                TextMeshProUGUI textCharacter,
                TextMeshProUGUI textDescription,
                Image imageCharacterBody,
                Image imageCharacterFace,
                Image imageCharacterGuild,
                Button buttonUse,
                Button buttonNew,
                Button buttonDelete)
            {

                textCharacter.text = name;
                if (Guid.Empty.Equals(id))
                {
                    textDescription.gameObject.SetActive(false);
                    buttonUse.gameObject.SetActive(false);
                    buttonDelete.gameObject.SetActive(false);
                    imageCharacterBody.gameObject.SetActive(false);
                    imageCharacterFace.gameObject.SetActive(false);
                    imageCharacterGuild.gameObject.SetActive(false);
                    buttonNew.gameObject.SetActive(true);
                }
                else
                {
                    textDescription.text = description;
                    imageCharacterBody.sprite = spriteBody;
                    imageCharacterFace.sprite = spriteFace;
                    imageCharacterGuild.sprite = spriteGuild;

                    textDescription.gameObject.SetActive(true);
                    buttonUse.gameObject.SetActive(true);
                    //buttonUse.interactable = true;
                    buttonDelete.gameObject.SetActive(true);
                    imageCharacterBody.gameObject.SetActive(spriteBody != null);
                    imageCharacterFace.gameObject.SetActive(spriteFace != null);
                    imageCharacterGuild.gameObject.SetActive(spriteGuild != null);
                    buttonNew.gameObject.SetActive(false);
                }
            }
        }


        protected override void Awake()
        {
            base.Awake();
            textTitle.text = Strings.CharacterSelection.title;
            textUse.text = Strings.CharacterSelection.play;
            textDelete.text = Strings.CharacterSelection.delete;
            textNew.text = Strings.CharacterSelection.New;
            textLogout.text = Strings.CharacterSelection.logout;

            buttonLeft.onClick.AddListener(() => ChangeCharacter(-1));
            buttonRight.onClick.AddListener(() => ChangeCharacter(1));

            buttonUse.onClick.AddListener(ClickUse);
            buttonNew.onClick.AddListener(ClickNew);
            buttonDelete.onClick.AddListener(ClickDelete);
            buttonLogout.onClick.AddListener(Logout);
        }

        internal void Draw()
        {
            if (!Networking.Network.Connected)
            {
                Hide();
                Interface.MenuUi.MainMenu.Show();
                Interface.MsgboxErrors.Add(new KeyValuePair<string, string>(null, Strings.Errors.lostconnection));
            }

            // Re-Enable our buttons if we're not waiting for the server anymore with it disabled.
            if (!Globals.WaitingOnServer)
            {
                if (!buttonUse.interactable)
                {
                    buttonUse.interactable = true;
                }

                if (!buttonNew.interactable)
                {
                    buttonNew.interactable = true;
                }

                if (!buttonDelete.interactable)
                {
                    buttonDelete.interactable = true;
                }

                if (!buttonLogout.interactable)
                {
                    buttonLogout.interactable = true;
                }
            }
        }

        private void Logout()
        {
            Interface.MenuUi.ResetInterface();
        }

        private void ChangeCharacter(int direction)
        {
            mSelectedChar += direction;

            if (mSelectedChar < 0)
            {
                mSelectedChar = slots.Count - 1;
            }
            else if (mSelectedChar >= slots.Count)
            {
                mSelectedChar = 0;
            }

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            //Show and hide Options based on the character count
            if (slots.Count > 1)
            {
                buttonLeft.gameObject.SetActive(true);
                buttonRight.gameObject.SetActive(true);
            }

            if (slots.Count <= 1)
            {
                buttonLeft.gameObject.SetActive(false);
                buttonRight.gameObject.SetActive(false);
            }

            slots[mSelectedChar].Show(textCharacter, textDescription, imageCharacterBody, imageCharacterHair, imageCharacterGuild, buttonUse, buttonNew, buttonDelete);
        }

        private void ClickUse()
        {
            if (Globals.WaitingOnServer)
            {
                return;
            }
            
            ChatboxMsg.ClearMessages();
            PacketSender.SendSelectCharacter(characters[mSelectedChar].Id);

            Globals.WaitingOnServer = true;
            buttonUse.interactable = false;
            buttonNew.interactable = false;
            buttonDelete.interactable = false;
            buttonLogout.interactable = false;
        }

        private void ClickDelete()
        {
            if (Globals.WaitingOnServer)
            {
                return;
            }
            
            Interface.InputBox.Show(
                    Strings.CharacterSelection.deletetitle.ToString(characters[mSelectedChar].Name),
                    Strings.CharacterSelection.deleteprompt.ToString(characters[mSelectedChar].Name),
                    true,
                    InputBox.InputType.YesNo,
                    DeleteCharacter,
                    null,
                    characters[mSelectedChar].Id);
        }

        private void DeleteCharacter(object sender, EventArgs e)
        {
            PacketSender.SendDeleteCharacter((Guid)((InputBox)sender).UserData);
            
            Globals.WaitingOnServer = true;
            buttonUse.interactable = false;
            buttonNew.interactable = false;
            buttonDelete.interactable = false;
            buttonLogout.interactable = false;

            mSelectedChar = 0;
            UpdateDisplay();
        }

        private void ClickNew()
        {
            if (Globals.WaitingOnServer)
            {
                return;
            }
            
            PacketSender.SendNewCharacter();

            Globals.WaitingOnServer = true;
            buttonUse.interactable = false;
            buttonNew.interactable = false;
            buttonDelete.interactable = false;
            buttonLogout.interactable = false;
        }
    }

}
