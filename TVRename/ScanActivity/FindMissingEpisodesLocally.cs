namespace TVRename
{
    internal class FindMissingEpisodesLocally : FindMissingEpisodes
    {
        public FindMissingEpisodesLocally(TVDoc doc) : base(doc)
        {
        }

        protected override string CheckName() => "Looked local filesystem for the missing files";

        protected override Finder.FinderDisplayType CurrentType() => Finder.FinderDisplayType.local;
    }
}