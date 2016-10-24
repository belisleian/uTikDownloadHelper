using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace uTikDownloadHelper
{
    public struct TitleInfo
    {
        public String titleID { get; set; }
        public String titleKey { get; set; }
        public String name { get; set; }
        public String region { get; set; }
        public TitleInfo(String titleID, String titleKey, String name, String region)
        {
            this.titleID = (titleID != null ? titleID.Trim() : "");
            this.titleKey = (titleKey != null ? titleKey.Trim() : "");
            this.name = (name != null ? name.Trim() : "");
            this.region = (region != null ? region.Trim() : "");
        }
        public ListViewItem getListViewItem()
        {
            ListViewItem item = new ListViewItem();
            item.Text = titleID;
            item.SubItems.Add(name);
            item.SubItems.Add(region);
            item.Tag = this;
            return item;
        }
    }
    public class TitleList 
    {
        private WebClient client = new WebClient();
        private String[] allowedTitleTypes = {"00050000", "0005000C" };
        public List<TitleInfo> titles = new List<TitleInfo> { };
        

        public List<TitleInfo> filter(String name, String region)
        {
            return titles.Where(title => (title.region == region || region == "Any") && title.name.ToLower().Contains(name.ToLower())).ToList();
        }

        public TitleList()
        {
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(titleDataFetched);
        }

        public void titleDataFetched(object sender, DownloadStringCompletedEventArgs e) {
            if (e.Error != null)
                return;

            titles.Clear();

            dynamic json = JArray.Parse(e.Result);

            foreach (dynamic obj in json)
            {
                if(obj.ticket == "1")
                {
                    TitleInfo info = new TitleInfo((String)(obj.titleID), (String)(obj.titleKey), (String)(obj.name), (String)(obj.region));

                    if (info.titleID.Length > 8 && allowedTitleTypes.Contains(info.titleID.Substring(0, 8)))

                        if (info.name.Length > 0)
                            titles.Add(info);
                }
            }

            titles = titles.OrderBy(o => o.name).ToList();
            OnListUpdated();
        }

        public void getTitleList()
        {
            client.DownloadStringAsync(new Uri("http://" + Common.Settings.ticketWebsite + "/json"));
        }

        public event EventHandler ListUpdated;
        protected virtual void OnListUpdated()
        {
            ListUpdated?.Invoke(this, null);
        }
    }
}
