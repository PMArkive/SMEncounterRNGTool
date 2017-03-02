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
        public bool UB_S = false;
        public bool nogender;
        public int gender_ratio;

        public static bool createtimeline;
        public static bool Considerdelay;
        public static int PreDelayCorrection = 0;
        public static int delaytime = 93; //For honey 186F =3.1s
        public static bool ConsiderBlink = true;
        public static int Modelnumber;
        public static int[] remain_frame;
        public static bool[] blink_flag;

        public static void ResetModelStatus()
        {
            remain_frame = new int[Modelnumber];
            blink_flag = new bool[Modelnumber];
        }

        public static bool IsSolgaleo;
        public static bool IsLunala;

        public class RNGResult
        {
            public int Nature;
            public int Clock;
            public uint PID, EC, PSV;
            public ulong row_r;
            public int[] IVs;
            public int[] p_Status;
            public bool Shiny;
            public bool Synchronize;
            public int Blink;
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
            public int IVsCount;
            public bool YourID;
            public int PIDType;
            public bool AbilityLocked;
            public bool NatureLocked;
            public bool GenderLocked;
        }

        public RNGResult Generate()
        {
            RNGResult st = new RNGResult();
            index = 0;

            st.row_r = Rand[0];
            st.Clock = (int)(st.row_r % 17);
            st.Blink = ((int)(st.row_r & 0x7F)) > 0 ? 0 : 1;

            // Reset Model Status
            if (!createtimeline || Honey)
                ResetModelStatus();

            // ---Start here when press A button---

            if (Considerdelay)
                st.frameshift = getframeshift();

            // UB using honey
            if (Wild && UB && Honey)
                st.UbValue = getUBValue();

            //Synchronize
            if (Wild && !UB_S)
                st.Synchronize = (int)(getrand() % 100) >= 50;
            else if (UB_S)
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
                if (UB_S)
                {
                    Advance(1);
                    st.Synchronize = blink_process();
                }
            }

            //UB is determined above
            bool lengendary = UB_S || !Wild;
            bool ShinyLocked_S = UB_S || ShinyLocked;
            if (lengendary)
                st.Lv = PokeLv;

            // Wild Normal Pokemon
            bool Wild_S = Wild && !UB_S;
            if (Wild_S)
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
            if (lengendary) roll_count = 1;
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
            if (ShinyLocked_S && st.PSV == TSV)
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
            if (Wild_S || AlwaysSynchro)
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
            if (nogender || UB_S)
                st.Gender = 0;
            else
                st.Gender = ((int)(getrand() % 252) >= gender_ratio) ? 1 : 2;

            //Item
            if (Wild_S)
                st.Item = (int)(getrand() % 100);

            return st;
        }

        public RNGResult GenerateEvent(EventRule e)
        {
            RNGResult st = new RNGResult();
            index = 0;

            st.row_r = Rand[0];
            st.Clock = (int)(st.row_r % 17);
            st.Blink = ((int)(st.row_r & 0x7F)) > 0 ? 0 : 1;

            // Reset Model Status
            if (!createtimeline)
                ResetModelStatus();

            // ---Start here when press A button---

            if (Considerdelay)
                st.frameshift = getframeshift(e);

            //Encryption Constant & PID
            if (e.PIDType < 3) // =3 Fixed
            {
                st.EC = (uint)(getrand() & 0xFFFFFFFF);
                st.PID = (uint)(getrand() & 0xFFFFFFFF);
                st.PSV = ((st.PID >> 16) ^ (st.PID & 0xFFFF)) >> 4;
                if (st.PSV == TSV && e.PIDType < 2)
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
            if (!e.AbilityLocked)
                st.Ability = (int)(getrand() & 1) + 1;

            //Nature
            if (!e.NatureLocked)
                st.Nature = (int)(currentrand() % 25);

            //Gender
            if (nogender || e.GenderLocked)
                st.Gender = 0;
            else
                st.Gender = ((int)(getrand() % 252) >= gender_ratio) ? 1 : 2;

            return st;
        }

        public static List<ulong> Rand = new List<ulong>();
        private static int index;
        public static void Resetindex() { index = 0; }

        public static void CreateBuffer(SFMT sfmt)
        {
            int RandBuffSize = 200;
            if (Considerdelay)
                RandBuffSize += Modelnumber * delaytime;

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
            Fix3v = UB_S = UbValue < UB_th;
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
                ButtonPressDelay();         //4-6F
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
                time_elapse(delaytime - crydelay);
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

        // n/30s elapsed
        private static void time_elapse(int n)
        {
            for (int totalframe = 0; totalframe < n; totalframe++)
            {
                for (int i = 0; i < Modelnumber; i++)
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
