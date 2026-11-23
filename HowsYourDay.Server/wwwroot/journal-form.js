function reset(form) {
    const button = form.querySelector('button[type="submit"]');
    if (button?.dataset.readyLabel) {
        button.textContent = button.dataset.readyLabel;
        button.disabled = false;
        form.removeAttribute("aria-busy");
    }
}

document.addEventListener("submit", event => {
    if (!event.target.matches("form[data-journal-save]")) return;

    const button = event.submitter;
    if (!button) return;

    button.dataset.readyLabel = button.textContent;
    button.textContent = "Saving…";
    button.disabled = true;
    event.target.setAttribute("aria-busy", "true");
});

window.addEventListener("pageshow", event => {
    if (event.persisted)
        document.querySelectorAll("form[data-journal-save]").forEach(reset);
});
