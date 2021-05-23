using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Menu
{

    public class CreditsWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textWindow;
        [SerializeField]
        private Button buttonBack;
        [SerializeField]
        private TextMeshProUGUI textBack;

        private void Start()
        {
            textWindow.text = Strings.Credits.title;

            textBack.text = Strings.Credits.back;
            buttonBack.onClick.AddListener(OnClickBack);
        }

        private void OnClickBack()
        {
            Hide();
            Interface.MenuUi.MainMenu.Show();
        }
    }

}
