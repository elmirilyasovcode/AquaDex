    function renderHeader(activePage) {
    const nav = [
        { href: 'index.html', label: 'Home' },
        { href: 'codex.html', label: 'Codex' },
        { href: 'catches.html', label: 'Catch Log' },
        { href: 'waterbodies.html', label: 'Waterbodies' },
        { href: 'forum.html', label: 'Forum' },
        { href: 'report.html', label: 'Report' },
        { href: 'profile.html', label: 'Profile' }
    ];

    const navHtml = nav.map(item =>
        `<a href="${item.href}" class="nav-link ${activePage === item.href ? 'active' : ''}">${item.label}</a>`
    ).join('');

    document.getElementById('site-header').innerHTML = `
    <div class="header-inner">
      <a href="index.html" class="logo logo-mark"><span class="logo-dot"></span>AquaDex</a>
      <nav class="nav-links">${navHtml}</nav>
      <div id="auth-status" class="auth-status"></div>
    </div>
  `;

    // Scroll-aware blur/shadow — now applies on every page, not just index.html
    window.addEventListener('scroll', () => {
        const header = document.getElementById('site-header');
        if (window.scrollY > 12) header.classList.add('scrolled');
        else header.classList.remove('scrolled');
    });

    (async () => {
        const authEl = document.getElementById('auth-status');
        try {
            const user = await apiRequest('/auth/me');
            authEl.innerHTML = `<span class="user-tag">${user.displayName}</span> <a href="#" id="logout-link">Logout</a>`;
            document.getElementById('logout-link').addEventListener('click', async (e) => {
                e.preventDefault();
                await apiRequest('/auth/logout', { method: 'POST' });
                window.location.href = 'login.html';
            });
        } catch {
            authEl.innerHTML = `<a href="login.html">Login</a>`;
        }
        })();


        (async () => {
            const authEl = document.getElementById('auth-status');
            try {
                const user = await apiRequest('/auth/me');
                authEl.innerHTML = `<span class="user-tag">${user.displayName}</span> <a href="#" id="logout-link">Logout</a>`;
                document.getElementById('logout-link').addEventListener('click', async (e) => {
                    e.preventDefault();
                    await apiRequest('/auth/logout', { method: 'POST' });
                    window.location.href = 'login.html';
                });

                // Add Admin link into the nav if this user has the Admin role
                if (user.roles.includes('Admin')) {
                    const navLinksEl = document.querySelector('.nav-links');
                    const adminLink = document.createElement('a');
                    adminLink.href = 'admin-roles.html';
                    adminLink.className = 'nav-link' + (activePage === 'admin-roles.html' ? ' active' : '');
                    adminLink.textContent = 'Admin';
                    navLinksEl.appendChild(adminLink);
                }
            } catch {
                authEl.innerHTML = `<a href="login.html">Login</a>`;
            }
        })();
}