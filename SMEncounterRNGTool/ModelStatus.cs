namespace SMEncounterRNGTool
{
    class ModelStatus
    {
        public SFMT sfmt;
        public int cnt;
        public byte Modelnumber;
        public int[] remain_frame;
        public ulong getrand { get { cnt++; return sfmt.NextUInt64(); } }

        public ModelStatus()
        {
            remain_frame = new int[Modelnumber];
        }

        public ModelStatus(byte n, SFMT st)
        {
            sfmt = (SFMT)st.DeepCopy();
            Modelnumber = n;
            remain_frame = new int[n];
        }

        public int NextState()
        {
            cnt = 0;
            for (int i = 0; i < Modelnumber; i++)
            {
                if (remain_frame[i] > 1)                       //Cooldown 2nd part
                {
                    remain_frame[i]--;
                    continue;
                }
                if (remain_frame[i] < 0)                       //Cooldown 1st part
                {
                    if (++remain_frame[i] == 0)                //Blinking
                        remain_frame[i] = (int)(getrand % 3) == 0 ? 36 : 30;
                    continue;
                }
                if ((int)(getrand & 0x7F) == 0)                //Not Blinking
                    remain_frame[i] = -5;
            }
            return cnt;
        }

        public void frameshift(int n)
        {
            for (int i = 0; i < n; i++)
                sfmt.NextInt64();
        }
    }
}