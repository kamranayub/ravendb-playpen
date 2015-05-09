using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Indexes;

namespace Filtering
{
    public class Users_Books_OnShelf : AbstractTransformerCreationTask<User>
    {
        public Users_Books_OnShelf()
        {
            TransformResults = users =>
                from user in users
                from userBook in user.Books
                let book = LoadDocument<Book>(userBook.BookId)
                let shelfEntries = from shelf in user.Shelves
                              from bos in shelf.Books
                              where bos.UserBookId == userBook.Id
                              select new { shelf.Id, bos.DateAddedToShelf }
                
                select new
                {
                    UserId = user.Id,
                    BookId = book.Id,
                    userBook.Id,
                    book.Title,
                    book.Authors,
                    book.Publisher,
                    userBook.Tags,
                    userBook.Rating,
                    userBook.Recommended,
                    userBook.DateAdded,
                    Shelves = shelfEntries.Select(x => x.Id),
                    ShelfEntries = shelfEntries
                };
        }
    }
}
