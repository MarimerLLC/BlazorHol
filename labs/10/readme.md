# Styling Blazor Apps

## Creating the Solution

1. Open Visual Studio
2. Click on Create a new project
3. Select Blazor Web App
4. Click Next
5. Enter the project name: `BlazorHolState`
6. Click Next
7. Use the following options:
   - Framework: .NET 8.0
   - Authentication Type: None
   - Configure for HTTPS: Checked
   - Interactive render mode: Auto (Server and WebAssembly)
   - Interactivity location: Per page/component
   - Include sample pages: Checked
8. Click Create

## Setting Global CSS

1. Open the `wwwroot` folder in the server project
2. Open the `app.css` file
3. Edit the `.btn-primary` class to have the following styles:

```css
.btn-primary {
    color: #929292;
    background-color: #26b050;
    border-color: #1861ac;
}

.btn-primary:hover {
    color: #808080;
    background-color: #00ff21;
    border-color: #285e8e;
}

.btn-primary:focus {
    color: #929292;
    background-color: #26b050;
    border-color: #000000;
}
```

4. Press F5 to run the app
5. Navigate to the Counter page
6. The button should now have the styles defined in the `app.css` file
7. Close the browser

## Setting Per-Component CSS

1. Open the `Counter.razor` file in the `Pages` folder
2. Right-click on the `Pages` folder in Solution Explorer and select Add -> New Item
3. Add a file named `Counter.razor.css`
4. Add the following CSS to the `Counter.razor.css` file:

```css
h1 {
    color: brown;
    font-family: Tahoma, Geneva, Verdana, sans-serif;
}

button {
    background-color: brown;
    color: white;
    font-family: Tahoma, Geneva, Verdana, sans-serif;
}
```

5. Press F5 to run the app
6. Navigate to the Counter page
7. The `h1` and `button` elements should now have the styles defined in the `Counter.razor.css` file
8. Close the browser
9. Open the `App.razor` file in the server project `Components` folder
10. Notice the consolidated css link:

```html
    <link rel="stylesheet" href="BlazorHolStyling.styles.css" />
```

This is the consolidated CSS file that contains all the per-component CSS files in the project.

