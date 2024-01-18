using UnityEngine;

//Used for objects that will affect the state or flow of the game
public abstract class GameFlowEventController : MonoBehaviour
{
    public abstract void TriggerGameFlowEvent(bool value);
}
