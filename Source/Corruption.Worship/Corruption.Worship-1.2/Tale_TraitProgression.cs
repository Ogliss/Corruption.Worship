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
    public class Tale_TraitProgression : Tale
    {
        public Tale InnerTale;

        public SoulTraitDef TraitDef;

        public int ObtainedAtDegree;

        public Pawn Pawn;

        public Tale_TraitProgression(SoulTraitDef soulTraitDef, int degree, Tale innerTale)
        {
            this.InnerTale = innerTale;
            this.TraitDef = soulTraitDef;
            this.ObtainedAtDegree = degree;
            var reference = new TaleReference(InnerTale);
            // not sure what you were trying to do here, so commented it out now just to compile
        //    this.Text = innerTale.def.LabelCap + GenDate.DateFullStringAt(innerTale.date; //reference.GenerateText(TextGenerationPurpose.ArtName, RulePackDefOf.NamerQuestDefault);
        }

        public override string ShortSummary => InnerTale.ShortSummary;

        public override bool Concerns(Thing th)
        {
            return InnerTale.Concerns(th);
        }

        public override void GenerateTestData()
        {
            InnerTale.GenerateTestData();
        }

        public override void Notify_FactionRemoved(Faction faction)
        {
            InnerTale.Notify_FactionRemoved(faction);
        }

        public string Text;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Tale>(ref this.InnerTale, "innerTale");
            Scribe_Defs.Look<SoulTraitDef>(ref this.TraitDef, "traitDef");
            Scribe_Values.Look<int>(ref this.ObtainedAtDegree, "obtainedAtDegree");
            Scribe_Values.Look<string>(ref this.Text, "text");
            Scribe_References.Look<Pawn>(ref this.Pawn, "pawn");
        }
    }
}
