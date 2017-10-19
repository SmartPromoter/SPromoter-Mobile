using System;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace SmartPromoter.Iphone
{
    public class FeedPdvCollections : UICollectionViewSource
    {
        public List<Pdv> Pdvs { get; set; }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return Pdvs.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell("pdvCell", indexPath) as FeedPDVCell;
            var animal = Pdvs[indexPath.Row];
            var frame = cell.Frame;
            frame.Size = new CoreGraphics.CGSize(collectionView.Superview.Frame.Width, cell.Frame.Height);
            frame.X = 0;
            cell.Frame = frame;
            cell.SetValuesCell(animal);
            return cell;
        }
    }
}

