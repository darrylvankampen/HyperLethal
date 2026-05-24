const itemsEl = document.getElementById("items");
const assortsEl = document.getElementById("assorts");
const reloadBtn = document.getElementById("reloadBtn");

reloadBtn.addEventListener("click", () => load());

function field(label, value, key, type = "text") {
  if (type === "textarea") {
    return `<label>${label}<textarea data-key="${key}">${value ?? ""}</textarea></label>`;
  }
  return `<label>${label}<input data-key="${key}" type="${type}" value="${value ?? ""}"></label>`;
}

function toNumber(v) {
  if (v === "" || v == null) return 0;
  const n = Number(v);
  return Number.isFinite(n) ? n : 0;
}

function inRange(v, min, max) {
  return Number.isFinite(v) && v >= min && v <= max;
}

function validateItemModel(model) {
  if (!model.name?.trim()) return "Name is required.";
  if (!model.shortName?.trim()) return "Short Name is required.";
  if (!model.description?.trim()) return "Description is required.";
  if (!inRange(model.damage, 1, 500)) return "Damage must be 1-500.";
  if (!inRange(model.penetrationPower, 1, 200)) return "Penetration must be 1-200.";
  if (!inRange(model.armorDamage, 1, 200)) return "Armor Damage must be 1-200.";
  if (!inRange(model.fragmentationChance, 0, 1)) return "Frag Chance must be 0.00-1.00.";
  if (!inRange(model.initialSpeed, 50, 3000)) return "Initial Speed must be 50-3000.";
  if (!inRange(model.fleaPriceRoubles, 1, 1000000)) return "Flea Price must be 1-1000000.";
  if (!inRange(model.handbookPriceRoubles, 1, 1000000)) return "Handbook Price must be 1-1000000.";
  return null;
}

function validateAssortModel(model) {
  if (!inRange(model.priceRoubles, 1, 1000000)) return "Price must be 1-1000000.";
  if (!inRange(model.loyaltyLevel, 1, 4)) return "Loyalty must be 1-4.";
  if (!inRange(model.stackObjectsCount, 1, 999999)) return "Stack must be 1-999999.";
  if (!inRange(model.buyRestrictionMax, 0, 999999)) return "Buy Restriction Max must be 0-999999.";
  return null;
}

async function save(card) {
  const model = {
    file: card.dataset.file,
    templateId: card.dataset.tpl,
    name: card.querySelector('[data-key="name"]').value,
    shortName: card.querySelector('[data-key="shortName"]').value,
    description: card.querySelector('[data-key="description"]').value,
    damage: toNumber(card.querySelector('[data-key="damage"]').value),
    penetrationPower: toNumber(card.querySelector('[data-key="penetrationPower"]').value),
    armorDamage: toNumber(card.querySelector('[data-key="armorDamage"]').value),
    fragmentationChance: toNumber(card.querySelector('[data-key="fragmentationChance"]').value),
    initialSpeed: toNumber(card.querySelector('[data-key="initialSpeed"]').value),
    fleaPriceRoubles: toNumber(card.querySelector('[data-key="fleaPriceRoubles"]').value),
    handbookPriceRoubles: toNumber(card.querySelector('[data-key="handbookPriceRoubles"]').value)
  };

  const status = card.querySelector(".status");
  const clientValidationError = validateItemModel(model);
  if (clientValidationError) {
    status.textContent = `Error: ${clientValidationError}`;
    return;
  }

  status.textContent = "Saving...";
  const res = await fetch("/hyperlethal/save", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(model)
  });

  if (!res.ok) {
    const text = await res.text();
    status.textContent = `Error: Save failed: ${text}`;
    return;
  }

  status.textContent = "Saved.";
}

async function saveAssort(card) {
  const model = {
    file: card.dataset.file,
    offerId: card.dataset.offerid,
    priceRoubles: toNumber(card.querySelector('[data-key="priceRoubles"]').value),
    loyaltyLevel: toNumber(card.querySelector('[data-key="loyaltyLevel"]').value),
    unlimitedCount: card.querySelector('[data-key="unlimitedCount"]').checked,
    stackObjectsCount: toNumber(card.querySelector('[data-key="stackObjectsCount"]').value),
    buyRestrictionMax: toNumber(card.querySelector('[data-key="buyRestrictionMax"]').value)
  };

  const status = card.querySelector(".status");
  const clientValidationError = validateAssortModel(model);
  if (clientValidationError) {
    status.textContent = `Error: ${clientValidationError}`;
    return;
  }

  status.textContent = "Saving...";
  const res = await fetch("/hyperlethal/save-assort", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(model)
  });

  if (!res.ok) {
    const text = await res.text();
    status.textContent = `Error: Save failed: ${text}`;
    return;
  }

  status.textContent = "Saved.";
}

function renderItems(items) {
  itemsEl.innerHTML = "";
  for (const item of items) {
    const card = document.createElement("article");
    card.className = "card";
    card.dataset.file = item.file;
    card.dataset.tpl = item.templateId;

    card.innerHTML = `
      <div class="meta">${item.templateId} · ${item.file}</div>
      <div class="row">
        ${field("Name", item.name, "name")}
        ${field("Short Name", item.shortName, "shortName")}
        ${field("Description", item.description, "description", "textarea")}
      </div>
      <div class="grid">
        ${field("Damage", item.damage, "damage", "number")}
        ${field("Penetration", item.penetrationPower, "penetrationPower", "number")}
        ${field("Armor Damage", item.armorDamage, "armorDamage", "number")}
        ${field("Frag Chance", item.fragmentationChance, "fragmentationChance", "number")}
        ${field("Initial Speed", item.initialSpeed, "initialSpeed", "number")}
        ${field("Price", item.fleaPriceRoubles, "fleaPriceRoubles", "number")}
        ${field("Handbook Price", item.handbookPriceRoubles, "handbookPriceRoubles", "number")}
      </div>
      <div class="actions"><button type="button">Save</button></div>
      <div class="status"></div>
    `;

    card.querySelector("button").addEventListener("click", () => save(card));
    itemsEl.appendChild(card);
  }
}

function renderAssorts(offers) {
  assortsEl.innerHTML = "";
  for (const offer of offers) {
    const card = document.createElement("article");
    card.className = "card";
    card.dataset.file = offer.file;
    card.dataset.offerid = offer.offerId;
    card.innerHTML = `
      <div class="meta">${offer.offerId} · ${offer.trader} · ${offer.templateId} · ${offer.file}</div>
      <div class="grid">
        ${field("Price", offer.priceRoubles, "priceRoubles", "number")}
        ${field("Loyalty", offer.loyaltyLevel, "loyaltyLevel", "number")}
        ${field("Stack", offer.stackObjectsCount, "stackObjectsCount", "number")}
        ${field("Buy Restriction Max", offer.buyRestrictionMax, "buyRestrictionMax", "number")}
        <label>Unlimited Count<input data-key="unlimitedCount" type="checkbox" ${offer.unlimitedCount ? "checked" : ""}></label>
      </div>
      <div class="actions"><button type="button">Save</button></div>
      <div class="status"></div>
    `;

    card.querySelector("button").addEventListener("click", () => saveAssort(card));
    assortsEl.appendChild(card);
  }
}

async function load() {
  itemsEl.textContent = "Loading...";
  assortsEl.textContent = "Loading...";

  const [itemsRes, assortsRes] = await Promise.all([
    fetch("/hyperlethal/data"),
    fetch("/hyperlethal/assorts")
  ]);

  if (!itemsRes.ok || !assortsRes.ok) {
    itemsEl.textContent = "Failed to load data.";
    assortsEl.textContent = "Failed to load assorts.";
    return;
  }

  const itemsPayload = await itemsRes.json();
  const assortsPayload = await assortsRes.json();
  renderItems(itemsPayload.items ?? []);
  renderAssorts(assortsPayload.offers ?? []);
}

load();
