﻿using System.Linq;

namespace SMEncounterRNGTool
{
    class Filters
    {
        #region calc_data
        public readonly static double[,] natures_mag =
        {
            { 1, 1, 1, 1, 1, 1 },
            { 1, 1.1, 0.9, 1, 1, 1 },
            { 1, 1.1, 1, 1, 1, 0.9 },
            { 1, 1.1, 1, 0.9, 1, 1 },
            { 1, 1.1, 1, 1, 0.9, 1 },
            { 1, 0.9, 1.1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1.1, 1, 1, 0.9 },
            { 1, 1, 1.1, 0.9, 1, 1 },
            { 1, 1, 1.1, 1, 0.9, 1 },
            { 1, 0.9, 1,1, 1, 1.1 },
            { 1, 1, 0.9, 1,1, 1.1 },
            { 1, 1,1, 1, 1, 1 },
            { 1, 1,1, 0.9, 1, 1.1 },
            { 1, 1,1, 1, 0.9, 1.1 },
            { 1, 0.9, 1, 1.1, 1,1 },
            { 1, 1, 0.9, 1.1, 1, 1 },
            { 1, 1, 1, 1.1, 1, 0.9 },
            { 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1.1, 0.9, 1 },
            { 1, 0.9, 1,1, 1.1, 1 },
            { 1, 1, 0.9, 1, 1.1, 1},
            { 1, 1, 1, 1, 1.1, 0.9 },
            { 1, 1, 1, 0.9, 1.1, 1 },
            { 1, 1, 1, 1, 1, 1}
        };
        #endregion

        public bool[] Nature;
        public bool[] HPType;
        public int Ability = -1;
        public int Gender = -1;
        public int[] IVup, IVlow, BS, Stats;
        public byte PerfectIVs;
        public bool Skip;
        public int Lv;
        public bool[] Slot;


        public bool CheckIVs(RNGSearch.RNGResult result)
        {
            for (int i = 0; i < 6; i++)
                if (IVlow[i] > result.IVs[i] || result.IVs[i] > IVup[i])
                    return false;
            if (result.IVs.Where(e => e == 31).Count() < PerfectIVs)
                return false;
            return true;
        }

        public bool CheckStats(RNGSearch.RNGResult result)
        {
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

        public void getStats(RNGSearch.RNGResult result)
        {
            int[] IV = result.IVs;
            result.Stats = new int[6];

            result.Stats[0] = (((BS[0] * 2 + IV[0]) * Lv) / 100) + Lv + 10;
            for (int i = 1; i < 6; i++)
                result.Stats[i] = (int)(((((BS[i] * 2 + IV[i]) * Lv) / 100) + 5) * natures_mag[result.Nature, i]);
        }

        public bool CheckHiddenPower(int[] IV)
        {
            if (HPType.All(n => !n)) return true;
            var val = 15 * ((IV[0] & 1) + 2 * (IV[1] & 1) + 4 * (IV[2] & 1) + 8 * (IV[5] & 1) + 16 * (IV[3] & 1) + 32 * (IV[4] & 1)) / 63;
            return HPType[val];
        }

        public bool CheckSlot(int slot)
        {
            if (Slot.All(n => !n)) return true;
            return Slot[slot];
        }
    }
}
