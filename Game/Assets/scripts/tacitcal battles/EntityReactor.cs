using UnityEngine;

#region EntityReactor

public class EntityReactor : CircularButton
{

    public Entity Entity { get; set; }

    public EntityReactor()
    {
        ClickableAction = CheckIfClickIsOnUI(() => TacticalState.SelectedHex = Entity.Hex.Reactor);
        OnMouseOverProperty = () => Entity.Hex.Reactor.OnMouseOverProperty();
    }
}

#endregion EntityReactor