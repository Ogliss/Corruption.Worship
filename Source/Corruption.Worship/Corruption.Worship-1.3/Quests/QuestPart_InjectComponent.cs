using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship.Quests
{
    internal class QuestPart_InjectComponent : QuestPart
    {
        private ThingWithComps target;
        private Type compType;

        public QuestPart_InjectComponent()
        {
        }

        public void SetComponent(ThingWithComps target, Type compType)
        {
            this.target = target;
            this.compType = compType;
            Inject();
        }

        private void Inject()
        {
            var comp = Activator.CreateInstance(compType) as ThingComp;
            if (this.target != null && !target.AllComps.Any(x => x.GetType() == compType))
            {
                comp.parent = this.target;
                this.target.AllComps.Add(comp);
            }
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.target, "target");
            Scribe_Values.Look(ref compType, "compType");
            if(Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.SetComponent(this.target, this.compType);
            }
        }
    }
}
