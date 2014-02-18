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
                var rayHit = Physics2D.Raycast(this.transform.position, Input.mousePosition - this.transform.position, LayerMask.NameToLayer("Entities"));
                if (rayHit.collider == null)
                {
                    Debug.Log("Ray from {0} missed".FormatWith(transform.position));
                    //TacticalState.SelectedHex = null;
                }
                else
                {
                    Debug.Log("Ray from {0} hit {1}".FormatWith(transform.position, rayHit.collider.transform.position));
                    //TacticalState.SelectedHex = rayHit.collider.gameObject.GetComponent<EntityReactor>().Entity.Hex.Reactor;
                }
            }
        }
        else
        {

            if (Input.GetMouseButton(1))
            {
                TacticalState.SelectedHex = null;
            }
        }
    }
}

#endregion
