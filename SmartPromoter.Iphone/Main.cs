using SPromoterMobile.Data;
using UIKit;

namespace SmartPromoter.Iphone
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main(string[] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
 			new SQLHelper().InitTables(Sqlite_IOS.DB.dataBase); 
			UIApplication.Main(args, null, "AppDelegate");
		}
	}
}
