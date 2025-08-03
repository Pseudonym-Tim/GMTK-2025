using UnityEngine;

/// <summary>
/// Self explanatory...
/// </summary>
public class DestroyAfterAnimation : MonoBehaviour
{
    public Animator animator;
    public string animationName;
    public float delay = 0f;

    private bool isDestroyed = false;

    private void Update()
    {
        if(!isDestroyed && animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            isDestroyed = true;
            Destroy(gameObject, delay);
        }
    }
}
