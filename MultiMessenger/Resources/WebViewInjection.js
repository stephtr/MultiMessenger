window.Notification = function Notification(title, options) {
    if (options.icon) {
        options.icon = new URL(options.icon, window.location.href).href;
    }
    var id = Math.random() + '';
    window.chrome.webview.postMessage({ action: 'notification.new', id, title, ...options });
    this.close = () => {
        window.chrome.webview.postMessage({ action: 'notification.close', id });
    };
    this.addEventListener = (eventName, callback) => {
        if (eventName == 'show') {
            callback();
        }
    };
};

window.Notification.permission = 'granted';
window.Notification.requestPermission = (cb) => cb('granted');

if (location.href.startsWith('https://web.whatsapp.com/')) {
    function updateUnread() {
        // first, get all conversations with unread messages
        var count = [].filter.call(document.querySelectorAll("#pane-side ._38M1B"),
            // filter out muted conversations
            (e) => !e.parentNode.parentNode.querySelectorAll('[data-icon=\"muted\"]').length
        ).length;
        window.chrome.webview.postMessage({ action: 'unreadMessages.update', count });
    }
    setInterval(updateUnread, 2500);
}
