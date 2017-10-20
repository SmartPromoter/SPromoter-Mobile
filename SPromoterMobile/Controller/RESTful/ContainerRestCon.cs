using PCLStorage;
using System;
using System.IO;
using System.Collections.Generic;

namespace SPromoterMobile.Controller.RESTful
{
    public class ContainerRestCon
    {
		/// <summary>
		/// Gets Full Path do arquivo.
		/// </summary>
		/// <returns>Verifica se o arquivo exise e retorna o full path do mesmo.</returns>
		/// <param name="nomeDoArquivo">Nome do arquivo com extensao.</param>
        public string GetNomeArquivoFullPath(string nomeDoArquivo)
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;
            IFolder folder = rootFolder.CreateFolderAsync("smartpromoterfiles",
			                CreationCollisionOption.OpenIfExists).Result;
			var checkResult = folder.CheckExistsAsync(nomeDoArquivo).Result;
            if (checkResult == ExistenceCheckResult.FileExists)
            {
				IFile file = folder.GetFileAsync(nomeDoArquivo).Result;
                return file.Path;
            }
            return null;
        }

		/// <summary>
		/// Get url do arquivo sem full path.
		/// </summary>
		/// <returns>Verifica se o arquivo exise e retorna a url sem full path do mesmo.</returns>
		/// <param name="nomeDoArquivo">Nome do arquivo.</param>
		public string GetNomeArquivo(string nomeDoArquivo)
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;
            IFolder folder = rootFolder.CreateFolderAsync("smartpromoterfiles",
			              CreationCollisionOption.OpenIfExists).Result;
			var checkResult = folder.CheckExistsAsync(nomeDoArquivo).Result;
            if (checkResult == ExistenceCheckResult.FileExists)
            {
				IFile file = folder.GetFileAsync(nomeDoArquivo).Result;
                return file.Name;
            }
            return null;
        }

		/// <summary>
		/// Gravar um arquivo no diretorio local.
		/// </summary>
		/// <returns>full path com extensao do arquivo inserido localmente</returns>
		/// <param name="arquivo">Data stream do arquivo.</param>
		/// <param name="nomeDoArquivo">Nome do arquivo com extensao.</param>
		public string GravarArquivo(Stream arquivo, string nomeDoArquivo)
		{
			try
			{
				IFolder rootFolder = FileSystem.Current.LocalStorage;
				IFolder folder =  rootFolder.CreateFolderAsync("smartpromoterfiles",
				                  CreationCollisionOption.OpenIfExists).Result;
				IFile file = folder.CreateFileAsync(nomeDoArquivo,
				             CreationCollisionOption.ReplaceExisting).Result;
				using (Stream stream = file.OpenAsync(FileAccess.ReadAndWrite).Result)
				{
					arquivo.Seek(0, SeekOrigin.Begin);
					arquivo.CopyTo(stream);
				}
				return file.Name;
			}
			catch (Exception)
			{
				return null;
			}
		}


		public List<string> ListFotos()
		{
			var listaDeFotos = new List<string>();
			IFolder rootFolder = FileSystem.Current.LocalStorage;
			IFolder folder = rootFolder.CreateFolderAsync("smartpromoterfiles",
				CreationCollisionOption.OpenIfExists).Result;
			var list = folder.GetFilesAsync().Result;
			foreach (var item in list)
			{
				listaDeFotos.Add(item.Name);	
			}
			return listaDeFotos;
		}


		/// <summary>
		/// Leia um arquivo local com base no full path.
		/// </summary>
		/// <returns>Data stream do arquivo</returns>
		/// <param name="nomeDoArquivo">nome do arquivo com extensao e full path.</param>
		public Stream LerArquivo(string nomeDoArquivo)
		{
			if (nomeDoArquivo == null)
			{
				return null;
			}
			IFolder rootFolder = FileSystem.Current.LocalStorage;
			IFolder folder = rootFolder.CreateFolderAsync("smartpromoterfiles",
				CreationCollisionOption.OpenIfExists).Result;
			var checkResult = folder.CheckExistsAsync(nomeDoArquivo).Result;
			if (checkResult == ExistenceCheckResult.FileExists)
			{
				IFile file = folder.GetFileAsync(nomeDoArquivo).Result;
				var result = file.OpenAsync(FileAccess.Read).Result;
				return result;
			}
			return null;

		}

		/// <summary>
		/// Deleta um arquivo local.
		/// </summary>
		/// <param name="nomeDoArquivo">Nome do arquivo com extesao e full path.</param>
		public void DeleteArquivo(string nomeDoArquivo)
		{
			IFolder rootFolder = FileSystem.Current.LocalStorage;
			IFolder folder = rootFolder.CreateFolderAsync("smartpromoterfiles",
			                CreationCollisionOption.OpenIfExists).Result;
			var checkResult = folder.CheckExistsAsync(nomeDoArquivo).Result;
			if (checkResult == ExistenceCheckResult.FileExists)
			{
				IFile file = folder.GetFileAsync(nomeDoArquivo).Result;
				file.DeleteAsync();
			}
		}
	}
}
