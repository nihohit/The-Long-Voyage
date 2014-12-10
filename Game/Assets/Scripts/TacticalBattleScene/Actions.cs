using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene
{
    #region PotentialAction

    ///
    /// Potential action represents a certain action, commited by a certain Entity.
    /// When ordered to it can create a button that when pressed activates it,
    /// it can remove the button from the display and it should destroy the button when destroyed.
    /// The button should receive the item's commit method as it's response when pressed.
    ///
    public abstract class PotentialAction
    {
        #region fields

        protected readonly IUnityButton m_button;

        //TODO - remove after testing if no longer needed
        private readonly string m_name;

        #endregion fields

        #region properties

        protected ActiveEntity ActingEntity { get; private set; }

        public HexReactor TargetedHex { get; private set; }

        public bool Destroyed { get; private set; }

        public string Name { get; private set; }

        public Action Callback { get; set; }

        #endregion properties

        #region constructor

        protected PotentialAction(ActiveEntity entity, string buttonName, Vector3 position, HexReactor targetedHex, String name)
        {
            Callback = () => { };
            Destroyed = false;
            Name = buttonName;

            // create a new button for the action, set it to appear when the mouse is over the hex and to trigger the action when clicked
            var command = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("CircularButton"), position, Quaternion.identity));
            command.name = name;
            TacticalState.TextureManager.UpdateButtonTexture(buttonName, command.GetComponent<SpriteRenderer>());
            m_button = command.GetComponent<SimpleButton>();
            m_button.ClickableAction = Commit;
            m_button.OnMouseOverAction = () => targetedHex.OnMouseOverAction();
            m_button.OnMouseExitAction = () => targetedHex.OnMouseExitAction();
            m_button.Unmark();

            m_name = name;
            ActingEntity = entity;
            TargetedHex = targetedHex;

            //Debug.Log("{0} created".FormatWith(this));
        }

        #endregion constructor

        #region public methods

        public virtual void DisplayAction(Vector3 position)
        {
            //if the condition for this command still stands, display it. otherwise destroy it
            if (!Destroyed && NecessaryConditions())
            {
                m_button.Mark(position);
            }
            else
            {
                Destroy();
            }
        }

        public virtual void DisplayAction()
        {
            DisplayAction(m_button.Position);
        }

        public virtual void RemoveDisplay()
        {
            if (!Destroyed)
            {
                m_button.Unmark();
            }
        }

        public virtual void Destroy()
        {
            if (!Destroyed)
            {
                //Debug.Log("{0} destroyed".FormatWith(this));
                RemoveDisplay();
                m_button.DestroyGameObject();
                Destroyed = true;
            }
        }

        public virtual void Commit()
        {
            StartCommit();
            Act(EndCommit);
        }

        public override string ToString()
        {
            return m_name;
        }

        public override bool Equals(object obj)
        {
            var action = obj as PotentialAction;
            return action != null &&
                action.m_button.Equals(m_button) &&
                action.Name.Equals(m_name) &&
                action.ActingEntity.Equals(ActingEntity) &&
                action.TargetedHex.Equals(TargetedHex);
        }

        public override int GetHashCode()
        {
            return Hasher.GetHashCode(m_name,
                TargetedHex,
                ActingEntity,
                m_button);
        }

        //represents the necessary conditions for the action to exist
        public abstract bool NecessaryConditions();

        #endregion public methods

        #region private methods

        //affects the acting entity with the action's costs
        protected abstract void AffectEntity();

        // This is the actual action
        protected abstract void Act(Action callback);

        private void StartCommit()
        {
            //Debug.Log("{0} start commit".FormatWith(this));
            Assert.AssertConditionMet(!Destroyed, "Action {0} was operated after being destroyed".FormatWith(this));
            AffectEntity();
        }

        private void EndCommit()
        {
            //Debug.Log("{0} end commit".FormatWith(this));
            foreach (var action in ActingEntity.Actions.Where(action => !action.NecessaryConditions()))
            {
                action.Destroy();
            }
            //makes it display all buttons;
            TacticalState.SelectedHex = TacticalState.SelectedHex;
            Callback();
        }

        #endregion private methods
    }

    #endregion PotentialAction

    #region MovementAction

    /// <summary>
    /// A potential movement action, into another hex.
    /// TODO - currently doesn't cost any energy
    /// </summary>
    public class MovementAction : PotentialAction
    {
        #region private members

        // the path the entity moves through
        private readonly IEnumerable<HexReactor> m_path;

        //TODO - does walking consume only movement points, or also energy (and if we implement that, produce heat)?
        private readonly double m_cost;

        #endregion private members

        #region constructors

        public MovementAction(MovingEntity entity, IEnumerable<HexReactor> path, double cost) :
            this(entity, path, cost, path.Last())
        { }

        public MovementAction(MovingEntity entity, IEnumerable<HexReactor> path, double cost, HexReactor lastHex) :
            base(entity, "movementMarker", path.Last().Position, lastHex, "{0} move to {1}".FormatWith(entity, path.Last().Coordinates))
        {
            m_path = path;
            m_button.OnMouseOverAction = DisplayPath;
            m_button.OnMouseExitAction = RemovePath;
            m_cost = cost;
            m_button.Scale = new Vector3(m_button.Scale.x * 2, m_button.Scale.y * 2, m_button.Scale.z);
            m_button.Renderer.sortingOrder = 0;
        }

        public MovementAction(MovementAction action, HexReactor hex, double cost) :
            this((MovingEntity)action.ActingEntity, action.m_path.Union(new[] { hex }), cost, hex)
        {
        }

        #endregion constructors

        #region private methods

        private void DisplayPath()
        {
            foreach (var hex in m_path)
            {
                hex.DisplayMovementMarker();
            }
        }

        private void RemovePath()
        {
            foreach (var hex in m_path)
            {
                hex.RemoveMovementMarker();
            }
        }

        #endregion private methods

        #region overloaded methods

        protected override void Act(Action callback)
        {
            TacticalState.SelectedHex = null;
            TacticalState.ResetAllActions();
            ((MovingEntity)ActingEntity).Move(m_path, callback);
        }

        public override void RemoveDisplay()
        {
            base.RemoveDisplay();
            TargetedHex.OnMouseOverAction = () => { };
            TargetedHex.OnMouseExitAction = () => { };
            RemovePath();
        }

        public override void DisplayAction()
        {
            base.DisplayAction();
            TargetedHex.OnMouseOverAction = DisplayPath;
            TargetedHex.OnMouseExitAction = RemovePath;
        }

        protected override void AffectEntity()
        {
            var movingEntity = ActingEntity as MovingEntity;
            Assert.NotNull(movingEntity, "{0} should be a Moving Entity".FormatWith(ActingEntity));
            Assert.EqualOrLesser(m_cost, movingEntity.AvailableSteps,
                 "{0} should have enough movement steps available. Its condition is {1}".
                    FormatWith(ActingEntity, ActingEntity.FullState()));
            movingEntity.AvailableSteps -= m_cost;
        }

        public override bool NecessaryConditions()
        {
            var movingEntity = ActingEntity as MovingEntity;
            Assert.NotNull(movingEntity,
               "{0} should be a Moving Entity".
                   FormatWith(ActingEntity));
            return m_cost <= movingEntity.AvailableSteps;
        }

        #endregion overloaded methods
    }

    #endregion MovementAction

    #region OperateSystemAction

    /// <summary>
    /// Operating a system on a specific hex action.
    /// </summary>
    public class OperateSystemAction : PotentialAction
    {
        private readonly Action m_action;

        public Subsystem System { get; private set; }

        public OperateSystemAction(ActiveEntity actingEntity, HexOperation effect, Subsystem subsystem, HexReactor targetedHex) :
            base(actingEntity, subsystem.Template.Name, (Vector2)targetedHex.Position, targetedHex, "{0} Operate {1} on {2}".FormatWith(actingEntity, subsystem.Template.Name, targetedHex))
        {
            m_action = () => effect(targetedHex);
            System = subsystem;
            RemoveDisplay();
        }

        protected override void Act(Action callback)
        {
            var from = ActingEntity.transform.position;
            var to = TargetedHex.transform.position;
            var shot = ((GameObject)GameObject.Instantiate(Resources.Load("Shot"), from, Quaternion.identity)).GetComponent<Shot>(); ;
            m_button.Renderer.sortingOrder = 1;
            shot.Init(to, from, Name,
                () =>
                {
                    m_action();
                    callback();
                });
        }

        protected override void AffectEntity()
        {
            Assert.EqualOrLesser(System.Template.EnergyCost, ActingEntity.CurrentEnergy,
               "{0} should have enough energy available. Its condition is {1}".
                                FormatWith(ActingEntity, ActingEntity.FullState()));
            ActingEntity.CurrentEnergy -= System.Template.EnergyCost;
            ActingEntity.CurrentHeat += System.Template.HeatGenerated;
        }

        public override bool NecessaryConditions()
        {
            return System.CanOperateNow();
        }

        //TODO - is there a more elegant way to prevent them from displaying?
        public override void DisplayAction()
        {
            if (!Destroyed)
            {
                TargetedHex.DisplayTargetMarker();
            }
        }

        public override void Destroy()
        {
            TargetedHex.RemoveTargetMarker(this);
            base.Destroy();
        }
    }

    #endregion OperateSystemAction

    #region ActionComparerByName

    public class ActionComparerByName : IEqualityComparer<OperateSystemAction>
    {
        private readonly IEqualityComparer<Subsystem> m_systemComparer = new SubsysteByTemplateComparer();

        public bool Equals(OperateSystemAction first, OperateSystemAction second)
        {
            return m_systemComparer.Equals(first.System, second.System);
        }

        public int GetHashCode(OperateSystemAction obj)
        {
            return m_systemComparer.GetHashCode(obj.System);
        }
    }

    #endregion
}