using Assets.Scripts.Base;
using Assets.Scripts.UnityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene {
  #region PotentialAction

  ///
  /// Potential action represents a certain action, committed by a certain Entity.
  /// When ordered to it can create a button that when pressed activates it,
  /// it can remove the button from the display and it should destroy the button when destroyed.
  /// The button should receive the item's commit method as it's response when pressed.
  ///
  public abstract class PotentialAction {
    #region properties

    protected ActiveEntity ActingEntity { get; private set; }

    public HexReactor TargetedHex { get; private set; }

    public string Name { get; private set; }

    public Action Callback { get; set; }

    #endregion properties

    #region constructor

    protected PotentialAction(ActiveEntity entity, string name, HexReactor targetedHex) {
      Callback = () => { };
      Name = name;
      ActingEntity = entity;
      TargetedHex = targetedHex;
    }

    #endregion constructor

    #region public methods

    public virtual void Commit() {
      StartCommit();
      Act(Callback);
      TacticalState.SelectedHex = TacticalState.SelectedHex;
    }

    public override string ToString() {
      return Name;
    }

    public override bool Equals(object obj) {
      var action = obj as PotentialAction;
      return action != null &&
        action.ActingEntity.Equals(ActingEntity) &&
        action.TargetedHex.Equals(TargetedHex);
    }

    public override int GetHashCode() {
      return Hasher.GetHashCode(
        Name,
        TargetedHex,
        ActingEntity);
    }

    #endregion public methods

    #region private methods

    private void StartCommit() {
      // Debug.Log("{0} start commit".FormatWith(this));
      Assert.AssertConditionMet(NecessaryConditions(), "Action {0} was operated when necessary conditions weren't possible".FormatWith(this));
      AffectEntity();
    }

    #endregion private methods

    #region abstract methods

    // represents the necessary conditions for the action to exist
    public abstract bool NecessaryConditions();

    // affects the acting entity with the action's costs
    protected abstract void AffectEntity();

    // This is the actual action
    protected abstract void Act(Action callback);

    #endregion abstract methods
  }

  #endregion PotentialAction

  #region MovementAction

  /// <summary>
  /// A potential movement action, into another hex.
  /// TODO - currently doesn't cost any energy
  /// </summary>
  public class MovementAction : PotentialAction {
    #region private members

    // the path the entity moves through
    private readonly IEnumerable<HexReactor> m_path;

    //TODO - does walking consume only movement points, or also energy (and if we implement that, produce heat)?
    private readonly double m_cost;

    #endregion private members

    #region constructors

    public MovementAction(MovingEntity entity, IEnumerable<HexReactor> path, double cost) :
      this(entity, path, cost, path.Last()) { }

    public MovementAction(MovingEntity entity, IEnumerable<HexReactor> path, double cost, HexReactor lastHex) :
      base(entity, "movementMarker", lastHex) {
      m_path = path;
      m_cost = cost;
    }

    public MovementAction(MovementAction action, HexReactor hex, double cost) :
      this((MovingEntity)action.ActingEntity, action.m_path.Union(new[] { hex }), cost, hex) {
    }

    #endregion constructors

    #region private methods

    private void DisplayPath() {
      foreach (var hex in m_path) {
        hex.DisplayMovementMarker();
      }
    }

    private void RemovePath() {
      foreach (var hex in m_path) {
        hex.RemoveMovementMarker();
      }
    }

    #endregion private methods

    #region overloaded methods

    protected override void Act(Action callback) {
      TacticalState.SelectedHex = null;
      ((MovingEntity)ActingEntity).Move(m_path, callback);
    }

    protected override void AffectEntity() {
      var movingEntity = ActingEntity.SafeCast<MovingEntity>("ActingEntity");
      Assert.EqualOrLesser(
        m_cost,
        movingEntity.AvailableSteps,
        "{0} should have enough movement steps available. Its condition is {1}".FormatWith(
          ActingEntity,
          ActingEntity.FullState()));
      movingEntity.AvailableSteps -= m_cost;
    }

    public override bool NecessaryConditions() {
      var movingEntity = ActingEntity.SafeCast<MovingEntity>("ActingEntity");
      return m_cost <= movingEntity.AvailableSteps;
    }

    #endregion overloaded methods
  }

  #endregion MovementAction

  #region OperateSystemAction

  /// <summary>
  /// Operating a system on a specific hex action.
  /// </summary>
  public class OperateSystemAction : PotentialAction {
    private readonly Action r_action;

    public Subsystem System { get; private set; }

    public OperateSystemAction(ActiveEntity actingEntity, HexOperation effect, Subsystem subsystem, HexReactor targetedHex) :
      base(
        actingEntity,
        subsystem.Template.Name,
        targetedHex) {
      this.r_action = () => effect(targetedHex);
      System = subsystem;
    }

    protected override void Act(Action callback) {
      var from = ActingEntity.transform.position;
      var to = TargetedHex.transform.position;
      var shot = UnityHelper.Instantiate<Shot>(from);
      System.Act();
      shot.Init(
        to,
        from,
        Name,
        () => {
          this.r_action();
          callback();
        });
    }

    protected override void AffectEntity() {
      Assert.EqualOrLesser(
        System.Template.EnergyCost,
        ActingEntity.CurrentEnergy,
        "{0} should have enough energy available. Its condition is {1}".FormatWith(ActingEntity, ActingEntity.FullState()));
      ActingEntity.CurrentEnergy -= System.Template.EnergyCost;
      ActingEntity.CurrentHeat += System.Template.HeatGenerated;
    }

    public override bool NecessaryConditions() {
      return System.CanOperateNow();
    }
  }

  #endregion OperateSystemAction

  #region ActionComparerByName

  public class ActionComparerByName : IEqualityComparer<OperateSystemAction> {
    private readonly IEqualityComparer<Subsystem> m_systemComparer = new SubsysteByTemplateComparer();

    public bool Equals(OperateSystemAction first, OperateSystemAction second) {
      return m_systemComparer.Equals(first.System, second.System);
    }

    public int GetHashCode(OperateSystemAction obj) {
      return m_systemComparer.GetHashCode(obj.System);
    }
  }

  #endregion ActionComparerByName
}