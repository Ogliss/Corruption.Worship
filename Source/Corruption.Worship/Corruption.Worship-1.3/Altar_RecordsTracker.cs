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
    public class Altar_RecordsTracker : IExposable
    {
        private DefMap<RecordDef, float> records = new DefMap<RecordDef, float>();

		public void Increment(RecordDef def)
		{
			if (def.type != RecordType.Int)
			{
				Log.Error("Tried to increment record \"" + def.defName + "\" whose record type is \"" + def.type + "\".");
			}
			else
			{
				records[def] = Mathf.Round(records[def] + 1f);
			}
		}

		public void AddTo(RecordDef def, float value)
		{
			if (def.type == RecordType.Int)
			{
				records[def] = Mathf.Round(records[def] + Mathf.Round(value));
			}
			else if (def.type == RecordType.Float)
			{
				records[def] += value;
			}
			else
			{
				Log.Error("Tried to add value to record \"" + def.defName + "\" whose record type is \"" + def.type + "\".");
			}
		}

		public float GetValue(RecordDef def)
		{
			float num = records[def];
			if (def.type == RecordType.Int || def.type == RecordType.Time)
			{
				return Mathf.Round(num);
			}
			return num;
		}

		public int GetAsInt(RecordDef def)
		{
			return Mathf.RoundToInt(records[def]);
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref records, "records");
			BackCompatibility.PostExposeData(this);
		}
    }
}
