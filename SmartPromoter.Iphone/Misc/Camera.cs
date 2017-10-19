using System;
using Foundation;
using SPromoterMobile.Controller.RESTful;
using UIKit;

namespace SmartPromoter.Iphone
{
    public static class Camera
    {

        static UIImagePickerController picker;
        static Action<NSDictionary> _callback;


        public static UIImage GetBitMap(string nameFile)
        {
            var container = new ContainerRestCon();
            var streamFoto = container.LerArquivo(nameFile);
            var teste = NSData.FromStream(streamFoto);
            return new UIImage(teste);
        }

        static void Init()
        {
            if (picker != null)
                return;
            picker = new UIImagePickerController()
            {
                Delegate = new CameraDelegate()
            };
        }

        class CameraDelegate : UIImagePickerControllerDelegate
        {
            public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info)
            {
                var cb = _callback;
                _callback = null;
                picker.DismissViewController(true, null);
                cb(info);
            }
        }

        public static void TakePicture(UIViewController parent, Action<NSDictionary> callback)
        {
            Init();
            picker.SourceType = UIImagePickerControllerSourceType.Camera;
            picker.CameraDevice = UIImagePickerControllerCameraDevice.Rear;
            _callback = callback;
            parent.PresentViewController(picker, true, null);
        }

        public static void SelectPicture(UIViewController parent, Action<NSDictionary> callback)
        {
            Init();
            picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            _callback = callback;
            parent.PresentViewController(picker, true, null);
        }

    }
}

