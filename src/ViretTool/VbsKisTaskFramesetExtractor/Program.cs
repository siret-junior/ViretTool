using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Proxy;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Newtonsoft.Json;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;
using ViretTool.BusinessLayer.Services;
using ViretTool.BusinessLayer.Submission;
using ViretTool.DataLayer.DataModel;

namespace VbsKisTaskFramesetExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: print usage, parameter check
            string datasetDirectory = args[0];
            string taskDatabaseFile= args[1];
            string outputDirectory = args[2];
            Directory.CreateDirectory(outputDirectory);

            using (WindsorContainer container = new WindsorContainer(
                    new DefaultKernel(
                        new ArgumentPassingDependencyResolver(),
                        new NotSupportedProxyFactory()),
                    new DefaultComponentInstaller()))
            {
                // init business layer
                Console.WriteLine("Initializing ranking service...");
                container.AddFacility<TypedFactoryFacility>();
                container.Install(FromAssembly.This());
                IDatasetServicesManager datasetServiceManager = container.Resolve<IDatasetServicesManager>();
                datasetServiceManager.OpenDataset(datasetDirectory);

                // init services
                DatasetServices dataset = datasetServiceManager.CurrentDataset;
                IQueryPersistingService queryPersistingService = dataset.QueryPersistingService;
                IBiTemporalRankingService rankingService = dataset.RankingService;

                // load tasks
                string[] taskJsons = File.ReadAllLines(taskDatabaseFile);
                List<dynamic> tasks = new List<dynamic>();
                foreach (string taskJson in taskJsons)
                {
                    dynamic task = JsonConvert.DeserializeObject(taskJson);
                    if (task.endTimeStamp != null)
                    {
                        tasks.Add(task);
                    }
                }
                tasks = tasks
                    .Where(task => task.name.ToString().StartsWith("Visual") || task.name.ToString().StartsWith("Textual"))
                    .OrderBy(task => task.endTimeStamp)
                    .ToList();

                // add 20 seconds at the beginning of visual tasks
                foreach (dynamic task in tasks)
                {
                    if (task.name.ToString().StartsWith("Visual"))
                    {
                        task.startTimeStamp -= 20 * 1000;
                    }
                }

                // load and export results
                string outputFilePath = Path.Combine(outputDirectory, "KisTasks.txt");
                using (StreamWriter writer = new StreamWriter(outputFilePath))
                {
                    foreach (dynamic task in tasks)
                    {
                        dynamic Task = new ExpandoObject();

                        Task.name = (string)task.name;
                        Task.startTimeStamp = (string)task.startTimeStamp;
                        Task.endTimeStamp = (string)task.endTimeStamp;
                        Task.videoId = (int)(task.videoRanges[0].videoNumber - 1); // V3C1 specific decrement
                        Task.startFrame = (int)task.videoRanges[0].startFrame;
                        Task.endFrame = (int)task.videoRanges[0].endFrame;

                        // load videoFrameSet
                        Video video = dataset.DatasetService.Dataset.Videos[Task.videoId];
                        Task.videoFrameSet = video.Frames.Select(frame => frame.Id).ToArray();

                        // load shotFrameSet
                        Frame[] shotFrameSet = video.Frames.Where(frame =>
                                frame.FrameNumber >= Task.startFrame
                                && frame.FrameNumber <= Task.endFrame)
                            .ToArray();
                        Task.shotFrameSet = shotFrameSet
                            .Select(frame => frame.Id)
                            .ToArray();

                        // store result
                        writer.WriteLine(JsonConvert.SerializeObject(Task));

                        // print shot bitmaps
                        foreach (Frame frame in shotFrameSet)
                        {
                            byte[] jpegData = dataset.ThumbnailService.GetThumbnail(frame.ParentVideo.Id, frame.FrameNumber).Image;
                            using (MemoryStream stream = new MemoryStream(jpegData))
                            using (Bitmap bitmap = new Bitmap(stream))
                            {
                                string bitmapFilename = 
                                    $"v{frame.ParentVideo.Id.ToString("00000")}" +
                                    $"_f{frame.FrameNumber.ToString("000000")}" +
                                    $".jpeg";
                                string taskSubdirectory = Path.Combine(outputDirectory, Task.name);
                                Directory.CreateDirectory(taskSubdirectory);
                                string bitmapFilePath = Path.Combine(taskSubdirectory, bitmapFilename);
                                bitmap.Save(bitmapFilePath, ImageFormat.Jpeg);
                            }
                        }
                    }
                }
            }
        }
    }
}
