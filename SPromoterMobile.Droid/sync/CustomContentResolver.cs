using System;
using Android.Content;
using Android.OS;
using Android.Accounts;

namespace spromotermobile.droid.sync
{
	public class CustomContentResolver
	{
		static readonly string AUTHORITY = "spromotermobile.droid.sync.StubProvider";
		static readonly string ACCOUNT_TYPE = "spromotermobile.droid.sync.SyncService";
		static readonly string ACCOUNT = "SMARTPROMOTER";
		static readonly long SECONDS_PER_MINUTE = 60L;
		static readonly long SYNC_INTERVAL_IN_MINUTES = 1L;
		static readonly long SYNC_INTERVAL = SYNC_INTERVAL_IN_MINUTES * SECONDS_PER_MINUTE;
		static Account mAccount;
		static CustomContentResolver instance;

		CustomContentResolver(Context context)
		{
			if (mAccount == null)
			{
				mAccount = CreateSyncAccount(context);
			}
			ContentResolver.AddPeriodicSync(mAccount, AUTHORITY, Bundle.Empty, SYNC_INTERVAL);
			ContentResolver.SetSyncAutomatically(mAccount, AUTHORITY, true);
			ContentResolver.MasterSyncAutomatically = true;
		}


		public static CustomContentResolver GetCustomContentResolver(Context context)
		{
			if (instance == null)
				instance = new CustomContentResolver(context.ApplicationContext);
			return instance;
		}

		static Account CreateSyncAccount(Context context)
		{
			var newAccount = new Account(ACCOUNT, ACCOUNT_TYPE);
			var accountManager = (AccountManager)context.GetSystemService(Context.AccountService);
			accountManager.AddAccountExplicitly(newAccount, null, null);
			return newAccount;
		}

		public void Sync(Context con)
		{
			try
			{

				var am = AccountManager.Get(con.ApplicationContext);
				Account account = am.GetAccountsByType(ACCOUNT_TYPE)[0];
				var isYourAccountSyncEnabled = ContentResolver.GetSyncAutomatically(account, AUTHORITY);
				var isMasterSyncEnabled = ContentResolver.MasterSyncAutomatically;
				if (!isYourAccountSyncEnabled && isMasterSyncEnabled)
				{
					instance = new CustomContentResolver(con.ApplicationContext);
				}
			}
			catch (Exception)
			{
				mAccount = null;
				instance = new CustomContentResolver(con.ApplicationContext);

			}
			finally
			{
				ContentResolver.SetIsSyncable(mAccount, AUTHORITY, 1);
				var data = new Bundle();
				data.PutBoolean(ContentResolver.SyncExtrasManual, true);
				data.PutBoolean(ContentResolver.SyncExtrasExpedited, true);
				ContentResolver.RequestSync(mAccount, AUTHORITY, data);
			}

		}


		public bool IsRunning()
		{
			return ContentResolver.IsSyncPending(mAccount, AUTHORITY) || ContentResolver.IsSyncActive(mAccount, AUTHORITY);
		}

		public bool IsPending()
		{
			return ContentResolver.IsSyncPending(mAccount, AUTHORITY);
		}

	}

}