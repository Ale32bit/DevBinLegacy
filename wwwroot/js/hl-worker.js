onmessage = (event) => {
    importScripts('~/js/highlight.pack.js');
    const result = self.hljs.highlightAuto(event.data);
    postMessage(result.value);
}