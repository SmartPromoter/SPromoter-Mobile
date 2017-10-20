
using SQLite;

namespace SPromoterMobile.Models.Tables
{
    [Table("TB_PRODUTO")]
    [Preserve(AllMembers =true)] 
    public class TB_PRODUTO
    {
        [PrimaryKey]
        public string ID { get; set; }
        [NotNull]
        public string NOME { get; set; }
        [NotNull]
        public string CATEGORIA { get; set; }
    }
}
