using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles core UI related setup and stuff...
/// </summary>
public class UIManager : Singleton<UIManager>
{
    [SerializeField] private bool initializeByDefault = false;

    private void Awake()
    {
        if(initializeByDefault)
        {
            SetupUI();
        }
    }

    public static void SetupUI()
    {
        List<UIComponent> uiComponentList = GetUIComponents<UIComponent>();

        foreach(UIComponent uiComponent in uiComponentList)
        {
            uiComponent.SetupUI();
        }
    }

    public static T GetUIComponent<T>() where T : UIComponent
    {
        return GetComponentRecursively<T>(UIParent.transform);
    }

    public static List<T> GetUIComponents<T>() where T : UIComponent
    {
        return GetComponentsRecursively<T>(UIParent.transform).ToList();
    }

    private static T GetComponentRecursively<T>(Transform parentTransform) where T : UIComponent
    {
        T component = parentTransform.GetComponent<T>();

        if(component != null)
        {
            return component;
        }

        for(int i = 0; i < parentTransform.childCount; i++)
        {
            T foundComponent = GetComponentRecursively<T>(parentTransform.GetChild(i));

            if(foundComponent != null)
            {
                return foundComponent;
            }
        }

        return null;
    }

    private static IEnumerable<T> GetComponentsRecursively<T>(Transform parentTransform) where T : UIComponent
    {
        T component = parentTransform.GetComponent<T>();

        if(component != null) { yield return component; }

        // Recurse into each child...
        for(int i = 0; i < parentTransform.childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);

            foreach(T childComponent in GetComponentsRecursively<T>(child))
            {
                yield return childComponent;
            }
        }
    }

    public static void UpdateUIForFullscreen(bool isOn)
    {
        List<UIComponent> uiComponentList = GetUIComponents<UIComponent>();

        foreach(UIComponent uiComponent in uiComponentList)
        {
            uiComponent.OnFullscreenResolutionChanged();

            if(uiComponent.UICanvas)
            {
                uiComponent.UICanvas.pixelPerfect = !isOn;
            }
        }
    }

    public static GameObject UIParent { get { return GameObject.Find("UI"); } }
}
