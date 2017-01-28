using UnityEngine;

namespace Assets.Scripts.UnityBase
{
	public abstract class SceneBehaviour : MonoBehaviour
	{
		public void OnApplicationQuit()
		{
			foreach(var result in Timer.Instance.Results())
			{
				Debug.Log(result);
			}
		}
	}
}
