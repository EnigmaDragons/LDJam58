using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using System.IO;

#if UNITY_EDITOR
public class ExhibitSourceEditorWindow : OdinEditorWindow
{
    private const string CsvFilePath = "Assets/exhibit_data.csv";
    
    [MenuItem("Tools/Exhibit/Exhibit Source Editor")]
    private static void Open()
    {
        var window = GetWindow<ExhibitSourceEditorWindow>("Exhibit Source Editor");
        window.minSize = new Vector2(800, 600);
        window.Show();
    }

    [Title("Exhibit Source Editor")]
    [InfoBox("Edit exhibit data directly in the table below. Changes are automatically saved to the CSV file after every field edit.")]
    [Title("Exhibit Data")]
    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 20, AlwaysExpanded = true)]
    [SerializeField] private List<ExhibitSourceData> _exhibits = new List<ExhibitSourceData>();

    [Title("Actions")]
    [Button("Reload from CSV", ButtonSizes.Medium)]
    private void ReloadFromCsv()
    {
        LoadFromCsv();
        EditorUtility.DisplayDialog("Reload Complete", "Data reloaded from CSV file!", "OK");
    }

    [Button("Save to CSV", ButtonSizes.Medium)]
    private void SaveToCsv()
    {
        SaveToCsvFile();
        EditorUtility.DisplayDialog("Save Complete", "Data saved to CSV file!", "OK");
    }

    [Button("Import from Different CSV", ButtonSizes.Medium)]
    private void ImportFromDifferentCsv()
    {
        var path = EditorUtility.OpenFilePanel("Import CSV", "", "csv");
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                var csvContent = File.ReadAllText(path);
                ParseAndImportCsv(csvContent);
                SaveToCsvFile(); // Save to the default location
                EditorUtility.DisplayDialog("Import Complete", "CSV data imported and saved successfully!", "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Import Error", $"Failed to import CSV: {e.Message}", "OK");
            }
        }
    }

    [Button("Clear All", ButtonSizes.Medium)]
    private void ClearAll()
    {
        if (EditorUtility.DisplayDialog("Clear All Exhibits", "Are you sure you want to clear all exhibits?", "Yes", "No"))
        {
            _exhibits.Clear();
            SaveToCsvFile();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        LoadFromCsv();
    }

    protected override void OnImGUI()
    {
        base.OnImGUI();
        
        // Auto-save on any GUI change
        if (GUI.changed)
        {
            SaveToCsvFile();
        }
    }

    protected override void OnBeginDrawEditors()
    {
        base.OnBeginDrawEditors();
        
        // Force save after any property changes
        if (Event.current.type == EventType.MouseUp || 
            Event.current.type == EventType.KeyUp ||
            Event.current.type == EventType.ExecuteCommand)
        {
            SaveToCsvFile();
        }
    }


    private void LoadFromCsv()
    {
        _exhibits.Clear();
        
        if (File.Exists(CsvFilePath))
        {
            try
            {
                var csvContent = File.ReadAllText(CsvFilePath);
                ParseAndImportCsv(csvContent);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load CSV file: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"CSV file not found at {CsvFilePath}. Creating empty list.");
        }
    }

    private void SaveToCsvFile()
    {
        try
        {
            var csvContent = GenerateCsvContent();
            File.WriteAllText(CsvFilePath, csvContent);
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save CSV file: {e.Message}");
        }
    }

    // Custom property processor to auto-save on field changes
    protected override IEnumerable<object> GetTargets()
    {
        yield return this;
    }

    private void ParseAndImportCsv(string csvContent)
    {
        var lines = csvContent.Split('\n');
        if (lines.Length < 2) return;

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
                
                if (fields.Length > 4 && int.TryParse(fields[4], out var enjoyment)) exhibit.SetEnjoyment(enjoyment);
                if (fields.Length > 5 && int.TryParse(fields[5], out var popularity)) exhibit.SetPopularity(popularity);
                if (fields.Length > 6) exhibit.SetArtistNotes(fields[6]);
                if (fields.Length > 7 && int.TryParse(fields[7], out var artistEffort)) exhibit.SetArtistEffort(artistEffort);
                if (fields.Length > 8 && int.TryParse(fields[8], out var vfxEffort)) exhibit.SetVfxEffort(vfxEffort);

                _exhibits.Add(exhibit);
            }
        }
    }

    private string GenerateCsvContent()
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Name,Theme,Size,Rarity,Enjoyment,Popularity,Artist Notes,Artist Effort,VFX Effort");
        
        foreach (var exhibit in _exhibits)
        {
            var exhibitName = EscapeCsvField(exhibit.Name);
            var theme = EscapeCsvField(exhibit.Theme);
            var size = EscapeCsvField(exhibit.Size);
            var rarity = EscapeCsvField(exhibit.Rarity);
            var enjoyment = exhibit.Enjoyment.ToString();
            var popularity = exhibit.Popularity.ToString();
            var artistNotes = EscapeCsvField(exhibit.ArtistNotes);
            var artistEffort = exhibit.ArtistEffort.ToString();
            var vfxEffort = exhibit.VfxEffort.ToString();
            
            csv.AppendLine($"{exhibitName},{theme},{size},{rarity},{enjoyment},{popularity},{artistNotes},{artistEffort},{vfxEffort}");
        }
        
        return csv.ToString();
    }

    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";
            
        // If the field contains comma, quote, or newline, wrap it in quotes and escape internal quotes
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }
        
        return field;
    }

    private string[] ParseCsvLine(string line)
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

[Serializable]
public class ExhibitSourceData
{
    [TableColumnWidth(120)]
    [SerializeField] private string _name = "";
    
    [TableColumnWidth(150)]
    [SerializeField] private string _theme = "";
    
    [TableColumnWidth(60)]
    [SerializeField] private string _size = "";
    
    [TableColumnWidth(80)]
    [SerializeField] private string _rarity = "";

    [TableColumnWidth(80)]
    [SerializeField] private int _enjoyment;
    
    [TableColumnWidth(80)]
    [SerializeField] private int _popularity;

    [TableColumnWidth(200)]
    [SerializeField] private string _artistNotes = "";
    
    [TableColumnWidth(80)]
    [SerializeField] private int _artistEffort;
    
    [TableColumnWidth(80)]
    [SerializeField] private int _vfxEffort;

    public string Name => _name;
    public string Theme => _theme;
    public string Size => _size;
    public string Rarity => _rarity;
    public int Enjoyment => _enjoyment;
    public int Popularity => _popularity;
    public string ArtistNotes => _artistNotes;
    public int ArtistEffort => _artistEffort;
    public int VfxEffort => _vfxEffort;

    public void SetName(string value) => _name = value;
    public void SetTheme(string value) => _theme = value;
    public void SetSize(string value) => _size = value;
    public void SetRarity(string value) => _rarity = value;
    public void SetEnjoyment(int value) => _enjoyment = value;
    public void SetPopularity(int value) => _popularity = value;
    public void SetArtistNotes(string value) => _artistNotes = value;
    public void SetArtistEffort(int value) => _artistEffort = value;
    public void SetVfxEffort(int value) => _vfxEffort = value;
}
#endif
