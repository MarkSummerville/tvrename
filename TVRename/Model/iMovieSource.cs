using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;

namespace TVRename
{
    // ReSharper disable once InconsistentNaming
    public interface iMovieSource
    {
        void Setup(FileInfo loadFrom, FileInfo cacheFile, CommandLineArgs args);
        bool Connect(bool showErrorMsgBox);
        void SaveCache();
        bool EnsureUpdated(SeriesSpecifier s, bool bannersToo, bool showErrorMsgBox);
        void UpdatesDoneOk();

        CachedMovieInfo GetMovie(PossibleNewMovie show, string languageCode, bool showErrorMsgBox);
        CachedMovieInfo GetMovie(int? id);
        bool HasMovie(int id);

        void Tidy(IEnumerable<MovieConfiguration> libraryValues);

        void ForgetEverything();
        void ForgetMovie(int id);
        void ForgetMovie(int tvdb, int tvmaze,int tmdb, bool makePlaceholder, bool useCustomLanguage, string langCode);
        void Update(CachedMovieInfo si);
        void AddPoster(int seriesId, IEnumerable<Banner> select);
        void LatestUpdateTimeIs(string time);
    }
}
