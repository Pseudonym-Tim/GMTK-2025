using UnityEngine;

/// <summary>
/// Entity animation related stuff...
/// </summary>
public class EntityAnimator
{
    public Animator Animator { get; private set; }
    private readonly string entityName;

    public EntityAnimator(Entity entity)
    {
        Animator = entity.GetComponent<Animator>();
        entityName = entity.name;

        if(!Animator)
        {
            Debug.Log($"[{entityName}] does not have an animator component!");
        }
    }

    public void PlayAnimationState(string animStateName, float normalizedTime = 0, int layer = 0)
    {
        // Grab current animator state info and state hash...
        AnimatorStateInfo stateInfo = GetCurrentAnimStateInfo(layer);
        int stateHash = Animator.StringToHash(animStateName);

        // Already playing? Skip!
        if(IsPlayingAnimationState(animStateName)) { return; }

        // No state found?
        if(!Animator.HasState(layer, stateHash))
        {
            Debug.Log($"Animation state: [{animStateName}] does not exist for: [{entityName}]!");
            return;
        }

        // Play the animation with specified layer and normalized time...
        Animator.Play(stateHash, layer, normalizedTime);
    }

    public bool IsAnimationFinished(int layer = 0)
    {
        if(Animator == null || Animator.layerCount <= layer) { return true; }

        AnimatorStateInfo animStateInfo = GetCurrentAnimStateInfo(layer);

        if(animStateInfo.fullPathHash == 0)
        {
            return true;
        }

        return animStateInfo.normalizedTime >= 1f && !Animator.IsInTransition(layer) && !animStateInfo.loop;
    }

    public float GetCurrentDuration(int layer = 0)
    {
        AnimatorStateInfo animStateInfo = GetCurrentAnimStateInfo(layer);
        return animStateInfo.length;
    }

    public float GetRemainingTime(int layer = 0)
    {
        AnimatorStateInfo animStateInfo = GetCurrentAnimStateInfo(layer);
        float normalizedTime = animStateInfo.normalizedTime % 1f;
        float totalDuration = animStateInfo.length;
        float currentTime = normalizedTime * totalDuration;
        float remainingTime = totalDuration - currentTime;
        return remainingTime;
    }

    public bool IsPlayingAnimationState(string animStateName, int layer = 0)
    {
        AnimatorStateInfo animStateInfo = GetCurrentAnimStateInfo(layer);
        return animStateInfo.IsName(animStateName);
    }

    public AnimatorStateInfo GetCurrentAnimStateInfo(int layer = 0)
    {
        return Animator.GetCurrentAnimatorStateInfo(layer);
    }

    public void SetFloat(string parameterName, float value) => Animator.SetFloat(parameterName, value);
    public void SetBool(string parameterName, bool value) => Animator.SetBool(parameterName, value);
    public void SetPlaybackSpeed(float speed) => Animator.speed = speed;
}
