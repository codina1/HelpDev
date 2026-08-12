import type { Metadata } from "next";
import { Vazirmatn } from "next/font/google";
import { AppShell } from "@/components/layout";
import { SITE } from "@/lib/constants";
import "./globals.css";

const vazirmatn = Vazirmatn({
  variable: "--font-vazirmatn",
  subsets: ["arabic", "latin"],
  display: "swap",
});

export const metadata: Metadata = {
  title: {
    default: SITE.name,
    template: `%s | ${SITE.name}`,
  },
  description: SITE.description,
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="fa" dir="rtl" className="dark" suppressHydrationWarning>
      <body className={`${vazirmatn.variable} antialiased`}>
        <AppShell>{children}</AppShell>
      </body>
    </html>
  );
}
