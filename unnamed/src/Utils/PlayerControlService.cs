using unnamed.Enums;

namespace unnamed.Utils;

public class PlayerControlService
{
    private PlayerAction? currentAction;
    private float? remainingTime;

    public void Sync(float deltaTime)
    {
        if(this.remainingTime is null) return;
        this.remainingTime -= deltaTime;

        if (this.remainingTime <= 0f)
        {
            this.remainingTime = null;
            this.currentAction = null;
        }
    }
    
    public PlayerAction? GetCurrentAction() => this.currentAction;

    public PlayerAction TryUpdateAction(PlayerAction action, float duration, out bool success)
    {
        if (!currentAction.HasValue)
        {
            this.currentAction = action;
            this.remainingTime = duration;
            success = true;
            return action;
        }

        if (action.GetPriority() < this.currentAction.Value.GetPriority())
        {
            if (this.remainingTime is <= 0f)
            {
                this.currentAction = action;
                this.remainingTime = duration;
                success = true;
                return action;
            }
            success = false;
            return currentAction.Value;
        }
        
        this.currentAction = action;
        this.remainingTime = duration;
        success = true;
        return this.currentAction.Value;
    }

    public PlayerAction TryUpdateAction(PlayerAction action, out bool success)
    {
        return TryUpdateAction(action, 0f, out success);
    }
    
}