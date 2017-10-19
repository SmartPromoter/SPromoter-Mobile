﻿using System;
using CoreGraphics;
using UIKit;

namespace SmartPromoter.Iphone
{
public class LoadingOverlay : UIView
	{
		UIActivityIndicatorView activitySpinner;
		UILabel loadingLabel;
        nfloat centerX;
        nfloat centerY;
        nfloat labelHeight;
        nfloat labelWidth;

        public void ExecOverLay()
        {
            // create the activity spinner, center it horizontall and put it 5 points above center x
            activitySpinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            activitySpinner.Frame = new CGRect(
                centerX - (activitySpinner.Frame.Width / 2),
                centerY - activitySpinner.Frame.Height - 20,
                activitySpinner.Frame.Width,
                activitySpinner.Frame.Height);
            activitySpinner.AutoresizingMask = UIViewAutoresizing.All;
            AddSubview(activitySpinner);
            activitySpinner.StartAnimating();

            // create and configure the "Loading Data" label
            loadingLabel = new UILabel(new CGRect(
                centerX - (labelWidth / 2),
                centerY + 20,
                labelWidth,
                labelHeight
                ))
            {
                BackgroundColor = UIColor.Clear,
                TextColor = UIColor.White,
                Text = "Aguarde...",
                TextAlignment = UITextAlignment.Center,
                AutoresizingMask = UIViewAutoresizing.All
            };
            AddSubview(loadingLabel);
        }


		public LoadingOverlay(CGRect frame) : base(frame)
		{
			// configurable bits
			BackgroundColor = UIColor.Black;
			Alpha = 0.75f;
			AutoresizingMask = UIViewAutoresizing.All;

			labelHeight = 22;
			labelWidth = Frame.Width - 20;

			// derive the center x and y
			centerX = Frame.Width / 2;
			centerY = Frame.Height / 2;
		}

		/// <summary>
		/// Fades out the control and then removes it from the super view
		/// </summary>
		public void Hide()
		{
#pragma warning disable iOSAndMacApiUsageIssue // Find issues with Mac and iOS API usage
            Animate(
#pragma warning restore iOSAndMacApiUsageIssue // Find issues with Mac and iOS API usage
                0.5, // duration
				() => { Alpha = 0; },
				() => { RemoveFromSuperview(); }
			);
		}
	}
}

