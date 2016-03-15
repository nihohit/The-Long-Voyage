using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.TacticalBattleScene;
using Assets.Scripts.Base;

public class SubsystemDataDisplay : MonoBehaviour 
{
	public Text m_SubsystemNameLabel;
	public Text m_SubsystemNameKey;

	public void Start()
	{
		m_SubsystemNameLabel = GameObject.Find("SubsystemNameLabel").GetComponent<Text>();
		m_SubsystemNameKey = GameObject.Find("SubsystemNameKey").GetComponent<Text>();
	}

	public void DisplayInfo(Subsystem subsystem)
	{
		m_SubsystemNameLabel.text = subsystem.Template.Name;
	}
}