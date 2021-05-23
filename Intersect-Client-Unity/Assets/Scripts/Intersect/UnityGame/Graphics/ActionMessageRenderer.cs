using Intersect.Client.Utils;
using System;
using TMPro;
using UnityEngine;

namespace Intersect.Client.UnityGame
{
    public class ActionMessageRenderer : MonoBehaviour
    {
        [SerializeField]
        private TextMeshPro textMessage;
        [SerializeField]
        private Transform myTransform;

        internal void Draw(string msg, float x, float y, Color clr)
        {
            textMessage.text = msg;
            textMessage.color = clr.ToColor32();
            myTransform.position = new Vector3(x, -y);
        }

        internal void Destroy()
        {
            Destroy(gameObject);
        }
    }
}