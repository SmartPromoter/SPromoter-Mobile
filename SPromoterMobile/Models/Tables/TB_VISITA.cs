using SQLite;

namespace SPromoterMobile.Models.Tables
{
    [Table("TB_VISITA")]
    [Preserve(AllMembers =true)] 
    public class TB_VISITA
    {
		[PrimaryKey]
        public string ID { get; set; }
        [NotNull]
        public string DATA_PROGAMADA { get; set; }
		public int STATUS { get; set; }
        public string INICIO { get; set; }
        public string FIM { get; set; }
		public string JUSTIFICATIVA { get; set; }
		public bool NOTIFICADO { get; set; }
        [NotNull]
        public string ENDERECO { get; set; }
        public double LAT_PDV { get; set; }
        public double LONG_PDV { get; set; }
		public double LAT { get; set; }
		public double LONG { get; set; }
        public int BATERIA { get; set; }
        [NotNull]
		public string ID_USER_RELACIONADO { get; set; }
    }
}
