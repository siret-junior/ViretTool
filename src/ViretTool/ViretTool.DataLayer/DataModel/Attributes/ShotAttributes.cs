using System;

namespace ViretTool.DataLayer.DataModel.Attributes
{
    public class ShotAttributes
    {
        public readonly Shot ParentShot;

        /// <summary>
        /// Time length of the shot.
        /// </summary>
        public TimeSpan Length { get; private set; }


        public ShotAttributes(Shot parentShot)
        {
            ParentShot = parentShot;

            Length = TimeSpan.Zero;
        }


        public override string ToString()
        {
            return Length.ToString(@"hh\:mm\:ss\:fff");
        }


        internal ShotAttributes WithLength(TimeSpan length)
        {
            Length = length;
            return this;
        }
    }
}
