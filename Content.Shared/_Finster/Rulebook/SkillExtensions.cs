// Content.Shared._Finster.Rulebook/SkillExtensions.cs
using Content.Shared._Finster.Rulebook;

public static class SkillExtensions
{
    public static SkillDifficulty GetDifficulty(this SkillType skill)
        => skill switch
        {
            SkillType.FirstAid => SkillDifficulty.Easy,
            SkillType.Diagnosis => SkillDifficulty.Average,
            SkillType.Surgery => SkillDifficulty.VeryHard,
            SkillType.Electrician => SkillDifficulty.Easy,
            SkillType.Mechanic => SkillDifficulty.Average,
            SkillType.Housekeeping => SkillDifficulty.Easy,
            _ => SkillDifficulty.Average // Default case
        };

    public static AttributeType GetBaseAttribute(this SkillType skill)
        => skill switch
        {
            SkillType.FirstAid => AttributeType.Intelligence,
            SkillType.Diagnosis => AttributeType.Intelligence,
            SkillType.Surgery => AttributeType.Intelligence,
            SkillType.Electrician => AttributeType.Intelligence,
            SkillType.Mechanic => AttributeType.Intelligence,
            SkillType.Housekeeping => AttributeType.Intelligence,
            _ => AttributeType.Intelligence // Default case
        };
}