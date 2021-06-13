using Intersect.Admin.Actions;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.General;
using Intersect.Client.Interface.Shared;
using Intersect.Client.Localization;
using Intersect.Client.Networking;
using Intersect.Client.UI.Components;
using Intersect.Client.UnityGame;
using Intersect.GameObjects.Maps.MapList;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game
{
    public class AdminWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private TextMeshProUGUI labelName;
        [SerializeField]
        private TMP_InputField inputName;
        [SerializeField]
        private Button buttonWarpMeTo;
        [SerializeField]
        private TextMeshProUGUI labelWarpMeTo;
        [SerializeField]
        private Button buttonWarpToMe;
        [SerializeField]
        private TextMeshProUGUI labelWarpToMe;
        [SerializeField]
        private Button buttonKick;
        [SerializeField]
        private TextMeshProUGUI labelKick;
        [SerializeField]
        private Button buttonBan;
        [SerializeField]
        private TextMeshProUGUI labelBan;
        [SerializeField]
        private Button buttonUnban;
        [SerializeField]
        private TextMeshProUGUI labelUnban;
        [SerializeField]
        private Button buttonKill;
        [SerializeField]
        private TextMeshProUGUI labelKill;
        [SerializeField]
        private Button buttonMute;
        [SerializeField]
        private TextMeshProUGUI labelMute;
        [SerializeField]
        private Button buttonUnmute;
        [SerializeField]
        private TextMeshProUGUI labelUnmute;
        [SerializeField]
        private TextMeshProUGUI labelSprite;
        [SerializeField]
        private TMP_Dropdown dropdownSprite;
        [SerializeField]
        private Image imageSprite;
        [SerializeField]
        private Button buttonSetSprite;
        [SerializeField]
        private TextMeshProUGUI labelSetSprite;
        [SerializeField]
        private TextMeshProUGUI labelFace;
        [SerializeField]
        private TMP_Dropdown dropdownFace;
        [SerializeField]
        private Image imageFace;
        [SerializeField]
        private Button buttonSetFace;
        [SerializeField]
        private TextMeshProUGUI labelSetFace;
        [SerializeField]
        private TextMeshProUGUI labelAccess;
        [SerializeField]
        private TMP_Dropdown dropdownAccess;
        [SerializeField]
        private Button buttonSetAccess;
        [SerializeField]
        private TextMeshProUGUI labelSetAccess;
        [SerializeField]
        private TextMeshProUGUI labelMapList;
        [SerializeField]
        private TextMeshProUGUI labelMapOrder;
        [SerializeField]
        private TreeNode treeNodePrefab;
        [SerializeField]
        private Transform mapTreeContainer;
        [SerializeField]
        private Toggle toggleChronologicalOrder;

        private readonly List<TreeNode> mapTree = new List<TreeNode>();

        private readonly List<string> accessValues = new List<string>(3)
        {
            "None",
            "Moderator",
            "Admin"
        };

        private void Start()
        {
            textTitle.text = Strings.Admin.title;
            buttonClose.onClick.AddListener(() => Hide());

            labelName.text = Strings.Admin.name;
            inputName.text = string.Empty;

            buttonWarpMeTo.onClick.AddListener(OnClickWarpMeTo);
            labelWarpMeTo.text = Strings.Admin.warpme2;

            buttonWarpToMe.onClick.AddListener(OnClickWarpToMe);
            labelWarpToMe.text = Strings.Admin.warp2me;

            buttonKick.onClick.AddListener(OnClickKick);
            labelKick.text = Strings.Admin.kick;

            buttonBan.onClick.AddListener(OnClickBan);
            labelBan.text = Strings.Admin.ban;

            buttonUnban.onClick.AddListener(OnClickUnban);
            labelUnban.text = Strings.Admin.unban;

            buttonKill.onClick.AddListener(OnClickKill);
            labelKill.text = Strings.Admin.kill;

            buttonMute.onClick.AddListener(OnClickMute);
            labelMute.text = Strings.Admin.mute;

            buttonUnmute.onClick.AddListener(OnClickUnmute);
            labelUnmute.text = Strings.Admin.unmute;

            labelSprite.text = Strings.Admin.sprite;

            string[] sprites = Globals.ContentManager.GetTextureNames(GameContentManager.TextureType.Entity);
            Array.Sort(sprites, new AlphanumComparatorFast());
            List<string> spriteList = new List<string>(sprites.Length + 1) {
                Strings.Admin.none
            };
            spriteList.AddRange(sprites);
            dropdownSprite.AddOptions(spriteList);
            dropdownSprite.onValueChanged.AddListener(ChangeSprite);
            imageSprite.enabled = false;
            buttonSetSprite.onClick.AddListener(OnClickSetSprite);
            labelSetSprite.text = Strings.Admin.setsprite;

            labelFace.text = Strings.Admin.face;

            string[] faces = Globals.ContentManager.GetTextureNames(GameContentManager.TextureType.Face);
            Array.Sort(faces, new AlphanumComparatorFast());
            List<string> faceList = new List<string>(faces.Length + 1) {
                Strings.Admin.none
            };
            faceList.AddRange(faces);
            dropdownFace.AddOptions(faceList);
            dropdownFace.onValueChanged.AddListener(ChangeFace);
            imageFace.enabled = false;
            buttonSetFace.onClick.AddListener(OnClickSetFace);
            labelSetFace.text = Strings.Admin.setface;

            labelAccess.text = Strings.Admin.access;
            List<string> access = new List<string>
            {
                Strings.Admin.access0,
                Strings.Admin.access1,
                Strings.Admin.access2
            };
            dropdownAccess.AddOptions(access);
            buttonSetAccess.onClick.AddListener(OnClickSetAccess);
            labelSetAccess.text = Strings.Admin.setpower;

            labelMapList.text = Strings.Admin.maplist;
            labelMapOrder.text = Strings.Admin.chronological;
            toggleChronologicalOrder.onValueChanged.AddListener(OnChangeToggleMapOrder);
        }

        public override void Show(object obj = null)
        {
            base.Show(obj);
            UpdateMapList();
        }

        internal void SetName(string name)
        {
            inputName.text = name;
        }

        private void UpdateMapList()
        {
            foreach (TreeNode node in mapTree)
            {
                node.Clear();
                Destroy(node.gameObject);
            }
            mapTree.Clear();

            AddMapListToTree(MapList.List, null);
        }

        private void AddMapListToTree(MapList mapList, TreeNode parent)
        {
            TreeNode tmpNode;
            if (toggleChronologicalOrder.isOn)
            {
                for (int i = 0; i < MapList.OrderedMaps.Count; i++)
                {
                    tmpNode = Instantiate(treeNodePrefab, mapTreeContainer, false);
                    mapTree.Add(tmpNode);
                    tmpNode.Set(MapList.OrderedMaps[i].Name, MapList.OrderedMaps[i].MapId, OnClickNode);
                }
            }
            else
            {
                for (int i = 0; i < mapList.Items.Count; i++)
                {
                    if (parent == null)
                    {
                        tmpNode = Instantiate(treeNodePrefab, mapTreeContainer, false);
                        mapTree.Add(tmpNode);
                    }
                    else
                    {
                        tmpNode = parent.AddNode(treeNodePrefab);
                    }

                    if (mapList.Items[i].GetType() == typeof(MapListFolder))
                    {
                        tmpNode.Set(mapList.Items[i].Name, null, null);
                        AddMapListToTree(((MapListFolder)mapList.Items[i]).Children, tmpNode);
                    }
                    else
                    {
                        tmpNode.Set(mapList.Items[i].Name, ((MapListMap)mapList.Items[i]).MapId, OnClickNode);
                    }
                }
            }
        }

        private void OnClickNode(object state)
        {
            PacketSender.SendAdminAction(new WarpToMapAction((Guid)state));
        }

        private void OnChangeToggleMapOrder(bool arg0)
        {
            UpdateMapList();
        }

        private bool HasName(bool checkIsSelf = false)
        {
            return !string.IsNullOrEmpty(inputName.text.Trim()) && (!checkIsSelf || !Globals.Me.Name.Equals(inputName.text, StringComparison.OrdinalIgnoreCase));
        }

        private void OnClickWarpToMe()
        {
            if (HasName())
            {
                PacketSender.SendAdminAction(new WarpMeToAction(inputName.text));
            }
        }


        private void OnClickWarpMeTo()
        {
            if (HasName())
            {
                PacketSender.SendAdminAction(new WarpToMeAction(inputName.text));
            }
        }

        private void OnClickKick()
        {
            if (HasName())
            {
                PacketSender.SendAdminAction(new KickAction(inputName.text));
            }
        }

        private void OnClickBan()
        {
            if (HasName(true))
            {
                Interface.BanMuteBox.Show(Strings.Admin.bancaption.ToString(inputName.text), Strings.Admin.banprompt.ToString(inputName.text), true, BanUser);
            }
        }

        private void BanUser(object sender, EventArgs e)
        {
            PacketSender.SendAdminAction(new BanAction(inputName.text, Interface.BanMuteBox.GetDuration(), Interface.BanMuteBox.GetReason(), Interface.BanMuteBox.BanIp()));
        }

        private void OnClickUnban()
        {
            if (HasName())
            {
                string name = inputName.text;
                Interface.InputBox.Show(Strings.Admin.unbancaption.ToString(name), Strings.Admin.unbanprompt.ToString(name), true, InputBox.InputType.YesNo,
                    (s, e) => PacketSender.SendAdminAction(new UnbanAction(name)), null, -1);
            }
        }

        private void OnClickKill()
        {
            if (HasName())
            {
                PacketSender.SendAdminAction(new KillAction(inputName.text));
            }
        }

        private void OnClickMute()
        {
            if (HasName())
            {
                Interface.BanMuteBox.Show(Strings.Admin.mutecaption.ToString(inputName.text), Strings.Admin.muteprompt.ToString(inputName.text), true, MuteUser);
            }
        }

        private void MuteUser(object sender, EventArgs e)
        {
            PacketSender.SendAdminAction(new MuteAction(inputName.text, Interface.BanMuteBox.GetDuration(), Interface.BanMuteBox.GetReason(), Interface.BanMuteBox.BanIp()));
        }

        private void OnClickUnmute()
        {
            if (HasName())
            {
                string name = inputName.text;
                Interface.InputBox.Show(Strings.Admin.unmutecaption.ToString(name), Strings.Admin.unmuteprompt.ToString(name), true, InputBox.InputType.YesNo,
                    (s, e) => PacketSender.SendAdminAction(new UnmuteAction(name)), null, -1);
            }
        }

        private void OnClickSetSprite()
        {
            int selected = dropdownSprite.value;
            if (HasName())
            {
                PacketSender.SendAdminAction(new SetSpriteAction(inputName.text, dropdownSprite.options[selected].text));
            }
        }

        private void OnClickSetFace()
        {
            int selected = dropdownFace.value;
            if (HasName())
            {
                PacketSender.SendAdminAction(new SetFaceAction(inputName.text, dropdownFace.options[selected].text));
            }
        }

        private void OnClickSetAccess()
        {
            if (HasName(true))
            {
                PacketSender.SendAdminAction(new SetAccessAction(inputName.text, accessValues[dropdownAccess.value]));
            }
        }

        private void ChangeFace(int index)
        {
            imageFace.enabled = index > 0;
            if (index == 0)
            {
                return;
            }
            imageFace.sprite = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Face, dropdownFace.options[index].text).GetSpriteDefault();
        }

        private void ChangeSprite(int index)
        {
            imageSprite.enabled = index > 0;
            if (index == 0)
            {
                return;
            }
            imageSprite.sprite = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Entity, dropdownSprite.options[index].text).GetSpriteDefault();
        }
    }
}
