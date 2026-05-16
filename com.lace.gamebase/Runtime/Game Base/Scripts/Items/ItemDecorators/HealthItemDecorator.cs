using UnityEngine;

namespace GameBase
{
    public class HealthRecoveryItemDecorator : MonoBehaviour, IItemDecorator
    {
        private int m_healAmount = 10;

        public void Use()
        {
            GameInstance.Instance.GetPlayerScript().HealDamage(m_healAmount);
        }
    }
}
