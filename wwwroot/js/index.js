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