using System;
using Assets.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Exhibits
{
    public class ExhibitEnjoymentDisplay : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text enjoymentText;
        [SerializeField] 
        private TMP_Text nameText;
        [SerializeField]
        private Image rarityImage;
        [SerializeField]
        private Image exhibitImage;
        [SerializeField] 
        private TMP_Text tagText;
        
        
        [SerializeField] private GameObject visuals;
        
        [SerializeField]
        private Sprite commonSprite;
        [SerializeField]
        private Sprite rareSprite;
        [SerializeField]
        private Sprite exoticSprite;
        [SerializeField]
        private Sprite mythicSprite;
        
        private ExhibitPrefab exhibitPrefab;
        public void SetDisplayPosition(Vector3 hitPoint)
        {
            transform.position = hitPoint;
        }
        
        public void SetDisplayExhibit(ExhibitPrefab exhibitInstance, bool deBurgirHasAGhost = false)
        {
            exhibitPrefab = exhibitInstance;
            UpdateRarity();
            UpdateVisuals();
            if(!deBurgirHasAGhost) UpdateScore();
        }

        private void UpdateScore()
        {
            var score = CurrentGameState.GetExhibitEnjoymentScore(exhibitPrefab);
            enjoymentText.text = $"{score}";
        }

        public void DoGhostBusters(int ghostBustingScore)
        {
            enjoymentText.text = $"{ghostBustingScore}";
        }
        
        private void UpdateVisuals()
        {
            nameText.text = exhibitPrefab.ExhibitTileType.DisplayName;
            exhibitImage.sprite = exhibitPrefab.ExhibitTileType.ExhibitSprite;
            tagText.text = GetTagString();
        }

        private string GetTagString()
        {
            var tags = exhibitPrefab.ExhibitTileType.Tags;

            var fullString = "";
            foreach (var tag in tags)
            {
                fullString += $"<sprite name=\"{tag}\">";
            }

            return fullString;
        }
        
        private void UpdateRarity()
        {
            rarityImage.sprite = exhibitPrefab.ExhibitTileType.Rarity switch
            {
                ExhibitRarity.Common => commonSprite,
                ExhibitRarity.Rare => rareSprite,
                ExhibitRarity.Exotic => exoticSprite,
                ExhibitRarity.Mythic => mythicSprite,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void EnableDisplay()
        {
            visuals.SetActive(true);
        }
        
        public void DisableDisplay()
        {
            visuals.SetActive(false);
        }
    }
}