import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { ABOUT_TEAM_MOCK, AboutTeam } from "./about-team";
import { TeamMemberCard } from "@/components/public/team/team-member-card";

describe("about page team", () => {
  it("renders mock team cards with avatar, role, bio, and socials", () => {
    const html = renderToStaticMarkup(<AboutTeam />);
    expect(html).toContain("تیم");
    expect(html).toContain("نمایشی");
    expect(html).toContain("team-member-card");
    for (const member of ABOUT_TEAM_MOCK) {
      expect(html).toContain(member.name);
      expect(html).toContain(member.role);
      expect(html).toContain(member.bio);
    }
  });

  it("renders a reusable member card from the shared component", () => {
    const html = renderToStaticMarkup(
      <TeamMemberCard
        member={{
          id: "demo",
          name: "نمونه",
          role: "نقش",
          bio: "بیو",
          initials: "ن",
          socials: { github: "#" },
        }}
      />,
    );
    expect(html).toContain("نمونه");
    expect(html).toContain("نقش");
    expect(html).toContain("بیو");
    expect(html).toContain("GitHub");
  });
});
