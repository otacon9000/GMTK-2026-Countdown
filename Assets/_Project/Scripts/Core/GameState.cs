namespace GmtkCountdown
{
    /// <summary>
    /// High-level states the game can be in during a single play session.
    /// The GameManager drives transitions between these; other systems react.
    /// </summary>
    public enum GameState
    {
        Countdown,
        Interruption,
        ComboResolution,
        TaskCompleted,
        GameOver,
        Break
    }
}
