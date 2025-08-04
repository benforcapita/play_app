// src/app/core/models/character.models.ts

/** ---------- Root types ---------- */
export interface Character {
    id?: number;
    name: string;
    /** NOTE: property name "class" is valid in TS, keep it to match API */
    class: string;
    species: string;
    /** Some API records may omit 'level' (backend defaulting). Keep it optional. */
    level?: number;
    sheet: CharacterSheet;
  }
  
  export interface CharacterSheet {
    characterInfo: CharacterInfo;
    appearance: Appearance;
    abilityScores: AbilityScores;
    savingThrows: SavingThrows;
    skills: Skill[];
    combat: Combat;
    proficiencies: Proficiencies;
    featuresAndTraits: FeatureTrait[];
    equipment: Equipment;
    spellcasting: Spellcasting;
    persona: Persona;
    backstory: Backstory;
  }
  
  /** ---------- Template 1 ---------- */
  export interface CharacterInfo {
    characterName: string;
    playerName: string;
    classAndLevel: string;
    species: string;
    background: string;
    experiencePoints: string;
    alignment: string;
  }
  
  export interface Appearance {
    size: string;
    gender: string;
    age: string;
    height: string;
    weight: string;
    skin: string;
    eyes: string;
    hair: string;
  }
  
  /** ---------- Template 2 ---------- */
  export interface AbilityScores {
    strength: AbilityScore;
    dexterity: AbilityScore;
    constitution: AbilityScore;
    intelligence: AbilityScore;
    wisdom: AbilityScore;
    charisma: AbilityScore;
  }
  export interface AbilityScore { score: number; modifier: number; }
  
  export interface SavingThrows {
    strength: ProficiencyFlag;
    dexterity: ProficiencyFlag;
    constitution: ProficiencyFlag;
    intelligence: ProficiencyFlag;
    wisdom: ProficiencyFlag;
    charisma: ProficiencyFlag;
  }
  export interface ProficiencyFlag { proficient: boolean; }
  
  /** ---------- Template 3 ---------- */
  export interface Skill {
    name: string;     // e.g., "Acrobatics"
    ability: string;  // e.g., "DEX" (your data currently has "", we can fill later)
    proficient: boolean;
    expert: boolean;
  }
  
  /** ---------- Template 4 ---------- */
  export interface Combat {
    armorClass: number;
    initiative: number;
    speed: string;
    proficiencyBonus: number;
    inspiration: boolean;
    hitPoints: HitPoints;
    hitDice: HitDice;
    deathSaves: DeathSaves;
    passiveScores: PassiveScores;
  }
  export interface HitPoints { max: number; current: number; temporary: number; }
  export interface HitDice { total: string; current: string; }
  export interface DeathSaves { successes: number; failures: number; }
  export interface PassiveScores { perception: number; insight: number; investigation: number; }
  
  /** ---------- Template 5 ---------- */
  export interface Proficiencies {
    armor: string[];
    weapons: string[];
    tools: string[];
    languages: string[];
  }
  
  /** ---------- Template 6 ---------- */
  export interface FeatureTrait {
    name: string;
    source: string;
    description: string;
    uses: string;
    action: string;
  }
  
  /** ---------- Template 7 ---------- */
  export interface Equipment {
    items: Item[];
    currency: Currency;
    carryingCapacity: CarryingCapacity;
  }
  export interface Item { name: string; quantity: number; weight: number; }
  export interface Currency { cp: number; sp: number; ep: number; gp: number; pp: number; }
  export interface CarryingCapacity { weightCarried: number; encumbered: number; pushDragLift: number; }
  
  /** ---------- Template 8 + 9 ---------- */
  export interface Spellcasting {
    class: string;
    ability: string;
    saveDC: number;
    attackBonus: number;
    spellSlots: SpellSlots;
    cantrips: Spell[];
    spellsKnown: Spell[];
  }
  export interface SpellSlots {
    level1: Slot; level2: Slot; level3: Slot; level4: Slot; level5: Slot;
    level6: Slot; level7: Slot; level8: Slot; level9: Slot;
  }
  export interface Slot { total: number; used: number; }
  
  export interface Spell {
    name: string;
    level: number;         // 0 for cantrip
    source: string;
    castingTime: string;
    range: string;
    components: string;    // "V, S, M"
    duration: string;
    description: string;
    attack: SpellAttack;
    save: SpellSave;
  }
  export interface SpellAttack { bonus: number; damage: string; }
  export interface SpellSave { ability: string; dc: number; }
  
  /** ---------- Template 10 ---------- */
  export interface Persona {
    personalityTraits: string;
    ideals: string;
    bonds: string;
    flaws: string;
  }
  export interface Backstory {
    alliesAndOrganizations: string;
    characterBackstory: string;
    additionalNotes: string;
  }
  
  /** ---------- Helpers ---------- */
  
  /** Narrowed union of valid section keys for /sheet/{section} endpoint */
  export type SheetSectionKey =
    | 'characterinfo' | 'appearance' | 'abilityscores' | 'savingthrows' | 'skills'
    | 'combat' | 'proficiencies' | 'featuresandtraits' | 'equipment'
    | 'spellcasting' | 'persona' | 'backstory';
  
  /** Factory with default values you can reuse in forms or when normalizing. */
  export function createEmptySheet(): CharacterSheet {
    const zero = (s = 10) => ({ score: s, modifier: Math.floor((s - 10) / 2) });
    return {
      characterInfo: {
        characterName: '', playerName: '', classAndLevel: '', species: '',
        background: '', experiencePoints: '', alignment: ''
      },
      appearance: { size: '', gender: '', age: '', height: '', weight: '', skin: '', eyes: '', hair: '' },
      abilityScores: {
        strength: zero(), dexterity: zero(), constitution: zero(),
        intelligence: zero(), wisdom: zero(), charisma: zero()
      },
      savingThrows: {
        strength: { proficient: false }, dexterity: { proficient: false },
        constitution: { proficient: false }, intelligence: { proficient: false },
        wisdom: { proficient: false }, charisma: { proficient: false }
      },
      skills: [],
      combat: {
        armorClass: 10, initiative: 0, speed: '30 ft.',
        proficiencyBonus: 2, inspiration: false,
        hitPoints: { max: 10, current: 10, temporary: 0 },
        hitDice: { total: '', current: '' },
        deathSaves: { successes: 0, failures: 0 },
        passiveScores: { perception: 10, insight: 10, investigation: 10 }
      },
      proficiencies: { armor: [], weapons: [], tools: [], languages: [] },
      featuresAndTraits: [],
      equipment: {
        items: [], currency: { cp: 0, sp: 0, ep: 0, gp: 0, pp: 0 },
        carryingCapacity: { weightCarried: 0, encumbered: 0, pushDragLift: 0 }
      },
      spellcasting: {
        class: '', ability: '', saveDC: 0, attackBonus: 0,
        spellSlots: {
          level1: { total: 0, used: 0 }, level2: { total: 0, used: 0 },
          level3: { total: 0, used: 0 }, level4: { total: 0, used: 0 },
          level5: { total: 0, used: 0 }, level6: { total: 0, used: 0 },
          level7: { total: 0, used: 0 }, level8: { total: 0, used: 0 },
          level9: { total: 0, used: 0 },
        },
        cantrips: [], spellsKnown: []
      },
      persona: { personalityTraits: '', ideals: '', bonds: '', flaws: '' },
      backstory: { alliesAndOrganizations: '', characterBackstory: '', additionalNotes: '' }
    };
  }
  
  export function createEmptyCharacter(): Character {
    return {
      name: '', class: '', species: '', level: 1,
      sheet: createEmptySheet()
    };
  }

  /** ---------- Extraction Models ---------- */

  export interface ExtractionJobResponse {
    jobToken: string;
    message: string;
  }

  export interface SectionResult {
    sectionName: string;
    isSuccessful: boolean;
    errorMessage?: string;
    processedAt?: string;
  }

  export interface ExtractionJobStatus {
    jobToken: string;
    status: 'pending' | 'running' | 'completed' | 'failed';
    createdAt: string;
    startedAt?: string;
    completedAt?: string;
    isSuccessful?: boolean;
    errorMessage?: string;
    sectionResults: SectionResult[];
    character?: Character;
  }

  export interface JobSummary {
    isSuccessful: boolean;
    successRate: number;
    successfulSections: number;
    totalSections: number;
    completedAt?: string;
  }

  export interface ExtractionResult {
    character: Character;
    jobSummary: JobSummary;
  }