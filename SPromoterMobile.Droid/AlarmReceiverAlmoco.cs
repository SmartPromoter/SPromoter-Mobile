using Android.App;
using Android.Content;

namespace spromotermobile.droid
{


	[BroadcastReceiver]
	public class AlarmReceiverAlmoco : BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{
			var message = intent.GetStringExtra("message");
			var title = intent.GetStringExtra("title");

			var resultintent = new Intent(context, typeof(MenuPdvs));
			resultintent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);

			var pending = PendingIntent.GetActivity(context, (int)TipoNotificacao.AlarmeAlmoco, resultintent, PendingIntentFlags.CancelCurrent);


			var builder = new Notification.Builder(context)
										  .SetContentTitle(title)
										  .SetContentText(message)
			                              .SetSmallIcon(Resource.Drawable.logosmartpromoter)
										  .SetDefaults(NotificationDefaults.All);
			builder.SetContentIntent(pending);
			var notification = builder.Build();

			var manager = NotificationManager.FromContext(context);
			manager.Notify((int)TipoNotificacao.AlarmeAlmoco, notification);

		}
	}
}

