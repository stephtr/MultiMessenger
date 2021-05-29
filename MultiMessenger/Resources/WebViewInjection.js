window.Notification = function Notification(title, options) {
    if (options.icon) {
        options.icon = new URL(options.icon, window.location.href).href;
    }
    window.chrome.webview.postMessage({ title, ...options });
};

window.Notification.permission = 'granted';
window.Notification.requestPermission = (cb) => cb('granted');