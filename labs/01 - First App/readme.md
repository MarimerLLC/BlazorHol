# Create Your First Blazor App

## Create the solution

1. Open Visual Studio
2. Click on Create a new project
3. Select Blazor Web App
4. Click Next
5. Enter the project name: `BlazorHolApp`
6. Click Next
7. Use the following options:
   - Framework: .NET 8.0
   - Authentication Type: None
   - Configure for HTTPS: Checked
   - Interactive render mode: Server
   - Interactivity location: Global
   - Include sample pages: Checked
8. Click Create

## Run the application

1. Press F5 to run the application
2. The application will open in the browser
3. You will see the default Blazor application
4. Navigate to the `Counter` page
5. Click on the `Click me` button
6. You will see the counter incrementing
   - Notice how the page does not reload when the counter increments. This is because of data binding. The `currentCount` field is _bound_ to the output with `@currentCount`.
   - Also notice how the button click event is handled by the `IncrementCount` method. This is an example of event binding. The `@onclick` directive is used to bind the click event to the `IncrementCount` method.

7. Navigate to the `Fetch Data` page
8. You will see a table with some data
   - Notice how the page loads, then the data appears
   - This is because the data is loaded asynchronously
   - There is a `Task.Delay` to simulate a delay in loading the data

## Modify the application

1. Open the file `Components/Pages/Home.razor`
2. Modify the content of the file to the following:

```html
@page "/"

<PageTitle>HOL Home</PageTitle>

<h1>Hands On Lab App</h1>

Welcome to the Blazor Hands On Lab App.
```

3. Save the file
4. Press F5 to run the application
5. Notice the new content in the home page

## Add a new page

1. Right-click on the `Components/Pages` folder
2. Click on Add > Razor Component
3. Enter the name `About.razor`
4. Click Add
5. Modify the content of the file to the following:

```html
@page "/about"

<h1>About</h1>

<p>This is your application's about page.</p>
```

6. Save the file
7. Open the file `Components/Layout/NavMenu.razor` file
8. Add a new `NavLink` component to the `nav` element:

```html
<div class="nav-item px-3">
    <NavLink class="nav-link" href="about">
        <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> About
    </NavLink>
</div>
```

9. Save the file
10. Press F5 to run the application
11. Click on the `About` link in the navigation menu
12. You will see the new page

## Adding a second route

1. Open the file "About.razor"
2. Add the another `@page` directive at the top of the file:

```html
@page "/about"
@page "/aboutus"

<h1>About</h1>

<p>This is your application's about page.</p>
```

3. Save the file
4. Edit the `NavMenu.razor` file
5. Add a new `NavLink` component to the `ul` element:

```html
<div class="nav-item px-3">
    <NavLink class="nav-link" href="aboutus">
        <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> About Us
    </NavLink>
</div>
```

6. Save the file
7. Press F5 to run the application
8. Click on the `About Us` link in the navigation menu
9. You will see the same page as the `About` link - both routes point to the same page

## Using two-way data binding

1. Open the file `Components/Pages/Counter.razor`
2. Add a new input element to the page:

```html
<input type="text" @bind="currentCount" />
```

3. Save the file
4. Press F5 to run the application
5. Navigate to the `Counter` page
6. You will see a text box
7. Type a number in the text box
8. Notice how the counter increments when you tab out of the text box
   - This is because the `currentCount` field is bound to the input element with `@bind="currentCount"`
9. Click on the `Click me` button
10. You will see the counter incrementing in the text box and the output as they are both bound to the same field
