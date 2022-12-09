using Corruption.Core;
using Corruption.Core.Gods;
using Corruption.Core.Items;
using Corruption.Core.Soul;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Corruption.Worship
{
    [StaticConstructorOnStartup]
    public class PilgrimageComp : WorldObjectComp
    {
        public static readonly Texture2D HomeIcon = ContentFinder<Texture2D>.Get("UI/Buttons/AutoHomeArea");

        public override void Initialize(WorldObjectCompProperties props)
        {
            base.Initialize(props);
        }

        public Caravan caravan
        {
            get
            {
                return this.parent as Caravan;
            }
        }

        public static int PILGRIMAGE_TRAVELRADIUS = 20;

        public int OriginalMapTile;

        private int currentDestinationTile;

        private int travelledTicks;

        private int ticksToTravel;

        private bool returningHome;

        private bool pilgrimageResolved;

        private float accumulatedSuccessChange => this.travelledTicks / this.ticksToTravel;

        private GodDef pilgrimGod;
        private PantheonDef pilgrimPathenon;

        private bool pilgrimageActive;

        private void CheckPilgrimageActive()
        {
            IEnumerable<Pawn> colonists = (caravan.pawns.InnerListForReading.Where(x => x.Soul() != null));
            foreach (Pawn p in colonists)
            {
                CompSoul soul = p.Soul();
                if (soul != null && soul.IsOnPilgrimage)
                {
                    this.pilgrimageActive = true;
                    this.ticksToTravel = Rand.Range(GenDate.TicksPerDay * 2, GenDate.TicksPerDay * 5);
                    this.caravan.SetFaction(CorruptionStoryTrackerUtilities.IoM_NPC);
                    this.pilgrimPathenon = soul.ChosenPantheon;
                    this.pilgrimGod = soul.FavourTracker.HighestFavour.God;
                    break;
                }
            }
            this.pilgrimageResolved = true;
            if (pilgrimageActive)
            {
                foreach (var pawn in this.caravan.PawnsListForReading)
                {
                    pawn.SetFactionDirect(CorruptionStoryTrackerUtilities.IoM_NPC);
                }
            }

        }

        public override void CompTick()
        {
            this.travelledTicks++;
            if (this.caravan.IsHashIntervalTick(GenDate.TicksPerDay))
            {
                UpdatePilgrimageInventory();
            }


            if (!this.pilgrimageResolved)
            {
                CheckPilgrimageActive();
            }
            base.CompTick();
            if (!this.returningHome)
            {
                if (this.parent.Tile == this.caravan.pather.Destination)
                {
                    this.PilgrimageDecisionPoint();
                }
            }
        }

        private void PilgrimageDecisionPoint()
        {
            if (this.travelledTicks >= ticksToTravel)
            {
                this.ReturnHome();
                return;
            }
            else
            {
                this.SetRandomDestination();
            }

        }

        private void UpdatePilgrimageInventory()
        {
            string text;
            if (this.caravan.needs.AnyPawnOutOfFood(out text))
            {
                ThingDef eatableDef = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsNutritionGivingIngestible).RandomElement();
                Thing food = ThingMaker.MakeThing(eatableDef);
                food.stackCount = eatableDef.stackLimit;
                this.caravan?.AddPawnOrItem(food, false);
                Messages.Message("PilgrimsForagedFood".Translate(), this.caravan, MessageTypeDefOf.PositiveEvent);

            }

            if (Rand.Value < accumulatedSuccessChange)
            {
                if (Rand.Value > 0.2f)
                {
                    ThingDef relicDef = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.HasComp(typeof(CompEmpyreal))).RandomElementByWeight(y => y.GetCompProperties<CompProperties_Empyreal>().DedicatedGod == this.pilgrimGod ? 1 : 0);
                    Thing newRelic = ThingMaker.MakeThing(relicDef);
                    this.caravan?.AddPawnOrItem(newRelic, false);
                }
                else
                {
                    Faction RandomFaction = Find.World.factionManager.AllFactions.Where(x => !x.HostileTo(Faction.OfPlayer) && x.def.basicMemberKind?.race == Faction.OfPlayer.def.basicMemberKind.race).RandomElement();
                    Pawn newFollower = PawnGenerator.GeneratePawn(RandomFaction.RandomPawnKind());
                    CompSoul newSoul = newFollower.Soul();
                    if (newFollower.skills.GetSkill(SkillDefOf.Social).Level < this.caravan?.pawns.InnerListForReading.Max(x => x.skills.GetSkill(SkillDefOf.Social).Level) + Rand.Range(-2, 2))
                    {
                        newSoul.ChosenPantheon = this.pilgrimPathenon;
                    }

                    this.caravan?.AddPawn(newFollower, true);
                }
            }
        }

        private void SetRandomDestination()
        {
            int tile;
            TileFinder.TryFindPassableTileWithTraversalDistance(this.parent.Tile, 5, 800, out tile, (int x) => Find.WorldGrid[x].biome.canAutoChoose, true, true);
            this.currentDestinationTile = tile;
            this.caravan?.pather.StartPath(tile, null, true);
        }

        public void ForceReturnHome()
        {
            this.ReturnHome();
        }

        private void ReturnHome()
        {
            this.caravan?.pather.StartPath(this.OriginalMapTile, new CaravanArrivalAction_Enter(Find.World.worldObjects.MapParentAt(this.OriginalMapTile)));
            this.returningHome = true;
            this.caravan?.SetFaction(Faction.OfPlayer);
            foreach (var pawn in caravan.PawnsListForReading)
            {
                pawn.SetFactionDirect(Faction.OfPlayer);
            }
            List<Pawn> Pawns = this.caravan?.PawnsListForReading.FindAll(x => x.RaceProps.Humanlike);
            for (int i = 0; i < Pawns.Count; i++)
            {
                CompSoul soul = Pawns[i].Soul();
                if (soul != null)
                {
                    soul.IsOnPilgrimage = false;
                    var god = soul.ChosenPantheon.GodsListForReading.RandomElementByWeight(x => soul.FavourTracker.FavourValueFor(x) + 1f);
                    soul.GainCorruption(god.favourCorruptionFactor * ticksToTravel / 1000f, god);
                }
            }
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.icon = HomeIcon;
            command_Action.defaultLabel = "Debug:ReturnHome".Translate();
            command_Action.defaultDesc = "Debug:ReturnHome".Translate();
            command_Action.action = delegate
            {
                this.ReturnHome();
            };
            yield return command_Action;

        }

        public override void PostExposeData()
        {
            Scribe_Values.Look<int>(ref this.ticksToTravel, "ticksToTravel", 2500);
            Scribe_Values.Look<int>(ref this.travelledTicks, "travelledTicks", 0);
            Scribe_Values.Look<int>(ref this.OriginalMapTile, "ticksToTravel", 2500);
            Scribe_Values.Look<int>(ref this.currentDestinationTile, "currentDestinationTile", 2500);
            Scribe_Values.Look<bool>(ref this.returningHome, "returningHome", false);
            Scribe_Values.Look<bool>(ref this.pilgrimageResolved, "pilgrimageResolved", false);
            Scribe_Defs.Look<GodDef>(ref this.pilgrimGod, "pilgrimGod");

        }
    }
}
