﻿namespace SMEncounterRNGTool
{
    public class RNGResult
    {
        public byte Nature;
        public byte Clock;
        public uint PID, EC;
        public uint PSV => ((PID >> 16) ^ (PID & 0xFFFF)) >> 4;
        public ulong row_r;
        public int[] IVs;
        public int[] Stats;
        public bool Shiny;
        public bool Synchronize;
        public byte Blink;
        public int frameshift;

        public int Encounter = -1;
        public byte Gender;
        public byte Ability;
        public byte UbValue = 100;
        public byte Slot;
        public byte Lv;
        public byte Item;

        public int realtime = -1;
    }
}
