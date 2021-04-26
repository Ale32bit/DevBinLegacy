let minLines = 10;
let observe;
if (window.attachEvent) {
    observe = function (element, event, handler) {
        element.attachEvent('on' + event, handler);
    };
}
else {
    observe = function (element, event, handler) {
        element.addEventListener(event, handler, false);
    };
}
let text = document.getElementById('paste-input');
function init() {
    function resize() {
        let lines = text.value.split(/\r*\n/).length;
        text.style.height = 'auto';
        text.style.height = lines > minLines ? text.scrollHeight + 'px' : 1 + minLines + "rem";
    }
    /* 0-timeout to get the already changed text */
    function delayedResize() {
        window.setTimeout(resize, 0);
    }
    observe(text, 'change', resize);
    observe(text, 'cut', delayedResize);
    observe(text, 'paste', delayedResize);
    observe(text, 'drop', delayedResize);
    observe(text, 'keydown', delayedResize);

    text.focus();
    text.select();
    resize();
}
text.style.height = minLines + "rem";
init();