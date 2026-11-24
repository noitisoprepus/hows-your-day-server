(() => {
    const key = "hows-your-day-theme";
    const root = document.documentElement;
    const deviceTheme = matchMedia("(prefers-color-scheme: dark)");

    try {
        const saved = localStorage.getItem(key);
        if (saved === "light" || saved === "dark") root.dataset.theme = saved;
    } catch {
        // The device preference is available when browser storage is blocked.
    }

    const isDark = () => root.dataset.theme ? root.dataset.theme === "dark" : deviceTheme.matches;

    function updateToggles() {
        const pressed = String(isDark());
        document.querySelectorAll("[data-theme-toggle]").forEach(button => {
            button.setAttribute("aria-pressed", pressed);
        });
    }

    document.addEventListener("click", event => {
        if (!event.target.closest("[data-theme-toggle]")) return;

        const theme = isDark() ? "light" : "dark";
        root.dataset.theme = theme;
        try {
            localStorage.setItem(key, theme);
        } catch {
            // Keep the choice for this page when browser storage is blocked.
        }
        updateToggles();
    });

    deviceTheme.addEventListener("change", updateToggles);
    document.addEventListener("DOMContentLoaded", () => {
        updateToggles();
        window.Blazor?.addEventListener("enhancedload", updateToggles);
    });
})();
