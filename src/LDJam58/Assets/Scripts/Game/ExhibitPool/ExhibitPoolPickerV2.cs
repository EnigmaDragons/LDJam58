using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Game.Messages;
using UnityEngine;

namespace Game.ExhibitPool
{
    /// <summary>
    /// ExhibitPoolPickerV2 implements the Tile Randomize rules:
    /// - Always pulls exactly 3 tiles
    /// - Only shows tiles that have not been picked (permanent removal)
    /// - Selects a single rarity for all 3 tiles (must have 3+ exhibits of that rarity)
    /// - Ensures 3 different tags are selected with retry logic
    /// - Uses hard-coded rarity percentages: Exhibit(56%), Rare(28%), Exotic(14%), Mythic(2%)
    /// </summary>
    public class ExhibitPoolPickerV2 : OnMessage<StartExhibitPick, ExhibitPicked>
    {
        [SerializeField] private ExhibitPoolObject exhibitPoolObject;
        
        // Hard-coded rarity percentages as described
        private const float EXHIBIT_PERCENTAGE = 0.56f;    // 56%
        private const float RARE_PERCENTAGE = 0.28f;       // 28%  
        private const float EXOTIC_PERCENTAGE = 0.14f;     // 14%
        private const float MYTHIC_PERCENTAGE = 0.02f;     // 2%
        
        // The pool of available exhibits (permanently removed when picked)
        private List<ExhibitTileType> availableExhibits;

        private void Start()
        {
            InitializePool();
        }

        /// <summary>
        /// Initialize the pool with all exhibits from the pool object
        /// </summary>
        private void InitializePool()
        {
            availableExhibits = new List<ExhibitTileType>();
            foreach (var exhibit in exhibitPoolObject.Exhibits)
            {
                availableExhibits.Add(ExhibitDataConverter.ConvertToExhibitTileType(exhibit));
            }
        }

        /// <summary>
        /// Main method that picks 3 random exhibits following Tile Randomize rules
        /// </summary>
        public List<ExhibitTileType> PickThreeExhibits()
        {
            var selectedExhibits = new List<ExhibitTileType>();
            var maxAttempts = 10; // Prevent infinite loops
            var attempts = 0;

            while (selectedExhibits.Count < 3 && attempts < maxAttempts)
            {
                attempts++;
                
                try
                {
                    // Step 1: Select a single rarity for all 3 tiles
                    var selectedRarity = SelectRarityForAllTiles();
                    
                    // Step 2: Get all exhibits of that rarity
                    var rarityPool = GetExhibitsOfRarity(selectedRarity);
                    
                    // Step 3: Ensure we have at least 3 exhibits of this rarity
                    if (rarityPool.Count < 3)
                    {
                        Debug.LogWarning($"Not enough {selectedRarity} exhibits available ({rarityPool.Count}/3 required). Retrying...");
                        continue;
                    }
                    
                    // Step 4: Select 3 different tags from this rarity pool
                    var selectedTags = SelectThreeDifferentTags(rarityPool);
                    
                    // Step 5: Draw one exhibit per tag
                    var exhibitsForThisAttempt = DrawExhibitsForTags(rarityPool, selectedTags);
                    
                    // If we got 3 different exhibits, add them to our selection
                    if (exhibitsForThisAttempt.Count == 3)
                    {
                        selectedExhibits.AddRange(exhibitsForThisAttempt);
                        break; // Success!
                    }
                    else
                    {
                        Debug.LogWarning($"Could not get 3 different exhibits for rarity {selectedRarity}. Retrying...");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in exhibit selection attempt {attempts}: {ex.Message}");
                    continue;
                }
            }

            if (selectedExhibits.Count < 3)
            {
                Debug.LogError($"Failed to select 3 exhibits after {maxAttempts} attempts. Returning partial selection.");
            }

            return selectedExhibits;
        }

        /// <summary>
        /// Step 1: Select a single rarity using hard-coded percentages
        /// </summary>
        private ExhibitRarity SelectRarityForAllTiles()
        {
            var randomValue = Rng.Float();
            
            // Check percentages in order of rarity (highest to lowest)
            if (randomValue < MYTHIC_PERCENTAGE)
                return ExhibitRarity.Mythic;
            
            if (randomValue < MYTHIC_PERCENTAGE + EXOTIC_PERCENTAGE)
                return ExhibitRarity.Exotic;
            
            if (randomValue < MYTHIC_PERCENTAGE + EXOTIC_PERCENTAGE + RARE_PERCENTAGE)
                return ExhibitRarity.Rare;
            
            // Default to Exhibit (Common) - covers the remaining 56%
            return ExhibitRarity.Common;
        }

        /// <summary>
        /// Get all available exhibits of a specific rarity
        /// </summary>
        private List<ExhibitTileType> GetExhibitsOfRarity(ExhibitRarity rarity)
        {
            return availableExhibits.Where(exhibit => exhibit.Rarity == rarity).ToList();
        }

        /// <summary>
        /// Step 2: Select 3 different tags from the rarity pool
        /// </summary>
        private List<ExhibitTag> SelectThreeDifferentTags(List<ExhibitTileType> rarityPool)
        {
            var allAvailableTags = new HashSet<ExhibitTag>();
            
            // Collect all unique tags from this rarity pool
            foreach (var exhibit in rarityPool)
            {
                foreach (var tag in exhibit.Tags)
                {
                    if (tag != ExhibitTag.None)
                    {
                        allAvailableTags.Add(tag);
                    }
                }
            }

            if (allAvailableTags.Count < 3)
            {
                throw new InvalidOperationException($"Not enough different tags available in rarity pool. Found {allAvailableTags.Count}, need 3.");
            }

            // Randomly select 3 different tags
            var availableTagsList = allAvailableTags.ToList();
            var selectedTags = new List<ExhibitTag>();
            
            for (int i = 0; i < 3; i++)
            {
                var randomIndex = Rng.Int(availableTagsList.Count);
                var selectedTag = availableTagsList[randomIndex];
                selectedTags.Add(selectedTag);
                
                // Remove this tag from available options to ensure uniqueness
                availableTagsList.RemoveAt(randomIndex);
            }

            return selectedTags;
        }

        /// <summary>
        /// Step 3: Draw one exhibit per tag with retry logic
        /// </summary>
        private List<ExhibitTileType> DrawExhibitsForTags(List<ExhibitTileType> rarityPool, List<ExhibitTag> selectedTags)
        {
            var selectedExhibits = new List<ExhibitTileType>();
            var maxTagAttempts = 5; // Prevent infinite loops per tag

            foreach (var tag in selectedTags)
            {
                var tagAttempts = 0;
                var exhibitFound = false;

                while (!exhibitFound && tagAttempts < maxTagAttempts)
                {
                    tagAttempts++;
                    
                    // Find all exhibits in the rarity pool that have this tag
                    var exhibitsWithTag = rarityPool
                        .Where(exhibit => exhibit.Tags.Contains(tag))
                        .ToList();

                    if (exhibitsWithTag.Count == 0)
                    {
                        Debug.LogWarning($"No exhibits found with tag {tag} in rarity pool. Retrying tag selection...");
                        break; // This will cause the whole process to retry
                    }

                    // Randomly select one exhibit with this tag
                    var randomExhibit = exhibitsWithTag.Random();
                    
                    // Make sure we don't select the same exhibit twice
                    if (!selectedExhibits.Contains(randomExhibit))
                    {
                        selectedExhibits.Add(randomExhibit);
                        exhibitFound = true;
                    }
                    else
                    {
                        // If we already have this exhibit, remove it from consideration
                        rarityPool.Remove(randomExhibit);
                        
                        // If we've exhausted the pool, we need to retry the whole process
                        if (rarityPool.Count == 0)
                        {
                            Debug.LogWarning($"Exhausted rarity pool while looking for unique exhibits with tag {tag}.");
                            break;
                        }
                    }
                }

                if (!exhibitFound)
                {
                    Debug.LogWarning($"Failed to find unique exhibit for tag {tag} after {maxTagAttempts} attempts.");
                    break; // This will cause the whole process to retry
                }
            }

            return selectedExhibits;
        }

        /// <summary>
        /// Permanently remove an exhibit from the available pool
        /// </summary>
        public void PermanentlyRemoveExhibit(ExhibitTileType exhibit)
        {
            if (availableExhibits.Contains(exhibit))
            {
                availableExhibits.Remove(exhibit);
                Debug.Log($"Permanently removed exhibit {exhibit.DisplayName} from pool. {availableExhibits.Count} exhibits remaining.");
            }
        }

        /// <summary>
        /// Get count of available exhibits by rarity (for debugging)
        /// </summary>
        public Dictionary<ExhibitRarity, int> GetAvailableExhibitCounts()
        {
            return availableExhibits
                .GroupBy(exhibit => exhibit.Rarity)
                .ToDictionary(group => group.Key, group => group.Count());
        }

        /// <summary>
        /// Handle the StartExhibitPick message
        /// </summary>
        protected override void Execute(StartExhibitPick msg)
        {
            if (CurrentGameState.ReadOnly.isPicking)
            {
                Debug.LogWarning("Already picking exhibits. Ignoring StartExhibitPick message.");
                return;
            }

            // Update game state to indicate we're picking
            CurrentGameState.UpdateState(gs => {
                gs.isPicking = true;
                return gs;
            });

            // Pick 3 exhibits using Tile Randomize rules
            var selectedExhibits = PickThreeExhibits();
            
            if (selectedExhibits.Count == 3)
            {
                // Publish the selection to the UI
                var payload = new BeginPickThree(selectedExhibits.ToArray());
                Message.Publish(payload);
                
                Debug.Log($"Successfully selected 3 exhibits: {string.Join(", ", selectedExhibits.Select(e => $"{e.DisplayName}({e.Rarity})"))}");
            }
            else
            {
                Debug.LogError($"Failed to select 3 exhibits. Only got {selectedExhibits.Count}.");
                
                // Reset picking state on failure
                CurrentGameState.UpdateState(gs => {
                    gs.isPicking = false;
                    return gs;
                });
            }
        }

        /// <summary>
        /// Handle the ExhibitPicked message - permanently remove the picked exhibit
        /// </summary>
        protected override void Execute(ExhibitPicked msg)
        {
            PermanentlyRemoveExhibit(msg.Exhibit);
        }
    }
}
