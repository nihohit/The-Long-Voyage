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
        }
    }
}

#endregion EntityReactor