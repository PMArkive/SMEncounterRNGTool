﻿using System;
using PKHeX.Core;

namespace SMEncounterRNGTool
{
    public class PersonalTable
    {
        public static readonly PersonalTable SM = new PersonalTable(Properties.Resources.personal_sm);

        private PersonalTable(byte[] data)
        {
           int size = PersonalInfoSM.SIZE; 
            if (size == 0)
            { Table = null; return; }

            byte[][] entries = splitBytes(data, size);
            PersonalInfo[] d = new PersonalInfo[data.Length / size];
            for (int i = 0; i < d.Length; i++)
                d[i] = new PersonalInfoSM(entries[i]);
            Table = d;
        }

        private static byte[][] splitBytes(byte[] data, int size)
        {
            byte[][] r = new byte[data.Length / size][];
            for (int i = 0; i < data.Length; i += size)
            {
                r[i / size] = new byte[size];
                Array.Copy(data, i, r[i / size], 0, size);
            }
            return r;
        }

        private readonly PersonalInfo[] Table;
        public PersonalInfo this[int index]
        {
            get
            {
                if (index < Table.Length)
                    return Table[index];
                return Table[0];
            }
            set
            {
                if (index < Table.Length)
                    return;
                Table[index] = value; 
            }
        }

        public int[] getAbilities(int species, int forme)
        {
            if (species >= Table.Length)
            { species = 0; Console.WriteLine("Requested out of bounds SpeciesID"); }
            return this[getFormeIndex(species, forme)].Abilities;
        }
        public int getFormeIndex(int species, int forme)
        {
            if (species >= Table.Length)
            { species = 0; Console.WriteLine("Requested out of bounds SpeciesID"); }
            return this[species].FormeIndex(species, forme);
        }
        public PersonalInfo getFormeEntry(int species, int forme)
        {
            return this[getFormeIndex(species, forme)];
        }

        public int TableLength => Table.Length;
    }
}
