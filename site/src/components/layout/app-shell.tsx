"use client";

import { usePathname } from "next/navigation";
import { AuthProvider } from "@/components/auth";
import { PublicBottomNav } from "@/components/public/public-bottom-nav";
import { PublicFooter } from "@/components/public/public-footer";
import { Header } from "@/components/layout/header";
import { Main } from "@/components/layout/main";

type AppShellProps = {
  children: React.ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  const pathname = usePathname();
  const usesOwnShell =
    pathname?.startsWith("/profile") || pathname?.startsWith("/admin");
  const isHome = pathname === "/";
  const isArticles = pathname?.startsWith("/articles");

  return (
    <AuthProvider>
      {usesOwnShell ? (
        children
      ) : (
        <div className="page-ambient pub-page pub-bottom-nav-spacer flex min-h-dvh flex-col text-foreground">
          <Header />
          {isHome || isArticles ? (
            <main className="min-w-0 flex-1">{children}</main>
          ) : (
            <Main>{children}</Main>
          )}
          <PublicFooter />
          <PublicBottomNav />
        </div>
      )}
    </AuthProvider>
  );
}
