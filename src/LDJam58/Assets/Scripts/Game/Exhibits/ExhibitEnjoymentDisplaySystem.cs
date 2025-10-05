using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Exhibits
{
    public class ExhibitEnjoymentDisplaySystem : OnMessage<StartPlacement, ExhibitPlaced>
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
            exhibitEnjoymentDisplay.DisableDisplay();
        }

        private void Update()
        {
            if (!isActive) return;
            
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f ,layerMask: raycastLayerMask))
            {
                exhibitEnjoymentDisplay.EnableDisplay();
                var hitInstance = hit.collider.gameObject;
                print(hitInstance.name);
                var rootObject = hitInstance.transform.parent.gameObject;
                var exhibitInstance = rootObject.GetComponent<ExhibitPrefab>();
                exhibitEnjoymentDisplay.SetDisplayPosition(hitInstance.transform.position);
                exhibitEnjoymentDisplay.SetDisplayExhibit(exhibitInstance);
            }
            else
            {
                exhibitEnjoymentDisplay.DisableDisplay();
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
    }
}