using System;
using System.Drawing;
using Foundation;
using UIKit;

namespace SmartPromoter.Iphone
{
		public class ToastView : NSObject
		{

			ToastSettings theSettings = new ToastSettings();

			string text;
			UIView view;
			public ToastView(string Text, int seconds)
			{
				text = Text;
				theSettings.Duration = seconds * 1000;
				theSettings.Gravity = ToastGravity.Bottom;
			}

			int offsetLeft;
			int offsetTop;
			public ToastView SetGravity(ToastGravity gravity, int OffSetLeft, int OffSetTop)
			{
				theSettings.Gravity = gravity;
				offsetLeft = OffSetLeft;
				offsetTop = OffSetTop;
				return this;
			}

			public ToastView SetPosition(PointF Position)
			{
				theSettings.Position = Position;
				return this;
			}

			public void Show()
			{
				UIButton v = UIButton.FromType(UIButtonType.Custom);
				view = v;

				var textToast = new NSString(text);

			    var textAttr = textToast.GetSizeUsingAttributes(new UIStringAttributes { Font = UIFont.SystemFontOfSize(16) });

				if (textAttr.Width >= 278)
				{
					textAttr.Width = 278;
				}
				if (textAttr.Height >= 70)
				{
					textAttr.Height = 70;
				}

				var label = new UILabel(new RectangleF(0, 0, (float)(textAttr.Width + 5), (float)(textAttr.Height + 5)));
				label.BackgroundColor = UIColor.Clear;
				label.TextColor = UIColor.White;
				label.Font = UIFont.SystemFontOfSize(16);
				label.Text = textToast;
				label.Lines = 0;
				label.ShadowColor = UIColor.DarkGray;
				label.ShadowOffset = new SizeF(1, 1);


				v.Frame = new RectangleF(0, 0, (float)(textAttr.Width + 10), (float)(textAttr.Height + 10));
				label.Center = new PointF((float)(v.Frame.Size.Width / 2), (float)(v.Frame.Height / 2));
				v.AddSubview(label);

				v.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0.7f);
				v.Layer.CornerRadius = 5;

				UIWindow window = UIApplication.SharedApplication.Windows[0];

				var point = new PointF((float)(window.Frame.Size.Width / 2), (float)(window.Frame.Size.Height / 2));

				switch (theSettings.Gravity)
				{
					case (ToastGravity.Top):
							point = new PointF((float)(window.Frame.Size.Width / 2), 45);
						break;
					case (ToastGravity.Bottom):
							point = new PointF((float)(window.Frame.Size.Width / 2), (float)(window.Frame.Size.Height - 90));
						break;
					case (ToastGravity.Center):
							point = new PointF((float)(window.Frame.Size.Width / 2), (float)(window.Frame.Size.Height / 2));
						break;
					default:
							point = theSettings.Position;
						break;
				}
				point = new PointF(point.X + offsetLeft, point.Y + offsetTop);
				v.Center = point;
				window.AddSubview(v);
				v.AllTouchEvents += HideToast();
				NSTimer.CreateScheduledTimer(theSettings.DurationSeconds, (NSTimer obj) => HideToast());
			}


			EventHandler HideToast()
			{
				return (sender, e) => {
				
					UIView.BeginAnimations("");
					view.Alpha = 0;
					UIView.CommitAnimations();
				};
			}

			void RemoveToast()
			{
				view.RemoveFromSuperview();
			}

		}
}

