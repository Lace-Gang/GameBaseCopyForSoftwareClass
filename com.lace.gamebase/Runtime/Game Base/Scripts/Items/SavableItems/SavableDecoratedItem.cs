using UnityEngine;

namespace GameBase
{
    public class SavableDecoratedItem : SavableItem
    {

        void Start()
        {
            m_decorators.Add(ItemDecoratorFactory.getDecorator("use on pickup"));
            m_decorators.Add(ItemDecoratorFactory.getDecorator("hp recovery"));
            m_decorators.Add(ItemDecoratorFactory.getDecorator("hp upgrade"));
            m_decorators.Add(ItemDecoratorFactory.getDecorator("score increase"));
        }

        public override void OnPickedUp()
        {
            HideItemInScene();
        }

        public override void Use()
        {
            //throw new System.NotImplementedException();
        }
    }
}
