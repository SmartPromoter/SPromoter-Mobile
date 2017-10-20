using SQLite;

namespace SPromoterMobile.Models.Tables
{
    [Table("TB_USUARIO")]
    [Preserve(AllMembers =true)] 
    public class TB_USUARIO
    {
        [PrimaryKey, Indexed]
        public string ID { get; set; }
        [NotNull]
        public string SERVIDOR { get; set; }
        [NotNull]
        public string LOGIN { get; set; }
        [NotNull]
        public string SENHA { get; set; }
        [NotNull]
        public string NOME { get; set; }
        [NotNull]
        public string CARGO { get; set; }

        public string AVATAR { get; set; }
		public int AVATAR_STATUS { get; set; }
        public bool ATIVO { get; set; }
		public string CHK_IN { get; set; } 
        public string CHK_OUT { get; set; }
        public string ALMOCO_INICIO { get; set; }
        public string ALMOCO_FIM { get; set; }
	}
}
