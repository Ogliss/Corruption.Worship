using Corruption.Core.Gods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_SpawnThing : WonderWorker_Targetable
    {
        protected virtual List<IntVec2> Pattern => new List<IntVec2>() { IntVec2.Zero };

        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters()
            {
                canTargetLocations = true,
                canTargetPawns = true,
                validator = ((TargetInfo x) => x.Cell.Standable(x.Map))
            };
        }

        protected override bool TryDoEffectOnTarget(GodDef god, int worshipPoints)
        {
            bool spawnedOne = false;
            for (int i = 0; i < this.Def.thingsToSpawn.Count; i++)
            {
                ThingDefCountClass entry = this.Def.thingsToSpawn[i];
                int countToCreate = entry.count / entry.thingDef.stackLimit;
                if (this.Def.spawnPattern.NullOrEmpty())
                {


                    for (int j = 0; j < countToCreate; j++)
                    {
                        Thing thing = ThingMaker.MakeThing(entry.thingDef);
                        thing.stackCount = Math.Min(thing.def.stackLimit, countToCreate);
                        if (this.Def.spawnForPlayerFaction && entry.thingDef.CanHaveFaction)
                        {
                            thing.SetFactionDirect(Faction.OfPlayer);
                        }
                        spawnedOne = spawnedOne || GenPlace.TryPlaceThing(thing, this.target.Cell, this.target.Map, ThingPlaceMode.Near);
                    }
                }
                else
                {
                    foreach (var position in this.Def.spawnPattern)
                    {
                        Thing thing = ThingMaker.MakeThing(entry.thingDef);
                        if (this.Def.spawnForPlayerFaction && entry.thingDef.CanHaveFaction)
                        {
                            thing.SetFactionDirect(Faction.OfPlayer);
                        }
                        thing.stackCount = Math.Min(thing.def.stackLimit, countToCreate);
                        spawnedOne = spawnedOne || GenPlace.TryPlaceThing(thing, this.target.Cell + new IntVec3(position.x, 0, position.z), this.target.Map, ThingPlaceMode.Near);

                    }
                }
            }

            return spawnedOne;
        }
    }

    public class WonderWorker_OrbitalStrike : WonderWorker_SpawnThing
    {
        protected override bool TryDoEffectOnTarget(GodDef god, int worshipPoints)
        {
            var toSpawn = this.Def.thingsToSpawn[0].thingDef;
            if (!typeof(OrbitalStrike).IsAssignableFrom(toSpawn.thingClass))
            {
                return false;
            }
            OrbitalStrike obj = (OrbitalStrike)GenSpawn.Spawn(this.Def.thingsToSpawn[0].thingDef, this.target.Cell, this.target.Map);
            obj.duration = this.Def.effectDurationRange.RandomInRange;
            obj.instigator = null;
            obj.weaponDef = null;
            obj.StartStrike();
            return true;
        }

    }
}
