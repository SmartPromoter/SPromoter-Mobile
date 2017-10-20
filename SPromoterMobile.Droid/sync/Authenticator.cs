using Android.Content;
using Android.OS;
using Android.Accounts;
using Java.Lang;

namespace spromotermobile.droid.sync
{
	public class Authenticator : AbstractAccountAuthenticator
	{

		public Authenticator(Context context) : base(context) { }

		public override Bundle AddAccount(AccountAuthenticatorResponse response, string accountType, string authTokenType, string[] requiredFeatures, Bundle options)
		{
			return null;
		}

		public override Bundle ConfirmCredentials(AccountAuthenticatorResponse response, Account account, Bundle options)
		{
			return null;
		}

		public override Bundle EditProperties(AccountAuthenticatorResponse response, string accountType)
		{
			throw new UnsupportedOperationException();
		}

		public override Bundle GetAuthToken(AccountAuthenticatorResponse response, Account account, string authTokenType, Bundle options)
		{
			return null;
		}

		public override string GetAuthTokenLabel(string authTokenType)
		{
			throw new UnsupportedOperationException();
		}

		public override Bundle HasFeatures(AccountAuthenticatorResponse response, Account account, string[] features)
		{
			return null;
		}

		public override Bundle UpdateCredentials(AccountAuthenticatorResponse response, Account account, string authTokenType, Bundle options)
		{
			return null;
		}
	}
}