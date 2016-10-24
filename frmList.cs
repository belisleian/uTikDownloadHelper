using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace uTikDownloadHelper
{
    public partial class frmList : Form
    {
        TitleList titles = new TitleList();
        String myExe = System.Reflection.Assembly.GetEntryAssembly().Location;

        public frmList()
        {
            InitializeComponent();
        }

        private void frmList_Closed(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void populateList()
        {
            lstMain.Items.Clear();
            List<TitleInfo> titleList = titleList = titles.filter(txtSearch.Text, comboRegion.SelectedItem.ToString());

            foreach (TitleInfo title in titleList)
            {
                lstMain.Items.Add(title.getListViewItem());
            }
            frmList_SizeChanged(null, null);
            enableDisableDownloadButton();
        }

        private void frmList_SizeChanged(object sender, EventArgs e)
        {
            lstMain.Columns[1].Width = lstMain.Width - lstMain.Columns[0].Width - lstMain.Columns[2].Width - 4 - SystemInformation.VerticalScrollBarWidth;
        }

        private void enableDisableDownloadButton()
        {
            if (lstMain.SelectedItems.Count > 0)
            {
                btnDownload.Enabled = true;
            } else
            {
                btnDownload.Enabled = false;
            }
        }

        private void frmList_Load(object sender, EventArgs e) {
            this.lblLoading.Location = lstMain.Location;
            this.lblLoading.Size = lstMain.Size;
            btnTitleKeyCheck.Location = lstMain.Location;
            btnTitleKeyCheck.Size = lstMain.Size;

            if (Common.Settings.ticketWebsite != null && Common.Settings.ticketWebsite.Length > 0)
                btnTitleKeyCheck.Dispose();
            
            titles.ListUpdated += (object send, EventArgs ev) =>
            {
                comboRegion.Items.Clear();
                comboRegion.Items.Add("Any");
                foreach (TitleInfo title in titles.titles)
                {
                    if (!comboRegion.Items.Contains(title.region) && title.region.Length > 0)
                    {
                        comboRegion.Items.Add(title.region);
                    }
                }
                String lastRegion = Common.Settings.lastSelectedRegion;
                if (comboRegion.Items.Contains(lastRegion))
                {
                    comboRegion.SelectedIndex = comboRegion.Items.IndexOf(lastRegion);
                } else
                {
                    comboRegion.SelectedIndex = 0;
                }
                comboRegion.Enabled = true;
                txtSearch.Enabled = true;
                lstMain.Enabled = true;
                lblLoading.Dispose();
            };
        }

        private void frmList_Shown(object sender, EventArgs e)
        {
            if (Common.Settings.ticketWebsite != null && Common.Settings.ticketWebsite.Length > 0)
            {
                titles.getTitleList();
            }
        }

        private void comboRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            populateList();
            Common.Settings.lastSelectedRegion = comboRegion.SelectedItem.ToString();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofdTik = new OpenFileDialog();
            ofdTik.Filter = ".tik File|*.tik";
            if (ofdTik.ShowDialog() == DialogResult.OK)
            {
                launchSelfWithTicket(ofdTik.FileName);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            populateList();
        }

        private void launchSelfWithTicket(String ticketPath)
        {
            var procStIfo = new ProcessStartInfo(myExe, "\"" + ticketPath + "\"");
            procStIfo.RedirectStandardOutput = false;
            procStIfo.UseShellExecute = false;
            procStIfo.CreateNoWindow = false;

            var proc = new Process();
            proc.StartInfo = procStIfo;
            proc.Start();
        }

        private void handleListItem(object sender, EventArgs e)
        {
            if (lstMain.SelectedItems.Count > 0)
            {
                TitleInfo info = (TitleInfo)lstMain.SelectedItems[0].Tag;
                launchSelfWithTicket("https://" + Common.Settings.ticketWebsite + "/ticket/" + info.titleID.ToLower() + ".tik");
            }
        }

        private void lstMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            enableDisableDownloadButton();
        }

        private void btnTitleKeyCheck_Click(object sender, EventArgs e)
        {
            String website = Microsoft.VisualBasic.Interaction.InputBox("What is the address of this website? \n\nThe Wii U Title Key Database", "Answer this question", "", -1, -1).ToLower();
            if (Common.getMD5Hash(website) == "d098abb93c29005dbd07deb43d81c5df")
            {
                Common.Settings.ticketWebsite = website;
                titles.getTitleList();
                ((Button)sender).Dispose();
            }
        }
    }
}
