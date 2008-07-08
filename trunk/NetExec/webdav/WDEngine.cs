using System.Configuration;
using System.IO;
using ITHit.WebDAV.Server;
using NetDrive;
using System;
namespace WebDAVServer.FileSystemListenerService
{
	public class WDEngine : Engine
	{
        public override string License
        {
            get
            {
                return @"<?xml version=""1.0"" encoding=""utf-8""?><License><Data><Product>IT Hit WebDAV Server .Net v2</Product><LicensedTo><![CDATA[n/a]]></LicensedTo><Quantity>1</Quantity><IssueDate><![CDATA[Tuesday, June 24, 2008]]></IssueDate><ExpirationDate><![CDATA[Thursday, July 24, 2008]]></ExpirationDate><Type>Evaluation</Type></Data><Signature><![CDATA[dcq3jy6+svYZ7KmUTYH/C2BFG2bIameniy4hyL990KWW4KD6xqcHvX/PXJ7b64kjui6mVs6UvSL+5+y5NMF2rA/PNen54FdtHlLuWabqpTeZu9X6vcZ/p3WwzwIluHEieIKWT4Mx5QTL8cUt5xgkofmaqIJhzRmFuvYUZSS01EU=]]></Signature></License>";
            }
        }

        public override IHierarchyItem GetHierarchyItem(string path)
        {
            string discPath = System.Environment.CurrentDirectory.TrimEnd('\\');// ConfigurationManager.AppSettings["StorageRootFolder"].TrimEnd('\\');
            discPath += path.Split('?')[0].Replace('/', '\\');

            // Remote Execution
            if (Path.GetFileName(path).StartsWith("$Remote Command Prompt", StringComparison.OrdinalIgnoreCase))
                return new RemoteExec(new FileInfo(discPath));
          /*  if (Path.GetFileName(path).Equals("$$resp", StringComparison.OrdinalIgnoreCase))
                return new RemoteExec(new FileInfo(discPath));
            */


            DirectoryInfo directory = new DirectoryInfo(discPath);
            if (directory.Exists)
            {           
                
                return new Folder(directory);
            }
            else
            {
                FileInfo file = new FileInfo(discPath);
                if (file.Exists)
                {
                    if ((file.Attributes & FileAttributes.Temporary) != 0)
                        return new LockNull(file); // lock-null item
                    else
                        return new Resource(file);
                }
            }
            return null;
        }
    }
}
