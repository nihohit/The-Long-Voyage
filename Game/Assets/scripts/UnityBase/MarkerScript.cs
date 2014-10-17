using Assets.Scripts.Base;
using System.Collections.Generic;
using System.Linq;
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

        private Queue<MoveOrder> m_movementRoute = new Queue<MoveOrder>();

        private MoveOrder m_currentOrder;

        private static double s_minDistance = 0.2;

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
            this.enabled = true;
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
            this.enabled = false;
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

        #endregion public methods

        #region private methods

        // runs on every frame
        private void Update()
        {
            if (m_currentOrder != null)
            {
                MoveTowardsCurrentPoint();

                if (transform.position.Distance(m_currentOrder.Point) < s_minDistance)
                {
                    m_currentOrder.ArrivalCallback();
                    if (m_movementRoute.Any())
                    {
                        m_currentOrder = m_movementRoute.Dequeue();
                    }
                    else
                    {
                        m_currentOrder = null;
                    }
                }
            }
        }

        private void MoveTowardsCurrentPoint()
        {
            var direction = m_currentOrder.Point - transform.position;
            var moveVector = direction.normalized * m_moveSpeed * Time.deltaTime;
            transform.position += moveVector;
            var eulerAngles = transform.rotation.eulerAngles;
            Debug.Log("first euler: {0}".FormatWith(eulerAngles));
            /*
            if (eulerAngles.z > 180)
            {
                eulerAngles.z = eulerAngles.z - 360;
                Debug.Log("last euler: {0}".FormatWith(eulerAngles));
            }
             */
            var rotationVector = direction.ToRotationVector();
            Debug.Log("rotation: {0}".FormatWith(rotationVector));
            Vector3 lerpVector = transform.rotation.eulerAngles.LerpAngle(direction.ToRotationVector(), m_moveSpeed * Time.deltaTime);
            Debug.Log("slerp: {0}".FormatWith(lerpVector));
            transform.eulerAngles = lerpVector;
        }

        #endregion private methods
    }
}