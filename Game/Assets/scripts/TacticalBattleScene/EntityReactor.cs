using Assets.scripts.UnityBase;
using UnityEngine;

namespace Assets.scripts.TacticalBattleScene
{
    #region EntityReactor

    public class EntityReactor : CircularButton
    {
        public TacticalEntity Entity { get; set; }

        public EntityReactor()
        {
            ClickableAction = CheckIfClickIsOnUI(() => TacticalState.SelectedHex = Entity.Hex.Reactor);
            OnMouseOverProperty = () => Entity.Hex.Reactor.OnMouseOverProperty();
        }
    }

    #endregion EntityReactor
}