using Game.NPC;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Devtools
{
    public class SpawnNpcStart : MonoBehaviour
    {
        [Button]
        private void SpawnNpcs(int count)
        {
            Message.Publish(new SpawnNpcs(count));
        }
        
    }
}