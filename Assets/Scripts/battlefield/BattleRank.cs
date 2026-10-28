
public enum BattleSide
{
    Heroes,
    Enemies
}

public readonly struct BattleRank
{
    public BattleSide Side { get; }
    public int Index { get; }

    public BattleRank(BattleSide side, int index)
    {
        Side = side;
        Index = index;
    }

    public bool IsValid => Index >= 1 && Index <= 4;
}
