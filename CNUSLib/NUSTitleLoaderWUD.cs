using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WudTool
{
    internal class NUSTitleLoaderWUD : NUSTitleLoader
    {
        private NUSTitleLoaderWUD()
            : base()
        {
        }

        public static NUSTitle loadNUSTitle(String WUDPath)
        {
            return loadNUSTitle(WUDPath, null);
        }

        public static NUSTitle loadNUSTitle(String WUDPath, byte[] titleKey)
        {
            NUSTitleLoader loader = new NUSTitleLoaderWUD();
            NUSTitleConfig config = new NUSTitleConfig();
            byte[] usedTitleKey = titleKey;
            FileInfo wudFile = new FileInfo(WUDPath);
            if (!wudFile.Exists)
            {
                //MessageBox.Show(WUDPath + " does not exist.");
                //System.exit(1);
            }

            WUDImage image = new WUDImage(wudFile);
            if (usedTitleKey == null)
            {
                FileInfo keyFile = new FileInfo(Path.GetDirectoryName(wudFile.FullName) + Path.DirectorySeparatorChar + Settings.WUD_KEY_FILENAME);
                if (!keyFile.Exists)
                {
                    //MessageBox.Show(keyFile.FullName + " does not exist and no title key was provided.");
                    return null;
                }
                usedTitleKey = File.ReadAllBytes(keyFile.FullName);
            }
            WUDInfo wudInfo = WUDInfoParser.createAndLoad(image.WUDDiscReader, usedTitleKey);
            if (wudInfo == null)
            {
                return null;
            }

            config.WUDInfo = (wudInfo);

            return loader.loadNusTitle(config);
        }

        protected override NUSDataProvider getDataProvider(NUSTitle title, NUSTitleConfig config)
        {
            return new NUSDataProviderWUD(title, config.WUDInfo);
        }
    }
}
