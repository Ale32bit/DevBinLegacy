let syntaxList = document.getElementById("paste-syntax");

let langs = hljs.listLanguages();

langs.sort();

langs.forEach(name => {
    let lang = hljs.getLanguage(name);

    let optionNode = document.createElement("option")

    optionNode.value = name;
    optionNode.text = lang.name;

    if (name == "plaintext") {
        optionNode.selected = true;
    }

    syntaxList.appendChild(optionNode);
});

function highlight(id) {
    addEventListener('load', () => {
        const code = document.getElementById(id);
        const worker = new Worker('~/js/hl-worker.js');
        worker.onmessage = (event) => { code.innerHTML = event.data; }
        worker.postMessage(code.textContent);
    });
}