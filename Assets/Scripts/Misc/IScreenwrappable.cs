using UnityEngine;

public interface IScreenWrappable
{
    Vector2 ScreenwrapBoundsSize { get; }
    Vector2 ScreenwrapBoundsOffset { get; }
    int MaxScreenwraps { get; }
    int ScreenwrapsUsed { get; set; }
    void OnScreenwrap();
}