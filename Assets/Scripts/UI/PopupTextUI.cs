using TMPro;
using UnityEngine;
using System.Collections;

/// <summary>
/// Screen-space UI popup text...
/// </summary>
public class PopupTextUI : UIComponent
{
    private const float DESTROY_TIME = 1.5f;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Transform rootTransform;

    private void Awake() => SetupUI();

    public override void SetupUI()
    {
        StartCoroutine(FlickerAndMove());
    }

    private IEnumerator FlickerAndMove()
    {
        float elapsed = 0f;
        float flickerStartTime = DESTROY_TIME * 0.5f;
        float flickerInterval = 0.1f;
        float moveSpeed = 30f;

        while(elapsed < DESTROY_TIME)
        {
            if(rootTransform != null)
            {
                rootTransform.localPosition += Vector3.up * moveSpeed * Time.deltaTime;
            }

            if(elapsed >= flickerStartTime)
            {
                messageText.enabled = !messageText.enabled;
                yield return new WaitForSeconds(flickerInterval);
                elapsed += flickerInterval;
            }
            else
            {
                yield return null;
                elapsed += Time.deltaTime;
            }
        }

        Destroy(gameObject);
    }

    public static PopupTextUI Create(Vector2 spawnPos, string message, Transform parent, float scale = 1.0f)
    {
        if(Camera.main != null)
        {
            spawnPos = Camera.main.WorldToScreenPoint(spawnPos);
            GameObject popupTextPrefab = (GameObject)Resources.Load("Prefabs/PopupTextUI");
            PopupTextUI popupTextUI = Instantiate(popupTextPrefab, spawnPos, Quaternion.identity).GetComponent<PopupTextUI>();
            popupTextUI.transform.SetParent(parent, true);
            popupTextUI.name = nameof(PopupTextUI);
            popupTextUI.transform.localScale = Vector3.one * scale;
            if(message != null) { popupTextUI.messageText.text = message; }
            return popupTextUI;
        }

        return null;
    }
}