export function navegar(url: string, reemplazar = false) {
  if (reemplazar) history.replaceState(null, '', url)
  else history.pushState(null, '', url)
  window.dispatchEvent(new PopStateEvent('popstate'))
  window.scrollTo(0, 0)
}
