using System;

namespace ViretTool.DataLayer.DataModel.Attributes
{
    public class FrameAttributes
    {
        public readonly Frame ParentFrame;

        /// <summary>
        /// Number of the frame in the source video.
        /// </summary>
        public int FrameNumber { get; private set; }

        /// <summary>
        /// Time of the frame in the source video.
        /// </summary>
        public TimeSpan Time { get; private set; }


        public FrameAttributes(Frame parentFrame)
        {
            ParentFrame = parentFrame;

            FrameNumber = -1;
            Time = TimeSpan.Zero;
        }


        public override string ToString()
        {
            return "FrameNumber: " + FrameNumber.ToString();
        }


        internal FrameAttributes WithFrameNumber(int frameNumber)
        {
            FrameNumber = frameNumber;
            return this;
        }

        internal FrameAttributes WithTime(TimeSpan time)
        {
            Time = time;
            return this;
        }

        // TODO: add attributes
    }
}
