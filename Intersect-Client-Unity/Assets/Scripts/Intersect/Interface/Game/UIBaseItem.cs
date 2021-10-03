using Intersect.Client.Interface.Game.Displayers;
using Intersect.Client.Utils;
using System.Collections;
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

        private PointerEventData pointerEventData = null;

        private void Awake()
        {
            myTransform = transform;
        }

        private void OnDisable()
        {
            if (!isDraggable)
            {
                return;
            }
            
            if(Singleton.Instance == null)
            {
                return;
            }

            Singleton.Instance.StartCoroutine(EndDragOnDisableCoroutine());
        }

        private IEnumerator EndDragOnDisableCoroutine()
        {
            yield return null;
            if (pointerEventData != null)
            {
                pointerEventData.pointerDrag = null;
            }

            OnEndDrag(null);
        }

        public virtual void Setup(int index, Transform descTransformDisplay)
        {
            Index = index;
            displayer.SetDescriptionPosition(descTransformDisplay, descOffset, descPivot);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isDraggable)
            {
                return;
            }

            pointerEventData = eventData;

            displayer.Drag(Interface.GameUi.MyRectTransform, true);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDraggable)
            {
                return;
            }

            pointerEventData = null;

            displayer.Drag(myTransform, false);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDraggable)
            {
                return;
            }

            displayer.SetPosition(eventData.position);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!isDraggable)
            {
                return;
            }

            Drop(eventData);
        }

        public abstract void Drop(PointerEventData eventData);
    }
}
