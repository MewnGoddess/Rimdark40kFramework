# RimDark 40k - Framework

## Rank System



### RankCategoryDef

Has fields:

#### GeneDef unlockedByGene
Gene needed to access rank category

#### HediffDef unlockedByHediff
Hediff needed to access rank category

#### TraitDef unlockedByTrait
Trait needed to access rank category

#### int traitDegree = 0
Trait degree for trait needed, relevant for traits using same def with multiple traits (e.g Jogger and fast walker)

### RankDef

Has fields:

#### string rankIconPath
Path to icon of rank in tree
        
#### RankCategoryDef rankCategory
Rank category which the rank falls under (Missing category will result in rank being unobtainable)

#### List<RankDef> rankRequirements = new List<RankDef>()
Required ranks to select this rank

#### List<Aptitude> requiredSkills = new List<Aptitude>()
Required skills to select this rank (e.g shooting 6)

#### List<GeneDef> requiredGenesAll = new List<GeneDef>()
Required genes to select this rank, pawn must have all these genes to be able to select the rank
        
#### List<GeneDef> requiredGenesOneAmong = new List<GeneDef>()
Required genes to select this rank, pawn must have at least one of these genes to be able to select the rank

#### Dictionary<TraitDef, int> requiredTraitsAll = new Dictionary<TraitDef, int>()
Required traits to select this rank, pawn must have all these traits to be able to select the rank
        
#### Dictionary<TraitDef, int> requiredTraitsOneAmong = new Dictionary<TraitDef, int>()
Required traits to select this rank, pawn must have at least one of these trait to be able to select the rank

#### List<StatModifier> statOffsets = new List<StatModifier>()
As with a lot of other defs, gives these statOffsets to the pawn when the rank is unlocked

#### List<StatModifier> statFactors = new List<StatModifier>()
As with a lot of other defs, gives these statFactors to the pawn when the rank is unlocked

#### List<ConditionalStatAffecter> conditionalStatAffecters = new List<ConditionalStatAffecter>()
As with genes, gives these ConditionalStatAffecter to the pawn when the rank is unlocked
        
#### List<AbilityDef> givesAbilities = new List<AbilityDef>()
Gives these abilities when the rank is unlocked.

#### List<VFECore.Abilities.AbilityDef> givesVFEAbilities = new List<VFECore.Abilities.AbilityDef>()
Gives these abilities when the rank is unlocked. (VFE ability def is different from Vanilla ability def and as such has its own list)

#### Vector2 displayPosition;
Display position of the rank on the rank tree. (0, 0) corresponds to the middel of the tree all the way to the left. The tree is non scrollable so at some point
you will reach how far to either direction you can go. This means x < 0 should not be done.

#### bool defaultFirstRank = false;
When you open the rank view, it will default to this rank of the selected category

#### Vector2 colonyLimitOfRank = new Vector2(-1, -1)
A limit on how many pawns of the colony may have this rank.
x is the initial number available (-1 here will disable this)
y is how many pawns is needed to increase this limit by 1 (-1 here will disable increase, but may still be limited by x)