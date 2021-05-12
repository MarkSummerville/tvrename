// 
// Main website for TVRename is http://tvrename.com
// 
// Source code available at https://github.com/TV-Rename/tvrename
// 
// Copyright (c) TV Rename. This code is released under GPLv3 https://github.com/TV-Rename/tvrename/blob/master/LICENSE.mdr
//
// For more information see http://thetvdb.com/wiki/index.php/API:banners.xml
//  

using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace TVRename
{
    public class Banner
    {
        public int BannerId;
        public int LanguageId;
        public string? BannerPath;
        public string? BannerType;
        private string? resolution;
        public double Rating;
        public int RatingCount;
        public int SeasonId;
        public int SeriesId;
        private string? thumbnailPath;

        public Banner(int seriesId, [NotNull] XElement r)
        {
            // <Banner>
            //        <id>708811</id>
            //        <BannerPath>seasonswide/79488-5.jpg</BannerPath>
            //        <BannerType>season</BannerType>
            //        <BannerType2>seasonwide</BannerType2>
            //        <Language>en</Language>
            //        <Rating/>
            //        <RatingCount>0</RatingCount>
            //        <Season>5</Season>
            //  blah blah
            // </Banner>
                SetDefaults();

                BannerId = r.ExtractInt("id")??-1;
                SeriesId = r.ExtractInt("seriesid")?? seriesId; // thetvdb cachedSeries id
                SeasonId = r.ExtractInt("seasonid",-1);
                BannerPath = XmlHelper.ReadStringFixQuotesAndSpaces(r.ExtractString("BannerPath"));
                BannerType = r.ExtractString("BannerType");
                LanguageId = r.ExtractInt("LanguageId",-1);
                resolution = r.ExtractString("Resolution");
                string sn = r.ExtractString("Rating");
                double.TryParse(sn, out Rating);
                RatingCount  = r.ExtractInt("RatingCount",-1);
                SeasonId = r.ExtractInt("Season",-1);
                thumbnailPath = r.ExtractString("ThumbnailPath");
        }

        public Banner(int seriesId, [NotNull] JObject json, int langId)
        {
            SetDefaults();
            // {
            //  "fileName": "string",
            //  "id": 0,
            //  "keyType": "string",
            //  "languageId": 0,
            //  "ratingsInfo": {
            //      "average": 0,
            //      "count": 0
            //      },
            //  "resolution": "string",
            //  "subKey": "string",         //May Contain Season Number
            //  "thumbnail": "string"
            //  }

            SeriesId = seriesId;

            BannerPath = (string)json["fileName"];
            BannerId = (int)json["id"];
            BannerType = (string)json["keyType"];
            LanguageId = json["languageId"] is null ? langId  : (int)json["languageId"];
            
            double.TryParse((string)json["ratingsInfo"]?["average"], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.CreateSpecificCulture("en-US"), out Rating);
            RatingCount = (int)json["ratingsInfo"]?["count"];

            resolution = (string)json["resolution"];
            int.TryParse((string)json["subKey"], out SeasonId);
            thumbnailPath = (string)json["thumbnail"];
        }

        public Banner(int seriesId)
        {
            SeriesId = seriesId;
        }

        public bool SameAs([NotNull] Banner  o) => BannerId == o.BannerId;

        public bool IsSeriesPoster() => BannerType == "poster";

        public bool IsSeriesBanner() => BannerType == "series";

        public bool IsSeasonPoster() => BannerType == "season";

        public bool IsSeasonBanner() => BannerType == "seasonwide";

        public bool IsFanart() => BannerType == "fanart";

        private void SetDefaults()
        {
            BannerId = -1;
            BannerPath = string.Empty;
            BannerType = string.Empty;
            LanguageId = -1;
            resolution = string.Empty;
            Rating = -1;
            RatingCount = 0;
            SeasonId = -1;
            SeriesId = -1;

            thumbnailPath = string.Empty;
        }

        public void WriteXml([NotNull] XmlWriter writer)
        {
            // <Banner>
            //        <id>708811</id>
            //        <BannerPath>seasonswide/79488-5.jpg</BannerPath>
            //        <BannerType>season</BannerType>
            //        <BannerType2>seasonwide</BannerType2>
            //        <Language>en</Language>
            //        <Rating/>
            //        <RatingCount>0</RatingCount>
            //        <Season>5</Season>
            //  blah blah
            // </Banner>

            writer.WriteStartElement("Banner");

            writer.WriteElement("id",BannerId);
            writer.WriteElement("BannerPath",BannerPath);
            writer.WriteElement("BannerType",BannerType);
            writer.WriteElement("LanguageId", LanguageId);
            writer.WriteElement("Resolution",resolution);
            writer.WriteElement("Rating",Rating);
            writer.WriteElement("RatingCount",RatingCount);
            writer.WriteElement("Season",SeasonId);  
            writer.WriteElement("ThumbnailPath",thumbnailPath);

            writer.WriteEndElement(); //Banner
        }
    }
}
