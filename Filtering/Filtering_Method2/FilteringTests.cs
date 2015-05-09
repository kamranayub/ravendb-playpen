using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Linq;
using Raven.Tests.Helpers;

namespace Filtering_Method2
{
    [TestFixture]
    public class FilteringTests : RavenTestBase
    {
        private IDocumentStore _store;

        [SetUp]
        public void Seed()
        {
            _store = NewDocumentStore();

            using (var session = _store.OpenSession())
            {
                var book1 = new Book()
                {
                    Id = "books/1",
                    Authors = new string[] { "Stephen King" },
                    Publisher = "Clearinghouse",
                    Title = "The Shining"
                };
                session.Store(book1);
                var book2 = new Book()
                {
                    Id = "books/2",
                    Authors = new string[] { "Damsel Grigsby" },
                    Publisher = "Gray Enterprises",
                    Title = "50 Shades of Gray"
                };
                session.Store(book2);
                var book3 = new Book()
                {
                    Id = "books/3",
                    Authors = new string[] { "Stephen King", "Sortie Fuller" },
                    Publisher = "Clearinghouse",
                    Title = "How I Learned to Love Myself (and Stay Sane)"
                };
                session.Store(book3);
                var book4 = new Book()
                {
                    Id = "books/4",
                    Authors = new string[] { "Neal Stephenson" },
                    Publisher = "Atlas Paper",
                    Title = "Cryptonomicon"
                };
                session.Store(book4);

                var user = new User()
                {
                    Id = "users/1",                    
                    
                };

                var shelves = new Shelf[]
                {
                    new Shelf()
                    {
                        Id = "users/1/shelves/1",
                        Name = "Want to Read"
                    },
                    new Shelf()
                    {
                        Id = "users/1/shelves/2",
                        Name = "Reading"
                    },
                    new Shelf()
                    {
                        Id = "users/1/shelves/3",
                        Name = "Read"
                    }
                };

                foreach (var shelf in shelves)
                {
                    session.Store(shelf);
                }

                var userBooks = new List<UserBook>()
                {
                    new UserBook()
                    {
                        Id = "users/1/books/1",
                        UserId = "users/1",
                        BookId = "books/1",
                        Rating = 5,
                        Tags = new string[] {"awesome", "borrowed"},
                        Recommended = true,
                        Shelves = new [] { "users/1/shelves/3" }
                    },
                    new UserBook()
                    {
                        Id = "users/1/books/2",
                        UserId = "users/1",
                        BookId = "books/2",
                        Rating = 1,
                        Tags = new string[] {"sucked"},
                        Recommended = false,
                        Shelves = new [] { "users/1/shelves/2" }
                    },
                    new UserBook()
                    {
                        Id = "users/1/books/3",
                        UserId = "users/1",
                        BookId = "books/3",
                        Rating = 0,
                        Tags = null,
                        Recommended = false,
                        Shelves = new [] { "users/1/shelves/1", "users/1/shelves/2" }
                    },
                    new UserBook()
                    {
                        Id = "users/1/books/4",
                        UserId = "users/1",
                        BookId = "books/4",
                        Rating = 4,
                        Tags = null,
                        Recommended = false
                    }
                };

                foreach (var userBook in userBooks)
                {
                    session.Store(userBook);
                }

                var booksOnShelves = new BookOnShelf[]
                {
                    new BookOnShelf()
                    {
                        DateAddedToShelf = DateTime.Now.AddDays(-5),
                        Id = "users/1/shelves/1/books/3",
                        UserId = "users/1",
                        ShelfId = "users/1/shelves/1",
                        UserBookId = "users/1/books/3"
                    },
                    new BookOnShelf()
                    {
                        DateAddedToShelf = DateTime.Now.AddDays(-15),
                        Id = "users/1/shelves/2/books/2",
                        UserId = "users/1",
                        ShelfId = "users/1/shelves/2",
                        UserBookId = "users/1/books/2",
                        Note = "Loved it!"
                    },
                    new BookOnShelf()
                    {
                        DateAddedToShelf = DateTime.Now.AddDays(-15),
                        Id = "users/1/shelves/2/books/3",
                        UserId = "users/1",
                        ShelfId = "users/1/shelves/2",
                        UserBookId = "users/1/books/3"
                    },
                    new BookOnShelf()
                    {
                        DateAddedToShelf = DateTime.Now,
                        Id = "users/1/shelves/3/books/1",
                        UserId = "users/1",
                        ShelfId = "users/1/shelves/3",
                        UserBookId = "users/1/books/1"
                    }
                };

                foreach (var bos in booksOnShelves)
                {
                    session.Store(bos);
                }

                session.Store(user);
                session.SaveChanges();

                new UserBooks_ByCriteria().Execute(_store);
                new UserBooks_BookProjection().Execute(_store);
                new UserBooks_ByShelfId_BookOnShelfProjection().Execute(_store);
                new BooksOnShelf_BookOnShelfProjection().Execute(_store);
            }
        }

        [TearDown]
        public void Teardown()
        {
            _store.Dispose();
        }

        [Test]
        public void CanReturnUser()
        {
            using (var session = _store.OpenSession())
            {
                var user = session.Load<User>("users/1");

                Assert.NotNull(user);
            }
        }

        [Test]
        public void CanReturnAllBooks()
        {
            using (var session = _store.OpenSession())
            {
                var books = session.Advanced.LoadStartingWith<UserBooks_BookProjection, BookProjection[]>("users/1/books/");

                Assert.NotNull(books);
                Assert.AreEqual(4, books.Length);
            }
        }

        [Test]
        public void CanFilterByTag()
        {
            using (var session = _store.OpenSession())
            {
                var filtered = session.Query<BookProjection, UserBooks_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(ub => ub.UserId == "users/1")
                    .Where(ub => ub.Tags.Contains("awesome"))
                    .TransformWith<UserBooks_BookProjection, BookProjection>()
                    .ToList();

                Assert.NotNull(filtered);
                Assert.AreEqual(1, filtered.Count);
                Assert.AreEqual("books/1", filtered[0].BookId);
            }
        }

        [Test]
        public void CanFilterByShelf()
        {
            using (var session = _store.OpenSession())
            {
                var booksInShelf =
                    session.Advanced.LoadStartingWith<BooksOnShelf_BookOnShelfProjection, BookOnShelfProjection>(
                        "users/1/shelves/2/books/");

                Assert.NotNull(booksInShelf);
                Assert.AreEqual(2, booksInShelf.Length);
                Assert.AreEqual("books/2", booksInShelf[0].BookId);
                Assert.AreNotEqual(DateTime.MinValue, booksInShelf[0].DateAddedToShelf);
            }
        }

        [Test]
        public void CanFilterByShelfAndAuthor()
        {
            using (var session = _store.OpenSession())
            {
                var filtered = session.Query<BookProjection, UserBooks_ByCriteria>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(ub => ub.Authors.Contains("Stephen King"))
                    .AddTransformerParameter("ShelfId", "users/1/shelves/2")
                    .TransformWith<UserBooks_ByShelfId_BookOnShelfProjection, BookOnShelfProjection>()
                    .ToList();
                
                Assert.AreEqual(1, filtered.Count);
                Assert.AreEqual("books/3", filtered[0].BookId);
                Assert.AreNotEqual(DateTime.MinValue, filtered[0].DateAddedToShelf);
                Assert.AreEqual(1, session.Advanced.NumberOfRequests);
            }
        }

        [Test]
        public void CanFilterByShelves()
        {
            using (var session = _store.OpenSession())
            {
                var filtered = session.Query<BookProjection, UserBooks_ByCriteria>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(ub => ub.Shelves.Contains("users/1/shelves/2") || ub.Shelves.Contains("users/1/shelves/3"))
                    .TransformWith<UserBooks_BookProjection, BookProjection>()
                    .ToList();

                Assert.AreEqual(3, filtered.Count);
                Assert.AreEqual("books/1", filtered[0].BookId);
                Assert.AreEqual("books/2", filtered[1].BookId);
                Assert.AreEqual("books/3", filtered[2].BookId);
            }
        }

        [Test]
        public void CanFilterByRating()
        {
            using (var session = _store.OpenSession())
            {
                var filtered = session.Query<BookProjection, UserBooks_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")
                    .Where(bs => bs.Rating == 4)
                    .TransformWith<UserBooks_BookProjection, BookProjection>()
                    .ToList();

                Assert.NotNull(filtered);
                Assert.AreEqual(1, filtered.Count);
                Assert.AreEqual("books/4", filtered[0].BookId);
            }
        }

        [Test]
        public void CanFilterByRecommended()
        {
            using (var session = _store.OpenSession())
            {
                var filtered = session.Query<BookProjection, UserBooks_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")
                    .Where(bs => !bs.Recommended)
                    .TransformWith<UserBooks_BookProjection, BookProjection>()
                    .ToList();

                Assert.NotNull(filtered);
                Assert.AreEqual(3, filtered.Count);
            }
        }

        [Test]
        public void CanFilterByAuthor()
        {
            using (var session = _store.OpenSession())
            {
                var filtered = session.Query<BookProjection, UserBooks_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")                    
                    .Where(bs => bs.Authors.Contains("Stephen King"))
                    .TransformWith<UserBooks_BookProjection, BookProjection>()
                    .ToList();

                Assert.NotNull(filtered);
                Assert.AreEqual(2, filtered.Count);
            }
        }

        [Test]
        public void CanFilterByAuthorsUsingOR()
        {
            using (var session = _store.OpenSession())
            {
                var filtered = session.Query<BookProjection, UserBooks_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")                    
                    .Where(bs => bs.Authors.Contains("Stephen King") || bs.Authors.Contains("Damsel Grigsby"))
                    .TransformWith<UserBooks_BookProjection, BookProjection>()
                    .ToList();

                Assert.NotNull(filtered);
                Assert.AreEqual(3, filtered.Count);
            }
        }

        [Test]
        public void CanFilterByAuthorsUsingAND()
        {
            using (var session = _store.OpenSession())
            {
                var filtered = session.Query<BookProjection, UserBooks_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")                    
                    .Where(bs => bs.Authors.Contains("Stephen King") && bs.Authors.Contains("Sortie Fuller"))
                    .TransformWith<UserBooks_BookProjection, BookProjection>()
                    .ToList();

                Assert.NotNull(filtered);
                Assert.AreEqual(1, filtered.Count);
            }
        }

        [Test]
        public void CanFilterByTagOrAuthor()
        {
            using (var session = _store.OpenSession())
            {
                var filtered = session.Query<BookProjection, UserBooks_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")
                    .Where(bs => bs.Tags.Contains("awesome") || bs.Authors.Contains("Sortie Fuller"))
                    .TransformWith<UserBooks_BookProjection, BookProjection>()
                    .ToList();

                Assert.NotNull(filtered);
                Assert.AreEqual(2, filtered.Count);
            }
        }

        [Test]
        public void CanFilterByRatingAndAuthor()
        {
            using (var session = _store.OpenSession())
            {
                var filtered = session.Query<BookProjection, UserBooks_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")
                    .Where(bs => bs.Rating == 0 && bs.Authors.Contains("Sortie Fuller"))
                    .TransformWith<UserBooks_BookProjection, BookProjection>()
                    .ToList();

                Assert.NotNull(filtered);
                Assert.AreEqual(1, filtered.Count);
            }
        }

        [Test]
        public void CanGetFacets()
        {
            using (var session = _store.OpenSession())
            {
                var bookFacets = new List<Facet>()
                {
                    new Facet<BookProjection>()
                    {
                        Name = x => x.Authors
                    },
                    new Facet<BookProjection>()
                    {
                        Name = x => x.Publisher
                    },
                    new Facet<BookProjection>()
                    {
                        Name = x => x.Tags
                    },
                    new Facet<BookProjection>()
                    {
                        Name = x => x.Rating
                    },
                    new Facet<BookProjection>()
                    {
                        Name = x => x.Recommended
                    },
                    new Facet<BookProjection>()
                    {
                        Name = x => x.Shelves
                    }
                };

                // all filterable facets for a single user's books
                var facetResults = session.Query<UserBook, UserBooks_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(bs => bs.UserId == "users/1")
                    .ToFacets(bookFacets);

                Assert.NotNull(facetResults);
                Assert.AreEqual(4, facetResults.Results["Authors"].Values.Count);
                Assert.AreEqual(4, facetResults.Results["Shelves"].Values.Count); // includes NULL_VALUE
            }
        }
    }
}
