using UnityEngine;

namespace GameBase
{
    public class HealthUpgradeItemDecorator : MonoBehaviour, IItemDecorator
    {
        protected float m_upgradeAmount = 10f;
        protected bool m_healToFull = false;
        protected float m_healingAmount = 0f;

        public void Use()
        {
            //Get reference to the player
            PlayerCharacter player = GameInstance.Instance.GetPlayerScript();

            player.UpgradeHealth(m_upgradeAmount, m_healToFull);    //upgrades player health, and fully heals player if indicated to do so
            player.HealDamage(m_healingAmount);                     //heals player by specified amount
        }
    }
}
