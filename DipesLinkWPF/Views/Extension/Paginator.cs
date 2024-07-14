using DipesLink.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DipesLink.Views.Extension
{

    public class Paginator<T> : IDisposable
    {
        private bool disposedValue;
        public ObservableCollection<T>? SourceData { get; set; }
        public static int PageSize { get; private set; }
        public int CurrentPage { get; set; } = 0;
        public int TotalPages => (int)Math.Ceiling(SourceData.Count / (double)PageSize);

        public Paginator(ObservableCollection<T>? sourceData, int pageSize = 500)
        {
            if (sourceData is null) return;
            SourceData = sourceData;
            PageSize = pageSize;
        }

        public ObservableCollection<T> GetPage(int pageNumber)
        {
            var page = new ObservableCollection<T>();
            int startIndex = pageNumber * PageSize;
            int endIndex = Math.Min((pageNumber + 1) * PageSize, SourceData.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                page.Add(SourceData[i]);
            }
            CurrentPage = pageNumber;
            return page;
        }

        public int GetCurrentPageNumber(int rowIndex)
        {
            rowIndex = Math.Max(0, rowIndex);
            int pageNumber = rowIndex / PageSize;
            return pageNumber;
        }

        public bool NextPage()
        {
            if (CurrentPage < TotalPages - 1)
            {
                CurrentPage++;
                return true;
            }
            return false;
        }

        public bool PreviousPage()
        {
            if (CurrentPage > 0)
            {
                CurrentPage--;
                return true;
            }
            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SourceData?.Clear();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
    //public class Paginator : IDisposable
    //{
    //    private bool disposedValue;
    //    public ObservableCollection<ExpandoObject>? SourceData { get; set; }
    //    public static int PageSize { get; private set; }
    //    public int CurrentPage { get; set; } = 0;
    //    public int TotalPages => (int)Math.Ceiling(SourceData.Count / (double)PageSize);

    //    public Paginator(ObservableCollection<ExpandoObject>? sourceData, int pageSize = 500)
    //    {
    //        if (sourceData is null) return;
    //        SourceData = sourceData;
    //        PageSize = pageSize;
    //    }


    //    public ObservableCollection<ExpandoObject> GetPage(int pageNumber)
    //    {
    //        var page = new ObservableCollection<ExpandoObject>();
    //        int startIndex = pageNumber * PageSize;
    //        int endIndex = Math.Min((pageNumber + 1) * PageSize, SourceData.Count);

    //        for (int i = startIndex; i < endIndex; i++)
    //        {
    //            page.Add(SourceData[i]);
    //        }
    //        CurrentPage = pageNumber;
    //        return page;
    //    }

    //    public int GetCurrentPageNumber(int rowIndex)
    //    {
    //        rowIndex = Math.Max(0, rowIndex);
    //        int pageNumber = rowIndex / PageSize;
    //        return pageNumber;
    //    }

    //    public bool NextPage()
    //    {
    //        if (CurrentPage < TotalPages - 1)
    //        {
    //            CurrentPage++;
    //            return true;
    //        }
    //        return false;
    //    }

    //    public bool PreviousPage()
    //    {
    //        if (CurrentPage > 0)
    //        {
    //            CurrentPage--;
    //            return true;
    //        }
    //        return false;
    //    }

    //    protected virtual void Dispose(bool disposing)
    //    {
    //        if (!disposedValue)
    //        {
    //            if (disposing)
    //            {
    //                SourceData?.Clear();
    //            }
    //            disposedValue = true;
    //        }
    //    }

    //    public void Dispose()
    //    {
    //        Dispose(true);
    //        GC.SuppressFinalize(this);
    //    }
    //}

}
