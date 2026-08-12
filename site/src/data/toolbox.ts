import type { ToolItem } from "@/types";

export const TOOLBOX_ITEMS: ToolItem[] = [
  {
    id: "git-cheat-sheet",
    title: "Git cheat sheet",
    description: "Everyday Git commands for branching and history.",
    content: `# Git cheat sheet

git status
git add .
git commit -m "message"
git push origin main

git checkout -b feature/name
git branch
git merge main

git log --oneline
git stash
git stash pop
git reset --soft HEAD~1`,
  },
  {
    id: "sql-snippets",
    title: "SQL snippets",
    description: "Common queries for filtering, joining, and grouping.",
    content: `-- SQL snippets

SELECT id, name, created_at
FROM users
WHERE active = true
ORDER BY created_at DESC
LIMIT 20;

SELECT u.name, COUNT(o.id) AS orders
FROM users u
LEFT JOIN orders o ON o.user_id = u.id
GROUP BY u.name
HAVING COUNT(o.id) > 0;

UPDATE users
SET last_seen_at = NOW()
WHERE id = 42;`,
  },
  {
    id: "js-snippets",
    title: "JS snippets",
    description: "Small JavaScript helpers for everyday tasks.",
    content: `// JS snippets

const unique = [...new Set(items)];

const groupBy = (arr, key) =>
  arr.reduce((acc, item) => {
    (acc[item[key]] ||= []).push(item);
    return acc;
  }, {});

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

const debounce = (fn, wait = 200) => {
  let t;
  return (...args) => {
    clearTimeout(t);
    t = setTimeout(() => fn(...args), wait);
  };
};`,
  },
  {
    id: "css-snippets",
    title: "CSS snippets",
    description: "Layout and utility patterns for modern UI.",
    content: `/* CSS snippets */

.center {
  display: grid;
  place-items: center;
}

.stack {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.truncate {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  border: 0;
}`,
  },
  {
    id: "docker-commands",
    title: "Docker commands",
    description: "Quick container and image commands.",
    content: `# Docker commands

docker ps
docker images
docker build -t app .
docker run -p 3000:3000 app

docker compose up -d
docker compose logs -f
docker compose down

docker exec -it container_name sh
docker system prune -f`,
  },
  {
    id: "http-status",
    title: "HTTP status codes",
    description: "Status codes you reach for most often.",
    content: `# HTTP status codes

200 OK
201 Created
204 No Content
301 Moved Permanently
302 Found
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
422 Unprocessable Entity
429 Too Many Requests
500 Internal Server Error
502 Bad Gateway
503 Service Unavailable`,
  },
];
