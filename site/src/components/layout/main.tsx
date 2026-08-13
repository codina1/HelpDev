type MainProps = {
  children: React.ReactNode;
};

export function Main({ children }: MainProps) {
  return (
    <main className="min-w-0 flex-1 overflow-y-auto">
      <div className="mx-auto w-full max-w-none px-4 py-5 pb-8 sm:px-6 lg:px-8 lg:py-6 lg:pb-10">
        {children}
      </div>
    </main>
  );
}
