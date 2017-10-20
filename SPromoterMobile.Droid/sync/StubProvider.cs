using Android.Content;
using Android.Database;

namespace spromotermobile.droid.sync
{

	[ContentProvider(new string[] { "spromotermobile.droid.sync.StubProvider" }, Syncable = true, Exported = false, Name = "spromotermobile.droid.sync.StubProvider")]
	public class StubProvider : ContentProvider
	{
		public override int Delete(Android.Net.Uri uri, string selection, string[] selectionArgs)
		{
			return 0;
		}

		public override string GetType(Android.Net.Uri uri)
		{
			return null;
		}

		public override Android.Net.Uri Insert(Android.Net.Uri uri, ContentValues values)
		{
			return null;
		}

		public override bool OnCreate()
		{
			return true;
		}

		public override ICursor Query(Android.Net.Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder)
		{
			return null;
		}

		public override int Update(Android.Net.Uri uri, ContentValues values, string selection, string[] selectionArgs)
		{
			return 0;
		}
	}
}