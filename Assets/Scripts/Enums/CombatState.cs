public enum CombatState
{
    NoCombat,
    AwaitingMoves, //nie przewiduje mozliwosci anulowania, bo to by oglupialo przeciwnika
    AutofilingMoves,
    RevealingMoves,
    AwaitingDefenderCard,
    AwaitingAttackerCard,
    //Dogfight,
    AwaitingDices,
    CombatResolution,
    Finished
}