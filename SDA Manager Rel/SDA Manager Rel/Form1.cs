using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Drawing;

namespace SDA_Manager_Rel
{
    public partial class Form1 : Form
    {
        string name = "Steam Desktop Authenticator";

        public Form1()
        {
            InitializeComponent();
            LoadAllSettings();

            MakeTransparent(listView1, listView1.Location.X, listView1.Location.Y);
            TransparentControl(panel4, pictureBox1);
            TransparentControl(label1, pictureBox1);
            TransparentControl(label2, pictureBox1);

            RefreshListOfAuths();
        }

        private enum WindowShowStyle : uint
        {
            /// <summary>Hides the window and activates another window.</summary>
            /// <remarks>See SW_HIDE</remarks>
            Hide = 0,
            /// <summary>Activates and displays a window. If the window is minimized
            /// or maximized, the system restores it to its original size and
            /// position. An application should specify this flag when displaying
            /// the window for the first time.</summary>
            /// <remarks>See SW_SHOWNORMAL</remarks>
            ShowNormal = 1,
            /// <summary>Activates the window and displays it as a minimized window.</summary>
            /// <remarks>See SW_SHOWMINIMIZED</remarks>
            ShowMinimized = 2,
            /// <summary>Activates the window and displays it as a maximized window.</summary>
            /// <remarks>See SW_SHOWMAXIMIZED</remarks>
            ShowMaximized = 3,
            /// <summary>Maximizes the specified window.</summary>
            /// <remarks>See SW_MAXIMIZE</remarks>
            Maximize = 3,
            /// <summary>Displays a window in its most recent size and position.
            /// This value is similar to "ShowNormal", except the window is not
            /// actived.</summary>
            /// <remarks>See SW_SHOWNOACTIVATE</remarks>
            ShowNormalNoActivate = 4,
            /// <summary>Activates the window and displays it in its current size
            /// and position.</summary>
            /// <remarks>See SW_SHOW</remarks>
            Show = 5,
            /// <summary>Minimizes the specified window and activates the next
            /// top-level window in the Z order.</summary>
            /// <remarks>See SW_MINIMIZE</remarks>
            Minimize = 6,
            /// <summary>Displays the window as a minimized window. This value is
            /// similar to "ShowMinimized", except the window is not activated.</summary>
            /// <remarks>See SW_SHOWMINNOACTIVE</remarks>
            ShowMinNoActivate = 7,
            /// <summary>Displays the window in its current size and position. This
            /// value is similar to "Show", except the window is not activated.</summary>
            /// <remarks>See SW_SHOWNA</remarks>
            ShowNoActivate = 8,
            /// <summary>Activates and displays the window. If the window is
            /// minimized or maximized, the system restores it to its original size
            /// and position. An application should specify this flag when restoring
            /// a minimized window.</summary>
            /// <remarks>See SW_RESTORE</remarks>
            Restore = 9,
            /// <summary>Sets the show state based on the SW_ value specified in the
            /// STARTUPINFO structure passed to the CreateProcess function by the
            /// program that started the application.</summary>
            /// <remarks>See SW_SHOWDEFAULT</remarks>
            ShowDefault = 10,
            /// <summary>Windows 2000/XP: Minimizes a window, even if the thread
            /// that owns the window is hung. This flag should only be used when
            /// minimizing windows from a different thread.</summary>
            /// <remarks>See SW_FORCEMINIMIZE</remarks>
            ForceMinimized = 11
        }


        private void LoadAllSettings()
        {
            //Control control = this.Controls.Find(checkboxParams.Split(';')[0], true);
            string[] checkboxesArr = FileOperations.readTextFromFile("checkboxes.txt").Split(new string[] { "{}" }, StringSplitOptions.RemoveEmptyEntries);
            string[] textboxesArr = FileOperations.readTextFromFile("textboxes.txt").Split(new string[] { "{}" }, StringSplitOptions.RemoveEmptyEntries);
            string[] radiobuttonsArr = FileOperations.readTextFromFile("radiobuttons.txt").Split(new string[] { "{}" }, StringSplitOptions.RemoveEmptyEntries);
            string[] numericupdownsArr = FileOperations.readTextFromFile("numericupdowns.txt").Split(new string[] { "{}" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string checkboxParams in checkboxesArr)
            {
                try
                {
                    CheckBox checkbox = this.Controls.Find(checkboxParams.Split(';')[0].Replace(Environment.NewLine, ""), true).FirstOrDefault() as CheckBox;
                    checkbox.Checked = Convert.ToBoolean(checkboxParams.Split(';')[1]);

                }
                catch (Exception ex) { }
            }
            foreach (string textboxParams in textboxesArr)
            {
                try
                {
                    TextBox textbox = this.Controls.Find(textboxParams.Split(';')[0].Replace(Environment.NewLine, ""), true).FirstOrDefault() as TextBox;
                    textbox.Text = textboxParams.Split(';')[1];

                }
                catch (Exception ex) { }
            }

            foreach (string radiobuttonParams in radiobuttonsArr)
            {
                try
                {
                    RadioButton radiobutton = this.Controls.Find(radiobuttonParams.Split(';')[0].Replace(Environment.NewLine, ""), true).FirstOrDefault() as RadioButton;
                    radiobutton.Checked = Convert.ToBoolean(radiobuttonParams.Split(';')[1]);

                }
                catch (Exception ex) { }
            }

            foreach (string numericupdownParams in numericupdownsArr)
            {
                try
                {

                    NumericUpDown numericupdown = this.Controls.Find(numericupdownParams.Split(';')[0].Replace(Environment.NewLine, ""), true).FirstOrDefault() as NumericUpDown;
                    numericupdown.Value = Convert.ToDecimal(numericupdownParams.Split(';')[1]);
                }
                catch (Exception ex)
                {

                }
            }
            //Controls["textBox2"].Text = "OK";// где textBox2 - это имя
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        // Get a handle to an application window.
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName,
            string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);


        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        /// <summary>
        /// Find window by Caption only. Note you must pass IntPtr.Zero as the first parameter.
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        const UInt32 WM_CLOSE = 0x0010;
        const UInt32 SW_HIDE = 0;
        const UInt32 SW_SHOW = 5;


        List<IntPtr> ListHandles = new List<IntPtr>();

        string GetWindowText(IntPtr hWnd)
        {
            int len = GetWindowTextLength(hWnd) + 1;
            StringBuilder sb = new StringBuilder(len);
            len = GetWindowText(hWnd, sb, len);
            return sb.ToString(0, len);
        }

        void RefreshListOfAuths()
        {
            listView1.Items.Clear();
            EnumWindows((hWnd, lParam) =>
            {
                if (/*(IsWindowVisible(hWnd) && GetWindowTextLength(hWnd) != 0) && */GetWindowText(hWnd).StartsWith(name))
                {
                    ListHandles.Add(hWnd);
                    listView1.Items.Add(GetHwnd.Get_WindowNickname(hWnd));
                }


                return true;
            }, IntPtr.Zero);

            string itemsStr = "";
            foreach (object item in listView1.Items)
            {
                itemsStr += item.ToString() + ";";
            }
            foreach (Panel panel in panel4.Controls.OfType<Panel>())
            {
                if (panel.Controls.OfType<CheckBox>().ToList()[0].Checked)
                {
                    string acc = panel.Controls.OfType<TextBox>().ToList()[0].Text;
                    if (!itemsStr.ToLower().Contains(acc.ToLower()))
                    {
                        listView1.Items.Add("[ " + acc + " ]");
                    }
                }
            }
        }


        void TransparentControl(Control label1, PictureBox picturebox1)
        {
            var pos = this.PointToScreen(label1.Location);
            pos = pictureBox1.PointToClient(pos);
            label1.Parent = pictureBox1;
            label1.Location = pos;
            label1.BackColor = Color.Transparent;
        }

        private void MakeTransparent(Control ctrl, int x, int y)
        {
            Bitmap bMap = new Bitmap(pictureBox1.BackgroundImage);
            Color[,] pixelArray = new Color[ctrl.Width, ctrl.Height];

            for (int i = 0; i < ctrl.Width; i++)
            {
                for (int j = 0; j < ctrl.Height; j++)
                {
                    pixelArray[i, j] = bMap.GetPixel(x + i, y + j);
                }
            }

            Bitmap bmp = new Bitmap(ctrl.Width, ctrl.Height);

            for (int i = 0; i < ctrl.Width; i++)
            {
                for (int j = 0; j < ctrl.Height; j++)
                {
                    bmp.SetPixel(i, j, pixelArray[i, j]);
                }
            }

            ctrl.BackgroundImage = bmp;
            ctrl.Location = new Point(x, y);
        }


        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox openHide = sender as CheckBox;

            if (openHide.Checked)
            {
                openHide.Text = "Все аутентификаторы <<";
                openHide.BackColor = Color.FromName("DarkGray");
                this.Width = 840;
            }
            else
            {
                openHide.Text = "Все аутентификаторы >>";
                openHide.BackColor = Color.FromName("Ivory");
                this.Width = 318;
            }
        }

        private void cbTopMost_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox openHide = sender as CheckBox;

            if (openHide.Checked)
            {
                openHide.Text = "Поверх всех окон: ON";
                TopMost = true;
            }
            else
            {
                openHide.Text = "Поверх всех окон: OFF";
                TopMost = false;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.vk.com/goniss");
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.vk.com/goniss");
        }

        private void button19_Click(object sender, EventArgs e)
        {
            FileOperations.saveTextToFile("checkboxes.txt", "");
            FileOperations.saveTextToFile("textboxes.txt", "");
            FileOperations.saveTextToFile("radiobuttons.txt", "");
            FileOperations.saveTextToFile("numericupdowns.txt", "");
            TraverseControls(this);
        }

        public void TraverseControls(Control ctrl)
        {
            if (!ctrl.HasChildren) return;

            foreach (Control childCtrl in ctrl.Controls)
            {
                if (childCtrl is CheckBox)
                {
                    CheckBox checkbox = childCtrl as CheckBox;
                    FileOperations.addTextToFile("checkboxes.txt", checkbox.Name + ";" + checkbox.Checked.ToString() + "{}");
                }
                if (childCtrl is RadioButton)
                {
                    RadioButton radiobutton = childCtrl as RadioButton;
                    FileOperations.addTextToFile("radiobuttons.txt", radiobutton.Name + ";" + radiobutton.Checked + "{}");
                }
                if (childCtrl is TextBox)
                {
                    TextBox textbox = childCtrl as TextBox;
                    FileOperations.addTextToFile("textboxes.txt", textbox.Name + ";" + textbox.Text + "{}");
                }
                if (childCtrl is NumericUpDown)
                {
                    NumericUpDown numericupdown = childCtrl as NumericUpDown;
                    FileOperations.addTextToFile("numericupdowns.txt", numericupdown.Name + ";" + numericupdown.Text + "{}");
                }
                TraverseControls(childCtrl);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            openApplicationByButton(sender as Button);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            openApplicationByButton(sender as Button);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            openApplicationByButton(sender as Button);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            openApplicationByButton(sender as Button);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            openApplicationByButton(sender as Button);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            openApplicationByButton(sender as Button);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            openApplicationByButton(sender as Button);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            openApplicationByButton(sender as Button);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            openApplicationByButton(sender as Button);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            openApplicationByButton(sender as Button);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            openApplicationByButton(sender as Button);
        }

        public void openApplicationByButton(Button btn)
        {
            try
            {
                Panel panel = btn.Parent as Panel;
                string Path = panel.Controls.OfType<TextBox>().ToList()[1].Text + "\\" + "Steam Desktop Authenticator.exe";

                Process _process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();

                startInfo.FileName = Path;

                _process.StartInfo = startInfo;
                _process.Start();


                //form3Things.refreshButton.PerformClick();
            }
            catch (Exception ex) { }

            RefreshListOfAuths();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                ListView listview = sender as ListView;

                foreach (IntPtr hWnd in ListHandles)
                {
                    if (GetHwnd.Get_WindowTextByHandle(hWnd).Contains(listview.SelectedItems[0].Text))
                    {
                        label3.Text = GetHwnd.Get_WindowAuthCode(hWnd);
                    }
                }
            }
            catch (Exception ex) { }

        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListView listbox = sender as ListView;

            foreach (IntPtr hWnd in ListHandles)
            {
                if (GetHwnd.Get_WindowTextByHandle(hWnd).Contains(listbox.SelectedItems[0].Text))
                {
                    string CODE = GetHwnd.Get_WindowAuthCode(hWnd);
                    label3.Text = CODE;
                    Clipboard.SetText(CODE);
                }
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    ListView listbox = sender as ListView;

                    if (!listbox.SelectedItems[0].Text.ToLower().Contains("account"))
                    {
                        foreach (Panel panel in panel4.Controls.OfType<Panel>())
                        {
                            if (listbox.SelectedItems[0].Text.Contains(panel.Controls.OfType<TextBox>().ToList()[0].Text) && panel.Controls.OfType<TextBox>().ToList()[0].Text != "")
                            {
                                panel.Controls.OfType<Button>().ToList()[0].PerformClick();
                            }
                        }
                    }

                    foreach (IntPtr hWnd in ListHandles)
                    {
                        if (GetHwnd.Get_WindowTextByHandle(hWnd).Contains(listbox.SelectedItems[0].Text))
                        {
                            ShowWindow(hWnd, WindowShowStyle.ShowNormal);
                            //ShowWindow(hWnd, WindowShowStyle.Show);
                            SetForegroundWindow(hWnd);

                        }
                    }
                }
            }
            catch (Exception ex) { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (IntPtr hWnd in ListHandles)
                {
                    foreach (ListViewItem item in listView1.SelectedItems)
                    {
                        if (GetHwnd.Get_WindowTextByHandle(hWnd).Contains(item.Text.ToString()))
                        {
                            if (hWnd == IntPtr.Zero)
                            {
                                //Console.WriteLine("Window not found");
                                return;
                            }

                            SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                        }
                    }

                }
            }
            catch (Exception ex) { }
            RefreshListOfAuths();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshListOfAuths();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Form2 form2 = new SDA_Manager_Rel.Form2();
            form2.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new SDA_Manager_Rel.Form2();
            form2.Show();
        }
    }

    public static class FileOperations 
    {
        public static string ReturnFilenameString(string wrongFilename)
        {
            return wrongFilename.Replace(":", "").Replace("|", "").Replace("\\", "").Replace("/", "").Replace("*", "").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("+", "").Replace(Environment.NewLine, "");
        }


        public static void addTextToFile(string filename, string textToAdd)
        {
            if (!System.IO.File.Exists(filename))
            {
                saveTextToFile(filename, "");
            }
            System.IO.StreamWriter writer = new System.IO.StreamWriter(filename, true);
            writer.WriteLine(textToAdd);
            writer.Close();
        }

        public static string readTextFromFile(string filename)
        {
            if (!System.IO.File.Exists(filename)) return "";
            try
            {
                return System.IO.File.ReadAllText(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
                return "";
            }
        }

        public static void saveTextToFile(string filename, string textToSave)
        {
            try
            {
                System.IO.File.WriteAllText(filename, textToSave);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }

    public static class GetHwnd
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i);

        public static string Get_WindowTextByHandle(IntPtr hWnd)
        {
            const Int32 SB_GETTEXT = 0x000D;
            const Int32 SB_GETTEXTLENGTH = 0x000E;

            List<IntPtr> ls = new List<IntPtr>();
            string enum_ = string.Empty;

            if (!hWnd.Equals(IntPtr.Zero))
                ls = GetChildWindows(hWnd);
            foreach (IntPtr iii in ls)
            {

                int length = (int)SendMessage(iii, SB_GETTEXTLENGTH, IntPtr.Zero, null);
                StringBuilder sb = new StringBuilder(length + 1);
                //string SB = "effbbsbdbdsbsdbsdbsb";
                SendMessage(iii, SB_GETTEXT, (IntPtr)sb.Capacity, sb);
                //return sb.ToString();
                enum_ += sb.ToString() + "\r\n";

            }

            return enum_;
        }

        public static string Get_WindowNickname(IntPtr hWnd)
        {
            const Int32 SB_GETTEXT = 0x000D;
            const Int32 SB_GETTEXTLENGTH = 0x000E;

            List<IntPtr> ls = new List<IntPtr>();
            string enum_ = string.Empty;

            if (!hWnd.Equals(IntPtr.Zero))
                ls = GetChildWindows(hWnd);
            foreach (IntPtr iii in ls)
            {

                int length = (int)SendMessage(iii, SB_GETTEXTLENGTH, IntPtr.Zero, null);
                StringBuilder sb = new StringBuilder(length + 1);
                //string SB = "effbbsbdbdsbsdbsdbsb";
                SendMessage(iii, SB_GETTEXT, (IntPtr)sb.Capacity, sb);
                //return sb.ToString();
                enum_ += sb.ToString() + "\r\n";

                if (Convert.ToString(sb).Contains("Account:")) return sb.ToString();
            }
            return "";
        }

        public static string Get_WindowAuthCode(IntPtr hWnd)
        {
            const Int32 SB_GETTEXT = 0x000D;
            const Int32 SB_GETTEXTLENGTH = 0x000E;

            List<IntPtr> ls = new List<IntPtr>();
            string enum_ = string.Empty;

            if (!hWnd.Equals(IntPtr.Zero))
                ls = GetChildWindows(hWnd);
            foreach (IntPtr iii in ls)
            {

                int length = (int)SendMessage(iii, SB_GETTEXTLENGTH, IntPtr.Zero, null);
                StringBuilder sb = new StringBuilder(length + 1);
                //string SB = "effbbsbdbdsbsdbsdbsb";
                SendMessage(iii, SB_GETTEXT, (IntPtr)sb.Capacity, sb);
                //return sb.ToString();
                enum_ += sb.ToString() + "\r\n";

                if (Convert.ToString(sb).Length == 5 && Convert.ToString(sb).ToUpper() == Convert.ToString(sb)) return sb.ToString();
            }
            return "";
        }

        public static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }


        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }


        public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint uMsg, IntPtr wParam, StringBuilder lParam);


        public static string getHwnd(string name)
        {

            const Int32 SB_GETTEXT = 0x000D;
            const Int32 SB_GETTEXTLENGTH = 0x000E;

            List<IntPtr> ls = new List<IntPtr>();
            string enum_ = string.Empty;
            IntPtr hWnd = FindWindow(null, name);
            if (!hWnd.Equals(IntPtr.Zero))
                ls = GetChildWindows(hWnd);
            foreach (IntPtr iii in ls)
            {

                int length = (int)SendMessage(iii, SB_GETTEXTLENGTH, IntPtr.Zero, null);
                StringBuilder sb = new StringBuilder(length + 1);
                //string SB = "effbbsbdbdsbsdbsdbsb";
                SendMessage(iii, SB_GETTEXT, (IntPtr)sb.Capacity, sb);
                //return sb.ToString();
                enum_ += sb.ToString() + "\r\n";
            }

            //Array.ForEach(ls.ToArray(), i => enum_ += sb.ToString() + Environment.NewLine);

            return enum_;
        }
    }

}
