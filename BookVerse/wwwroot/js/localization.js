(function () {
    const savedCulture = document.cookie.split('; ').find(x => x.startsWith('bookverse.culture='))?.split('=')[1];
    const isAuthenticationPage = /^\/Account\/(Login|Register)/i.test(location.pathname);
    const culture = savedCulture || (isAuthenticationPage ? 'en' : 'vi');
    document.documentElement.lang = culture;
    const existingSwitcher = document.querySelector('form[action*="/Localization/SetLanguage"]');
    if (existingSwitcher) existingSwitcher.id = 'bookverse-language-switcher';
    if (!document.getElementById('bookverse-language-switcher')) {
        const switcher = document.createElement('div');
        switcher.id = 'bookverse-language-switcher';
        switcher.style.cssText = 'display:flex;align-items:center;gap:2px;background:var(--card,#fff);color:var(--card-foreground,#222);border:1px solid var(--border,#ddd);border-radius:999px;padding:3px;white-space:nowrap';
        switcher.innerHTML = '<button type="button" data-lang="vi" aria-label="Tiếng Việt" style="border:0;border-radius:999px;background:transparent;color:inherit;padding:5px 9px;cursor:pointer">VI</button><button type="button" data-lang="en" aria-label="English" style="border:0;border-radius:999px;background:transparent;color:inherit;padding:5px 9px;cursor:pointer">EN</button>';
        switcher.addEventListener('click', event => {
            const lang = event.target.dataset.lang;
            if (!lang) return;
            document.cookie = 'bookverse.culture=' + lang + ';path=/;max-age=31536000;samesite=lax';
            location.reload();
        });
        const registerButton = document.querySelector('header a[href="/Account/Register"]');
        const headerTarget = document.getElementById('bookverse-language-switcher-slot')
            || registerButton?.parentElement
            || document.querySelector('header .header-actions, header .topbar-actions, header nav, header');
        if (headerTarget) headerTarget.appendChild(switcher);
        else document.body.insertBefore(switcher, document.body.firstChild);
    }
    document.querySelectorAll('#bookverse-language-switcher button').forEach(button => {
        const language = button.dataset.lang || button.value;
        const active = language === culture;
        button.setAttribute('aria-pressed', active ? 'true' : 'false');
        if (active) {
            button.style.background = '#102579';
            button.style.color = '#fff';
        }
    });
    if (culture !== 'en') return;

    fetch('/Localization/ClientDictionary?culture=' + encodeURIComponent(culture))
        .then(response => response.ok ? response.json() : {})
        .then(dictionary => {
            const translate = value => {
                const trimmed = value.trim();
                if (!trimmed) return value;
                return dictionary[trimmed] ? value.replace(trimmed, dictionary[trimmed]) : value;
            };
            const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
            const nodes = [];
            while (walker.nextNode()) nodes.push(walker.currentNode);
            nodes.forEach(node => {
                if (!node.parentElement?.closest('script,style,code,pre')) node.nodeValue = translate(node.nodeValue || '');
            });
            document.querySelectorAll('[placeholder],[title],[aria-label]').forEach(element => {
                ['placeholder', 'title', 'aria-label'].forEach(attribute => {
                    const value = element.getAttribute(attribute);
                    if (value) element.setAttribute(attribute, dictionary[value.trim()] || value);
                });
            });
        });
})();
