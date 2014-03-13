using System.Collections.Generic;

public enum MapTypes { Swamp, Hills, Forest, Desert, RiverSide }
public enum DispersalMethod { Randomly, GatheringArea }

public class MapGenerationInstructions
{
	public int AmountOfHexes { get; set; }
	
	public MapTypes Type { get; set; }

    public IEnumerable<TeamDefinition> Teams { get; set; }
}

public class TeamDefinition
{
    //TODO - once we'll have some kind of base entity generated from XML files, this shouldn't be the final entity type, but the base type and a factory will convert them to the final entity type
    public IEnumerable<Entity> Units { get; set; }

    public DispersalMethod Dispersal { get; set; } 

    public Loyalty Loyalty { get; set; }
}


