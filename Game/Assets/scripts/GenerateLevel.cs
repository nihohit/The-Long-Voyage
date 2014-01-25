using UnityEngine;
using System.Collections;

public class GenerateLevel : MonoBehaviour {

    public GameObject greenHex;
	public Camera mainCamera;
	public GUITexture texture;

	void Update()
	{
		float xAxisValue = Input.GetAxis("Horizontal");
		float yAxisValue = Input.GetAxis("Vertical");
		float zAxisValue =  Input.GetAxisRaw("Zoom");

		if(Camera.current != null)
		{
			Camera.current.transform.Translate(new Vector3(xAxisValue, yAxisValue, zAxisValue));
		}
		if (Input.GetMouseButton(1)) 
		{
			TacticalState.Instance.SelectedHex = null;
		}
	}

	// Use this for initialization
	void Start () 
    {
		//TODO - remove when there's no direct access to level generation
		FileHandler.Init();
		HexReactor.Init();

		if(GlobalState.Instance.AmountOfHexes < 1)
		{
			GlobalState.Instance.AmountOfHexes = FileHandler.GetIntProperty(
				"default map size", 
				FileAccessor.TerrainGeneration);
		}
		var entryPoint = Vector3.zero;
		var hexSize = greenHex.renderer.bounds.size;
		
        for (int i = 0; i < GlobalState.Instance.AmountOfHexes; i++)
        {
			entryPoint = new Vector3(entryPoint.x - ( hexSize.x/2), entryPoint.y + (hexSize.x*Mathf.Sqrt(3)/2) , entryPoint.z);
			var amountOfHexesInRow = i + GlobalState.Instance.AmountOfHexes;
			for(int j = 0; j < amountOfHexesInRow ; j++)
			{
				CreateHex(new Vector3(entryPoint.x + j*hexSize.x, entryPoint.y, entryPoint.z));
			}
        }
		
		mainCamera.transform.position = new Vector3(entryPoint.x  + ((GlobalState.Instance.AmountOfHexes - 1) * hexSize.x), entryPoint.y, entryPoint.z - 40);
		mainCamera.transform.Rotate(new Vector3(180,180,180));
		
		for (int i = GlobalState.Instance.AmountOfHexes - 2 ; i >= 0; i--)
        {
			entryPoint = new Vector3(entryPoint.x + ( hexSize.x/2), entryPoint.y + (hexSize.x*Mathf.Sqrt(3)/2) , entryPoint.z);
			var amountOfHexesInRow = i + GlobalState.Instance.AmountOfHexes;
			for(int j = 0; j < amountOfHexesInRow ; j++)
			{
				CreateHex(new Vector3(entryPoint.x + j*hexSize.x, entryPoint.y, entryPoint.z));
			}
        }
	}

	#region private methods

	GameObject CreateHex(Vector3 nextPosition)
	{
		var hex = (GameObject)Instantiate(greenHex, nextPosition, Quaternion.identity);
		//hex.transform.Rotate(new Vector3(270,0,0));
		return hex;
	}
	
	GameObject CreateHex()
	{
		return CreateHex(Vector3.zero);
	}
	
	float GetFlatAngle(Vector3 origin, Vector3 target)
	{
		var adjacentLength = GetDistance(origin, Vector3.zero);
		var hypothenuseLength = GetDistance(origin, target);
		return Mathf.Cos(adjacentLength / hypothenuseLength);
	}
	
	float GetDistance(Vector3 origin, Vector3 target)
	{
		return Mathf.Sqrt(Mathf.Pow(origin.x - target.x, 2f)	+ Mathf.Pow(origin.y - target.y, 2f) + Mathf.Pow(origin.z - target.z, 2f));
	}

	#endregion
}
