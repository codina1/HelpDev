type MainProps = {
  children: React.ReactNode;
};

export function Main({ children }: MainProps) {
  return (
    <main className="min-w-0 flex-1 overflow-y-auto">
      <div className="mx-auto max-w-[1400px] px-4 py-6 pb-10 lg:px-6 lg:py-8 lg:pb-12">
        {children}
      </div>
    </main>
  );
}
