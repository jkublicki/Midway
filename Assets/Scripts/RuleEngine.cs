using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using static RuleEngine;

//poprzedaæ go musi coœ co ustali ten input-context: dwojako: input i context
//coœ w rodzaju silnika decyzyjnego: input-context -> action-set
//tutaj zarówno wejscie jak i wyjscie jest mocno abstrakcyjne; utworzenie tej abstrakcji na wejsciu i obsluzenie na wyjsciu bedzie zlozone

//implementator decyzji rule executora bedzie potrzebowac wiecej informacji niz w tych enumach, np. koordynaty, kierunek zakretu
//zarazem powinien byc w stanie dzialac wylacznie w oparciu o output rule executora
//wiec niech przez rule executora przeleci cos w rodzaju "extra data/details"


public static class RuleEngine
{
    public enum InputTargetE
    {
        HEX,
        ARROW_LEFT,
        ARROW_FORWARD,
        ARROW_RIGHT,
        ATTACK,
        OFF_MAP,
        CANCEL_COMBAT,
        FINISH_COMBAT //to wyj¹tkowo zdarzenie, a nie input
    }

    public enum InputHexContentE
    {
        NA,
        EMPTY,
        ENEMY,
        OWN_READY,
        OWN_USED
    }

    public enum TargetHexE
    {
        NA,
        EMPTY,
        OFF_MAP,
        OCCUPIED
    }

    public enum HasActiveUnitE
    {
        NA,
        TRUE,
        FALSE
    }

    public enum CanMoveFurtherE
    {
        NA,
        TRUE,
        FALSE
    }

    public enum MovedThisTurnE
    {
        NA,
        TRUE,
        FALSE
    }


    public enum ResultingActionE
    {
        HIDE_OVERLAY,
        SHOW_OVERLAY,
        SHOW_MOVE_OVERLAY,
        MOVE_UNIT_LEFT,
        MOVE_UNIT_FORWARD,
        MOVE_UNIT_RIGHT,
        DESTROY_UNIT,
        SWAP_UNITS_LEFT,
        SWAP_UNITS_FORWARD,
        SWAP_UNITS_RIGHT,
        //SHOW_COMBAT_MENU
        INITIALIZE_COMBAT
    }

    private static Dictionary<Condition, List<ResultingActionE>> rules = new Dictionary<Condition, List<ResultingActionE>>()
    {
        //wa¿ne: NA w silnik decyzyjnym oznacza "nieistotne" -> ka¿da wartoœæ bêdzie pasowaæ

        { new Condition(InputTargetE.HEX, InputHexContentE.EMPTY, TargetHexE.NA, HasActiveUnitE.FALSE, CanMoveFurtherE.NA, MovedThisTurnE.NA), new List<ResultingActionE>() { } },
        { new Condition(InputTargetE.HEX, InputHexContentE.EMPTY, TargetHexE.NA, HasActiveUnitE.TRUE, CanMoveFurtherE.NA, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY } },
        { new Condition(InputTargetE.HEX, InputHexContentE.ENEMY, TargetHexE.NA, HasActiveUnitE.FALSE, CanMoveFurtherE.NA, MovedThisTurnE.NA), new List<ResultingActionE>() { } },
        { new Condition(InputTargetE.HEX, InputHexContentE.ENEMY, TargetHexE.NA, HasActiveUnitE.TRUE, CanMoveFurtherE.NA, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY } },
        { new Condition(InputTargetE.HEX, InputHexContentE.OWN_USED, TargetHexE.NA, HasActiveUnitE.FALSE, CanMoveFurtherE.NA, MovedThisTurnE.NA), new List<ResultingActionE>() { } },
        { new Condition(InputTargetE.HEX, InputHexContentE.OWN_USED, TargetHexE.NA, HasActiveUnitE.TRUE, CanMoveFurtherE.NA, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY } },
        { new Condition(InputTargetE.HEX, InputHexContentE.OWN_READY, TargetHexE.NA, HasActiveUnitE.FALSE, CanMoveFurtherE.NA, MovedThisTurnE.FALSE), new List<ResultingActionE>() { ResultingActionE.SHOW_OVERLAY } },
        { new Condition(InputTargetE.HEX, InputHexContentE.OWN_READY, TargetHexE.NA, HasActiveUnitE.FALSE, CanMoveFurtherE.NA, MovedThisTurnE.TRUE), new List<ResultingActionE>() { ResultingActionE.SHOW_MOVE_OVERLAY } },



        { new Condition(InputTargetE.HEX, InputHexContentE.OWN_READY, TargetHexE.NA, HasActiveUnitE.TRUE, CanMoveFurtherE.NA, MovedThisTurnE.FALSE), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.SHOW_OVERLAY } },
        { new Condition(InputTargetE.HEX, InputHexContentE.OWN_READY, TargetHexE.NA, HasActiveUnitE.TRUE, CanMoveFurtherE.NA, MovedThisTurnE.TRUE), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.SHOW_MOVE_OVERLAY } },


        { new Condition(InputTargetE.ARROW_LEFT, InputHexContentE.NA, TargetHexE.EMPTY, HasActiveUnitE.NA, CanMoveFurtherE.TRUE, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.MOVE_UNIT_LEFT, ResultingActionE.SHOW_MOVE_OVERLAY } },
        { new Condition(InputTargetE.ARROW_FORWARD, InputHexContentE.NA, TargetHexE.EMPTY, HasActiveUnitE.NA, CanMoveFurtherE.TRUE, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.MOVE_UNIT_FORWARD, ResultingActionE.SHOW_MOVE_OVERLAY } },
        { new Condition(InputTargetE.ARROW_RIGHT, InputHexContentE.NA, TargetHexE.EMPTY, HasActiveUnitE.NA, CanMoveFurtherE.TRUE, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.MOVE_UNIT_RIGHT, ResultingActionE.SHOW_MOVE_OVERLAY } },



        { new Condition(InputTargetE.ARROW_LEFT, InputHexContentE.NA, TargetHexE.EMPTY, HasActiveUnitE.NA, CanMoveFurtherE.FALSE, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.MOVE_UNIT_LEFT} },
        { new Condition(InputTargetE.ARROW_FORWARD, InputHexContentE.NA, TargetHexE.EMPTY, HasActiveUnitE.NA, CanMoveFurtherE.FALSE, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.MOVE_UNIT_FORWARD} },
        { new Condition(InputTargetE.ARROW_RIGHT, InputHexContentE.NA, TargetHexE.EMPTY, HasActiveUnitE.NA, CanMoveFurtherE.FALSE, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.MOVE_UNIT_RIGHT} },




        { new Condition(InputTargetE.ARROW_LEFT, InputHexContentE.NA, TargetHexE.OFF_MAP, HasActiveUnitE.NA, CanMoveFurtherE.NA, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.MOVE_UNIT_LEFT, ResultingActionE.DESTROY_UNIT } },
        { new Condition(InputTargetE.ARROW_FORWARD, InputHexContentE.NA, TargetHexE.OFF_MAP, HasActiveUnitE.NA, CanMoveFurtherE.NA, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.MOVE_UNIT_FORWARD, ResultingActionE.DESTROY_UNIT } },
        { new Condition(InputTargetE.ARROW_RIGHT, InputHexContentE.NA, TargetHexE.OFF_MAP, HasActiveUnitE.NA, CanMoveFurtherE.NA, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.MOVE_UNIT_RIGHT, ResultingActionE.DESTROY_UNIT } },



        { new Condition(InputTargetE.ARROW_LEFT, InputHexContentE.NA, TargetHexE.OCCUPIED, HasActiveUnitE.NA, CanMoveFurtherE.TRUE, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.SWAP_UNITS_LEFT, ResultingActionE.SHOW_MOVE_OVERLAY } },
        { new Condition(InputTargetE.ARROW_FORWARD, InputHexContentE.NA, TargetHexE.OCCUPIED, HasActiveUnitE.NA, CanMoveFurtherE.TRUE, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.SWAP_UNITS_FORWARD, ResultingActionE.SHOW_MOVE_OVERLAY } },
        { new Condition(InputTargetE.ARROW_RIGHT, InputHexContentE.NA, TargetHexE.OCCUPIED, HasActiveUnitE.NA, CanMoveFurtherE.TRUE, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.SWAP_UNITS_RIGHT, ResultingActionE.SHOW_MOVE_OVERLAY } },

        { new Condition(InputTargetE.ARROW_LEFT, InputHexContentE.NA, TargetHexE.OCCUPIED, HasActiveUnitE.NA, CanMoveFurtherE.FALSE, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.SWAP_UNITS_LEFT } },
        { new Condition(InputTargetE.ARROW_FORWARD, InputHexContentE.NA, TargetHexE.OCCUPIED, HasActiveUnitE.NA, CanMoveFurtherE.FALSE, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.SWAP_UNITS_FORWARD } },
        { new Condition(InputTargetE.ARROW_RIGHT, InputHexContentE.NA, TargetHexE.OCCUPIED, HasActiveUnitE.NA, CanMoveFurtherE.FALSE, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.SWAP_UNITS_RIGHT } },




        { new Condition(InputTargetE.ATTACK, InputHexContentE.NA, TargetHexE.NA, HasActiveUnitE.NA, CanMoveFurtherE.NA, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY, ResultingActionE.INITIALIZE_COMBAT } },

        { new Condition(InputTargetE.CANCEL_COMBAT, InputHexContentE.NA, TargetHexE.NA, HasActiveUnitE.NA, CanMoveFurtherE.NA, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.SHOW_OVERLAY } },
        { new Condition(InputTargetE.FINISH_COMBAT, InputHexContentE.NA, TargetHexE.NA, HasActiveUnitE.NA, CanMoveFurtherE.NA, MovedThisTurnE.NA), new List<ResultingActionE>() { } },


        { new Condition(InputTargetE.OFF_MAP, InputHexContentE.NA, TargetHexE.NA, HasActiveUnitE.FALSE, CanMoveFurtherE.NA, MovedThisTurnE.NA), new List<ResultingActionE>() { } },
        { new Condition(InputTargetE.OFF_MAP, InputHexContentE.NA, TargetHexE.NA, HasActiveUnitE.TRUE, CanMoveFurtherE.NA, MovedThisTurnE.NA), new List<ResultingActionE>() { ResultingActionE.HIDE_OVERLAY } }

    };

    private static List<KeyValuePair<Condition, List<ResultingActionE>>> rulesList;

    static RuleEngine()
    {
        rulesList = new List<KeyValuePair<Condition, List<ResultingActionE>>>(rules);
    }

    public static List<ResultingActionE> PerformEvaluation(Condition condition)
    {
        /*
        List<ResultingActionE> resultingActionList;
        rules.TryGetValue(condition, out resultingActionList);
        return (resultingActionList);
        */

        foreach (KeyValuePair<Condition, List<ResultingActionE>> kvp in rulesList)
        {
            if (ConditionMatches(condition, kvp.Key))
            {
                return kvp.Value;
            }
        }

        Debug.Log("W zestawie regul nie znaleziono pasujacej do zadanego warunku!");
        return new List<ResultingActionE>();
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
