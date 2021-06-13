using Intersect.Client.General;
using TMPro;
using UnityEngine;

namespace Intersect.Client.UnityGame.Graphics
{
    public class ChatBubbleRenderer : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textMessage;

        private long mRenderTimer;
        private GameObject myGameObject;

        private void Awake()
        {
            myGameObject = gameObject;
        }

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

        public void Draw(bool show)
        {
            myGameObject.SetActive(show);
        }
    }
}