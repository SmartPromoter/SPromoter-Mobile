using System;
using Android.Content;
using Android.Provider;
using Android.Graphics;
using SPromoterMobile.Controller.RESTful;
using System.IO;
using Android.Media;
using System.Collections.Generic;

namespace spromotermobile.droid
{
    public class Camera : IDisposable
    {
		internal enum CameraCode { OnActivityResultCode = 188 }
        Context context;
		Java.IO.File fotoDir;

        public Camera(Context context)
        {
            this.context = context;
        }

		internal Intent PerfomCamera()
        {
			var intent = new Intent(MediaStore.ActionImageCapture);   
			fotoDir = new Java.IO.File(context.GetExternalFilesDir(Environment.CurrentDirectory),
			                           string.Format("SP_TEMP_{0}.jpeg", Guid.NewGuid()));
			intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(fotoDir));
            return intent;
        }


		/// <summary>
		/// Insere fotos que sobrescrevem a outra.
		/// </summary>
		/// <returns>url da foto</returns>
		/// <param name="nameFile">Nome do Arquivo.</param>
		internal List<string> PerformOnActivity(List<string> nameFile)
        {
				var container = new ContainerRestCon();
				var contentUri = Android.Net.Uri.FromFile(fotoDir);
				var foto = LoadAndResizeBitmap(contentUri.Path);

				var ei = new ExifInterface(contentUri.Path);
				var orientation = ei.GetAttributeInt(ExifInterface.TagOrientation, (int)Orientation.Undefined);
            switch (orientation)
            {
                case (int)Orientation.Rotate90:
                    foto = RotateImage(foto, 90);
                    break;
                case (int)Orientation.Rotate180:
                    foto = RotateImage(foto, 180);
                    break;
                case (int)Orientation.Rotate270:
                    foto = RotateImage(foto, 270);
                    break;
                case (int)Orientation.Normal:
                    break;
            }

            using (var stream = new MemoryStream())
				{
					var urls = new List<string>();
					foto.Compress(Bitmap.CompressFormat.Jpeg, 70, stream);
					foreach (var item in nameFile)
					{
						urls.Add(container.GravarArquivo(stream, item + "." + Bitmap.CompressFormat.Jpeg));
					}
					fotoDir.Delete();
					foto = null;
					GC.Collect();
					return urls;
				}
        }

        /// <summary>
        /// Insere fotos que nao sobrescrevem a outra ( usa o timeStamp para diferenciar )
        /// </summary>
        /// <returns>url da foto</returns>
        /// <param name="nameFile">Nome do arquivo</param>
        /// <param name="timeStamp">Data atualizada</param>
        internal string PerformOnActivity(string nameFile, DateTime timeStamp)
		{
				var container = new ContainerRestCon();
				var contentUri = Android.Net.Uri.FromFile(fotoDir);
				var foto = LoadAndResizeBitmap(contentUri.Path);

				var ei = new ExifInterface(contentUri.Path);
				var orientation = ei.GetAttributeInt(ExifInterface.TagOrientation, (int)Orientation.Undefined);
            switch (orientation)
            {
                case (int)Orientation.Rotate90:
                    foto = RotateImage(foto, 90);
                    break;
                case (int)Orientation.Rotate180:
                    foto = RotateImage(foto, 180);
                    break;
                case (int)Orientation.Rotate270:
                    foto = RotateImage(foto, 270);
                    break;
                case (int)Orientation.Normal:
                    break;
            }

            using (var stream = new MemoryStream())
				{
					foto.Compress(Bitmap.CompressFormat.Jpeg, 70, stream);
					var url = container.GravarArquivo(stream,
						nameFile + "_TIMESTAMP_" + timeStamp.Ticks + "." + Bitmap.CompressFormat.Jpeg);
					fotoDir.Delete();
					foto = null;
					GC.Collect();
					return url;
				}
        }

        /// <summary>
        /// Rotaciona a imagem de acordo com a posicao do celular ( HotFix para samsung devices )
        /// </summary>
        /// <returns>Imagem rotacionada</returns>
        /// <param name="sourceImage">Source image.</param>
        /// <param name="degree">Degree.</param>
        static Bitmap RotateImage(Bitmap sourceImage, float degree)
		{
			var matrix = new Matrix();
			matrix.PostRotate(degree);
			return Bitmap.CreateBitmap(sourceImage, 0, 0, sourceImage.Width, sourceImage.Height, matrix, true);
		}

		public Bitmap GetBitMap(string nameFile)
        {
			var container = new ContainerRestCon();
            var streamFoto = container.LerArquivo(nameFile);
			var option = new BitmapFactory.Options { InSampleSize = 4 };
			option.InPreferredConfig = Bitmap.Config.Rgb565;
			return BitmapFactory.DecodeStream(streamFoto, null, option);
        }

        Bitmap LoadAndResizeBitmap(string fileName)
        {
            var options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileName, options);
            int REQUIRED_SIZE = 460;
            float height = options.OutHeight;
            float width = options.OutWidth;
            double inSampleSize = 1D;
            if (height > REQUIRED_SIZE || width > REQUIRED_SIZE)
            {
                var halfHeight = (int)(height / 2);
				var halfWidth = (int)(width / 2);

                while ((halfHeight / inSampleSize) > REQUIRED_SIZE && (halfWidth / inSampleSize) > REQUIRED_SIZE)
                {
                    inSampleSize *= 2;
                }
            }

            var options2 = new BitmapFactory.Options { InSampleSize = (int)inSampleSize };
            Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options2);
            return resizedBitmap;
        }

        #region IDisposable Support
        bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    fotoDir.Dispose();
                    context.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}