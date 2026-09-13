# Testing with Bunit

## Install Bunit Templates

1. Open a terminal
2. Run the following command:

```bash
dotnet new install bunit.template
```

## Creating the Solution

1. Open Visual Studio
2. Click on Create a new project
3. Select Blazor Web App
4. Click Next
5. Enter the project name: `BlazorHolTestApp`
6. Click Next
7. Use the following options:
   - Framework: .NET 10.0
   - Authentication Type: None
   - Configure for HTTPS: Checked
   - Interactive render mode: Server
   - Interactivity location: Global
   - Include sample pages: Checked
8. Click Create

## Add a Hello World Component

1. In the `Components/Pages` folder, add a new component named `HelloWorld.razor`:

```razor
<h1>Hello world from Blazor</h1>
```

## Create a new Bunit test project

1. In Visual Studio click on File -> Add -> New Project
2. Select the bUnit Test Project template
3. Name the project `BlazorTests`
4. Select mstest as the test framework
5. Target .NET 10.0
6. Select the `BlazorTests` project
7. In the `BlazorTests` project, add a Project Reference to the `BlazorHolTestApp` project
8. In the `BlazorTests` project, add a using statement to the `_Imports.razor` file:

```razor
@using BlazorHolTestApp.Components.Pages
```

9. In the `BlazorTests` project, delete the `Counter.razor` file, as we will be using the component from the `BlazorHolTestApp` project

> **Note:** This lab uses bUnit 2. If you find older bUnit examples online, note that bUnit 2 renamed `TestContext` to `BunitContext` and `RenderComponent<T>()` to `Render<T>()`. Test classes now inherit directly from `BunitContext`.

## Create a C# test for the HelloWorld component

1. In the `BlazorTests` project, add a new file named `HelloWorldCsharpTests.cs`
2. Add the following code:

```csharp
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
```

## Create a Razor test for the HelloWorld component

1. In the `BlazorTests` project, add a new file named `HelloWorldRazorTest.razor`
2. Add the following code:

```razor
@attribute [TestClass]

@inherits BunitContext

@code
{
    [TestMethod]
    public void HelloWorldComponentRendersCorrectly()
    {
        // Act
        var cut = Render(@<HelloWorld />);

        // Assert
        cut.MarkupMatches(@<h1>Hello world from Blazor</h1>);
    }
}
```

## Fix the Counter Tests

1. In the `BlazorTests` project, open the `CounterCSharpTest.cs` file
2. Add the following using statement at the top of the file:

```csharp
using BlazorHolTestApp.Components.Pages;
```

3. Replace the two `MarkupMatches` lines with the following code:

```csharp
    cut.Find("p").MarkupMatches("<p role=\"status\">Current count: 0</p>");
```

and

```csharp
    cut.Find("p").MarkupMatches("<p role=\"status\">Current count: 1</p>");
```

4. Open the `CounterRazorTests.razor` file
5. Replace the two `MarkupMatches` lines with the following code:

```razor
    cut.Find("p").MarkupMatches(@<p role="status">Current count: 0</p>);
```

and

```razor
    cut.Find("p").MarkupMatches(@<p role="status">Current count: 1</p>);
```

The `role="status"` attribute is added to the `p` elements to make the test pass. Microsoft has changed the Blazor template more recently than the bUnit templates have changed.

## Run the tests

1. In Visual Studio, right-click on the `BlazorTests` project
2. Click on Run Tests
