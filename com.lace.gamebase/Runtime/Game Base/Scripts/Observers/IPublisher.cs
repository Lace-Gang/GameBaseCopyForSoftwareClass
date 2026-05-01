using UnityEngine;

namespace GameBase
{
    public interface IPublisher
    {
        public void Publish(string newState);

        public void RegisterSubscriber(ISubscriber subscriber);

        public void UnregisterSubscriber(ISubscriber subscriber);
    }
}
