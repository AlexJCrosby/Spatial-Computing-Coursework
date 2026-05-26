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

    public SpellCastRequest(
        EyeTargetable target,
        Transform castPoint,
        SpellInputSource inputSource
    )
    {
        Target = target;
        CastPoint = castPoint;
        InputSource = inputSource;
    }

    public bool HasValidTarget => Target != null && Target.CanBeTargeted;
    public Vector3 TargetPoint => Target.GetTargetPoint();
}