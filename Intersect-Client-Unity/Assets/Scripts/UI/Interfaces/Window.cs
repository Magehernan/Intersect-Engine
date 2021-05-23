using UnityEngine;

namespace Intersect.Client.UnityGame
{
    public abstract class Window : MonoBehaviour
    {

        private RectTransform myRectTransform;
        public RectTransform MyRectTransform
        {
            get
            {
                if (myRectTransform == null)
                {
                    myRectTransform = GetComponent<RectTransform>();
                }
                return myRectTransform;
            }
        }


        private GameObject myGameObject;
        public GameObject MyGameObject
        {
            get
            {
                if (myGameObject == null)
                {
                    myGameObject = gameObject;
                }
                return myGameObject;
            }
        }

        public bool IsHidden
        {
            get => !MyGameObject.activeSelf;
            set => MyGameObject.SetActive(!value);
        }

        public bool IsClosable { get; set; } = true;

        public bool IsVisible => MyGameObject.activeSelf;

        //protected abstract MessageTypes ShowMessage { get; }
        protected virtual bool VisibleOnInit { get; } = false;

        private bool initiated = false;
        public void InitWindow()
        {
            if (!initiated)
            {
                //if (ShowMessage != MessageTypes.None) {
                //	MessageManager.AttachListener(ShowMessage, Show);
                //}
                initiated = true;
                MyGameObject.SetActive(VisibleOnInit);
            }
        }

        protected virtual void Awake()
        {
            InitWindow();
        }

        //protected virtual void OnDestroy() {
        //	if (ShowMessage != MessageTypes.None && initiated) {
        //		MessageManager.DetachListener(ShowMessage, Show);
        //	}
        //}

        public virtual void Show(object obj = null)
        {
            MyGameObject.SetActive(true);
        }

        public virtual void Hide(object obj = null)
        {
            if (IsClosable)
            {
                MyGameObject.SetActive(false);
            }
        }
    }
}