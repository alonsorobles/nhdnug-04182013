using System;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Embedded;

namespace Tests
{
    [TestFixture]
    public class SampleFixtureWithEmbeddableDocumentStore
    {
        private readonly IDocumentStore _embeddableDocumentStore;
       
        public SampleFixtureWithEmbeddableDocumentStore()
        {
            _embeddableDocumentStore = new EmbeddableDocumentStore().Initialize();
        }

        [Test]
        public void SampleTest()
        {
            MyDocument document;
            using (var session = _embeddableDocumentStore.OpenSession())
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
            }
            Assert.IsNotEmpty(document.Id);
        }
    }

    public class ChildObject
    {
        public string Hello { get; set; }
    }

    public class MyDocument
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public int Integer { get; set; }

        public double Double { get; set; }

        public decimal Decimal { get; set; }

        public DateTime Date { get; set; }

        public string[] Array { get; set; }

        public ChildObject ChildObject { get; set; }

        public object AnonymousObject { get; set; }

        public object Boolean { get; set; }
    }
}
