﻿using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Corruption.Worship
{
    public class JobDriver_Spectate : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null, errorOnFailed);
		}

		public override IEnumerable<Toil> MakeNewToils()
		{
			if (this.job.GetTarget(TargetIndex.A).HasThing)
			{
				this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
			}
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			yield return new Toil
			{
				tickAction = delegate ()
				{
					this.pawn.rotationTracker.FaceCell(this.job.GetTarget(TargetIndex.B).Cell);
					this.pawn.GainComfortFromCellIfPossible(false);
					if (this.pawn.IsHashIntervalTick(100))
					{
						this.pawn.jobs.CheckForJobOverride();
					}
				},
				defaultCompleteMode = ToilCompleteMode.Never,
				handlingFacing = true
			};
			yield break;
		}

		public const TargetIndex MySpotOrChairInd = TargetIndex.A;

		public const TargetIndex WatchTargetInd = TargetIndex.B;
	}
}
