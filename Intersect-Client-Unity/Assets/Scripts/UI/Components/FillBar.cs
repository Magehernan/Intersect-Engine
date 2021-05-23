using UnityEngine;

namespace Intersect.Client.UI.Components
{
    public class FillBar : MonoBehaviour
    {
        [SerializeField]
        private RectTransform fillTransform = default;

        [SerializeField, Range(0f, 1f)]
        private float value = 0f;

        [SerializeField]
        private float changeSpeed = 0f;

        private float currentValue = -1f;

        [SerializeField]
        private GameObject selfGO;

        private void Update()
        {
            if (currentValue != value)
            {
                if (value > currentValue)
                {
                    currentValue += changeSpeed * Time.deltaTime;
                    if (currentValue > value)
                    {
                        currentValue = value;
                    }
                }
                else
                {
                    currentValue -= changeSpeed * Time.deltaTime;
                    if (currentValue < value)
                    {
                        currentValue = value;
                    }
                }

                Resize(currentValue);
            }
        }

        public void ChangeValue(float value, bool instant = false)
        {
            this.value = value;
            if (instant || changeSpeed == 0f || currentValue == -1f)
            {
                currentValue = value;
                Resize(value);
            }
        }

        public void Show()
        {
            selfGO.SetActive(true);
        }

        public void Hide()
        {
            selfGO.SetActive(false);
        }

        private void Resize(float value)
        {
            Vector2 anchorMax = fillTransform.anchorMax;
            anchorMax.x = value;
            fillTransform.anchorMax = anchorMax;
        }


        private void OnValidate()
        {
            Resize(value);
        }
    }

}