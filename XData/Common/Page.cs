using System.Collections.Generic;

namespace XData.Common
{
    /// <summary>
    /// 分页参数
    /// </summary>
    public class Page
    {
        private int pageIndex;
        private int pageSize = 10;

        /// <summary>
        /// 每页的记录数
        /// </summary>
        public int PageSize
        {
            get { return pageSize; }
            set
            {
                pageSize = value;
                if (pageSize < 1)
                {
                    pageSize = 1;
                }
            }
        }
        /// <summary>
        /// 页索引，从1开始
        /// </summary>
        public int PageIndex
        {
            get { return pageIndex; }
            set
            {
                pageIndex = value;
                if (pageIndex < 1)
                {
                    pageIndex = 1;
                }
            }
        }

        internal int Skip
        {
            get { return PageIndex <= 1 ? 0 : (PageIndex - 1) * PageSize; }
        }
    }

    /// <summary>
    /// 分页数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Page<T> : Page
    {
        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages
        {
            get
            {
                var c = TotalRecords / PageSize;
                var m = TotalRecords % PageSize;
                if (m > 0)
                {
                    c++;
                }
                return c;
            }
        }
        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalRecords { get; set; }
        /// <summary>
        /// 当前页数据
        /// </summary>
        public List<T> Items { get; set; }
    }
}
