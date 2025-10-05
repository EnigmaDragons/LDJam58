using System.Collections.Generic;

public static class TagSynergies
{
    public static readonly List<TagSynergy> All = new List<TagSynergy>
    {
        new TagSynergy { Tag1 = ExhibitTag.Theme_Beast, Tag2 = ExhibitTag.Theme_Beast, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Botanical, Tag2 = ExhibitTag.Theme_Botanical, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Fantasy, Tag2 = ExhibitTag.Theme_Fantasy, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Gothic, Tag2 = ExhibitTag.Theme_Gothic, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Invention, Tag2 = ExhibitTag.Theme_Beast, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Invention, Tag2 = ExhibitTag.Theme_Invention, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Arcane, Tag2 = ExhibitTag.Theme_Invention, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Arcane, Tag2 = ExhibitTag.Theme_Arcane, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Model, Tag2 = ExhibitTag.Theme_Botanical, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Model, Tag2 = ExhibitTag.Theme_Model, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Modern, Tag2 = ExhibitTag.Theme_Beast, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Modern, Tag2 = ExhibitTag.Theme_Fantasy, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Modern, Tag2 = ExhibitTag.Theme_Arcane, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Modern, Tag2 = ExhibitTag.Theme_Modern, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Pirate, Tag2 = ExhibitTag.Theme_Gothic, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Pirate, Tag2 = ExhibitTag.Theme_Pirate, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Space, Tag2 = ExhibitTag.Theme_Beast, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Space, Tag2 = ExhibitTag.Theme_Botanical, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Space, Tag2 = ExhibitTag.Theme_Space, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Steampunk, Tag2 = ExhibitTag.Theme_Fantasy, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Steampunk, Tag2 = ExhibitTag.Theme_Modern, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Steampunk, Tag2 = ExhibitTag.Theme_Pirate, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Steampunk, Tag2 = ExhibitTag.Theme_Steampunk, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Treasure, Tag2 = ExhibitTag.Theme_Invention, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Treasure, Tag2 = ExhibitTag.Theme_Space, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Treasure, Tag2 = ExhibitTag.Theme_Treasure, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Warfare, Tag2 = ExhibitTag.Theme_Botanical, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Warfare, Tag2 = ExhibitTag.Theme_Warfare, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Scripts, Tag2 = ExhibitTag.Theme_Scripts, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Earth, Tag2 = ExhibitTag.Theme_Invention, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Earth, Tag2 = ExhibitTag.Theme_Earth, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Lightning, Tag2 = ExhibitTag.Theme_Treasure, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Lightning, Tag2 = ExhibitTag.Theme_Lightning, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Fire, Tag2 = ExhibitTag.Theme_Botanical, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Fire, Tag2 = ExhibitTag.Theme_Treasure, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Fire, Tag2 = ExhibitTag.Theme_Scripts, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Fire, Tag2 = ExhibitTag.Theme_Fire, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Light, Tag2 = ExhibitTag.Theme_Gothic, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Light, Tag2 = ExhibitTag.Theme_Space, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Light, Tag2 = ExhibitTag.Theme_Light, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Shadow, Tag2 = ExhibitTag.Theme_Invention, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Shadow, Tag2 = ExhibitTag.Theme_Light, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Shadow, Tag2 = ExhibitTag.Theme_Shadow, SynergyValue = 1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Ice, Tag2 = ExhibitTag.Theme_Botanical, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Ice, Tag2 = ExhibitTag.Theme_Scripts, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Ice, Tag2 = ExhibitTag.Theme_Fire, SynergyValue = -1 },
        new TagSynergy { Tag1 = ExhibitTag.Theme_Ice, Tag2 = ExhibitTag.Theme_Ice, SynergyValue = 1 },
    };
}

public class TagSynergy
{
    public ExhibitTag Tag1;
    public ExhibitTag Tag2;
    public int SynergyValue;
}