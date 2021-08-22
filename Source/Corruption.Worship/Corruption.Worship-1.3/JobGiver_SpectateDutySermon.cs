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
	public class JobGiver_SpectateDutySermon : ThinkNode_JobGiver
	{
		public override Job TryGiveJob(Pawn pawn)
		{
			PawnDuty duty = pawn.mindState.duty;
			if (duty == null)
			{
				return null;
			}
			if ((duty.spectateRectPreferredSide == SpectateRectSide.None || !SpectatorCellFinder.TryFindSpectatorCellFor(pawn, duty.spectateRect, pawn.Map, out IntVec3 cell, duty.spectateRectPreferredSide)) && !SpectatorCellFinder.TryFindSpectatorCellFor(pawn, duty.spectateRect, pawn.Map, out cell, duty.spectateRectAllowedSides))
			{
				return null;
			}
			IntVec3 centerCell = duty.spectateRect.CenterCell;
			BuildingAltar altar = centerCell.GetFirstThing<BuildingAltar>(pawn.Map);

			if(altar == null || altar.CurrentActiveSermon == null)
			{
				return null;
			}

			Building edifice = cell.GetEdifice(pawn.Map);
			if (edifice != null && edifice.def.category == ThingCategory.Building && edifice.def.building.isSittable && pawn.CanReserve(edifice))
			{
				return JobMaker.MakeJob(WorshipJobDefOf.AttendSermon, edifice, centerCell);
			}
			return JobMaker.MakeJob(WorshipJobDefOf.AttendSermon, cell, centerCell);
		}
	}
}
