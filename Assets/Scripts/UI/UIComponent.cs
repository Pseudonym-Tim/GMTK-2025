using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all UI components...
/// </summary>
public class UIComponent : MonoBehaviour
{
    public Canvas UICanvas;
    public CanvasGroup UICanvasGroup;

    public virtual void SetupUI()
    {

    }

    public virtual void ResetUI()
    {

    }

    public virtual void Show(bool showUI = true)
    {

    }

    public virtual void OnFullscreenResolutionChanged()
    {

    }

    public void DestroyUI()
    {
        Destroy(gameObject);
    }

    public void SetCanvasInteractivity(bool interactable)
    {
        if(UICanvasGroup != null)
        {
            UICanvasGroup.interactable = interactable;
            UICanvasGroup.blocksRaycasts = interactable;
        }
    }

    public virtual bool IsShown => UICanvas.enabled;
    public virtual bool IsInteractable => UICanvasGroup.interactable;
    public GameObject UIObject => gameObject;
}
