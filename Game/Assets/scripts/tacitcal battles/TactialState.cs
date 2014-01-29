using UnityEngine;
using System.Collections;

public class TacticalState : Singleton<TacticalState>
{
	private HexReactor m_selectedHex;

	private static TacticalState s_instance;
	
	public HexReactor SelectedHex
	{ 
		get
		{
			return m_selectedHex;
		}
		set
		{
			if(value == null && m_selectedHex != null)
			{
				m_selectedHex.Unselect();
			}

			m_selectedHex = value;
			if(m_selectedHex !=null)
				m_selectedHex.Select();
		}
	}
	
	private TacticalState() { }
}


