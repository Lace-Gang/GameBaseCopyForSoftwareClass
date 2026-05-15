using UnityEngine;

namespace GameBase
{
    public interface ISubscriber
    {
        public void UpdateState(string newState);
    }
}
