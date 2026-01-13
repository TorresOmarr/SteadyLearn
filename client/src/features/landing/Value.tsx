import { type Locale, getCopy } from '../../lib/i18n'
import { Zap, BarChart3, ShieldCheck } from 'lucide-react'

const icons = [Zap, BarChart3, ShieldCheck]

export function Value({ locale }: { locale: Locale }) {
  const t = getCopy(locale)
  return (
    <section className="py-16">
      <div className="mb-8 text-center">
        <span className="inline-block rounded-full bg-indigo-100 px-4 py-1 text-xs font-semibold uppercase tracking-widest text-indigo-600">
          {t.bulletTitle}
        </span>
      </div>
      <div className="grid gap-6 sm:grid-cols-3">
        {t.bullets.map((item, idx) => {
          const Icon = icons[idx]
          return (
            <div
              key={item}
              className="group rounded-2xl border border-border/70 bg-card p-6 shadow-sm transition hover:-translate-y-1 hover:shadow-lg"
            >
              <div className="mb-4 inline-flex h-12 w-12 items-center justify-center rounded-xl bg-indigo-500/10 text-indigo-600 transition group-hover:bg-indigo-500/20">
                <Icon className="h-6 w-6" />
              </div>
              <p className="text-base leading-relaxed text-foreground/90">{item}</p>
            </div>
          )
        })}
      </div>
    </section>
  )
}
