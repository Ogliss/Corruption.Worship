using Corruption.Core.Gods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Corruption.Worship
{
    public class SermonTemplate : Sermon
    {
        public string Name = "";

        public Pawn Preacher;

        public Pawn Assistant;

        public Pawn Target;

        private static IntRange AvailableRange = new IntRange(0, 23);

        public bool Active;

        private int preferredStartTime;

        public int PreferredStartTime => preferredStartTime;

        public float Length;

        public GodDef DedicatedTo;

        public bool HeldToday;

        public float SermonDurationHours = 0.5f;

        public SermonTemplate() : base(WorshipRitualDefOf.Corruption_Sermon)
        {
        }

        public void SetStartTime(int value)
        {
            this.preferredStartTime = Mathf.Clamp(value, AvailableRange.min, AvailableRange.max);
        }

        public SermonTemplate(string name, Pawn preacher, bool active, int startTime, float length, RitualDef def) : base(def)
        {
            this.Name = name;
            this.Preacher = preacher;
            Active = active;
            preferredStartTime = startTime;
            Length = length;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref preferredStartTime, "preferredStartTime");
            Scribe_Values.Look<float>(ref Length, "Length");
            Scribe_Values.Look<bool>(ref Active, "Active");
            Scribe_Defs.Look<GodDef>(ref this.DedicatedTo, "DedicatedTo");
            Scribe_References.Look<Pawn>(ref this.Preacher, "Preacher");
            Scribe_References.Look<Pawn>(ref this.Assistant, "Assistant");
            Scribe_Values.Look<string>(ref this.Name, "Name");
            Scribe_Values.Look<bool>(ref this.HeldToday, "HeldToday");
            Scribe_Values.Look<float>(ref this.SermonDurationHours, "SermonDurationHours");
        }
    }
}
