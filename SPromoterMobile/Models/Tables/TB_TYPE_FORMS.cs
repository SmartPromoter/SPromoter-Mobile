using SQLite;

namespace SPromoterMobile.Models.Tables
{
    [Table("TB_TYPE_FORMS")]
    [Preserve(AllMembers =true)] 
	public class TB_TYPE_FORMS
	{
		[PrimaryKey]
		public string ID { get; set; }
		[NotNull]
		public string FORM { get; set; }
	}
}
