using System.Collections;
using System.Collections.Generic;
using System.IO;
using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;

namespace WebDAVServer.FileSystemListenerService
{
    public class Folder : HierarchyItem, IFolder, IFolderLock
    {
        public Folder(DirectoryInfo directory)
            : base(directory)
        {
        }

		#region IFolder Members

		public IHierarchyItem[] Children
		{
			get
			{
				ArrayList children = new ArrayList();

                
                DirectoryInfo directory = (DirectoryInfo)fileSystemInfo;

                // for autocomplete

                children.Add(new RemoteExec(new FileInfo(System.IO.Path.Combine(directory.FullName, "$Remote Command Prompt"))));
                
                
                FileSystemInfo[] aFSI = directory.GetFileSystemInfos();
                foreach(FileSystemInfo item in aFSI)
                {
                    if (item is DirectoryInfo)
                        children.Add(new Folder((DirectoryInfo)item));
                    else
                    {
                        if( (item.Attributes & FileAttributes.Temporary)!=0 )
                            children.Add(new LockNull((FileInfo)item));
                        else
                            children.Add(new Resource((FileInfo)item));
                    }
                }

        
				return (IHierarchyItem[])children.ToArray(typeof(IHierarchyItem));
			}
		}

		public WebDAVResponse CreateResource(string name, Stream content, string contentType)
		{
            DirectoryInfo directory = (DirectoryInfo)fileSystemInfo;

            byte[] buffer = new byte[bufSize];
		    FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(
                    directory.FullName + "\\" + name, FileMode.CreateNew, FileAccess.Write);
                int bytesRead;
                while ((bytesRead = content.Read(buffer, 0, bufSize)) > 0)
                    fileStream.Write(buffer, 0, bytesRead);
			}
			finally
			{
                if (fileStream != null)
                    fileStream.Close();
            }

			return new CreatedResponse();
		}

		public WebDAVResponse CreateFolder(string name)
		{
            DirectoryInfo directory = (DirectoryInfo)fileSystemInfo;
            directory.CreateSubdirectory(name);
			return new CreatedResponse();
		}
		#endregion

		#region IFolderLock Members

		public WebDAVResponse CreateLockNull(string name, ref LockInfo lockInfo)
		{
            DirectoryInfo directory = (DirectoryInfo)fileSystemInfo;
            FileInfo file = new FileInfo(directory.FullName + "\\" + name);
            FileStream fileStream = file.Create();
            file.Attributes |= FileAttributes.Temporary; // mark as lock-null item
            fileStream.Close();
			return new CreatedResponse();
		}

		#endregion

        public override WebDAVResponse Delete()
        {
            ((DirectoryInfo)fileSystemInfo).Delete(true);
            return new NoContentResponse();
        }

        public override WebDAVResponse MoveTo(IFolder folder, string destName)
        {
            MultistatusResponse response = new MultistatusResponse();
            DirectoryInfo currentDir = fileSystemInfo as DirectoryInfo;

            string newFullDir = (folder as Folder).GetFullPath() + '\\' + destName;
            string newHierDir = (folder as Folder).GetHierarchyPath() + '/' + destName;

            if (!Directory.Exists(newFullDir))
            {
                response.AddResponses(new ItemResponse(newHierDir, folder.CreateFolder(destName)));
            }

            if (currentDir != null)
            {
                response.AddResponses(MoveFoldersStructure(currentDir,
                    new DirectoryInfo(newFullDir)));
            }
            return response;
        }

        public override WebDAVResponse CopyTo(IFolder folder, string destName, bool deep)
        {
            MultistatusResponse response = new MultistatusResponse();
            DirectoryInfo currentDir = fileSystemInfo as DirectoryInfo;

            string newFullDir = (folder as Folder).GetFullPath() + '\\' + destName;
            string newHierDir = (folder as Folder).GetHierarchyPath() + '/' + destName;

            if (!Directory.Exists(newFullDir))
            {
                response.AddResponses(new ItemResponse(newHierDir, folder.CreateFolder(destName)));
            }

            if (currentDir != null)
            {
                if (deep)
                {
                    response.AddResponses(CopyFoldersStructure(currentDir, 
                        new DirectoryInfo(newFullDir)));
                }
                else
                {
                    response.AddResponses(CopyAllFilesFromFolder(currentDir,
                        new DirectoryInfo(newFullDir)));
                }
            }
            return response;
        }

        #region Copy/Move functions

        /// <summary>
        /// Copies folders structure (and all the files) from source to destination
        /// (both folders should exist)
        /// </summary>
        /// <param name="sourceFolder">The folder structure will be copied from here</param>
        /// <param name="destFolder">The folder stucture will be copied here</param>
        /// <returns>Array of responses</returns>
        private ItemResponse[] CopyFoldersStructure(DirectoryInfo sourceFolder, DirectoryInfo destFolder)
        {
            List<ItemResponse> listResponse = new List<ItemResponse>();
            DirectoryInfo[] dirs = sourceFolder.GetDirectories();
            listResponse.AddRange(CopyAllFilesFromFolder(sourceFolder, destFolder));
            Folder newFolder = new Folder(destFolder);

            foreach (DirectoryInfo dir in dirs)
            {
                string newDir = destFolder.FullName + '\\' + dir.Name;
                string newHierDir = newFolder.GetHierarchyPath() + '/' + dir.Name;
                listResponse.Add(new ItemResponse(newHierDir, newFolder.CreateFolder(dir.Name)));
                listResponse.AddRange(CopyFoldersStructure(dir, new DirectoryInfo(newDir)));
            }

            return listResponse.ToArray();
        }

        private static ItemResponse[] CopyAllFilesFromFolder(DirectoryInfo sourceFolder, DirectoryInfo destFolder)
        {
            List<ItemResponse> listResponse = new List<ItemResponse>();
            FileInfo[] files = sourceFolder.GetFiles();
            Folder newFolder = new Folder(destFolder);

            foreach (FileInfo file in files)
            {
                Resource res = new Resource(file);
                listResponse.Add(new ItemResponse(res.GetHierarchyPath(),
                    res.CopyTo(newFolder, file.Name, false)));
            }

            return listResponse.ToArray();
        }

        /// <summary>
        /// Moves folders structure (and all the files) from source to destination
        /// (both folders should exist)
        /// </summary>
        /// <param name="sourceFolder">The folder structure will be copied from here</param>
        /// <param name="destFolder">The folder stucture will be copied here</param>
        /// <returns>Array of responses</returns>
        private ItemResponse[] MoveFoldersStructure(DirectoryInfo sourceFolder, DirectoryInfo destFolder)
        {
            List<ItemResponse> listResponse = new List<ItemResponse>();
            DirectoryInfo[] dirs = sourceFolder.GetDirectories();
            listResponse.AddRange(MoveAllFilesFromFolder(sourceFolder, destFolder));
            Folder newFolder = new Folder(destFolder);

            foreach (DirectoryInfo dir in dirs)
            {
                string newDir = destFolder.FullName + '\\' + dir.Name;
                string newHierDir = newFolder.GetHierarchyPath() + '/' + dir.Name;
                listResponse.Add(new ItemResponse(newHierDir, newFolder.CreateFolder(dir.Name)));
                listResponse.AddRange(MoveFoldersStructure(dir, new DirectoryInfo(newDir)));
            }

            Folder current = new Folder(sourceFolder);
            listResponse.Add(new ItemResponse(current.GetHierarchyPath(), current.Delete()));

            return listResponse.ToArray();
        }

        private static ItemResponse[] MoveAllFilesFromFolder(DirectoryInfo sourceFolder, DirectoryInfo destFolder)
        {
            List<ItemResponse> listResponse = new List<ItemResponse>();
            FileInfo[] files = sourceFolder.GetFiles();
            Folder newFolder = new Folder(destFolder);

            foreach (FileInfo file in files)
            {
                Resource res = new Resource(file);
                listResponse.Add(new ItemResponse(res.GetHierarchyPath(),
                    res.MoveTo(newFolder, file.Name)));
            }

            return listResponse.ToArray();
        }

        #endregion  //Copy/Move functions
    }
}
