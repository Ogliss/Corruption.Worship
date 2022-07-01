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
	[StaticConstructorOnStartup]
    public class CompFireOverlayColored : CompFireOverlayBase
	{
		protected CompRefuelable refuelableComp;
		protected CompFlickable flickableComp;
		protected BuildingAltar altar;

		public static readonly Graphic FireGraphicGreen = GraphicDatabase.Get<Graphic_Flicker>("Things/Overlays/FireGreen", ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);
		public static readonly Graphic FireGraphicBlue = GraphicDatabase.Get<Graphic_Flicker>("Things/Overlays/FireBlue", ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);
		public static readonly Graphic FireGraphicPink = GraphicDatabase.Get<Graphic_Flicker>("Things/Overlays/FirePink", ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);

		public Graphic FireGraphic = CompFireOverlay.FireGraphic;

		public new CompProperties_FireOverlayColorable Props => (CompProperties_FireOverlayColorable)props;

		private List<CompProperties_FireOverlayColorable.FirePosition> firePositions = new List<CompProperties_FireOverlayColorable.FirePosition>();

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			switch (Props.colorOption)
			{
				case CompProperties_FireOverlayColorable.FireColorOption.Blue:
					{
						this.FireGraphic = FireGraphicBlue;
						break;
					}
				case CompProperties_FireOverlayColorable.FireColorOption.Green:
					{
						this.FireGraphic = FireGraphicGreen;
						break;
					}
				case CompProperties_FireOverlayColorable.FireColorOption.Pink:
					{
						this.FireGraphic = FireGraphicPink;
						break;
					}
			}
		}

		public override void PostDraw()
		{
			base.PostDraw();
			if ((refuelableComp == null || refuelableComp.HasFuel) && ( flickableComp == null || flickableComp.SwitchIsOn ) && (this.altar == null || this.altar.CurrentActiveSermon != null))
			{
				foreach (var pos in this.firePositions)
				{
					Vector3 drawPos = parent.DrawPos + pos.offset;
					drawPos.y += 0.2f;
					FireGraphic.Draw(drawPos, Rot4.North, parent);
				}
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			refuelableComp = parent.GetComp<CompRefuelable>();
			flickableComp = parent.GetComp<CompFlickable>();
			altar = this.parent as BuildingAltar;
			this.firePositions.Clear();
			this.firePositions = this.Props.firePositions.FindAll(x => x.rotation == this.parent.Rotation);
			if (this.firePositions.NullOrEmpty())
			{
				this.firePositions = this.Props.firePositions.FindAll(x => x.rotation == Rot4.North);
			}
		}
	}

	public class CompProperties_FireOverlayColorable : CompProperties_FireOverlay
	{
		public List<FirePosition> firePositions = new List<FirePosition>();

		public CompProperties_FireOverlayColorable.FireColorOption colorOption;

		public enum FireColorOption
		{
			Base,			
			Blue,
			Green,
			Pink
		}

		public string texPath = "";

		public CompProperties_FireOverlayColorable()
		{
			this.compClass = typeof(CompFireOverlayColored);		
		}

		public sealed class FirePosition
		{
			public Rot4 rotation;
			public Vector3 offset;

			public override string ToString()
			{
				return string.Concat("Rot:", rotation.ToString(), ";", "Offset:", offset.ToString());
			}
		}
	}
}
