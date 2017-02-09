using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PKHeX
{
    public partial class Util
    {
        #region Form Translation
        
        private static object FindControl(string name, Control.ControlCollection c)
        {
            Control control = c.Find(name, true).FirstOrDefault();
            if (control != null)
                return control;
            foreach (MenuStrip menu in c.OfType<MenuStrip>())
            {
                var item = menu.Items.Find(name, true).FirstOrDefault();
                if (item != null)
                    return item;
            }
            foreach (ContextMenuStrip strip in FindContextMenuStrips(c.OfType<Control>()))
            {
                var item = strip.Items.Find(name, true).FirstOrDefault();
                if (item != null)
                    return item;
            }
            foreach (DataGridView dg in c.OfType<DataGridView>())
            {
                foreach (var col in dg.Columns.OfType<DataGridViewColumn>())
                    if (col.Name == name)
                        return col;
            }
            foreach (Control x in c)
            {
                var z = FindControl(name, x.Controls);
                if (z != null)
                    return z;
            }
            return null;
        }
        private static List<ContextMenuStrip> FindContextMenuStrips(IEnumerable<Control> c)
        {
            List<ContextMenuStrip> cs = new List<ContextMenuStrip>();
            foreach (Control control in c)
            {
                if (control.ContextMenuStrip != null)
                    cs.Add(control.ContextMenuStrip);

                else if (control.Controls.Count > 0)
                    cs.AddRange(FindContextMenuStrips(control.Controls.OfType<Control>()));
            }
            return cs;
        }
        internal static void CenterToForm(Control child, Control parent)
        {
            int x = parent.Location.X + (parent.Width - child.Width) / 2;
            int y = parent.Location.Y + (parent.Height - child.Height) / 2;
            child.Location = new Point(Math.Max(x, 0), Math.Max(y, 0));
        }
        #endregion

        #region Message Displays

        /// <summary>
        /// Displays a dialog showing the details of an error.
        /// </summary>
        /// <param name="lines">User-friendly message about the error.</param>
        /// <returns>The <see cref="DialogResult"/> associated with the dialog.</returns>
        internal static DialogResult Error(params string[] lines)
        {
            System.Media.SystemSounds.Exclamation.Play();
            string msg = string.Join(Environment.NewLine + Environment.NewLine, lines);
            return MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        internal static DialogResult Alert(params string[] lines)
        {
            System.Media.SystemSounds.Asterisk.Play();
            string msg = string.Join(Environment.NewLine + Environment.NewLine, lines);
            return MessageBox.Show(msg, "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static DialogResult Prompt(MessageBoxButtons btn, params string[] lines)
        {
            System.Media.SystemSounds.Question.Play();
            string msg = string.Join(Environment.NewLine + Environment.NewLine, lines);
            return MessageBox.Show(msg, "Prompt", btn, MessageBoxIcon.Asterisk);
        }

        internal static int getIndex(ComboBox cb)
        {
            return (int)(cb?.SelectedValue ?? 0);
        }
        #endregion
    }
}
