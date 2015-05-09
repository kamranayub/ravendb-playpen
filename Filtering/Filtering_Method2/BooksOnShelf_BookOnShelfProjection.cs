using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Indexes;

namespace Filtering_Method2
{
    public class BooksOnShelf_BookOnShelfProjection : AbstractTransformerCreationTask<BookOnShelf>
    {
        public BooksOnShelf_BookOnShelfProjection()
        {
            TransformResults = booksOnShelves =>
                from bos in booksOnShelves
                where bos != null
                let userBook = LoadDocument<UserBook>(bos.UserBookId)
                let book = LoadDocument<Book>(userBook.BookId)
                select new
                {
                    userBook.Id,
                    userBook.UserId,
                    BookId = book.Id,
                    book.Title,
                    book.Authors,
                    book.Publisher,
                    userBook.Tags,
                    userBook.Rating,
                    userBook.Recommended,
                    userBook.DateAdded,
                    bos.DateAddedToShelf,
                    bos.ShelfId,
                    bos.Order,
                    bos.Note
                };
        }
    }
}
