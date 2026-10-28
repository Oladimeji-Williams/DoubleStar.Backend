// Removes the leftover "paste this file" header comments from C# files: stale path headers
// (// Api/Controllers/X.cs) and change markers (// MODIFIED, // full replacement, ...).
// Only ever deletes a single comment line at the very top of a file. Multi-line change logs are
// listed instead of deleted so you can decide what is worth keeping.
//
// Usage: node tools/strip-header-comments.mjs [root]   (default: current folder)
import { readdirSync, readFileSync, statSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const SKIP = new Set(['obj', 'bin', '.git', 'node_modules', 'Migrations']);
const PATH_HEADER = /^\/\/\s*[\w.\-/\\]+\.cs\s*$/;
const MARKER = /MODIFIED|replace the whole file|full replacement|NEW FILE/i;

function* csFiles(dir) {
  for (const entry of readdirSync(dir)) {
    if (SKIP.has(entry)) continue;
    const path = join(dir, entry);
    if (statSync(path).isDirectory()) yield* csFiles(path);
    else if (path.endsWith('.cs')) yield path;
  }
}

const isPlainComment = (line) => line.startsWith('//') && !line.startsWith('///');

let stripped = 0;
const reviewByHand = [];

for (const file of csFiles(process.argv[2] ?? '.')) {
  const raw = readFileSync(file, 'utf8');
  const bom = raw.startsWith('\uFEFF') ? '\uFEFF' : '';
  const text = bom ? raw.slice(1) : raw;
  const eol = text.includes('\r\n') ? '\r\n' : '\n';
  const lines = text.split(/\r?\n/);

  let blockLength = 0;
  while (blockLength < lines.length && isPlainComment(lines[blockLength])) blockLength++;
  if (blockLength === 0) continue;

  const first = lines[0];
  const isPath = PATH_HEADER.test(first);
  const isMarker = MARKER.test(first);
  if (!isPath && !isMarker) continue;

  if (isMarker && blockLength > 1) {
    reviewByHand.push(file);
    continue;
  }

  lines.shift();
  while (lines.length > 0 && lines[0].trim() === '' && isPath === false) lines.shift();
  writeFileSync(file, bom + lines.join(eol));
  stripped++;
}

console.log(`Removed a header comment from ${stripped} files.`);
if (reviewByHand.length > 0) {
  console.log(`\nMulti-line change notes left alone (${reviewByHand.length}), tidy these by hand:`);
  for (const file of reviewByHand) console.log('  ' + file);
}