using UnityEngine;

namespace Assets.Scripts.UnityBase
{
    /// <summary>
    /// A script meant to handle movement and display of sprites.
    /// </summary>
    public class MarkerScript : MonoBehaviour, IUnityMarker
    {
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
    }
}