namespace SMEncounterRNGTool
{
    class ModelStatus
    {
        public static SFMT smft;
        public static byte Modelnumber;
        public int[] remain_frame;
        public bool[] blink_flag;

        public ModelStatus()
        {
            remain_frame = new int[Modelnumber];
            blink_flag = new bool[Modelnumber];
        }

        public int NextState()
        {
            int cnt = 0;
            for (int i = 0; i < Modelnumber; i++)
            {
                if (remain_frame[i] > 0)
                    remain_frame[i]--;

                if (remain_frame[i] == 0)
                {
                    //Blinking
                    if (blink_flag[i])
                    {
                        remain_frame[i] = (int)(smft.NextUInt64() % 3) == 0 ? 36 : 30;
                        cnt++;
                        blink_flag[i] = false;
                    }
                    //Not Blinking
                    else
                    {
                        if ((int)(smft.NextUInt64() & 0x7F) == 0)
                        {
                            remain_frame[i] = 5;
                            blink_flag[i] = true;
                        }
                        cnt++;
                    }
                }
            }
            return cnt;
        }

        public static void frameshift(int n)
        {
            for (int i = 0; i < n; i++)
                smft.NextInt64();
        }
    }
}