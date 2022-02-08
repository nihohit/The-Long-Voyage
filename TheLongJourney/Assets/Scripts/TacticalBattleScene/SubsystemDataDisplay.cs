using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.TacticalBattleScene;
using Assets.Scripts.Base;
using System.Linq;

public class SubsystemDataDisplay : MonoBehaviour {
  private Text m_name;
  private Text m_condition;
  private Text m_ammo;
  private Text m_actions;

  public void Start() {

    m_name = transform.GetChild(0).GetComponent<Text>();
    m_condition = transform.GetChild(1).GetComponent<Text>();
    m_ammo = transform.GetChild(2).GetComponent<Text>();
    m_actions = transform.GetChild(3).GetComponent<Text>();
  }

  public void DisplayInfo(Assets.Scripts.TacticalBattleScene.Subsystem subsystem) {
    m_name.text = subsystem.Template.Name;
    m_condition.text = subsystem.OperationalCondition.ToString();
    m_ammo.text = subsystem.Template.MaxAmmo != -1 ? "{0}/{1}".FormatWith(subsystem.Ammo, subsystem.Template.MaxAmmo) : "Inf";
    m_actions.text = "{0}/{1}".FormatWith(subsystem.RemainingActions, subsystem.Template.ActionsPerTurn);
  }
}