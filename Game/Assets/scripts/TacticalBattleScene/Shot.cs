using Assets.Scripts.UnityBase;
using System;
using UnityEngine;

namespace Assets.Scripts.TacticalBattleScene
{
    /// <summary>
    ///  Script wrapper for shots moving across the screen
    /// </summary>
    public class Shot : MonoBehaviour
    {
        private bool m_started;
        private Vector2 m_movementFraction;
        private Vector3 m_endPoint;
        private Action m_callback;

        private const double s_minDistance = 0.2;

        public void Init(Vector2 to, Vector2 from, string shotName, Action callback)
        {
            m_callback = callback;
            transform.position = from;
            m_endPoint = to;
            m_started = true;
            var differenceVector = to - from;
            gameObject.transform.Rotate(differenceVector.ToRotationVector());

            //TODO - movement speed is not a constant for different shots. Use code from here - http://www.attiliocarotenuto.com/83-articles-tutorials/unity/292-unity-3-moving-a-npc-along-a-path
            m_movementFraction = differenceVector / 30;
            TacticalState.TextureManager.UpdateEffectTexture(shotName, this.GetComponent<SpriteRenderer>());
        }

        // Use this for initialization
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            if (!m_started) return;
            transform.position = (Vector2)m_movementFraction + (Vector2)transform.position;
            if (m_endPoint.Distance(transform.position) < s_minDistance)
            {
                Destroy(gameObject);
                m_callback();
            }
        }
    }
}