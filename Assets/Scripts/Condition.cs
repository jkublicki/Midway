using System;

public struct Condition
{
    public InputTarget InputTarget;
    public InputHexContent InputHexContent;
    public TargetHex TargetHex;
    public HasActiveUnit HasActiveUnit;
    public CanMoveFurther CanMoveFurther;
    public MovedThisTurn MovedThisTurn;

    public Condition(InputTarget inputTarget, InputHexContent inputHexContent,
                     TargetHex targetHex, HasActiveUnit hasActiveUnit,
                     CanMoveFurther canMoveFurther, MovedThisTurn movedThisTurn)
    {
        InputTarget = inputTarget;
        InputHexContent = inputHexContent;
        TargetHex = targetHex;
        HasActiveUnit = hasActiveUnit;
        CanMoveFurther = canMoveFurther;
        MovedThisTurn = movedThisTurn;
    }

    public override bool Equals(object obj)
    {
        if (obj is Condition other)
        {
            return InputTarget == other.InputTarget && InputHexContent == other.InputHexContent &&
                   TargetHex == other.TargetHex && HasActiveUnit == other.HasActiveUnit &&
                   CanMoveFurther == other.CanMoveFurther && MovedThisTurn == other.MovedThisTurn;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(InputTarget, InputHexContent, TargetHex, HasActiveUnit, CanMoveFurther, MovedThisTurn);
    }

    public override string ToString()
    {
        return $"Condition(InputTarget: {InputTarget}, InputHexContent: {InputHexContent}, " +
               $"TargetHex: {TargetHex}, HasActiveUnit: {HasActiveUnit}, " +
               $"CanMoveFurther: {CanMoveFurther}, MovedThisTurn: {MovedThisTurn})";
    }
}