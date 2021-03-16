using System;
using UnityEngine;

namespace ComputerInterface.ViewLib
{
    public class UITextPageHandler : UIPageHandler
    {
        private string[] _lines;

        public UITextPageHandler(EKeyboardKey prevKey, EKeyboardKey nextKey) : base(prevKey, nextKey)
        {
            
        }

        public UITextPageHandler()
        {
            
        }

        /// <summary>
        /// Sets the text for the pager
        /// Lines are identified by \n
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            SetLines(text.Split('\n'));
        }

        /// <summary>
        /// Sets the lines of the pager
        /// </summary>
        /// <param name="lines"></param>
        public void SetLines(string[] lines)
        {
            _lines = lines;
            MaxPage = Mathf.CeilToInt((float)lines.Length / EntriesPerPage) - 1;
            CurrentPage = 0;
        }

        /// <summary>
        /// Get the lines for the given page
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public string[] GetLinesForPage(int page)
        {
            if (_lines == null)
            {
                Debug.LogError("Lines are not set yet\nPlease set the lines first");
            }

            var startIdx = EntriesPerPage * page;
            ItemsOnScreen = Math.Min(EntriesPerPage, _lines.Length - startIdx);
            var pageLines = new string[ItemsOnScreen];
            for (int i = 0; i < ItemsOnScreen; i++)
            {
                pageLines[i] = _lines[startIdx + i];
            }

            return pageLines;
        }

        /// <summary>
        /// Get the lines for the current page
        /// </summary>
        /// <returns></returns>
        public string[] GetLinesForCurrentPage()
        {
            return GetLinesForPage(CurrentPage);
        }

        /// <summary>
        /// Get the text for the given page
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public string GetTextForPage(int page)
        {
            return string.Join("\n", GetLinesForPage(page));
        }

        /// <summary>
        /// Get the text for the current page
        /// </summary>
        /// <returns></returns>
        public string GetTextForCurrentPage()
        {
            return GetTextForPage(CurrentPage);
        }
    }
}