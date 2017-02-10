using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMEncounterRNGTool
{
    class RNGSearch
    {
        // Search Settings
        public int TSV;
        public int gender_ratio;
        public bool nogender;
        public bool AlwaysSynchro;
        public int Synchro_Stat;
        public int FrameCorrection;
        public bool Fix3v;
        public bool ShinyCharm;
        public bool Honey, UB, UB_S, Wild;
        public bool Sync;
        public int Lv_max, Lv_min;
        public int UB_th;
        public static List<ulong> Rand;

        private int index;


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

            public int Encounter = -1;
            public int Gender;
            public int Ability = -1;
            public int UbValue = 100;
            public int Slot = -1;
            public int Lv = -1;
            public int Item = -1;
        }

        public RNGResult Generate()
        {
            RNGResult st = new RNGResult();

            index = 0;

            //シンクロ -- Synchronize
            st.row_r = currentrand();
            st.Clock = (int)(st.row_r % 17);
            st.Blink = ((int)(st.row_r & 0x7F)) > 0 ? 0 : 1;

            if (Sync && (!Honey || !UB))
                st.Synchronize = (int)(getrand() % 100) >= 50;
            if (AlwaysSynchro)
                st.Synchronize = true;

            if (!Honey && Wild)
                st.Encounter = (int)(getrand() % 100);
            if (Honey)
                st.Encounter = -1;

            if (UB)
            {
                st.UbValue = (int)(getrand() % 100);
                UB_S = st.UbValue < UB_th;
                Fix3v = UB_S;
            }

            if (Sync && Honey && UB)
                st.Synchronize = (int)(getrand() % 100) >= 50;

            if (Wild && !UB_S)
            {
                st.Slot = getslot((int)(getrand() % 100));
                st.Lv = (int)(getrand() % (ulong)(Lv_max - Lv_min + 1)) + Lv_min;
                st.Item = (int)(getrand() % 60);
            }

            //Something
                index += 60 + FrameCorrection;

            //Encryption Constant
            st.EC = (uint)(getrand() & 0xFFFFFFFF);

            //PID
            int roll_count = ShinyCharm ? 3 : 1;
            if (UB_S) roll_count = 1;
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
            int[] IV = new int[6] { 0, 0, 0, 0, 0, 0 };

            int cnt = Fix3v ? 3 : 0;
            while (cnt > 0)
            {
                int ran = (int)(getrand() % 6);
                if (IV[ran] != 32)
                {
                    IV[ran] = 32;
                    cnt--;
                }
            }

            for (int i = 0; i < 6; i++) //IV
            {
                if (IV[i] == 32)
                    IV[i] = 31;
                else
                    IV[i] = (int)(getrand() & 0x1F);
            }
            st.IVs = (int[])IV.Clone();

            //Something
            if (AlwaysSynchro)
                index++;

            //Ability
            if (!Fix3v)
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

            return st;
        }

        private ulong getrand()
        {
            return Rand[index++];
        }

        private ulong currentrand()
        {
            return Rand[index];
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
    }
}
