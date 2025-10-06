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
        
        [SerializeField]
        private Vector3Variable displayOffset;
        
        private ExhibitPrefab exhibitPrefab;
        public void SetDisplayPosition(Vector3 hitPoint)
        {
            if (displayOffset == null)
            {
                displayOffset = new Vector3Variable {
                    Value = new Vector3(-3f, 1.5f, 0)
                };
            }

            transform.position = hitPoint + displayOffset.Value;
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
            tagText.text = exhibitPrefab.ExhibitTileType.Tags.Sprites();
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