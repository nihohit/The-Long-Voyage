using Assets.Scripts.Base;
using Assets.Scripts.InterSceneCommunication;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.StrategicGameScene {
  using LogicBase;
  using MapGeneration;
  using UnityBase;

  public class StrategicMapScript : SceneBehaviour {
    #region private fields

    private LocationInformation m_currentLocation;

    private List<Button> m_choiceButtonList;

    private GameObject m_locationsParent;

    #endregion private fields

    #region properties

    // ReSharper disable InconsistentNaming
    public GameObject TextPanel;

    public Button LoadupButton;
    public Button DoneButton;
    public Text LocationText;
    public MarkerScript Marker;

    // ReSharper restore InconsistentNaming

    #endregion properties

    // Use this for initialization
    private void Start() {
      m_locationsParent = new GameObject();
      m_locationsParent.name = "Locations";
      m_choiceButtonList = GameObject.Find("MainUI").GetComponentsInChildren<Button>().Where(button => button.name.Equals("ChoiceButton")).ToList();

      InitGlobalState();

      m_currentLocation = GlobalState.Instance.StrategicMap.CurrentLocation;
      Camera.main.transform.position = new Vector3(m_currentLocation.Coordinates.x, m_currentLocation.Coordinates.y, Camera.main.transform.position.z);

      // if we're after a battle, add the battle salvage to our equipment
      if (GlobalState.Instance.BattleSummary != null) {
        var battleResult = GlobalState.Instance.BattleSummary;
        GlobalState.Instance.BattleSummary = null;
        GlobalState.Instance.StrategicMap.State.AvailableEntities.AddRange(battleResult.SalvagedEntities);
        GlobalState.Instance.StrategicMap.State.AvailableSystems.AddRange(battleResult.SalvagedSystems);
        GlobalState.Instance.StrategicMap.State.EquippedEntities.Clear();
        GlobalState.Instance.StrategicMap.State.EquippedEntities.AddRange(battleResult.SurvivingEntities);
      }

      if (m_currentLocation.WasVisited) {
        RemoveTextualUI();
      } else {
        m_currentLocation.WasVisited = true;
        this.SetupTextualGui(m_currentLocation.Vignette);
      }
    }

    private void SetupTextualGui(VignetteTemplate vignette) {
      LoadupButton.gameObject.SetActive(false);
      Marker.Visible = false;
      LocationText.text = vignette.Message;

      HandleResult(vignette.Result);

      if (vignette.Choices.IsNullOrEmpty()) {
        RemoveChoices();
        return;
      }

      DoneButton.gameObject.SetActive(false);

      var availableChoices = vignette.Choices.Where(choice => choice.Condition.Available()).ToList();
      var options = availableChoices.Select(choice => choice.Description).ToJoinedString("\n");

      Assert.EqualOrGreater(
        m_choiceButtonList.Count,
        availableChoices.Count,
        "There are more LocationScript options then buttons.\n options: {0}".FormatWith(options));

      for (int i = 0; i < availableChoices.Count; i++) {
        var button = m_choiceButtonList[i];

        var choice = availableChoices.ElementAt(i);
        SetButton(button, choice);
      }

      for (int i = availableChoices.Count; i < m_choiceButtonList.Count; i++) {
        m_choiceButtonList[i].gameObject.SetActive(false);
      }
    }

    private void SetButton(Button button, ChoiceTemplate choice) {
      button.onClick.AddListener(() => Choose(choice));
      var buttonText = button.GetComponentInChildren<Text>();
      buttonText.text = choice.Description;
    }

    private void Choose(ChoiceTemplate choiceTemplate) {
      RemoveChoices();
      SetupTextualGui(choiceTemplate.Result);
    }

    private void HandleResult(EventResult eventResult) {
      if (eventResult == null) {
        return;
      }

      if (eventResult.ResultType.HasFlag(EventResultType.AffectRelations)) {
        AffectRelations(eventResult.Key, eventResult.Value);
      }

      if (eventResult.ResultType.HasFlag(EventResultType.Fight)) {
        DoneButton.onClick.RemoveAllListeners();
        DoneButton.onClick.AddListener(() => this.StartBattle());
      }
    }

    private void StartBattle() {
      UnityEngine.SceneManagement.SceneManager.LoadScene("TacticalBattleScene");
    }

    private void AffectRelations(string faction, double affect) {
      GlobalState.Instance.StrategicMap.State.Relations[faction] =
        GlobalState.Instance.StrategicMap.State.Relations.TryGetOrAdd(faction, () => 0) + affect;
    }

    private void RemoveChoices() {
      foreach (var button in m_choiceButtonList) {
        button.gameObject.SetActive(false);
      }

      DoneButton.gameObject.SetActive(true);
      DoneButton.onClick.AddListener(this.RemoveTextualUI);
    }

    private void RemoveTextualUI() {
      TextPanel.SetActive(false);
      LoadupButton.gameObject.SetActive(true);
      Timer.Instance.TimedAction(DisplayWorldMap, "DisplayWorldMap");
      Marker.Mark(m_currentLocation.Coordinates);
      Marker.Visible = true;
    }

    private void DisplayWorldMap() {
      var roads = new GameObject("roads");

      foreach (var currentLocationInfo in GlobalState.Instance.StrategicMap.Map) {
        var newLocation = LocationScript.CreateLocationScript(currentLocationInfo);
        newLocation.transform.SetParent(m_locationsParent.transform);

        foreach (var nextLocationInfo in currentLocationInfo.ConnectedLocations) {
          // TODO: if we want to avoid double lines, we ca add an order, or name the objects and check
          var lineRenderer = new GameObject().AddComponent<LineRenderer>();
          lineRenderer.transform.SetParent(roads.transform);
          lineRenderer.gameObject.name = "LineFrom {0} to {1}".FormatWith(
            nextLocationInfo.Coordinates,
            currentLocationInfo.Coordinates);
          lineRenderer.SetVertexCount(2);
          lineRenderer.SetPosition(0, nextLocationInfo.Coordinates);
          lineRenderer.SetPosition(1, currentLocationInfo.Coordinates);
          lineRenderer.SetColors(Color.black, Color.black);
          lineRenderer.SetWidth(0.1f, 0.1f);
          var whiteDiffuseMat = new Material(Shader.Find("Sprites/Default"));
          lineRenderer.material = whiteDiffuseMat;
        }

        if (m_currentLocation.ConnectedLocations.Contains(currentLocationInfo)) {
          newLocation.ClickableAction = () => MoveToLocation(newLocation.Information);
        }

      }
    }

    private void MoveToLocation(LocationInformation locationInformation) {
      UnityEngine.Debug.Log("Moving to {0}".FormatWith(locationInformation));
      GlobalState.Instance.StrategicMap.CurrentLocation = locationInformation;
      UnityEngine.SceneManagement.SceneManager.LoadScene("StrategicMapScene");
    }

    // TODO - remove when this scene won't be accessed directly.
    private void InitGlobalState() {
      if (GlobalState.Instance.ActiveGame) {
        return;
      }

      GlobalState.Instance.StartNewGame("Default");

      var scenario = GlobalState.Instance.Configurations.StartingScenarios.GetAllConfigurations().First();
      GlobalState.Instance.StrategicMap.State.EquippedEntities.AddRange(scenario.PlayerMechs);
      GlobalState.Instance.StrategicMap.State.AvailableEntities.Add(new SpecificEntity(
        GlobalState.Instance.Configurations.ActiveEntities.GetAllConfigurations().ChooseRandomValue()));
      GlobalState.Instance.StrategicMap.State.AvailableSystems.Add(
        GlobalState.Instance.Configurations.Subsystems.GetAllConfigurations().ChooseRandomValue());
      GlobalState.Instance.StrategicMap.State.AvailableEntities.Add(new SpecificEntity(
        GlobalState.Instance.Configurations.ActiveEntities.GetAllConfigurations().ChooseRandomValue()));
      GlobalState.Instance.StrategicMap.State.AvailableSystems.Add(
        GlobalState.Instance.Configurations.Subsystems.GetAllConfigurations().ChooseRandomValue());

      Timer.Instance.TimedAction(CreateLocations, "CreateLocations");
    }

    private void CreateLocations() {
      IWorldGenerator generator = new SetWorldGenerator();
      var worldMap = generator.GenerateStrategicMap();
      GlobalState.Instance.StrategicMap.Map.AddRange(worldMap);
      GlobalState.Instance.StrategicMap.CurrentLocation = worldMap
        .First(
          location => location.Biome != Biome.Mountain &&
          location.Biome != Biome.Sea);
    }
  }
}