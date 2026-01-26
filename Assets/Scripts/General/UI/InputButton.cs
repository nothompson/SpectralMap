    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.InputSystem;
    using TMPro;    

    public class InputButton : MonoBehaviour
    {
        InputAction targetAction;
        private InputActionRebindingExtensions.RebindingOperation rebind;
        public string actionMap;
        public string actionType;
        public string compositePart;
        public SpriteText text;

        void OnEnable()
        {
            targetAction = InputManager.Instance.inputs.FindAction(actionMap + "/" + actionType, true);

            StartCoroutine(Refresh());
        }

        IEnumerator Refresh()
        {
            yield return null;
            RefreshBinding();
        }

        public void StartRebind()
        {
            if(targetAction == null) return;

            rebind?.Cancel();

            int bindingIndex = FindTargetBindingIndex();

            bool enabled = targetAction.enabled;

            if(enabled) targetAction.Disable();

            rebind = targetAction.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("<Gamepad>");

            if(actionType == "Menu") rebind.WithControlsExcluding("Mouse");
            
            rebind
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    operation.Dispose();

                    targetAction.Enable();
                    RefreshBinding();
                })
                .OnCancel(operation =>
                {
                    operation.Dispose();
                    targetAction.Enable();
                })
                .Start();
        }

        int FindTargetBindingIndex()
        {
            var bindings = targetAction.bindings;

        if (!string.IsNullOrEmpty(compositePart))
        {
            for(int i = 0; i < bindings.Count; i++)
            {
                if(bindings[i].isPartOfComposite && bindings[i].name.Equals(compositePart, System.StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
        }

        for(int i = 0; i < bindings.Count; i++)
        {
            if(!bindings[i].isComposite && !bindings[i].isPartOfComposite)
            {
                return i;
            }
        }

        return 0;
        }

       void RefreshBinding()
    {
        if(targetAction == null || text == null) return;

        int bindingIndex = FindTargetBindingIndex();
        var binding = targetAction.bindings[bindingIndex];

        string path = string.IsNullOrEmpty(binding.overridePath) ? binding.effectivePath : binding.overridePath;

        string display;

        if(path.Contains("leftButton")) display = "M1";
        else if(path.Contains("rightButton")) display = "M2";
        else if(path.Contains("middleButton")) display = "M3";
        else display = targetAction.GetBindingDisplayString(bindingIndex);

        text.input = display;
        text.Refresh();
    }

        public void StopRebind()
        {
            rebind?.Cancel();
        }

        void OnDisable()
        {
            rebind?.Cancel();
        }
    }
