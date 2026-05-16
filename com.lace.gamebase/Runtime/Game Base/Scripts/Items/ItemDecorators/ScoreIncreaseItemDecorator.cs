using UnityEngine;

namespace GameBase
{
    public class ScoreIncreaseItemDecorator : MonoBehaviour, IItemDecorator
    {
        protected float m_scoreIncrease = 10;

        public void Use()
        {
            GameInstance.Instance.AddOrRemoveScore(m_scoreIncrease);
        }
    }
}
