using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;
using Assets.Scripts.UnityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene
{
    #region actions

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

        private bool m_active;

        #endregion fields

        #region properties

        protected ActiveEntity ActingEntity { get; private set; }

        public HexReactor TargetedHex { get; private set; }

        public bool Destroyed { get; private set; }

        public string Name { get; private set; }

        #endregion properties

        #region constructor

        protected PotentialAction(ActiveEntity entity, string buttonName, Vector3 position, HexReactor targetedHex, String name)
        {
            m_active = false;
            Destroyed = false;
            Name = buttonName;

            // create a new button for the action, set it to appear when the mouse is over the hex and to trigger the action when clicked
            var command = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("CircularButton"), position, Quaternion.identity));
            command.name = name;
            TacticalState.TextureManager.UpdateButtonTexture(buttonName, command.GetComponent<SpriteRenderer>());
            m_button = command.GetComponent<SimpleButton>();
            m_button.ClickableAction = () =>
            {
                m_active = true;
                Commit();
            };
            m_button.OnMouseOverAction = () => targetedHex.OnMouseOverAction();
            m_button.OnMouseExitAction = () => targetedHex.OnMouseExitAction();
            m_button.Unmark();

            m_name = name;
            ActingEntity = entity;
            TargetedHex = targetedHex;
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
                RemoveDisplay();
                m_button.DestroyGameObject();
                Destroyed = true;
            }
        }

        public virtual void Commit()
        {
            Assert.AssertConditionMet((!Destroyed) || m_active, "Action {0} was operated after being destroyed".FormatWith(this));
            Debug.Log("{0} committing {1}".FormatWith(ActingEntity, this));
            m_active = false;
            AffectEntity();
            foreach (var action in ActingEntity.Actions)
            {
                if (!action.NecessaryConditions())
                {
                    action.Destroy();
                }
            }
            //makes it display all buttons;
            TacticalState.SelectedHex = TacticalState.SelectedHex;
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
            base(entity, "movementMarker", path.Last().Position, lastHex, "movement to {0}".FormatWith(path.Last().Coordinates))
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

        public override void Destroy()
        {
            TargetedHex.OnMouseOverAction = () => { };
            TargetedHex.OnMouseExitAction = () => { };
            RemovePath();
            base.Destroy();
        }

        public override void Commit()
        {
            base.Commit();
            ((MovingEntity)ActingEntity).Move(m_path);
            TacticalState.SelectedHex = null;
            //TODO - should effects on commiting entity be calculated here? Energy / heat cost, etc.?
            Destroy();
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

        public SubsystemTemplate System { get; private set; }

        public OperateSystemAction(ActiveEntity actingEntity, HexOperation effect, SubsystemTemplate template, HexReactor targetedHex) :
            base(actingEntity, template.Name, (Vector2)targetedHex.Position, targetedHex, "Operate {0} on {1}".FormatWith(template.Name, targetedHex))
        {
            m_action = () => effect(targetedHex);
            System = template;
            RemoveDisplay();
        }

        public override void Commit()
        {
            base.Commit();
            var from = ActingEntity.transform.position;
            var to = TargetedHex.transform.position;
            var shot = ((GameObject)GameObject.Instantiate(Resources.Load("Shot"), from, Quaternion.identity)).GetComponent<Shot>(); ;
            m_button.Renderer.sortingOrder = 1;
            shot.Init(to, from, Name);
            m_action();
            TargetedHex.DisplayCommands(true);
        }

        protected override void AffectEntity()
        {
            Assert.EqualOrLesser(System.EnergyCost, ActingEntity.CurrentEnergy,
               "{0} should have enough energy available. Its condition is {1}".
                                FormatWith(ActingEntity, ActingEntity.FullState()));
            ActingEntity.CurrentEnergy -= System.EnergyCost;
            ActingEntity.CurrentHeat += System.HeatGenerated;
        }

        public override bool NecessaryConditions()
        {
            return System.EnergyCost <= ActingEntity.CurrentEnergy;
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

    #endregion actions
}