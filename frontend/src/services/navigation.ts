export function navegar(url: string) {
  history.pushState(null, '', url)
  window.dispatchEvent(new PopStateEvent('popstate'))
  window.scrollTo(0, 0)
}
