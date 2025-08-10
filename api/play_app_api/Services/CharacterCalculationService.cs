namespace play_app_api.Services;

/// <summary>
/// Service for calculating derived D&D 5e character statistics using official rules
/// </summary>
public static class CharacterCalculationService
{
    #region Core Calculations

    /// <summary>
    /// Calculate ability modifier: floor((Score - 10) / 2)
    /// </summary>
    public static int CalculateAbilityModifier(int abilityScore)
    {
        return (int)Math.Floor((abilityScore - 10) / 2.0);
    }

    /// <summary>
    /// Calculate proficiency bonus by character level
    /// Lvl 1–4: +2 | 5–8: +3 | 9–12: +4 | 13–16: +5 | 17–20: +6
    /// </summary>
    public static int CalculateProficiencyBonus(int level)
    {
        return level switch
        {
            >= 1 and <= 4 => 2,
            >= 5 and <= 8 => 3,
            >= 9 and <= 12 => 4,
            >= 13 and <= 16 => 5,
            >= 17 and <= 20 => 6,
            _ => 2 // Default for invalid levels
        };
    }

    #endregion

    #region Combat Calculations

    /// <summary>
    /// Calculate unarmored AC: 10 + DexMod + Misc
    /// </summary>
    public static int CalculateUnarmoredAC(int dexterityModifier, int miscBonus = 0)
    {
        return 10 + dexterityModifier + miscBonus;
    }

    /// <summary>
    /// Calculate initiative: DexMod + Misc
    /// </summary>
    public static int CalculateInitiative(int dexterityModifier, int miscBonus = 0)
    {
        return dexterityModifier + miscBonus;
    }

    /// <summary>
    /// Calculate saving throw bonus: AbilityMod + (Proficient? Prof : 0) + Misc
    /// </summary>
    public static int CalculateSavingThrow(int abilityModifier, bool isProficient, int proficiencyBonus, int miscBonus = 0)
    {
        return abilityModifier + (isProficient ? proficiencyBonus : 0) + miscBonus;
    }

    /// <summary>
    /// Calculate passive score: 10 + SkillBonus + Misc
    /// </summary>
    public static int CalculatePassiveScore(int skillBonus, int miscBonus = 0)
    {
        return 10 + skillBonus + miscBonus;
    }

    #endregion

    #region Hit Points & Dice

    /// <summary>
    /// Calculate level 1 HP: HitDieMax + ConMod
    /// </summary>
    public static int CalculateLevel1HP(int hitDieMax, int constitutionModifier)
    {
        return hitDieMax + constitutionModifier;
    }

    /// <summary>
    /// Calculate HP for higher levels using average: Previous + AvgHitDie + ConMod
    /// </summary>
    public static int CalculateHPForLevel(int previousHP, int hitDieSize, int constitutionModifier)
    {
        int avgHitDie = hitDieSize switch
        {
            6 => 4,   // d6
            8 => 5,   // d8
            10 => 6,  // d10
            12 => 7,  // d12
            _ => 5    // Default d8
        };
        return previousHP + avgHitDie + constitutionModifier;
    }

    /// <summary>
    /// Calculate total hit dice pool: equals character level
    /// </summary>
    public static int CalculateTotalHitDice(int level)
    {
        return level;
    }

    #endregion

    #region Skills & Proficiencies

    /// <summary>
    /// Calculate skill bonus: RelevantAbilityMod + ProficiencyTier + Misc
    /// ProficiencyTier = 0 (none) | Prof (proficient) | 2*Prof (expertise) | 0.5*Prof (jack-of-all-trades, round down)
    /// </summary>
    public static int CalculateSkillBonus(int abilityModifier, SkillProficiency proficiency, int proficiencyBonus, int miscBonus = 0)
    {
        int proficiencyTier = proficiency switch
        {
            SkillProficiency.None => 0,
            SkillProficiency.Proficient => proficiencyBonus,
            SkillProficiency.Expertise => 2 * proficiencyBonus,
            SkillProficiency.JackOfAllTrades => (int)Math.Floor(proficiencyBonus / 2.0),
            _ => 0
        };
        
        return abilityModifier + proficiencyTier + miscBonus;
    }

    #endregion

    #region Attacks & Spellcasting

    /// <summary>
    /// Calculate weapon attack bonus: AbilityMod + (Proficient? Prof : 0) + Misc
    /// </summary>
    public static int CalculateAttackBonus(int abilityModifier, bool isProficient, int proficiencyBonus, int miscBonus = 0)
    {
        return abilityModifier + (isProficient ? proficiencyBonus : 0) + miscBonus;
    }

    /// <summary>
    /// Calculate spell attack bonus: SpellAbilityMod + Prof + Misc
    /// </summary>
    public static int CalculateSpellAttackBonus(int spellcastingAbilityModifier, int proficiencyBonus, int miscBonus = 0)
    {
        return spellcastingAbilityModifier + proficiencyBonus + miscBonus;
    }

    /// <summary>
    /// Calculate spell save DC: 8 + Prof + SpellAbilityMod + Misc
    /// </summary>
    public static int CalculateSpellSaveDC(int spellcastingAbilityModifier, int proficiencyBonus, int miscBonus = 0)
    {
        return 8 + proficiencyBonus + spellcastingAbilityModifier + miscBonus;
    }

    /// <summary>
    /// Calculate prepared spells: SpellcastingAbilityMod + CasterLevel (min 1)
    /// </summary>
    public static int CalculatePreparedSpells(int spellcastingAbilityModifier, int casterLevel)
    {
        return Math.Max(1, spellcastingAbilityModifier + casterLevel);
    }

    #endregion

    #region Movement & Carrying

    /// <summary>
    /// Calculate carrying capacity: 15 × StrengthScore (in pounds)
    /// </summary>
    public static int CalculateCarryingCapacity(int strengthScore)
    {
        return 15 * strengthScore;
    }

    /// <summary>
    /// Calculate push/drag/lift capacity: 30 × StrengthScore (in pounds)
    /// </summary>
    public static int CalculatePushDragLift(int strengthScore)
    {
        return 30 * strengthScore;
    }

    /// <summary>
    /// Calculate long jump distance with running start: StrengthScore ft
    /// </summary>
    public static int CalculateLongJump(int strengthScore, bool runningStart = true)
    {
        return runningStart ? strengthScore : (int)Math.Floor(strengthScore / 2.0);
    }

    /// <summary>
    /// Calculate high jump height with running start: 3 + StrMod ft
    /// </summary>
    public static int CalculateHighJump(int strengthModifier, bool runningStart = true)
    {
        int height = 3 + strengthModifier;
        return runningStart ? height : (int)Math.Floor(height / 2.0);
    }

    #endregion

    #region XP & Leveling

    /// <summary>
    /// Get XP threshold for next level (5e table)
    /// </summary>
    public static int GetXPThresholdForLevel(int level)
    {
        return level switch
        {
            1 => 0,
            2 => 300,
            3 => 900,
            4 => 2700,
            5 => 6500,
            6 => 14000,
            7 => 23000,
            8 => 34000,
            9 => 48000,
            10 => 64000,
            11 => 85000,
            12 => 100000,
            13 => 120000,
            14 => 140000,
            15 => 165000,
            16 => 195000,
            17 => 225000,
            18 => 265000,
            19 => 305000,
            20 => 355000,
            _ => 355000 // Cap at level 20
        };
    }

    #endregion
}

/// <summary>
/// Skill proficiency levels for calculations
/// </summary>
public enum SkillProficiency
{
    None = 0,
    JackOfAllTrades = 1,
    Proficient = 2,
    Expertise = 3
}