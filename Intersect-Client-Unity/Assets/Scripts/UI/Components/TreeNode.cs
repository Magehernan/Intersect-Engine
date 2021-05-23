using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.UI.Components
{
    public class TreeNode : MonoBehaviour
    {
        [SerializeField]
        private Button button;
        [SerializeField]
        private Image imageConnection;
        [SerializeField]
        private Sprite spriteConnectionUp;
        [SerializeField]
        private Sprite spriteConnectionUpDown;
        [SerializeField]
        private Image imageToggle;
        [SerializeField]
        private Sprite spriteToogleOn;
        [SerializeField]
        private Sprite spriteToogleOff;
        [SerializeField]
        private TextMeshProUGUI textName;
        [SerializeField]
        private Transform childContainer;

        private bool showingChields = false;
        private List<TreeNode> chields;
        private Action<object> onClick;
        private object state;

        private GameObject gameOb;

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
            gameOb = gameObject;
            imageConnection.enabled = false;
            imageToggle.enabled = false;
            textName.enabled = false;
        }


        private void OnClick()
        {
            if (chields is null)
            {
                onClick?.Invoke(state);
                return;
            }

            showingChields = !showingChields;
            UpdateToogle();

            foreach (TreeNode node in chields)
            {
                node.gameOb.SetActive(showingChields);
            }
        }

        private void UpdateToogle()
        {
            imageToggle.sprite = showingChields ? spriteToogleOn : spriteToogleOff;
        }

        private void SetAsChield(bool isLast)
        {
            imageConnection.sprite = isLast ? spriteConnectionUp : spriteConnectionUpDown;
            imageConnection.enabled = true;
        }

        public void Set(string name, object state, Action<object> onClick)
        {
            textName.text = name;
            textName.enabled = true;
            this.state = state;
            this.onClick = onClick;
        }

        public TreeNode AddNode(TreeNode treeNodePrefab)
        {
            if (chields is null)
            {
                chields = new List<TreeNode>();
                imageToggle.enabled = true;
                UpdateToogle();
            }

            TreeNode treeNode = Instantiate(treeNodePrefab, childContainer, false);
            //actualizo la imagen del hijo anterior si es que tiene
            int lastChield = chields.Count - 1;
            if (lastChield >= 0)
            {
                chields[lastChield].SetAsChield(false);
            }
            //agregamos a la lista de hijos
            chields.Add(treeNode);
            //lo marcamos como ultimo
            treeNode.SetAsChield(true);
            //lo mostramos u ocultamos segun se este mostrando los hijos
            treeNode.gameObject.SetActive(showingChields);

            return treeNode;
        }

        internal void Clear()
        {
            if (chields != null)
            {
                foreach (TreeNode node in chields)
                {
                    node.Clear();
                    Destroy(node.gameOb);
                }
                chields.Clear();
            }
        }
    }
}