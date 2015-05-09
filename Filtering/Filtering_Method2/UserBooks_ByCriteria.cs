using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Filtering_Method2
{
    public class UserBooks_ByCriteria : AbstractIndexCreationTask<UserBook, BookProjection>
    {
        public UserBooks_ByCriteria()
        {
            Map = userBooks =>
                from userBook in userBooks
                let book = LoadDocument<Book>(userBook.BookId)
                select new
                {
                    userBook.Id,
                    userBook.UserId,
                    book.Authors,
                    book.Publisher,
                    userBook.Tags,
                    userBook.Rating,
                    userBook.Recommended,
                    userBook.Shelves
                };
        }
    }
}
