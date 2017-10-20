using Android.Accounts;
using Android.Content;
using Android.OS;



namespace spromotermobile.droid.sync
{
	public class SyncAdapter : AbstractThreadedSyncAdapter
	{
		readonly Context context;

		public SyncAdapter(Context context, bool autoInitialize) : base(context, autoInitialize)
		{
			this.context = context.ApplicationContext;
			GPS.GetGPSTracker(context);
#if !DEBUG
			HockeyApp.Android.CrashManager.Register(Context);
#endif
		}


		public SyncAdapter(Context context, bool autoInitialize, bool allowParallelSyncs) : base(context, autoInitialize, allowParallelSyncs)
		{
			this.context = context.ApplicationContext;
			GPS.GetGPSTracker(context);
#if !DEBUG
			HockeyApp.Android.CrashManager.Register(Context);
#endif
		}


		public override void OnPerformSync(Account account, Bundle extras, string authority, ContentProviderClient provider, SyncResult syncResult)
		{
			Sincronizador.context = context.ApplicationContext;
			Sincronizador.ExecSync();
		}


	}
}