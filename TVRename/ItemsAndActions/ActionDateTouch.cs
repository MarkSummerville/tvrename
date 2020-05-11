// 
// Main website for TVRename is http://tvrename.com
// 
// Source code available at https://github.com/TV-Rename/tvrename
// 
// Copyright (c) TV Rename. This code is released under GPLv3 https://github.com/TV-Rename/tvrename/blob/master/LICENSE.md
// 

using Alphaleonis.Win32.Filesystem;
using JetBrains.Annotations;

namespace TVRename
{
    using System;

    public class ActionDateTouch : ActionFileMetaData
    {
        private readonly ShowItem show; // if for an entire show, rather than specific episode
        private readonly ProcessedSeason processedSeason; // if for an entire show, rather than specific episode
        private readonly FileInfo whereFile;
        private readonly DirectoryInfo whereDirectory;
        private readonly DateTime updateTime;

        public ActionDateTouch(FileInfo f, ProcessedEpisode pe, DateTime date)
        {
            Episode = pe;
            whereFile = f;
            updateTime = date;
        }

        public ActionDateTouch(DirectoryInfo dir, ProcessedSeason sn, DateTime date)
        {
            processedSeason = sn;
            whereDirectory = dir;
            updateTime = date;
        }

        public ActionDateTouch(DirectoryInfo dir, ShowItem si, DateTime date)
        {
            show = si;
            whereDirectory = dir;
            updateTime = date;
        }

        [CanBeNull]
        public override string Produces => whereFile?.FullName?? whereDirectory?.FullName;

        #region Action Members

        [NotNull]
        public override string Name => "Update Timestamp";

        [CanBeNull]
        public override string ProgressText => whereFile?.Name??whereDirectory?.Name;

        public override long SizeOfWork => 100;

        [NotNull]
        public override ActionOutcome Go(TVRenameStats stats)
        {
            try
            {
                if (whereFile != null)
                {
                    ProcessFile(whereFile, updateTime);
                }

                if (whereDirectory != null)
                {
                    System.IO.Directory.SetLastWriteTimeUtc(whereDirectory.FullName, updateTime);
                }
            }
            catch (UnauthorizedAccessException uae)
            {
                return new ActionOutcome(uae);
            }
            catch (Exception e)
            {
                return new ActionOutcome(e);
            }

            return ActionOutcome.Success();
        }

        private static void ProcessFile([NotNull] FileInfo whereFile, DateTime updateTime)
        {
            bool priorFileReadonly = whereFile.IsReadOnly;
            if (priorFileReadonly)
            {
                whereFile.IsReadOnly = false;
            }

            File.SetLastWriteTimeUtc(whereFile.FullName, updateTime);
            if (priorFileReadonly)
            {
                whereFile.IsReadOnly = true;
            }
        }

        #endregion

        #region Item Members

        public override bool SameAs(Item o)
        {
            return o is ActionDateTouch touch && touch.whereFile == whereFile && touch.whereDirectory == whereDirectory;
        }

        public override int CompareTo(object o)
        {
            ActionDateTouch nfo = o as ActionDateTouch;

            if (Episode is null)
            {
                return 1;
            }

            if (nfo?.Episode is null)
            {
                return -1;
            }

            if (whereFile != null)
            {
                return string.Compare(whereFile.FullName + Episode.Name, nfo.whereFile.FullName + nfo.Episode.Name, StringComparison.Ordinal);
            }

            return string.Compare(whereDirectory.FullName + Episode.Name, nfo.whereDirectory.FullName + nfo.Episode.Name, StringComparison.Ordinal);
        }

        #endregion

        #region Item Members

        [CanBeNull]
        public override IgnoreItem Ignore => whereFile is null ? null : new IgnoreItem(whereFile.FullName);

        public override string SeriesName => Episode != null ? Episode.Show.ShowName :
            processedSeason != null ? processedSeason.Show.ShowName : show.ShowName;
        public override string SeasonNumber => Episode != null ? Episode.AppropriateSeasonNumber.ToString() :
            processedSeason != null ? processedSeason.SeasonNumber.ToString() : string.Empty;
        public override string AirDateString =>
            updateTime.CompareTo(DateTime.MaxValue) != 0 ? updateTime.ToShortDateString() : "";
        [CanBeNull]
        public override string DestinationFolder => whereFile?.DirectoryName ?? whereDirectory?.FullName;
        [CanBeNull]
        public override string DestinationFile => whereFile?.Name ?? whereDirectory?.Name;
        [CanBeNull]
        public override string TargetFolder => whereFile?.DirectoryName??whereDirectory?.Name;
        [NotNull]
        public override string ScanListViewGroup => "lvgUpdateFileDates";
        public override int IconNumber => 7;

        #endregion
    }
}
