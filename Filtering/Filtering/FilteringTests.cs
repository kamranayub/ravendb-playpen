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

namespace Filtering
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
                    Authors = new string[] {"Stephen King"},
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
                    Books = new List<UserBook>()
                    {
                        new UserBook()
                        {
                            Id = "users/1/books/1",
                            BookId = "books/1",
                            Rating = 5,
                            Tags = new string[] { "awesome", "borrowed" },
                            Recommended = true
                        },
                        new UserBook()
                        {
                            Id = "users/1/books/2",
                            BookId = "books/2",
                            Rating = 1,
                            Tags = new string[] { "sucked" },
                            Recommended = false
                        },
                        new UserBook()
                        {
                            Id = "users/1/books/3",
                            BookId = "books/3",
                            Rating = 0,
                            Tags = null,
                            Recommended = false
                        },
                        new UserBook()
                        {
                            Id = "users/1/books/4",
                            BookId = "books/4",
                            Rating = 4,
                            Tags = null,
                            Recommended = false
                        }
                    },
                    Shelves = new Shelf[]
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
                    }
                };

                user.Shelves[0].Books = new BookOnShelf[]
                {
                    new BookOnShelf()
                    {
                        DateAddedToShelf = DateTime.Now.AddDays(-5),
                        Id = "users/1/shelves/1/books/3",
                        UserBookId = "users/1/books/3"
                    }
                };
                user.Shelves[1].Books = new BookOnShelf[]
                {
                    new BookOnShelf()
                    {
                        DateAddedToShelf = DateTime.Now.AddDays(-15),
                        Id = "users/1/shelves/2/books/2",
                        UserBookId = "users/1/books/2"
                    },
                    new BookOnShelf()
                    {
                        DateAddedToShelf = DateTime.Now.AddDays(-15),
                        Id = "users/1/shelves/2/books/2",
                        UserBookId = "users/1/books/3"
                    }
                };
                user.Shelves[2].Books = new BookOnShelf[]
                {
                    new BookOnShelf()
                    {
                        DateAddedToShelf = DateTime.Now,
                        Id = "users/1/shelves/3/books/1",
                        UserBookId = "users/1/books/1"
                    }
                };

                session.Store(user);
                session.SaveChanges();

                new Users_Books_ByCriteria().Execute(_store);
                new Users_Books_OnShelf().Execute(_store);
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
                var books = session.Load<Users_Books_OnShelf, BookProjection[]>("users/1");

                Assert.NotNull(books);
                Assert.AreEqual(4, books.Length);
            }
        }

        [Test]
        public void CanFilterByTag()
        {
            using (var session = _store.OpenSession())
            {
                var filtered = session.Query<BookProjection, Users_Books_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")
                    .Where(bs => bs.Tags.Contains("awesome"))
                    .ProjectFromIndexFieldsInto<BookProjection>()
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
                var filtered = session.Query<BookProjection, Users_Books_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")
                    .Where(bs => bs.Shelves.Contains("users/1/shelves/2"))
                    .ProjectFromIndexFieldsInto<BookProjection>()
                    .ToList();

                Assert.NotNull(filtered);
                Assert.AreEqual(2, filtered.Count);
                Assert.AreEqual("books/2", filtered[0].BookId);
                Assert.AreNotEqual(DateTime.MinValue, filtered[0].ShelfEntries.First(x => x.Id == "users/1/shelves/2").DateAddedToShelf);
            }
        }

        [Test]
        public void CanFilterByShelfAndAuthor()
        {
            using (var session = _store.OpenSession())
            {
                var filtered = session.Query<BookProjection, Users_Books_ByCriteria>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(ub => ub.Authors.Contains("Stephen King"))
                    .Where(ub => ub.Shelves.Contains("users/1/shelves/2"))
                    .ProjectFromIndexFieldsInto<BookProjection>()
                    .ToList();

                Assert.AreEqual(1, filtered.Count);
                Assert.AreEqual("books/3", filtered[0].BookId);
                Assert.AreNotEqual(DateTime.MinValue, filtered[0].ShelfEntries.First(x => x.Id == "users/1/shelves/2").DateAddedToShelf);
                Assert.AreEqual(1, session.Advanced.NumberOfRequests);
            }
        }

        [Test]
        public void CanFilterByShelves()
        {
            using (var session = _store.OpenSession())
            {
                var filtered = session.Query<BookProjection, Users_Books_ByCriteria>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(ub => ub.Shelves.Contains("users/1/shelves/2") || ub.Shelves.Contains("users/1/shelves/3"))
                    .ProjectFromIndexFieldsInto<BookProjection>()
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
                var filtered = session.Query<BookProjection, Users_Books_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")
                    .Where(bs => bs.Rating == 4)
                    .ProjectFromIndexFieldsInto<BookProjection>()
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
                var filtered = session.Query<BookProjection, Users_Books_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")
                    .Where(bs => !bs.Recommended)
                    .ProjectFromIndexFieldsInto<BookProjection>()
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
                var filtered = session.Query<BookProjection, Users_Books_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")
                    .Where(bs => bs.Authors.Contains("Stephen King"))
                    .ProjectFromIndexFieldsInto<BookProjection>()
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
                var filtered = session.Query<BookProjection, Users_Books_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")
                    .Where(bs => bs.Authors.Contains("Stephen King") || bs.Authors.Contains("Damsel Grigsby"))
                    .ProjectFromIndexFieldsInto<BookProjection>()
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
                var filtered = session.Query<BookProjection, Users_Books_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")
                    .Where(bs => bs.Authors.Contains("Stephen King") && bs.Authors.Contains("Sortie Fuller"))
                    .ProjectFromIndexFieldsInto<BookProjection>()
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
                var filtered = session.Query<BookProjection, Users_Books_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")
                    .Where(bs => bs.Tags.Contains("awesome") || bs.Authors.Contains("Sortie Fuller"))
                    .ProjectFromIndexFieldsInto<BookProjection>()
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
                var filtered = session.Query<BookProjection, Users_Books_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(u => u.UserId == "users/1")
                    .Where(bs => bs.Rating == 0 && bs.Authors.Contains("Sortie Fuller"))
                    .ProjectFromIndexFieldsInto<BookProjection>()
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
                var facets = new List<Facet>()
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
                    },
                };

                var facetResults = session.Query<BookProjection, Users_Books_ByCriteria>()
                    .Customize(q => q.WaitForNonStaleResults())
                    .Where(bs => bs.UserId == "users/1")
                    .ToFacets(facets);

                Assert.NotNull(facetResults);
                Assert.AreEqual(4, facetResults.Results["Authors"].Values.Count);
                Assert.AreEqual(4, facetResults.Results["Shelves"].Values.Count);
            }
        }
    }
}
