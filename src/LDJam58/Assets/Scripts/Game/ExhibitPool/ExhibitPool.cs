using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

namespace Game.ExhibitPool
{
    [CreateAssetMenu(fileName = "ExhibitPool", menuName = "Exhibits/ExhibitPool")]
    public class ExhibitPoolObject : ScriptableObject
    {
        [SerializeField] private TextAsset csvFile;
        
        private List<ExhibitSourceData> _loadedExhibits;
        
        public List<ExhibitSourceData> Exhibits 
        { 
            get 
            {
                if (_loadedExhibits == null)
                {
                    LoadExhibitsFromCsv();
                }
                return _loadedExhibits;
            } 
        }
        
        private void LoadExhibitsFromCsv()
        {
            _loadedExhibits = new List<ExhibitSourceData>();
            
            if (csvFile != null)
            {
                var csvData = ParseCsvContent(csvFile.text);
                foreach (var data in csvData)
                {
                    _loadedExhibits.Add(data);
                }
            } else {
                Log.Error("No CSV file set for ExhibitPool");
            }
        }
        
        private List<ExhibitSourceData> ParseCsvContent(string csvContent)
        {
            var csvExhibits = new List<ExhibitSourceData>();
            var lines = csvContent.Split('\n');
            
            if (lines.Length < 2) return csvExhibits;

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

                    csvExhibits.Add(exhibit);
                }
            }
            
            return csvExhibits;
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
        
        private void OnValidate()
        {
            // Reload when the CSV file changes
            if (_loadedExhibits != null)
            {
                _loadedExhibits = null;
            }
        }
    }
}