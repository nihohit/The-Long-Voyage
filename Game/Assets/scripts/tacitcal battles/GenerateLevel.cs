using UnityEngine;
using System.Collections;

public class GenerateLevel : MonoBehaviour {

    public GameObject greenHex;
	public Camera mainCamera;

	void Update()
	{
        lock (TacticalState.Lock)
        {
            float xAxisValue = Input.GetAxis("Horizontal");
            float yAxisValue = Input.GetAxis("Vertical");
            float zAxisValue = Input.GetAxisRaw("Zoom");

            if (Camera.current != null)
            {
                Camera.current.transform.Translate(new Vector3(xAxisValue, yAxisValue, zAxisValue));
            }
            if (Input.GetMouseButton(1))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (TacticalState.SelectedHex != null && TacticalState.SelectedHex.MarkedHex.Content != null)
                    {
                        TacticalState.SelectedHex.MarkedHex.Content.Marker.collider2D.enabled = false;
                        var mouse = Input.mousePosition;
                        var mouseRay = Camera.main.ScreenPointToRay(mouse);
                        RaycastHit mouseHit;
                        Physics.Raycast(mouseRay, out mouseHit);
                        var mousePosition = mouseHit.transform.position;
                        mousePosition.z = 0;
                        var startingPoint = TacticalState.SelectedHex.transform.position;
                        Debug.Log("Shooting ray from {0} to {1}".FormatWith(startingPoint, mousePosition));
                        var layer = LayerMask.NameToLayer("Entities");
                        var layerMask = 1 << layer;
                        var rayHit = Physics2D.Raycast(startingPoint, mousePosition - startingPoint, 1000, layerMask);
                        if (rayHit.collider == null)
                        {
                            Debug.Log("Ray from {0} missed".FormatWith(startingPoint));
                            //TacticalState.SelectedHex = null;
                        }
                        else
                        {
                            Debug.Log("Ray from {0} hit {1}".FormatWith(startingPoint, rayHit.collider.transform.position));
                            Debug.Log("collider was active: {0}".FormatWith(rayHit.collider.enabled));
                            Debug.Log("object is: {0}".FormatWith(rayHit.collider.gameObject.ToString()));
                            //TacticalState.SelectedHex = rayHit.collider.gameObject.GetComponent<EntityReactor>().Entity.Hex.Reactor;
                        }
                        TacticalState.SelectedHex.MarkedHex.Content.Marker.collider2D.enabled = true;
                    }
                }
                else
                {
                    if (TacticalState.SelectedHex != null && TacticalState.SelectedHex.MarkedHex.Content == null)
                    {
                        //TODO - this is just a temporary measure, to create mechs
                        var mech = new Mech(new Subsystem[] {new Laser(Loyalty.EnemyArmy), new MissileLauncher(Loyalty.EnemyArmy)},
                                            ((GameObject)Instantiate(Resources.Load("Mech"), transform.position, Quaternion.identity)).GetComponent<EntityReactor>());
                        mech.Marker.internalRenderer = mech.Marker.GetComponent<SpriteRenderer>();
                        TacticalState.SelectedHex.MarkedHex.Content = mech;
                    }
                    TacticalState.SelectedHex = null;
                }
            }
        }
	}

	// Use this for initialization
	void Start () 
    {
		//TODO - remove when there's no direct access to level generation
		FileHandler.Init();
		HexReactor.Init();

        TacticalState.Init(new[]{Loyalty.Player});

		if(GlobalState.AmountOfHexes < 1)
		{
			GlobalState.AmountOfHexes = FileHandler.GetIntProperty(
				"default map size", 
				FileAccessor.TerrainGeneration);
		}
		var entryPoint = Vector3.zero;
		var hexSize = greenHex.renderer.bounds.size;

		var target = 2*GlobalState.AmountOfHexes - 1;

		//the math became a bit complicated when trying to account for correct coordinates. 
		for (int i = - GlobalState.AmountOfHexes + 1 ; i <= 0; i++)
        {
			entryPoint = new Vector3(entryPoint.x - ( hexSize.x/2), entryPoint.y + (hexSize.x*Mathf.Sqrt(3)/2) , entryPoint.z);
			var amountOfHexesInRow = i + target;
			var entryCoordinate = (float)-i  / 2 - GlobalState.AmountOfHexes + 1;
			for(float j = 0 ; j < amountOfHexesInRow  ; j++)
			{
				CreateHex(
					new Vector3(entryPoint.x + j*hexSize.x, entryPoint.y, entryPoint.z), 
					new Vector2(entryCoordinate + j, i));
			}
        }
		
		mainCamera.transform.position = new Vector3(entryPoint.x  + ((GlobalState.AmountOfHexes - 1) * hexSize.x), entryPoint.y, entryPoint.z - 70);
		mainCamera.transform.Rotate(new Vector3(180,180,180));

		for (int i = 1 ; i < target - GlobalState.AmountOfHexes + 1; i++)
        {
			entryPoint = new Vector3(entryPoint.x + ( hexSize.x/2), entryPoint.y + (hexSize.x*Mathf.Sqrt(3)/2) , entryPoint.z);
			var amountOfHexesInRow = target - i;
			var entryCoordinate = (float)i  / 2 - GlobalState.AmountOfHexes + 1;
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

