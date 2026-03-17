(() => {
  function normalize(s) {
    return (s || "").toString().toLowerCase().trim();
  }

  function getRows(grid) {
    // ASP.NET GridView renders as a table. We filter body rows only.
    const rows = Array.from(grid.querySelectorAll("tr"));
    if (rows.length <= 1) return [];

    // Heuristic: first row is header if it contains <th>
    const start = rows[0].querySelector("th") ? 1 : 0;
    return rows.slice(start);
  }

  function applyToGrid(grid, q) {
    const rows = getRows(grid);
    for (const row of rows) {
      const text = normalize(row.innerText || row.textContent);
      const show = !q || text.includes(q);
      row.style.display = show ? "" : "none";
    }
  }

  function applySearch(q) {
    document.querySelectorAll("table.grid-view").forEach((grid) => applyToGrid(grid, q));
  }

  function init() {
    const input = document.getElementById("topSearchInput");
    if (!input) return;

    const handler = () => applySearch(normalize(input.value));
    input.addEventListener("input", handler);

    // Let Esc clear the search quickly.
    input.addEventListener("keydown", (e) => {
      if (e.key === "Escape") {
        input.value = "";
        handler();
      }
    });

    handler();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
})();

