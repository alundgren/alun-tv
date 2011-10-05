using System;

namespace AlunTv.Test
{
    public class ShowInfoCache : ShowInfo
    {
        public static string IdFromSourceId(string sourceId)
        {
            return String.Format("SourceShowInfoCaches/{0}", sourceId);
        }

        public override string SourceId
        {
            get { return Id.Substring("SourceShowInfoCaches/".Length); }
        }
    }
}