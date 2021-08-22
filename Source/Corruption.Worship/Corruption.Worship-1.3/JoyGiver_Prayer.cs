using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Corruption.Worship
{
    public class JoyGiver_Prayer : JoyGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            IntVec3 result = pawn.Position;
            if (pawn.ownership == null)
            {
                return null;
            }
            Room ownedRoom = pawn.ownership.OwnedRoom;
            LocalTargetInfo shrineTarget = LocalTargetInfo.Invalid;
            Thing ownedShrine = ownedRoom?.ContainedAndAdjacentThings.FirstOrDefault(x => x.TryGetComp<CompShrine>() != null);

            Predicate<Thing> shrineCheck = delegate (Thing t)
            {
                return t.TryGetComp<CompShrine>() != null;
            };
            Thing nearbyShrine = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors), 10f, shrineCheck);

            Thing preferredShrine = ownedShrine != null ? ownedShrine : nearbyShrine;
            if (preferredShrine != null)
            {
                if (preferredShrine is BuildingAltar altar)
                {
                    Building chair;
                    if (!WatchBuildingUtility.TryFindBestWatchCell(altar, pawn, true, out result, out chair))
                    {
                        WatchBuildingUtility.TryFindBestWatchCell(altar as Thing, pawn, false, out result, out chair);
                    }                    
                }
                else
                {
                    result = preferredShrine.InteractionCell;
                }
                shrineTarget = preferredShrine;
            }
            else if (ownedRoom != null && ownedRoom.Cells.Where((IntVec3 c) => c.Standable(pawn.Map) && !c.IsForbidden(pawn) && pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.None)).TryRandomElement(out IntVec3 cell))
            {
                result = cell;
            }
            if (shrineTarget == null || shrineTarget == LocalTargetInfo.Invalid || (!pawn.CanReserveAndReach(shrineTarget, PathEndMode.ClosestTouch, Danger.None, 1)))
            {
                return JobMaker.MakeJob(def.jobDef, result);
            }
            return JobMaker.MakeJob(def.jobDef, result, shrineTarget);
        }

        public override Job TryGiveJobWhileInBed(Pawn pawn)
        {
            return JobMaker.MakeJob(def.jobDef, pawn.CurrentBed());
        }
    }
}
