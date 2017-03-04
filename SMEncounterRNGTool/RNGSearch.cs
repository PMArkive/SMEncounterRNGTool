using System.Collections.Generic;

namespace SMEncounterRNGTool
{
    class RNGSearch
    {
        // Search Settings

        public bool AlwaysSynchro;
        public int Synchro_Stat;
        public bool Fix3v;

        public int TSV;
        public bool ShinyLocked;
        public bool ShinyCharm;

        public int PokeLv;

        public bool Wild, Honey, UB;
        public int Lv_max, Lv_min;
        public int UB_th;
        public bool IsUB = false;
        public bool nogender;
        public int gender_ratio;

        public static bool createtimeline;
        public static bool Considerdelay;
        public static int PreDelayCorrection = 0;
        public static int delaytime = 93; //For honey 186F =3.1s
        public static bool ConsiderBlink = true;
        public static int modelnumber;
        public static int[] remain_frame;
        public static bool[] blink_flag;

        public static void ResetModelStatus()
        {
            remain_frame = new int[modelnumber];
            blink_flag = new bool[modelnumber];
        }

        public static bool IsSolgaleo;
        public static bool IsLunala;

        public class RNGResult
        {
            public int Nature;
            public byte Clock;
            public uint PID, EC, PSV;
            public ulong row_r;
            public int[] IVs;
            public int[] p_Status;
            public bool Shiny;
            public bool Synchronize;
            public byte Blink;
            public int frameshift;

            public int Encounter = -1;
            public int Gender;
            public int Ability = 1;
            public int UbValue = 100;
            public int Slot = -1;
            public int Lv = -1;
            public int Item = 100;

            public int realtime = -1;
        }

        public class EventRule
        {
            public int[] IVs;
            public uint TSV;
            public int IVsCount;
            public bool YourID;
            public int PIDType;
            public bool AbilityLocked;
            public byte Ability;
            public bool NatureLocked;
            public byte Nature;
            public bool GenderLocked;
            public byte Gender;
            public bool OtherInfo;
            public int TID = -1;
            public int SID = -1;
            public uint EC = 0;
            public uint PID = 0;
        }

        public RNGResult Generate()
        {
            RNGResult st = new RNGResult();
            index = 0;

            st.row_r = Rand[0];
            st.Clock = (byte)(st.row_r % 17);

            // Reset model Status
            if (!createtimeline || Honey)
                ResetModelStatus();

            // ---Start here when press A button---

            st.frameshift = getframeshift();
            if (!Considerdelay)
            {
                Resetindex(); ResetModelStatus();
            }

            // UB using honey
            if (Wild && UB && Honey)
                st.UbValue = getUBValue();

            //Synchronize
            if (Wild && !IsUB)
                st.Synchronize = (int)(getrand() % 100) >= 50;
            else if (IsUB)
            {
                if (ConsiderBlink)
                {
                    time_elapse(7);
                    st.Synchronize = blink_process();
                }
            }
            else if (!Wild)
            {
                if (AlwaysSynchro)
                    st.Synchronize = true;
                else if (ConsiderBlink)
                    st.Synchronize = blink_process();
            }

            // Encounter
            if (Wild && !Honey)
                st.Encounter = (int)(getrand() % 100);

            // UB w/o Honey
            if (Wild && UB && !Honey)
            {
                st.UbValue = getUBValue();
                if (IsUB)
                {
                    Advance(1);
                    st.Synchronize = blink_process();
                }
            }

            //UB is determined above
            bool Listed = IsUB || !Wild;
            bool IsShinyLocked = IsUB || ShinyLocked;
            if (Listed)
                st.Lv = PokeLv;

            // Wild Normal Pokemon
            bool IsWild = Wild && !IsUB;
            if (IsWild)
            {
                st.Slot = getslot((int)(getrand() % 100));
                st.Lv = (int)(getrand() % (ulong)(Lv_max - Lv_min + 1)) + Lv_min;
                Advance(1);
            }

            //Something
            if (!AlwaysSynchro)
                Advance(60);

            //Encryption Constant
            st.EC = (uint)(getrand() & 0xFFFFFFFF);

            //PID
            int roll_count = ShinyCharm ? 3 : 1;
            if (Listed) roll_count = 1;
            for (int i = 0; i < roll_count; i++)
            {
                st.PID = (uint)(getrand() & 0xFFFFFFFF);
                st.PSV = ((st.PID >> 16) ^ (st.PID & 0xFFFF)) >> 4;
                if (st.PSV == TSV)
                {
                    st.Shiny = true;
                    break;
                }
            }
            if (IsShinyLocked && st.PSV == TSV)
            {
                st.PID = st.PID ^ 0x10000000;
                st.PSV = st.PSV ^ 0x100;
                st.Shiny = false;
            }

            //IV
            st.IVs = new int[6] { -1, -1, -1, -1, -1, -1 };
            int cnt = Fix3v ? 3 : 0;
            while (cnt > 0)
            {
                int ran = (int)(getrand() % 6);
                if (st.IVs[ran] < 0)
                {
                    st.IVs[ran] = 31;
                    cnt--;
                }
            }
            for (int i = 0; i < 6; i++)
                if (st.IVs[i] < 0)
                    st.IVs[i] = (int)(getrand() & 0x1F);

            //Ability
            if (IsWild || AlwaysSynchro)
                st.Ability = (int)(getrand() & 1) + 1;

            //Nature
            st.Nature = (int)(currentrand() % 25);
            if (st.Synchronize)
            {
                if (Synchro_Stat >= 0) st.Nature = Synchro_Stat;
            }
            else
                index++;

            //Gender
            if (nogender || IsUB)
                st.Gender = 0;
            else
                st.Gender = ((int)(getrand() % 252) >= gender_ratio) ? 1 : 2;

            //Item
            if (IsWild)
                st.Item = (int)(getrand() % 100);

            return st;
        }

        public RNGResult GenerateEvent(EventRule e)
        {
            RNGResult st = new RNGResult();
            index = 0;

            st.row_r = Rand[0];
            st.Clock = (byte)(st.row_r % 17);

            // Reset model Status
            if (!createtimeline)
                ResetModelStatus();

            // ---Start here when press A button---

            st.frameshift = getframeshift(e);
            if (!Considerdelay)
            {
                Resetindex(); ResetModelStatus();
            }

            //Encryption Constant
            st.EC = e.EC > 0 ? e.EC : (uint)(getrand() & 0xFFFFFFFF);

            //PID
            st.PID = e.PIDType == 3 ? e.PID : (uint)(getrand() & 0xFFFFFFFF);
            st.PSV = ((st.PID >> 16) ^ (st.PID & 0xFFFF)) >> 4;
            if (st.PSV == e.TSV && e.PIDType < 2)
            {
                if (e.PIDType == 0) // Random
                    st.Shiny = true;
                else if (e.PIDType == 1) // Random NonShiny
                {
                    st.PID = st.PID ^ 0x10000000;
                    st.PSV = st.PSV ^ 0x100;
                }
            }
            else if (e.PIDType == 2)// Random Shiny
            {
                if (e.OtherInfo)
                {
                    st.PID = (uint)(((e.TID ^ e.SID ^ (st.PID & 0xFFFF)) << 16) + (st.PID & 0xFFFF));
                    st.PSV = e.TSV;
                }
                st.Shiny = true;
            }

            //IV
            st.IVs = (int[])e.IVs.Clone();
            int cnt = e.IVsCount;
            while (cnt > 0)
            {
                int ran = (int)(getrand() % 6);
                if (st.IVs[ran] < 0)
                {
                    st.IVs[ran] = 31;
                    cnt--;
                }
            }
            for (int i = 0; i < 6; i++)
                if (st.IVs[i] < 0)
                    st.IVs[i] = (int)(getrand() & 0x1F);

            //Ability
            st.Ability = e.AbilityLocked ? e.Ability : (int)(getrand() & 1) + 1;

            //Nature
            st.Nature = e.NatureLocked ? e.Nature : (int)(getrand() % 25);

            //Gender
            st.Gender = (e.GenderLocked || nogender) ? e.Gender : (int)(getrand() % 252) >= gender_ratio ? 1 : 2;
            return st;
        }

        public static List<ulong> Rand = new List<ulong>();
        private static int index;
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

        private static ulong getrand()
        {
            return Rand[index++];
        }

        private static ulong currentrand()
        {
            return Rand[index];
        }

        private static void Advance(int d)
        {
            index += d;
        }

        private int getUBValue()
        {
            int UbValue = (int)(getrand() % 100);
            Fix3v = IsUB = UbValue < UB_th;
            return UbValue;
        }

        public static int getslot(int rand)
        {
            if (rand < 20)
                return 1;
            if (rand < 40)
                return 2;
            if (rand < 50)
                return 3;
            if (rand < 60)
                return 4;
            if (rand < 70)
                return 5;
            if (rand < 80)
                return 6;
            if (rand < 90)
                return 7;
            if (rand < 95)
                return 8;
            if (rand < 99)
                return 9;
            return 10;
        }

        public int getframeshift()
        {
            if (!Wild)
                ButtonPressDelay();          //4-6F
            if (Honey)
                index += PreDelayCorrection; //Pre-HoneyCorrection
            time_delay();
            return index;
        }

        public int getframeshift(EventRule e)
        {
            ButtonPressDelay();
            if (e.YourID)
            {
                Advance(10);
                time_delay();
            }
            return index;
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

        private static bool blink_process()
        {
            bool sync = (int)(getrand() % 100) >= 50;
            time_elapse(3);
            return sync;
        }

        //model # changes when screen turns black
        private static void Rearrange()
        {
            modelnumber = 5;//2 guys offline...
            int[] order = new int[5] { 0, 1, 2, 5, 6 };
            for (int i = 0; i < 5; i++)
            {
                remain_frame[i] = remain_frame[order[i]];
                blink_flag[i] = blink_flag[order[i]];
            }
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
                        //Blinking
                        if (blink_flag[i])
                        {
                            remain_frame[i] = (int)(getrand() % 3) == 0 ? 36 : 30;
                            blink_flag[i] = false;
                        }
                        //Not Blinking
                        else if ((int)(getrand() & 0x7F) == 0)
                        {
                            remain_frame[i] = 5;
                            blink_flag[i] = true;
                        }
                    }
                }
            }
        }

    }
}