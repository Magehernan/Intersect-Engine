using Intersect.Client.General;
using System;
using TMPro;
using UnityEngine;

namespace Intersect.Client.UnityGame.Graphics
{
    public class ChatBubbleRenderer : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textMessage;

        private long mRenderTimer;

        public void Set(string message)
        {
            textMessage.text = message;
            mRenderTimer = Globals.System.GetTimeMs() + 5000;
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public bool TimeOut()
        {
            return mRenderTimer < Globals.System.GetTimeMs();
        }
    }
}