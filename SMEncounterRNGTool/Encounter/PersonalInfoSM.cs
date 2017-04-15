using System;

namespace PKHeX.Core
{
    public class PersonalInfoSM : PersonalInfo
    {
        public const int SIZE = 0x54;
        public PersonalInfoSM(byte[] data)
        {
            if (data.Length != SIZE)
                return;
            Data = data;
        }
        public override byte[] Write()
        {
            return Data;
        }
        
        // No accessing for 3C-4B

        public int SpecialZ_Item { get { return BitConverter.ToUInt16(Data, 0x4C); } set { BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x4C); } }
        public int SpecialZ_BaseMove { get { return BitConverter.ToUInt16(Data, 0x4E); } set { BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x4E); } }
        public int SpecialZ_ZMove { get { return BitConverter.ToUInt16(Data, 0x50); } set { BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x50); } }
        public bool LocalVariant { get { return Data[0x52] == 1; } set { Data[0x52] = (byte)(value ? 1 : 0); } }

        public override int HP { get { return Data[0x00]; } set { Data[0x00] = (byte)value; } }
        public override int ATK { get { return Data[0x01]; } set { Data[0x01] = (byte)value; } }
        public override int DEF { get { return Data[0x02]; } set { Data[0x02] = (byte)value; } }
        public override int SPE { get { return Data[0x03]; } set { Data[0x03] = (byte)value; } }
        public override int SPA { get { return Data[0x04]; } set { Data[0x04] = (byte)value; } }
        public override int SPD { get { return Data[0x05]; } set { Data[0x05] = (byte)value; } }
        public override int[] Types
        {
            get { return new int[] { Data[0x06], Data[0x07] }; }
            set
            {
                if (value?.Length != 2) return;
                Data[0x06] = (byte)value[0];
                Data[0x07] = (byte)value[1];
            }
        }
        public override int CatchRate { get { return Data[0x08]; } set { Data[0x08] = (byte)value; } }
        private int EVYield { get { return BitConverter.ToUInt16(Data, 0x0A); } set { BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x0A); } }
        public override int[] Items
        {
            get { return new int[] { BitConverter.ToInt16(Data, 0xC), BitConverter.ToInt16(Data, 0xE), BitConverter.ToInt16(Data, 0x10) }; }
            set
            {
                if (value?.Length != 3) return;
                BitConverter.GetBytes((short)value[0]).CopyTo(Data, 0xC);
                BitConverter.GetBytes((short)value[1]).CopyTo(Data, 0xE);
                BitConverter.GetBytes((short)value[2]).CopyTo(Data, 0x10);
            }
        }
        public override int Gender { get { return Data[0x12]; } set { Data[0x12] = (byte)value; } }
        public override int[] Abilities
        {
            get { return new int[] { Data[0x18], Data[0x19], Data[0x1A] }; }
            set
            {
                if (value?.Length != 3) return;
                Data[0x18] = (byte)value[0];
                Data[0x19] = (byte)value[1];
                Data[0x1A] = (byte)value[2];
            }
        }
        protected internal override int FormStatsIndex { get { return BitConverter.ToUInt16(Data, 0x1C); } set { BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x1C); } }
        public override int FormeCount { get { return Data[0x20]; } set { Data[0x20] = (byte)value; } }
    }
}
