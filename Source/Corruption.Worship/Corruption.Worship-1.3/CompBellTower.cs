using Corruption.Worship;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace Corruption.Worship
{
    public class CompBellTower : ThingComp
    {
        public CompProperties_BellTower BellProps => this.props as CompProperties_BellTower;

        private Sustainer bellSustainer;

        public Graphic BellGraphic;

        private int bellAngle = 0;
        private bool clockWise = true;
        private bool isRinging = false;
        private int ringoutTicks = 0;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                this.BellGraphic = GraphicDatabase.Get<Graphic_Single>(this.BellProps.graphicData.texPath, ShaderDatabase.Cutout, this.BellProps.graphicData.drawSize, Color.white);
            });
        }

        private static Vector3 RotateBell(Vector3 point, Vector3 pivot, float angle)
        {
            var normalized = point - pivot;
            normalized = Quaternion.Euler(0f, angle, 0f) * normalized;
            return normalized + pivot;
        }

        internal void StartRinging()
        {
            this.ringoutTicks = this.BellProps.ringoutTicks;
            this.isRinging = true;
        }

        public override void CompTick()
        {
            base.CompTick();

            if (!this.isRinging && this.bellAngle == 0f)
            {
                return;
            }
            if (this.isRinging)
            {
                this.TryInitializeSustainer();

                //if (bellAngle == this.BellProps.RotationRange.max || bellAngle == this.BellProps.RotationRange.min)
                //{
                //    WorshipSoundDefOf.BellRingA.PlayOneShot(new TargetInfo(this.parent.Position, this.parent.Map));
                //}

                this.ringoutTicks--;
                if (this.ringoutTicks <= 0)
                {
                    this.isRinging = false;
                    if (bellSustainer != null)
                    {
                        bellSustainer.End();
                        bellSustainer = null;
                    }
                }

            }

            if (this.clockWise)
            {
                this.bellAngle++;
                if (this.bellAngle >= this.BellProps.RotationRange.max)
                {
                    this.clockWise = false;
                }
            }
            else
            {
                this.bellAngle--;
                if (this.bellAngle <= this.BellProps.RotationRange.min)
                {
                    this.clockWise = true;
                }
            }
        }

        private void TryInitializeSustainer()
        {
            if (this.bellSustainer == null)
            {
                bellSustainer = WorshipSoundDefOf.BellRingSustainer.TrySpawnSustainer(new TargetInfo(parent.Position, parent.Map));
            }
        }

        public void Notify_SermonStarting(BuildingAltar altar)
        {
            if (altar != null)
            {
                Lord sermon = altar.GetLord();
                if (sermon != null)
                {
                    foreach (var pawn in altar.Map.mapPawns.FreeColonistsSpawned.Where(x => x.Position.InHorDistOf(altar.Position, this.BellProps.maxDistance)))
                    {
                        if (!sermon.ownedPawns.Contains(pawn))
                        {
                            Lord curLord = pawn.GetLord();
                            if (curLord != null)
                            {
                                curLord.Notify_PawnLost(pawn, PawnLostCondition.LeftVoluntarily);
                            }
                            sermon.AddPawn(pawn);
                        }
                    }
                }
            }
        }

        public override void PostDraw()
        {
            var drawPos = RotateBell(this.parent.DrawPos + this.BellProps.graphicData.drawOffset, this.parent.DrawPos + this.BellProps.graphicData.drawOffset + new Vector3(0f, 0f, this.BellProps.graphicData.drawSize.y / 2f), this.bellAngle);

            //Graphics.DrawMesh(MeshPool.plane20, this.parent.DrawPos, Quaternion.AngleAxis(0f, Vector3.up), BaseContent.BlackMat, 0);
            if (this.BellGraphic != null)
            {
                Graphics.DrawMesh(this.BellGraphic.MeshAt(Rot4.North), drawPos, Quaternion.AngleAxis(bellAngle, Vector3.up), this.BellGraphic.MatSingle, 1);
            }
            base.PostDraw();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref bellAngle, "bellAngle");
            Scribe_Values.Look<bool>(ref clockWise, "clockWise");
            Scribe_Values.Look<int>(ref ringoutTicks, "ringoutTicks");
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            GenDraw.DrawRadiusRing(this.parent.Position, Math.Min(GenRadial.MaxRadialPatternRadius, this.BellProps.RingRange));
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (Prefs.DevMode)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "Debug: Ring Bell";
                command_Action.action = delegate
                {
                    this.ringoutTicks = 600;
                    this.isRinging = !this.isRinging;
                };
                yield return command_Action;
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
            {
                string text = "RingBellTower".Translate();
                yield return new FloatMenuOption(text, delegate
                {
                    Job job = JobMaker.MakeJob(WorshipJobDefOf.RingBellTower, parent);
                    selPawn.jobs.TryTakeOrderedJob(job);
                });
            }
        }
    }

    public class CompProperties_BellTower : CompProperties
    {
        public GraphicData graphicData;

        public IntRange RotationRange = new IntRange(-30, 30);

        public float RingRange = 30f;

        public int maxDistance = 20;

        public int ringoutTicks = 120;

        public CompProperties_BellTower()
        {
            this.compClass = typeof(CompBellTower);
        }
    }
}
