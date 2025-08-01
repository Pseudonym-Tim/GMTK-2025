using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything to do with entity screenwrapping...
/// </summary>
public class ScreenwrapManager : Singleton<ScreenwrapManager>
{
    private static readonly List<IScreenWrappable> wrappables = new List<IScreenWrappable>();

    public static void Register(IScreenWrappable wrappable)
    {
        if(!wrappables.Contains(wrappable))
        {
            wrappables.Add(wrappable);
        }
    }

    public static void Unregister(IScreenWrappable wrappable)
    {
        if(wrappables.Contains(wrappable))
        {
            wrappables.Remove(wrappable);
        }
    }

    private void Update()
    {
        Rect screenRect = GetScreenRect();

        // Copy list, so unregistering during the wrap won't break iteration...
        List<IScreenWrappable> toCheck = new List<IScreenWrappable>(wrappables);

        foreach(IScreenWrappable wrappable in toCheck)
        {
            AttemptWrap(wrappable, screenRect);
        }
    }

    private Rect GetScreenRect()
    {
        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        Vector2 center = (Vector2)cam.transform.position;
        return new Rect(center - new Vector2(halfWidth, halfHeight), new Vector2(halfWidth * 2f, halfHeight * 2f));
    }

    public static Vector2[] GetWrapOffsets()
    {
        Vector2 size = Instance.GetScreenRect().size;
        float w = size.x;
        float h = size.y;

        return new Vector2[]
        {
            new Vector2( 0f,  0f),
            new Vector2( w,  0f),
            new Vector2(-w,  0f),
            new Vector2( 0f,  h),
            new Vector2( 0f, -h),
            new Vector2( w,  h),
            new Vector2( w, -h),
            new Vector2(-w,  h),
            new Vector2(-w, -h)
        };
    }

    public static Vector2 GetBestWrappedPosition(Vector2 origin, Vector2 target)
    {
        Vector2 bestPos = target;
        float bestDist = float.MaxValue;

        foreach(Vector2 offset in GetWrapOffsets())
        {
            Vector2 candidate = target + offset;
            float distance = Vector2.Distance(origin, candidate);

            if(distance < bestDist)
            {
                bestDist = distance;
                bestPos = candidate;
            }
        }

        return bestPos;
    }

    private void AttemptWrap(IScreenWrappable wrappable, Rect screenRect)
    {
        Entity entity = wrappable as Entity;

        if(entity == null)
        {
            return;
        }

        Vector2 centerOfMass = entity.CenterOfMass;
        Vector2 size = wrappable.ScreenwrapBoundsSize;
        Vector2 offset = wrappable.ScreenwrapBoundsOffset;
        float halfWidth = size.x / 2f;
        float halfHeight = size.y / 2f;

        // Right-side wrap...
        if(centerOfMass.x + offset.x - halfWidth > screenRect.xMax)
        {
            float newX = screenRect.xMin - halfWidth - offset.x;
            AttemptWrap(entity, wrappable, new Vector2(newX - centerOfMass.x, 0f));
        }

        // Left-side wrap...
        else if(centerOfMass.x + offset.x + halfWidth < screenRect.xMin)
        {
            float newX = screenRect.xMax + halfWidth - offset.x;
            AttemptWrap(entity, wrappable, new Vector2(newX - centerOfMass.x, 0f));
        }

        // Top-side wrap...
        if(centerOfMass.y + offset.y - halfHeight > screenRect.yMax)
        {
            float newY = screenRect.yMin - halfHeight - offset.y;
            AttemptWrap(entity, wrappable, new Vector2(0f, newY - centerOfMass.y));
        }

        // Bottom-side wrap...
        else if(centerOfMass.y + offset.y + halfHeight < screenRect.yMin)
        {
            float newY = screenRect.yMax + halfHeight - offset.y;
            AttemptWrap(entity, wrappable, new Vector2(0f, newY - centerOfMass.y));
        }
    }

    private void AttemptWrap(Entity entity, IScreenWrappable wrappable, Vector2 translation)
    {
        if(wrappable.MaxScreenwraps < 0 || wrappable.ScreenwrapsUsed < wrappable.MaxScreenwraps)
        {
            entity.Teleport(entity.EntityPosition + translation);
            wrappable.ScreenwrapsUsed++;
            wrappable.OnScreenwrap();
        }
    }
}