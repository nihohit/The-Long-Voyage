using UnityEngine;
using Assets.Scripts.UnityBase;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Base;
using System;

namespace Assets.Scripts.TacticalBattleScene {
  public class PotentialActionsMarker : MonoBehaviour {
    private IUnityButton m_movementButton;
    private List<SimpleButton> m_systemButtons;
    private Dictionary<string, Sprite> m_spriteDict;

    public void Start() {
      var buttons = GetComponentsInChildren<SimpleButton>().ToList();
      m_movementButton = buttons.First(button => button.gameObject.name.Equals("MovementAction"));
      buttons.Remove((SimpleButton)m_movementButton);
      m_systemButtons = buttons;
      var sprites = Resources.LoadAll<Sprite>("SystemsUI");
      m_spriteDict = sprites.ToDictionary(
        sprite => sprite.name,
        sprite => sprite);
    }

    public void SetOnHex(HexReactor hex, IEnumerable<PotentialAction> actions) {
      Assert.NotNullOrEmpty(actions, "actions");

      this.gameObject.SetActive(true);

      var movementAction = actions.Select(action => action as MovementAction).FirstOrDefault(action => action != null);
      var systemActions = actions.Select(action => action as OperateSystemAction).Where(action => action != null && action.NecessaryConditions()).ToList();

      if (movementAction != null) {
        m_movementButton.Mark();
        m_movementButton.ClickableAction = movementAction.Commit;
        movementAction.Callback = () => TacticalState.SelectedHex = movementAction.TargetedHex;
      } else {
        m_movementButton.Unmark();
      }

      for (int i = 0; i < m_systemButtons.Count; i++) {
        if (i < systemActions.Count) {
          var button = m_systemButtons[i];
          button.Mark();
          button.ClickableAction = CreateAction(systemActions[i].Commit, hex, actions);

          var renderer = button.GetComponent<SpriteRenderer>();
          renderer.sprite = m_spriteDict.Get(systemActions[i].Name);
        } else {
          m_systemButtons[i].Unmark();
        }
      }

      this.transform.position = hex.transform.position;
    }

    private Action CreateAction(Action action, HexReactor hex, IEnumerable<PotentialAction> actions) {
      return () => {
        action();
        SetOnHex(hex, actions);
      };
    }
  }
}
