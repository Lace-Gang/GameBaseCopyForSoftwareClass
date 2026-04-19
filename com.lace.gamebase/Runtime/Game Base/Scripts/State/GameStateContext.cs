using UnityEngine;

namespace GameBase
{
    public class GameStateContext
    {
        IGameStateUpdate state;

        public GameStateContext(IGameStateUpdate initialState = null)
        {
            state = initialState;
        }

        public void UpdateGame()
        {
            state?.ExecuteStateUpdate();
        }

        public void SetContext(IGameStateUpdate newState)
        {
            state = newState;
        }
    }
}
