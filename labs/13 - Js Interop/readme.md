# JavaScript Interop

## Creating the Solution

1. Open Visual Studio
2. Click on Create a new project
3. Select Blazor Web App
4. Click Next
5. Enter the project name: `BlazorHolJsInterop`
6. Click Next
7. Use the following options:
   - Framework: .NET 8.0
   - Authentication Type: None
   - Configure for HTTPS: Checked
   - Interactive render mode: Server
   - Interactivity location: Global
   - Include sample pages: Checked
8. Click Create

## Adding a JavaScript Library

1. Right-click on the `wwwroot` folder
2. Click on Add > New Item
3. Search for `JavaScript` and select `JavaScript File`
4. Enter the name `chart.js`
5. Click Add
6. Add the following content to the file:

```javascript
function createChart(canvasId) {
    var ctx = document.getElementById(canvasId).getContext('2d');
    var chart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['Red', 'Blue', 'Yellow', 'Green', 'Purple', 'Orange'],
            datasets: [{
                label: '# of Votes',
                data: [12, 19, 3, 5, 2, 3],
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(255, 206, 86, 0.2)',
                    'rgba(75, 192, 192, 0.2)',
                    'rgba(153, 102, 255, 0.2)',
                    'rgba(255, 159, 64, 0.2)'
                ],
                borderColor: [
                    'rgba(255, 99, 132, 1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(255, 206, 86, 1)',
                    'rgba(75, 192, 192, 1)',
                    'rgba(153, 102, 255, 1)',
                    'rgba(255, 159, 64, 1)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}
```

This code creates a function called `createChart` that takes a canvas ID as a parameter and creates a new Chart.js chart on that canvas.

## Using the JavaScript Library

1. Open the `App.razor` file in the `Components` folder
2. Add the following code to the file at the bottom of the `body` tag:

```html
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
<script src="chart.js"></script>
```

This code will load the Chart.js library and the custom JavaScript file we created earlier so they are available in the application.

3. Open the `Home.razor` file in the `Components/Pages` folder
4. Add the following code to the file to inject the `IJSRuntime` service:

```html
@inject IJSRuntime JSRuntime
```

This code injects the `IJSRuntime` service into the component so we can call JavaScript functions from C# code.

5. Add the following code to the file to create a canvas element for the chart:

```html
<canvas id="myChart" width="400" height="400"></canvas>
```

6. Add the following code to the file to create a chart when the component is initialized:

```html
@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("createChart", "myChart");
        }
    }
}
```

7. Run the application
8. You should see a bar chart displayed on the home page

## References

- [Chart.js Documentation](https://www.chartjs.org/docs/latest/)
- [JavaScript Interop in Blazor](https://docs.microsoft.com/en-us/aspnet/core/blazor/call-javascript-from-dotnet?view=aspnetcore-6.0)
- [Using Chart.js with Blazor](https://puresourcecode.com/dotnet/blazor/using-chart-js-with-blazor/#:~:text=First%20step%20is%20to%20add%20the%20library%20in,the%20application%20reads%20the%20script%20from%20the%20CDN)
