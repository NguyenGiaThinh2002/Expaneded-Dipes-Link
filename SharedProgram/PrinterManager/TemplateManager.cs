using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SharedProgram.PrinterManager
{
    public class TemplateManager
    {
        // A readonly list to hold multiple template lists
        public readonly ObservableCollection<List<string>> TemplateLists = new();

        // A separate readonly list to serve as the first-found template list
        public readonly ObservableCollection<List<string>> TemplateListFirstFound = new();

        public readonly List<string> PrinterTemplateList = new();

        //private List<string> _PrinterTemplateList = new List<string>(Enumerable.Repeat(string.Empty, 4));
        //public List<string> PrinterTemplateList
        //{
        //    get { return _PrinterTemplateList; }
        //    set { _PrinterTemplateList = value; OnPropertyChanged(); }
        //}

        /// <summary>
        /// Initializes the TemplateManager and creates the specified number of template lists for both collections.
        /// </summary>
        /// <param name="listCount">The number of template lists to create.</param>
        public TemplateManager(int listCount)
        {
            for (int i = 0; i < listCount; i++)
            {
                TemplateLists.Add(new List<string>());
                TemplateListFirstFound.Add(new List<string>());
                PrinterTemplateList.Add("");
            }
        }

        public TemplateManager()
        {
            
        }

        /// <summary>
        /// Adds a template to a specific list in _templateLists by index.
        /// </summary>
        /// <param name="listIndex">The index of the list to add the template to.</param>
        /// <param name="template">The template to add.</param>
        public void AddTemplateToMain(int listIndex, string template)
        {
            if (listIndex >= 0 && listIndex < TemplateLists.Count)
            {
                var list = TemplateLists[listIndex];
                if (!string.IsNullOrWhiteSpace(template) && !list.Contains(template))
                {
                    list.Add(template);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(listIndex), "List index is out of range.");
            }
        }

        /// <summary>
        /// Adds a template to a specific list in TemplateListFirstFound by index.
        /// </summary>
        /// <param name="listIndex">The index of the list to add the template to.</param>
        /// <param name="template">The template to add.</param>
        public void AddTemplateToFirstFound(int listIndex, string template)
        {
            if (listIndex >= 0 && listIndex < TemplateListFirstFound.Count)
            {
                var list = TemplateListFirstFound[listIndex];
                if (!string.IsNullOrWhiteSpace(template) && !list.Contains(template))
                {
                    list.Add(template);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(listIndex), "List index is out of range.");
            }
        }

        /// <summary>
        /// Retrieves a template list from _templateLists by its index.
        /// </summary>
        /// <param name="listIndex">The index of the list to retrieve.</param>
        /// <returns>The template list at the specified index.</returns>
        public List<string>? GetMainTemplateList(int listIndex)
        {
            return listIndex >= 0 && listIndex < TemplateLists.Count ? TemplateLists[listIndex] : null;
        }

        /// <summary>
        /// Retrieves a template list from TemplateListFirstFound by its index.
        /// </summary>
        /// <param name="listIndex">The index of the list to retrieve.</param>
        /// <returns>The template list at the specified index.</returns>
        public List<string>? GetFirstFoundTemplateList(int listIndex)
        {
            return listIndex >= 0 && listIndex < TemplateListFirstFound.Count ? TemplateListFirstFound[listIndex] : null;
        }
    }
}
