using System.IO;
using System.Net;
using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;

namespace WebDAVServer.FileSystemListenerService
{
    public class Resource : HierarchyItem, IResource
    {
        public Resource(FileInfo file)
            : base(file)
        {
        }

		#region IResource Members

		public string ContentType
		{
			get
			{
                return MimeType.GetMimeType(this.fileSystemInfo.Extension)
                        ?? "application/octet-stream";
			}
		}

		public long ContentLength
		{
			get
			{
                FileInfo file = (FileInfo)fileSystemInfo;
                return file.Length;
			}
		}

        public WebDAVResponse WriteToStream(Stream output, long startIndex, long count)
		{
            FileInfo file = (FileInfo)fileSystemInfo;
            byte[] buffer = new byte[bufSize];
            FileStream fileStream = null;
            try
			{
                fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                fileStream.Seek(startIndex, SeekOrigin.Begin);
			    int bytesRead;
			    while ((bytesRead = fileStream.Read(buffer, 0, (int)(count > bufSize ? bufSize : count))) > 0)
				{
                    try
                    {
                        output.Write(buffer, 0, bytesRead);
                    }
                    catch (HttpListenerException ex)
                    {
                        if ((ex.ErrorCode == 1229) || (ex.ErrorCode == 64))
                        { // client closed connection
                            // 1. ErrorCode=1229. An operation was attempted on a nonexistent network connection.
                            // 2. ErrorCode=64. The specified network name is no longer available.
                            return new OkResponse();
                        }
                        throw;
                    }
                    count -= bytesRead;
				}
			}
			finally
			{
                if (fileStream != null) 
                    fileStream.Close();
			}
			
			return new OkResponse();
		}

		public WebDAVResponse SaveFromStream(Stream content, string contentType)
		{
            FileInfo file = (FileInfo)fileSystemInfo;

            byte[] buffer = new byte[bufSize];
		    FileStream fileStream = null;
            try
            {
                fileStream = file.Open(FileMode.Truncate, FileAccess.Write);
                int bytesRead;
                while ((bytesRead = content.Read(buffer, 0, bufSize)) > 0)
                    fileStream.Write(buffer, 0, bytesRead);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }

//            if (content.Length > 0)
                return new OkResponse();
//            else
//                return new NoContentResponse();
		}
		#endregion

        public override WebDAVResponse Delete()
        {
            fileSystemInfo.Delete();
            return new NoContentResponse();
        }

        public override WebDAVResponse MoveTo(IFolder folder, string destName)
        {
            if (!Directory.Exists((folder as Folder).GetFullPath()))
                return new ConflictResponse();

            string newFullName = (folder as Folder).GetFullPath() + '\\' + destName;

            try
            {
                if (!File.Exists(newFullName))
                {
                    File.Move(fileSystemInfo.FullName, newFullName);
                    return new CreatedResponse();
                }
                else
                {
                    File.Delete(newFullName);
                    File.Move(fileSystemInfo.FullName, newFullName);
                    return new NoContentResponse();
                }
            }
            catch (IOException)
            {
                return new ConflictResponse();
            }
        }

        public override WebDAVResponse CopyTo(IFolder folder, string destName, bool deep)
        {
            if (!Directory.Exists((folder as Folder).GetFullPath()))
                return new ConflictResponse();

            string newFullName = (folder as Folder).GetFullPath() + '\\' + destName;

            try
            {
                if (!File.Exists(newFullName))
                {
                    File.Copy(fileSystemInfo.FullName, newFullName);
                    return new CreatedResponse();
                }
                else
                {
                    File.Delete(newFullName);
                    File.Copy(fileSystemInfo.FullName, newFullName);
                    return new NoContentResponse();
                }
            }
            catch (IOException)
            {
                return new ConflictResponse();
            }
        }
    }
}
