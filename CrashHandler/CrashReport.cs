using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace EasyCraft.CrashHandler
{
    public partial class CrashReport : Form
    {
        private const string EMAIL_REGEX = @"^\S+@\S+\.\S+$";

        bool emailCorrect = false;
        Color status_none = Color.FromArgb(150, 150, 150);
        Color status_low = Color.FromArgb(200, 0, 0);
        Color status_mid = Color.FromArgb(214, 207, 0);
        Color status_high = Color.FromArgb(137, 247, 35);
        Color status_max = Color.FromArgb(8, 181, 2);

        class FileListItem
        {
            public FileListItem(string _type, string file)
            {
                fileType = _type;
                name = Path.GetFileName(file);
                path = Path.GetFullPath(file);
            }

            public string fileType { get; set; }
            public string name { get; set; }
            public string path { get; set; }
        }

        private bool GetFile(Dictionary<string, string> args, string key, out string file)
        {
            file = null;
            if (!args.ContainsKey(key.ToUpper())) return false;
            file = args[key.ToUpper()];
            if (!File.Exists(file)) return false;
            return true;
        }

        private bool GetPaths(Dictionary<string, string> args, string key, out string[] paths)
        {
            paths = null;
            if (!args.ContainsKey(key.ToUpper())) return false;
            List<string> tmp = new List<string>(args[key.ToUpper()].Split(';'));
            List<int> remove = new List<int>();
            for (int i = 0; i < tmp.Count; i++)
                if (!File.Exists(tmp[i]) && !Directory.Exists(tmp[i])) remove.Add(i);
            foreach (int i in remove) tmp.RemoveAt(i);
            if (tmp.Count == 0) return false;
            paths = tmp.ToArray();
            return true;
        }

        public CrashReport(Dictionary<string, string> args)
        {
            List<FileListItem> files = new List<FileListItem>();
            if (GetFile(args, "/c", out string logPath)) files.Add(new FileListItem("log", logPath));
            if (GetFile(args, "/d", out string dumpPath)) files.Add(new FileListItem("dump", dumpPath));

            if (GetPaths(args, "/a", out string[] attachments))
            {
                foreach(string attachment in attachments)
                {
                    if (File.Exists(attachment)) files.Add(new FileListItem("file", attachment));
                    else if (Directory.Exists(attachment)) files.Add(new FileListItem("folder", attachment));
                }
            }

            InitializeComponent();
            fileListView.SetObjects(files);
        }

        private void emailInput_TextChanged(object sender, EventArgs e)
        {
            emailCorrect = Regex.IsMatch(emailInput.Text, EMAIL_REGEX);
            emailInput.ForeColor = emailCorrect ? Color.Black : Color.Red;
            UpdateStatus();
        }

        private void fileListView_DoubleClick(object sender, EventArgs e)
        {
            if(fileListView.SelectedObject != null)
            {
                Process.Start(((FileListItem)fileListView.SelectedObject).path);
            }
        }

        private void UpdateStatus()
        {
            int status = 0;
            if (emailCorrect) status++;
            if (descInput.Text.Length > 15) status++;
            if (descInput.Text.Length > 120) status++;
            if (descInput.Text.Length > 300) status++;
            switch (status)
            {
                case 0:
                    status1.BackColor = status_none;
                    status2.BackColor = status_none;
                    status3.BackColor = status_none;
                    status4.BackColor = status_none;
                    break;
                case 1:
                    status1.BackColor = status_low;
                    status2.BackColor = status_none;
                    status3.BackColor = status_none;
                    status4.BackColor = status_none;
                    break;
                case 2:
                    status1.BackColor = status_mid;
                    status2.BackColor = status_mid;
                    status3.BackColor = status_none;
                    status4.BackColor = status_none;
                    break;
                case 3:
                    status1.BackColor = status_high;
                    status2.BackColor = status_high;
                    status3.BackColor = status_high;
                    status4.BackColor = status_none;
                    break;
                case 4:
                    status1.BackColor = status_max;
                    status2.BackColor = status_max;
                    status3.BackColor = status_max;
                    status4.BackColor = status_max;
                    break;
            }
        }

        private void descInput_TextChanged(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show(this, "Do you want to send the report?", "Confirm Action", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //TODO: make sending
                Thread.Sleep(500);
                Close();
            }
        }
    }
}
