using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Indexes;

namespace Filtering_Method2
{
    public class UserBooks_ByShelfId_BookOnShelfProjection : AbstractTransformerCreationTask<UserBook>
    {
        public UserBooks_ByShelfId_BookOnShelfProjection()
        {
            TransformResults = userBooks =>
                from userBook in userBooks
                let bos = LoadDocument<BookOnShelf>(Parameter("ShelfId") + "/" + userBook.BookId)
                let book = LoadDocument<Book>(userBook.BookId)
                where bos != null
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
