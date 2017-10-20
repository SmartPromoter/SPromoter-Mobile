
using SQLite;

namespace SPromoterMobile.Models.Tables
{
    [Table("TB_CACHE")]
    [Preserve(AllMembers =true)] 
    public class TB_CACHE
    {
        [NotNull]
        public int ID { get; set; }
        public string CACHE { get; set; }
        public string CACHESYNC { get; set; }
        [NotNull]
        public string ID_USER_LOGGED { get; set; }
        [PrimaryKey, AutoIncrement]
        public int INDEX { get; set; }
	}
}
