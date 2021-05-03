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