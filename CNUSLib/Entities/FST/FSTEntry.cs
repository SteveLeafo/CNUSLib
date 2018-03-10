using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class FSTEntry
    {
        public static byte FSTEntry_DIR = (byte)0x01;
        public static byte FSTEntry_notInNUS = (byte)0x80;

        public String filename;
        public String path;
        public FSTEntry parent;

        public List<FSTEntry> Children = new List<FSTEntry>();

        public short flags;

        public long fileSize;
        public long fileOffset;

        public Content content;

        public bool isDir;
        public bool isRoot;
        public bool isNotInPackage;

        short _contentFSTID;
        public short contentFSTID
        {
            get
            {
                return _contentFSTID;
            }
            set
            {
                _contentFSTID = value;
            }
        }


        public FSTEntry(FSTEntryParam fstParam)
        {
            this.filename = fstParam.Filename;
            this.path = fstParam.Path;
            this.flags = fstParam.Flags;
            this.parent = fstParam.Parent;
            if (parent != null)
            {
                parent.Children.Add(this);
            }
            this.fileSize = fstParam.FileSize;
            this.fileOffset = fstParam.FileOffset;
            this.content = fstParam.Content;
            if (content != null)
            {
                content.addEntry(this);
            }
            this.isDir = fstParam.isDir;
            this.isRoot = fstParam.isRoot;
            this.isNotInPackage = fstParam.notInPackage;
            this.contentFSTID = fstParam.ContentFSTID;
        }

        /**
         * Creates and returns a new FST Entry
         * @return
         */
        public static FSTEntry getRootFSTEntry()
        {
            FSTEntryParam param = new FSTEntryParam();
            param.isRoot = true;
            param.isDir = true;
            return new FSTEntry(param);
        }

        public String getFullPath()
        {
            return path + filename;
        }

        public int getEntryCount()
        {
            int count = 1;
            foreach (FSTEntry entry in Children)
            {
                count += entry.getEntryCount();
            }
            return count;
        }

        public List<FSTEntry> getDirChildren()
        {
            return getDirChildren(false);
        }

        public List<FSTEntry> getDirChildren(bool all)
        {
            List<FSTEntry> result = new List<FSTEntry>();
            foreach (FSTEntry child in Children)
            {
                if (child.isDir && (all || !child.isNotInPackage))
                {
                    result.Add(child);
                }
            }
            return result;
        }

        public List<FSTEntry> getFileChildren()
        {
            return getFileChildren(false);
        }

        public List<FSTEntry> getFileChildren(bool all)
        {
            List<FSTEntry> result = new List<FSTEntry>();
            foreach (FSTEntry child in Children)
            {
                if ((all && !child.isDir || !child.isDir))
                {
                    result.Add(child);
                }
            }
            return result;
        }

        public List<FSTEntry> getFSTEntriesByContent(Content content)
        {
            List<FSTEntry> entries = new List<FSTEntry>();
            if (this.content == null)
            {
                //MessageBox.Show("Error in getFSTEntriesByContent, content null");
                //System.exit(0);
            }
            else
            {
                if (this.content.Equals(content))
                {
                    entries.Add(this);
                }
            }
            foreach (FSTEntry child in Children)
            {
                entries.AddRange(child.getFSTEntriesByContent(content));
            }
            return entries;
        }

        public long getFileOffsetBlock()
        {
            //if(content.isHashed){
            //    return (fFileOffse)/0xFC00) * 0x10000;
            //}else{
            //    return FileOffset();
            //}
            return fileOffset;
        }

        public void printRecursive(int space)
        {
            //for(int i = 0;i<space;i++){
            //    System.out.print(" ");
            //}
            //System.out.print(Filename());
            //if(isNotInPackage()){
            //    System.out.print(" (not in package)");
            //}
            //System.out.println();
            //for(FSTEntry child : DirChildren(true)){
            //    child.printRecursive(space + 5);
            //}
            //for(FSTEntry child : FileChildren(true)){
            //    child.printRecursive(space + 5);
            //}
        }

        public override string ToString()
        {
            return "FSTEntry [filename=" + filename + ", path=" + path + ", flags=" + flags + ", filesize=" + fileSize
                    + ", fileoffset=" + fileOffset + ", content=" + content + ", isDir=" + isDir + ", isRoot=" + isRoot
                    + ", notInPackage=" + isNotInPackage + "]";
        }
    }
}
