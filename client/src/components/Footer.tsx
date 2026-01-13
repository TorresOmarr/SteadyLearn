import { type Locale, getCopy } from '../lib/i18n'

export function Footer({ locale }: { locale: Locale }) {
  const t = getCopy(locale)
  return (
    <footer className="border-t border-border/40 bg-slate-50/60 py-10">
      <div className="container flex flex-col items-center justify-between gap-4 sm:flex-row">
        <div className="flex items-center gap-2 text-sm font-medium text-foreground">
          <span className="inline-flex h-7 w-7 items-center justify-center rounded-lg bg-indigo-600 text-xs font-bold text-white shadow">
            SL
          </span>
          {t.footer}
        </div>
        <p className="text-xs text-muted-foreground">{t.footerRights}</p>
      </div>
    </footer>
  )
}
