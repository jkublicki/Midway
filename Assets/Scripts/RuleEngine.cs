using System;
using System.Collections.Generic;
using UnityEngine;



//silnik decyzyjny
public static class RuleEngine
{

    private static Dictionary<Condition, List<ResultingAction>> rules = new Dictionary<Condition, List<ResultingAction>>()
    {
        //wa¿ne: NA w silnik decyzyjnym oznacza "nieistotne" -> ka¿da wartoœæ bêdzie pasowaæ

        { new Condition(InputTarget.Hex, InputHexContent.Empty, TargetHex.NA, HasActiveUnit.FALSE, CanMoveFurther.NA, MovedThisTurn.NA), new List<ResultingAction>() { } },
        { new Condition(InputTarget.Hex, InputHexContent.Empty, TargetHex.NA, HasActiveUnit.TRUE, CanMoveFurther.NA, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay } },
        { new Condition(InputTarget.Hex, InputHexContent.Enemy, TargetHex.NA, HasActiveUnit.FALSE, CanMoveFurther.NA, MovedThisTurn.NA), new List<ResultingAction>() { } },
        { new Condition(InputTarget.Hex, InputHexContent.Enemy, TargetHex.NA, HasActiveUnit.TRUE, CanMoveFurther.NA, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay } },
        { new Condition(InputTarget.Hex, InputHexContent.OwnUsed, TargetHex.NA, HasActiveUnit.FALSE, CanMoveFurther.NA, MovedThisTurn.NA), new List<ResultingAction>() { } },
        { new Condition(InputTarget.Hex, InputHexContent.OwnUsed, TargetHex.NA, HasActiveUnit.TRUE, CanMoveFurther.NA, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay } },
        { new Condition(InputTarget.Hex, InputHexContent.OwnReady, TargetHex.NA, HasActiveUnit.FALSE, CanMoveFurther.NA, MovedThisTurn.FALSE), new List<ResultingAction>() { ResultingAction.ShowOverlay } },
        { new Condition(InputTarget.Hex, InputHexContent.OwnReady, TargetHex.NA, HasActiveUnit.FALSE, CanMoveFurther.NA, MovedThisTurn.TRUE), new List<ResultingAction>() { ResultingAction.ShowMoveOverlay } },



        { new Condition(InputTarget.Hex, InputHexContent.OwnReady, TargetHex.NA, HasActiveUnit.TRUE, CanMoveFurther.NA, MovedThisTurn.FALSE), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.ShowOverlay } },
        { new Condition(InputTarget.Hex, InputHexContent.OwnReady, TargetHex.NA, HasActiveUnit.TRUE, CanMoveFurther.NA, MovedThisTurn.TRUE), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.ShowMoveOverlay } },


        { new Condition(InputTarget.ArrowLeft, InputHexContent.NA, TargetHex.Empty, HasActiveUnit.NA, CanMoveFurther.TRUE, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.MoveUnitLeft, ResultingAction.ShowMoveOverlay } },
        { new Condition(InputTarget.ArrowForward, InputHexContent.NA, TargetHex.Empty, HasActiveUnit.NA, CanMoveFurther.TRUE, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.MoveUnitForward, ResultingAction.ShowMoveOverlay } },
        { new Condition(InputTarget.ArrowRight, InputHexContent.NA, TargetHex.Empty, HasActiveUnit.NA, CanMoveFurther.TRUE, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.MoveUnitRight, ResultingAction.ShowMoveOverlay } },



        { new Condition(InputTarget.ArrowLeft, InputHexContent.NA, TargetHex.Empty, HasActiveUnit.NA, CanMoveFurther.FALSE, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.MoveUnitLeft} },
        { new Condition(InputTarget.ArrowForward, InputHexContent.NA, TargetHex.Empty, HasActiveUnit.NA, CanMoveFurther.FALSE, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.MoveUnitForward} },
        { new Condition(InputTarget.ArrowRight, InputHexContent.NA, TargetHex.Empty, HasActiveUnit.NA, CanMoveFurther.FALSE, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.MoveUnitRight} },




        { new Condition(InputTarget.ArrowLeft, InputHexContent.NA, TargetHex.OffMap, HasActiveUnit.NA, CanMoveFurther.NA, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.MoveUnitLeft, ResultingAction.DestroyUnit } },
        { new Condition(InputTarget.ArrowForward, InputHexContent.NA, TargetHex.OffMap, HasActiveUnit.NA, CanMoveFurther.NA, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.MoveUnitForward, ResultingAction.DestroyUnit } },
        { new Condition(InputTarget.ArrowRight, InputHexContent.NA, TargetHex.OffMap, HasActiveUnit.NA, CanMoveFurther.NA, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.MoveUnitRight, ResultingAction.DestroyUnit } },



        { new Condition(InputTarget.ArrowLeft, InputHexContent.NA, TargetHex.Occupied, HasActiveUnit.NA, CanMoveFurther.TRUE, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.SwapUnitsLeft, ResultingAction.ShowMoveOverlay } },
        { new Condition(InputTarget.ArrowForward, InputHexContent.NA, TargetHex.Occupied, HasActiveUnit.NA, CanMoveFurther.TRUE, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.SwapUnitsForward, ResultingAction.ShowMoveOverlay } },
        { new Condition(InputTarget.ArrowRight, InputHexContent.NA, TargetHex.Occupied, HasActiveUnit.NA, CanMoveFurther.TRUE, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.SwapUnitsRight, ResultingAction.ShowMoveOverlay } },

        { new Condition(InputTarget.ArrowLeft, InputHexContent.NA, TargetHex.Occupied, HasActiveUnit.NA, CanMoveFurther.FALSE, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.SwapUnitsLeft } },
        { new Condition(InputTarget.ArrowForward, InputHexContent.NA, TargetHex.Occupied, HasActiveUnit.NA, CanMoveFurther.FALSE, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.SwapUnitsForward } },
        { new Condition(InputTarget.ArrowRight, InputHexContent.NA, TargetHex.Occupied, HasActiveUnit.NA, CanMoveFurther.FALSE, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.SwapUnitsRight } },




        { new Condition(InputTarget.Attack, InputHexContent.NA, TargetHex.NA, HasActiveUnit.NA, CanMoveFurther.NA, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay, ResultingAction.InitializeCombat } },

        //anulowanie byloby mylace dla drugiego gracza
        //{ new Condition(InputTarget.CANCEL_COMBAT, InputHexContent.NA, TargetHex.NA, HasActiveUnit.NA, CanMoveFurther.NA, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.ShowOverlay } },
        
        
        { new Condition(InputTarget.FinishCombat, InputHexContent.NA, TargetHex.NA, HasActiveUnit.NA, CanMoveFurther.NA, MovedThisTurn.NA), new List<ResultingAction>() { } },


        { new Condition(InputTarget.OffMap, InputHexContent.NA, TargetHex.NA, HasActiveUnit.FALSE, CanMoveFurther.NA, MovedThisTurn.NA), new List<ResultingAction>() { } },
        { new Condition(InputTarget.OffMap, InputHexContent.NA, TargetHex.NA, HasActiveUnit.TRUE, CanMoveFurther.NA, MovedThisTurn.NA), new List<ResultingAction>() { ResultingAction.HideOverlay } }

    };

    private static List<KeyValuePair<Condition, List<ResultingAction>>> rulesList;

    static RuleEngine()
    {
        rulesList = new List<KeyValuePair<Condition, List<ResultingAction>>>(rules);
    }

    public static List<ResultingAction> PerformEvaluation(Condition condition)
    {
        /*
        List<ResultingAction> resultingActionList;
        rules.TryGetValue(condition, out resultingActionList);
        return (resultingActionList);
        */

        foreach (KeyValuePair<Condition, List<ResultingAction>> kvp in rulesList)
        {
            if (ConditionMatches(condition, kvp.Key))
            {
                return kvp.Value;
            }
        }

        Debug.Log("W zestawie regul nie znaleziono pasujacej do zadanego warunku!");
        return new List<ResultingAction>();
    }



    private static bool FieldMatches<T>(T inputValue, T ruleValue) where T : Enum
    {
        // NA in rule means "irrelevant" (always matches); otherwise, must match exactly
        return Enum.GetName(typeof(T), ruleValue) == "NA" || inputValue.Equals(ruleValue);
    }

    private static bool ConditionMatches(Condition input, Condition rule)
    {
        if (!FieldMatches(input.InputTarget, rule.InputTarget)) return false;
        if (!FieldMatches(input.InputHexContent, rule.InputHexContent)) return false;
        if (!FieldMatches(input.TargetHex, rule.TargetHex)) return false;
        if (!FieldMatches(input.HasActiveUnit, rule.HasActiveUnit)) return false;
        if (!FieldMatches(input.CanMoveFurther, rule.CanMoveFurther)) return false;
        if (!FieldMatches(input.MovedThisTurn, rule.MovedThisTurn)) return false;
        return true;
    }
}
