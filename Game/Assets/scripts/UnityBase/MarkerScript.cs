using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Base;
using UnityEngine;

namespace Assets.Scripts.UnityBase
{
    /// <summary>
    /// A script meant to handle movement and display of sprites.
    /// </summary>
    public class MarkerScript : MonoBehaviour, IUnityMarker
    {
        #region private fields

        private bool m_moveWithRotation;

        private readonly Queue<MoveOrder> m_movementRoute = new Queue<MoveOrder>();

        private MoveOrder m_currentOrder;

        private const double c_minDistance = 0.4;

        private float m_moveSpeed;

        #endregion private fields

        #region properties

        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        public Vector3 Scale
        {
            get { return transform.localScale; }
            set { transform.localScale = value; }
        }

        public bool Visible
        {
            get
            {
                return Renderer.enabled;
            }
            set
            {
                if (value)
                {
                    Mark();
                }
                else
                {
                    Unmark();
                }
            }
        }

        public SpriteRenderer Renderer { get { return gameObject.GetComponent<SpriteRenderer>(); } }

        #endregion properties

        #region public methods

        // Displays the sprite at the given location.
        public virtual void Mark(Vector3 position)
        {
            enabled = true;
            Renderer.enabled = true;
            Position = position;
        }

        public virtual void Mark()
        {
            Mark(Position);
        }

        public virtual void Unmark()
        {
            Renderer.enabled = false;
            enabled = false;
        }

        public void DestroyGameObject()
        {
            UnityEngine.Object.Destroy(gameObject);
        }

        public void BeginMove(IEnumerable<MoveOrder> orders, float movementSpeed, bool withRotation)
        {
            // ensure that the marker isn't currently in motion
            Assert.IsNull(m_currentOrder, "m_currentOrder");
            Assert.IsEmpty(m_movementRoute, "m_movementRoute");
            Assert.NotNullOrEmpty(orders, "orders");
            Assert.Greater(movementSpeed, 0);

            m_moveSpeed = movementSpeed;
            m_moveWithRotation = withRotation;
            foreach (var order in orders)
            {
                m_movementRoute.Enqueue(order);
            }
            m_currentOrder = m_movementRoute.Dequeue();
        }

        // runs on every frame
        public virtual void Update()
        {
            if (IsMoving())
            {
                MoveTowardsCurrentPoint();

                if (transform.position.Distance(m_currentOrder.Point) < c_minDistance)
                {
                    transform.position = m_currentOrder.Point;
                    var currentOrder = m_currentOrder;
                    m_currentOrder = m_movementRoute.Any() ? m_movementRoute.Dequeue() : null;
                    currentOrder.ArrivalCallback();
                }
            }
        }

        protected bool IsMoving()
        {
            return m_currentOrder != null;
        }

        #endregion public methods

        #region private methods

        private void MoveTowardsCurrentPoint()
        {
            // if the last frame took too much time, skip this frame.
            if (Time.deltaTime > 0.05f)
            {
                return;
            }

            var direction = m_currentOrder.Point - transform.position;
            var moveVector = direction.normalized * m_moveSpeed * Time.deltaTime;
            transform.position += moveVector;

            if (m_moveWithRotation)
            {
                var directionAngle = direction.ToRotationVector();
                var lerpVector = transform.rotation.eulerAngles
                    .LerpAngle(directionAngle,
                        m_moveSpeed * Time.deltaTime);
                transform.eulerAngles = lerpVector;
            }
        }

        #endregion private methods
    }
}