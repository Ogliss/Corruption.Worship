using Corruption.Core.Gods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Corruption.Worship
{
    public class CompShrine : ThingComp, IThingHolder, IOpenable
    {
        public CompProperties_Shrine Props
        {
            get
            {
                return this.props as CompProperties_Shrine;
            }
        }

        public Thing InstalledEffigy
        {
            get
            {
                return (this.innerContainer.Count != 0) ? this.innerContainer[0] : null;
            }
        }

        public bool HasEffigy
        {
            get
            {
                return this.InstalledEffigy != null && this.Props.requiresEffigy == true;
            }
        }

        public Thing thingToInstall;

        protected ThingOwner innerContainer;

        public CompShrine()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (parent.Faction == Faction.OfPlayer && innerContainer.Count > 0)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.action = DropEffigy;
                command_Action.defaultLabel = "CommandDropEffigy".Translate();
                command_Action.defaultDesc = "CommandDropEffigyDesc".Translate();
                if (HasEffigy == false)
                {
                    command_Action.Disable("CommandDropEffigyFailure".Translate());
                }
                command_Action.hotKey = KeyBindingDefOf.Misc8;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject");
                yield return command_Action;
            }
        }


        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var option in base.CompFloatMenuOptions(selPawn))
            {
                yield return option;
            }

            if (this.Props.requiresEffigy && this.InstalledEffigy == null)
            {
                foreach (var effigy in selPawn.inventory.GetDirectlyHeldThings().Where(x => x is ItemEffigy))
                {
                    yield return new FloatMenuOption("InstallEffigy".Translate(effigy.Label), delegate
                    {
                        selPawn.inventory.innerContainer.TryDrop(effigy, selPawn.Position, selPawn.Map, ThingPlaceMode.Near, out Thing _);
                        Job job = ItemEffigy.InstallEffigyJob(selPawn, effigy, this.parent);
                        selPawn.jobs.StartJob(job, JobCondition.InterruptForced, null, true);
                    });
                }

                foreach (var effigy in selPawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling().Where(x => x is ItemEffigy))
                {
                    yield return new FloatMenuOption("InstallEffigy".Translate(effigy.Label), delegate
                    {
                        Job job = ItemEffigy.InstallEffigyJob(selPawn, effigy, this.parent);
                        selPawn.jobs.StartJob(job, JobCondition.InterruptForced, null, true);
                    });
                }
            }

        }

        public bool CanOpen
        {
            get
            {
                return this.InstalledEffigy != null && this.Props.requiresEffigy == true;
            }
        }

        public void Open()
        {
            this.DropEffigy();
        }

        public void DropEffigy()
        {
            IntVec3 c = this.parent.Position;
            if (this.parent.def.hasInteractionCell)
            {
                c = this.parent.InteractionCell;
            }
            this.innerContainer.TryDropAll(c, this.parent.Map, ThingPlaceMode.Near);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (this.HasEffigy)
            {
                Mesh mesh = MeshPool.plane10;
                Vector3 vector = this.parent.DrawPos;
                vector.y += 1f;
                vector.z += 0.11f;
                Vector3 s = new Vector3(1.0f, 1f, 1.0f);
                Matrix4x4 matrix = default(Matrix4x4);
                Mesh drawMesh = MeshPool.plane10;
                if (this.parent.Rotation == Rot4.West)
                {
                    drawMesh = MeshPool.plane10Flip;
                }
                matrix.SetTRS(vector, Quaternion.AngleAxis(0f, Vector3.up), s);
                Graphics.DrawMesh(drawMesh, matrix, this.InstalledEffigy.Graphic.MatAt(this.parent.Rotation, null), 0);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
             this
            });
        }
    }

    public class CompProperties_Shrine : CompProperties
    {
        public Vector3 EffigyDrawOffset;

        public float worshipFactor = 10f;

        public bool requiresEffigy = true;

        public List<GodDef> dedicatedTo = new List<GodDef>();

        public CompProperties_Shrine()
        {
            this.compClass = typeof(CompShrine);
        }
    }
}
