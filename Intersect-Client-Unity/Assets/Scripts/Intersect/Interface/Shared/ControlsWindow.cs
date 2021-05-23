using Intersect.Client.Core.Controls;
using Intersect.Client.Localization;
using Intersect.Client.UnityGame;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Shared
{
    public class ControlsWindow : Window
    {
        [SerializeField]
        private TextMeshProUGUI textTitle;
        [SerializeField]
        private ControlEditor controlEditorPrefab;
        [SerializeField]
        private Transform controlContainer;
        [SerializeField]
        private TextMeshProUGUI textRestore;
        [SerializeField]
        private Button buttonRestore;
        [SerializeField]
        private TextMeshProUGUI textApply;
        [SerializeField]
        private Button buttonApply;
        [SerializeField]
        private TextMeshProUGUI textCancel;
        [SerializeField]
        private Button buttonCancel;

        private readonly Dictionary<Control, ControlEditor> controlEditors = new Dictionary<Control, ControlEditor>();

        private Controls edittingControls;

        protected override void Awake()
        {
            base.Awake();

            textTitle.text = Strings.Controls.title;
            textRestore.text = Strings.Options.restore;
            textApply.text = Strings.Options.apply;
            textCancel.text = Strings.Options.cancel;
            buttonRestore.onClick.AddListener(OnClickRestore);
            buttonApply.onClick.AddListener(OnClickApply);
            buttonCancel.onClick.AddListener(OnClickCancel);

            foreach (Control control in Enum.GetValues(typeof(Control)))
            {
                ControlEditor controlEditor = Instantiate(controlEditorPrefab, controlContainer, false);
                controlEditors.Add(control, controlEditor);
            }
        }

        private void OnEnable()
        {
            Core.Input.KeyDown += OnKeyDown;
        }

        private void OnDisable()
        {
            Core.Input.KeyDown -= OnKeyDown;
        }

        public override void Show(object obj = null)
        {
            base.Show(null);
            ControlEditor.CanEdit = true;

            edittingControls = new Controls(Controls.ActiveControls);
            foreach (Control control in Enum.GetValues(typeof(Control)))
            {
                if (controlEditors.TryGetValue(control, out ControlEditor controlEditor))
                {
                    controlEditor.Setup(edittingControls, control);
                    controlEditor.SetKeys(edittingControls.ControlMapping[control]);
                }
            }
        }

        private void OnClickRestore()
        {
            edittingControls.ResetDefaults();
            foreach (Control control in Enum.GetValues(typeof(Control)))
            {
                if (controlEditors.TryGetValue(control, out ControlEditor controlEditor))
                {
                    controlEditor.SetKeys(edittingControls.ControlMapping[control]);
                }
            }
        }

        private void OnClickCancel()
        {
            Hide();
        }

        private void OnClickApply()
        {
            Controls.ActiveControls = edittingControls;
            Controls.ActiveControls.Save();

            Hide();
        }

        private void OnKeyDown(KeyCode key)
        {
            if (ControlEditor.CanEdit)
            {
                return;
            }
            foreach (KeyValuePair<Control, ControlEditor> controlPair in controlEditors)
            {
                if (controlPair.Value.OnKeyDown(key))
                {
                    RemoveDupliateKeys(key, controlPair.Key);
                }
            }
        }

        private void RemoveDupliateKeys(KeyCode key, Control control)
        {
            if (key == KeyCode.None)
            {
                return;
            }

            foreach (KeyValuePair<Control, ControlMap> controlMapping in edittingControls.ControlMapping)
            {
                if (controlMapping.Key == control)
                {
                    continue;
                }

                if (controlMapping.Value.Key1 == key)
                {
                    //Remove this mapping
                    controlMapping.Value.Key1 = KeyCode.None;

                    //Update UI
                    if (controlEditors.TryGetValue(controlMapping.Key, out ControlEditor controlEditor))
                    {
                        controlEditor.RemoveKey(1);
                    }
                }

                if (controlMapping.Value.Key2 == key)
                {
                    //Remove this mapping
                    controlMapping.Value.Key2 = KeyCode.None;

                    //Update UI
                    if (controlEditors.TryGetValue(controlMapping.Key, out ControlEditor controlEditor))
                    {
                        controlEditor.RemoveKey(2);
                    }
                }
            }
        }
    }
}
