using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CNUSLib
{
    public class NUSTitle
    {
        public FST FST;
        public TMD TMD;
        public Ticket ticket;

        public bool skipExistingFiles = true;
        public NUSDataProvider dataProvider = null;

        public List<FSTEntry> getAllFSTEntriesFlatByContentID(short ID)
        {
            return getFSTEntriesFlatByContent(TMD.getContentByID((int)ID));
        }

        public List<FSTEntry> getFSTEntriesFlatByContentIndex(int index)
        {
            return getFSTEntriesFlatByContent(TMD.getContentByIndex(index));
        }

        public List<FSTEntry> getFSTEntriesFlatByContent(Content content)
        {
            return getFSTEntriesFlatByContents(new List<Content>(new Content[] { content }));
        }

        public List<FSTEntry> getFSTEntriesFlatByContents(List<Content> list)
        {
            List<FSTEntry> entries = new List<FSTEntry>();
            foreach (Content c in list)
            {
                foreach (FSTEntry f in c.entries)
                {
                    entries.Add(f);
                }
            }
            return entries;
        }

        public List<FSTEntry> getAllFSTEntriesFlat()
        {
            return getFSTEntriesFlatByContents(new List<Content>(TMD.getAllContents().Values));
        }

        public FSTEntry getFSTEntryByFullPath(String givenFullPath)
        {
            String fullPath = givenFullPath.Replace("/", "\\\\");
            if (!fullPath.StartsWith("\\")) fullPath = "\\" + fullPath;
            foreach (FSTEntry f in getAllFSTEntriesFlat())
            {
                if (f.getFullPath().Equals(fullPath))
                {
                    return f;
                }
            }
            return null;
        }

        public List<FSTEntry> getFSTEntriesByRegEx(String regEx)
        {
            List<FSTEntry> files = getAllFSTEntriesFlat();
            Regex p = new Regex(regEx);

            List<FSTEntry> result = new List<FSTEntry>();

            foreach (FSTEntry f in files)
            {
                String match = f.getFullPath().Replace("\\", "/");
                Match m = p.Match(match);
                if (m.Success)
                {
                    result.Add(f);
                }
            }
            return result;
        }

        public void printFiles()
        {
            //getFST().getRoot().printRecursive(0);
        }

        public void printContentFSTInfos()
        {
            //foreach (var e in getFST().getContentFSTInfos().entrySet()) 
            //{
            //    System.out.println(String.format("%08X", e.getKey()) + ": " + e.getValue());
            //}
        }

        public void printContentInfos()
        {
            //for (Entry<Integer, Content> e : getTMD().getAllContents().entrySet()) {

            //    System.out.println(String.format("%08X", e.getKey()) + ": " + e.getValue());
            //    System.out.println(e.getValue().getContentFSTInfo());
            //    for (FSTEntry entry : e.getValue().getEntries()) {
            //        System.out.println(entry.getFullPath() + String.format(" size: %016X", entry.getFileSize())
            //                + String.format(" offset: %016X", entry.getFileOffset()) + String.format(" flags: %04X", entry.getFlags()));
            //    }
            //    System.out.println("-");
            //}
        }

        public void cleanup()
        {
            if (dataProvider != null)
            {
                dataProvider.cleanup();
            }
        }

        public void printDetailedData()
        {
            printFiles();
            printContentFSTInfos();
            printContentInfos();

            //System.out.println();
        }

        public override string ToString()
        {
            return "NUSTitle [dataProvider=" + dataProvider + "]";
        }
    }
}
