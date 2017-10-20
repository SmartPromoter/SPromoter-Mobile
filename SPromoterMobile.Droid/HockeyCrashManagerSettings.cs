//
//  HockeyCrashManagerSettings.cs
//
//  Author:
//       Leonard Colusso  <leonardcolusso@smartpromoter.trade>
//
//  Copyright (c) 2016 SmartPromoter
//
using HockeyApp.Android;

namespace spromotermobile.droid
{
	public class HockeyCrashManagerSettings : CrashManagerListener
	{
		readonly string IDRegistryUser;
		readonly string IDRegistryServer;

		public HockeyCrashManagerSettings(string nameUser, string server) {
			if (string.IsNullOrEmpty(nameUser)) {
				IDRegistryUser = "NullUser";
			}
			else {
				IDRegistryUser = nameUser;
			}
			if (string.IsNullOrEmpty(server))
			{
				IDRegistryServer = "NullServer";
			}
			else {
				IDRegistryServer = server;
			}
		}


		public HockeyCrashManagerSettings()
		{
			IDRegistryUser = "NullUser";
			IDRegistryServer = "NullServer";
		}

		public override bool ShouldAutoUploadCrashes()
		{
			return true;
		}

		public override string Contact
		{
			get
			{
				return IDRegistryServer;
			}
		}


		public override string UserID
		{
			get
			{
				return IDRegistryUser;
			}
		}

	}
}
