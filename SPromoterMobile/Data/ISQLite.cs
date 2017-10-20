using SPromoterMobile.Models;
using SQLite;

namespace SPromoterMobile.Data
{
    [Preserve(AllMembers =true)] 
	public interface ISQLite
	{
		SQLiteAsyncConnection GetConnection();
	}
}

