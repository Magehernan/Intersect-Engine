using Intersect.Client.Core.Controls;
using Intersect.Client.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intersect.Client.Interface.Shared
{
    public class ControlEditor : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textControl;
        [SerializeField]
        private Button buttonPrimary;
        [SerializeField]
        private TextMeshProUGUI textPrimary;
        [SerializeField]
        private Button buttonSecondary;
        [SerializeField]
        private TextMeshProUGUI textSecondary;

        private Controls edittingControls;

        private Control control;

        private bool isEditting;
        private int edittingKey;

        public static bool CanEdit;

        private void Awake()
        {
            buttonPrimary.onClick.AddListener(OnClickPrimary);
            buttonSecondary.onClick.AddListener(OnClickSecondary);
        }

        internal void Setup(Controls edittingControls, Control control)
        {
            this.edittingControls = edittingControls;
            this.control = control;
            textControl.text = Strings.Controls.controldict[control.ToString().ToLower()];
            isEditting = false;
        }

        internal void SetKeys(ControlMap controlMap)
        {
            textPrimary.text = Strings.Keys.keydict[controlMap.Key1];
            textSecondary.text = Strings.Keys.keydict[controlMap.Key2];
        }

        private void OnClickPrimary()
        {
            if (CanEdit)
            {
                CanEdit = false;
                isEditting = true;
                edittingKey = 1;
                textPrimary.text = Strings.Controls.listening;
            }
        }

        private void OnClickSecondary()
        {
            if (CanEdit)
            {
                CanEdit = false;
                isEditting = true;
                edittingKey = 2;
                textSecondary.text = Strings.Controls.listening;
            }
        }

        public bool OnKeyDown(KeyCode key)
        {
            if (!isEditting)
            {
                return false;
            }
            if (!edittingControls.ControlMapping.TryGetValue(control, out ControlMap controlMap))
            {
                return false;
            }
            //edittingControls.UpdateControl(control, edittingKey, key);
            if (edittingKey == 1)
            {
                controlMap.Key1 = key;
                textPrimary.text = Strings.Keys.keydict[key];
                if (controlMap.Key2 == key)
                {
                    controlMap.Key2 = KeyCode.None;
                    RemoveKey(2);
                }
            }
            else
            {
                controlMap.Key2 = key;
                textSecondary.text = Strings.Keys.keydict[key];
                if (controlMap.Key1 == key)
                {
                    controlMap.Key1 = KeyCode.None;
                    RemoveKey(1);
                }
            }

            isEditting = false;
            //esperamos a activar la edicion para evitar el click si se esta intentando asignar un boton del mouse
            Task.Delay(500).ContinueWith(t => CanEdit = true);
            return true;
        }


        internal void RemoveKey(int keyNum)
        {
            if (keyNum == 1)
            {
                textPrimary.text = Strings.Keys.keydict[KeyCode.None];
            }
            else
            {
                textSecondary.text = Strings.Keys.keydict[KeyCode.None];
            }
        }
    }
}
