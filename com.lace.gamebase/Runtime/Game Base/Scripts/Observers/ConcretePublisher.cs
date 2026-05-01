using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase
{
    public class ConcretePublisher : MonoBehaviour, IPublisher
    {
        private List<ISubscriber> subscribers = new List<ISubscriber>();

        private static ConcretePublisher _instance;
        public static ConcretePublisher Instance { get { return _instance; } private set { } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        public void Publish(string newState)
        {
            foreach(ISubscriber subscriber in subscribers)
            {
                subscriber.UpdateState(newState);
            }
        }

        public void RegisterSubscriber(ISubscriber subscriber)
        {
            if(!subscribers.Contains(subscriber))
            {
                subscribers.Add(subscriber);
            }
        }

        public void UnregisterSubscriber(ISubscriber subscriber)
        {
            if(subscribers.Contains(subscriber))
            {  
                subscribers.Remove(subscriber);
            }
        }
    }
}
