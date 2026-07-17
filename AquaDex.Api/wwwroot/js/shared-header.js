function renderHeader(activePage) {
    const nav = [
        { href: 'index.html', label: 'Home' },
        { href: 'codex.html', label: 'Codex' },
        { href: 'catches.html', label: 'Catch Log' },
        { href: 'waterbodies.html', label: 'Waterbodies' },
        { href: 'profile.html', label: 'Profile' }
    ];

    const navHtml = nav.map(item =>
        `<a href="${item.href}" class="nav-link ${activePage === item.href ? 'active' : ''}">${item.label}</a>`
    ).join('');

    document.getElementById('site-header').innerHTML = `
    <div class="header-inner">
      <a href="index.html" class="logo">AquaDex</a>
      <nav class="nav-links">${navHtml}</nav>
      <div id="auth-status" class="auth-status"></div>
    </div>
  `;

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
}