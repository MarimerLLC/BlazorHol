using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bunit;
using BlazorTests;
using BlazorHolTestApp.Components.Pages;

namespace BlazorTests;

[TestClass]
public class HelloWorldTests : BunitTestContext
{
    [TestMethod]
    public void HelloWorldComponentRendersCorrectly()
    {
        // Act
        var cut = RenderComponent<HelloWorld>();

        // Assert
        cut.MarkupMatches("<h1>Hello world from Blazor</h1>");
    }
}
