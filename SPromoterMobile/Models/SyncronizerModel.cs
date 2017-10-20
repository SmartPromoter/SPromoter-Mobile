using SPromoterMobile.Data;

namespace SPromoterMobile.Models
{
    [Preserve(AllMembers =true)] 
    public class SyncronizerModel
    {
        public SyncronizerDA db;
		public string versionName;
        public CacheDA dbCache;
    }
}
