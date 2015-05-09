using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filtering_Method2
{
    public class User
    {
        public string Id { get; set; }
    }

    public class Shelf
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    public class UserBook
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string BookId { get; set; }

        public DateTime DateAdded { get; set; }

        public string[] Tags { get; set; }

        public int Rating { get; set; }

        public bool Recommended { get; set; }

        public string[] Shelves { get; set; }
    }

    public class BookOnShelf
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string ShelfId { get; set; }

        public string UserBookId { get; set; }

        /// <summary>
        /// When the book was added to the shelf
        /// </summary>
        public DateTime DateAddedToShelf { get; set; }

        /// <summary>
        /// Note for book on this shelf
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Custom sort order on the shelf
        /// </summary>
        public int Order { get; set; }
    }

    public class Book
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string[] Authors { get; set; }

        public string Publisher { get; set; }
    }


    public class BookProjection
    {
        public string Id { get; set; }

        public string BookId { get; set; }

        public string UserId { get; set; }

        public DateTime DateAdded { get; set; }

        public string Title { get; set; }

        public string[] Authors { get; set; }

        public string Publisher { get; set; }

        public string[] Tags { get; set; }

        public int Rating { get; set; }

        public bool Recommended { get; set; }

        public string[] Shelves { get; set; }
    }

    public class BookOnShelfProjection : BookProjection
    {
        public string ShelfId { get; set; }

        public DateTime DateAddedToShelf { get; set; }

        public string Note { get; set; }

        public int Order { get; set; }
    }
}
