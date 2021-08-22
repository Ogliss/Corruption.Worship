using Corruption.Core.Soul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship
{
    public class RitualCategoryDef : Def
    {
    }

    public class RitualDef : Def
    {
        public string FullDescription;

        public Type ritualClass;

        public RitualCategoryDef Category;

        public RitualWorker Worker;

        public List<SkillRequirement> skillRequirements = new List<SkillRequirement>();

        public SimpleCurve successCurve = new SimpleCurve(new List<CurvePoint>() { new CurvePoint(0f, 1f) });

        public List<ThingCountClass> things = new List<ThingCountClass>();
        public PawnGroupMaker pawnGroupMaker;
        public PawnGroupMakerParms pawnGroupMakerParms;
        public ThingDef effectMote;

        public float points;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                    this.Worker = (RitualWorker)Activator.CreateInstance(this.ritualClass, this);                
            });
        }
    }

    public abstract class RitualWorker
    {
        public RitualWorker(RitualDef def)
        {
            this.Def = def;
        }

        public RitualDef Def;

        public abstract void FinishRitual(Pawn ritualLeader, LocalTargetInfo localTargetInfo, List<Pawn> participants);
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

    public class RitualWorker_Interrogation : RitualWorker
    {
        public RitualWorker_Interrogation(RitualDef def) : base(def)
        {
        }

        public override void FinishRitual(Pawn ritualLeader, LocalTargetInfo localTargetInfo, List<Pawn> participants)
        {
            Pawn pawn = localTargetInfo.Pawn;
            if (pawn != null)
            {
                var soul = pawn.Soul();
                if (soul != null && !soul.KnownToPlayer)
                {
                    soul.KnownToPlayer = true;
                }
            }
        }
    }

    public class RitualWorker_Summoning : RitualWorker
    {
        public RitualWorker_Summoning(RitualDef def) : base(def)
        {
        }

        public override void FinishRitual(Pawn ritualLeader, LocalTargetInfo localTargetInfo, List<Pawn> participants)
        {
            var pawns = this.Def.pawnGroupMaker.GeneratePawns(this.Def.pawnGroupMakerParms);
            if (pawns.Any())
            {
                MoteMaker.MakeStaticMote(localTargetInfo.Cell, ritualLeader.Map, this.Def.effectMote);

                foreach (var pawn in pawns)
                {
                    GenSpawn.Spawn(pawn, localTargetInfo.Cell, ritualLeader.Map, WipeMode.VanishOrMoveAside);
                }
            }
        }
    }
}
