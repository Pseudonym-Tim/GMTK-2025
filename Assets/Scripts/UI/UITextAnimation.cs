using UnityEngine;
using TMPro;

/// <summary>
/// Handles TextMeshPro animation effects...
/// </summary>
public class UITextAnimation : MonoBehaviour
{
    public TMP_Text textMesh;

    public enum EffectType { Wave, Shake, Pulse }
    public EffectType effectType = EffectType.Wave;

    public float effectIntensity = 3.75f;
    public float effectSpeed = 7.0f;

    private Mesh mesh;
    private Vector3[] verts;

    private void Awake()
    {
        if(textMesh == null) { textMesh = GetComponent<TMP_Text>(); }
    }

    private void Update()
    {
        if(textMesh == null) { textMesh = GetComponent<TMP_Text>(); }
        textMesh.ForceMeshUpdate();
        mesh = textMesh.mesh;
        verts = mesh.vertices;

        float adjustedIntensity = effectIntensity * (textMesh.fontSize / 100f); // Adjust intensity based on font size...

        for(int i = 0; i < textMesh.textInfo.characterCount; i++)
        {
            TMP_CharacterInfo characterInfo = textMesh.textInfo.characterInfo[i];
            int index = characterInfo.vertexIndex;

            if(characterInfo.isVisible) // Check if the character is visible...
            {
                Vector3 offset = Vector3.zero;

                // Apply effect based on selected type...
                switch(effectType)
                {
                    case EffectType.Wave:
                        offset = Wobble(Time.unscaledTime + i, adjustedIntensity);
                        break;
                    case EffectType.Shake:
                        offset = Shake(adjustedIntensity);
                        break;
                    case EffectType.Pulse:
                        offset = Pulse(Time.unscaledTime + i, adjustedIntensity);
                        break;
                }

                verts[index] += offset;
                verts[index + 1] += offset;
                verts[index + 2] += offset;
                verts[index + 3] += offset;
            }
        }

        mesh.vertices = verts;
        textMesh.canvasRenderer.SetMesh(mesh);
    }

    Vector3 Wobble(float time, float intensity)
    {
        return new Vector3(0, Mathf.Sin(time * effectSpeed) * intensity, 0);
    }

    Vector3 Shake(float intensity)
    {
        return new Vector3(Random.Range(-intensity, intensity), Random.Range(-intensity, intensity), 0);
    }

    Vector3 Pulse(float time, float intensity)
    {
        float scale = Mathf.Sin(time * effectSpeed) * intensity;
        return new Vector3(scale, scale, 0);
    }

    public void ResetText()
    {
        if(textMesh == null)
        {
            textMesh = GetComponent<TMP_Text>();
        }

        textMesh.ForceMeshUpdate();
    }

    /*public void ResetText()
    {
        if(textMesh == null)
        {
            textMesh = GetComponent<TMP_Text>();
        }

        if(mesh == null || verts == null)
        {
            textMesh.ForceMeshUpdate();
            mesh = textMesh.mesh;
            verts = mesh.vertices;
        }

        TMP_MeshInfo[] meshInfo = textMesh.textInfo.meshInfo;

        if(meshInfo == null || meshInfo.Length == 0)
        {
            return;
        }

        for(int i = 0; i < textMesh.textInfo.characterCount; i++)
        {
            TMP_CharacterInfo characterInfo = textMesh.textInfo.characterInfo[i];
            int index = characterInfo.vertexIndex;

            if(characterInfo.isVisible)
            {
                if(index + 3 >= verts.Length || index + 3 >= meshInfo[0].vertices.Length)
                {
                    continue; // Skip if out of bounds...
                }

                verts[index] = meshInfo[0].vertices[index];
                verts[index + 1] = meshInfo[0].vertices[index + 1];
                verts[index + 2] = meshInfo[0].vertices[index + 2];
                verts[index + 3] = meshInfo[0].vertices[index + 3];
            }
        }

        mesh.vertices = verts;
        textMesh.canvasRenderer.SetMesh(mesh);
    }*/
}
