//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
namespace AssemblyCSharp
{
	public enum MapTypes { Swamp, Hills, Forest, Desert, RiverSide }

	public class MapGenerationInstructions
	{
		public int AmountOfHexes { get; set; }
		
		public MapTypes Type { get; set; }

		public MapGenerationInstructions ()
		{
		}
	}

	public class Map
	{
		public Hex[][] Hexes {get; set;}
	}

}

