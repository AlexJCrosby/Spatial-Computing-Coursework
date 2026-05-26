using UnityEngine;

public enum SpellInputSource
{
    Keyboard,
    Voice
}

public struct SpellCastRequest
{
    public EyeTargetable Target;
    public Transform CastPoint;
    public SpellInputSource InputSource;
    public bool BypassCooldown;

    public SpellCastRequest(
        EyeTargetable target,
        Transform castPoint,
        SpellInputSource inputSource,
        bool bypassCooldown = false
    )
    {
        Target = target;
        CastPoint = castPoint;
        InputSource = inputSource;
        BypassCooldown = bypassCooldown;
    }

    public bool HasValidTarget => Target != null && Target.CanBeTargeted;
    public Vector3 TargetPoint => Target.GetTargetPoint();
}