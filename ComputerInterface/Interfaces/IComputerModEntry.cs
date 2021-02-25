using System;

namespace ComputerInterface.Interfaces
{
    public interface IComputerModEntry
    {
        /// <summary>
        /// Name of the entry that is shown in the menu
        /// can be the name of the mod for example
        /// </summary>
        string EntryName { get; }

        /// <summary>
        /// The Type of the first View that is going to be shown.
        /// Needs to be of type <see cref="IComputerView"/>
        /// </summary>
        Type EntryViewType { get; }
    }
}