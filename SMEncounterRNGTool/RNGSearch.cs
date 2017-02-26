﻿using System;
using System.Collections.Generic;

namespace SMEncounterRNGTool
{
    class RNGSearch
    {
        // Search Settings
        public int TSV;

        public bool AlwaysSynchro;
        public int Synchro_Stat;
        public int FrameCorrection;
        public bool Fix3v;
        public bool ShinyCharm;

        public int PokeLv;

        public bool Wild, Honey, UB;
        public int Lv_max, Lv_min;
        public int UB_th;
        public bool UB_S = false;
        public bool nogender;
        public int gender_ratio;
        
        public bool Considerdelay;
        public static int delaycorrection = 3;
        public static int delaytime = 93; //For honey 186F =3.1s
        public static int npcnumber = 1;
        public static int[] remain_frame;
        public static bool[] blink_flag;

        public class RNGResult
        {
            public int Nature;
            public int Clock;
            public uint PID, EC, PSV;
            public UInt64 row_r;
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
            public int Item = -1;
        }

        public RNGResult Generate()
        {
            RNGResult st = new RNGResult();
            index = 0;

            //Synchronize
            st.row_r = Rand[0];
            st.Clock = (int)(st.row_r % 17);
            st.Blink = ((int)(st.row_r & 0x7F)) > 0 ? 0 : 1;
            
            if (Considerdelay)
                st.frameshift = getframeshift();

            if (Wild && UB && Honey)
                st.UbValue = getUBValue();

            if (Wild && !Honey)
                st.Encounter = (int)(getrand() % 100);
            else
                st.Encounter = -1;

            if (UB_S)
                st.Synchronize = blink_process(10);
            if (Wild && !UB_S)
                st.Synchronize = (int)(getrand() % 100) >= 50;
            if (!Wild)
                st.Synchronize = blink_process(FrameCorrection);
            if (AlwaysSynchro)
                st.Synchronize = true;

            if (Wild && UB && !Honey)
                st.UbValue = getUBValue();

            //UB is determined above
            bool lengendary = UB_S || !Wild;
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
            Advance(60);

            //Encryption Constant
            st.EC = (uint)(getrand() & 0xFFFFFFFF);

            //PID
            int roll_count = ShinyCharm ? 3 : 1;
            if (lengendary) roll_count = 1;
            for (int i = 0; i < roll_count; i++) //pid
            {
                st.PID = (uint)(getrand() & 0xFFFFFFFF);
                st.PSV = ((st.PID >> 16) ^ (st.PID & 0xFFFF)) >> 4;
                if (st.PSV == TSV)
                {
                    st.Shiny = true;
                    break;
                }
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

        public static List<ulong> Rand;
        private static int index;
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

        // 1/30s elapsed
        private static void time_elapse()
        {
            for (int i = 0; i < npcnumber; i++)
            {
                if (remain_frame[i] > 0)
                    remain_frame[i]--;

                if (remain_frame[i] == 0)
                {
                    //Blinking
                    if (blink_flag[i])
                    {
                        if ((int)(getrand() % 3) == 0)
                            remain_frame[i] = 36;
                        else
                            remain_frame[i] = 30;
                        blink_flag[i] = false;
                    }
                    //Not Blinking
                    else
                    {
                        if ((int)(getrand() & 0x7F) == 0)
                        {
                            remain_frame[i] = 5;
                            blink_flag[i] = true;
                        }
                    }
                }
            }
        }

        private static int getframeshift()
        {
            index = delaycorrection;
            remain_frame = new int[npcnumber];
            blink_flag = new bool[npcnumber];
            for (int totalframe = 0; totalframe < delaytime; totalframe++)
                time_elapse();
            return index;
        }

        private bool blink_process(int t_total)
        {
            //for sync
            bool tmp = false;
            if (!Considerdelay)
            {
                //Reset NPC Status
                remain_frame = new int[npcnumber];
                blink_flag = new bool[npcnumber];
            }
            for (int totalframe = 1; totalframe <= t_total; totalframe++)
            {
                time_elapse();
                // 3/30s before finishing
                if (totalframe == t_total - 3) 
                    tmp = (int)(getrand() % 100) >= 50;
            }
            return tmp;
        }

    }
}
