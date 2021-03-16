using System;
using UnityEngine;

namespace ComputerInterface.ViewLib
{
    public class UIElementPageHandler<T> : UIPageHandler
    {
        private T[] _elements;

        public UIElementPageHandler(EKeyboardKey prevKey, EKeyboardKey nextKey) : base(prevKey, nextKey)
        {
            
        }

        public UIElementPageHandler()
        {
            
        }

        /// <summary>
        /// Sets the elements for the pages
        /// </summary>
        /// <param name="elements"></param>
        public void SetElements(T[] elements)
        {
            _elements = elements;
            MaxPage = Mathf.CeilToInt((float)elements.Length / EntriesPerPage) - 1;
            CurrentPage = 0;
        }

        /// <summary>
        /// iterates through the elements of the given page
        /// and returns them with the callback
        /// </summary>
        /// <param name="page"></param>
        /// <param name="elementCallback">Callback with (Element T, Index i)</param>
        public void DrawElements(int page, Action<T, int> elementCallback)
        {
            if (elementCallback == null)
            {
                return;
            }

            var elements = GetElementsForPage(page);
            for (int i = 0; i < elements.Length; i++)
            {
                elementCallback(elements[i], i);
            }
        }

        /// <summary>
        /// iterates through the elements of the current page
        /// and returns them with the callback
        /// </summary>
        /// <param name="elementCallback">Callback with (Element T, Index i)</param>
        public void DrawElements(Action<T, int> elementCallback)
        {
            if (elementCallback == null)
            {
                return;
            }

            var elements = GetElementsForPage(CurrentPage);
            for (int i = 0; i < elements.Length; i++)
            {
                elementCallback(elements[i], i);
            }
        }

        /// <summary>
        /// Gets the elements for the given page
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public T[] GetElementsForPage(int page)
        {
            if (_elements == null)
            {
                Debug.LogError("Elements are not set yet\nPlease set the lines first");
                return null;
            }

            var startIdx = EntriesPerPage * page;
            ItemsOnScreen = Math.Min(EntriesPerPage, _elements.Length - startIdx);
            var pageElements = new T[ItemsOnScreen];
            for (int i = 0; i < ItemsOnScreen; i++)
            {
                pageElements[i] = _elements[startIdx + i];
            }

            return pageElements;
        }
    }
}