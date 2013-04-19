using System;
using System.Collections.Generic;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;
using System.Linq;

namespace Tests
{
    [TestFixture]
    public class PlayingAround
    {
        private readonly IDocumentStore _documentStore;

        public PlayingAround()
        {
            _documentStore = new DocumentStore
                {
                    Url = "http://localhost:8080",
                    DefaultDatabase = "PlayingAround"
                };
            _documentStore.Initialize();
        }


        [Test]
        public void CreateDocument()
        {
            MyDocument document;
            using (var session = _documentStore.OpenSession())
            {
                document = new MyDocument
                    {
                        Content = "Some Content",
                        Integer = 1,
                        Double = 1.0,
                        Decimal = 2m,
                        Date = DateTime.UtcNow,
                        Array = new[] {"Item 1", "Item 2"},
                        ChildObject = new ChildObject {Hello = "World!"},
                        AnonymousObject = new {Thing = "Stuff"}
                    };
                session.Store(document);
                session.SaveChanges();
            }
            Assert.IsNotEmpty(document.Id);
        }

        [Test]
        public void ModifyDocument()
        {
            const string modifiedContent = "Modified content";
            var newDocument = new MyDocument
                {
                    Content = "New Content"
                };
            using (var session = _documentStore.OpenSession())
            {
                session.Store(newDocument);
                session.SaveChanges();
            }

            using (var anotherSession = _documentStore.OpenSession())
            {
                var existingDocument = anotherSession.Load<MyDocument>(newDocument.Id);
                existingDocument.Content = modifiedContent;
                anotherSession.SaveChanges();
            }

            MyDocument modifiedDocument;
            using (var yetAnotherSession = _documentStore.OpenSession())
            {
                modifiedDocument = yetAnotherSession.Load<MyDocument>(newDocument.Id);
            }

            Assert.AreEqual(modifiedDocument.Content, modifiedContent);
            Assert.AreNotEqual(modifiedDocument.Content, newDocument.Content);
            Assert.AreNotSame(modifiedDocument, newDocument);
        }

        [Test]
        public void DeleteDocument()
        {
            var newDocument = new MyDocument
                {
                    Content = "New Content"
                };
            using (var session = _documentStore.OpenSession())
            {
                session.Store(newDocument);
                session.SaveChanges();
            }

            using (var anotherSession = _documentStore.OpenSession())
            {
                var existingDocument = anotherSession.Load<MyDocument>(newDocument.Id);
                anotherSession.Delete(existingDocument);
                anotherSession.SaveChanges();
            }

            MyDocument deletedDocument;
            using (var yetAnotherSession = _documentStore.OpenSession())
            {
                deletedDocument = yetAnotherSession.Load<MyDocument>(newDocument.Id);
            }

            Assert.IsNull(deletedDocument);
        }

        [Test]
        public void QueryDocuments()
        {
            const string findableContent = "Finders keepers!";
            const string otherContent = "Can't see me!";
            var count = 0;
            var newDocuments = new[]
                {
                    new MyDocument
                        {
                            Content = findableContent,
                            Boolean = true,
                            Integer = ++count,
                            ChildObject = new ChildObject {Hello = "Query"}
                        },
                    new MyDocument
                        {
                            Content = findableContent,
                            Boolean = true,
                            Integer = ++count,
                            ChildObject = new ChildObject {Hello = "Query"}
                        },
                    new MyDocument
                        {
                            Content = findableContent,
                            Boolean = true,
                            Integer = ++count,
                            ChildObject = new ChildObject {Hello = "Query"}
                        },
                    new MyDocument
                        {
                            Content = findableContent,
                            Boolean = true,
                            Integer = ++count,
                            ChildObject = new ChildObject {Hello = "Query"}
                        },
                    new MyDocument
                        {
                            Content = findableContent,
                            Boolean = true,
                            Integer = ++count,
                            ChildObject = new ChildObject {Hello = "Query"}
                        },
                    new MyDocument
                        {
                            Content = otherContent,
                            Boolean = false,
                            Integer = ++count,
                            ChildObject = new ChildObject {Hello = "Query"}
                        },
                    new MyDocument
                        {
                            Content = otherContent,
                            Boolean = false,
                            Integer = ++count,
                            ChildObject = new ChildObject {Hello = "Query"}
                        },
                    new MyDocument
                        {
                            Content = otherContent,
                            Boolean = false,
                            Integer = ++count,
                            ChildObject = new ChildObject {Hello = "Query"}
                        },
                    new MyDocument
                        {
                            Content = otherContent,
                            Boolean = false,
                            Integer = ++count,
                            ChildObject = new ChildObject {Hello = "Query"}
                        },
                    new MyDocument
                        {
                            Content = otherContent,
                            Boolean = false,
                            Integer = ++count,
                            ChildObject = new ChildObject {Hello = "Query"}
                        }
                };

            using (var session = _documentStore.OpenSession())
            {
                foreach (var newDocument in newDocuments)
                    session.Store(newDocument);
                session.SaveChanges();
            }

            List<MyDocument> foundDocuments;
            using (var anotherSession = _documentStore.OpenSession())
            {
                foundDocuments =
                    anotherSession.Query<MyDocument>()
                    .Where(doc => doc.Content == findableContent).ToList();
            }
            Assert.AreEqual(5, foundDocuments.Count);

            using (var yetAnotherSession = _documentStore.OpenSession())
            {
                foundDocuments =
                    yetAnotherSession.Query<MyDocument>()
                    .Where(doc => doc.ChildObject.Hello == "Query").ToList();
            }
            Assert.AreEqual(10, foundDocuments.Count);

            using (var cleanUpSession = _documentStore.OpenSession())
            {
                foreach (var newDocument in newDocuments)
                {
                    var deleteMe = cleanUpSession.Load<MyDocument>(newDocument.Id);
                    cleanUpSession.Delete(deleteMe);
                }
                cleanUpSession.SaveChanges();
            }
        }
    }
}