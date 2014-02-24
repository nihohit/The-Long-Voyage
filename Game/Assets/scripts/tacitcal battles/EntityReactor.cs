using UnityEngine;

#region EntityReactor

public class EntityReactor : CircularButton
{
    private Entity m_ent;
    public Entity Entity
    {
        get { return m_ent; }
        set
        {
            m_ent = value;
            this.Action = () => 
                TacticalState.SelectedHex = m_ent.Hex.Reactor;
            this.OnMouseExitProperty = () => { };
            this.OnMouseOverProperty = OnOver;
        }
    }

    private void OnOver()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetMouseButton(1))
            {

            }
        }
    }
}

#endregion
