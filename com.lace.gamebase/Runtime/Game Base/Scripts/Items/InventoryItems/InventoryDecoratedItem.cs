using UnityEngine;

namespace GameBase
{
    public class InventoryDecoratedItem : InventoryItem
    {
        void Start()
        {
            m_decorators.Add(ItemDecoratorFactory.getDecorator("hp recovery"));
            m_decorators.Add(ItemDecoratorFactory.getDecorator("hp upgrade"));
            m_decorators.Add(ItemDecoratorFactory.getDecorator("score increase"));
        }

        public override void Use()
        {
            //throw new System.NotImplementedException();
        }

    }
}
