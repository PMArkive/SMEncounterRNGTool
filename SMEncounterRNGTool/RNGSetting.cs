using System.Linq;
using System.Collections.Generic;

namespace SMEncounterRNGTool
{
    class RNGSetting
    {
        // Background Info (Global variables)
        public static bool AlwaysSynchro;
        public static byte Synchro_Stat;
        public static bool Fix3v;
        public static int TSV;
        public static bool ShinyLocked;
        public static bool ShinyCharm;
        public static byte PokeLv;

        public static bool Wild, Honey, UB, fishing, SOS;
        public static byte Lv_max, Lv_min;
        public static byte ChainLength;
        public static byte UB_th, Encounter_th;
        public static bool nogender;
        public static byte gender_ratio;

        public static byte slottype;
        public static bool Considerhistory;
        public static bool ConsiderBagEnteringTime;
        public static bool Considerdelay;
        public static int PreDelayCorrection;
        public static int delaytime = 93; //For honey 186F =3.1s
        public static byte modelnumber;
        public static int[] remain_frame;
        public static bool[] blink_flag;

        // Personal Info
        public bool IsUB;

        // Generated Attributes
        private bool IsShinyLocked => ShinyLocked || IsUB;
        private bool IsWild => Wild && !IsUB;
        private bool NoGender => nogender || IsUB;
        private int PIDroll_count => (ShinyCharm && IsWild ? 3 : 1) + (SOS ? AddtionalPIDRollCount() : 0);
        private int PerfectIVCount => Fix3v || IsUB ? 3 : 0;
        private static bool SpecialWild => Encounter_th == 101 || SOS;

        public static void ResetModelStatus()
        {
            remain_frame = new int[modelnumber];
            blink_flag = new bool[modelnumber];
        }

        public static bool IsSolgaleo;
        public static bool IsLunala;
        public static bool SolLunaReset;

        public RNGResult Generate(EventRule e = null)
        {
            if (e == null)
                return GenerateRegular();
            return GenerateEvent(e);
        }

        public RNGResult GenerateRegular()
        {
            RNGResult st = new RNGResult();
            index = 0;
            st.row_r = Rand[0];
            st.Clock = (byte)(st.row_r % 17);
            st.Lv = PokeLv;

            // Reset model Status
            if (!Considerhistory)
                ResetModelStatus();

            // ---Start here when press A button---

            // timedelay before generating
            st.frameshift = getframeshift();

            // Reset status
            if (!Considerdelay)
            {
                Resetindex();
                ResetModelStatus();
            }

            if (Wild)
                GenerateWild(st); // Get sync/slot/encounter info
            else
                GenerateStationary(st); // Get sync info

            st.Synchronize &= Synchro_Stat < 25; //Check if has Synchronizer

            //Something
            if (!AlwaysSynchro && !SOS)
                Advance(60);

            //EC-PID-IVs-Nature Cap

            //Encryption Constant
            st.EC = (uint)(getrand & 0xFFFFFFFF);

            //PID
            for (int i = 0; i < PIDroll_count; i++)
            {
                st.PID = (uint)(getrand & 0xFFFFFFFF);
                if (st.PSV == TSV)
                    break;
            }
            if (IsShinyLocked && st.PSV == TSV)
                st.PID ^= 0x10000000;
            st.Shiny = st.PSV == TSV;

            //IV
            st.IVs = new[] { -1, -1, -1, -1, -1, -1 };
            while (st.IVs.Count(iv => iv == 31) < PerfectIVCount)
                st.IVs[(int)(getrand % 6)] = 31;
            for (int i = 0; i < 6; i++)
                if (st.IVs[i] < 0)
                    st.IVs[i] = (int)(getrand & 0x1F);

            //Ability
            st.Ability = (byte)(IsWild || AlwaysSynchro ? (getrand & 1) + 1 : 1);

            //Nature
            st.Nature = (byte)(st.Synchronize ? Synchro_Stat : getrand % 25);

            //Gender
            st.Gender = (byte)(NoGender ? 0 : ((int)(getrand % 252) >= gender_ratio ? 1 : 2));

            //Item
            st.Item = (byte)(IsWild && !SOS ? getrand % 100 : 100);

            return st;
        }
        private void GenerateStationary(RNGResult st)
        {
            if (AlwaysSynchro)
            {
                st.Synchronize = true;
                return;
            }
            st.Synchronize = blink_process();
            if (SolLunaReset)
                modelnumber = 7;
        }
        private void GenerateWild(RNGResult st)
        {
            if (Honey)
                GenerateHoney(st);
            else
                GenerateNonHoney(st);

            // Wild Normal Pokemon
            if (IsWild && !SpecialWild)
            {
                st.Slot = getslot((int)(getrand % 100));
                st.Lv = (byte)(getrand % (ulong)(Lv_max - Lv_min + 1) + Lv_min);
                Advance(1);
            }
        }
        private void GenerateHoney(RNGResult st)
        {
            if (UB)
                st.UbValue = getUBValue();
            // Normal wild
            if (!IsUB)
            {
                st.Synchronize = (int)(getrand % 100) >= 50;
                return;
            }
            // UB
            time_elapse(7);
            st.Synchronize = blink_process();
        }
        private void GenerateNonHoney(RNGResult st)
        {
            // SOS
            if (SOS) return;

            // Fishing Encounter
            if (fishing)
            {
                st.Encounter = (int)(getrand % 100);
                time_elapse(12);
            }
            // Sync
            st.Synchronize = (int)(getrand % 100) >= 50;

            // Encounter
            if (Encounter_th == 101 || fishing) return;
            st.Encounter = (int)(getrand % 100);

            // UB
            if (!UB) return;
            st.UbValue = getUBValue();
            if (IsUB)
            {
                Advance(1);
                st.Synchronize = blink_process();
            }
        }

        public RNGResult GenerateEvent(EventRule e)
        {
            RNGResult st = new RNGResult();
            index = 0;

            st.row_r = Rand[0];
            st.Clock = (byte)(st.row_r % 17);
            st.Lv = PokeLv;

            // Reset model Status
            if (!Considerhistory)
                ResetModelStatus();

            // ---Start here when press A button---

            st.frameshift = getframeshift(e);
            if (!Considerdelay)
            {
                Resetindex();
                ResetModelStatus();
            }

            //Encryption Constant
            st.EC = e.EC > 0 ? e.EC : (uint)(getrand & 0xFFFFFFFF);

            //PID
            switch (e.PIDType)
            {
                case 0: //Random PID
                    st.PID = (uint)(getrand & 0xFFFFFFFF);
                    break;
                case 1: //Random NonShiny
                    st.PID = (uint)(getrand & 0xFFFFFFFF);
                    if (st.PSV == e.TSV)
                        st.PID ^= 0x10000000;
                    break;
                case 2: //Random Shiny
                    st.PID = (uint)(getrand & 0xFFFFFFFF);
                    if (e.OtherInfo)
                        st.PID = (uint)(((e.TID ^ e.SID ^ (st.PID & 0xFFFF)) << 16) + (st.PID & 0xFFFF));
                    break;
                case 3: //Specified
                    st.PID = e.PID;
                    break;
            }
            st.Shiny = e.PIDType != 1 && (e.PIDType == 2 || st.PSV == e.TSV);

            //IV
            st.IVs = (int[])e.IVs.Clone();
            int cnt = e.IVsCount;
            while (cnt > 0)
            {
                int ran = (int)(getrand % 6);
                if (st.IVs[ran] < 0)
                {
                    st.IVs[ran] = 31;
                    cnt--;
                }
            }
            for (int i = 0; i < 6; i++)
                if (st.IVs[i] < 0)
                    st.IVs[i] = (int)(getrand & 0x1F);

            //Ability
            st.Ability = e.AbilityLocked ? e.Ability : (byte)(e.Ability == 0 ? (getrand & 1) + 1 : getrand % 3 + 1);

            //Nature
            st.Nature = e.NatureLocked ? e.Nature : (byte)(getrand % 25);

            //Gender
            st.Gender = e.GenderLocked || nogender ? e.Gender : (byte)(getrand % 252 >= gender_ratio ? 1 : 2);
            return st;
        }

        public static List<ulong> Rand = new List<ulong>();
        private static int index;
        private static void Advance(int d) { index += d; }
        private static ulong getrand => Rand[index++];

        public static void Resetindex() { index = 0; }
        public static void CreateBuffer(SFMT sfmt, bool CalcDelay)
        {
            int RandBuffSize = 200;
            if (CalcDelay)
                RandBuffSize += modelnumber * delaytime;

            Rand.Clear();
            for (int i = 0; i < RandBuffSize; i++)
                Rand.Add(sfmt.NextUInt64());
        }

        // n/30s elapsed
        private static void time_elapse(int n)
        {
            for (int totalframe = 0; totalframe < n; totalframe++)
            {
                for (int i = 0; i < modelnumber; i++)
                {
                    if (remain_frame[i] > 0)
                        remain_frame[i]--;
                    if (remain_frame[i] == 0)
                    {
                        if (blink_flag[i])                      //Blinking
                        {
                            remain_frame[i] = (int)(getrand % 3) == 0 ? 36 : 30;
                            blink_flag[i] = false;
                        }
                        else if ((int)(getrand & 0x7F) == 0)  //Not Blinking
                        {
                            remain_frame[i] = 5;
                            blink_flag[i] = true;
                        }
                    }
                }
            }
        }

        private static void ButtonPressDelay()
        {
            time_elapse(2);
        }

        private static void time_delay()
        {
            if (IsSolgaleo || IsLunala)
            {
                int crydelay = IsSolgaleo ? 79 : 76;
                time_elapse(delaytime - crydelay - 19);
                if (modelnumber == 7) Rearrange();
                time_elapse(19);
                Advance(1);     //Cry Inside Time Delay
                time_elapse(crydelay);
            }
            else
                time_elapse(delaytime);
        }

        public static int getframeshift()
        {
            if (Honey)
            {
                if (ConsiderBagEnteringTime)
                    time_elapse(6);
                ResetModelStatus();
                time_elapse(1);              //Blink process also occurs when loading map
                index += PreDelayCorrection - modelnumber;  //Pre-HoneyCorrection
            }
            else
                ButtonPressDelay();          //4F
            time_delay();
            return index;
        }

        public static int getframeshift(EventRule e)
        {
            ButtonPressDelay();
            if (e.YourID && !e.IsEgg)
            {
                Advance(10);
                time_delay();
            }
            return index;
        }

        private static bool blink_process()
        {
            bool sync = (int)(getrand % 100) >= 50;
            time_elapse(3);
            return sync;
        }

        //model # changes when screen turns black
        private static void Rearrange()
        {
            modelnumber = 5;//2 guys offline...
            int[] order = new[] { 0, 1, 2, 5, 6 };
            for (int i = 0; i < 5; i++)
            {
                remain_frame[i] = remain_frame[order[i]];
                blink_flag[i] = blink_flag[order[i]];
            }
        }

        private byte getUBValue()
        {
            byte UbValue = (byte)(getrand % 100);
            IsUB = UbValue < UB_th;
            return UbValue;
        }

        public static byte getslot(int rand)
        {
            byte[] SlotSplitter = SlotDistribution[slottype];
            for (byte i = 1; i <= 9; i++)
            {
                rand -= SlotSplitter[i - 1];
                if (rand < 0)
                    return i;
            }
            return 10;
        }

        private readonly static byte[][] SlotDistribution = new byte[][]
        {
            new byte[] { 20,20,10,10,10,10,10,5,4,1 },
            new byte[] { 10,10,20,20,10,10,10,5,4,1 },
        };

        private static byte AddtionalPIDRollCount()
        {
            if (ChainLength < 11) return 0;
            if (ChainLength < 21) return 4;
            if (ChainLength < 31) return 8;
            return 12;
        }

        private static byte getperfectivcount()
        {
            if (ChainLength < 5) return 0;
            if (ChainLength < 10) return 1;
            if (ChainLength < 20) return 2;
            if (ChainLength < 30) return 3;
            return 4;
        }

        private static byte getHAthershold()
        {
            if (ChainLength < 10) return 0;
            if (ChainLength < 20) return 5;
            if (ChainLength < 30) return 10;
            return 15;
        }
    }
}