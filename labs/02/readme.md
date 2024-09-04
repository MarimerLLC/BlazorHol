# Creating Razor Compoonents

## Composing Razor Components

1. Open the `BlazorHolApp` solution in Visual Studio.
1. Open the `Home.razor` file.
1. Add the `Weather` element to the `Home.razor` file:

```html
@page "/"

<PageTitle>HOL Home</PageTitle>

<h1>Hands On Lab App</h1>

Welcome to the Blazor Hands On Lab App.

<Weather />
```

Notice how it is colored differently. It is a Razor Component.

4. Press F5 to run the app.
5. Notice how the `Weather` component is displayed on the page, and you can still also go to the Weather page.

## Passing Parameters to Razor Components

1. Open the `Counter.razor` file.
1. Add a parameter to the `Counter` component:

```html
@page "/counter"

<PageTitle>Counter</PageTitle>

<h1>Counter</h1>

<input type="text" @bind="CurrentCount" />

<p role="status">Current count: @CurrentCount</p>

<button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

@code {
    [Parameter]
    public int CurrentCount { get; set;}

    private void IncrementCount()
    {
        CurrentCount++;
    }
}
```

Notice how the `currentCount` field has been replaced with a `CurrentCount` property that is decorated with the `[Parameter]` attribute.

3. Open the `Home.razor` file.
4. Remove the `Weather` component from the `Home` component.
5. Add the `Counter` component to the `Home` component:

```html
@page "/"

<PageTitle>HOL Home</PageTitle>

<h1>Hands On Lab App</h1>

Welcome to the Blazor Hands On Lab App.

<Counter CurrentCount="42" />
```

6. Press F5 to run the app.
7. Notice how the `Counter` component is displayed on the page, and the `CurrentCount` property is set to `42`.
