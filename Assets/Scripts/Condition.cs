using System;

public struct Condition
{
    public RuleEngine.InputTargetE InputTarget;
    public RuleEngine.InputHexContentE InputHexContent;
    public RuleEngine.TargetHexE TargetHex;
    public RuleEngine.HasActiveUnitE HasActiveUnit;
    public RuleEngine.CanMoveFurtherE CanMoveFurther;
    public RuleEngine.MovedThisTurnE MovedThisTurn;

    public Condition(RuleEngine.InputTargetE inputTarget, RuleEngine.InputHexContentE inputHexContent,
                     RuleEngine.TargetHexE targetHex, RuleEngine.HasActiveUnitE hasActiveUnit,
                     RuleEngine.CanMoveFurtherE canMoveFurther, RuleEngine.MovedThisTurnE movedThisTurn)
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