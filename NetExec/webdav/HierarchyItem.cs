using System;
using System.Configuration;
using System.IO;
using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;

namespace WebDAVServer.FileSystemListenerService
{
    public abstract class HierarchyItem : IHierarchyItem, ILock
    {
        protected FileSystemInfo fileSystemInfo;
        protected static int bufSize = 1048576; //(1Mb) buffer size used when reading and writing file content

        public HierarchyItem(FileSystemInfo fileSystemInfo)
        {
            this.fileSystemInfo = fileSystemInfo;
        }

		#region IHierarchyItem Members

		public string Name
		{
            get { return fileSystemInfo.Name; }
		}

		public DateTime Created
		{
            get { return fileSystemInfo.CreationTimeUtc; }
		}

		public DateTime Modified
		{
            get { return fileSystemInfo.LastWriteTimeUtc; }
		}

		public IFolder Parent
		{
			get
			{
                string storageRootFolder = Environment.CurrentDirectory.TrimEnd('\\');// ConfigurationManager.AppSettings["StorageRootFolder"].TrimEnd('\\');
                string parentPath = fileSystemInfo.FullName.TrimEnd('\\');
                int index = parentPath.LastIndexOf('\\');
                if (index <= storageRootFolder.Length-1)
                    return null;
                DirectoryInfo directory = new DirectoryInfo(parentPath.Remove(index));
                return new Folder(directory);
			}
		}

        public virtual WebDAVResponse CopyTo(IFolder folder, string destName, bool deep)
		{
			return new NotAllowedResponse();
		}

		public virtual WebDAVResponse MoveTo(IFolder folder, string destName)
		{
			return new NotAllowedResponse();
		}

		public abstract WebDAVResponse Delete();

		public WebDAVResponse GetProperties(ref Property[] props)
		{
            /*
			if(props == null) // get all properties
			{
				ArrayList l = new ArrayList();
				Property p = new Property();
				p.Namespace = "urn:schemas-microsoft-com:";
				p.Name = "Win32CreationTime";
				p.Value = fileSystemInfo.CreationTimeUtc.ToString("r");
				l.Add(p);

				p = new Property();
				p.Namespace = "urn:schemas-microsoft-com:";
				p.Name = "Win32LastModifiedTime";
                p.Value = fileSystemInfo.LastWriteTimeUtc.ToString("r");
				l.Add(p);

				p = new Property();
				p.Namespace = "urn:schemas-microsoft-com:";
				p.Name = "Win32LastAccessTime";
                p.Value = fileSystemInfo.LastAccessTimeUtc.ToString("r");
				l.Add(p);

                p = new Property();
				p.Namespace = "urn:schemas-microsoft-com:";
				p.Name = "Win32FileAttributes";
				p.Value = fileSystemInfo.Attributes.ToString();
				l.Add(p);

				props = (Property[])l.ToArray(typeof(Property));
			}
			else // get selected properties
			{
				for(int i=0; i<props.Length; i++)
                {
                    if(props[i].Namespace.ToLower()=="urn:schemas-microsoft-com:")
                    {
                        switch (props[i].Name)
                        {
                            case "Win32CreationTime": props[i].Value = fileSystemInfo.CreationTimeUtc.ToString("r"); break;
                            case "Win32LastModifiedTime": props[i].Value = fileSystemInfo.LastWriteTimeUtc.ToString("r"); break;
                            case "Win32LastAccessTime": props[i].Value = fileSystemInfo.LastAccessTimeUtc.ToString("r"); break;
                            case "Win32FileAttributes": props[i].Value = fileSystemInfo.Attributes.ToString(); break;
                        }
                    }
                }
			}
            */
			return new OkResponse();
		}

        public WebDAVResponse GetPropertyNames(ref Property[] props)
        {
            props = new Property[0];
            return new OkResponse();
        }

		public virtual WebDAVResponse UpdateProperties(Property[] setProps, Property[] delProps)
		{
            return new MultipropResponse();
		}

		#endregion

		#region ILock Members

        public LockInfo[] ActiveLocks
        {
            get 
            {
                LockInfo[] lockInfo = new LockInfo[1];
                lockInfo[0].Token = Guid.Empty.ToString();
                lockInfo[0].Shared = true;
                lockInfo[0].Deep = false;
                lockInfo[0].Owner = "Name";
                return lockInfo;
            }
        }

        public WebDAVResponse Lock(ref LockInfo lockInfo)
		{
            lockInfo.Token = Guid.Empty.ToString();
			return new OkResponse();
		}
		
        public WebDAVResponse RefreshLock(ref LockInfo lockInfo)
		{
			lockInfo.Shared = true;
			lockInfo.Deep = false;
            lockInfo.Owner = "Name";
			return new OkResponse();
		}

		public WebDAVResponse Unlock(string lockToken)
		{
            if (this is ILockNull) // delete lock-null item
                fileSystemInfo.Delete();
			return new NoContentResponse();
		}

		#endregion

        public virtual string GetFullPath()
        {
            return fileSystemInfo.FullName.TrimEnd('\\');
        }

        /// <summary>
        /// Returns path of the item in the hierarchy tree
        /// </summary>
        internal string GetHierarchyPath()
        {
            string storageRootFolder = Environment.CurrentDirectory.TrimEnd('\\'); //ConfigurationManager.AppSettings["StorageRootFolder"].TrimEnd('\\');
            int index = storageRootFolder.Length;          
            if (this.GetFullPath().Length > index) index++;
            
            return this.GetFullPath().Remove(0, index).Replace('\\', '/');
        }

        public virtual string Path
        {
            get
            {
                return this.GetHierarchyPath();
            }
        }
	}
}
