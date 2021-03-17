using System.Text;
using UnityEngine;

namespace ComputerInterface.ViewLib
{
    public class UIPageHandler
    {
        public int CurrentPage { get; set; }

        /// <summary>
        /// Last Page (0 indexed)
        /// </summary>
        public int MaxPage { get; protected set; }

        /// <summary>
        /// How many lines are allowed per page
        /// </summary>
        public int EntriesPerPage { get; set; }

        /// <summary>
        /// How many elements are on the current page
        /// </summary>
        public int ItemsOnScreen { get; protected set; }

        /// <summary>
        /// 0 = left mark (<!--<-->)
        /// 1 = right mark (>)
        /// 2 = current page
        /// 3 = max page
        /// </summary>
        public string Footer = "{0} {2}/{3} {1}";

        public string PrevMark = "<";
        public string NextMark = ">";

        private readonly bool _useKeys;
        private readonly EKeyboardKey _prevKey;
        private readonly EKeyboardKey _nextKey;


        public UIPageHandler(EKeyboardKey prevKey, EKeyboardKey nextKey)
        {
            _prevKey = prevKey;
            _nextKey = nextKey;
            _useKeys = true;
        }

        public UIPageHandler()
        {
        }

        public bool HandleKeyPress(EKeyboardKey key)
        {
            if (!_useKeys) return false;

            if (key == _prevKey)
            {
                PreviousPage();
                return true;
            }

            if (key == _nextKey)
            {
                NextPage();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Goes to the next page
        /// </summary>
        /// <returns></returns>
        public void NextPage()
        {
            if (CurrentPage < MaxPage)
            {
                CurrentPage++;
            }
        }

        /// <summary>
        /// Goes to the previous page
        /// </summary>
        /// <returns></returns>
        public void PreviousPage()
        {
            if (CurrentPage > 0)
            {
                CurrentPage--;
            }
        }

        /// <summary>
        /// Advances the page to the specidied line
        /// </summary>
        /// <param name="idx"></param>
        /// <returns>line number relative to the page</returns>
        public int MovePageToIdx(int idx)
        {
            var page = Mathf.FloorToInt((float)idx / EntriesPerPage);
            CurrentPage = page;
            return idx % EntriesPerPage;
        }

        /// <summary>
        /// Given the index of an item relative to the page
        /// returns the absolute index
        /// </summary>
        /// <param name="page"></param>
        /// <param name="itemIdx"></param>
        /// <returns></returns>
        public int GetAbsoluteIndex(int page, int itemIdx)
        {
            return page * EntriesPerPage + itemIdx;
        }

        /// <summary>
        /// Given the index of an item relative to the page
        /// returns the absolute index
        /// </summary>
        /// <param name="itemIdx"></param>
        /// <returns></returns>
        public int GetAbsoluteIndex(int itemIdx)
        {
            return GetAbsoluteIndex(CurrentPage, itemIdx);
        }

        public void AppendFooter(StringBuilder str)
        {
            for (int i = 0; i < EntriesPerPage - ItemsOnScreen; i++)
            {
                str.AppendLine();
            }

            str.Append(GetFooter());
        }

        private string GetFooter()
        {
            return string.Format(Footer,
                CurrentPage > 0 ? PrevMark : " ",
                CurrentPage < MaxPage ? NextMark : " ",
                CurrentPage+1, MaxPage+1);
        }
    }
}