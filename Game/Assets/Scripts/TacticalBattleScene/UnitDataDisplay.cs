using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.TacticalBattleScene;
using Assets.Scripts.Base;

public class UnitDataDisplay : MonoBehaviour
{
	private Text m_loyaltyLabel;
	private Text m_nameLabel;
	private Text m_shieldLabel;
	private Text m_healthLabel;
	private Text m_heatLabel;
	private Text m_energyLabel;

	public void Start()
	{
		m_nameLabel = GameObject.Find("UnitNameLabel").GetComponent<Text>();
		m_loyaltyLabel = GameObject.Find("UnitLoyaltyLabel").GetComponent<Text>();
		m_shieldLabel = GameObject.Find("UnitShieldLabel").GetComponent<Text>();
		m_heatLabel = GameObject.Find("UnitHeatLabel").GetComponent<Text>();
		m_healthLabel = GameObject.Find("UnitHealthLabel").GetComponent<Text>();
		m_energyLabel = GameObject.Find("UnitEnergyLabel").GetComponent<Text>();
	}

	public void DisplayInfo(EntityReactor entity)
	{
		if(entity == null)
		{
			this.gameObject.SetActive(false);
			return;
		}

		this.gameObject.SetActive(true);
		m_loyaltyLabel.text = entity.Loyalty.ToString();
		m_nameLabel.text = entity.Name;
		m_healthLabel.text = "{0}/{1}".FormatWith(entity.Health, entity.Template.Health);
		var activeEntity = entity as ActiveEntity;
		if(activeEntity != null)
		{
			m_heatLabel.text = "{0}/{1}".FormatWith(activeEntity.CurrentHeat, activeEntity.Template.MaxHeat);
			m_energyLabel.text = "{0}/{1}".FormatWith(activeEntity.CurrentEnergy, activeEntity.Template.MaxEnergy);
			m_shieldLabel.text = "{0}/{1}".FormatWith(activeEntity.Shield, activeEntity.Template.MaxShields);
		}
	}
}
