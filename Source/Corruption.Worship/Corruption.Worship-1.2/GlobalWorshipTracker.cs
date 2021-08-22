using Corruption.Core;
using Corruption.Core.Gods;
using Corruption.Core.Soul;
using Corruption.Worship.Wonders;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship
{
    public class GlobalWorshipTracker : WorldComponent
    {
        public GlobalWorshipTracker(World world) : base(world)
        {
            this.PlayerPantheon = PantheonDefOf.ImperialCult;
        }

        public DefMap<WonderDef, int> GlobalWonderCooldownTicks = new DefMap<WonderDef, int>();

        private PantheonDef playerPantheon;

        public PantheonDef PlayerPantheon
        {
            get { return playerPantheon; }
            set
            {
                playerPantheon = value;
                foreach (var member in value.members)
                {
                    if (!this.Favours.Any(x => x.God == member.god))
                    {
                        this.Favours.Add(new FavourProgress(member.god, 0f));
                    }
                }
            }
        }

        public List<FavourProgress> Favours = new List<FavourProgress>();

        public List<FavourProgress> PantheonFavours
        {
            get
            {
                return this.Favours.FindAll(x => this.PlayerPantheon.IsMember(x.God));
            }
        }

        public float PantheonFavourPercentage
        {
            get
            {
                var pantheonMembers = PlayerPantheon.members;
                float totalWeight = pantheonMembers.Sum(x => x.pantheonWeight);
                float curProgress = 0.001f;
                foreach (var member in pantheonMembers)
                {
                    var progress = GetFavourProgressFor(member.god);
                    curProgress += (member.pantheonWeight / totalWeight) * progress.FavourPercentage;
                }
                return curProgress;
            }
        }

        public FavourProgress GetFavourProgressFor(GodDef god)
        {
            foreach (var progress in this.Favours)
            {
                if (progress.God.Equals(god))
                {
                    return progress;
                }
            }
            return null;
        }

        public bool TryAddFavor(GodDef god, float value, Pawn pawn = null)
        {
            value *= ModSettings_Corruption.WorshipGainSpeedFactor;
            var favor = this.Favours.FirstOrDefault(x => x.God == god);
            if (favor == null)
            {
                favor = new FavourProgress(god, value);
                this.Favours.Add(favor);
            }
            if (favor.TryAddProgress(value))
            {
                pawn?.records.AddTo(WorshipRecordDefOf.GlobalFavourContributed, value);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            int curTick = Find.TickManager.TicksGame;
            if (curTick % GenDate.TicksPerDay == 0)
            {
                foreach (var attribute in playerPantheon.pantheonAttributes.Where(x => x.effectWorker != null))
                {
                    if (Find.TickManager.TicksGame % attribute.effectTick == 0)
                    {
                        attribute.effectWorker.TickDay();
                    }
                }
            }
            else if (curTick % 2500 == 0)
            {
                foreach (var attribute in playerPantheon.pantheonAttributes.Where(x => x.effectWorker != null))
                {
                    if (Find.TickManager.TicksGame % attribute.effectTick == 0)
                    {
                        attribute.effectWorker.TickLong();
                    }
                }

                foreach (var favour in this.Favours)
                {
                    favour.Deteriorate();
                }
            }
            foreach (var kvp in this.GlobalWonderCooldownTicks)
            {
                if (kvp.Value > 0)
                {
                    this.GlobalWonderCooldownTicks[kvp.Key]--;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<PantheonDef>(ref this.playerPantheon, "playerPantheon");
            Scribe_Collections.Look<FavourProgress>(ref this.Favours, "favors", LookMode.Deep);
            Scribe_Deep.Look(ref this.GlobalWonderCooldownTicks, "triggeredWonders");
            if(Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if(this.GlobalWonderCooldownTicks == null)
                {
                    this.GlobalWonderCooldownTicks = new DefMap<WonderDef, int>();
                }
            }
        }

        public static GlobalWorshipTracker Current
        {
            get
            {
                return Find.World.GetComponent<GlobalWorshipTracker>();
            }
        }

        internal void ConsumeFavourFor(int value, GodDef god)
        {
            this.GetFavourProgressFor(god).Favour -= value;
        }
    }
}
