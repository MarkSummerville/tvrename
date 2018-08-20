// 
// Main website for TVRename is http://tvrename.com
// 
// Source code available at https://github.com/TV-Rename/tvrename
// 
// This code is released under GPLv3 https://github.com/TV-Rename/tvrename/blob/master/LICENSE.md
// 
using System;
using Alphaleonis.Win32.Filesystem;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

// These are what is used when processing folders for missing episodes, renaming, etc. of files.

// A "ProcessedEpisode" is generated by processing an Episode from thetvdb, and merging/renaming/etc.
//
// A "ShowItem" is a show the user has added on the "My Shows" tab

// TODO: C++ to C# conversion stopped it using some of the typedefs, such as "IgnoreSeasonList".  (a) probably should
// rename that to something more generic like IntegerList, and (b) then put it back into the classes & functions
// that use it (e.g. ShowItem.IgnoreSeasons)

namespace TVRename
{
    public class ProcessedEpisode : Episode
    {
        public int EpNum2; // if we are a concatenation of episodes, this is the last one in the series. Otherwise, same as EpNum
        public bool Ignore;
        public bool NextToAir;
        public int OverallNumber;
        public ShowItem SI;
        public ProcessedEpisodeType type;
        public List<Episode> sourceEpisodes;

        public enum ProcessedEpisodeType { single, split, merged};


        public ProcessedEpisode(SeriesInfo ser, Season airseas, Season dvdseas, ShowItem si)
            : base(ser, airseas,dvdseas)
        {
            this.NextToAir = false;
            this.OverallNumber = -1;
            this.Ignore = false;
            this.EpNum2 = si.DVDOrder? this.DVDEpNum: this.AiredEpNum;
            this.SI = si;
            this.type = ProcessedEpisodeType.single;
        }

        public ProcessedEpisode(ProcessedEpisode O)
            : base(O)
        {
            this.NextToAir = O.NextToAir;
            this.EpNum2 = O.EpNum2;
            this.Ignore = O.Ignore;
            this.SI = O.SI;
            this.OverallNumber = O.OverallNumber;
            this.type = O.type;
        }

        public ProcessedEpisode(Episode e, ShowItem si)
            : base(e)
        {
            this.OverallNumber = -1;
            this.NextToAir = false;
            this.EpNum2 = si.DVDOrder ? this.DVDEpNum : this.AiredEpNum;
            this.Ignore = false;
            this.SI = si;
            this.type = ProcessedEpisodeType.single;
        }
        public ProcessedEpisode(Episode e, ShowItem si, ProcessedEpisodeType t)
            : base(e)
        {
            this.OverallNumber = -1;
            this.NextToAir = false;
            this.EpNum2 = si.DVDOrder ? this.DVDEpNum : this.AiredEpNum;
            this.Ignore = false;
            this.SI = si;
            this.type = t;
        }

        public ProcessedEpisode(Episode e, ShowItem si, List<Episode> episodes)
            : base(e)
        {
            this.OverallNumber = -1;
            this.NextToAir = false;
            this.EpNum2 = si.DVDOrder ? this.DVDEpNum : this.AiredEpNum;
            this.Ignore = false;
            this.SI = si;
            this.sourceEpisodes = episodes;
            this.type = ProcessedEpisodeType.merged ;
        }

        public int AppropriateSeasonNumber => this.SI.DVDOrder ? this.DVDSeasonNumber : this.AiredSeasonNumber;

        public Season AppropriateSeason => this.SI.DVDOrder ? this.TheDVDSeason : this.TheAiredSeason;

        public int AppropriateEpNum
        {
            get => this.SI.DVDOrder ? DVDEpNum : this.AiredEpNum;
            set
            {
                if (this.SI.DVDOrder) DVDEpNum = value;
                else this.AiredEpNum = value;
            }
        }


        public string NumsAsString()
        {
            if (this.AppropriateEpNum == this.EpNum2)
                return this.AppropriateEpNum.ToString();
            else
                return this.AppropriateEpNum + "-" + this.EpNum2;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static int EPNumberSorter(ProcessedEpisode e1, ProcessedEpisode e2)
        {
            int ep1 = e1.AiredEpNum;
            int ep2 = e2.AiredEpNum;

            return ep1 - ep2;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static int DVDOrderSorter(ProcessedEpisode e1, ProcessedEpisode e2)
        {
            int ep1 = e1.DVDEpNum;
            int ep2 = e2.DVDEpNum;

            return ep1 - ep2;
        }

        public DateTime? GetAirDateDT(bool inLocalTime)
        {

            if (!inLocalTime)
                return GetAirDateDT();

            // do timezone adjustment
            return GetAirDateDT(this.SI.GetTimeZone());
        }

        public string HowLong()
        {
            DateTime? airsdt = GetAirDateDT(true);
            if (airsdt == null)
                return "";
            DateTime dt = (DateTime)airsdt;

            TimeSpan ts = dt.Subtract(DateTime.Now); // how long...
            if (ts.TotalHours < 0)
                return "Aired";
            else
            {
                int h = ts.Hours;
                if (ts.TotalHours >= 1)
                {
                    if (ts.Minutes >= 30)
                        h += 1;
                    return ts.Days + "d " + h + "h"; // +ts->Minutes+"m "+ts->Seconds+"s";
                }
                else
                    return Math.Round(ts.TotalMinutes) + "min";
            }
        }

        public string DayOfWeek()
        {
            DateTime? dt = GetAirDateDT(true);
            return (dt != null) ? dt.Value.ToString("ddd") : "-";
        }

        public string TimeOfDay()
        {
            DateTime? dt = GetAirDateDT(true);
            return (dt != null) ? dt.Value.ToString("t") : "-";
        }

    }

public class ShowItem
    {
        public bool AutoAddNewSeasons;
        public string AutoAdd_FolderBase; // TODO: use magical renaming tokens here
        public bool AutoAdd_FolderPerSeason;
        public string AutoAdd_SeasonFolderName; // TODO: use magical renaming tokens here

        public bool CountSpecials;
        public string CustomShowName;
        public bool DVDOrder; // sort by DVD order, not the default sort we get
        public bool DoMissingCheck;
        public bool DoRename;
        public bool ForceCheckFuture;
        public bool ForceCheckNoAirdate;
        public List<int> IgnoreSeasons;
        public Dictionary<int, List<String>> ManualFolderLocations;
        public bool PadSeasonToTwoDigits;
        public Dictionary<int, List<ProcessedEpisode>> SeasonEpisodes; // built up by applying rules.
        public Dictionary<int, List<ShowRule>> SeasonRules;
        public bool ShowNextAirdate;
        public int TVDBCode;
        public bool UseCustomShowName;
        public bool UseSequentialMatch;
        public List<string> AliasNames = new List<string>();
        public bool UseCustomSearchURL;
        public String CustomSearchURL;

        public String ShowTimeZone;
        private TimeZone SeriesTZ;
        private string LastFiguredTZ;

        
        public DateTime? BannersLastUpdatedOnDisk { get; set; }

        public ShowItem()
        {
            SetDefaults();
        }

        public ShowItem(int tvDBCode)
        {
            SetDefaults();
            this.TVDBCode = tvDBCode;
        }

        private void FigureOutTimeZone()
        {
            string tzstr = this.ShowTimeZone;

            if (string.IsNullOrEmpty(tzstr))
                tzstr = TimeZone.DefaultTimeZone();

            this.SeriesTZ = TimeZone.TimeZoneFor(tzstr);

            this.LastFiguredTZ = tzstr;
        }

        public TimeZone GetTimeZone()
        {
            // we cache the timezone info, as the fetching is a bit slow, and we do this a lot
            if (this.LastFiguredTZ != this.ShowTimeZone)
                this.FigureOutTimeZone();

            return this.SeriesTZ;
        }

        public ShowItem(XmlReader reader)
        {
            SetDefaults();

            reader.Read();
            if (reader.Name != "ShowItem")
                return; // bail out

            reader.Read();
            while (!reader.EOF)
            {
                if ((reader.Name == "ShowItem") && !reader.IsStartElement())
                    break; // all done

                if (reader.Name == "ShowName")
                {
                    this.CustomShowName = reader.ReadElementContentAsString();
                    this.UseCustomShowName = true;
                }
                if (reader.Name == "UseCustomShowName")
                    this.UseCustomShowName = reader.ReadElementContentAsBoolean();
                if (reader.Name == "CustomShowName")
                    this.CustomShowName = reader.ReadElementContentAsString();
                else if (reader.Name == "TVDBID")
                    this.TVDBCode = reader.ReadElementContentAsInt();
                else if (reader.Name == "CountSpecials")
                    this.CountSpecials = reader.ReadElementContentAsBoolean();
                else if (reader.Name == "ShowNextAirdate")
                    this.ShowNextAirdate = reader.ReadElementContentAsBoolean();
                else if (reader.Name == "AutoAddNewSeasons")
                    this.AutoAddNewSeasons = reader.ReadElementContentAsBoolean();
                else if (reader.Name == "FolderBase")
                    this.AutoAdd_FolderBase = reader.ReadElementContentAsString();
                else if (reader.Name == "FolderPerSeason")
                    this.AutoAdd_FolderPerSeason = reader.ReadElementContentAsBoolean();
                else if (reader.Name == "SeasonFolderName")
                    this.AutoAdd_SeasonFolderName = reader.ReadElementContentAsString();
                else if (reader.Name == "DoRename")
                    this.DoRename = reader.ReadElementContentAsBoolean();
                else if (reader.Name == "DoMissingCheck")
                    this.DoMissingCheck = reader.ReadElementContentAsBoolean();
                else if (reader.Name == "DVDOrder")
                    this.DVDOrder = reader.ReadElementContentAsBoolean();
                else if (reader.Name == "UseCustomSearchURL")
                    this.UseCustomSearchURL = reader.ReadElementContentAsBoolean();
                else if (reader.Name == "CustomSearchURL")
                    this.CustomSearchURL = reader.ReadElementContentAsString();
                else if (reader.Name == "TimeZone")
                    this.ShowTimeZone = reader.ReadElementContentAsString();
                else if (reader.Name == "ForceCheckAll") // removed 2.2.0b2
                    this.ForceCheckNoAirdate = this.ForceCheckFuture = reader.ReadElementContentAsBoolean();
                else if (reader.Name == "ForceCheckFuture")
                    this.ForceCheckFuture = reader.ReadElementContentAsBoolean();
                else if (reader.Name == "ForceCheckNoAirdate")
                    this.ForceCheckNoAirdate = reader.ReadElementContentAsBoolean();
                else if (reader.Name == "PadSeasonToTwoDigits")
                    this.PadSeasonToTwoDigits = reader.ReadElementContentAsBoolean();
                else if (reader.Name == "BannersLastUpdatedOnDisk")
                {
                    if (!reader.IsEmptyElement)
                    {

                        this.BannersLastUpdatedOnDisk = reader.ReadElementContentAsDateTime();
                    }else
                    reader.Read();
                }

                else if (reader.Name == "UseSequentialMatch")
                    this.UseSequentialMatch = reader.ReadElementContentAsBoolean();
                else if (reader.Name == "IgnoreSeasons")
                {
                    if (!reader.IsEmptyElement)
                    {
                        reader.Read();
                        while (reader.Name != "IgnoreSeasons")
                        {
                            if (reader.Name == "Ignore")
                                this.IgnoreSeasons.Add(reader.ReadElementContentAsInt());
                            else
                                reader.ReadOuterXml();
                        }
                    }
                    reader.Read();
                }
                else if (reader.Name == "AliasNames")
                {
                    if (!reader.IsEmptyElement)
                    {
                        reader.Read();
                        while (reader.Name != "AliasNames")
                        {
                            if (reader.Name == "Alias")
                                this.AliasNames.Add(reader.ReadElementContentAsString());
                            else
                                reader.ReadOuterXml();
                        }
                    }
                    reader.Read();
                }
                else if (reader.Name == "Rules")
                {
                    if (!reader.IsEmptyElement)
                    {
                        int snum = int.Parse(reader.GetAttribute("SeasonNumber"));
                        this.SeasonRules[snum] = new List<ShowRule>();
                        reader.Read();
                        while (reader.Name != "Rules")
                        {
                            if (reader.Name == "Rule")
                            {
                                this.SeasonRules[snum].Add(new ShowRule(reader.ReadSubtree()));
                                reader.Read();
                            }
                        }
                    }
                    reader.Read();
                }
                else if (reader.Name == "SeasonFolders")
                {
                    if (!reader.IsEmptyElement)
                    {
                        int snum = int.Parse(reader.GetAttribute("SeasonNumber"));
                        this.ManualFolderLocations[snum] = new List<String>();
                        reader.Read();
                        while (reader.Name != "SeasonFolders")
                        {
                            if ((reader.Name == "Folder") && reader.IsStartElement())
                            {
                                string ff = reader.GetAttribute("Location");
                                if (AutoFolderNameForSeason(snum) != ff)
                                    this.ManualFolderLocations[snum].Add(ff);
                            }
                            reader.Read();
                        }
                    }
                    reader.Read();
                }

                else
                    reader.ReadOuterXml();
            } // while
        }

        internal bool UsesManualFolders()
        {
            return this.ManualFolderLocations.Count>0;
        }

        public SeriesInfo TheSeries()
        {
            return TheTVDB.Instance.GetSeries(this.TVDBCode);
        }

        public string ShowName
        {
            get
            {
                if (this.UseCustomShowName)
                    return this.CustomShowName;
                SeriesInfo ser = TheSeries();
                if (ser != null)
                    return ser.Name;
                return "<" + this.TVDBCode + " not downloaded>";
            }
        }

        public List<String> getSimplifiedPossibleShowNames()
        {
            List<String> possibles = new List<String>();

            String simplifiedShowName = Helpers.SimplifyName(this.ShowName);
            if (!(simplifiedShowName == "")) { possibles.Add( simplifiedShowName); }

            //Check the custom show name too
            if (this.UseCustomShowName)
            {
                String simplifiedCustomShowName = Helpers.SimplifyName(this.CustomShowName);
                if (!(simplifiedCustomShowName == "")) { possibles.Add(simplifiedCustomShowName); }
            }

            //Also add the aliases provided
            possibles.AddRange(from alias in this.AliasNames select Helpers.SimplifyName(alias));

            return possibles;

        }

        public string ShowStatus
        {
            get{
                SeriesInfo ser = TheSeries();
                if (ser != null ) return ser.getStatus();
                return "Unknown";
            }
        }

        public enum ShowAirStatus
        {
            NoEpisodesOrSeasons,
            Aired,
            PartiallyAired,
            NoneAired
        }

        public ShowAirStatus SeasonsAirStatus
        {
            get
            {
                if (this.HasSeasonsAndEpisodes)
                {
                    if (this.HasAiredEpisodes && !this.HasUnairedEpisodes)
                    {
                        return ShowAirStatus.Aired;
                    }
                    else if (this.HasUnairedEpisodes && !this.HasAiredEpisodes)
                    {
                        return ShowAirStatus.NoneAired;
                    }
                    else if (this.HasAiredEpisodes && this.HasUnairedEpisodes)
                    {
                        return ShowAirStatus.PartiallyAired;
                    }
                    else
                    {
                        //System.Diagnostics.Debug.Assert(false, "That is weird ... we have 'seasons and episodes' but none are aired, nor unaired. That case shouldn't actually occur !");
                        return ShowAirStatus.NoEpisodesOrSeasons;
                    }
                }
                else
                {
                    return ShowAirStatus.NoEpisodesOrSeasons;
                }
            }
        }

        private bool HasSeasonsAndEpisodes
        {
            get {
                //We can use AiredSeasons as it does not matter which order we do this in Aired or DVD
                if (TheSeries() == null || TheSeries().AiredSeasons == null || TheSeries().AiredSeasons.Count <= 0)
                    return false;
                foreach (KeyValuePair<int, Season> s in TheSeries().AiredSeasons)
                {
                    if(this.IgnoreSeasons.Contains(s.Key))
                        continue;
                    if (s.Value.Episodes != null && s.Value.Episodes.Count > 0)
                    {
                        return true;
                    }
                }
                return false;
        }
        }

        private bool HasUnairedEpisodes
        {
            get
            {
                if (!this.HasSeasonsAndEpisodes) return false;

                foreach (KeyValuePair<int, Season> s in TheSeries().AiredSeasons)
                {
                    if (this.IgnoreSeasons.Contains(s.Key))
                        continue;
                    if (s.Value.Status(GetTimeZone()) == Season.SeasonStatus.NoneAired ||
                        s.Value.Status(GetTimeZone()) == Season.SeasonStatus.PartiallyAired)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private bool HasAiredEpisodes
        {
                get{
                    if (!this.HasSeasonsAndEpisodes) return false;

                    foreach (KeyValuePair<int, Season> s in TheSeries().AiredSeasons)
                    {
                        if(this.IgnoreSeasons.Contains(s.Key))
                            continue;
                        if (s.Value.Status(GetTimeZone()) == Season.SeasonStatus.PartiallyAired || s.Value.Status(GetTimeZone()) == Season.SeasonStatus.Aired)
                        {
                            return true;
                        }
                    }
                    return false;
        }
        }


        public string[] Genres => TheSeries()?.GetGenres();

        public void SetDefaults()
        {
            this.ManualFolderLocations = new Dictionary<int, List<string>>();
            this.IgnoreSeasons = new List<int>();
            this.UseCustomShowName = false;
            this.CustomShowName = "";
            this.UseSequentialMatch = false;
            this.SeasonRules = new Dictionary<int, List<ShowRule>>();
            this.SeasonEpisodes = new Dictionary<int, List<ProcessedEpisode>>();
            this.ShowNextAirdate = true;
            this.TVDBCode = -1;
            //                WhichSeasons = gcnew List<int>;
            //                NamingStyle = (int)NStyle.DefaultStyle();
            this.AutoAddNewSeasons = true;
            this.PadSeasonToTwoDigits = false;
            this.AutoAdd_FolderBase = "";
            this.AutoAdd_FolderPerSeason = true;
            this.AutoAdd_SeasonFolderName = "Season ";
            this.DoRename = true;
            this.DoMissingCheck = true;
            this.CountSpecials = false;
            this.DVDOrder = false;
            this.CustomSearchURL = "";
            this.UseCustomSearchURL = false;
            this.ForceCheckNoAirdate = false;
            this.ForceCheckFuture = false;
            this.BannersLastUpdatedOnDisk = null; //assume that the baners are old and have expired
            this.ShowTimeZone = TVRename.TimeZone.DefaultTimeZone(); // default, is correct for most shows

            this.LastFiguredTZ = "";

        }

        public List<ShowRule> RulesForSeason(int n)
        {
            return this.SeasonRules.ContainsKey(n) ? this.SeasonRules[n] : null;
        }

        public string AutoFolderNameForSeason(int n)
        {
            bool leadingZero = TVSettings.Instance.LeadingZeroOnSeason || this.PadSeasonToTwoDigits;
            string r = this.AutoAdd_FolderBase;
            if (string.IsNullOrEmpty(r))
                return "";

            if (!r.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                r += System.IO.Path.DirectorySeparatorChar.ToString();
            if (this.AutoAdd_FolderPerSeason)
            {
                if (n == 0)
                    r += TVSettings.Instance.SpecialsFolderName;
                else
                {
                    r += this.AutoAdd_SeasonFolderName;
                    if ((n < 10) && leadingZero)
                        r += "0";
                    r += n.ToString();
                }
            }
            return r;
        }

        public int MaxSeason()
        {
            int max = 0;
            foreach (KeyValuePair<int, List<ProcessedEpisode>> kvp in this.SeasonEpisodes)
            {
                if (kvp.Key > max)
                    max = kvp.Key;
            }
            return max;
        }

        //StringNiceName(int season)
        //{
        //    // something like "Simpsons (S3)"
        //    return String.Concat(ShowName," (S",season,")");
        //}

        public void WriteXMLSettings(XmlWriter writer)
        {
            writer.WriteStartElement("ShowItem");

            XMLHelper.WriteElementToXML(writer,"UseCustomShowName",this.UseCustomShowName);
            XMLHelper.WriteElementToXML(writer,"CustomShowName",this.CustomShowName);
            XMLHelper.WriteElementToXML(writer,"ShowNextAirdate",this.ShowNextAirdate);
            XMLHelper.WriteElementToXML(writer,"TVDBID",this.TVDBCode);
            XMLHelper.WriteElementToXML(writer,"AutoAddNewSeasons",this.AutoAddNewSeasons);
            XMLHelper.WriteElementToXML(writer,"FolderBase",this.AutoAdd_FolderBase);
            XMLHelper.WriteElementToXML(writer,"FolderPerSeason",this.AutoAdd_FolderPerSeason);
            XMLHelper.WriteElementToXML(writer,"SeasonFolderName",this.AutoAdd_SeasonFolderName);
            XMLHelper.WriteElementToXML(writer,"DoRename",this.DoRename);
            XMLHelper.WriteElementToXML(writer,"DoMissingCheck",this.DoMissingCheck);
            XMLHelper.WriteElementToXML(writer,"CountSpecials",this.CountSpecials);
            XMLHelper.WriteElementToXML(writer,"DVDOrder",this.DVDOrder);
            XMLHelper.WriteElementToXML(writer,"ForceCheckNoAirdate",this.ForceCheckNoAirdate);
            XMLHelper.WriteElementToXML(writer,"ForceCheckFuture",this.ForceCheckFuture);
            XMLHelper.WriteElementToXML(writer,"UseSequentialMatch",this.UseSequentialMatch);
            XMLHelper.WriteElementToXML(writer,"PadSeasonToTwoDigits",this.PadSeasonToTwoDigits);
            XMLHelper.WriteElementToXML(writer, "BannersLastUpdatedOnDisk", this.BannersLastUpdatedOnDisk);
            XMLHelper.WriteElementToXML(writer, "TimeZone", this.ShowTimeZone);


            writer.WriteStartElement("IgnoreSeasons");
            foreach (int i in this.IgnoreSeasons)
            {
                XMLHelper.WriteElementToXML(writer,"Ignore",i);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("AliasNames");
            foreach (string str in this.AliasNames)
            {
                XMLHelper.WriteElementToXML(writer,"Alias",str);
            }
            writer.WriteEndElement();

            XMLHelper.WriteElementToXML(writer, "UseCustomSearchURL", this.UseCustomSearchURL);
            XMLHelper.WriteElementToXML(writer, "CustomSearchURL",this.CustomSearchURL);

            foreach (KeyValuePair<int, List<ShowRule>> kvp in this.SeasonRules)
            {
                if (kvp.Value.Count > 0)
                {
                    writer.WriteStartElement("Rules");
                    XMLHelper.WriteAttributeToXML(writer ,"SeasonNumber",kvp.Key);

                    foreach (ShowRule r in kvp.Value)
                        r.WriteXML(writer);

                    writer.WriteEndElement(); // Rules
                }
            }
            foreach (KeyValuePair<int, List<String>> kvp in this.ManualFolderLocations)
            {
                if (kvp.Value.Count > 0)
                {
                    writer.WriteStartElement("SeasonFolders");

                    XMLHelper.WriteAttributeToXML(writer,"SeasonNumber",kvp.Key);

                    foreach (string s in kvp.Value)
                    {
                        writer.WriteStartElement("Folder");
                        XMLHelper.WriteAttributeToXML(writer,"Location",s);
                        writer.WriteEndElement(); // Folder
                    }

                    writer.WriteEndElement(); // Rules
                }
            }

            writer.WriteEndElement(); // ShowItem
        }

        public static List<ProcessedEpisode> ProcessedListFromEpisodes(List<Episode> el, ShowItem si)
        {
            List<ProcessedEpisode> pel = new List<ProcessedEpisode>();
            foreach (Episode e in el)
                pel.Add(new ProcessedEpisode(e, si));
            return pel;
        }

        public Dictionary<int, List<ProcessedEpisode>> GetDVDSeasons()
        {
            //We will create this on the fly
            Dictionary<int, List<ProcessedEpisode>> returnValue = new Dictionary<int, List<ProcessedEpisode>>();
            foreach (KeyValuePair<int, List<ProcessedEpisode>> kvp in this.SeasonEpisodes)
            {
                foreach (ProcessedEpisode ep in kvp.Value)
                {

                    if (!returnValue.ContainsKey(ep.DVDSeasonNumber ))
                    {
                        returnValue.Add(ep.DVDSeasonNumber, new List<ProcessedEpisode>());
                        
                    }
                    returnValue[ep.DVDSeasonNumber].Add(ep);
                }
            }

            return returnValue;
        }

        public Dictionary<int, List<string>> AllFolderLocations()
        {
            return AllFolderLocations( true);
        }

        public Dictionary<int, List<string>> AllFolderLocationsEpCheck(bool checkExist)
        {
            return AllFolderLocations(true, checkExist);
        }

        public Dictionary<int, List<string>> AllFolderLocations(bool manualToo,bool checkExist=true)
        {
            Dictionary<int, List<string>> fld = new Dictionary<int, List<string>>();

            if (manualToo)
            {
                foreach (KeyValuePair<int, List<string>> kvp in this.ManualFolderLocations)
                {
                    if (!fld.ContainsKey(kvp.Key))
                        fld[kvp.Key] = new List<String>();
                    foreach (string s in kvp.Value)
                        fld[kvp.Key].Add(s.TTS());
                }
            }

            if (this.AutoAddNewSeasons && (!string.IsNullOrEmpty(this.AutoAdd_FolderBase)))
            {
                int highestThereIs = -1;
                foreach (KeyValuePair<int, List<ProcessedEpisode>> kvp in this.SeasonEpisodes)
                {
                    if (kvp.Key > highestThereIs)
                        highestThereIs = kvp.Key;
                }
                foreach (int i in this.SeasonEpisodes.Keys)
                {
                    if (this.IgnoreSeasons.Contains(i)) continue;

                    string newName = AutoFolderNameForSeason(i);
                    if (string.IsNullOrEmpty(newName)) continue;

                    if (checkExist && !Directory.Exists(newName)) continue;

                    if (!fld.ContainsKey(i)) fld[i] = new List<string>();

                    if (!fld[i].Contains(newName)) fld[i].Add(newName.TTS());
                }
            }

            return fld;
        }

        public static int CompareShowItemNames(ShowItem one, ShowItem two)
        {
            string ones = one.ShowName; // + " " +one->SeasonNumber.ToString("D3");
            string twos = two.ShowName; // + " " +two->SeasonNumber.ToString("D3");
            return ones.CompareTo(twos);
        }

        public Season GetSeason(int snum)
        {
            return this.DVDOrder? TheSeries().DVDSeasons[snum]: TheSeries().AiredSeasons[snum];
        }
    }
}
