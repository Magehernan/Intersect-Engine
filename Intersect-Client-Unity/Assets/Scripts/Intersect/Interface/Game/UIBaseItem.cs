using Intersect.Client.Interface.Game.Displayers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Intersect.Client.Interface.Game
{
    public abstract class UIBaseItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField]
        protected ItemSpellDisplayer displayer;
        [SerializeField]
        private Vector2 descOffset;
        [SerializeField]
        private Vector2 descPivot = new Vector2(1f, 1f);

        public int Index { get; protected set; }

        protected Transform myTransform;

        protected bool isDraggable = true;

        private void Awake()
        {
            myTransform = transform;
        }

        public virtual void Setup(int index, Transform descTransformDisplay)
        {
            Index = index;
            displayer.SetDescriptionPosition(descTransformDisplay, descOffset, descPivot);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isDraggable)
            {
                displayer.Drag(Interface.GameUi.MyRectTransform, true);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isDraggable)
            {
                displayer.Drag(myTransform, false);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isDraggable)
            {
                displayer.SetPosition(eventData.position);
            }
        }

        public abstract void OnDrop(PointerEventData eventData);

    }
}
