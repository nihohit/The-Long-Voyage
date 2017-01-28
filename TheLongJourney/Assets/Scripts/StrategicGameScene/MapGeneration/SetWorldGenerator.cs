using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.InterSceneCommunication;

namespace Assets.Scripts.StrategicGameScene.MapGeneration
{
	public class SetWorldGenerator : IWorldGenerator {
		private int index;

		public IEnumerable<LocationInformation> GenerateStrategicMap() {
			List<string> vignettes = new List<string>{"EmptyEncounter", "Bandits"};
			List<LocationInformation> world = vignettes.Select(name => Information(name)).ToList();
			for (int i = 0; i < world.Count; i++) {
				if (i > 0) {
					world [i].ConnectedLocations.Add (world [i - 1]);
					world [i-1].ConnectedLocations.Add (world [i]);
				}
			}
			
			return world;
		}

		LocationInformation Information(string vignetteName) {
			VignetteTemplate vignette = GlobalState.Instance.Configurations.Vignettes.GetConfiguration(vignetteName);
			return new LocationInformation (new Vector2 (0, index++), vignette, new List<LocationInformation> ());
		}
	}
}
