using Assets.scripts.UnityBase;

namespace Assets.scripts.TacticalBattleScene
{
    #region EntityReactor

    /// <summary>
    /// A script wrapper for entities
    /// </summary>
    public class EntityReactor : SimpleButton
    {
        // The entity the reactor is wrapping
        public TacticalEntity Entity { get; set; }

        public EntityReactor()
        {
            ClickableAction = CheckIfClickIsOnUI(() => TacticalState.SelectedHex = Entity.Hex.Reactor);
            OnMouseOverAction = () => Entity.Hex.Reactor.OnMouseOverAction();
        }
    }

    #endregion EntityReactor
}