using RimWorld;
using System;
using System.Collections.Generic;
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
}
