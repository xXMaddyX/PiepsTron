const Mybutton = /**@type {HTMLButtonElement} */ (document.querySelector("#test-button"));

Mybutton.addEventListener("click", () => {
    console.log("blub")
    window.chrome.webview.postMessage({Name: "testsignal", Data: "Das sind die test daten zur sendung"})
})