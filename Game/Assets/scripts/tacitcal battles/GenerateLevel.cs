using UnityEngine;
using System.Collections;

public class GenerateLevel : MonoBehaviour 
{
    #region public members

    public GameObject greenHex;
    public GameObject woodHex;
	public Camera mainCamera;

    #endregion

    #region MonoBehaviour overrides

	void Update()
	{
        lock (TacticalState.Lock)
        {
            if (Camera.current != null)
            {
                float xAxisValue = Input.GetAxis("Horizontal");
                float yAxisValue = Input.GetAxis("Vertical");
                float zAxisValue = Input.GetAxisRaw("Zoom");
                Camera.current.transform.Translate(new Vector3(xAxisValue, yAxisValue, zAxisValue));
            }
            if (Input.GetMouseButton(1))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (TacticalState.SelectedHex != null && TacticalState.SelectedHex.MarkedHex.Content == null)
                    {
                        //TODO - this is just a temporary measure, to create mechs
                        var mech = new Mech(new Subsystem[] {new Laser(Loyalty.EnemyArmy), new MissileLauncher(Loyalty.EnemyArmy)},
                        ((GameObject)Instantiate(Resources.Load("Mech"), transform.position, Quaternion.identity)).GetComponent<EntityReactor>());
                        TacticalState.SelectedHex.MarkedHex.Content = mech;
                        TacticalState.AddEntity(mech);
                        Debug.Log("created {0} at {1}".FormatWith(mech, TacticalState.SelectedHex));
                    }
                    TacticalState.SelectedHex = null;
                }
                else
                {
                    if (TacticalState.SelectedHex != null && TacticalState.SelectedHex.MarkedHex.Content != null)
                    {
                        Debug.Log(TacticalState.SelectedHex.MarkedHex.Content);
                    }
                    TacticalState.SelectedHex = null;
                }
            }
        }
	}

	// Use this for initialization
	void Start () 
    {
        TacticalState.Init(new[]{Loyalty.Player});

		if(GlobalState.AmountOfHexes < 1)
		{
            InitiateGlobalState();
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
                CreateRandomHex(
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
                CreateRandomHex(
					new Vector3(entryPoint.x + j*hexSize.x, entryPoint.y, entryPoint.z), 
					new Vector2(entryCoordinate + j, i));
			}
        }
	}

    #endregion

	#region private methods

	private void CreateGrassHex(Vector3 nextPosition, Vector2 hexCoordinates)
	{
		var hex = (GameObject)Instantiate(greenHex, nextPosition, Quaternion.identity);
		//hex.transform.Rotate(new Vector3(270,0,0));
		var reactor = hex.GetComponent<HexReactor>();
		reactor.MarkedHex = new Hex(hexCoordinates, reactor);
	}

    private void CreateLightTreesHex(Vector3 nextPosition, Vector2 hexCoordinates)
    {
        CreateWoodHex(nextPosition, hexCoordinates).MarkedHex.Content = new SparseTrees(((GameObject)Instantiate(Resources.Load("SparseTrees"), transform.position, Quaternion.identity)).GetComponent<EntityReactor>());
    }

    private void CreateDenseTreesHex(Vector3 nextPosition, Vector2 hexCoordinates)
    {
        CreateWoodHex(nextPosition, hexCoordinates).MarkedHex.Content = new DenseTrees(((GameObject)Instantiate(Resources.Load("DenseTrees"), transform.position, Quaternion.identity)).GetComponent<EntityReactor>());
    }

    private void CreateBuildingHex(Vector3 nextPosition, Vector2 hexCoordinates)
    {
        CreateWoodHex(nextPosition, hexCoordinates).MarkedHex.Content = new Building(((GameObject)Instantiate(Resources.Load("Building"), transform.position, Quaternion.identity)).GetComponent<EntityReactor>());
    }

    private HexReactor CreateWoodHex(Vector3 nextPosition, Vector2 hexCoordinates)
    {
        var hex = (GameObject)Instantiate(woodHex, nextPosition, Quaternion.identity);
        //hex.transform.Rotate(new Vector3(270,0,0));
        var reactor = hex.GetComponent<HexReactor>();
        reactor.MarkedHex = new Hex(hexCoordinates, reactor);
        reactor.MarkedHex.Conditions = TraversalConditions.Broken;
        return reactor;
    }

	
    private void CreateRandomHex(Vector3 nextPosition, Vector2 hexCoordinates)
	{
        var random = Randomiser.Next(1,8);
        switch(random)
        {
            case(1):
                CreateLightTreesHex(nextPosition, hexCoordinates);
                break;
            case(2):
                CreateDenseTreesHex(nextPosition, hexCoordinates);
                break;
            case(3):
                CreateBuildingHex(nextPosition, hexCoordinates);
                break;
            default:
                CreateGrassHex(nextPosition, hexCoordinates);
                break;

        }
	}

    private void CreateRandomHex(Vector2 hexCoordinates)
    {
        CreateRandomHex(Vector3.zero, hexCoordinates);
    }

	
	private float GetDistance(Vector3 origin, Vector3 target)
	{
		return Mathf.Sqrt(Mathf.Pow(origin.x - target.x, 2f)	+ Mathf.Pow(origin.y - target.y, 2f) + Mathf.Pow(origin.z - target.z, 2f));
	}

    private void InitiateGlobalState()
    {
        //TODO - replace with exception throwing when we remove the direct access to level generation
        FileHandler.Init();
        HexReactor.Init();
        GlobalState.AmountOfHexes = FileHandler.GetIntProperty(
            "default map size", 
            FileAccessor.TerrainGeneration);
    }

	#endregion
}

