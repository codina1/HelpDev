"use client";

import { useState } from "react";
import {
  AiCard,
  ArticleCard,
  Badge,
  Button,
  Card,
  EmptyState,
  ErrorState,
  Input,
  LoadingState,
  Modal,
  RoadmapCard,
  Tabs,
  ToolCard,
} from "@/components/ui/ds";
import { dsColors, dsTypography } from "@/lib/design-system";
import { ApiClientError } from "@/lib/api/errors";
import { PublicContainer } from "@/components/ui/public/v2/public-container";

export default function DesignSystemClient() {
  const [tab, setTab] = useState("colors");
  const [modalOpen, setModalOpen] = useState(false);

  return (
    <div dir="rtl" className="ds-fade space-y-10 pb-16">
      <PublicContainer size="wide" className="pt-8">
        <p className="mb-2 text-[11px] font-bold tracking-wide text-[color:var(--ds-secondary)]">
          Sprint 50D-1
        </p>
        <h1 className="text-3xl font-extrabold text-[color:var(--ds-fg)] sm:text-4xl">
          HelpDev Design System
        </h1>
        <p className="mt-2 max-w-2xl text-[14px] leading-7 text-[color:var(--ds-muted)]">
          سیستم طراحی پریمیوم SaaS برای پلتفرم دانش مهندسی هوش مصنوعی — توکن‌ها، primitives و کارت‌ها.
        </p>

        <div className="mt-6">
          <Tabs
            value={tab}
            onChange={setTab}
            aria-label="بخش‌های دیزاین سیستم"
            items={[
              { id: "colors", label: "رنگ‌ها" },
              { id: "typography", label: "تایپوگرافی" },
              { id: "buttons", label: "دکمه‌ها" },
              { id: "cards", label: "کارت‌ها" },
              { id: "badges", label: "بج‌ها" },
              { id: "states", label: "حالت‌ها" },
            ]}
          />
        </div>
      </PublicContainer>

      <PublicContainer size="wide" className="space-y-8">
        {tab === "colors" ? (
          <section aria-labelledby="ds-colors" className="ds-slide space-y-4">
            <h2 id="ds-colors" className="text-xl font-extrabold text-[color:var(--ds-fg)]">
              Colors
            </h2>
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
              {Object.entries(dsColors).map(([name, value]) => (
                <Card key={name} hover={false} className="!p-3">
                  <div
                    className="mb-3 h-16 rounded-[var(--ds-radius-md)] border border-[color:var(--ds-border)]"
                    style={{ background: value }}
                    aria-hidden
                  />
                  <p className="text-[13px] font-bold text-[color:var(--ds-fg)]">{name}</p>
                  <p dir="ltr" className="font-mono text-[11px] text-[color:var(--ds-muted)]">
                    {value}
                  </p>
                </Card>
              ))}
            </div>
          </section>
        ) : null}

        {tab === "typography" ? (
          <section aria-labelledby="ds-type" className="ds-slide space-y-4">
            <h2 id="ds-type" className="text-xl font-extrabold text-[color:var(--ds-fg)]">
              Typography
            </h2>
            <Card hover={false} className="space-y-4">
              <p style={{ fontSize: dsTypography.display.size, fontWeight: dsTypography.display.weight }}>
                Display — دانش مهندسی
              </p>
              <p style={{ fontSize: dsTypography.h1.size, fontWeight: dsTypography.h1.weight }}>H1 عنوان اصلی</p>
              <p style={{ fontSize: dsTypography.h2.size, fontWeight: dsTypography.h2.weight }}>H2 عنوان بخش</p>
              <p style={{ fontSize: dsTypography.h3.size, fontWeight: dsTypography.h3.weight }}>H3 عنوان کارت</p>
              <p style={{ fontSize: dsTypography.body.size, lineHeight: dsTypography.body.lineHeight }}>
                Body — متن بدنه برای توضیح محصول و محتوای مهندسی.
              </p>
              <p style={{ fontSize: dsTypography.caption.size, fontWeight: dsTypography.caption.weight }}>
                Caption — برچسب و متادیتا
              </p>
            </Card>
          </section>
        ) : null}

        {tab === "buttons" ? (
          <section aria-labelledby="ds-buttons" className="ds-slide space-y-4">
            <h2 id="ds-buttons" className="text-xl font-extrabold text-[color:var(--ds-fg)]">
              Buttons
            </h2>
            <div className="flex flex-wrap gap-3">
              <Button>Primary</Button>
              <Button variant="secondary">Secondary</Button>
              <Button variant="ghost">Ghost</Button>
              <Button variant="danger">Danger</Button>
              <Button size="sm">Small</Button>
              <Button size="lg">Large</Button>
              <Button onClick={() => setModalOpen(true)}>باز کردن Modal</Button>
            </div>
            <div className="max-w-md">
              <label className="mb-1 block text-[12px] font-semibold text-[color:var(--ds-muted)]" htmlFor="ds-input">
                Input
              </label>
              <Input id="ds-input" placeholder="مثال ورودی..." aria-label="ورودی نمونه" />
            </div>
          </section>
        ) : null}

        {tab === "cards" ? (
          <section aria-labelledby="ds-cards" className="ds-slide space-y-4">
            <h2 id="ds-cards" className="text-xl font-extrabold text-[color:var(--ds-fg)]">
              Cards
            </h2>
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              <ArticleCard
                title="معماری Microservice"
                href="/articles"
                category="مقاله"
                readingTime="۸ دقیقه"
                summary="اصول طراحی سرویس‌های مستقل"
              />
              <ToolCard title="Cursor" href="/toolbox" category="AI" description="دستیار کدنویسی هوشمند" />
              <RoadmapCard
                title="Frontend Engineer"
                href="/roadmap"
                nodes={[{ label: "HTML" }, { label: "React" }, { label: "Next.js" }]}
              />
              <AiCard
                title="دستیار مهندسی"
                description="مسیر یادگیری و حل مسئله را با دانش پلتفرم پیدا کنید."
              />
            </div>
          </section>
        ) : null}

        {tab === "badges" ? (
          <section aria-labelledby="ds-badges" className="ds-slide space-y-4">
            <h2 id="ds-badges" className="text-xl font-extrabold text-[color:var(--ds-fg)]">
              Badges
            </h2>
            <div className="flex flex-wrap gap-2">
              <Badge>default</Badge>
              <Badge variant="primary">primary</Badge>
              <Badge variant="secondary">secondary</Badge>
              <Badge variant="ai">AI</Badge>
              <Badge variant="success">success</Badge>
              <Badge variant="warning">warning</Badge>
              <Badge variant="outline">outline</Badge>
            </div>
          </section>
        ) : null}

        {tab === "states" ? (
          <section aria-labelledby="ds-states" className="ds-slide grid gap-4 lg:grid-cols-3">
            <EmptyState
              title="مسیر مهندسی شما هنوز ساخته نشده"
              description="با دستیار AI یک نقشه راه بسازید."
              ctaLabel="ساخت مسیر با AI"
              ctaHref="/learning/assistant"
            />
            <LoadingState rows={3} />
            <ErrorState
              error={
                new ApiClientError({
                  message: "نمونه خطا",
                  status: 500,
                  correlationId: "demo-corr",
                })
              }
              onRetry={() => undefined}
            />
          </section>
        ) : null}
      </PublicContainer>

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title="نمونه Modal"
        footer={
          <Button size="sm" onClick={() => setModalOpen(false)}>
            متوجه شدم
          </Button>
        }
      >
        این دیالوگ از primitives دیزاین سیستم استفاده می‌کند و با Esc بسته می‌شود.
      </Modal>
    </div>
  );
}
