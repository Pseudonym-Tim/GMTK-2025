using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Fading screen transition...
/// </summary>
public class FadeUI : UIComponent
{
    private const float DEFAULT_FADE_TIME = 1f;

    public static event Action OnFadeInComplete;
    public static event Action OnFadeOutComplete;

    private Coroutine currentCoroutine;
    public bool IsFading { get; private set; }

    public override void SetupUI()
    {
        UICanvasGroup.alpha = 1f;
    }

    public Coroutine FadeIn(float duration = DEFAULT_FADE_TIME, float delay = 0f)
    {
        return StartFade(duration, true, delay);
    }

    public Coroutine FadeOut(float duration = DEFAULT_FADE_TIME, float delay = 0f)
    {
        return StartFade(duration, false, delay);
    }

    private Coroutine StartFade(float duration, bool fadeIn, float delay)
    {
        if(currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(DoFade(duration, fadeIn, delay));
        return currentCoroutine;
    }

    private IEnumerator DoFade(float duration, bool fadeIn, float delay)
    {
        IsFading = true;

        // If there's a delay, wait first...
        if(delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        float startAlpha = fadeIn ? 1f : 0f;
        float endAlpha = fadeIn ? 0f : 1f;
        UICanvasGroup.alpha = startAlpha;

        float elapsed = 0f;

        while(elapsed < duration)
        {
            float t = elapsed / duration;
            UICanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        UICanvasGroup.alpha = endAlpha;

        if(fadeIn)
        {
            OnFadeInComplete?.Invoke();
        }
        else
        {
            OnFadeOutComplete?.Invoke();
        }

        IsFading = false;
    }
}