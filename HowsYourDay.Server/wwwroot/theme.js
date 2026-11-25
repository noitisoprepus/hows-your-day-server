(() => {
    const key = "hows-your-day-theme";
    const deviceTheme = matchMedia("(prefers-color-scheme: dark)");
    let theme;

    try {
        const saved = localStorage.getItem(key);
        if (saved === "light" || saved === "dark") theme = saved;
    } catch {
        // The device preference is available when browser storage is blocked.
    }

    const isDark = () => theme ? theme === "dark" : deviceTheme.matches;

    function applyTheme() {
        // Enhanced navigation can replace the root element's attributes.
        if (theme) document.documentElement.dataset.theme = theme;
        const pressed = String(isDark());
        document.querySelectorAll("[data-theme-toggle]").forEach(button => {
            button.setAttribute("aria-pressed", pressed);
        });
    }

    applyTheme();

    document.addEventListener("click", event => {
        if (!event.target.closest("[data-theme-toggle]")) return;

        theme = isDark() ? "light" : "dark";
        try {
            localStorage.setItem(key, theme);
        } catch {
            // Keep the choice for this page when browser storage is blocked.
        }
        applyTheme();
    });

    deviceTheme.addEventListener("change", applyTheme);
    document.addEventListener("DOMContentLoaded", () => {
        applyTheme();
        window.Blazor?.addEventListener("enhancedload", applyTheme);
    });
})();
