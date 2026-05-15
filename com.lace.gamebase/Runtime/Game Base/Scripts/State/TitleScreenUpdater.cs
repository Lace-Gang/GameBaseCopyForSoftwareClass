using UnityEngine;

namespace GameBase
{
    public class TitleScreenUpdater : GameUpdater, IGameStateUpdate
    {
        public override void ExecuteStateUpdate()
        {
            //Transitions to main menu if any key is pressed
            if (Input.anyKey)
            {
                GameInstance.Instance.m_gameState = GameState.LOADMAINMENU;   //transition to "load main menu" state
            }
        }
    }
}
