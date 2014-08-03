using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.scripts.UnityBase
{
    public interface IUnityMarker
    {
        void Mark(Vector3 position);

        void Mark();

        void Unmark();

        void DestroyGameObject();

        Vector3 Position { get; set; }

        Vector3 Scale { get; set; }

        SpriteRenderer Renderer { get; }
    }

    public interface IUnityButton : IUnityMarker
    {
        Action ClickableAction { get; set; }
        Action OnMouseOverAction { get; set; }
        Action OnMouseExitAction { get; set; }
    }
}
