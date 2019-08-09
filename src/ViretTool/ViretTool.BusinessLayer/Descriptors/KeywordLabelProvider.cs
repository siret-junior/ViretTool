using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Descriptors
{
    /// <summary>
    /// Reused and modified from "ViretTool.PresentationLayer.Controls.Common.KeywordSearch.Suggestion.LabelProvider"
    /// TODO: merge these 2 pieces of code
    /// </summary>
    public class KeywordLabelProvider : IKeywordLabelProvider<string>
    {
        private const string KEYWORD_LABELS_EXTENSION = ".label";

        public Dictionary<string, List<int>> LabelToSynsetMapping { get; } = new Dictionary<string, List<int>>();
        public Dictionary<int, string> SynsetToLabelMapping { get; } = new Dictionary<int, string>();


        public KeywordLabelProvider(string inputFile)
        {
            LoadFromFile(inputFile);
        }
        

        public List<int> GetSynsets(string label)
        {
            return LabelToSynsetMapping[label];
        }

        // TODO: label ID numbered from 0 sequentially is used here instead of the actual synset ID -> fix that
        public string GetLabel(int synsetId)
        {
            string label;
            if (SynsetToLabelMapping.TryGetValue(synsetId, out label))
            {
                return label;
            }
            else
            {
                return $"==({synsetId}) NO LABEL==";
            }
        }


        public void LoadFromFile(string inputFile)
        {
            using (StreamReader reader = new StreamReader(inputFile))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split('~');
                    if (parts.Length != 6)
                    {
                        throw new FormatException("Line has invalid number of parts.");
                    }

                    var nameParts = parts[2].Split('#');
                    int minLenght = int.MaxValue;
                    foreach (var item in nameParts)
                    {
                        int length = item.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                        if (length < minLenght)
                        {
                            minLenght = length;
                        }
                    }

                    int id = -1;
                    if (parts[0] != "H")
                    {
                        id = int.Parse(parts[0]);
                    }

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
                    int synsetId = id;
                    //int synsetId = int.Parse(parts[1]);
                    string synsetName = string.Join(", ", nameParts);

                    // TODO: label ID numbered from 0 sequentially is used here instead of the actual synset ID -> fix that
                    if (synsetId != -1)
                    {
                        SynsetToLabelMapping.Add(synsetId, synsetName);
                    }

                    if (!LabelToSynsetMapping.ContainsKey(synsetName))
                    {
                        LabelToSynsetMapping.Add(synsetName, new List<int> { synsetId });
                    }
                    else
                    {
                        LabelToSynsetMapping[synsetName].Add(synsetId);
                    }
                }
            }
        }


        public static KeywordLabelProvider FromDirectory(string directory)
        {
            string inputFile = Directory.GetFiles(directory)
                    .Where(dir => Path.GetFileName(dir).EndsWith(KEYWORD_LABELS_EXTENSION))
                    .FirstOrDefault();

            if (inputFile != null)
            {
                return new KeywordLabelProvider(inputFile);
            }
            else
            {
                return null;
            }
        }
    }
}
