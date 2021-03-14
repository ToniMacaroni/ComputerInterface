using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ComputerInterface.ViewLib
{
    public class UIPageHandler
    {
        public int CurrentPage { get; private set; }

        /// <summary>
        /// Last Page
        /// </summary>
        public int MaxPage { get; private set; }

        /// <summary>
        /// How many lines are allowed per page
        /// </summary>
        public int EntriesPerPage { get; set; }

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

        private string[] _lines;
        private int _itemsOnScreen;

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
        /// <param name="line"></param>
        /// <returns>line number relative to the page</returns>
        public int MovePageToLine(int line)
        {
            var page = Mathf.FloorToInt((float) line / EntriesPerPage);
            CurrentPage = page;
            return line % EntriesPerPage;
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
            var startIdx = EntriesPerPage * page;
            _itemsOnScreen = Math.Min(EntriesPerPage, _lines.Length - startIdx);
            var pageLines = new string[_itemsOnScreen];
            for (int i = 0; i < _itemsOnScreen; i++)
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

        public void AppendFooter(StringBuilder str)
        {
            for (int i = 0; i < EntriesPerPage - _itemsOnScreen; i++)
            {
                str.AppendLine();
            }

            str.Append(GetFooter());
        }

        private string GetFooter()
        {
            return string.Format(Footer,
                CurrentPage > 0 ? PrevMark : "  ",
                CurrentPage < MaxPage ? NextMark : "  ",
                CurrentPage, MaxPage);
        }
    }
}