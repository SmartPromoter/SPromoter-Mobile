using Android.App;
using Android.Content;
using Android.OS;

namespace spromotermobile.droid.sync
{

	[Service(Name = "spromotermobile.droid.sync.AuthenticatorService", Exported = false)]
	[IntentFilter(new string[] { "android.accounts.AccountAuthenticator" })]
	[MetaData("android.accounts.AccountAuthenticator", Resource = "@xml/authenticator")]
	public class AuthenticatorService : Service
	{

		Authenticator mAuthenticator;

		public override void OnCreate()
		{
			mAuthenticator = new Authenticator(this);
		}


		public override IBinder OnBind(Intent intent)
		{
			return mAuthenticator.IBinder;
		}
	}
}