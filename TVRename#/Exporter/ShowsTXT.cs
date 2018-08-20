using System;
using System.Collections.Generic;

namespace TVRename
{
    // ReSharper disable once InconsistentNaming
    internal class ShowsTXT : ShowsExporter
    {
        public ShowsTXT(List<ShowItem> shows) : base(shows)
        {
        }

        public override bool Active() =>TVSettings.Instance.ExportShowsTXT;
        protected override string Location() =>TVSettings.Instance.ExportShowsTXTTo;

        public override void Run()
        {
            if (!Active()) return;

            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Location()))
                {
                    foreach (ShowItem si in this.Shows)
                    {
                        file.WriteLine(si.ShowName);
                    }

                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }
}
