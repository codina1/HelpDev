import type { Editor } from "@tiptap/react";
import { NodeSelection } from "@tiptap/pm/state";

export function getSelectedTopLevelIndex(editor: Editor): number | null {
  const { $from } = editor.state.selection;
  if ($from.depth < 1) return null;
  return $from.index(0);
}

export function moveSelectedBlock(editor: Editor, direction: -1 | 1): boolean {
  const { state } = editor;
  const $from = state.selection.$from;
  if ($from.depth < 1) return false;
  const index = $from.index(0);
  const parent = $from.node(0);
  const target = index + direction;
  if (target < 0 || target >= parent.childCount) return false;

  const from = $from.before(1);
  const node = $from.node(1);
  const to = from + node.nodeSize;

  let insertPos = 1;
  for (let i = 0; i < target; i += 1) {
    if (i === index) continue;
    insertPos += parent.child(i).nodeSize;
  }
  if (direction > 0) insertPos = from;

  const sliceNode = node;
  const tr = state.tr.delete(from, to);
  const mappedInsert = direction < 0 ? insertPos : tr.mapping.map(to);
  tr.insert(mappedInsert, sliceNode);
  editor.view.dispatch(tr.scrollIntoView());
  editor.commands.focus();
  return true;
}

export function duplicateSelectedBlock(editor: Editor): boolean {
  const { state } = editor;
  const $from = state.selection.$from;
  if ($from.depth < 1) return false;
  const from = $from.before(1);
  const node = $from.node(1);
  const to = from + node.nodeSize;
  const tr = state.tr.insert(to, node.copy(node.content));
  editor.view.dispatch(tr.scrollIntoView());
  return true;
}

export function deleteSelectedBlock(editor: Editor): boolean {
  const { state } = editor;
  const $from = state.selection.$from;
  if ($from.depth < 1) return false;
  if (state.doc.childCount <= 1) {
    return editor.chain().focus().clearNodes().setParagraph().run();
  }
  const from = $from.before(1);
  const node = $from.node(1);
  const tr = state.tr.delete(from, from + node.nodeSize);
  editor.view.dispatch(tr.scrollIntoView());
  return true;
}

export function selectCurrentBlock(editor: Editor): boolean {
  const { state } = editor;
  const $from = state.selection.$from;
  if ($from.depth < 1) return false;
  const pos = $from.before(1);
  const selection = NodeSelection.create(state.doc, pos);
  editor.view.dispatch(state.tr.setSelection(selection));
  return true;
}

export function selectedBlockType(editor: Editor): string {
  const { $from } = editor.state.selection;
  if ($from.depth < 1) return "doc";
  return $from.node(1).type.name;
}

export function selectedBlockAttrs(editor: Editor): Record<string, unknown> {
  const { $from } = editor.state.selection;
  if ($from.depth < 1) return {};
  return ($from.node(1).attrs ?? {}) as Record<string, unknown>;
}
