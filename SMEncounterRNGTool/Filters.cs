﻿using System.Linq;

namespace SMEncounterRNGTool
{
    class Filters
    {
        public bool[] Nature;
        public bool[] HPType;
        public int Ability = -1;
        public int Gender = -1;
        public int[] IVup, IVlow, Stats;
        public byte PerfectIVs;
        public bool Skip;
        public byte Lv;
        public bool[] Slot;

        public bool CheckIVs(RNGResult result)
        {
            for (int i = 0; i < 6; i++)
                if (IVlow[i] > result.IVs[i] || result.IVs[i] > IVup[i])
                    return false;
            if (result.IVs.Count(e => e == 31) < PerfectIVs)
                return false;
            return true;
        }

        public bool CheckStats(RNGResult result, int[] BS)
        {
            int[] IV = result.IVs;
            result.Stats = new int[6];
            result.Stats[0] = (((BS[0] * 2 + IV[0]) * result.Lv) / 100) + result.Lv + 10;
            for (int i = 1; i < 6; i++)
                result.Stats[i] = (((BS[i] * 2 + IV[i]) * result.Lv) / 100) + 5;
            Pokemon.NatureAdjustment(result.Stats, result.Nature);

            for (int i = 0; i < 6; i++)
                if (Stats[i] != 0 && Stats[i] != result.Stats[i])
                    return false;
            return true;
        }

        public bool CheckNature(int resultnature)
        {
            if (Nature.All(n => !n)) return true;
            return Nature[resultnature];
        }
        public bool CheckHiddenPower(RNGResult result)
        {
            var val = Pokemon.getHiddenPowerValue(result.IVs);
            result.hiddenpower = (byte)val;
            if (HPType.All(n => !n)) return true;
            return HPType[val];
        }

        public bool CheckSlot(int slot)
        {
            if (Slot.All(n => !n)) return true;
            return Slot[slot];
        }
    }
}
