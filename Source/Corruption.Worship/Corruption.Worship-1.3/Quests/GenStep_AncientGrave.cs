using Corruption.Worship.Sketches;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using RimWorld.SketchGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Corruption.Worship.Quests
{
    public class GenStep_AncientGrave : GenStep_Scatterer
    {
        private static readonly SimpleCurve RuinSizeChanceCurve = new SimpleCurve
    {
        new CurvePoint(6f, 0f),
        new CurvePoint(6.001f, 10f),
        new CurvePoint(10f, 7f),
        new CurvePoint(30f, 0f)
    };

        private int randomSize;

        public override int SeedPart => 1348417666;

        public override bool TryFindScatterCell(Map map, out IntVec3 result)
        {
            randomSize = Mathf.RoundToInt(Rand.ByCurve(RuinSizeChanceCurve));
            return base.TryFindScatterCell(map, out result);
        }

        public override bool CanScatterAt(IntVec3 c, Map map)
        {
            if (!base.CanScatterAt(c, map))
            {
                return false;
            }
            if (!c.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy))
            {
                return false;
            }
            CellRect rect = new CellRect(c.x, c.z, randomSize, randomSize).ClipInsideMap(map);
            if (!CanPlaceAncientBuildingInRange(rect, map))
            {
                return false;
            }
            return true;
        }

        protected bool CanPlaceAncientBuildingInRange(CellRect rect, Map map)
        {
            foreach (IntVec3 cell in rect.Cells)
            {
                if (cell.InBounds(map))
                {
                    TerrainDef terrainDef = map.terrainGrid.TerrainAt(cell);
                    if (terrainDef.HasTag("River") || terrainDef.HasTag("Road"))
                    {
                        return false;
                    }
                    if (!GenConstruct.CanBuildOnTerrain(ThingDefOf.Wall, cell, map, Rot4.North))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            CellRect rect = new CellRect(c.x, c.z, randomSize, randomSize).ClipInsideMap(map);
            if (CanPlaceAncientBuildingInRange(rect, map))
            {
                RimWorld.SketchGen.ResolveParams parms2 = default(RimWorld.SketchGen.ResolveParams);
                parms2.sketch = new Sketch();
                parms2.monumentSize = new IntVec2(rect.Width, rect.Height);
                var sketch = SketchGen.Generate(SketchResolverDefOf.MonumentRuin, parms2);
                if (parms.sitePart != null && parms.sitePart.things != null && parms.sitePart.things.Any)
                {
                    foreach (var item in parms.sitePart.things)
                    {
                        this.AddRelic(sketch, item, parms2);
                    }
                }

                var spawnedThings = new List<Thing>();

                sketch.Spawn(map, rect.CenterCell, null, Sketch.SpawnPosType.Unchanged, Sketch.SpawnMode.Normal, wipeIfCollides: false, clearEdificeWhereFloor: false, spawnedThings, dormant: false, buildRoofsInstantly: false, delegate (SketchEntity entity, IntVec3 cell)
                {
                    bool result = false;
                    foreach (IntVec3 adjacentCell in entity.OccupiedRect.AdjacentCells)
                    {
                        IntVec3 c2 = cell + adjacentCell;
                        if (c2.InBounds(map))
                        {
                            Building edifice = c2.GetEdifice(map);
                            if (edifice == null || !edifice.def.building.isNaturalRock)
                            {
                                return true;
                            }
                        }
                    }
                    return result;
                });


                bool itemReplaced = false;
                foreach (var expectedSpawn in parms.sitePart.things)
                {
                    var thingToReplace = spawnedThings.FirstOrDefault(x => x.def == expectedSpawn.def);
                    if (thingToReplace != null)
                    {
                        var pos = thingToReplace.Position;
                        var rot = thingToReplace.Rotation;
                        thingToReplace.Destroy();
                        expectedSpawn.Rotation = rot;
                        GenSpawn.Spawn(expectedSpawn, pos, map);
                        if (expectedSpawn is Building_Grave casket)
                        {
                            casket.GetStoreSettings().filter.SetAllowAll(casket.def.building.defaultStorageSettings.filter);
                            var corpse = CorpseGenerator.GenerateDessicatedCorpse(Corruption.Core.FactionsDefOf.IoM_NPC.basicMemberKind, new IntRange(GenDate.TicksPerYear * 100, GenDate.TicksPerYear * 90));
                            corpse.InnerPawn.story.traits.GainTrait(new Trait(WorshipTraitDefOfs.HumanSaint));
                        }
                        itemReplaced = true;
                    }
                }

                if (!itemReplaced)
                {
                    QuestUtility.SendQuestTargetSignals(map.Parent.questTags, "NothingFound", map.Parent.Named("SUBJECT"));
                }

            }
        }


        private void AddRelic(Sketch sketch, Thing item, RimWorld.SketchGen.ResolveParams parms)
        {
            if (item != null)
            {
                RimWorld.SketchGen.ResolveParams parms3 = parms;
                parms3.sketch = sketch;
                parms3.wallEdgeThing = item.def;
                parms3.requireFloor = true;
                parms3.thingCentral = item.def;
                WorshipSketchDefOf.AddThingsCentralFixed.Resolve(parms3);
                SketchResolverDefOf.AddWallEdgeThings.Resolve(parms3);
            }
        }
    }
}
