using UnityEngine;

namespace GameBase
{
    public class BasicDecoratedItem : ItemBase
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            m_decorators.Add(ItemDecoratorFactory.getDecorator("use on pickup"));
            m_decorators.Add(ItemDecoratorFactory.getDecorator("hp recovery"));
            m_decorators.Add(ItemDecoratorFactory.getDecorator("hp upgrade"));
            m_decorators.Add(ItemDecoratorFactory.getDecorator("score increase"));
        }


        public override void OnPickedUp()
        {
            HideItemInScene();   //Hides item in the scene
        }

        public override void Use()
        {
            //throw new System.NotImplementedException();
        } 
    }
}
