# Creating Razor Components

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

## Raising Events from Razor Components

1. Open the `Counter.razor` file.
2. Add an event to the `Counter` component:

```html
@page "/counter"

<PageTitle>Counter</PageTitle>

<h1>Counter</h1>

<input type="text" @bind="CurrentCount" />

<p role="status">Current count: @CurrentCount</p>

<button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

@code {
    [Parameter]
    public EventCallback<int> CurrentCountChanged { get; set; }

    [Parameter]
    public int StartCount { get; set; }

    public int CurrentCount { get; set;}

    protected override void OnInitialized()
    {
        CurrentCount = StartCount;
    }

    private async Task IncrementCount()
    {
        CurrentCount++;
        await CurrentCountChanged.InvokeAsync(CurrentCount);
    }
}
```

Because handling the event in the parent page will cause that page to re-render, it is necessary to change the `CurrentCount` property to _not_ be directly set by the parent page. Instead, the parent page will set the `StartCount` property, and the `Counter` component will set the `CurrentCount` property in the `OnInitialized` method.

The `IncrementCount` method now raises the `CurrentCountChanged` event, which the parent page can handle.

3. Open the `Home.razor` file.
4. Add a `CurrentCount` field to the `Home` component:

```html
@page "/"

<PageTitle>HOL Home</PageTitle>

<h1>Hands On Lab App</h1>

Welcome to the Blazor Hands On Lab App.

<p>Count: @CurrentCount</p>

<Counter StartCount="42" />

@code {
    private int CurrentCount = 0;
}
```

Notice that `Counter` now has a `StartCount` parameter instead of the `CurrentCount` parameter.

5. Handle the `CurrentCountChanged` event in the `Home` component:

```html
@page "/"

<PageTitle>HOL Home</PageTitle>

<h1>Hands On Lab App</h1>

Welcome to the Blazor Hands On Lab App.

<p>Count: @CurrentCount</p>

<Counter StartCount="42" CurrentCountChanged="CountChanged"/>

@code {
    private int CurrentCount = 0;

    private void CountChanged(int count)
    {
        this.CurrentCount = count;
    }
}
```

6. Press F5 to run the app.
7. Notice how the `CurrentCounter` value changes as you change the value in the `Counter` component.

## Cascading Parameters

1. Open the `Counter.razor` file.
2. Change the parameter to a cascading parameter:

```csharp
@code {
    [CascadingParameter]
    public int StartCount { get; set; }
}
```

3. Open the `Home.razor` file.
4. Add a `CascadingValue` element to the `Home` component:

```html
@page "/"

<PageTitle>HOL Home</PageTitle>

<h1>Hands On Lab App</h1>

Welcome to the Blazor Hands On Lab App.

<CascadingValue Value="123">
    <div class="border border-4">
        <Counter/>
    </div>
    <div class="border border-4">
        <Counter/>
    </div>
</CascadingValue>
```

5. Press F5 to run the app.
6. Notice how the `Counter` components are displayed on the page, and the `CurrentCount` property is set to `123`.
7. Notice how changing the value in one `Counter` component has no impact on the other `Counter` component.
