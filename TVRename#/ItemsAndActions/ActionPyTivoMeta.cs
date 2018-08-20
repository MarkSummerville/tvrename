// 
// Main website for TVRename is http://tvrename.com
// 
// Source code available at https://github.com/TV-Rename/tvrename
// 
// This code is released under GPLv3 https://github.com/TV-Rename/tvrename/blob/master/LICENSE.md
// 
namespace TVRename
{
    using System;
    using System.Windows.Forms;
    using System.IO;
    using Directory = Alphaleonis.Win32.Filesystem.Directory;
    using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;

    public class ActionPyTivoMeta : Item, Action, ScanListItem, ActionWriteMetadata
    {
        public FileInfo Where;

        public ActionPyTivoMeta(FileInfo nfo, ProcessedEpisode pe)
        {
            this.Episode = pe;
            this.Where = nfo;
        }

        public string produces => this.Where.FullName;

        #region Action Members

        public string Name => "Write pyTivo Meta";

        public bool Done { get; private set; }
        public bool Error { get; private set; }
        public string ErrorText { get; set; }

        public string ProgressText => this.Where.Name;

        public double PercentDone => this.Done ? 100 : 0;

        public long SizeOfWork => 10000;

        public bool Go( ref bool pause, TVRenameStats stats)
        {
            // "try" and silently fail.  eg. when file is use by other...
            StreamWriter writer;
            try
            {
                // create folder if it does not exist. (Only really applies when .meta\ folder is being used.)
                if (!this.Where.Directory.Exists)
                    Directory.CreateDirectory(this.Where.Directory.FullName);
                writer = new System.IO.StreamWriter(this.Where.FullName, false, System.Text.Encoding.GetEncoding(1252));
                if (writer == null)
                    return false;
            }
            catch (Exception)
            {
                this.Done = true;
                return true;
            }

            // See: http://pytivo.sourceforge.net/wiki/index.php/Metadata
            writer.WriteLine($"title : {this.Episode.SI.ShowName}");
            writer.WriteLine($"seriesTitle : {this.Episode.SI.ShowName}");
            writer.WriteLine($"episodeTitle : {this.Episode.Name}");
            writer.WriteLine(
                $"episodeNumber : {this.Episode.AppropriateSeasonNumber}{this.Episode.AppropriateEpNum:0#}");
            writer.WriteLine("isEpisode : true");
            writer.WriteLine($"description : {this.Episode.Overview}");
            if (this.Episode.FirstAired != null)
                writer.WriteLine($"originalAirDate : {this.Episode.FirstAired.Value:yyyy-MM-dd}T00:00:00Z");
            writer.WriteLine($"callsign : {this.Episode.SI.TheSeries().getNetwork()}");

            WriteEntries(writer, "vDirector", this.Episode.EpisodeDirector);
            WriteEntries(writer, "vWriter", this.Episode.Writer);
            WriteEntries(writer, "vActor", String.Join("|", this.Episode.SI.TheSeries().GetActors()));
            WriteEntries(writer, "vGuestStar", this.Episode.EpisodeGuestStars); // not worring about actors being repeated
            WriteEntries(writer, "vProgramGenre", String.Join("|", this.Episode.SI.TheSeries().GetGenres()));

            writer.Close();
            this.Done = true;
            return true;
        }

        private void WriteEntries(StreamWriter writer, string Heading, string Entries)
        {
            if (string.IsNullOrEmpty(Entries))
                return;
            if (!Entries.Contains("|"))
                writer.WriteLine($"{Heading} : {Entries}");
            else
            {
                foreach (string entry in Entries.Split('|'))
                    if (!string.IsNullOrEmpty(entry))
                        writer.WriteLine($"{Heading} : {entry}");
            }
        }

        #endregion

        #region Item Members

        public bool SameAs(Item o)
        {
            return (o is ActionPyTivoMeta) && ((o as ActionPyTivoMeta).Where == this.Where);
        }

        public int Compare(Item o)
        {
            ActionPyTivoMeta nfo = o as ActionPyTivoMeta;

            if (this.Episode == null)
                return 1;
            if (nfo ==null || nfo.Episode == null)
                return -1;
            return (this.Where.FullName + this.Episode.Name).CompareTo(nfo.Where.FullName + nfo.Episode.Name);
        }

        #endregion

        #region ScanListItem Members

        public IgnoreItem Ignore
        {
            get
            {
                if (this.Where == null)
                    return null;
                return new IgnoreItem(this.Where.FullName);
            }
        }

        public ListViewItem ScanListViewItem
        {
            get
            {
                ListViewItem lvi = new ListViewItem {Text = this.Episode.SI.ShowName};

                lvi.SubItems.Add(this.Episode.AppropriateSeasonNumber.ToString());
                lvi.SubItems.Add(this.Episode.NumsAsString());
                DateTime? dt = this.Episode.GetAirDateDT(true);
                if ((dt != null) && (dt.Value.CompareTo(DateTime.MaxValue)) != 0)
                    lvi.SubItems.Add(dt.Value.ToShortDateString());
                else
                    lvi.SubItems.Add("");

                lvi.SubItems.Add(this.Where.DirectoryName);
                lvi.SubItems.Add(this.Where.Name);

                lvi.Tag = this;

                //lv->Items->Add(lvi);
                return lvi;
            }
        }

        string ScanListItem.TargetFolder
        {
            get
            {
                if (this.Where == null)
                    return null;
                return this.Where.DirectoryName;
            }
        }

        public string ScanListViewGroup => "lvgActionMeta";

        public int IconNumber => 7;

        public ProcessedEpisode Episode { get; private set; }

        #endregion

    }
}
