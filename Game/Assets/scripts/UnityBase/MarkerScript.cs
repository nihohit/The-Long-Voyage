﻿using UnityEngine;

namespace Assets.scripts.UnityBase
{
    public class MarkerScript : MonoBehaviour
    {
        public SpriteRenderer internalRenderer;

        public virtual void Mark(Vector3 position)
        {
            this.enabled = true;
            internalRenderer.enabled = true;
            internalRenderer.transform.position = position;
        }

        public virtual void Mark()
        {
            Mark(this.transform.position);
        }

        public virtual void Unmark()
        {
            internalRenderer.enabled = false;
            this.enabled = false;
        }
    }
}