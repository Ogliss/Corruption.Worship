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
	public class JobGiver_Duel : ThinkNode_JobGiver
	{
		public override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.RaceProps.Humanlike && pawn.WorkTagIsDisabled(WorkTags.Violent))
			{
				return null;
			}
			Pawn otherPawn = ((MentalState_SocialFighting)pawn.MentalState).otherPawn;
			var verb = pawn.meleeVerbs.TryGetMeleeVerb(otherPawn);
			if (verb == null)
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, otherPawn);
			job.verbToUse = verb;
			return job;
		}
	}
}
