using System.IO;
using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;

namespace NetExec
{
	public class LockNull : HierarchyItem, ILockNull
	{
        public LockNull(FileInfo file)
            : base(file)
        {
        }

		public WebDAVResponse ConvertToResource(Stream content, string contentType)
		{
            FileInfo file = (FileInfo)fileSystemInfo;

            byte[] buffer = new byte[bufSize];
		    FileStream fileStream = null;
            try
            {
                fileStream = file.Open(FileMode.Open, FileAccess.Write);
                int bytesRead;
                while ((bytesRead = content.Read(buffer, 0, bufSize)) > 0)
                    fileStream.Write(buffer, 0, bytesRead);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }

            file.Attributes &= ~FileAttributes.Temporary; // remove lock-null marker

			return new CreatedResponse();
		}

		public WebDAVResponse ConvertToFolder()
		{
            FileInfo file = (FileInfo)fileSystemInfo;
            string path = file.FullName;
            file.Delete();
            Directory.CreateDirectory(path);

			return new CreatedResponse();
		}

		public override WebDAVResponse CopyTo(IFolder folder, string destName, bool deep)
		{
			return new NotAllowedResponse();
		}

		public override WebDAVResponse MoveTo(IFolder folder, string destName)
		{
			return new NotAllowedResponse();
		}

		public override WebDAVResponse Delete()
		{
			 return new NotAllowedResponse();
		}

		public override WebDAVResponse UpdateProperties(Property[] setProps, Property[] delProps)
		{
			return new NotAllowedResponse();
		}
    }
}
