using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship
{
    public class RoomRoleWorker_PlaceOfWorship : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            int num = 0;
            List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
            for (int i = 0; i < containedAndAdjacentThings.Count; i++)
            {
                BuildingAltar altar = containedAndAdjacentThings[i] as BuildingAltar;
                if (altar != null || (containedAndAdjacentThings[i].TryGetComp<CompShrine>() != null && !(containedAndAdjacentThings[i] is Building_Bed)))
                {
                    num++;
                }
            }
            if (num <= 1)
            {
                return 0f;
            }
            return (float)num * 100100f;
        }
    }
}
