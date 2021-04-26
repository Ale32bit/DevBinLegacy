let pasteField = document.getElementById("paste-input");

pasteField.style.overflowY = "hidden";

function autosize() {
    let lines = pasteField.value.split(/\r\n|\r|\n/).length;
    pasteField.rows = (lines);
}

autosize()