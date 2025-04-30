export function alertUser() {
    alert('The button was selected!');
}

export function addHandlers() {
    const btn = document.getElementById("alertBtn");
    btn.addEventListener("click", alertUser);
}
