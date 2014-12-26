using UnityEngine;
using Assets.Scripts.InterSceneCommunication;
using Assets.Scripts.Base;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace Assets.Scripts.StrategicGameScene
{
    public class StrategicMapScript : MonoBehaviour
    {
        #region private fields

        private Location m_currentLocation;

        private string m_chosenDecision = null;

        private List<Button> m_choiceButtonList;

        #endregion

        #region properties

        public GameObject TextPanel;
        public Button InventoryButton;
        public Button Choice1Button;
        public Button Choice2Button;
        public Button Choice3Button;
        public Button Choice4Button;
        public Button DoneButton;
        public Text LocationText;

        #endregion

        // Use this for initialization
        private void Start()
        {
            m_choiceButtonList = new List<Button> { Choice1Button, Choice2Button, Choice3Button, Choice4Button };

            InitGlobalState();

            m_currentLocation = GlobalState.Instance.StrategicMap.CurrentLocation;

            // if we're after a battle, add the battle salvage to our eqiupment
            if (GlobalState.Instance.BattleSummary != null)
            {
                var battleResult = GlobalState.Instance.BattleSummary;
                GlobalState.Instance.BattleSummary = null;
                GlobalState.Instance.StrategicMap.State.AvailableEntities.AddRange(battleResult.SalvagedEntities);
                GlobalState.Instance.StrategicMap.State.AvailableSystems.AddRange(battleResult.SalvagedSystems);
                GlobalState.Instance.StrategicMap.State.EquippedEntities.Clear();
                GlobalState.Instance.StrategicMap.State.EquippedEntities.AddRange(battleResult.SurvivingEntities);
            }

            if (m_currentLocation.DoneChoosing)
            {
                RemoveTextualUI();
            }
            else
            {
                SetupTextualUI();
            }
        }

        private void SetupTextualUI()
        {
            InventoryButton.gameObject.SetActive(false);
            DoneButton.gameObject.SetActive(false);
            LocationText.text = m_currentLocation.Template.Message;

            Assert.EqualOrGreater(m_choiceButtonList.Count, m_currentLocation.Template.Choices.Count(),
                "There are more location options then buttons.\n options: {0}".FormatWith(
                string.Join("\n",
                    m_currentLocation.Template.Choices.Select(choice => choice.Description).ToArray())));

            for (int i = 0; i < m_currentLocation.Template.Choices.Count(); i++)
            {
                var button = m_choiceButtonList[i];
                var choice = m_currentLocation.Choices.ElementAt(i);
                SetButton(button, choice);
            }

            for (int i = m_currentLocation.Template.Choices.Count(); i < m_choiceButtonList.Count; i++)
            {
                m_choiceButtonList[i].gameObject.SetActive(false);
            }

            DoneButton.onClick.AddListener(this.RemoveTextualUI);
        }

        private void SetButton(Button button, PlayerActionChoice choice)
        {
            button.onClick.AddListener(() => Choose(choice.Choose()));
            var buttonText = button.GetComponentInChildren<Text>();
            buttonText.text = choice.Template.Description;
        }

        private void Choose(string choiceMessage)
        {
            m_chosenDecision = choiceMessage;
            foreach (var button in m_choiceButtonList)
            {
                button.gameObject.SetActive(false);
            }

            DoneButton.gameObject.SetActive(true);
            LocationText.text = choiceMessage;
        }

        private void RemoveTextualUI()
        {
            TextPanel.SetActive(false);
            InventoryButton.gameObject.SetActive(true);
            InventoryButton.onClick.AddListener(() => Application.LoadLevel("InventoryScene"));
        }

        // TODO - remove when this scene won't be accessed directly.
        private void InitGlobalState()
        {
            if (GlobalState.Instance.ActiveGame)
            {
                return;
            }

            var currentLocation = new Location(
                new LocationTemplate(
                    "This is a check",
                    new[]
                        {
                            new PlayerActionChoiceTemplate("First action", 1, ChoiceResults.None, "Nothing happend"),
                            new PlayerActionChoiceTemplate("Lose mech", 1, ChoiceResults.LoseMech, "You lost a mech"),
                            new PlayerActionChoiceTemplate("Get Mech", 1, ChoiceResults.GetMech, "You got a mech"),
                        }),
                null);

            GlobalState.Instance.StartNewGame("Default");
            GlobalState.Instance.StrategicMap.CurrentLocation = currentLocation;
            GlobalState.Instance.DefaultInitialization();
        }
    }
}