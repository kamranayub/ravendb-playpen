using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Indexes;

namespace Filtering_Method2
{
    public class UserBooks_BookProjection : AbstractTransformerCreationTask<UserBook>
    {
        public UserBooks_BookProjection()
        {
            TransformResults = userBooks =>
                from userBook in userBooks
                let book = LoadDocument<Book>(userBook.BookId)
                let shelfEntries = from bos in LoadDocument<BookOnShelf>(userBook.Shelves.Select(x => x + "/" + userBook.BookId))
                                   select new
                                   {
                                       bos.ShelfId,
                                       bos.DateAddedToShelf,
                                       bos.Order,
                                       bos.Note
                                   }
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
                    userBook.Shelves,
                    ShelfEntries = shelfEntries
                };
        }
    }
}
