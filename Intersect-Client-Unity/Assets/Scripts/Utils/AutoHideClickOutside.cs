using System;
using System.Collections.Generic;
using UnityEngine;

namespace Intersect.Client.Utils
{
    public class AutoHideClickOutside : MonoBehaviour
    {
        [SerializeField]
        private List<RectTransform> rectTransformsToCheck;
        [SerializeField]
        private List<GameObject> gameObjectsToHide;
        [SerializeField]
        private List<GameObject> gameObjectsToShow;
        [SerializeField]
        private bool hideWhenClickInside = false;
        [SerializeField]
        private bool forceHideOnDrag = false;

        private Camera mainCamera;
        private Action onHide;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        public void Init(Action[] onHideActions = default, RectTransform[] rectTransformsToCheck = default, GameObject[] gameObjectsToHide = default, GameObject[] gameObjectsToShow = default)
        {
            if (onHideActions != null)
            {
                for (int i = 0; i < onHideActions.Length; i++)
                {
                    onHide += onHideActions[i];
                }
            }

            if (rectTransformsToCheck != null)
            {
                this.rectTransformsToCheck.AddRange(rectTransformsToCheck);
            }

            if (gameObjectsToHide != null)
            {
                this.gameObjectsToHide.AddRange(gameObjectsToHide);
            }

            if (gameObjectsToShow != null)
            {
                this.gameObjectsToShow.AddRange(gameObjectsToShow);
            }
        }

        public void Init(Action onHideAction, RectTransform[] rectTransformsToCheck = default, GameObject[] gameObjectsToHide = default, GameObject[] gameObjectsToShow = default)
        {
            onHide += onHideAction;

            if (rectTransformsToCheck != null)
            {
                this.rectTransformsToCheck.AddRange(rectTransformsToCheck);
            }

            if (gameObjectsToHide != null)
            {
                this.gameObjectsToHide.AddRange(gameObjectsToHide);
            }

            if (gameObjectsToShow != null)
            {
                this.gameObjectsToShow.AddRange(gameObjectsToShow);
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.touchCount <= 0)
            {
                if (Input.GetMouseButtonDown(0))
                {

                    Touch mouseTouch = new Touch
                    {
                        position = Input.mousePosition
                    };

                    if (ProcessTouch(mouseTouch))
                    {
                        ProcessHide();
                    }
                }
            }
#endif

#if UNITY_ANDROID
            if (Input.touchCount > 0)
            {
                bool clicked = false;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began)
                    {
                        clicked = ProcessTouch(touch);
                    }

                    if (forceHideOnDrag)
                    {
                        if (touch.phase == TouchPhase.Moved)
                        {
                            Vector2 position = touch.deltaPosition / touch.deltaTime;
                            if (Mathf.Abs(position.y) >= (0.2f * Screen.height) || Mathf.Abs(position.x) >= 0.2f * Screen.width)
                            {
                               clicked = true;
                            }
                        }
                    }

                }

                if (clicked)
                {
                    ProcessHide();
                }
            }
#endif
        }

        private bool ProcessTouch(Touch touch)
        {
            for (int i = 0; i < rectTransformsToCheck.Count; i++)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransformsToCheck[i], touch.position))
                {
                    return hideWhenClickInside ? true : false;
                }
            }
            return hideWhenClickInside ? false : true;
        }

        private void ProcessHide()
        {
            onHide?.Invoke();
            SetActiveStateOnGameObjects(gameObjectsToHide, false);
            SetActiveStateOnGameObjects(gameObjectsToShow, true);
        }

        private void SetActiveStateOnGameObjects(List<GameObject> gameObjects, bool state)
        {
            if (gameObjects != null || gameObjects.Count <= 0)
            {
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    gameObjects[i].SetActive(state);
                }
            }
        }

    }
}
