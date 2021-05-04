let syntaxList = document.getElementById("paste-syntax");
let langs = hljs.listLanguages();
langs.sort();

langs.forEach(name => {
    let lang = hljs.getLanguage(name);

    let optionNode = document.createElement("option")

    optionNode.value = name;
    optionNode.text = lang.name;

    syntaxList.appendChild(optionNode);
});

let exposureList = document.getElementById("paste-exposure");
let encryptKey = document.getElementById("paste-key");
let encryptLabel = document.getElementById("paste-key-label");
exposureList.onchange = function onChange() {
    let value = this.value;
    if (value === "3") { // ENCRYPTED OPTION
        encryptKey.disabled = false;
        encryptKey.required = true;
        encryptKey.classList.add("border-warning");
        encryptKey.classList.remove("text-muted");
        encryptLabel.classList.remove("text-muted");
    } else {
        encryptKey.disabled = true;
        encryptKey.required = false;
        encryptKey.classList.remove("border-warning");
        encryptKey.classList.add("text-muted");
        encryptLabel.classList.add("text-muted")
    }
}


let pasteField = document.getElementById("paste-input");
const vh = Math.max(document.documentElement.clientHeight || 0, window.innerHeight || 0)

pasteField.style.overflowY = "hidden";

function autosize() {
    let lines = pasteField.value.split(/\r\n|\r|\n/).length;
    pasteField.rows = (lines);

    if (pasteField.clientHeight > vh * 0.74) {
        pasteField.style.overflowY = "auto";
    } else {
        pasteField.style.overflowY = "hidden";
    }
}

autosize()

function validate() {
    if (exposureList.value === "3") {
        console.log("encrypting...");
        let key = encryptKey.value;
        let pasteRaw = pasteField.value;
        let enc = encrypt(pasteRaw, key);
        pasteField.value = enc;
        encryptKey.value = ""; // for safety reasons
        document.forms[0].submit();
    } else {
        document.forms[0].submit();
    }
}

if (document.location.protocol !== "https:") { // SECURE CONTEXT
    let option = document.getElementById("encrypt-option");
    option.disabled = true;
    option.textContent += "?";
    option.title = "'Encrypted' option only works in secure contexts!";
    option.style.cursor = "help";
}

