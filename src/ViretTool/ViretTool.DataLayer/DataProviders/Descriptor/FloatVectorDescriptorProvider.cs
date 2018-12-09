﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.ThumbnailIO;

namespace ViretTool.DataLayer.DataProviders.Descriptor
{
    public class FloatVectorDescriptorProvider
    {
        public const string FILE_EXTENSION = ".floatvector";

        public static FloatVectorDescriptorProvider FromDirectory(string inputDirectory)
        {
            string[] files = Directory.GetFiles(inputDirectory);
            string inputFile = files.Where(path => path.EndsWith(FILE_EXTENSION)).Single();
            return FromFile(inputFile);
        }

        public static FloatVectorDescriptorProvider FromFile(string inputFile)
        {
            throw new NotImplementedException();
        }
    }
}