using System;
using System.Linq;
using PKHeX.Core;

namespace SMEncounterRNGTool
{
    public class EncounterArea
    {
        public byte Location;
        public EncounterSlot[] Slots;
        public EncounterArea() { }

        private EncounterArea(byte[] data)
        {
            Location = data[0];
            Slots = new EncounterSlot[(data.Length - 2) / 4];
            for (int i = 0; i < Slots.Length; i++)
            {
                ushort SpecForm = BitConverter.ToUInt16(data, 2 + i * 4);
                Slots[i] = new EncounterSlot
                {
                    Species = SpecForm & 0x7FF,
                    Form = SpecForm >> 11,
                    LevelMin = data[4 + i * 4],
                    LevelMax = data[5 + i * 4],
                    SlotNumber = i,
                };
            }
        }

        private static EncounterArea[] getEncounter(byte[] fileData, string identifier)
        {
            if (fileData == null || fileData.Length < 4)
                return null;

            if (identifier[0] != fileData[0] || identifier[1] != fileData[1])
                return null;

            int count = BitConverter.ToUInt16(fileData, 2); int ctr = 4;
            int start = BitConverter.ToInt32(fileData, ctr); ctr += 4;
            byte[][] entries = new byte[count][];
            for (int i = 0; i < count; i++)
            {
                int end = BitConverter.ToInt32(fileData, ctr); ctr += 4;
                int len = end - start;
                byte[] tmpdata = new byte[len];
                Buffer.BlockCopy(fileData, start, tmpdata, 0, len);
                entries[i] = tmpdata;
                start = end;
            }

            if (entries == null)
                return null;

            EncounterArea[] data = new EncounterArea[entries.Length];
            for (int i = 0; i < data.Length; i++)
                data[i] = new EncounterArea(entries[i]);
            return data;
        }

        internal static readonly EncounterArea[] EncounterSun = getEncounter(Properties.Resources.encounter_sn,"sm");
        internal static readonly EncounterArea[] EncounterMoon = getEncounter(Properties.Resources.encounter_mn, "sm");

        internal static readonly byte[] SMLocationList = EncounterSun.Select(e => e.Location).ToArray();
    }
}
