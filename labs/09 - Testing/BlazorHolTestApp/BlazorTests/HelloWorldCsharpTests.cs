using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunit;
using BlazorHolTestApp.Components.Pages;

namespace BlazorTests;

[TestClass]
public class HelloWorldCsharpTests : BunitContext
{
    [TestMethod]
    public void HelloWorldComponentRendersCorrectly()
    {
        // Act
        var cut = Render<HelloWorld>();

        // Assert
        cut.MarkupMatches("<h1>Hello world from Blazor</h1>");
    }
}
