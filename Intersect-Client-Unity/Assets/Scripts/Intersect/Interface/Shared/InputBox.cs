using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Shared
{
    public class InputBox : Window
    {
        [SerializeField]
        private Image imageModal;
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private TextMeshProUGUI textPrompt;
        [SerializeField]
        private TMP_InputField inputText;
        [SerializeField]
        private GameObject inputTextGO;
        [SerializeField]
        private Button buttonOk;
        [SerializeField]
        private RectTransform buttonOkTransform;
        [SerializeField]
        private TextMeshProUGUI textButtonOk;
        [SerializeField]
        private Button buttonCancel;
        [SerializeField]
        private GameObject buttonCancelGO;
        [SerializeField]
        private TextMeshProUGUI textButtonCancel;

        public enum InputType
        {
            OkayOnly,
            YesNo,
            NumericInput,
            TextInput,
        }

        [NonSerialized]
        public string TextValue;

        [NonSerialized]
        public object UserData;

        [NonSerialized]
        public float Value;

        private InputType mInputType;
        private event EventHandler OkayEventHandler;
        private event EventHandler CancelEventHandler;

        private void Start()
        {
            buttonOk.onClick.AddListener(OnClickOk);
            buttonCancel.onClick.AddListener(OnClickCancel);
        }

        public void Show(string title, string prompt, bool modal, InputType inputtype, EventHandler okayYesSubmitClicked, EventHandler cancelClicked, object userData, int quantity = 0)
        {
            imageModal.raycastTarget = modal;
            textTitle.text = title;
            textPrompt.text = prompt;

            OkayEventHandler = okayYesSubmitClicked;
            CancelEventHandler = cancelClicked;

            UserData = userData;
            mInputType = inputtype;

            if (inputtype == InputType.NumericInput || inputtype == InputType.TextInput)
            {
                inputTextGO.SetActive(true);
                inputText.Select();
            }
            else
            {
                inputTextGO.SetActive(false);
            }

            switch (mInputType)
            {
                case InputType.YesNo:
                {
                    textButtonOk.text = Strings.InputBox.yes;
                    textButtonCancel.text = Strings.InputBox.no;
                    Vector2 anchoredPosition = buttonOkTransform.anchoredPosition;
                    anchoredPosition.x = -90f;
                    buttonOkTransform.anchoredPosition = anchoredPosition;
                    buttonCancelGO.SetActive(true);
                    inputTextGO.SetActive(false);
                }
                break;
                case InputType.OkayOnly:
                {
                    textButtonOk.text = Strings.InputBox.okay;
                    Vector2 anchoredPosition = buttonOkTransform.anchoredPosition;
                    anchoredPosition.x = 0f;
                    buttonOkTransform.anchoredPosition = anchoredPosition;
                    buttonCancelGO.SetActive(false);
                    inputTextGO.SetActive(false);
                }
                break;
                case InputType.NumericInput:
                {
                    textButtonOk.text = Strings.InputBox.okay;
                    textButtonCancel.text = Strings.InputBox.cancel;
                    Vector2 anchoredPosition = buttonOkTransform.anchoredPosition;
                    anchoredPosition.x = -90f;
                    buttonOkTransform.anchoredPosition = anchoredPosition;
                    buttonCancelGO.SetActive(true);
                    inputTextGO.SetActive(true);
                    inputText.contentType = TMP_InputField.ContentType.IntegerNumber;
                    inputText.text = quantity.ToString();
                }
                break;
                case InputType.TextInput:
                {
                    textButtonOk.text = Strings.InputBox.okay;
                    textButtonCancel.text = Strings.InputBox.cancel;
                    Vector2 anchoredPosition = buttonOkTransform.anchoredPosition;
                    anchoredPosition.x = -90f;
                    buttonOkTransform.anchoredPosition = anchoredPosition;
                    buttonCancelGO.SetActive(true);
                    inputTextGO.SetActive(true);
                    inputText.contentType = TMP_InputField.ContentType.Autocorrected;
                    inputText.text = string.Empty;
                }
                break;
            }
            Show();
        }

        private void OnClickCancel()
        {
            if (mInputType == InputType.NumericInput)
            {
                Value = int.Parse(inputText.text);
            }

            if (mInputType == InputType.TextInput)
            {
                TextValue = inputText.text;
            }

            CancelEventHandler?.Invoke(this, EventArgs.Empty);
            Hide();
        }

        private void OnClickOk()
        {

            if (mInputType == InputType.NumericInput)
            {
                Value = int.Parse(inputText.text);
            }

            if (mInputType == InputType.TextInput)
            {
                TextValue = inputText.text;
            }

            OkayEventHandler?.Invoke(this, EventArgs.Empty);
            Hide();
        }
    }
}
