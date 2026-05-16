using UnityEngine;

namespace GameBase
{
    public class UseOnPickupItemDecorator : MonoBehaviour, IItemDecorator
    {
        public void Use()
        {
            this.gameObject.GetComponent<ItemBase>().Use();
        }
    }
}
