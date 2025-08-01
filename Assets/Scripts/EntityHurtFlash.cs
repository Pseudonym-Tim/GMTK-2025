using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to hurt flashing...
/// </summary>
[RequireComponent(typeof(Entity))]
public class EntityHurtFlash : MonoBehaviour
{
    private static readonly int FLASH_PROPERTY = Shader.PropertyToID("_Flash");
    private const float DEFAULT_FLASH_TIME = 0.1f;
    private List<SpriteRenderer> flashSpriteRenderers;

    public static void Setup(Entity entity, float flashTime = DEFAULT_FLASH_TIME)
    {
        EntityHurtFlash hurtFlash = entity.GetComponent<EntityHurtFlash>() ?? entity.AddComponent<EntityHurtFlash>();
        hurtFlash.Setup(flashTime);
    }

    public void Setup(float flashTime = DEFAULT_FLASH_TIME)
    {
        flashSpriteRenderers = new List<SpriteRenderer>();
        SpriteRenderer[] spriteRendererList = GetComponentsInChildren<SpriteRenderer>();

        foreach(SpriteRenderer spriteRenderer in spriteRendererList)
        {
            if(spriteRenderer.material.HasProperty(FLASH_PROPERTY))
            {
                flashSpriteRenderers.Add(spriteRenderer);
            }
        }

        if(flashSpriteRenderers.Count > 0)
        {
            StopAllCoroutines();
            StartCoroutine(PerformFlash(flashTime));
        }
        else
        {
            Destroy(this);
        }
    }

    private IEnumerator PerformFlash(float flashTime)
    {
        SetHurtFlash(1f);
        yield return new WaitForSeconds(flashTime);
        SetHurtFlash(0f);
        Destroy(this);
    }

    private void SetHurtFlash(float value)
    {
        foreach(SpriteRenderer spriteRenderer in flashSpriteRenderers)
        {
            if(spriteRenderer != null)
            {
                Material material = spriteRenderer.material;

                if(material != null)
                {
                    material.SetFloat(FLASH_PROPERTY, value);
                }
            }
        }
    }
}