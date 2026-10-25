using System;

public enum EffectType
{
    ModifyHP, GainGold, EndTurn, ExtraAttackDice, ExtraDefenseDice,
    SpawnWanderingMonster, SpawnByTable, DamageArea, Paralyze,
    SkipNextTurn, Teleport, DisarmTrap
}

public enum EffectTarget { Self, Target, AllInArea }
public enum AreaShape { None, Line, Room }

[Serializable]
public class CardEffectData
{
    public EffectType type;
    public int value;
    public EffectTarget target = EffectTarget.Self;
    public AreaShape area = AreaShape.None;
    public bool consumeOnNextRoll;
}