using Android.App;
using Android.Content;
using Android.OS;

namespace spromotermobile.droid.sync
{
	[Service(Name = "spromotermobile.droid.sync.SyncService", Exported = false)]
	[IntentFilter(new string[] { "android.content.SyncAdapter" })]
	[MetaData("android.content.SyncAdapter", Resource = "@xml/syncadapter")]
	public class SyncService : Service
	{
		static readonly object sSyncAdapterLock = new object();
		static SyncAdapter sSyncAdapter;


		public override void OnCreate()
		{
			lock (sSyncAdapterLock)
			{
				if (sSyncAdapter == null)
				{
					sSyncAdapter = new SyncAdapter(ApplicationContext, true);
				}
			}
		}

		public override IBinder OnBind(Intent intent)
		{
			return sSyncAdapter.SyncAdapterBinder;
		}


	}
}