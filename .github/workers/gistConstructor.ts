// Config
const filePath = ".resources/es3-passwords.json";
const outputFilePath = "temp-es3-passwords.md";

// Read the JSON data from the file
const data = JSON.parse(await Deno.readTextFile(filePath)) as {
  t: string;
  p: string;
}[];

// Sort the data by title (t) in a human-friendly way, considering numeric values in the titles
data.sort((a, b) => a.t.localeCompare(b.t, undefined, { numeric: true }));

// Generate the Markdown content
let md = "# 💾🗝️(ES3) Easy Save 3: SaveData Passwords\n";
for (const p of data) {
  md += `\n### ${p.t}\n`;
  md += "```plaintext\n";
  md += `${p.p}\n`;
  md += "```\n";
}

// Write the generated Markdown content to a file
await Deno.writeTextFile(outputFilePath, md);
console.log("Generated file: " + outputFilePath);