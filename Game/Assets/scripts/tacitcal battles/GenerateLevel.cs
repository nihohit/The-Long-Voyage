using UnityEngine;
using System.Collections;

public class GenerateLevel : MonoBehaviour {

    public GameObject greenHex;
	public Camera mainCamera;

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

		var target = 2*GlobalState.Instance.AmountOfHexes - 1;

		//the math became a bit complicated when trying to account for correct coordinates. 
		for (int i = - GlobalState.Instance.AmountOfHexes + 1 ; i <= 0; i++)
        {
			entryPoint = new Vector3(entryPoint.x - ( hexSize.x/2), entryPoint.y + (hexSize.x*Mathf.Sqrt(3)/2) , entryPoint.z);
			var amountOfHexesInRow = i + target;
			var entryCoordinate = (float)-i  / 2 - GlobalState.Instance.AmountOfHexes + 1;
			for(float j = 0 ; j < amountOfHexesInRow  ; j++)
			{
				CreateHex(
					new Vector3(entryPoint.x + j*hexSize.x, entryPoint.y, entryPoint.z), 
					new Vector2(entryCoordinate + j, i));
			}
        }
		
		mainCamera.transform.position = new Vector3(entryPoint.x  + ((GlobalState.Instance.AmountOfHexes - 1) * hexSize.x), entryPoint.y, entryPoint.z - 70);
		mainCamera.transform.Rotate(new Vector3(180,180,180));

		for (int i = 1 ; i < target - GlobalState.Instance.AmountOfHexes + 1; i++)
        {
			entryPoint = new Vector3(entryPoint.x + ( hexSize.x/2), entryPoint.y + (hexSize.x*Mathf.Sqrt(3)/2) , entryPoint.z);
			var amountOfHexesInRow = target - i;
			var entryCoordinate = (float)i  / 2 - GlobalState.Instance.AmountOfHexes + 1;
			for(float j = 0; j < amountOfHexesInRow ; j++)
			{
				CreateHex(
					new Vector3(entryPoint.x + j*hexSize.x, entryPoint.y, entryPoint.z), 
					new Vector2(entryCoordinate + j, i));
			}
        }
	}

	#region private methods

	void CreateHex(Vector3 nextPosition, Vector2 hexCoordinates)
	{
		var hex = (GameObject)Instantiate(greenHex, nextPosition, Quaternion.identity);
		//hex.transform.Rotate(new Vector3(270,0,0));
		var reactor = hex.GetComponent<HexReactor>();
		reactor.MarkedHex = new Hex(hexCoordinates, reactor);
	}
	
	void CreateHex(Vector2 hexCoordinates)
	{
		CreateHex(Vector3.zero, hexCoordinates);
	}

	
	float GetDistance(Vector3 origin, Vector3 target)
	{
		return Mathf.Sqrt(Mathf.Pow(origin.x - target.x, 2f)	+ Mathf.Pow(origin.y - target.y, 2f) + Mathf.Pow(origin.z - target.z, 2f));
	}

	#endregion
}
