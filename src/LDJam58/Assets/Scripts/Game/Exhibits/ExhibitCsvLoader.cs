using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Assets.Scripts;

public static class ExhibitCsvLoader
{
    public static List<ExhibitSourceData> LoadFromCsv(string csvPath)
    {
        var exhibits = new List<ExhibitSourceData>();
        
        if (!File.Exists(csvPath))
        {
            Debug.LogError($"CSV file not found at {csvPath}");
            return exhibits;
        }

        try
        {
            var csvContent = File.ReadAllText(csvPath);
            var lines = csvContent.Split('\n');
            
            if (lines.Length < 2) return exhibits;

            // Skip header line
            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var fields = ParseCsvLine(line);
                if (fields.Length >= 4) // Minimum required fields
                {
                    var exhibit = new ExhibitSourceData();
                    exhibit.SetName(fields[0]);
                    exhibit.SetTheme(fields[1]);
                    exhibit.SetSize(fields[2]);
                    exhibit.SetRarity(fields[3]);
                    
                    if (fields.Length > 4 && int.TryParse(fields[4], out var enjoyment)) 
                        exhibit.SetEnjoyment(enjoyment);
                    if (fields.Length > 5 && int.TryParse(fields[5], out var popularity)) 
                        exhibit.SetPopularity(popularity);
                    if (fields.Length > 6) 
                        exhibit.SetArtistNotes(fields[6]);
                    if (fields.Length > 7 && int.TryParse(fields[7], out var artistEffort)) 
                        exhibit.SetArtistEffort(artistEffort);
                    if (fields.Length > 8 && int.TryParse(fields[8], out var vfxEffort)) 
                        exhibit.SetVfxEffort(vfxEffort);

                    exhibits.Add(exhibit);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load CSV file: {e.Message}");
        }

        return exhibits;
    }

    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var currentField = "";
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // Escaped quote
                    currentField += '"';
                    i++; // Skip next quote
                }
                else
                {
                    // Toggle quote state
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.Trim());
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        
        fields.Add(currentField.Trim());
        return fields.ToArray();
    }
}

public static class ExhibitDataConverter
{
    public static ExhibitTileType ConvertToExhibitTileType(ExhibitSourceData sourceData)
    {
        var exhibit = new ExhibitTileType();
        
        exhibit.DisplayName = sourceData.Name;
        exhibit.Size = ParseSize(sourceData.Size);
        exhibit.Rarity = ParseRarity(sourceData.Rarity);
        exhibit.Enjoyment = sourceData.Enjoyment;
        exhibit.Popularity = sourceData.Popularity;
        exhibit.Tags = ParseTags(sourceData.Theme);
                
        return exhibit;
    }

    private static void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(obj, value);
    }

    private static Vector2Int ParseSize(string sizeString)
    {
        if (string.IsNullOrEmpty(sizeString))
            return new Vector2Int(1, 1);

        var parts = sizeString.Split('x');
        if (parts.Length == 2 && 
            int.TryParse(parts[0], out var x) && 
            int.TryParse(parts[1], out var y))
        {
            return new Vector2Int(x, y);
        }

        return new Vector2Int(1, 1);
    }

    private static ExhibitRarity ParseRarity(string rarityString)
    {
        if (string.IsNullOrEmpty(rarityString))
            return ExhibitRarity.Common;

        return rarityString.ToLower() switch
        {
            "common" => ExhibitRarity.Common,
            "rare" => ExhibitRarity.Rare,
            "exotic" => ExhibitRarity.Exotic,
            "mythic" => ExhibitRarity.Mythic,
            "exhibit" => ExhibitRarity.Common, // Treat "Exhibit" as Common
            _ => ExhibitRarity.Common
        };
    }

    private static List<ExhibitTag> ParseTags(string themeString)
    {
        var tags = new List<ExhibitTag>();
        
        if (string.IsNullOrEmpty(themeString))
            return tags;

        var themeParts = themeString.Split(',');
        foreach (var part in themeParts)
        {
            var cleanPart = part.Trim();
            var tag = ParseTag(cleanPart);
            if (tag != ExhibitTag.None)
            {
                tags.Add(tag);
            }
        }

        return tags;
    }

    private static ExhibitTag ParseTag(string tagString)
    {
        return tagString.ToLower() switch
        {
            "fantasy" => ExhibitTag.Theme_Fantasy,
            "model" => ExhibitTag.Theme_Model,
            "modern" => ExhibitTag.Theme_Modern,
            "steampunk" => ExhibitTag.Theme_Steampunk,
            "invention" => ExhibitTag.Theme_Invention,
            "space" => ExhibitTag.Theme_Space,
            "beast" => ExhibitTag.Theme_Beast,
            "light" => ExhibitTag.Theme_Light,
            "earth" => ExhibitTag.Theme_Earth,
            "fire" => ExhibitTag.Theme_Fire,
            "ice" => ExhibitTag.Theme_Ice,
            "lightning" => ExhibitTag.Theme_Lightning,
            "botanical" => ExhibitTag.Theme_Botanical,
            "arcane" => ExhibitTag.Theme_Arcane,
            "scripts" => ExhibitTag.Theme_Scripts,
            "shadow" => ExhibitTag.Theme_Shadow,
            "warfare" => ExhibitTag.Theme_Warfare,
            "treasure" => ExhibitTag.Theme_Treasure,
            "pirate" => ExhibitTag.Theme_Pirate,
            "gothic" => ExhibitTag.Theme_Shadow, // Map Gothic to Shadow for now
            _ => ExhibitTag.None
        };
    }
}
