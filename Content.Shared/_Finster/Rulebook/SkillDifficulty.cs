namespace Content.Shared._Finster.Rulebook;

public enum SkillDifficulty
{
    Easy,       // 1: +0  | 2: +2 | +1 per 2 levels after
    Average,    // 1: -1  | 2: +0 | +1 per 1 level after
    Hard,       // 1: -2  | 2: -1 | +1 per 1 level after
    VeryHard    // 1: -3  | 2: -2 | +1 per 1 level after
}

[AttributeUsage(AttributeTargets.Field)] // Only valid on enum fields
public sealed class SkillDifficultyAttribute : Attribute
{
    public SkillDifficulty Difficulty { get; }

    public SkillDifficultyAttribute(SkillDifficulty difficulty)
    {
        Difficulty = difficulty;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class BaseAttributeAttribute : Attribute
{
    public AttributeType Attribute { get; }

    public BaseAttributeAttribute(AttributeType attribute)
    {
        Attribute = attribute;
    }
}