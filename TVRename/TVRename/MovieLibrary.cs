using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace TVRename
{
    public class MovieLibrary : SafeList<MovieConfiguration>
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        [NotNull]
        public IEnumerable<MovieConfiguration> Movies => this;

        [NotNull]
        public IEnumerable<SeriesSpecifier> SeriesSpecifiers
        {
            get
            {
                return this.Select(series => new SeriesSpecifier(series.TvdbCode, series.TVmazeCode,series.TmdbCode, series.UseCustomLanguage, series.CustomLanguageCode, series.ShowName, series.Provider, series.CachedMovie?.Imdb , MediaConfiguration.MediaType.movie)).ToList();
            }
        }

        public List<(int, string)> Collections => Movies
            .Select(c => (c.CachedMovie?.CollectionId, c.CachedMovie?.CollectionName))
            .Where(a => a.CollectionId.HasValue && a.CollectionName.HasValue())
            .Select(a => (a.CollectionId.Value, a.CollectionName)).Distinct().ToList();

        [NotNull]
        public IEnumerable<string> GetGenres()
        {
            List<string> allGenres = new List<string>();
            foreach (MovieConfiguration si in Movies)
            {
                allGenres.AddRange(si.Genres);
            }

            List<string> distinctGenres = allGenres.Distinct().ToList();
            distinctGenres.Sort();
            return distinctGenres;
        }

        public MovieConfiguration? GetMovie(int id, TVDoc.ProviderType provider)
        {
            List<MovieConfiguration>? matching = this.Where(configuration => configuration.IdCode(provider) == id).ToList();

            if (!matching.Any())
            {
                return null;
            }
            if (matching.Count == 1)
            {
                return matching.First();
            }
            Logger.Error($"Movie Library has multiple: {matching.Select(x => x.ToString()).ToCsv()}");
            return matching.First();
        }
        
        /*
        internal void Add([NotNull] MovieConfiguration found)
        {
            if (found.Code == -1)
            {
                return;
            }

            if (TryAdd(found.Code, found))
            {
                return;
            }

            if (ContainsKey(found.Code))
            {
                Logger.Warn($"Failed to Add {found.ShowName} with {found.SourceProviderName}={found.Code} to library, but it's already present");
            }
            else
            {
                Logger.Error($"Failed to Add {found.ShowName} with {found.SourceProviderName}={found.Code} to library");
            }
        }

        internal void Remove([NotNull] MovieConfiguration si)
        {
            if (!TryRemove(si.TmdbCode, out _))
            {
                Logger.Error($"Failed to remove {si.ShowName} from the library with TMDBCode={si.TmdbCode}");
            }
        }
        */

        public void LoadFromXml(XElement? xmlSettings)
        {
            if (xmlSettings != null)
            {
                foreach (MovieConfiguration si in xmlSettings.Descendants("MovieItem").Select(showSettings => new MovieConfiguration(showSettings)))
                {
                    // if (si.UseCustomShowName) // see if custom show name is actually the real show name
                    // {
                    //     CachedSeriesInfo ser = si.TheSeries();
                    //     if (ser != null && si.CustomShowName == ser.Name)
                    //     {
                    //         // then, turn it off
                    //         si.CustomShowName = string.Empty;
                    //         si.UseCustomShowName = false;
                    //     }
                    // }

                    Add(si);
                }
            }
        }

        public List<MovieConfiguration> GetSortedMovies()
        {
            List<MovieConfiguration> returnList = Movies.ToList();
            returnList.Sort(MediaConfiguration.CompareNames);
            return returnList;
        }

        [NotNull]
        public IEnumerable<string> GetNetworks()
        {
            return Movies
                .Select(si => si.CachedMovie)
                .Where(seriesInfo => !string.IsNullOrWhiteSpace(seriesInfo?.Network))
                .Select(seriesInfo => seriesInfo.Network)
                .Distinct()
                .OrderBy(s => s);
        }

        [NotNull]
        public IEnumerable<string> GetContentRatings()
        {
            return Movies.Select(si => si.CachedMovie)
                .Where(s => !string.IsNullOrWhiteSpace(s?.ContentRating))
                .Select(s => s.ContentRating)
                .Distinct()
                .OrderBy(s => s);
        }

        [NotNull]
        public IEnumerable<string> GetYears()
        {
            return Movies.Select(si => si.CachedMovie?.Year)
                .Where(s => s.HasValue)
                .Select(s => s!.ToString())
                .Distinct()
                .OrderBy(s => s);
        }

        [NotNull]
        public IEnumerable<string> GetStatuses()
        {
            return Movies
                .Select(s=>s.CachedMovie)
                .Where(s => !string.IsNullOrWhiteSpace(s?.Status))
                .Select(s => s.Status)
                .Distinct()
                .OrderBy(s => s);
        }

        public MovieConfiguration? GetMovie(PossibleNewMovie ai)
        {
            return GetMovie(ai.ProviderCode,ai.Provider);
        }

        public void AddRange(IEnumerable<MovieConfiguration>? addedShows)
        {
            if (addedShows is null)
            {
                return;
            }

            foreach (MovieConfiguration show in addedShows)
            {
                Add(show);
            }
        }
    }
}
