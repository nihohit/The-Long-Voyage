using Assets.Scripts.Base;
using Assets.Scripts.InterSceneCommunication;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.StrategicGameScene
{
    public class StrategicMapScript : MonoBehaviour
    {
        #region private fields

        private LocationScript m_currentLocation;

        private List<Button> m_choiceButtonList;

        private IEnumerable<GameObject> m_nextLocations;

        #endregion private fields

        #region properties

        // ReSharper disable InconsistentNaming
        public GameObject TextPanel;

        public Button InventoryButton;
        public Button Choice1Button;
        public Button Choice2Button;
        public Button Choice3Button;
        public Button Choice4Button;
        public Button DoneButton;
        public Text LocationText;
        // ReSharper restore InconsistentNaming

        #endregion properties

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

            if (m_currentLocation.DoneDisplayingContent)
            {
                RemoveTextualUI();
            }
            else
            {
                this.SetupTextualGui();
            }
        }

        private void SetupTextualGui()
        {
            InventoryButton.gameObject.SetActive(false);

            if (m_currentLocation.Template.Choices == null)
            {
                RemoveChoices();
                return;
            }

            DoneButton.gameObject.SetActive(false);
            LocationText.text = m_currentLocation.Template.Message;

            Assert.EqualOrGreater(
                m_choiceButtonList.Count,
                m_currentLocation.Template.Choices.Count(),
                "There are more LocationScript options then buttons.\n options: {0}".FormatWith(
                    string.Join("\n", m_currentLocation.Template.Choices.Select(choice => choice.Description).ToArray())));

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
        }

        private void SetButton(Button button, PlayerActionChoice choice)
        {
            button.onClick.AddListener(() => Choose(choice.Choose()));
            var buttonText = button.GetComponentInChildren<Text>();
            buttonText.text = choice.Template.Description;
        }

        private void Choose(string choiceMessage)
        {
            LocationText.text = choiceMessage;
            RemoveChoices();
        }

        private void RemoveChoices()
        {
            foreach (var button in m_choiceButtonList)
            {
                button.gameObject.SetActive(false);
            }

            DoneButton.gameObject.SetActive(true);
            DoneButton.onClick.AddListener(this.RemoveTextualUI);
        }

        private void RemoveTextualUI()
        {
            m_currentLocation.DoneDisplayingContent = true;
            TextPanel.SetActive(false);
            InventoryButton.gameObject.SetActive(true);
            InventoryButton.onClick.AddListener(() => Application.LoadLevel("InventoryScene"));
            //AddNextLocations();
        }

        private void AddNextLocations()
        {
            foreach (var nextLocation in m_currentLocation.NextLocations)
            {
                nextLocation.Seen();
                //nextLocation.
            }
        }

        // TODO - remove when this scene won't be accessed directly.
        private void InitGlobalState()
        {
            if (GlobalState.Instance.ActiveGame)
            {
                return;
            }

            CreateLocations();
        }

        private void CreateLocations()
        {
            var currentLocation = LocationScript.CreateLocationScript(
                Vector2.zero,
                new LocationTemplate(
                    "Check",
                    "This is a check",
                    new[]{
                            new ChoiceTemplate("First action", ChoiceResults.None, "Nothing happend"),
                            new ChoiceTemplate("Lose mech", ChoiceResults.LoseMech, "You lost a mech"),
                            new ChoiceTemplate("Get Mech", ChoiceResults.GetMech, "You got a mech"),
                            new ChoiceTemplate("Get Mech", ChoiceResults.GetMech, "You got a mech"),
                        }),
                null);

            GlobalState.Instance.StartNewGame("Default");
            GlobalState.Instance.StrategicMap.CurrentLocation = currentLocation;
            GlobalState.Instance.DefaultInitialization();
        }
    }
}