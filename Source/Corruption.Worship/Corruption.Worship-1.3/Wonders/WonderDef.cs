using Corruption.Core.Gods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderDef : Def
    {
        public Type workerClass;

        public List<GodDef> associatedGods = new List<GodDef>();

        public List<ThingDefCountClass> thingsToSpawn = new List<ThingDefCountClass>();

        public List<IntVec2> spawnPattern = new List<IntVec2>();

        public List<HediffDef> hediffsToAdd = new List<HediffDef>();

        private int cooldownTicks;

        public int CooldownTicks => cooldownTicks;

        public float minHediffSeverity = 0.1f;

        public TraitDef traitToGive;

        public TaleDef taleToCreate;

        public List<AbilityDef> unlockedPowers = new List<AbilityDef>();

        public IncidentDef incident;

        public MentalStateDef mentalStateToStart;

        public int fixedIncidentPoints;

        public string wonderIconPath;

        public Texture2D wonderIcon;

        public bool pointsScalable;

        public SimpleCurve pointsCurve;

        public int favourCost;

        public int yieldFavour = 0;

        public List<QuestScriptDef> questScriptDefs = new List<QuestScriptDef>();
        
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                    this.wonderIcon = ContentFinder<Texture2D>.Get(this.wonderIconPath, true);
                    this.workerInt = (WonderWorker)Activator.CreateInstance(this.workerClass);
                    this.workerInt.Def = this;
            });
        }

        private WonderWorker workerInt;
        public bool spawnForPlayerFaction;
        internal IntRange effectDurationRange = IntRange.one;

        public WonderWorker Worker
        {
            get
            {
                return this.workerInt;
            }
        }

        public int ResolveWonderPoints(IIncidentTarget target, int worshipPoints)
        {
            if (this.pointsScalable && this.pointsCurve != null)
            {

                int result = (int)this.pointsCurve.Evaluate(worshipPoints);
                return result;
            }
            else if (this.pointsScalable && this.fixedIncidentPoints < 1f)
            {
                return (int)(StorytellerUtility.DefaultThreatPointsNow(target));
            }
            else
            {
                return this.fixedIncidentPoints;
            }
        }

        public string ToolTip
        {
            get
            {
                var builder = new StringBuilder();
                builder.AppendLine(this.LabelCap);
                builder.AppendLine();
                builder.AppendLine(this.description);
                builder.AppendLine();
                builder.AppendLine("FavourCost".Translate(new NamedArgument(this.favourCost, "COST")));
                return builder.ToString();
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (this.mentalStateToStart == null && this.workerClass == typeof(WonderWorker_StartMentalState))
            {
                yield return this.defName + " has type " + typeof(WonderWorker_StartMentalState).Name + " but mentalStateToStart is null";
            }
            if (this.traitToGive == null && this.workerClass == typeof(WonderWorker_AddSpecialTrait))
            {
                yield return this.defName + " has type " + typeof(WonderWorker_AddSpecialTrait).Name + " but traitToGive is null";
            }
            if (this.hediffsToAdd.NullOrEmpty() && this.workerClass == typeof(WonderWorker_AddHediff))
            {
                yield return this.defName + " has type " + typeof(WonderWorker_AddHediff).Name + " but HediffsToAdd are null or empty";
            }
        }
    }
}
