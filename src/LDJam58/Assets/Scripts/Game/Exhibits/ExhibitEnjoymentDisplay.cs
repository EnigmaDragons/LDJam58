using System;
using Assets.Scripts;
using TMPro;
using UnityEngine;

namespace Game.Exhibits
{
    public class ExhibitEnjoymentDisplay : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text enjoymentText;
        
        [SerializeField] private GameObject visuals;
        
        public void SetDisplayPosition(Vector3 hitPoint)
        {
            transform.position = hitPoint;
        }
        
        public void SetDisplayExhibit(ExhibitPrefab exhibitInstance)
        {
            var score = CurrentGameState.GetExhibitEnjoymentScore(exhibitInstance);
            enjoymentText.text = $"Enjoyment: {score}";
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