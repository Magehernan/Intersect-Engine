using Intersect.Client.Entities;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.GameObjects;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Game.Displayers
{
    public class EntityDisplayer : MonoBehaviour
    {
        private const string PLAYER = "Player";
        [SerializeField]
        private Image imagePaperdollPrefab;


        private Entity entity;
        private string[] paperdolls;
        private readonly List<Image> images = new List<Image>();


        public void Set(Entity entity)
        {
            this.entity = entity;

            foreach (Image image in images)
            {
                if (image != null)
                {
                    image.enabled = false;
                }
            }
            paperdolls = new string[Options.PaperdollOrder[1].Count];
            for (int i = 0; i < paperdolls.Length; i++)
            {
                paperdolls[i] = string.Empty;
                if (images.Count <= i)
                {
                    images.Add(null);
                }
            }
        }

        public void UpdateImage()
        {
            if (entity == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(entity.Face))
            {

                if (paperdolls[0].Equals(entity.Face))
                {
                    return;
                }
                GameTexture faceTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Face, entity.Face);
                if (faceTex != null)
                {
                    Image imageFace = images[0];
                    if (imageFace == null)
                    {
                        imageFace = Instantiate(imagePaperdollPrefab, transform, false);
                        images[0] = imageFace;
                        imageFace.transform.SetSiblingIndex(0);
                    }
                    paperdolls[0] = entity.Face;
                    imageFace.sprite = faceTex.GetSpriteDefault();
                    foreach (Image image in images)
                    {
                        if (image != null)
                        {
                            image.enabled = false;
                        }
                    }
                    imageFace.enabled = true;
                    return;
                }
            }

            Guid[] equipment = entity.Equipment;

            List<string> paperdollOrder = Options.PaperdollOrder[1];
            for (int z = 0; z < paperdollOrder.Count; z++)
            {
                string paperdoll = string.Empty;
                GameContentManager.TextureType type = GameContentManager.TextureType.Paperdoll;
                //nos fijamos si hay que dibujar o no el paperdoll
                int index = Options.EquipmentSlots.IndexOf(paperdollOrder[z]);

                if (paperdollOrder[z] == PLAYER)
                {
                    type = GameContentManager.TextureType.Entity;
                    paperdoll = entity.MySprite;
                }
                else if (index > -1 && index < equipment.Length && equipment[index] != Guid.Empty)
                {
                    ItemBase itemData = ItemBase.Get(equipment[index]);
                    if (itemData != null)
                    {
                        if (entity.Gender == 0)
                        {
                            paperdoll = itemData.MalePaperdoll;
                        }
                        else
                        {
                            paperdoll = itemData.FemalePaperdoll;
                        }
                    }
                }

                if (paperdoll == paperdolls[z])
                {
                    continue;
                }

                Image image = images[z];
                GameTexture paperdollTex = null;
                //cargamos la imagen
                if (!string.IsNullOrEmpty(paperdoll))
                {
                    paperdollTex = Globals.ContentManager.GetTexture(type, paperdoll);
                }

                //si tenemos texturaa
                if (paperdollTex != null)
                {
                    if (image == null)
                    {
                        image = Instantiate(imagePaperdollPrefab, transform, false);
                        images[z] = image;
                        image.transform.SetSiblingIndex(z);
                    }

                    image.sprite = paperdollTex.GetSpriteDefault();
                }

                if (image != null)
                {
                    image.enabled = !string.IsNullOrEmpty(paperdoll);
                }

                paperdolls[z] = paperdoll;
            }
        }
    }
}
