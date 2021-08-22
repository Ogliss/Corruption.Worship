using Corruption.Core.Gods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship
{
    public class BuildingAltar : Building
    {
        private SermonTemplate _sermon;

        public SermonTemplate CurrentActiveSermon
        {
            get { return _sermon; }
            set
            {
                _sermon = value;
            }
        }

        public CompShrine CompShrine => this.TryGetComp<CompShrine>();

        public CompAffectedByFacilities CompAffectedByFacilities => this.TryGetComp<CompAffectedByFacilities>();

        public List<SermonTemplate> Templates = new List<SermonTemplate>();

        public bool SermonActive => this.CurrentActiveSermon != null;

        public string RoomName;

        public Pawn Deacon = null;

        public Altar_RecordsTracker records = new Altar_RecordsTracker();

        public override void PostMake()
        {
            base.PostMake();
            this.Templates.AddRange(SermonUtility.StandardTemplates().ToList());
            foreach (var template in this.Templates)
            {
                template.DedicatedTo = GlobalWorshipTracker.Current.PlayerPantheon.GodsListForReading.RandomElement();
            }
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                this.Deacon = Map.mapPawns.FreeColonistsSpawned.RandomElement<Pawn>();
            }
            RoomName = "Temple";
            TickManager f = Find.TickManager;

            f.RegisterAllTickabilityFor(this);
            LessonAutoActivator.TeachOpportunity(WorshipConceptDefOf.AltarKnowledge, OpportunityType.GoodToKnow);

        }

        public override void Tick()
        {
            base.Tick();
            if (!this.Spawned)
            {
                return;
            }

            if (this.IsHashIntervalTick(600))
            {
                float curHour = GenLocalDate.HourFloat(this.Map);
                if ((int)curHour == 0)
                {
                    foreach (var template in this.Templates)
                    {
                        template.HeldToday = false;
                    }
                }
                if (this.CurrentActiveSermon == null)
                {
                    foreach (var template in this.Templates)
                    {
                        if (template.Active && !template.HeldToday && (int)curHour >= template.PreferredStartTime)
                        {
                            if (this.TryStartSermon(template))
                            {
                                break;
                            }
                            else
                            {
                                this.CurrentActiveSermon = null;
                            }
                        }
                    }
                }
                else
                {
                    if (this.CurrentActiveSermon.HeldToday && this.CurrentActiveSermon.PreferredStartTime > curHour + 3f)
                    {
                        this.EndSermon();
                    }
                }
            }
        }

        public void EndSermon()
        {
            if (this.CurrentActiveSermon != null)
            {
                this.CurrentActiveSermon.HeldToday = true;
            }
            this.CurrentActiveSermon = null;
        }

        public bool TryStartSermon(SermonTemplate template)
        {
            this.CurrentActiveSermon = template;
            if (SermonUtility.ForceSermon(this, template))
            {
                return true;
            }
            this.CurrentActiveSermon = null;
            return false;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            
            return base.GetGizmos();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.Look<Pawn>(ref this.Deacon, "preacher", false);
            Scribe_Deep.Look<Altar_RecordsTracker>(ref this.records, "records");
            Scribe_Values.Look<string>(ref this.RoomName, "RoomName", "Temple", false);
            Scribe_Deep.Look<SermonTemplate>(ref this._sermon, "CurrentActiveSermon");
            Scribe_Collections.Look<SermonTemplate>(ref this.Templates, "Templates", LookMode.Deep);
        }
    }
}
