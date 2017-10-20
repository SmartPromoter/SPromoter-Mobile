using SQLite;

namespace SPromoterMobile.Models.Tables
{
    [Table("TB_TAREFAS")]
    [Preserve(AllMembers =true)] 
    public class TB_TAREFAS
    {        
		public int tipo { get; set; }
        [NotNull]
        public string FORMULARIO { get; set; }
        [NotNull]
		public string VISITA_ID { get; set; }
        [NotNull]
		public string PRODUTO_ID { get; set; }
        [NotNull]
		public string ID_FORM_SCHEMA { get; set; }
        [NotNull, PrimaryKey]
		public string ID_SERVER_FORM { get; set; }
		public double LAT { get; set; }
		public double LONG { get; set; }
        public int BATERIA { get; set; }
		public int STATUS { get; set; }
		public int STATUSCACHE { get; set; }
    }
}
