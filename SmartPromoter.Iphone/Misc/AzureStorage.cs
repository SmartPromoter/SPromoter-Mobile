//
//  AzureStorage.cs
//
//  Author:
//       Leonard Colusso  <leonardcolusso@smartpromoter.trade>
//
//  Copyright (c) 2016 SmartPromoter
//
using System;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SPromoterMobile;
using SPromoterMobile.Controller.RESTful;
using SPromoterMobile.Models.Exceptions;

namespace SmartPromoter.Iphone
{
    public class AzureStorage
    {

        public CloudBlobClient clientAzureStorage;
        public ContainerRestCon containerLocal = new ContainerRestCon();

        public string DownloadImage(string name, string folder)
        {
            try
            {
                if (clientAzureStorage != null)
                {
                    var container = clientAzureStorage.GetContainerReference(folder.Substring(0, folder.IndexOf(".", StringComparison.CurrentCulture)));
                    var blob = container.GetBlockBlobReference(name);
                    using (Stream image = new MemoryStream())
                    {
                        blob.DownloadToStreamAsync(image).Wait();
                        if (image.Length > 0)
                        {
                            return containerLocal.GravarArquivo(image, name);
                        }
                        throw new Container404Exception();
                    }
                }
                throw new UnauthorizedException();
            }
            catch (Exception e)
            {
                if (e.InnerException.Message.Contains("The specified blob does not exist."))
                {
                    return null;
                }
                if (e.InnerException.Message.Contains("Server failed to authenticate the request."))
                {
                    throw new UnauthorizedException();
                }
                if (e.InnerException.Message.Contains("The specified container does not exist."))
                {
                    throw new Container404Exception();
                }
            }
            return null;
        }

        public void UploadImage(Stream stream, string name, string folder)
        {
            try
            {
                if (clientAzureStorage != null)
                {
                    var container = clientAzureStorage.GetContainerReference(folder.Substring(0, folder.IndexOf(".", StringComparison.CurrentCulture)));
                    var blob = container.GetBlockBlobReference(name);
                    blob.UploadFromStreamAsync(stream).Wait();
                }
                else { throw new UnauthorizedException(); }
            }
            catch (Exception e)
            {
                if (e.InnerException.Message.Contains("The specified container does not exist."))
                {
                    throw new Container404Exception();
                }
                throw new UnauthorizedException();
            }
        }

        public AzureStorage()
        {
            var storageAccount = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(AzureCredenciais.user, AzureCredenciais.key);
            var storageURI = new StorageUri(new Uri(AzureCredenciais.url));
            clientAzureStorage = new CloudBlobClient(storageURI, storageAccount);
        }
    }
}
