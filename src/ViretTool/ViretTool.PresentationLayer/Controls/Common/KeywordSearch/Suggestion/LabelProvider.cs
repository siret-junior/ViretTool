﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ViretTool.PresentationLayer.Controls.Common.KeywordSearch.Suggestion
{
    class LabelProvider
    {
        private readonly string _filePath;
        private readonly Dictionary<string, List<int>> _idMapping = new Dictionary<string, List<int>>();

        private readonly Dictionary<int, Label> _labels = new Dictionary<int, Label>();

        /// <summary>
        /// Asynchronously loads <see cref="Labels"/> from filePath argument.
        /// </summary>
        /// <param name="filePath">Relative or absolute location of .labels file.</param>
        public LabelProvider(string filePath)
        {
            _filePath = filePath;
            LoadTask = Task.Factory.StartNew(LoadFromFile);
        }

        /// <summary>
        /// Mapping of label names to WordNet synset ids
        /// </summary>
        public Dictionary<string, List<int>> IdMapping { get { return (LoadTask.Status == TaskStatus.RanToCompletion) ? _idMapping : null; } }

        /// <summary>
        /// Mapping of WordNet synset ids to labels
        /// </summary>
        public Dictionary<int, Label> Labels { get { return (LoadTask.Status == TaskStatus.RanToCompletion) ? _labels : null; } }

        /// <summary>
        /// Task responsible for filling <see cref="Labels"/>. Access <see cref="Labels"/> only after completion.
        /// </summary>
        public Task LoadTask { get; private set; }

        private int[] GetSynsetIds(string text)
        {
            if (text.Length == 0)
            {
                return null;
            }

            string[] parts = text.Split('#');
            int[] ints = new int[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                ints[i] = int.Parse(parts[i]);
            }

            return ints;
        }

        private void LoadFromFile()
        {
            using (StreamReader reader = new StreamReader(_filePath))
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

                    int id = -1;
                    if (parts[0] != "H")
                    {
                        id = int.Parse(parts[0]);
                    }

                    Label label = new Label()
                                {
                                    //Id = id,
                                    IsOnlyHypernym = (id == -1),
                                    SynsetId = int.Parse(parts[1]),
                                    Name = string.Join(", ", nameParts),
                                    Names = nameParts,
                                    Hyponyms = GetSynsetIds(parts[3]),
                                    Hypernyms = GetSynsetIds(parts[4]),
                                    Description = parts[5],
                                    NameLenghtInWords = minLenght
                                };

                    _labels.Add(label.SynsetId, label);

                    if (!_idMapping.ContainsKey(label.Name))
                    {
                        _idMapping.Add(label.Name, new List<int> { label.SynsetId });
                    }
                    else
                    {
                        _idMapping[label.Name].Add(label.SynsetId);
                    }
                }
            }
        }
    }
}
