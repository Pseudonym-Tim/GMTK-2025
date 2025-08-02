using UnityEngine;
using TMPro;

/// <summary>
/// Number rolling text effect...
/// </summary>
public class NumberRollerUI : UIComponent
{
    public TMP_Text textMesh;

    public float rollDuration = 1.5f;
    private string targetText;
    private char[] currentChars;
    private float[] lockTimes;
    private float startTime;

    private void Awake()
    {
        if(textMesh == null)
        {
            textMesh = GetComponent<TMP_Text>();
        }
    }

    public void StartRoll(string finalText)
    {
        targetText = finalText;
        currentChars = finalText.ToCharArray();
        lockTimes = new float[currentChars.Length];
        startTime = Time.time;
        float perCharDelay = rollDuration / currentChars.Length;

        for(int i = 0; i < currentChars.Length; i++)
        {
            lockTimes[i] = startTime + rollDuration - i * perCharDelay;
        }

        textMesh.text = new string(currentChars);
    }

    private void Update()
    {
        if(string.IsNullOrEmpty(targetText))
        {
            return;
        }

        bool anyStillRolling = false;

        for(int i = 0; i < currentChars.Length; i++)
        {
            if(Time.time < lockTimes[i])
            {
                currentChars[i] = (char)('0' + Random.Range(0, 10));
                anyStillRolling = true;
            }
            else
            {
                currentChars[i] = targetText[i];
            }
        }

        if(anyStillRolling)
        {
            textMesh.text = new string(currentChars);
        }
    }
}
