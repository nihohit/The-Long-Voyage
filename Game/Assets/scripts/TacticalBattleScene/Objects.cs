using Assets.scripts.Base;
using Assets.scripts.LogicBase;
using Assets.scripts.UnityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.scripts.TacticalBattleScene
{
    #region enums

    public enum Biome { Tundra, City, Grass, Desert, Swamp }

    [Flags]
    public enum HexEffect
    {
        None = 0,
        Heating = 1,
        Chilling = 2,
    }

    //the logic behind the numbering is that the addition of this enumerator and MovementType gives the following result - if the value is between 0-5, no movement penalty. above 4 - slow, above 6 - impossible
    public enum TraversalConditions
    {
        Easy = 0,
        Uneven = 1, //hard to crawl, everything else is fine
        Broken = 2, //hard to crawl or walk, everything else is fine
        NoLand = 4, //can't crawl or walk, everything else is fine
        Blocked = 5 //can only fly
    }

    // there needs to be an order of importance - the more severe damage has a higher value
    public enum SystemCondition { Operational = 0, OutOfAmmo = 1, Neutralized = 2, Destroyed = 3 }

    #endregion enums

    #region delegates

    public delegate bool EntityCheck(TacticalEntity ent);

    public delegate void HexOperation(Hex hex);

    public delegate double HexTraversalCost(Hex hex);

    public delegate bool HexCheck(Hex hex);

    #endregion delegates

    #region actions

    #region PotentialAction

    /*
 * Potential action represents a certain action, commited by a certain Entity.
 * When ordered to it can create a button that when pressed activates it,
 * it can remove the button from the display and it should destroy the button when destroyed.
 * The button should receive the item's commit method as it's response when pressed.
 */

    public abstract class PotentialAction
    {
        #region fields

        protected readonly SimpleButton m_button;

        //TODO - remove after testing if no longer needed
        private readonly string m_name;

        private bool m_active;

        #endregion fields

        #region properties

        protected ActiveEntity ActingEntity { get; private set; }

        public Hex TargetedHex { get; private set; }

        public bool Destroyed { get; private set; }

        public string Name { get; private set; }

        #endregion properties

        #region constructor

        protected PotentialAction(ActiveEntity entity, string buttonName, Vector3 position, Hex targetedHex, String name)
        {
            m_active = false;
            Destroyed = false;
            Name = buttonName;
            var command = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("Button"), position, Quaternion.identity));
            command.name = name;
            TacticalState.TextureManager.UpdateButtonTexture(buttonName, command.GetComponent<SpriteRenderer>());
            m_button = command.GetComponent<SimpleButton>();
            m_button.ClickableAction = () =>
            {
                m_active = true;
                Commit();
            };
            m_button.OnMouseOverProperty = () => targetedHex.Reactor.OnMouseOverProperty();
            m_button.OnMouseExitProperty = () => targetedHex.Reactor.OnMouseExitProperty();
            m_button.Unmark();
            m_name = name;
            ActingEntity = entity;
            TargetedHex = targetedHex;
        }

        #endregion constructor

        #region public methods

        public virtual void DisplayButton(Vector3 position)
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

        public virtual void DisplayButton()
        {
            DisplayButton(m_button.transform.position);
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
            Debug.Log("{0} committing {1}".FormatWith(ActingEntity, m_name));
            Assert.AssertConditionMet((!Destroyed) || m_active, "Action {0} was operated after being destroyed".FormatWith(this));
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

    public class MovementAction : PotentialAction
    {
        #region private members

        private readonly IEnumerable<Hex> m_path;

        //TODO - does walking consume only movement points, or also energy (and if we implement that, produce heat)?
        private readonly double m_cost;

        #endregion private members

        #region constructors

        public MovementAction(ActiveEntity entity, IEnumerable<Hex> path, double cost) :
            this(entity, path, cost, path.Last())
        { }

        public MovementAction(ActiveEntity entity, IEnumerable<Hex> path, double cost, Hex lastHex) :
            base(entity, "movementMarker", path.Last().Position, lastHex, "movement to {0}".FormatWith(path.Last().Coordinates))
        {
            m_path = path;
            m_button.OnMouseOverProperty = DisplayPath;
            m_button.OnMouseExitProperty = RemovePath;
            m_cost = cost;
            m_button.transform.localScale = new Vector3(m_button.transform.localScale.x * 2, m_button.transform.localScale.y * 2, m_button.transform.localScale.z);
            m_button.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 0;
        }

        public MovementAction(MovementAction action, Hex hex, double cost) :
            this(action.ActingEntity, action.m_path.Union(new[] { hex }), cost, hex)
        {
        }

        #endregion constructors

        #region private methods

        private void DisplayPath()
        {
            foreach (var hex in m_path)
            {
                hex.Reactor.DisplayMovementMarker();
            }
        }

        private void RemovePath()
        {
            foreach (var hex in m_path)
            {
                hex.Reactor.RemoveMovementMarker();
            }
        }

        #endregion private methods

        #region overloaded methods

        public override void RemoveDisplay()
        {
            base.RemoveDisplay();
            TargetedHex.Reactor.OnMouseOverProperty = () => { };
            TargetedHex.Reactor.OnMouseExitProperty = () => { };
            RemovePath();
        }

        public override void DisplayButton()
        {
            base.DisplayButton();
            TargetedHex.Reactor.OnMouseOverProperty = DisplayPath;
            TargetedHex.Reactor.OnMouseExitProperty = RemovePath;
        }

        public override void Destroy()
        {
            TargetedHex.Reactor.OnMouseOverProperty = () => { };
            TargetedHex.Reactor.OnMouseExitProperty = () => { };
            RemovePath();
            base.Destroy();
        }

        public override void Commit()
        {
            base.Commit();
            TargetedHex.Content = ActingEntity;
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

    public class OperateSystemAction : PotentialAction
    {
        private readonly Action m_action;

        public SubsystemTemplate System { get; private set; }

        public OperateSystemAction(ActiveEntity entity, HexOperation effect, SubsystemTemplate template, Hex targetedHex) :
            base(entity, template.Name, (Vector2)targetedHex.Position, targetedHex, "Operate {0} on {1}".FormatWith(template.Name, targetedHex))
        {
            m_action = () => effect(targetedHex);
            System = template;
            RemoveDisplay();
        }

        public override void Commit()
        {
            base.Commit();
            var from = ActingEntity.Reactor.transform.position;
            var to = TargetedHex.Reactor.transform.position;
            var shot = ((GameObject)GameObject.Instantiate(Resources.Load("Shot"), from, Quaternion.identity)).GetComponent<Shot>(); ;
            m_button.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;
            shot.Init(to, from, Name);
            m_action();
            TargetedHex.Reactor.DisplayCommands(true);
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
        public override void DisplayButton()
        {
            if (!Destroyed)
            {
                TargetedHex.Reactor.DisplayTargetMarker();
            }
        }

        public override void Destroy()
        {
            TargetedHex.Reactor.RemoveTargetMarker(this);
            base.Destroy();
        }
    }

    #endregion OperateSystemAction

    #endregion actions
}