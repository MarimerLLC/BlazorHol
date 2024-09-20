export function returnDataAsync() {
    DotNet.invokeMethodAsync('BlazorJsInterop.Client', 'GetStaticResult')
        .then(data => {
            console.log(data);
        });
}

export function addHandlers() {
    const btn = document.getElementById("callbackBtn");
    btn.addEventListener("click", returnDataAsync);
}
