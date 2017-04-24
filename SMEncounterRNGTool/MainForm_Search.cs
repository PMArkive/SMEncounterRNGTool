using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static PKHeX.Util;

namespace SMEncounterRNGTool
{
    public partial class MainForm : Form
    {
        private void CalcList_Click(object sender, EventArgs e)
        {
            if (IsEvent && NatureLocked.Checked && Event_Nature.SelectedIndex == 0)
                Event_Nature.SelectedIndex = 1;
            if (ivmin0.Value > ivmax0.Value)
                Error(SETTINGERROR_STR[lindex] + L_H.Text);
            else if (ivmin1.Value > ivmax1.Value)
                Error(SETTINGERROR_STR[lindex] + L_A.Text);
            else if (ivmin2.Value > ivmax2.Value)
                Error(SETTINGERROR_STR[lindex] + L_B.Text);
            else if (ivmin3.Value > ivmax3.Value)
                Error(SETTINGERROR_STR[lindex] + L_C.Text);
            else if (ivmin4.Value > ivmax4.Value)
                Error(SETTINGERROR_STR[lindex] + L_D.Text);
            else if (ivmin5.Value > ivmax5.Value)
                Error(SETTINGERROR_STR[lindex] + L_S.Text);
            else if (CreateTimeline.Checked)
                createtimeline();
            else if (Frame_min.Value > Frame_max.Value)
                Error(SETTINGERROR_STR[lindex] + L_frame.Text);
            else if (Refinement.Checked)
                ConsiderHistoryStationarySearch();
            else
                StationarySearch();
        }

        private void getblinkflaglist(int min, int max, SFMT sfmt)
        {
            blinkflaglist = new byte[max - min + 2];
            SFMT st = (SFMT)sfmt.DeepCopy();
            if (ModelNumber == 1)
                MarkNoNPCFlag(st, min, max);
            else
                MarkMultipleNPCFlag(st, min, max);
        }

        private void MarkNoNPCFlag(SFMT st, int min, int max)
        {
            int blink_flag = 0;
            ulong rand;
            for (int i = 0; i < min - 2; i++)
                st.NextUInt64();
            if ((int)(st.NextUInt64() & 0x7F) == 0)
                blinkflaglist[0] = (byte)((int)(st.NextUInt64() % 3) == 0 ? 36 : 30);
            else if ((int)(st.NextUInt64() & 0x7F) == 0)
                blink_flag = 1;
            for (int i = min; i <= max; i++)
            {
                rand = st.NextUInt64();
                if (blink_flag == 1)
                {
                    blinkflaglist[i - min] = 5;
                    blinkflaglist[++i - min] = (byte)((int)(rand % 3) == 0 ? 36 : 30);
                    blink_flag = 0; st.NextUInt64(); // Reset and advance
                }
                if ((int)(rand & 0x7F) == 0)
                    blink_flag = blinkflaglist[i - min] = 1;
            }
        }

        private void MarkMultipleNPCFlag(SFMT st, int min, int max)
        {
            int Model_n = ModelNumber;
            int blink_flag = 0;
            int[] Unsaferange = new[] { 35 * (Model_n - 1), 41 * (Model_n - 1) };
            List<ulong> Randlist = new List<ulong>();
            int Min = Math.Max(min - Unsaferange[1], 418);
            for (int i = 0; i < Min; i++)
                st.NextUInt64();
            for (int i = 0; i <= (Model_n - 1) * 5 + 1; i++) // Create Buffer for checkafter
                Randlist.Add(st.NextUInt64());
            for (int i = Min; i <= max; i++, Randlist.RemoveAt(0), Randlist.Add(st.NextUInt64()))
            {
                if ((Randlist[0] & 0x7F) == 0)
                {
                    blink_flag = Unsaferange[blink_flag == 0 ? Checkafter(Randlist) : 1];
                    if (i >= min) blinkflaglist[i - min] = (byte)(blink_flag == 0 ? 1 : 3);
                    continue;
                }
                if (blink_flag > 0)
                {
                    blink_flag--;
                    if (i >= min) blinkflaglist[i - min] = 2;
                }
            }
        }

        private byte Checkafter(List<ulong> Randlist)
        {
            if (Randlist.Skip(1).Take(Randlist.Count - 2).Any(r => (r & 0x7F) == 0))
                return 1;
            if (Randlist.Last() % 3 == 0) return 1;
            return 0;
        }



        private EventRule geteventsetting()
        {
            int[] IVs = new[] { -1, -1, -1, -1, -1, -1 };
            for (int i = 0; i < 6; i++)
                if (EventIVLocked[i].Checked)
                    IVs[i] = (int)EventIV[i].Value;
            if (IVsCount.Value > 0 && IVs.Count(iv => iv >= 0) + IVsCount.Value > 5)
            {
                Error(SETTINGERROR_STR[lindex] + L_IVsCount.Text);
                IVs = new[] { -1, -1, -1, -1, -1, -1 };
            }
            EventRule e = new EventRule
            {
                IVs = (int[])IVs.Clone(),
                IVsCount = (byte)IVsCount.Value,
                YourID = YourID.Checked,
                PIDType = (byte)Event_PIDType.SelectedIndex,
                AbilityLocked = AbilityLocked.Checked,
                NatureLocked = NatureLocked.Checked,
                GenderLocked = GenderLocked.Checked,
                OtherInfo = OtherInfo.Checked,
                EC = (uint)Event_EC.Value,
                Ability = (byte)Event_Ability.SelectedIndex,
                Nature = (byte)(Event_Nature.SelectedIndex - 1),
                Gender = (byte)Event_Gender.SelectedIndex,
                IsEgg = IsEgg.Checked
            };
            if (e.YourID)
                e.TSV = (uint)TSV.Value;
            else
            {
                e.TID = (int)Event_TID.Value;
                e.SID = (int)Event_SID.Value;
                e.TSV = (uint)(e.TID ^ e.SID) >> 4;
                e.PID = (uint)Event_PID.Value;
            }
            return e;
        }

        private Filters FilterSettings => new Filters
        {
            Nature = Nature.CheckBoxItems.Skip(1).Select(e => e.Checked).ToArray(),
            HPType = HiddenPower.CheckBoxItems.Skip(1).Select(e => e.Checked).ToArray(),
            Gender = Gender.SelectedIndex,
            Ability = Ability.SelectedIndex,
            Slot = Slot.CheckBoxItems.Select(e => e.Checked).ToArray(),
            IVlow = IVlow,
            IVup = IVup,
            BS = ByStats.Checked ? BS : null,
            Stats = ByStats.Checked ? Stats : null,
            Wild = Wild.Checked,
            Skip = DisableFilters.Checked,
            Lv = (byte)Filter_Lv.Value,
            PerfectIVs = (byte)PerfectIVs.Value,
            SafeFOnly = SafeFOnly.Checked,
            BlinkFOnly = BlinkOnly.Checked,
            ShinyOnly = ShinyOnly.Checked,
            EncounterOnly = EncounteredOnly.Checked,
            Encounter_th = (byte)Encounter_th.Value,
            UBOnly = UBOnly.Checked,
            UB_th = (byte)Special_th.Value,
            MainRNGEgg = MainRNGEgg.Checked,
        };

        private void RefreshRNGSettings(SFMT sfmt)
        {
            RNGSetting.Considerhistory = CreateTimeline.Checked || Refinement.Checked;
            RNGSetting.Considerdelay = ShowResultsAfterDelay.Checked;
            RNGSetting.PreDelayCorrection = (int)Correction.Value;
            RNGSetting.delaytime = (int)Timedelay.Value / 2;
            RNGSetting.modelnumber = ModelNumber;
            RNGSetting.Wild = Wild.Checked;
            RNGSetting.ConsiderBagEnteringTime = EnterBagTime.Checked;
            RNGSetting.IsSolgaleo = PK.IsSolgaleo;
            RNGSetting.IsLunala = PK.IsLunala;
            RNGSetting.SolLunaReset = (RNGSetting.IsSolgaleo || RNGSetting.IsLunala) && ModelNumber == 7;
            RNGSetting.IsExeggutor = PK.IsExeggutor;
            RNGSetting.Synchro_Stat = (byte)(SyncNature.SelectedIndex - 1);
            RNGSetting.TSV = (int)TSV.Value;
            RNGSetting.AlwaysSynchro = AlwaysSynced.Checked;
            RNGSetting.Honey = Honey.Checked;
            RNGSetting.UB = PK.UB;
            RNGSetting.QR = PK.QR;
            RNGSetting.ShinyCharm = ShinyCharm.Checked;
            RNGSetting.Fix3v = Fix3v.Checked;
            RNGSetting.randomgender = FuncUtil.IsRandomGender((int)GenderRatio.SelectedValue);
            RNGSetting.gender = FuncUtil.getGenderRatio((int)GenderRatio.SelectedValue);
            RNGSetting.PokeLv = PK.Level;
            RNGSetting.Lv_min = (byte)Lv_min.Value;
            RNGSetting.Lv_max = (byte)Lv_max.Value;
            RNGSetting.Rate = (byte)Special_th.Value;
            RNGSetting.Encounter_th = (byte)Encounter_th.Value;
            RNGSetting.ShinyLocked = PK.ShinyLocked;

            RNGSetting.fishing = Fishing.Checked;
            RNGSetting.SOS = SOS.Checked;
            RNGSetting.ChainLength = (byte)ChainLength.Value;

            RNGSetting.route17 = ea.Location == 120;
            RNGSetting.IsMainRNGEgg = MainRNGEgg.Checked;
            RNGSetting.IsMinior = PK.IsBlank && Wild.Checked && (int)SlotSpecies.SelectedValue == 774;

            RNGSetting.ResetModelStatus();
            RNGSetting.CreateBuffer(sfmt, ConsiderDelay.Checked);
        }

        private void Prepare(SFMT sfmt)
        {
            dgvrowlist.Clear();
            DGV.Rows.Clear();
            status = new ModelStatus(ModelNumber, sfmt);
            if (ea.Location == 120) // Route 17
                status.route17 = true;
            setting = FilterSettings;
            rng = new RNGSetting();
            e = IsEvent ? geteventsetting() : null;
            RefreshRNGSettings(sfmt);
        }

        private void MarkResults(RNGResult result, int blinkidx = -1, int realtime = -1)
        {
            // Mark realtime
            if (realtime > -1)
                result.realtime = realtime;
            // Mark Blink
            if (0 <= blinkidx && blinkidx < blinkflaglist.Length)
                result.Blink = blinkflaglist[blinkidx];
        }

        private static readonly string[] blinkmarks = { "-", "★", "?", "? ★" };

        private DataGridViewRow getRow(int i, RNGResult result)
        {
            int d = i - (int)Time_max.Value;
            string true_nature = StringItem.naturestr[result.Nature];
            string SynchronizeFlag = result.Synchronize ? "O" : "X";
            if (result.SpecialEnctrValue < Special_th.Value && ShowDelay)
                result.Blink = 2;
            string BlinkFlag = result.Blink < 4 ? blinkmarks[result.Blink] : result.Blink.ToString();
            string PSV = result.PSV.ToString("D4");
            string Encounter = result.Encounter == -1 ? "-" : result.Encounter.ToString();
            string Slot = result.Slot == 0 ? "-" : result.Slot.ToString();
            string Lv = result.Lv == 0 ? "-" : result.Lv.ToString();
            string Item = result.Item == 100 ? "-" : result.Item.ToString();
            string UbValue = result.SpecialEnctrValue == 100 ? "-" : result.SpecialEnctrValue.ToString();
            string randstr = result.row_r.ToString("X16");
            string PID = result.PID.ToString("X8");
            string EC = result.EC.ToString("X8");
            string time = CreateTimeline.Checked || Refinement.Checked ? (2 * result.realtime).ToString() + "F" : "-";

            if (!Advanced.Checked)
            {
                if (Encounter != "-")
                    Encounter = result.Encounter < Encounter_th.Value ? "O" : "X";
                if (UbValue != "-")
                    UbValue = result.SpecialEnctrValue < Special_th.Value ? "O" : "X";
                if (UbValue == "O")
                    Slot = PK.QR ? "QR" : "UB";
                if (result.Item < 50)
                    Item = "50%";
                else if (result.Item < 55)
                    Item = "5%";
                else
                    Item = "-";
                time = CreateTimeline.Checked || Refinement.Checked ? (result.realtime / 30.0).ToString("F") + " s" : "-";
            }

            if (IsEvent && !OtherInfo.Checked && e.PIDType > 1)
                PID = PSV = "-";

            string frameadvance = result.frameshift.ToString("+#;-#;0");
            int[] Status = ShowStats.Checked ? result.Stats : result.IVs;

            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(DGV);

            string research = (result.row_r % 6).ToString() + " " + (result.row_r % 32).ToString("D2") + " " + (result.row_r % 100).ToString("D2") + " " + (result.row_r % 252).ToString("D3");

            row.SetValues(
                i, d.ToString("+#;-#;0"), BlinkFlag,
                Status[0], Status[1], Status[2], Status[3], Status[4], Status[5],
                true_nature, SynchronizeFlag, StringItem.hpstr[result.hiddenpower + 1], result.Clock, PSV, frameadvance, UbValue, Slot, Lv, StringItem.genderstr[result.Gender], StringItem.abilitystr[result.Ability], Item, Encounter,
                randstr, PID, EC, time, research
                );

            if (result.Shiny)
                row.DefaultCellStyle.BackColor = Color.LightCyan;

            row.Cells[24].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            Font BoldFont = new Font("Microsoft Sans Serif", 8, FontStyle.Bold);
            for (int k = 0; k < 6; k++)
            {
                if (result.IVs[k] < 1)
                {
                    row.Cells[3 + k].Style.Font = BoldFont;
                    row.Cells[3 + k].Style.ForeColor = Color.OrangeRed;
                }
                else if (result.IVs[k] > 29)
                {
                    row.Cells[3 + k].Style.Font = BoldFont;
                    row.Cells[3 + k].Style.ForeColor = Color.MediumSeaGreen;
                }
            }
            return row;
        }

        private void StationarySearch()
        {
            // Blinkflag
            SFMT sfmt = new SFMT((uint)Seed.Value);
            int max, min;
            if (AroundTarget.Checked)
            {
                min = (int)Time_max.Value - 100; max = (int)Time_max.Value + 100;
            }
            else
            {
                min = (int)Frame_min.Value; max = (int)Frame_max.Value;
            }
            getblinkflaglist(min, max, sfmt);
            // Advance
            for (int i = 0; i < min; i++)
                sfmt.NextUInt64();
            // Prepare
            Prepare(sfmt);
            bool HasStageFrame = ModelNumber == 1;
            // Start
            for (int i = min; i <= max; i++, RNGSetting.Rand.RemoveAt(0), RNGSetting.Rand.Add(sfmt.NextUInt64()))
            {
                if (HasStageFrame)
                    RNGSetting.StageFrame = blinkflaglist[i - min] >= 5 ? blinkflaglist[i - min] : (byte)0;
                RNGResult result = rng.Generate(e);
                MarkResults(result, i - min);
                if (!setting.CheckResult(result))
                    continue;
                dgvrowlist.Add(getRow(i, result));
                if (dgvrowlist.Count > 100000) break;
            }
            DGV.Rows.AddRange(dgvrowlist.ToArray());
            DGV.CurrentCell = null;
        }

        private void ConsiderHistoryStationarySearch()
        {
            // Blinkflag
            SFMT sfmt = new SFMT((uint)Seed.Value);
            int max, min;
            if (AroundTarget.Checked)
            {
                min = (int)Time_max.Value - 100; max = (int)Time_max.Value + 100;
            }
            else
            {
                min = (int)Frame_min.Value; max = (int)Frame_max.Value;
            }
            int StartFrame = (int)Frame_min.Value;
            getblinkflaglist(min, max, sfmt);
            // Advance
            for (int i = 0; i < StartFrame; i++)
                sfmt.NextUInt64();
            // Prepare
            Prepare(sfmt);
            int frameadvance;
            int[] remain_frame;
            int realtime = 0;
            bool phase;
            // Start
            for (int i = StartFrame; i <= max;)
            {
                remain_frame = (int[])status.remain_frame.Clone();
                phase = status.phase;
                frameadvance = status.NextState();

                while (frameadvance > 0)
                {
                    RNGSetting.remain_frame = (int[])remain_frame.Clone();
                    RNGSetting.phase = phase;
                    RNGResult result = rng.Generate(e);
                    RNGSetting.Rand.RemoveAt(0);
                    RNGSetting.Rand.Add(sfmt.NextUInt64());
                    frameadvance--;
                    i++;
                    if (i <= min || i > max)
                        continue;
                    MarkResults(result, i - min - 1, realtime);
                    if (!setting.CheckResult(result))
                        continue;
                    dgvrowlist.Add(getRow(i - 1, result));
                }
                realtime++;
                if (dgvrowlist.Count > 100000) break;
            }
            DGV.Rows.AddRange(dgvrowlist.ToArray());
            DGV.CurrentCell = null;
        }

        private void createtimeline()
        {
            // Blinkflag
            SFMT sfmt = new SFMT((uint)Seed.Value);
            int start_frame = (int)Frame_min.Value;
            getblinkflaglist(start_frame, start_frame, sfmt);
            // Advance
            for (int i = 0; i < start_frame; i++)
                sfmt.NextUInt64();
            // Prepare
            Prepare(sfmt);
            int totaltime = (int)TimeSpan.Value * 30;
            int frame = (int)Frame_min.Value;
            int frameadvance, Currentframe;
            // Start
            for (int i = 0; i <= totaltime; i++)
            {
                Currentframe = frame;
                RNGSetting.remain_frame = (int[])status.remain_frame.Clone();
                RNGSetting.phase = status.phase;

                RNGResult result = rng.Generate(e);
                MarkResults(result, i, i);

                frameadvance = status.NextState();
                frame += frameadvance;
                for (int j = 0; j < frameadvance; j++)
                {
                    RNGSetting.Rand.RemoveAt(0);
                    RNGSetting.Rand.Add(sfmt.NextUInt64());
                }

                if (!setting.CheckResult(result))
                    continue;
                dgvrowlist.Add(getRow(Currentframe, result));
                if (dgvrowlist.Count > 100000) break;
            }
            DGV.Rows.AddRange(dgvrowlist.ToArray());
            DGV.CurrentCell = null;
        }
    }
}
