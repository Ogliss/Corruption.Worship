using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Corruption.Worship
{
    public class Building_WorshipStatue : Building_Art
    {
        private Rot4 statueRotaton;
        private string graphicPath;

        private Graphic_Multi graphicInner;

        public override Graphic Graphic
        {
            get
            {
                if (this.graphicPath == null)
                {
                    Graphic_Random baseGraphic = this.DefaultGraphic as Graphic_Random;
                    var gPath = baseGraphic.SubGraphicFor(this).path.Split('_');
                    var builder = new StringBuilder();
                    builder.Append(gPath[0]);
                    for (int i = 1; i < gPath.Length - 1; i++)
                    {
                        builder.Append("_" + gPath[i]);
                    }
                    this.graphicPath = builder.ToString();
                }
                if (this.graphicInner == null)
                {
                    this.graphicInner = (Graphic_Multi)GraphicDatabase.Get(typeof(Graphic_Multi), this.graphicPath, this.def.graphicData.shaderType.Shader, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo, this.def.graphicData, this.def.graphicData.shaderParameters);
                }
                return this.graphicInner;
            }
        }
        
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            this.Graphic.Draw(drawLoc, this.statueRotaton, this);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            var rotateCommand = new Command_Action();
            rotateCommand.defaultLabel = "RotateThingRight".Translate();
            rotateCommand.defaultDesc = "RotateThingRightDesc".Translate();
            rotateCommand.icon = TexUI.RotRightTex;
            rotateCommand.action = delegate
            {
                this.statueRotaton.Rotate(RotationDirection.Clockwise);
            };
            yield return rotateCommand;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Rot4>(ref this.statueRotaton, "statueRotaton");
            Scribe_Values.Look<string>(ref this.graphicPath, "graphicPath");
        }
    }
}
