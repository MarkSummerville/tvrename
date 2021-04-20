using System;
using JetBrains.Annotations;

namespace TVRename
{
    internal class FolderBaseMovieCheck : MovieCheck
    {
        public FolderBaseMovieCheck([NotNull] MovieConfiguration movie) : base(movie) { }

        public override bool Check() => Movie.UseAutomaticFolders && !Movie.AutomaticFolderRoot.HasValue();

        public override string Explain() => "This Movie does not have an automatic folder base specified.";

        protected override void FixInternal()
        {
            //Movie.AutomaticFolderRoot = TVSettings.Instance.MovieLibraryFolders[1];
            //return;
            throw new NotImplementedException(); //TODO
        }

        public override string CheckName => "[Movie] Use Default folder supplied";
    }
}
