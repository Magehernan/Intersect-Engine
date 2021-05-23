using Intersect.Client.UnityGame;
using Intersect.Utilities;
using TMPro;
using UnityEngine;

namespace Intersect.Client.Interface.Game
{
    public class AnnouncementWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textAnnouncement;

        private string mLabelText;
        private long mDisplayUntil;

        public void Draw()
        {
            // Only update when we're visible to the user.
            if (IsVisible)
            {
                textAnnouncement.text = mLabelText;

                // Are we still supposed to be visible?
                if (Timing.Global.Milliseconds > mDisplayUntil)
                {
                    Hide();
                }
            }
        }


        internal void ShowAnnouncement(string announcementText, long displayTime)
        {
            mLabelText = announcementText;
            mDisplayUntil = Timing.Global.Milliseconds + displayTime;
            Show();
        }
    }
}