using System.Collections.Generic;

public static class RoomPool
{
    public static List<RoomType> All = new List<RoomType>
    {
        MakeRoom("A Room of Ice & Fire", new ExhibitTag[] { ExhibitTag.Theme_Ice, ExhibitTag.Theme_Ice, ExhibitTag.Theme_Ice, ExhibitTag.Theme_Fire, ExhibitTag.Theme_Fire, ExhibitTag.Theme_Fire }, 2),
        MakeRoom("Dark & Light", new ExhibitTag[] { ExhibitTag.Theme_Light, ExhibitTag.Theme_Light, ExhibitTag.Theme_Light, ExhibitTag.Theme_Shadow, ExhibitTag.Theme_Shadow, ExhibitTag.Theme_Shadow }, 2),
        MakeRoom("Elemental Wonder", new ExhibitTag[] { ExhibitTag.Theme_Fire, ExhibitTag.Theme_Ice, ExhibitTag.Theme_Lightning, ExhibitTag.Theme_Earth, ExhibitTag.Theme_Shadow, ExhibitTag.Theme_Light }, 2),
        MakeRoom("Through The Ages", new ExhibitTag[] { ExhibitTag.Theme_Fantasy, ExhibitTag.Theme_Fantasy, ExhibitTag.Theme_Steampunk, ExhibitTag.Theme_Steampunk, ExhibitTag.Theme_Modern, ExhibitTag.Theme_Modern }, 2),
        MakeRoom("Living Nature", new ExhibitTag[] { ExhibitTag.Theme_Beast, ExhibitTag.Theme_Beast, ExhibitTag.Theme_Beast, ExhibitTag.Theme_Botanical, ExhibitTag.Theme_Botanical, ExhibitTag.Theme_Botanical }, 2),
        MakeRoom("Wizarding World", new ExhibitTag[] { ExhibitTag.Theme_Arcane, ExhibitTag.Theme_Arcane, ExhibitTag.Theme_Arcane, ExhibitTag.Theme_Fantasy, ExhibitTag.Theme_Fantasy, ExhibitTag.Theme_Fantasy }, 2),
        MakeRoom("Coven", new ExhibitTag[] { ExhibitTag.Theme_Gothic, ExhibitTag.Theme_Gothic, ExhibitTag.Theme_Gothic, ExhibitTag.Theme_Arcane, ExhibitTag.Theme_Arcane, ExhibitTag.Theme_Shadow, ExhibitTag.Theme_Shadow }, 2),
        MakeRoom("Plunder", new ExhibitTag[] { ExhibitTag.Theme_Pirate, ExhibitTag.Theme_Pirate, ExhibitTag.Theme_Pirate, ExhibitTag.Theme_Treasure, ExhibitTag.Theme_Treasure, ExhibitTag.Theme_Treasure }, 2),
        MakeRoom("Energy & Power", new ExhibitTag[] { ExhibitTag.Theme_Invention, ExhibitTag.Theme_Invention, ExhibitTag.Theme_Invention, ExhibitTag.Theme_Lightning, ExhibitTag.Theme_Lightning, ExhibitTag.Theme_Lightning }, 2),
        MakeRoom("Natural World", new ExhibitTag[] { ExhibitTag.Theme_Earth, ExhibitTag.Theme_Earth, ExhibitTag.Theme_Earth, ExhibitTag.Theme_Botanical, ExhibitTag.Theme_Botanical, ExhibitTag.Theme_Botanical }, 2),
        MakeRoom("Frontiers of Discovery", new ExhibitTag[] { ExhibitTag.Theme_Modern, ExhibitTag.Theme_Modern, ExhibitTag.Theme_Modern, ExhibitTag.Theme_Space, ExhibitTag.Theme_Space, ExhibitTag.Theme_Space }, 2),
        MakeRoom("Dark Library", new ExhibitTag[] { ExhibitTag.Theme_Scripts, ExhibitTag.Theme_Scripts, ExhibitTag.Theme_Scripts, ExhibitTag.Theme_Gothic, ExhibitTag.Theme_Gothic, ExhibitTag.Theme_Shadow, ExhibitTag.Theme_Shadow }, 2),
        MakeRoom("Great Wars", new ExhibitTag[] { ExhibitTag.Theme_Model, ExhibitTag.Theme_Model, ExhibitTag.Theme_Model, ExhibitTag.Theme_Warfare, ExhibitTag.Theme_Warfare, ExhibitTag.Theme_Warfare }, 2),
        MakeRoom("Age of Innovation", new ExhibitTag[] { ExhibitTag.Theme_Steampunk, ExhibitTag.Theme_Steampunk, ExhibitTag.Theme_Steampunk, ExhibitTag.Theme_Invention, ExhibitTag.Theme_Invention, ExhibitTag.Theme_Invention }, 2),
        MakeRoom("Fantasy Warfare", new ExhibitTag[] { ExhibitTag.Theme_Fantasy, ExhibitTag.Theme_Fantasy, ExhibitTag.Theme_Fantasy, ExhibitTag.Theme_Warfare, ExhibitTag.Theme_Warfare, ExhibitTag.Theme_Warfare }, 2),
        MakeRoom("World Wonders", new ExhibitTag[] { ExhibitTag.Theme_Fantasy, ExhibitTag.Theme_Fantasy, ExhibitTag.Theme_Fantasy, ExhibitTag.Theme_Model, ExhibitTag.Theme_Model, ExhibitTag.Theme_Model }, 2),

        // Non-adjacency rooms (multiplier 3, no adjacency bonus)
        MakeRoom("Mythical Beasts", new ExhibitTag[] { ExhibitTag.Theme_Beast, ExhibitTag.Theme_Beast, ExhibitTag.Theme_Beast, ExhibitTag.Theme_Beast, ExhibitTag.Theme_Beast }, 3),
        MakeRoom("Exotic Plants", new ExhibitTag[] { ExhibitTag.Theme_Botanical, ExhibitTag.Theme_Botanical, ExhibitTag.Theme_Botanical, ExhibitTag.Theme_Botanical, ExhibitTag.Theme_Botanical }, 3),
        MakeRoom("Times of Fantasy", new ExhibitTag[] { ExhibitTag.Theme_Fantasy, ExhibitTag.Theme_Fantasy, ExhibitTag.Theme_Fantasy, ExhibitTag.Theme_Fantasy, ExhibitTag.Theme_Fantasy }, 3),
        MakeRoom("Gothic", new ExhibitTag[] { ExhibitTag.Theme_Gothic, ExhibitTag.Theme_Gothic, ExhibitTag.Theme_Gothic, ExhibitTag.Theme_Gothic, ExhibitTag.Theme_Gothic }, 3),
        MakeRoom("Engineering Marvels", new ExhibitTag[] { ExhibitTag.Theme_Invention, ExhibitTag.Theme_Invention, ExhibitTag.Theme_Invention, ExhibitTag.Theme_Invention, ExhibitTag.Theme_Invention }, 3),
        MakeRoom("The Arcanum",     new ExhibitTag[] { ExhibitTag.Theme_Arcane, ExhibitTag.Theme_Arcane, ExhibitTag.Theme_Arcane, ExhibitTag.Theme_Arcane, ExhibitTag.Theme_Arcane }, 3),
        MakeRoom("The Model Gallery", new ExhibitTag[] { ExhibitTag.Theme_Model, ExhibitTag.Theme_Model, ExhibitTag.Theme_Model, ExhibitTag.Theme_Model, ExhibitTag.Theme_Model }, 3),
        MakeRoom("Modern Marvels", new ExhibitTag[] { ExhibitTag.Theme_Modern, ExhibitTag.Theme_Modern, ExhibitTag.Theme_Modern, ExhibitTag.Theme_Modern, ExhibitTag.Theme_Modern }, 3),
        MakeRoom("Pirates", new ExhibitTag[] { ExhibitTag.Theme_Pirate, ExhibitTag.Theme_Pirate, ExhibitTag.Theme_Pirate, ExhibitTag.Theme_Pirate, ExhibitTag.Theme_Pirate }, 3),
        MakeRoom("Space Exploration", new ExhibitTag[] { ExhibitTag.Theme_Space, ExhibitTag.Theme_Space, ExhibitTag.Theme_Space, ExhibitTag.Theme_Space, ExhibitTag.Theme_Space }, 3),
        MakeRoom("Age of Steam", new ExhibitTag[] { ExhibitTag.Theme_Steampunk, ExhibitTag.Theme_Steampunk, ExhibitTag.Theme_Steampunk, ExhibitTag.Theme_Steampunk, ExhibitTag.Theme_Steampunk }, 3),
        MakeRoom("Treasures of History", new ExhibitTag[] { ExhibitTag.Theme_Treasure, ExhibitTag.Theme_Treasure, ExhibitTag.Theme_Treasure, ExhibitTag.Theme_Treasure, ExhibitTag.Theme_Treasure }, 3),
        MakeRoom("War", new ExhibitTag[] { ExhibitTag.Theme_Warfare, ExhibitTag.Theme_Warfare, ExhibitTag.Theme_Warfare, ExhibitTag.Theme_Warfare, ExhibitTag.Theme_Warfare }, 3),
        MakeRoom("The Scriptorium", new ExhibitTag[] { ExhibitTag.Theme_Scripts, ExhibitTag.Theme_Scripts, ExhibitTag.Theme_Scripts, ExhibitTag.Theme_Scripts, ExhibitTag.Theme_Scripts }, 3),
        MakeRoom("The Gemstone Gallery", new ExhibitTag[] { ExhibitTag.Theme_Earth, ExhibitTag.Theme_Earth, ExhibitTag.Theme_Earth, ExhibitTag.Theme_Earth, ExhibitTag.Theme_Earth }, 3),
        MakeRoom("Shocking Discoveries", new ExhibitTag[] { ExhibitTag.Theme_Lightning, ExhibitTag.Theme_Lightning, ExhibitTag.Theme_Lightning, ExhibitTag.Theme_Lightning, ExhibitTag.Theme_Lightning }, 3),
        MakeRoom("Burning Curiousities", new ExhibitTag[] { ExhibitTag.Theme_Fire, ExhibitTag.Theme_Fire, ExhibitTag.Theme_Fire, ExhibitTag.Theme_Fire, ExhibitTag.Theme_Fire }, 3),
        MakeRoom("Hall of Luminance", new ExhibitTag[] { ExhibitTag.Theme_Light, ExhibitTag.Theme_Light, ExhibitTag.Theme_Light, ExhibitTag.Theme_Light, ExhibitTag.Theme_Light }, 3),
        MakeRoom("Hall of Shadows", new ExhibitTag[] { ExhibitTag.Theme_Shadow, ExhibitTag.Theme_Shadow, ExhibitTag.Theme_Shadow, ExhibitTag.Theme_Shadow, ExhibitTag.Theme_Shadow }, 3),
        MakeRoom("Frozen Exhibits", new ExhibitTag[] { ExhibitTag.Theme_Ice, ExhibitTag.Theme_Ice, ExhibitTag.Theme_Ice, ExhibitTag.Theme_Ice, ExhibitTag.Theme_Ice }, 3)
    };

    private static RoomType MakeRoom(string name, ExhibitTag[] req, int mult)
    {
        return new RoomType {
            Name = name,
            Requirement = req,
            Multiplier = mult
        };
    }
}
