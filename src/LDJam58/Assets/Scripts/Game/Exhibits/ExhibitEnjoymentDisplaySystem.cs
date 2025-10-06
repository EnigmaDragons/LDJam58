using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Exhibits
{
    public class ExhibitEnjoymentDisplaySystem : OnMessage<StartPlacement, ExhibitPlaced, OpenMuseum, SeasonInitialized>
    {
        [SerializeField]
        private LayerMask raycastLayerMask;
        [SerializeField]
        private ExhibitEnjoymentDisplay exhibitEnjoymentDisplay;
        private Camera raycastCamera;
        
        private bool isActive;

        [Button]
        public void StartSystem()
        {
            isActive = true;
        }

        [Button]
        public void StopSystem()
        {
            isActive = false;
            //exhibitEnjoymentDisplay.DisableDisplay();
        }

        private void Update()
        {
            if (!isActive || CurrentGameState.ReadOnly == null) return;
            
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f ,layerMask: raycastLayerMask))
            {
                //exhibitEnjoymentDisplay.EnableDisplay();
                var hitInstance = hit.collider.gameObject;
                print(hitInstance.name);
                var rootObject = hitInstance.transform.parent.gameObject;
                var exhibitInstance = rootObject.GetComponent<ExhibitPrefab>();
                if (CurrentGameState.ReadOnly.focusedExhibit != exhibitInstance.ExhibitTileType.DisplayName)
                    CurrentGameState.UpdateState(x =>
                    {
                        x.focusedExhibit = exhibitInstance.ExhibitTileType.DisplayName;
                        x.focusedRoom = x.Exhibits[exhibitInstance.ExhibitTileType.DisplayName].roomId;
                    });
                //exhibitEnjoymentDisplay.SetDisplayPosition(hitInstance.transform.position);
                //exhibitEnjoymentDisplay.SetDisplayExhibit(exhibitInstance);
            }
            else
            {
                //exhibitEnjoymentDisplay.DisableDisplay();
            }
        }

        protected override void Execute(StartPlacement msg)
        {
            StopSystem();
        }

        protected override void Execute(ExhibitPlaced msg)
        {
            StartSystem();
        }

        protected override void Execute(OpenMuseum msg)
        {
            StopSystem();
        }

        protected override void Execute(SeasonInitialized msg)
        {
            StartSystem();
        }
    }
}