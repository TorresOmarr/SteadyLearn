import { LanguageSwitcher } from './LanguageSwitcher'
import { Button } from './ui/button'
import { type Locale, getCopy } from '../lib/i18n'

type Props = {
  locale: Locale
  onToggleLocale: () => void
  onLogin: () => void
  onRegister: () => void
  children: React.ReactNode
}

export function Layout({ locale, onToggleLocale, onLogin, onRegister, children }: Props) {
  const t = getCopy(locale)
  return (
    <div className="min-h-screen bg-background text-foreground">
      <header className="sticky top-0 z-30 border-b border-border/40 bg-white/70 backdrop-blur-lg supports-[backdrop-filter]:bg-white/60">
        <div className="container flex items-center justify-between py-3">
          <div className="flex items-center gap-2 text-lg font-semibold tracking-tight">
            <span className="inline-flex h-9 w-9 items-center justify-center rounded-xl bg-gradient-to-br from-indigo-600 to-indigo-500 text-white text-sm font-bold shadow-md">
              SL
            </span>
            <span className="hidden sm:inline">SteadyLearn</span>
          </div>
          <nav className="flex items-center gap-2">
            <LanguageSwitcher locale={locale} onToggle={onToggleLocale} />
            <Button variant="ghost" size="sm" onClick={onLogin}>
              {t.loginCta}
            </Button>
            <Button size="sm" onClick={onRegister}>
              {t.registerCta}
            </Button>
          </nav>
        </div>
      </header>
      <main className="container pb-20 pt-10">{children}</main>
    </div>
  )
}
