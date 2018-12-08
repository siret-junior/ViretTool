using System.Collections.Generic;

namespace ViretTool.PresentationLayer.Controls.Common.KeywordSearch.Suggestion
{
    /// <summary>
    /// Interface for each suggestion item, providing the actual text to write in a seach box
    /// </summary>
    interface IIdentifiable
    {
        bool HasChildren { get; }
        bool HasOnlyChildren { get; }

        IEnumerable<int> Children { get; }

        /// <summary>
        /// </summary>
        /// <returns>Class ID of the text representation</returns>
        int Id { get; }

        string TextDescription { get; }

        /// <summary>
        /// </summary>
        /// <returns>Text to be writen into a search box</returns>
        string TextRepresentation { get; }
    }
}
