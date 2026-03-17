(() => {
  const SELECT_CLASS = "js-dropdown-search";
  const WRAP_CLASS = "js-dropdown-search-wrap";
  const INPUT_CLASS = "js-dropdown-search-input";

  function normalize(text) {
    return (text || "").toString().trim().toLowerCase();
  }

  function enhanceSelect(select) {
    if (!select || select.dataset.dropdownSearchEnhanced === "1") return;
    if (select.tagName !== "SELECT") return;

    // Avoid enhancing hidden/disabled selects unless explicitly requested later
    // (still safe, but the UI is odd).
    const wrap = document.createElement("div");
    wrap.className = WRAP_CLASS;

    const input = document.createElement("input");
    input.type = "search";
    input.autocomplete = "off";
    input.spellcheck = false;
    input.className = INPUT_CLASS;
    input.placeholder = "Type to filter…";

    const parent = select.parentNode;
    parent.insertBefore(wrap, select);
    wrap.appendChild(input);
    wrap.appendChild(select);

    // Native <select> doesn't reliably respect hidden/display on <option> across browsers.
    // To make filtering consistent, keep a copy of the original options and rebuild.
    const originalOptions = Array.from(select.options).map((opt) => ({
      value: opt.value,
      text: opt.text,
      disabled: opt.disabled,
    }));

    const alwaysInclude = (opt) => normalize(opt.value) === "" || opt.disabled;

    const applyFilter = () => {
      const q = normalize(input.value);
      const currentValue = select.value;

      // Clear then rebuild option list.
      select.options.length = 0;

      const matches = [];
      for (const opt of originalOptions) {
        const isPlaceholderOrDisabled = alwaysInclude(opt);
        const matchesFilter = !q || normalize(opt.text).includes(q);
        const isCurrentlySelected = opt.value === currentValue;
        const include = isPlaceholderOrDisabled || matchesFilter || isCurrentlySelected;
        if (!include) continue;
        matches.push(opt);
      }

      for (const opt of matches) {
        const o = document.createElement("option");
        o.value = opt.value;
        o.text = opt.text;
        o.disabled = !!opt.disabled;
        select.add(o);
      }

      // Restore selection so it is never lost when filtering.
      const hasCurrent = matches.some((o) => o.value === currentValue);
      if (hasCurrent) select.value = currentValue;
      syncInputToSelection();
    };

    const syncInputToSelection = () => {
      const idx = select.selectedIndex;
      if (idx >= 0 && select.options[idx]) {
        const opt = select.options[idx];
        input.value = normalize(opt.value) === "" ? "" : opt.text;
      }
    };

    input.addEventListener("input", applyFilter);

    // Open dropdown when user focuses the filter so they see options (and filtered list when typing).
    input.addEventListener("focus", () => select.focus());

    select.addEventListener("change", () => {
      syncInputToSelection();
      applyFilter();
    });

    input.addEventListener("keydown", (e) => {
      if (e.key === "Escape") {
        input.value = "";
        applyFilter();
        select.focus();
      }
      if (e.key === "Enter") {
        e.preventDefault();
        var firstReal = -1;
        for (var i = 0; i < select.options.length; i++) {
          if (normalize(select.options[i].value) !== "") {
            firstReal = i;
            break;
          }
        }
        if (firstReal >= 0) {
          select.selectedIndex = firstReal;
          syncInputToSelection();
        }
        select.focus();
      }
    });

    select.dataset.dropdownSearchEnhanced = "1";
    applyFilter();
  }

  function enhanceAll() {
    document.querySelectorAll(`select.${SELECT_CLASS}`).forEach(enhanceSelect);
  }

  function runEnhance() {
    enhanceAll();
  }
  if (document.readyState === "loading") {
    window.addEventListener("load", runEnhance);
  } else {
    runEnhance();
  }
})();

