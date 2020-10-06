using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Descriptors.Keyword
{
    public class KeywordDescriptorFactory
    {
        public const string KEYWORD_EXTENSION = ".framesynsets";

        public static IDescriptorProvider<(int synsetId, float probability)[]> FromDirectory(string directory)
        {
            string inputFile = Directory.GetFiles(directory)
                    .Where(dir => Path.GetFileName(dir).EndsWith(KEYWORD_EXTENSION))
                    .FirstOrDefault();

            if (inputFile != null)
            {
                return new KeywordDescriptorProvider(inputFile);
            }
            else
            {
                return new KeywordDescriptorProviderDummy();
            }
        }
    }
}
