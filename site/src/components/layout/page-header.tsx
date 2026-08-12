type PageHeaderProps = {
  title: string;
  description?: string;
};

export function PageHeader({ title, description }: PageHeaderProps) {
  return (
    <div className="mb-8">
      <p className="ui-kicker mb-2">HelpDev</p>
      <h1 className="ui-title">{title}</h1>
      {description ? <p className="ui-body mt-2 max-w-2xl">{description}</p> : null}
    </div>
  );
}
