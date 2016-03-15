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
	private Text m_movementLabel;

	private Text m_heatKey;
	private Text m_energyKey;
	private Text m_shieldKey;
	private Text m_movementKey;

	private Text m_nothingVisibleLabel;

	private Text[] activeEntityOnlyFields;
	private Text[] movingEntityOnlyFields;

	public GameObject subsystemDataDisplay;

	public void Start()
	{
		m_nameLabel = GameObject.Find("UnitNameLabel").GetComponent<Text>();
		m_loyaltyLabel = GameObject.Find("UnitLoyaltyLabel").GetComponent<Text>();
		m_shieldLabel = GameObject.Find("UnitShieldLabel").GetComponent<Text>();
		m_heatLabel = GameObject.Find("UnitHeatLabel").GetComponent<Text>();
		m_healthLabel = GameObject.Find("UnitHealthLabel").GetComponent<Text>();
		m_energyLabel = GameObject.Find("UnitEnergyLabel").GetComponent<Text>();
		m_movementLabel = GameObject.Find("UnitMovementLabel").GetComponent<Text>();

		m_heatKey = GameObject.Find("UnitHeatKey").GetComponent<Text>();
		m_energyKey = GameObject.Find("UnitEnergyKey").GetComponent<Text>();
		m_shieldKey = GameObject.Find("UnitShieldKey").GetComponent<Text>();
		m_movementKey = GameObject.Find("UnitMovementKey").GetComponent<Text>();

		m_nothingVisibleLabel = GameObject.Find ("NothingVisibleLabel").GetComponent<Text> ();

		activeEntityOnlyFields = new Text[] {m_heatLabel, m_energyLabel, m_shieldLabel, m_heatKey, m_energyKey, m_shieldKey};
		movingEntityOnlyFields = new Text[] {m_movementLabel, m_movementKey};
	}

	public void DisplayInfo(EntityReactor entity)
	{
		if(entity == null)
		{
			this.gameObject.SetActive(false);
			m_nothingVisibleLabel.enabled = false;
			return;
		}

		if (!entity.Hex.VisibleContent ()) {
			// if the entity is not visible, hide all the fields
			foreach (Text oneField in activeEntityOnlyFields) {
				oneField.enabled = false;
			}
			foreach (Text oneField in movingEntityOnlyFields) {
				oneField.enabled = false;
			}
			Debug.Log ("Hex is in FOW");
			m_nothingVisibleLabel.enabled = true;
			return;
		}
		Debug.Log ("Hex is visible");

		this.gameObject.SetActive(true);
		m_nothingVisibleLabel.enabled = false;
		m_loyaltyLabel.text = entity.Loyalty.ToString();
		m_nameLabel.text = entity.Name;
		m_healthLabel.text = "{0}/{1}".FormatWith(entity.Health, entity.Template.Health);
		var activeEntity = entity as ActiveEntity;
		var movingEntity = entity as MovingEntity;

		if (activeEntity != null) {
			foreach (Text oneField in activeEntityOnlyFields) {
				oneField.enabled = true;
			}
			m_heatLabel.text = "{0}/{1}".FormatWith (activeEntity.CurrentHeat, activeEntity.Template.MaxHeat);
			m_energyLabel.text = "{0}/{1}".FormatWith (activeEntity.CurrentEnergy, activeEntity.Template.MaxEnergy);
			m_shieldLabel.text = "{0}/{1}".FormatWith (activeEntity.Shield, activeEntity.Template.MaxShields);

			var subsystems = activeEntity.Systems;
			foreach (Subsystem subsystem in subsystems) {
				Debug.Log ("Found subsystem: " + subsystem.Template.Name);
				GameObject s = CreateSubsystemDataDisplay ();
				SubsystemDataDisplay subsystemController = s.GetComponent<SubsystemDataDisplay> ();
				subsystemController.DisplayInfo (subsystem);
			}
		} else {
			foreach (Text oneField in activeEntityOnlyFields) {
				oneField.enabled = false;
			}
		}
		if (movingEntity != null) {
			foreach (Text oneField in movingEntityOnlyFields) {
				oneField.enabled = true;
			}
			m_movementLabel.text = "{0}/{1}".FormatWith (movingEntity.AvailableSteps, movingEntity.Template.MaxSpeed);
		} else {
			foreach (Text oneField in movingEntityOnlyFields) {
				oneField.enabled = false;
			}
		}
	}

	private GameObject CreateSubsystemDataDisplay () {
		Vector3 displayPosition = new Vector3 (0, 0, 0);
		GameObject s = (GameObject)Instantiate(subsystemDataDisplay, displayPosition, Quaternion.identity);
		s.transform.parent = transform;
		return s;
	}
}
