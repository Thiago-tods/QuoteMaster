(() => {
  const sidebar = document.getElementById('appSidebar');
  const toggle = document.querySelector('[data-sidebar-toggle]');
  const scrim = document.querySelector('[data-sidebar-close]');
  const closeSidebar = () => {
    sidebar?.classList.remove('open');
    scrim?.classList.remove('visible');
    toggle?.setAttribute('aria-expanded', 'false');
  };

  toggle?.addEventListener('click', () => {
    const open = sidebar?.classList.toggle('open') ?? false;
    scrim?.classList.toggle('visible', open);
    toggle.setAttribute('aria-expanded', String(open));
  });
  scrim?.addEventListener('click', closeSidebar);
  document.querySelectorAll('form[data-confirm]').forEach(form => {
    form.addEventListener('submit', event => {
      if (!window.confirm(form.dataset.confirm)) event.preventDefault();
    });
  });
})();
