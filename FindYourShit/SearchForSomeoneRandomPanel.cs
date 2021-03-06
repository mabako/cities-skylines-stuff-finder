﻿using ColossalFramework.UI;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace fys
{
    class SearchForSomeoneRandomPanel : MonoBehaviour
    {
        private UITextField inputField = null;

        /// <summary>
        /// Name we last entered. Used for allowing 'Enter' again to show multiple people.
        /// </summary>
        private string lastSearchedName = "";

        /// <summary>
        /// Index of the thing we last looked up
        /// </summary>
        private int lastSearchedNameId;

        private bool lastSearchPartial = false;

        private void Awake()
        {
            inputField = UIView.GetAView().AddUIComponent(typeof(UITextField)) as UITextField;
            inputField.width = 300;
            inputField.height = 45;

            inputField.text = "";
            inputField.maxLength = 100;

            inputField.enabled = true;
            inputField.builtinKeyNavigation = true;
            inputField.isInteractive = true;
            inputField.readOnly = false;

            inputField.horizontalAlignment = UIHorizontalAlignment.Center;
            inputField.verticalAlignment = UIVerticalAlignment.Middle;

            inputField.selectionSprite = "EmptySprite";
            inputField.selectionBackgroundColor = new Color32(0, 171, 234, 255);
            inputField.normalBgSprite = "TextFieldPanel";
            inputField.textColor = new Color32(174, 197, 211, 255);
            inputField.disabledTextColor = new Color32(254, 254, 254, 255);
            inputField.textScale = 2f;
            inputField.opacity = 1;
            inputField.color = new Color32(58, 88, 104, 255);
            inputField.disabledColor = new Color32(254, 254, 254, 255);


            inputField.AlignTo(UIView.Find("FullScreenContainer"), UIAlignAnchor.BottomRight);
            inputField.relativePosition -= new Vector3(10, 10);

            /*
            alternatively:
            left edge of field at middle of screen (0), then move to center
            inputField.transformPosition = new Vector2(0f, -0.7f);
            inputField.relativePosition -= new Vector3(inputField.width/2, 0);
            */

            inputField.Hide();
            inputField.eventTextSubmitted += searchTextUpdated;
        }

        private void searchTextUpdated(UIComponent component, string value)
        {
            if (value.Length == 0)
                return;

            if(value != lastSearchedName)
            {
                lastSearchedName = value;
                lastSearchedNameId = -1;
                lastSearchPartial = false;
            }

            if(Search(false))
            {
                // Just found normally.
            }
            else
            {
                if (Search(true))
                {
                    lastSearchPartial = true;
                }
                else
                {
                    // Still not found, welp. What can ya do...
                    inputField.Focus();

                    // And, finally...
                    FocusOn(InstanceID.Empty);
                }
            }
        }

        internal void Toggle()
        {
            if (inputField.isVisible)
                Hide();
            else
                Show();
        }

        internal void Hide()
        {
            inputField.Hide();
        }

        internal void Show()
        {
            inputField.Show();
            inputField.Focus();
        }

        /// <summary>
        /// Note: DO NOT DO THIS AT HOME.
        /// 
        /// If Colossal Order renames these variables or their type, this'll break compat at runtime. All other changes would show on compile time already as their types are available.
        /// </summary>
        /// <typeparam name="T">Type of the variable</typeparam>
        /// <param name="obj">Instance of which to obtain the variable</param>
        /// <param name="fieldName">Name of the field</param>
        /// <returns>Value of the variable, casted to the appropriate type. If it fails, probably some exception... who knows. IT'S MAGIC.</returns>
        private T GetPrivateVariable<T>(object obj, string fieldName)
        {
            return (T)obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
        }

        internal void ContinueSearchMayhaps()
        {
            if (!inputField.isVisible)
                return;

            Search(lastSearchPartial);
        }

        internal bool Search(bool partial)
        {
            if (string.IsNullOrEmpty(lastSearchedName))
                return false;

            InstanceID firstFoundId = InstanceID.Empty, usedId = InstanceID.Empty;
            int match = 0;

            // Credit where Credit is due of course, discovered as part of "Reddit for Chirpy" (my other mod).
            object L = GetPrivateVariable<object>(InstanceManager.instance, "m_lock");
            do { }
            while (!Monitor.TryEnter(L, SimulationManager.SYNCHRONIZE_TIMEOUT));

            try
            {
                // do we have someone called <name>?
                Dictionary<InstanceID, string> dict = GetPrivateVariable<Dictionary<InstanceID, string>>(InstanceManager.instance, "m_names");

                foreach (KeyValuePair<InstanceID, string> entry in dict)
                {
                    if (Match(partial, entry.Value, lastSearchedName))
                    {
                        // this is indeed the first person we found
                        if (firstFoundId.IsEmpty)
                            firstFoundId = entry.Key;

                        if (match++ > lastSearchedNameId)
                        {
                            // Start would have been with (match=0, lastSearchedNameId=-1)
                            // Second time, it'd be (match=0, lastSearchedNameId=0), (match=1, lastSearchedNameId=0) before it'd find someone.
                            ++lastSearchedNameId;
                            usedId = entry.Key;
                            break;
                        }
                    }
                }
            }
            finally
            {
                Monitor.Exit(L);
            }

            if (!usedId.IsEmpty)
            {
                FocusOn(usedId);
                return true;
            }
            else if (!firstFoundId.IsEmpty)
            {
                // we just found the first person again, so we don't want it the next time around
                lastSearchedNameId = 0;
                FocusOn(firstFoundId);
                return true;
            }
            else
                return false;
        }

        private bool Match(bool partial, string instanceName, string searchedFor)
        {
            if (searchedFor == "*")
                return true;

            if (partial)
                return CultureInfo.CurrentCulture.CompareInfo.IndexOf(instanceName, searchedFor, CompareOptions.IgnoreCase) >= 0;
            else
                return CultureInfo.CurrentCulture.CompareInfo.Compare(instanceName, searchedFor, CompareOptions.IgnoreCase) == 0;
        }

        
        private void FocusOn(InstanceID id)
        {
            // Why doesn't this work on people :(
            DefaultTool.OpenWorldInfoPanel(id, ToolsModifierControl.cameraController.transform.position);
            ToolsModifierControl.cameraController.SetTarget(id, ToolsModifierControl.cameraController.transform.position, false);
        }
    }
}
