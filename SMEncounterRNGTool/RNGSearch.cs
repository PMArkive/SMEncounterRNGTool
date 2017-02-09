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
        public bool Sync = false;
        public int Lv_max, Lv_min;
        public int UB_th;

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
            public int UbValue = -1;
            public int Slot = -1;
            public int Lv = -1;
            public int Item = -1;
        }

        public RNGResult Generate(SFMT sfmt)
        {
            RNGResult st = new RNGResult();

            SFMT tmp = (SFMT)sfmt.DeepCopy();
            //シンクロ -- Synchronize
            st.row_r = tmp.NextUInt64();
            st.Clock = (int)(st.row_r % 17);
            st.Blink = ((int)(st.row_r & 0x7F)) > 0 ? 0 : 1;

            if (Sync && (!Honey || !UB))
                st.Synchronize = (int)(sfmt.NextUInt64() % 100) >= 50;
            if (AlwaysSynchro)
                st.Synchronize = true;

            if (!Honey && Wild)
                st.Encounter = (int)(sfmt.NextUInt64() % 100);
            if (Honey)
                st.Encounter = -1;

            if (UB)
            {
                st.UbValue = (int)(sfmt.NextUInt64() % 100);
                UB_S = st.UbValue < UB_th;
                Fix3v = UB_S;
            }

            if (Sync && Honey && UB)
                st.Synchronize = (int)(sfmt.NextUInt64() % 100) >= 50;

            if (Wild && !UB_S)
            {
                st.Slot = getslot((int)(sfmt.NextUInt64() % 100));
                st.Lv = (int)(sfmt.NextUInt64() % (ulong)(Lv_max - Lv_min + 1)) + Lv_min;
                st.Item = (int)(sfmt.NextUInt64() % 60);
            }

            for (int i = 0; i < FrameCorrection; i++)
                sfmt.NextUInt64();

            //謎の消費 -- Something
            for (int i = 0; i < 60; i++)
                sfmt.NextUInt64();

            //暗号化定数 -- Encryption Constant
            st.EC = (uint)(sfmt.NextUInt64() & 0xFFFFFFFF);

            //性格値 -- PID
            int roll_count = ShinyCharm ? 3 : 1;
            for (int i = 0; i < roll_count; i++) //pid
            {
                st.PID = (uint)(sfmt.NextUInt64() & 0xFFFFFFFF);
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
                int ran = (int)(sfmt.NextUInt64() % 6);
                if (IV[ran] != 32)
                {
                    IV[ran] = 32;
                    cnt--;
                }
            }

            for (int i = 0; i < 6; i++) //IV
            {
                if (IV[i] == 32)
                {
                    IV[i] = 31;
                }
                else
                {
                    IV[i] = (int)(sfmt.NextUInt64() & 0x1F);
                }
            }
            st.IVs = (int[])IV.Clone();

            //Something
            if (AlwaysSynchro)
                sfmt.NextUInt64();

            //Ability
            if (!Fix3v)
                st.Ability = (int)(sfmt.NextUInt64() & 1) + 1;

            //Nature
            if (!st.Synchronize)
                st.Nature = (int)(sfmt.NextUInt64() % 25);
            else
            {
                if (Synchro_Stat >= 0)
                    st.Nature = Synchro_Stat;
                else
                {
                    tmp = (SFMT)sfmt.DeepCopy();
                    st.Nature = (int)(tmp.NextUInt64() % 25);
                }
            }

            //Gender
            if (nogender || UB_S)
                st.Gender = 0;
            else
                st.Gender = ((int)(sfmt.NextUInt64() % 252) >= gender_ratio) ? 1 : 2;

            return st;
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
