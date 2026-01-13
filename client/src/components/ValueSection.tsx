import { type Locale, getCopy } from '../lib/i18n'

export function ValueSection({ locale }: { locale: Locale }) {
  const t = getCopy(locale)
  return (
    <section className="container py-12">
      <div className="mb-6 text-sm font-semibold uppercase tracking-wide text-indigo-600">{t.bulletTitle}</div>
      <div className="grid gap-4 sm:grid-cols-3">
        {t.bullets.map((item) => (
          <div key={item} className="rounded-xl border border-border/80 bg-card/80 p-4 shadow-sm">
            <p className="text-sm leading-relaxed text-foreground/90">{item}</p>
          </div>
        ))}
      </div>
    </section>
  )}
