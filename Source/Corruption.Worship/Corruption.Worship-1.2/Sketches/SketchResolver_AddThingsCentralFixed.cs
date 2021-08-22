using RimWorld;
using RimWorld.SketchGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship.Output.Sketches
{
    public class SketchResolver_AddThingsCentralFixed : SketchResolver
	{
		private HashSet<IntVec3> processed = new HashSet<IntVec3>();

		public override void ResolveInt(ResolveParams parms)
		{
			CellRect outerRect = parms.rect ?? parms.sketch.OccupiedRect;
			bool allowWood = parms.allowWood ?? true;
			ThingDef stuff = GenStuff.RandomStuffInexpensiveFor(parms.thingCentral, null, (ThingDef x) => SketchGenUtility.IsStuffAllowed(x, allowWood, parms.useOnlyStonesAvailableOnMap, allowFlammableWalls: true, parms.thingCentral));
			bool requireFloor = parms.requireFloor ?? false;
			processed.Clear();
			try
			{
				foreach (IntVec3 item in outerRect.Cells.InRandomOrder())
				{
					CellRect cellRect = SketchGenUtility.FindBiggestRectAt(item, outerRect, parms.sketch, processed, (IntVec3 x) => !parms.sketch.ThingsAt(x).Any() && (!requireFloor || (parms.sketch.TerrainAt(x) != null && parms.sketch.TerrainAt(x).layerable)));
					if (cellRect.Width >= parms.thingCentral.size.x + 2 && cellRect.Height >= parms.thingCentral.size.z + 2)
					{
						parms.sketch.AddThing(parms.thingCentral, new IntVec3(cellRect.CenterCell.x - parms.thingCentral.size.x / 2, 0, cellRect.CenterCell.z - parms.thingCentral.size.z / 2), Rot4.North, stuff, 1, null, null, wipeIfCollides: false);
					}
				}
			}
			finally
			{
				processed.Clear();
			}
		}

		public override bool CanResolveInt(ResolveParams parms)
		{
			return true;
		}
	}
}
