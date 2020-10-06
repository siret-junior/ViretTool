using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Descriptors.KeywordLabel
{
    public class KeywordLabelFactory
    {
        private const string KEYWORD_LABELS_EXTENSION = ".label";

        public static IKeywordLabelProvider<string> FromDirectory(string directory)
        {
            string inputFile = Directory.GetFiles(directory)
                    .Where(dir => Path.GetFileName(dir).EndsWith(KEYWORD_LABELS_EXTENSION))
                    .FirstOrDefault();

            if (inputFile != null)
            {
                return FromFile(inputFile);
            }
            else
            {
                return new KeywordLabelProviderDummy();
            }
        }


        public static IKeywordLabelProvider<string> FromFile(string inputFile)
        {
            Dictionary<string, List<int>> labelToSynsetMapping = new Dictionary<string, List<int>>();
            Dictionary<int, string> synsetToLabelMapping = new Dictionary<int, string>();

            using (StreamReader reader = new StreamReader(inputFile))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split('~');
                    if (parts.Length != 6)
                    {
                        throw new FormatException("Line has invalid number of parts.");
                    }

                    string[] nameParts = parts[2].Split('#');
                    int minLenght = int.MaxValue;
                    foreach (string item in nameParts)
                    {
                        int length = item.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                        if (length < minLenght)
                        {
                            minLenght = length;
                        }
                    }

                    //int id = -1;
                    //if (parts[0] != "H")
                    //{
                    //    id = int.Parse(parts[0]);
                    //}

                    //var label = new Label()
                    //{
                    //    Id = id,
                    //    SynsetId = int.Parse(parts[1]),
                    //    Name = string.Join(", ", nameParts),
                    //    Names = nameParts,
                    //    Hyponyms = GetSynsetIds(parts[3]),
                    //    Hypernyms = GetSynsetIds(parts[4]),
                    //    Description = parts[5],
                    //    NameLenghtInWords = minLenght
                    //};

                    // TODO: label ID numbered from 0 sequentially is used here instead of the actual synset ID -> fix that
                    //int synsetId = id;
                    int synsetId = int.Parse(parts[1]);
                    string synsetName = string.Join(", ", nameParts);

                    // TODO: label ID numbered from 0 sequentially is used here instead of the actual synset ID -> fix that
                    if (synsetId != -1)
                    {
                        synsetToLabelMapping.Add(synsetId, synsetName);
                    }

                    if (!labelToSynsetMapping.ContainsKey(synsetName))
                    {
                        labelToSynsetMapping.Add(synsetName, new List<int> { synsetId });
                    }
                    else
                    {
                        labelToSynsetMapping[synsetName].Add(synsetId);
                    }
                }
            }

            return new KeywordLabelProvider(labelToSynsetMapping, synsetToLabelMapping);
        }

    }
}
