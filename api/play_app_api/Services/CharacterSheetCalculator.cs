namespace play_app_api.Services;

/// <summary>
/// Service to calculate and fill missing character sheet values using D&D 5e rules
/// </summary>
public static class CharacterSheetCalculator
{
    /// <summary>
    /// Calculate and fill all missing values in a character sheet
    /// </summary>
    public static void CalculateMissingValues(CharacterSheet sheet)
    {
        // First calculate ability modifiers (needed for everything else)
        CalculateAbilityModifiers(sheet);
        
        // Calculate proficiency bonus from level
        var proficiencyBonus = CharacterCalculationService.CalculateProficiencyBonus(sheet.CharacterInfo.Level);
        
        // Calculate combat values
        CalculateCombatValues(sheet, proficiencyBonus);
        
        // Calculate skill values
        CalculateSkillValues(sheet, proficiencyBonus);
        
        // Calculate spellcasting values
        CalculateSpellcastingValues(sheet, proficiencyBonus);
        
        // Calculate equipment/carrying values
        CalculateEquipmentValues(sheet);
        
        // Calculate derived character info
        CalculateCharacterInfoValues(sheet);
    }

    #region Ability Score Calculations

    private static void CalculateAbilityModifiers(CharacterSheet sheet)
    {
        if (sheet.AbilityScores?.Strength != null)
        {
            if (sheet.AbilityScores.Strength.Modifier == 0) // Only if not set
                sheet.AbilityScores.Strength.Modifier = CharacterCalculationService.CalculateAbilityModifier(sheet.AbilityScores.Strength.Score);
        }
        
        if (sheet.AbilityScores?.Dexterity != null)
        {
            if (sheet.AbilityScores.Dexterity.Modifier == 0)
                sheet.AbilityScores.Dexterity.Modifier = CharacterCalculationService.CalculateAbilityModifier(sheet.AbilityScores.Dexterity.Score);
        }
        
        if (sheet.AbilityScores?.Constitution != null)
        {
            if (sheet.AbilityScores.Constitution.Modifier == 0)
                sheet.AbilityScores.Constitution.Modifier = CharacterCalculationService.CalculateAbilityModifier(sheet.AbilityScores.Constitution.Score);
        }
        
        if (sheet.AbilityScores?.Intelligence != null)
        {
            if (sheet.AbilityScores.Intelligence.Modifier == 0)
                sheet.AbilityScores.Intelligence.Modifier = CharacterCalculationService.CalculateAbilityModifier(sheet.AbilityScores.Intelligence.Score);
        }
        
        if (sheet.AbilityScores?.Wisdom != null)
        {
            if (sheet.AbilityScores.Wisdom.Modifier == 0)
                sheet.AbilityScores.Wisdom.Modifier = CharacterCalculationService.CalculateAbilityModifier(sheet.AbilityScores.Wisdom.Score);
        }
        
        if (sheet.AbilityScores?.Charisma != null)
        {
            if (sheet.AbilityScores.Charisma.Modifier == 0)
                sheet.AbilityScores.Charisma.Modifier = CharacterCalculationService.CalculateAbilityModifier(sheet.AbilityScores.Charisma.Score);
        }
    }

    #endregion

    #region Combat Calculations

    private static void CalculateCombatValues(CharacterSheet sheet, int proficiencyBonus)
    {
        if (sheet.Combat == null) return;
        
        var dexMod = sheet.AbilityScores?.Dexterity?.Modifier ?? 0;
        var conMod = sheet.AbilityScores?.Constitution?.Modifier ?? 0;
        
        // Calculate armor class (if not set or is default 10)
        if (sheet.Combat.ArmorClass <= 10)
        {
            sheet.Combat.ArmorClass = CharacterCalculationService.CalculateUnarmoredAC(dexMod);
        }
        
        // Calculate initiative (if not set)
        if (sheet.Combat.Initiative == 0)
        {
            sheet.Combat.Initiative = CharacterCalculationService.CalculateInitiative(dexMod);
        }
        
        // Calculate proficiency bonus (if not set or is default 2)
        if (sheet.Combat.ProficiencyBonus <= 2)
        {
            sheet.Combat.ProficiencyBonus = proficiencyBonus;
        }
        
        // Calculate hit points
        CalculateHitPoints(sheet, conMod);
        
        // Calculate hit dice
        CalculateHitDice(sheet);
        
        // Calculate passive scores
        CalculatePassiveScores(sheet, proficiencyBonus);
        
        // Calculate saving throws
        CalculateSavingThrows(sheet, proficiencyBonus);
    }

    private static void CalculateHitPoints(CharacterSheet sheet, int conMod)
    {
        if (sheet.Combat?.HitPoints == null) return;
        
        var level = sheet.CharacterInfo?.Level ?? 1;
        
        // Calculate max HP if not set or is default 10
        if (sheet.Combat.HitPoints.Max <= 10)
        {
            // Assume d8 hit die for unknown class (most common)
            var hitDieMax = GetHitDieMaxForClass(sheet.CharacterInfo?.Class ?? "");
            
            if (level == 1)
            {
                sheet.Combat.HitPoints.Max = CharacterCalculationService.CalculateLevel1HP(hitDieMax, conMod);
            }
            else
            {
                // Calculate HP for multiclass using average
                var baseHP = CharacterCalculationService.CalculateLevel1HP(hitDieMax, conMod);
                for (int i = 2; i <= level; i++)
                {
                    baseHP = CharacterCalculationService.CalculateHPForLevel(baseHP, hitDieMax, conMod);
                }
                sheet.Combat.HitPoints.Max = baseHP;
            }
        }
        
        // Set current HP to max if not set or is default 10
        if (sheet.Combat.HitPoints.Current <= 10)
        {
            sheet.Combat.HitPoints.Current = sheet.Combat.HitPoints.Max;
        }
        
        // Temporary HP defaults to 0
        // (already defaulted in model)
    }

    private static void CalculateHitDice(CharacterSheet sheet)
    {
        if (sheet.Combat?.HitDice == null) return;
        
        var level = sheet.CharacterInfo?.Level ?? 1;
        var hitDieSize = GetHitDieSizeForClass(sheet.CharacterInfo?.Class ?? "");
        
        // Set total hit dice if not set
        if (string.IsNullOrEmpty(sheet.Combat.HitDice.Total))
        {
            sheet.Combat.HitDice.Total = $"{level}d{hitDieSize}";
        }
        
        // Set current hit dice if not set (assume full)
        if (string.IsNullOrEmpty(sheet.Combat.HitDice.Current))
        {
            sheet.Combat.HitDice.Current = $"{level}d{hitDieSize}";
        }
    }

    private static void CalculatePassiveScores(CharacterSheet sheet, int proficiencyBonus)
    {
        if (sheet.Combat?.PassiveScores == null) return;
        
        var wisMod = sheet.AbilityScores?.Wisdom?.Modifier ?? 0;
        var intMod = sheet.AbilityScores?.Intelligence?.Modifier ?? 0;
        
        // Calculate passive scores if they're default 10
        if (sheet.Combat.PassiveScores.Perception <= 10)
        {
            var perceptionBonus = CharacterCalculationService.CalculateSkillBonus(wisMod, SkillProficiency.None, proficiencyBonus);
            sheet.Combat.PassiveScores.Perception = CharacterCalculationService.CalculatePassiveScore(perceptionBonus);
        }
        
        if (sheet.Combat.PassiveScores.Insight <= 10)
        {
            var insightBonus = CharacterCalculationService.CalculateSkillBonus(wisMod, SkillProficiency.None, proficiencyBonus);
            sheet.Combat.PassiveScores.Insight = CharacterCalculationService.CalculatePassiveScore(insightBonus);
        }
        
        if (sheet.Combat.PassiveScores.Investigation <= 10)
        {
            var investigationBonus = CharacterCalculationService.CalculateSkillBonus(intMod, SkillProficiency.None, proficiencyBonus);
            sheet.Combat.PassiveScores.Investigation = CharacterCalculationService.CalculatePassiveScore(investigationBonus);
        }
    }

    private static void CalculateSavingThrows(CharacterSheet sheet, int proficiencyBonus)
    {
        if (sheet.SavingThrows == null) return;
        
        // Get ability modifiers
        var strMod = sheet.AbilityScores?.Strength?.Modifier ?? 0;
        var dexMod = sheet.AbilityScores?.Dexterity?.Modifier ?? 0;
        var conMod = sheet.AbilityScores?.Constitution?.Modifier ?? 0;
        var intMod = sheet.AbilityScores?.Intelligence?.Modifier ?? 0;
        var wisMod = sheet.AbilityScores?.Wisdom?.Modifier ?? 0;
        var chaMod = sheet.AbilityScores?.Charisma?.Modifier ?? 0;
        
        // For now, assume no proficiencies (would need class data to determine)
        // TODO: Add class-based saving throw proficiencies
        var strSave = CharacterCalculationService.CalculateSavingThrow(strMod, false, proficiencyBonus);
        var dexSave = CharacterCalculationService.CalculateSavingThrow(dexMod, false, proficiencyBonus);
        var conSave = CharacterCalculationService.CalculateSavingThrow(conMod, false, proficiencyBonus);
        var intSave = CharacterCalculationService.CalculateSavingThrow(intMod, false, proficiencyBonus);
        var wisSave = CharacterCalculationService.CalculateSavingThrow(wisMod, false, proficiencyBonus);
        var chaSave = CharacterCalculationService.CalculateSavingThrow(chaMod, false, proficiencyBonus);
        
        // Set saving throws (these would need to be implemented in the model)
        // For now, we'll skip this since the saving throw structure needs to be updated
    }

    #endregion

    #region Skill Calculations

    private static void CalculateSkillValues(CharacterSheet sheet, int proficiencyBonus)
    {
        // Skills would need to be implemented in the Skills model
        // This is a placeholder for when that structure is available
    }

    #endregion

    #region Spellcasting Calculations

    private static void CalculateSpellcastingValues(CharacterSheet sheet, int proficiencyBonus)
    {
        if (sheet.Spellcasting == null) return;
        
        // Get spellcasting ability modifier (would need to determine from class)
        var spellMod = GetSpellcastingModifier(sheet);
        
        // Calculate spell save DC if not set
        if (sheet.Spellcasting.SaveDC == 0)
        {
            sheet.Spellcasting.SaveDC = CharacterCalculationService.CalculateSpellSaveDC(spellMod, proficiencyBonus);
        }
        
        // Calculate spell attack bonus if not set
        if (sheet.Spellcasting.AttackBonus == 0)
        {
            sheet.Spellcasting.AttackBonus = CharacterCalculationService.CalculateSpellAttackBonus(spellMod, proficiencyBonus);
        }
        
        // Calculate spell slots if not set
        CalculateSpellSlots(sheet);
    }

    private static void CalculateSpellSlots(CharacterSheet sheet)
    {
        if (sheet.Spellcasting?.SpellSlots == null) return;
        
        var level = sheet.CharacterInfo?.Level ?? 1;
        var className = sheet.CharacterInfo?.Class ?? "";
        
        // Get spell slot progression for class
        var slots = GetSpellSlotsForClassLevel(className, level);
        
        // Apply to each level if not already set
        if (sheet.Spellcasting.SpellSlots.Level1?.Total == 0)
            sheet.Spellcasting.SpellSlots.Level1.Total = slots[0];
        if (sheet.Spellcasting.SpellSlots.Level2?.Total == 0)
            sheet.Spellcasting.SpellSlots.Level2.Total = slots[1];
        // ... continue for all 9 levels
    }

    #endregion

    #region Equipment Calculations

    private static void CalculateEquipmentValues(CharacterSheet sheet)
    {
        if (sheet.Equipment == null) return;
        
        var strScore = sheet.AbilityScores?.Strength?.Score ?? 10;
        
        // Calculate carrying capacity if not set
        if (sheet.Equipment.CarryingCapacity != null)
        {
            if (sheet.Equipment.CarryingCapacity.Encumbered <= 0)
            {
                sheet.Equipment.CarryingCapacity.Encumbered = CharacterCalculationService.CalculateCarryingCapacity(strScore);
            }
            
            if (sheet.Equipment.CarryingCapacity.PushDragLift <= 0)
            {
                sheet.Equipment.CarryingCapacity.PushDragLift = CharacterCalculationService.CalculatePushDragLift(strScore);
            }
        }
    }

    #endregion

    #region Character Info Calculations

    private static void CalculateCharacterInfoValues(CharacterSheet sheet)
    {
        if (sheet.CharacterInfo == null) return;
        
        // Calculate next level XP if not set or is default 300
        if (sheet.CharacterInfo.NextLevelXp <= 300)
        {
            var nextLevel = sheet.CharacterInfo.Level + 1;
            sheet.CharacterInfo.NextLevelXp = CharacterCalculationService.GetXPThresholdForLevel(nextLevel);
        }
    }

    #endregion

    #region Helper Methods

    private static int GetHitDieMaxForClass(string className)
    {
        return className.ToLower() switch
        {
            "barbarian" => 12,
            "fighter" or "paladin" or "ranger" => 10,
            "artificer" or "bard" or "cleric" or "druid" or "monk" or "rogue" or "warlock" => 8,
            "sorcerer" or "wizard" => 6,
            _ => 8 // Default d8
        };
    }

    private static int GetHitDieSizeForClass(string className)
    {
        return GetHitDieMaxForClass(className);
    }

    private static int GetSpellcastingModifier(CharacterSheet sheet)
    {
        var className = sheet.CharacterInfo?.Class?.ToLower() ?? "";
        
        return className switch
        {
            "cleric" or "druid" or "ranger" => sheet.AbilityScores?.Wisdom?.Modifier ?? 0,
            "paladin" => sheet.AbilityScores?.Charisma?.Modifier ?? 0,
            "sorcerer" or "bard" or "warlock" => sheet.AbilityScores?.Charisma?.Modifier ?? 0,
            "wizard" or "artificer" => sheet.AbilityScores?.Intelligence?.Modifier ?? 0,
            _ => 0 // Non-spellcaster
        };
    }

    private static int[] GetSpellSlotsForClassLevel(string className, int level)
    {
        // Simplified spell slot progression (full casters)
        // Returns array of slots for levels 1-9
        if (IsFullCaster(className))
        {
            return level switch
            {
                1 => [2, 0, 0, 0, 0, 0, 0, 0, 0],
                2 => [3, 0, 0, 0, 0, 0, 0, 0, 0],
                3 => [4, 2, 0, 0, 0, 0, 0, 0, 0],
                4 => [4, 3, 0, 0, 0, 0, 0, 0, 0],
                5 => [4, 3, 2, 0, 0, 0, 0, 0, 0],
                // ... continue full progression
                _ => [0, 0, 0, 0, 0, 0, 0, 0, 0]
            };
        }
        
        return [0, 0, 0, 0, 0, 0, 0, 0, 0]; // No slots for non-casters
    }

    private static bool IsFullCaster(string className)
    {
        return className.ToLower() switch
        {
            "bard" or "cleric" or "druid" or "sorcerer" or "wizard" => true,
            _ => false
        };
    }

    #endregion
}