var notifications = {};

window.Notification = function Notification(title, options) {
    if (options.icon) {
        options.icon = new URL(options.icon, window.location.href).href;
    }
    var id = Math.random() + '';
    window.chrome.webview.postMessage({ action: 'notification.new', id, title, ...options });
    this.close = () => {
        window.chrome.webview.postMessage({ action: 'notification.close', id });
    };
    var clickHandlers = [];
    this.addEventListener = (eventName, callback) => {
        switch (eventName) {
            case 'show': callback(); break;
            case 'click': clickHandlers.push(callback); break;
        }
    };
    this.triggerClickEvent = () => {
        debugger;
        var evt = new CustomEvent('click');
        evt.notification = this;
        clickHandlers.forEach((cb) => cb(evt));
    };
    notifications[id] = this;
};

window.Notification.permission = 'granted';
window.Notification.requestPermission = (cb) => cb('granted');

window.chrome.webview.addEventListener('message', (ev) => {
    switch (ev.data.action) {
        case 'notification.activated':
            {
                var notification = notifications[ev.data.id];
                if (!notification) return;
                notification.triggerClickEvent();
            }
            break;
    }
});

var getUnreadConversationCount = null;

if (location.href.startsWith('https://web.whatsapp.com/')) {
    getUnreadConversationCount = () => {
        var unreadMessageNodes = document.querySelectorAll("#pane-side ._38M1B");
        // filter out muted conversations
        return [].filter.call(unreadMessageNodes,
            (el) => !el.parentNode.parentNode.querySelectorAll('[data-icon=\"muted\"]').length
        ).length;
    };
}

if (location.href.startsWith('https://www.messenger.com/')) {
    getUnreadConversationCount = () => {
        var conversations = document.querySelectorAll("[role='navigation'] [role='row']");
        return [].map.call(conversations, (el) => (+getComputedStyle(el.querySelector('span:first-child')).fontWeight >= 600 ? 1 : 0))
            .reduce((sum, val) => sum + val, 0);
    };
}

if (location.href.startsWith('https://www.instagram.com/')) {
    getUnreadConversationCount = () => document.querySelectorAll("[style='height: 8px; width: 8px;']").length;
}

if (getUnreadConversationCount != null) {
    setInterval(() => {
        window.chrome.webview.postMessage({ action: 'unreadMessages.update', count: getUnreadConversationCount() });
    }, 2500);
}
