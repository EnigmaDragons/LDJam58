using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.NPC
{
    public class NpcSpawner : OnMessage<SpawnNpcs>
    {
        [AssetsOnly]
        [SerializeField] 
        private GameObject npcPrefab;
        [SerializeField]
        private NpcNavigation npcNavigation;
        [SerializeField]
        private Transform spawnPoint;
        
        private void SpawnNpcs(int npcCount)
        {
            for (int i = 0; i < npcCount; i++)
            {
                var inst = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);
                var agent =  inst.GetComponent<NpcAgentController>();
                agent.Init(npcNavigation);
                agent.StartPickCoroutine();
            }
        }
        
        protected override void Execute(SpawnNpcs msg)
        {
            SpawnNpcs(msg.NpcCount);
        }
    }
}