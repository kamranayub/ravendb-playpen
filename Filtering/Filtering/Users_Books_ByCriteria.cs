using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Filtering
{
    public class Users_Books_ByCriteria : AbstractIndexCreationTask<User, BookProjection>
    {
        public Users_Books_ByCriteria()
        {
            Map = users => 
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
                    Shelves = shelfEntries.Any() ? shelfEntries.Select(x => x.Id) : null,
                    ShelfEntries = shelfEntries
                };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}
