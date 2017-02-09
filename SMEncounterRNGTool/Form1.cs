using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static PKHeX.Util;

namespace SMEncounterRNGTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        #region HexNumericFunction
        private void NumericUpDown_Enter(object sender, EventArgs e)
        {
            NumericUpDown NumericUpDown = sender as NumericUpDown;
            NumericUpDown.Select(0, NumericUpDown.Text.Length);
        }

        private void NumericUpDown_Check(object sender, CancelEventArgs e)
        {
            NumericUpDown NumericUpDown = sender as NumericUpDown;
            Control ctrl = NumericUpDown;
            if (ctrl == null)
                return;
            if (!string.IsNullOrEmpty(NumericUpDown.Text))
                return;
            foreach (var box in ((NumericUpDown)ctrl).Controls.OfType<TextBox>())
            {
                // クリップボードへコピー
                box.Undo();
                break;
            }
        }
        #endregion

        List<NumericUpDown> IVlow = new List<NumericUpDown>();
        List<NumericUpDown> IVup = new List<NumericUpDown>();
        List<NumericUpDown> BS = new List<NumericUpDown>();
        List<NumericUpDown> Stat = new List<NumericUpDown>();


        private void Form1_Load(object sender, EventArgs e)
        {
            Type dgvtype = typeof(DataGridView);
            System.Reflection.PropertyInfo dgvPropertyInfo = dgvtype.GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            dgvPropertyInfo.SetValue(DGV, true, null);

            foreach (string t in SearchSetting.hpstr)
                HiddenPower.Items.Add(t);

            IVlow.Add(ivmin0);
            IVlow.Add(ivmin1);
            IVlow.Add(ivmin2);
            IVlow.Add(ivmin3);
            IVlow.Add(ivmin4);
            IVlow.Add(ivmin5);

            IVup.Add(ivmax0);
            IVup.Add(ivmax1);
            IVup.Add(ivmax2);
            IVup.Add(ivmax3);
            IVup.Add(ivmax4);
            IVup.Add(ivmax5);

            BS.Add(BS_0);
            BS.Add(BS_1);
            BS.Add(BS_2);
            BS.Add(BS_3);
            BS.Add(BS_4);
            BS.Add(BS_5);

            Stat.Add(Stat0);
            Stat.Add(Stat1);
            Stat.Add(Stat2);
            Stat.Add(Stat3);
            Stat.Add(Stat4);
            Stat.Add(Stat5);

            Nature.Items.Add("-");
            SyncNature.Items.Add("-");
            foreach (string t in SearchSetting.naturestr)
            {
                Nature.Items.Add(t);
                SyncNature.Items.Add(t);
            }

            foreach (string t in SearchSetting.genderstr)
                Gender.Items.Add(t);

            for (int i = 0; i < SearchSetting.pokedex.GetLength(0); i++)
            {
                Poke.Items.Add(SearchSetting.pokedex[i, 0]);
            }

            HiddenPower.SelectedIndex = 0;
            Nature.SelectedIndex = 0;
            SyncNature.SelectedIndex = 0;
            Gender.SelectedIndex = 0;
            Ability.SelectedIndex = 0;
            Slot.SelectedIndex = 0;

            Seed.Value = Properties.Settings.Default.Seed;
            ShinyCharm.Checked = Properties.Settings.Default.ShinyCharm;
            TSV.Value = Properties.Settings.Default.TSV;
            Advanced.Checked = Properties.Settings.Default.Advance;

            if (Properties.Settings.Default.ClockInput)
                StartClockInput.Checked = true;
            else
                EndClockInput.Checked = true;

            if (Properties.Settings.Default.Method)
                Stationary.Checked = true;
            else
                Wild.Checked = true;
            ByIVs.Checked = true;

            Advanced_CheckedChanged(null, null);
            Method_CheckedChanged(null, null);
            SearchMethod_CheckedChanged(null, null);
        }

        #region SearchSeedfunction
        private void Clear_Click(object sender, EventArgs e)
        {
            Clock_List.Text = "";
        }

        private void Back_Click(object sender, EventArgs e)
        {
            string str = Clock_List.Text;
            if (Clock_List.Text != "")
            {
                if (str.LastIndexOf(",") != -1)
                {
                    str = str.Remove(str.LastIndexOf(","));
                }
                else
                {
                    str = "";
                }
            }
            Clock_List.Text = str;
        }

        private void Get_Clock_Number(object sender, EventArgs e)
        {
            string str = ((Button)sender).Name;
            string number = str.Remove(0, str.IndexOf("button") + 6);

            if (Clock_List.Text == "")
            {
                Clock_List.Text += Convert_Clock(number);
            }
            else
            {
                Clock_List.Text += "," + Convert_Clock(number);
            }
            SearchforSeed(null, null);
        }

        private string Convert_Clock(string n)
        {
            int tmp = Convert.ToInt32(n);
            if (EndClockInput.Checked)
            {
                if (tmp >= 4)
                {
                    tmp -= 4;
                }
                else
                {
                    tmp += 13;
                }
                n = tmp.ToString();
            }
            return n;
        }


        private void SearchforSeed(object sender, EventArgs e)
        {
            var needle = Clock_List.Text.Split(',');
            if (Clock_List.Text.Where(c => c == ',').Count() >= 7)
            {
                var text = "";
                try
                {
                    SeedResults.Text = "请稍后";
                    var results = SFMTSeedAPI.request(Clock_List.Text);
                    if (results == null || results.Count() == 0)
                    {
                        text = "未找到";
                    }
                    else
                    {
                        text = string.Join(" ", results.Select(r => r.seed));
                        if (results.Count() == 1)
                        {
                            Time_min.Value = 418 + Clock_List.Text.Where(c => c == ',').Count();
                            uint s0;
                            if (uint.TryParse(text, NumberStyles.HexNumber, null, out s0))
                                Seed.Value = s0;
                        }
                    }
                }
                catch (Exception exc)
                {
                    text = exc.Message;
                }
                finally
                {
                    SeedResults.Text = text;
                }
            }
            else
                SeedResults.Text = "";
        }
        #endregion


        #region validcheck
        private void TSV_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.TSV = (short)TSV.Value;
            Properties.Settings.Default.Save();
        }

        private void ShinyCharm_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShinyCharm = ShinyCharm.Checked;
            Properties.Settings.Default.Save();
        }

        private void ClockInput_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ClockInput = StartClockInput.Checked;
            Properties.Settings.Default.Save();
        }


        private void Advanced_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Advance = Advanced.Checked;
            Properties.Settings.Default.Save();
            UB_th.Visible = Advanced.Checked;
        }


        private void Seed_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Seed = Seed.Value;
            Properties.Settings.Default.Save();
        }

        private void UB_CheckedChanged(object sender, EventArgs e)
        {
            if (UB.Checked)
            {
                Wild.Checked = true;
                UBOnly.Enabled = true;
            }
            else
            {
                UBOnly.Checked = UBOnly.Enabled = false;
            }
        }

        private void Method_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Method = Stationary.Checked;
            Properties.Settings.Default.Save();
            Fix3v.Enabled = Wild.Checked;
            GenderRatio.Visible = UB.Visible = Honey.Visible = UB_th.Visible = Encounter_th.Visible = Wild.Checked;
            label9.Visible = L_Lv.Visible = L_gender.Visible = L_Ability.Visible = L_Slot.Visible = Wild.Checked;
            Lv_min.Visible = Lv_max.Visible = Slot.Visible = EncounteredOnly.Visible = Gender.Visible = UBOnly.Visible = Ability.Visible = Wild.Checked;
            //AlwaysSynced.Visible = Stationary.Checked;
            if (Stationary.Checked)
            {
                GenderRatio.SelectedIndex = 0;
                UB.Checked = Honey.Checked = false;
                Fix3v.Checked = true;
            }
            else
            {
                GenderRatio.SelectedIndex = 1;
                Fix3v.Checked = false;
                AlwaysSynced.Checked = false;
            }
            UB_CheckedChanged(null, null);
        }

        private void UBOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (UBOnly.Checked)
                EncounteredOnly.Checked = true;
        }

        private void AlwaysSynced_CheckedChanged(object sender, EventArgs e)
        {
            Sync.Checked = AlwaysSynced.Checked;
        }

        private void SyncNature_SelectedIndexChanged(object sender, EventArgs e)
        {
            Sync.Checked = (SyncNature.SelectedIndex > 0);
            if (AlwaysSynced.Checked)
                Nature.SelectedIndex = SyncNature.SelectedIndex;
        }

        private void SearchMethod_CheckedChanged(object sender, EventArgs e)
        {
            IVPanel.Visible = ByIVs.Checked;
            StatPanel.Visible = ByStats.Checked;
        }
        #endregion

        private void Poke_SelectedIndexChanged(object sender, EventArgs e)
        {
            AlwaysSynced.Checked = (Poke.SelectedIndex > 5) && (Poke.SelectedIndex < 10);
            for (int i = 0; i < 6; i++)
            {
                BS[i].Value = Convert.ToInt32(SearchSetting.pokedex[Poke.SelectedIndex, i + 1]);
            }
            switch (Poke.SelectedIndex)
            {
                case 3: NPC.Value = 1; Lv_Search.Value = 60; break; // Tapu Fini
                case 4: NPC.Value = 2; Lv_Search.Value = 55; break; // Solgaleo
                case 5: NPC.Value = 3; Lv_Search.Value = 55; break; // Lunala
                case 6: NPC.Value = 8; Lv_Search.Value = 40; break; // Type:Null
                case 7: NPC.Value = 6; Lv_Search.Value = 50; break; // Magearna sometimes NPC# =7
                case 8: NPC.Value = 3; Lv_Search.Value = 50; break; // Zygarde-10%
                case 9: NPC.Value = 3; Lv_Search.Value = 50; break; // Zygarde-50%
                case 10: NPC.Value = 0; Lv_Search.Value = 55; break; //
                case 11: Lv_Search.Value = 65; break; //
                case 13: Lv_Search.Value = 65; break; //
                case 14: NPC.Value = 0; Lv_Search.Value = 65; break; //
                case 16: Lv_Search.Value = 70; break; //
                case 17: Lv_Search.Value = 75; break; //
                default: NPC.Value = 0; Lv_Search.Value = 60; break;
            }
        }

        private int CalcFrame(int min, int max)
        {
            uint InitialSeed = (uint)Seed.Value;
            int NPC_n = (int)NPC.Value + 1;
            SFMT sfmt = new SFMT(InitialSeed);

            for (int i = 0; i < min; i++)
                sfmt.NextUInt64();

            int n_count = 0;

            int[] remain_frame = new int[NPC_n];
            int total_frame = 0;
            bool[] blink_flag = new bool[NPC_n];


            while (min + n_count < max)
            {
                //NPC Loop
                for (int i = 0; i < NPC_n; i++)
                {
                    if (remain_frame[i] > 0)
                        remain_frame[i]--;

                    if (remain_frame[i] == 0)
                    {
                        //Blinking
                        if (blink_flag[i])
                        {
                            if ((int)(sfmt.NextUInt64() % 3) == 0)
                            {
                                remain_frame[i] = 36;
                            }
                            else
                            {
                                remain_frame[i] = 30;
                            }
                            n_count++;
                            blink_flag[i] = false;
                        }
                        //Not Blinking
                        else
                        {
                            if ((int)(sfmt.NextUInt64() & 0x7F) == 0)
                            {
                                remain_frame[i] = 5;
                                blink_flag[i] = true;
                            }
                            n_count++;
                        }
                    }
                }
                total_frame++;
            }
            return total_frame;
        }

        private void CalcTime_Click(object sender, EventArgs e)
        {

            try
            {
                int totaltime = CalcFrame((int)Time_min.Value, (int)Time_max.Value) - (int)Timedelay.Value / 2;
                float realtime = (float)totaltime / 30;
                TimeResult.Text = $"计时器设置为{totaltime * 2}帧," + realtime.ToString("F") + "秒";
            }
            catch
            {

            }
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            HiddenPower.SelectedIndex = 0;
            if (!AlwaysSynced.Checked) Nature.SelectedIndex = 0;
            Gender.SelectedIndex = 0;
            Ability.SelectedIndex = 0;
            Slot.SelectedIndex = 0;

            Lv_Search.Value = 0;
            for (int i = 0; i < 6; i++)
            {
                IVlow[i].Value = 0;
                IVup[i].Value = 31;
                Stat[i].Value = 0;
            }
        }

        #region Search

        private void CalcList_Click(object sender, EventArgs e)
        {
            if (Frame_min.Value > Frame_max.Value)
                Error("帧数上限小于下限");
            else if (ivmin0.Value > ivmax0.Value)
                Error("HP上限小于下限");
            else if (ivmin1.Value > ivmax1.Value)
                Error("攻击上限小于下限");
            else if (ivmin2.Value > ivmax2.Value)
                Error("防御上限小于下限");
            else if (ivmin3.Value > ivmax3.Value)
                Error("特攻上限小于下限");
            else if (ivmin4.Value > ivmax4.Value)
                Error("特防上限小于下限");
            else if (ivmin5.Value > ivmax5.Value)
                Error("速度上限小于下限");
            else if (0 > TSV.Value || TSV.Value > 4095)
                Error("TSV不正确");
            else if (!Wild.Checked && !Stationary.Checked)
                Error("条件设置不合理");
            else
                StationarySearch();
        }

        private void StationarySearch()
        {
            uint InitialSeed = (uint)Seed.Value;
            int max, min;
            if (AroundTarget.Checked)
            {
                min = (int)Time_max.Value - 100;
                max = (int)Time_max.Value + 100;
            }
            else
            {
                min = (int)Frame_min.Value;
                max = (int)Frame_max.Value;
            }

            SFMT sfmt = new SFMT(InitialSeed);
            SFMT seed = new SFMT(InitialSeed);
            List<DataGridViewRow> list = new List<DataGridViewRow>();
            DGV.Rows.Clear();

            var setting = getSettings();
            var rng = getRNGSettings();

            for (int i = 0; i < min; i++)
                sfmt.NextUInt64();

            int blink_flag = 0;
            for (int i = min; i <= max; i++, sfmt.NextUInt64())
            {
                seed = (SFMT)sfmt.DeepCopy();
                RNGSearch.RNGResult result = rng.Generate(seed);

                switch (blink_flag)
                {
                    case 0:
                        if (result.Blink == 1) blink_flag = 1; break;
                    case 1:
                        blink_flag = (result.row_r % 3) == 0 ? 36 : 30; result.Blink = 5; break;
                    default:
                        result.Blink = blink_flag; blink_flag = 0; break;
                }

                if (!frameMatch(result, setting))
                    continue;


                list.Add(getRow_Sta(i, rng, result, DGV));
            }
            DGV.Rows.AddRange(list.ToArray());
            DGV.CurrentCell = null;
        }

        private SearchSetting getSettings()
        {
            int[] IVup = { (int)ivmax0.Value, (int)ivmax1.Value, (int)ivmax2.Value, (int)ivmax3.Value, (int)ivmax4.Value, (int)ivmax5.Value, };
            int[] IVlow = { (int)ivmin0.Value, (int)ivmin1.Value, (int)ivmin2.Value, (int)ivmin3.Value, (int)ivmin4.Value, (int)ivmin5.Value, };
            int[] BS = { (int)BS_0.Value, (int)BS_1.Value, (int)BS_2.Value, (int)BS_3.Value, (int)BS_4.Value, (int)BS_5.Value, };
            int[] Status = { (int)Stat0.Value, (int)Stat1.Value, (int)Stat2.Value, (int)Stat3.Value, (int)Stat4.Value, (int)Stat5.Value, };

            return new SearchSetting
            {
                Nature = Nature.SelectedIndex - 1,
                HPType = HiddenPower.SelectedIndex - 1,
                Gender = Gender.SelectedIndex,
                Ability = Ability.SelectedIndex,
                Slot = Slot.SelectedIndex,
                IVlow = IVlow,
                IVup = IVup,
                BS = BS,
                Status = Status,
                Skip = DisableFilters.Checked,
                Lv = (int)Lv_Search.Value,
            };
        }

        private RNGSearch getRNGSettings()
        {
            int gender_threshold = 0;
            switch (GenderRatio.SelectedIndex)
            {
                case 1: gender_threshold = 126; break;
                case 2: gender_threshold = 30; break;
                case 3: gender_threshold = 63; break;
                case 4: gender_threshold = 189; break;
            }
            var rng = new RNGSearch
            {
                Synchro_Stat = SyncNature.SelectedIndex - 1,
                TSV = (int)TSV.Value,
                AlwaysSynchro = AlwaysSynced.Checked,
                FrameCorrection = (int)Framecorrection.Value,
                Honey = Honey.Checked,
                UB = UB.Checked,
                ShinyCharm = ShinyCharm.Checked,
                Wild = Wild.Checked,
                Fix3v = Fix3v.Checked,
                gender_ratio = gender_threshold,
                nogender = GenderRatio.SelectedIndex == 0,
                Sync = Sync.Checked,
                Lv_min = (int)Lv_min.Value,
                Lv_max = (int)Lv_max.Value,
                UB_th = (int)UB_th.Value,
            };
            return rng;
        }

        private bool frameMatch(RNGSearch.RNGResult result, SearchSetting setting)
        {
            setting.getStatus(result, setting);

            //ここで弾く
            if (setting.Skip)
                return true;

            if (ShinyOnly.Checked && !result.Shiny)
                return false;

            if (ByIVs.Checked && !setting.validIVs(result.IVs))
                return false;

            if (ByStats.Checked && !setting.validStatus(result, setting))
                return false;

            if (!setting.mezapa_check(result.IVs))
                return false;

            if (Wild.Checked)
            {
                if (setting.Nature != -1 && setting.Nature != result.Nature)
                    return false;

                if (setting.Gender != 0 && setting.Gender != result.Gender)
                    return false;

                if (setting.Ability != 0 && setting.Ability != result.Ability)
                    return false;

                if (setting.Slot != 0 && setting.Slot != result.Slot)
                    return false;

                if (EncounteredOnly.Checked && result.Encounter >= Encounter_th.Value)
                    return false;

                if (result.UbValue >= UB_th.Value && setting.Lv != 0 && setting.Lv != result.Lv)
                    return false;

                if (UBOnly.Checked && result.UbValue >= UB_th.Value)
                    return false;
            }

            return true;
        }

        private DataGridViewRow getRow_Sta(int i, RNGSearch rng, RNGSearch.RNGResult result, DataGridView dgv)
        {
            int d = i - (int)Time_max.Value;
            string true_nature = SearchSetting.naturestr[result.Nature];
            string SynchronizeFlag = (result.Synchronize ? "O" : "X");
            string BlinkFlag = (result.Blink == 1 ? "★" : "-");
            BlinkFlag = result.Blink > 1 ? result.Blink.ToString() : BlinkFlag;
            string Ability = (result.Ability == -1) ? "-" : result.Ability.ToString();
            string Encounter = (result.Encounter == -1) ? "-" : result.Encounter.ToString();
            string Slot = (result.Slot == -1) ? "-" : result.Slot.ToString();
            string Lv = (result.Item == -1) ? "-" : result.Lv.ToString();
            string Item = (result.Item == -1) ? "-" : result.Item.ToString();
            string UbValue = (result.UbValue == 100) ? "-" : result.UbValue.ToString();
            string[] status = new string[6];


            if (!Advanced.Checked)
            {
                Encounter = (result.Encounter < Encounter_th.Value) ? "O" : "X";
                UbValue = result.UbValue < UB_th.Value ? "O" : "X";
            }

            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dgv);

            row.SetValues(
                i, d, BlinkFlag,
                result.IVs[0], result.IVs[1], result.IVs[2], result.IVs[3], result.IVs[4], result.IVs[5],
                true_nature, SynchronizeFlag, SearchSetting.genderstr[result.Gender], Ability, result.Clock, result.PSV, Encounter, Slot, Lv, Item, UbValue, result.row_r.ToString("X16")
                );

            if (result.Shiny)
            {
                row.DefaultCellStyle.BackColor = Color.LightCyan;
            }
            return row;
        }

        #endregion

    }
}
