// Content.Shared._Finster.Rulebook/SkillAttributes.cs
using Content.Shared._Finster.Rulebook;

[AttributeUsage(AttributeTargets.Field)]
public sealed class SkillDifficultyAttribute : Attribute
{
    public SkillDifficulty Difficulty { get; }
    public SkillDifficultyAttribute(SkillDifficulty difficulty) => Difficulty = difficulty;
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class BaseAttributeAttribute : Attribute
{
    public AttributeType Attribute { get; }
    public BaseAttributeAttribute(AttributeType attribute) => Attribute = attribute;
}