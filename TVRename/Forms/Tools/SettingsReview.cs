using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using JetBrains.Annotations;

namespace TVRename.Forms
{
    public partial class SettingsReview : Form
    {
        private readonly List<SettingsCheck> set;
        private readonly TVDoc mDoc;
        private readonly UI mainUi;

        public SettingsReview([NotNull] TVDoc doc, UI main)
        {
            InitializeComponent();
            set = new List<SettingsCheck>();
            mDoc = doc;
            mainUi = main;
            Scan();
        }

        // ReSharper disable once InconsistentNaming
        private void UpdateUI()
        {
            olvDuplicates.SetObjects(set.Where(check => check.Check()));
            olvDuplicates.Sort(olvCheck);
        }

        private void AddRcMenuItem(string label, EventHandler command)
        {
            ToolStripMenuItem tsi = new ToolStripMenuItem(label);
            tsi.Click += command;
            possibleMergedEpisodeRightClickMenu.Items.Add(tsi);
        }

        private void PossibleMergedEpisodeRightClickMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            possibleMergedEpisodeRightClickMenu.Close();
        }

        private void BwScan_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker) sender;
            int total = mDoc.FilmLibrary.Count + mDoc.TvLibrary.Count;
            int current = 0;

            set.Clear();

            foreach (MovieConfiguration movie in mDoc.FilmLibrary.GetSortedMovies())
            {
                set.Add(new CustomLanguageMovieCheck(movie,mDoc));
                set.Add(new CustomNameMovieCheck(movie,mDoc));
                set.Add(new CustomRegionMovieCheck(movie,mDoc));
                set.Add(new ManualFoldersMovieCheck(movie,mDoc));

                set.Add(new DefaultDoMissingMovieCheck(movie,mDoc));
                set.Add(new DefaultDoRenameMovieCheck(movie,mDoc));

                set.Add(new FilenameMovieCheck(movie,mDoc));
                set.Add(new MovieProviderCheck(movie,mDoc));
                set.Add(new SubdirectoryMovieCheck(movie,mDoc));
                set.Add(new FolderBaseMovieCheck(movie,mDoc));
                
                bw.ReportProgress(100 * current++ / total, movie.ShowName);
            }

            foreach (ShowConfiguration show in mDoc.TvLibrary.GetSortedShowItems())
            {
                set.Add(new CustomLanguageTvShowCheck(show,mDoc));
                set.Add(new CustomNameTvShowCheck(show,mDoc));
                set.Add(new CustomRegionTvShowCheck(show,mDoc));
                set.Add(new CustomSearchTvShowCheck(show,mDoc));
                set.Add(new UseManualFoldersTvShowCheck(show,mDoc));

                set.Add(new DefaultAirDateMatchingTvCheck(show,mDoc));
                set.Add(new DefaultDoMissingTvCheck(show,mDoc));
                set.Add(new DefaultDoRenameTvCheck(show,mDoc));
                set.Add(new DefaultEpisodeNameMatchingTvCheck(show,mDoc));
                set.Add(new DefaultFutureEpisodesTvCheck(show,mDoc));
                set.Add(new DefaultNoAirdateTvCheck(show,mDoc));
                set.Add(new DefaultSequentialMatchingTvCheck(show,mDoc));
                set.Add(new DefaultShowAirDateTvCheck(show,mDoc));
                set.Add(new DefaultSpecialsAsEpisodesTvCheck(show,mDoc));
                set.Add(new DefaultUseDvdTvCheck(show,mDoc));

                set.Add(new TvShowEpisodeNameCheck(show,mDoc));
                set.Add(new TvShowProviderCheck(show,mDoc));
                set.Add(new TvShowSeasonFormatCheck(show,mDoc));
                set.Add(new FolderBaseTvCheck(show, mDoc));

                bw.ReportProgress(100 * current++ / total, show.ShowName);
            }
        }

        private void BwScan_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbProgress.Value = e.ProgressPercentage;
            lblStatus.Text = e.UserState.ToString();
        }

        private void BwScan_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnRefresh.Visible = true;
            pbProgress.Visible = false;
            lblStatus.Visible = false;
            if (olvDuplicates.IsDisposed)
            {
                return;
            }

            UpdateUI();
        }
        private void BtnRefresh_Click_1(object sender, EventArgs e)
        {
            Scan();
        }

        private void Scan()
        {
            btnRefresh.Visible = false;
            pbProgress.Visible = true;
            lblStatus.Visible = true;
            bwScan.RunWorkerAsync();
        }

        private void olvDuplicates_CellRightClick(object sender, CellRightClickEventArgs e)
        {
            if (e.Model is null)
            {
                return;
            }

            possibleMergedEpisodeRightClickMenu.Items.Clear();
            if (olvDuplicates.SelectedObjects.Count == 1)
            {
                SettingsCheck mlastSelected = (SettingsCheck) e.Model;

                if (mlastSelected is MovieCheck mcheck)
                {
                    MovieConfiguration si = mcheck.Movie;
                    AddRcMenuItem("Force Refresh",
                        (o, args) => mainUi.ForceMovieRefresh(new List<MovieConfiguration> {si}, false));

                    AddRcMenuItem("Edit Movie", (o, args) => mainUi.EditMovie(si));

                    possibleMergedEpisodeRightClickMenu.Items.Add(new ToolStripSeparator());
                    foreach (string? f in si.Locations)
                    {
                        AddRcMenuItem("Visit " + f, (o, args) => Helpers.OpenFolder(f));
                    }
                }
                else if (mlastSelected is TvShowCheck tcheck)
                {
                    ShowConfiguration si = tcheck.Show;
                    AddRcMenuItem("Force Refresh",
                        (o, args) => mainUi.ForceRefresh(new List<ShowConfiguration> {si}, false));

                    AddRcMenuItem("Edit TV Show", (o, args) => mainUi.EditShow(si));
                }

                possibleMergedEpisodeRightClickMenu.Items.Add(new ToolStripSeparator());
                AddRcMenuItem("Fix Issue", (o, args) => Remedy(mlastSelected));
            }
            else
            {
                AddRcMenuItem("Fix Issues", (o, args) =>
                {
                    foreach (SettingsCheck? selected in olvDuplicates.SelectedObjects.OfType<SettingsCheck>())
                    {
                        Remedy(selected);
                    }
                    mDoc.SetDirty();
                });
            }
        }

        private void Remedy(SettingsCheck selected)
        {
            selected.Fix();
            if (!selected.IsError)
            {
                olvDuplicates.RemoveObject(selected);
                set.Remove(selected);
            }
        }
    }
}
