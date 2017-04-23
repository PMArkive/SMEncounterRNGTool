using System.Linq;

namespace SMEncounterRNGTool
{
    class Filters
    {
        public bool[] Nature;
        public bool[] HPType;
        public int Ability = -1;
        public int Gender = -1;
        public int[] IVup, IVlow, BS, Stats;
        public byte PerfectIVs;
        public bool Skip;
        public byte Lv;
        public bool[] Slot;
        public bool Wild;
        public bool SafeFOnly, BlinkFOnly, ShinyOnly, EncounterOnly, UBOnly;
        public byte Encounter_th, UB_th;
        public bool MainRNGEgg;

        private bool CheckIVs(RNGResult result)
        {
            for (int i = 0; i < 6; i++)
                if (IVlow[i] > result.IVs[i] || result.IVs[i] > IVup[i])
                    return false;
            if (result.IVs.Count(e => e == 31) < PerfectIVs)
                return false;
            return true;
        }

        private bool CheckStats(RNGResult result)
        {
            result.Stats = Pokemon.getStats(result.IVs, result.Nature, result.Lv, BS);
            for (int i = 0; i < 6; i++)
                if (Stats[i] != 0 && Stats[i] != result.Stats[i])
                    return false;
            return true;
        }

        private bool CheckNature(int resultnature)
        {
            if (Nature.All(n => !n)) return true;
            return Nature[resultnature];
        }

        private bool CheckHiddenPower(RNGResult result)
        {
            var val = Pokemon.getHiddenPowerValue(result.IVs);
            result.hiddenpower = (byte)val;
            if (HPType.All(n => !n)) return true;
            return HPType[val];
        }

        private bool CheckSlot(int slot)
        {
            if (Slot.All(n => !n)) return true;
            return Slot[slot];
        }

        private bool CheckBlink(int blinkflag)
        {
            if (BlinkFOnly)
                return blinkflag > 4;
            if (SafeFOnly)
                return blinkflag < 2;
            return true;
        }

        public bool CheckResult(RNGResult result)
        {
            if (Skip)
                return true;
            if (ShinyOnly && !result.Shiny)
                return false;
            if (!CheckBlink(result.Blink))
                return false;
            if (MainRNGEgg)
                return true;
            if (BS == null ? !CheckIVs(result) : !CheckStats(result))
                return false;
            if (!CheckHiddenPower(result))
                return false;
            if (!CheckNature(result.Nature))
                return false;
            if (Gender != 0 && Gender != result.Gender)
                return false;
            if (Ability != 0 && Ability != result.Ability)
                return false;
            if (Wild)
            {
                if (Lv != 0 && Lv != result.Lv)
                    return false;
                if (EncounterOnly && result.Encounter >= Encounter_th)
                    return false;
                if (UBOnly && result.SpecialEnctrValue >= UB_th)
                    return false;
                if (!CheckSlot(result.Slot))
                    return false;
            }
            return true;
        }
    }
}
