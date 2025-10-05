using System;
using System.Collections.Generic;
using System.Linq;
using Game.Exhibits;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Game.TilePlacement
{
    public class GhostTile : MonoBehaviour
    {
        [SerializeField]
        private string ghostLayerMask;
        [SerializeField]
        private Material errorMaterial;
        [SerializeField]
        private Material highScoreMaterial;
        [SerializeField]
        private Material ghostMaterial;
        [SerializeField]
        private Material lowScoreMaterial;

        [SerializeField] 
        private GameObject textVisuals;
        [SerializeField]
        private ExhibitEnjoymentDisplay enjoymentDisplay;
        
        public GameObject GhostPlaceable => ghostPlaceable;
        private GameObject ghostPlaceable;
        private ExhibitPrefab ghostPrefab;
        private List<Renderer> ghostRenderers;
        
        bool stateChanged = false;

        public bool IsOverlapping { get; private set; }
        private void OnTriggerEnter(Collider other)
        {
            IsOverlapping = true;
            stateChanged = true;
            textVisuals.SetActive(false);
            UpdateMaterial(errorMaterial);
        }

        private void OnTriggerExit(Collider other)
        {
            IsOverlapping = false;
            stateChanged = true;
            UpdateMaterial(ghostMaterial);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!IsOverlapping)
            {
                textVisuals.SetActive(false);
                UpdateMaterial(errorMaterial);
                stateChanged = true;
            }
            IsOverlapping = true;
        }


        private void Awake()
        {
            ghostRenderers = new List<Renderer>();
        }

        public void UpdatePlaceable(ExhibitTileType exhibitTileType)
        {
            if(ghostPlaceable != null) Destroy(ghostPlaceable);
            
            ghostPlaceable = Instantiate(exhibitTileType.ExhibitPrefab, transform);
            ghostPrefab = ghostPlaceable.GetComponent<ExhibitPrefab>();
            ghostPrefab.Init(exhibitTileType);
            enjoymentDisplay.SetDisplayExhibit(ghostPrefab, true);
            //change all the instance layers to Ghost
            SetLayerRecursively(ghostPlaceable, LayerMask.NameToLayer(ghostLayerMask));
            CollectRenderers(ghostPlaceable);
            SetCollidersToTrigger();
            SetupNavmesh();
            UpdateMaterial(ghostMaterial);
        }

        public void DisablePlaceable()
        {
            if(ghostPlaceable != null) ghostPlaceable.SetActive(false);
            textVisuals.SetActive(false);
        }

        public void EnablePlaceable()
        {
            if(ghostPlaceable != null) ghostPlaceable.SetActive(true);
            textVisuals.SetActive(true);
        }
        
        private void UpdateMaterial(Material material)
        {
            foreach (var renderer in ghostRenderers)
            {
                renderer.material = material;
            }
        }

        public void UpdateGhostScore(int baseScore, int newGhostScore)
        {
            textVisuals.SetActive(true);
            enjoymentDisplay.DoGhostBusters(newGhostScore);
            
            if(newGhostScore > baseScore) UpdateMaterial(highScoreMaterial);
            else if (newGhostScore < baseScore)  UpdateMaterial(lowScoreMaterial);
            else UpdateMaterial(ghostMaterial);
        }
        
        private void SetCollidersToTrigger()
        {
            var colliders = GetComponentsInChildren<Collider>(true);
            foreach (Collider col in colliders)
            {         
                if (col is MeshCollider meshCollider)
                {
                    if (meshCollider.convex)
                    {
                        col.isTrigger = true;
                    }
                }
            }
        }
        
        private void SetupNavmesh()
        {
            var obstacle = ghostPlaceable.GetComponentInChildren<NavMeshObstacle>();
            obstacle.enabled = false;
        }

        private void CollectRenderers(GameObject placeable)
        {
            ghostRenderers.Clear();
            var mainRend =  placeable.GetComponent<Renderer>();
            var childRend = placeable.GetComponentsInChildren<Renderer>(placeable);
            
            if(mainRend) ghostRenderers.Add(mainRend);
            ghostRenderers.AddRange(childRend);
        }
        
        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null)
            {
                return;
            }

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        public bool StateChanged()
        {
            if (stateChanged)
            {
                stateChanged = false;
                return true;
            }
                
            return false;
        }

        
        Vector3Int oldPosition;

        public void PositionWasUpdated(Vector3Int newPosition)
        {
            if (oldPosition != newPosition)
            {
                stateChanged = true;
                oldPosition = newPosition;
            }
        }
    }
}