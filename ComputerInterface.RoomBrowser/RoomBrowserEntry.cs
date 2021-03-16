using System;
using ComputerInterface.Interfaces;

namespace ComputerInterface.RoomBrowser
{
    internal class RoomBrowserEntry : IComputerModEntry
    {
        public string EntryName => "Room Browser";
        public Type EntryViewType => typeof(RoomListView);
    }
}