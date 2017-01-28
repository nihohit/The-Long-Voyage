using System;
using System.Collections.Generic;

namespace Assets.Scripts.StrategicGameScene.MapGeneration
{
	public interface IWorldGenerator
	{
		IEnumerable<LocationInformation> GenerateStrategicMap();
	}
}

