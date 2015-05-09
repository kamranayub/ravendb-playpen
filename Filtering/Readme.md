# Filtering

Finding approaches to solving a complex filtering issue.

## Domain

Domain model is based on book collecting app GoodReads with some extra business rules. One of the biggest is that a book can exist outside a Shelf.

- Book - A book, existing somewhere in space
- User - A user collects books
- Shelf - A user can put a book on one or more shelves, such as "Want to Read"
- BookOnShelf - A book can have specific information about itself on a shelf, such as the order its in, the date it was added, and a note for why it was added to the shelf
- UserBook - A book a User adds to their library. It can be added to a Shelf but it doesn't have to be. A user's book can have tags and other user-specific information.

## Goals

The approach should allow combining multiple filters, including AND/OR, on the different fields of both a UserBook and a Book. The UI will retrieve all available facets, present the interface to filter, and then keep it updated each request. Since the user could take an action that could result in a stale index (e.g. adding a new tag to a UserBook), retrieving the Facets could be stale--however, this can be mitigated by manipulating the available facets client-side after such requests (i.e. the API returns successful result containing the new available FacetValue).

## Filtering context (All vs. Shelf)

### All My Books context

When viewing "All My Books" you want to know what shelves a book is on and you may want to filter on it. Therefore, "Shelves" needs to be a facet you can filter on. One or more shelves can be filtered on. You do not necessarily need to see BookOnShelf information though.

There should **not** be a [potentially stale] index query to view all books for a user. Filtering can be stale.

### Shelf context

When viewing a specific Shelf, you will want to know the specific information stored in BookOnShelf as well as the UserBook information. You will also want to know what other shelves the book is in, to display it to the user.

There should **not** be a [potentially stale] index query to view all books on a shelf. Filtering can be stale.

## Two Approaches

### Approach 1: Store UserBook, Shelf, and BookOnShelf in the User document

**Pros**

- Easy to reason about
- Easy to query
- Could do all client-side filtering (the tests do not, though)
- Uses Transforms to project server-side

**Cons**

- Bloated criteria index--stores all relevant information for projection
- Does not scale well since everything is in a single document

### Approach 2: Store UserBook, Shelf, and BookOnShelf in separate collections

**Pros**

- Separation into Collections, easy to scale/optimize/shard
- Easy to query
- Does not store fields in criteria index
- Uses Transforms to project server-side
- Cleaner index
- Considered a Best Practice

**Cons**

- A bit harder to conceptualize
- Stores a back reference to Shelves on UserBook for filtering/facets
  - If you didn't do this, need a way to filter by multiple shelves
    and also do other filtering in a single request? Shelves also won't show
    up in FacetResults.
- More manual ID upkeep

## Play with it

If you have a better approach or optimizations, make them and see if you can keep the tests green and everything within a single request to Raven.