using UnityEngine;

namespace GameBase
{
    public static class ItemDecoratorFactory
    {
        public static IItemDecorator getDecorator(string decoratorName)
        {
            switch (decoratorName.ToLower())
            {
                case "use on pickup":
                    return new UseOnPickupItemDecorator();
                    break;
                case "hp recovery":
                    return new HealthRecoveryItemDecorator();
                    break;
                case "hp upgrade":
                    return new HealthUpgradeItemDecorator();
                    break;
                case "score increase":
                    return new ScoreIncreaseItemDecorator();
                    break;
                default:
                    break;
            }
                return null;
        }
    }
}
