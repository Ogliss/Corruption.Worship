using Corruption.Core.Soul;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.Worship
{
	// Corruption.Worship.RitualTargetFilter_Altar
	public class RitualTargetFilter_Altar : RitualTargetFilter
	{
		public RitualTargetFilter_Altar()
		{
		}

		public RitualTargetFilter_Altar(RitualTargetFilterDef def) : base(def)
		{
		}

		public override bool CanStart(TargetInfo initiator, TargetInfo selectedTarget, out string rejectionReason)
		{
			CacheAltars();
			TargetInfo targetInfo = this.BestTarget(initiator, selectedTarget);
			rejectionReason = "";
			if (!targetInfo.IsValid)
			{
				rejectionReason = "AbilityDisabledNoAltarOrRitualsSpot".Translate();
				return false;
			}
			return true;
		}

		private List<BuildingAltar> Altars = new List<BuildingAltar>();
		private void CacheAltars()
		{
			this.Altars.Clear();
			this.Altars = Find.CurrentMap.listerBuildings.allBuildingsColonist.Where(x => x is BuildingAltar).Cast<BuildingAltar>().ToList();
		}

		public override TargetInfo BestTarget(TargetInfo initiator, TargetInfo selectedTarget)
		{
			Pawn pawn = initiator.Thing as Pawn;
			if (pawn == null)
			{
				return null;
			}
			Thing thing = null;
			float num = 99999f;
			foreach (Building building in Altars)
			{
				if (building is BuildingAltar altar && altar.SermonActive && altar.CurrentActiveSermon.Preacher == pawn)
				{
					if (building.def.isAltar && pawn.CanReach(building, PathEndMode.Touch, pawn.NormalMaxDanger(), false, false, TraverseMode.ByPawn))
					{
						int lengthHorizontalSquared = (pawn.Position - building.Position).LengthHorizontalSquared;
						if ((float)lengthHorizontalSquared < num)
						{
							thing = building;
							num = (float)lengthHorizontalSquared;
						}
					}
				}
			}
			if (thing == null && this.def.fallbackToRitualSpot)
			{
				foreach (Thing thing2 in pawn.Map.listerThings.ThingsOfDef(ThingDefOf.RitualSpot))
				{
					if (pawn.CanReach(thing2, PathEndMode.Touch, pawn.NormalMaxDanger(), false, false, TraverseMode.ByPawn))
					{
						int lengthHorizontalSquared2 = (pawn.Position - thing2.Position).LengthHorizontalSquared;
						if ((float)lengthHorizontalSquared2 < num)
						{
							thing = thing2;
							num = (float)lengthHorizontalSquared2;
						}
					}
				}
			}
			return thing;
		}

		public override IEnumerable<string> GetTargetInfos(TargetInfo initiator)
		{
			yield return "RitualTargetGatherAltarOrRitualSpotInfo".Translate();
			yield break;
		}
	}

	public class RitualObligationTargetWorker_Altar : RitualObligationTargetFilter
	{
		public RitualObligationTargetWorker_Altar()
		{
		}

		public RitualObligationTargetWorker_Altar(RitualObligationTargetFilterDef def) : base(def)
		{
		}

		public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
		{
			if (!ModLister.CheckIdeology("Altar target"))
			{
				yield break;
			}
			Ideo ideo = this.parent.ideo;
			foreach (TargetInfo targetInfo in RitualObligationTargetWorker_Altar.GetTargetsWorker(obligation, map, ideo))
			{
				yield return targetInfo;
			}
			IEnumerator<TargetInfo> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06005E1D RID: 24093 RVA: 0x00208F54 File Offset: 0x00207154
		public static IEnumerable<TargetInfo> GetTargetsWorker(RitualObligation obligation, Map map, Ideo ideo)
		{
			int num;
			for (int i = 0; i < ideo.PreceptsListForReading.Count; i = num + 1)
			{
				Precept_Building precept_Building = ideo.PreceptsListForReading[i] as Precept_Building;
				if (precept_Building != null && precept_Building.ThingDef.isAltar)
				{
					foreach (Thing t in precept_Building.presenceDemand.AllBuildings(map))
					{
						yield return t;
					}
					IEnumerator<Thing> enumerator = null;
				}
				num = i;
			}
			yield break;
			yield break;
		}

		public override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
		{
			return RitualObligationTargetWorker_Altar.CanUseTargetWorker(target, obligation, this.parent.ideo);
		}

		public static bool CanUseTargetWorker(TargetInfo target, RitualObligation obligation, Ideo ideo)
		{
			BuildingAltar building = target.Thing as BuildingAltar;
			return building != null && building.Faction != null && building.Faction.IsPlayer && RitualObligationTargetWorker_Altar.GetTargetsWorker(obligation, building.Map, ideo).Contains(building);
		}

		public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
		{
			foreach (string text in RitualObligationTargetWorker_Altar.GetTargetInfosWorker(this.parent.ideo))
			{
				yield return text;
			}
			IEnumerator<string> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06005E21 RID: 24097 RVA: 0x00208FE5 File Offset: 0x002071E5
		public static IEnumerable<string> GetTargetInfosWorker(Ideo ideo)
		{
			int num;
			for (int i = 0; i < ideo.PreceptsListForReading.Count; i = num + 1)
			{
				Precept_Building precept_Building = ideo.PreceptsListForReading[i] as Precept_Building;
				if (precept_Building != null && precept_Building.ThingDef.isAltar)
				{
					yield return precept_Building.LabelCap;
				}
				num = i;
			}
			yield break;
		}

		// Token: 0x06005E22 RID: 24098 RVA: 0x00208FF5 File Offset: 0x002071F5
		public override List<string> MissingTargetBuilding(Ideo ideo)
		{
			if (!this.GetTargetInfos(null).Any<string>())
			{
				return new List<string>
				{
					"Altar".Translate()
				};
			}
			return null;
		}
	}

	// Corruption.Worship.RitualBehaviorWorker_Sermon
	public class RitualBehaviorWorker_Sermon : RitualBehaviorWorker
    {
        public RitualBehaviorWorker_Sermon()
        {
        }

        public RitualBehaviorWorker_Sermon(RitualBehaviorDef def) : base(def)
        {
        }

        public override LordJob CreateLordJob(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
        {
            return new LordJob_Joinable_Speech(target, organizer, ritual, this.def.stages, assignments, true);
        }
	}

	// Corruption.Worship.RitualStage_AtTheAltar
	public class RitualStage_AtTheAltar : RitualStage
	{
		public override TargetInfo GetSecondFocus(LordJob_Ritual ritual)
		{
			return ritual.selectedTarget.Cell.GetFirstThing<Thing>(ritual.Map);
		}
	}
	// Corruption.Worship.RitualOutcomeEffectWorker_Sermon
	public class RitualOutcomeEffectWorker_Sermon : RitualOutcomeEffectWorker_FromQuality
	{
		public override bool SupportsAttachableOutcomeEffect
		{
			get
			{
				return false;
			}
		}

		public RitualOutcomeEffectWorker_Sermon()
		{
		}

		public RitualOutcomeEffectWorker_Sermon(RitualOutcomeEffectDef def) : base(def)
		{
		}

		public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
		{
			BuildingAltar altar = jobRitual.selectedTarget.Thing as BuildingAltar;
			Pawn organizer = jobRitual.Organizer;
			FinishRitual(organizer, jobRitual.selectedTarget, totalPresence.Keys.ToList());
			float quality = base.GetQuality(jobRitual, progress);
			OutcomeChance outcome = this.GetOutcome(quality, jobRitual);
			ThoughtDef memory = outcome.memory;
			LookTargets lookTargets = organizer;
			string text = null;
			if (jobRitual.Ritual != null)
			{
				this.ApplyAttachableOutcome(totalPresence, jobRitual, outcome, out text, ref lookTargets);
			}
			string text2 = "";
			string text3 = "";
			foreach (KeyValuePair<Pawn, int> keyValuePair in totalPresence)
			{
				Pawn key = keyValuePair.Key;
				if (key != organizer && organizer.Position.InHorDistOf(key.Position, 18f))
				{
					Thought_Memory thought_Memory = base.MakeMemory(key, jobRitual, memory);
					thought_Memory.otherPawn = organizer;
					thought_Memory.moodPowerFactor = ((key.Ideo == organizer.Ideo) ? 1f : 0.5f);
					key.needs.mood.thoughts.memories.TryGainMemory(thought_Memory, null);
					if (memory == ThoughtDefOf.InspirationalSpeech)
					{
						if (Rand.Chance(RitualOutcomeEffectWorker_Speech.InspirationChanceFromInspirationalSpeech))
						{
							InspirationDef randomAvailableInspirationDef = key.mindState.inspirationHandler.GetRandomAvailableInspirationDef();
							if (randomAvailableInspirationDef != null && key.mindState.inspirationHandler.TryStartInspiration(randomAvailableInspirationDef, "LetterSpeechInspiration".Translate(key.Named("PAWN"), organizer.Named("SPEAKER")), true))
							{
								text2 = text2 + "  - " + key.NameShortColored.Resolve() + "\n";
							}
						}
						if (ModsConfig.IdeologyActive && key.Ideo != organizer.Ideo && Rand.Chance(RitualOutcomeEffectWorker_Speech.ConversionChanceFromInspirationalSpeech))
						{
							key.ideo.SetIdeo(organizer.Ideo);
							text3 = text3 + "  - " + key.NameShortColored.Resolve() + "\n";
						}
					}
				}
			}
			TaggedString taggedString = "LetterFinishedSpeech".Translate(organizer.Named("ORGANIZER")).CapitalizeFirst() + " " + ("Letter" + memory.defName).Translate() + "\n\n" + this.OutcomeQualityBreakdownDesc(quality, progress, jobRitual);
			if (!text3.NullOrEmpty())
			{
				taggedString += "\n\n" + "LetterSpeechConvertedListeners".Translate(organizer.Named("PAWN"), organizer.Ideo.Named("IDEO")).CapitalizeFirst() + ":\n\n" + text3.TrimEndNewlines();
			}
			if (!text2.NullOrEmpty())
			{
				taggedString += "\n\n" + "LetterSpeechInspiredListeners".Translate() + "\n\n" + text2.TrimEndNewlines();
			}
			if (progress < 1f)
			{
				taggedString += "\n\n" + "LetterSpeechInterrupted".Translate(progress.ToStringPercent(), organizer.Named("ORGANIZER"));
			}
			if (text != null)
			{
				taggedString += "\n\n" + text;
			}
			string text4;
			this.ApplyDevelopmentPoints(jobRitual.Ritual, outcome, out text4);
			if (text4 != null)
			{
				taggedString += "\n\n" + text4;
			}
			Find.LetterStack.ReceiveLetter("OutcomeLetterLabel".Translate(outcome.label.Named("OUTCOMELABEL"), jobRitual.Ritual.Label.Named("RITUALLABEL")), taggedString, RitualOutcomeEffectWorker_Speech.PositiveOutcome(memory) ? LetterDefOf.RitualOutcomePositive : LetterDefOf.RitualOutcomeNegative, lookTargets, null, null, null, null);
			Ability ability = organizer.abilities.GetAbility(AbilityDefOf.Speech, true);
			RoyalTitle mostSeniorTitle = organizer.royalty.MostSeniorTitle;
			if (ability != null && mostSeniorTitle != null)
			{
				ability.StartCooldown(mostSeniorTitle.def.speechCooldown.RandomInRange);
			}
		}

		public void FinishRitual(Pawn ritualLeader, TargetInfo localTargetInfo, List<Pawn> participants)
		{
			BuildingAltar altar = localTargetInfo.Thing as BuildingAltar;
			if (altar == null)
			{
				Log.Warning($"Tried to finish Sermon Ritual from non-altar target {localTargetInfo.ToString()}");
				return;
			}
			var soul = ritualLeader.Soul();
			if (soul == null)
			{
				return;
			}

			float num = 0f;

			num += participants.Count * 20f;

			num *= altar.CurrentActiveSermon.DedicatedTo.favourCorruptionFactor;
			soul.GainCorruption(num);

			altar.records.AddTo(WorshipRecordDefOf.SermonsHeldAltar, 1);
			altar.records.AddTo(WorshipRecordDefOf.SermonAttendees, participants.Count);

			altar.EndSermon();
		}
		private static bool PositiveOutcome(ThoughtDef outcome)
		{
			return outcome == ThoughtDefOf.EncouragingSpeech || outcome == ThoughtDefOf.InspirationalSpeech;
		}

		private static readonly float InspirationChanceFromInspirationalSpeech = 0.05f;

		private static readonly float ConversionChanceFromInspirationalSpeech = 0.02f;
	}

	public class RitualWorker_Sermon : RitualWorker
    {
        public RitualWorker_Sermon(RitualDef def) : base(def)
        {

        }

        public override void FinishRitual(Pawn ritualLeader, LocalTargetInfo localTargetInfo, List<Pawn> participants)
        {
            BuildingAltar altar = localTargetInfo.Thing as BuildingAltar;
            if (altar == null)
            {
                Log.Warning($"Tried to finish Sermon Ritual from non-altar target {localTargetInfo.ToString()}");
                return;
            }
            var soul = ritualLeader.Soul();
            if (soul == null)
            {
                return;
            }

            float num = 0f;

            num += participants.Count * 20f;

            num *= altar.CurrentActiveSermon.DedicatedTo.favourCorruptionFactor;
            soul.GainCorruption(num);

            altar.records.AddTo(WorshipRecordDefOf.SermonsHeldAltar, 1);
            altar.records.AddTo(WorshipRecordDefOf.SermonAttendees, participants.Count);

            altar.EndSermon();
        }
    }
}
