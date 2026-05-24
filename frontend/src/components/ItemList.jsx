import { useState } from "react";

function ItemList() {
  const [item, setItem] = useState("");
  const [items, setItems] = useState([]);

  function addItem() {
    const trimmed = item.trim();
    if (!trimmed) return;
    setItems(prev => [...prev, trimmed]);
    setItem("");
  }

  function handleKeyDown(e) {
    if (e.key === "Enter") addItem();
  }

  return (
    <div className="card">
      <h2>List Builder</h2>
      <p className="card-subtitle">Type an item and add it to the list.</p>

      <div className="search-row">
        <input
          type="text"
          placeholder="Enter item…"
          value={item}
          onChange={(e) => setItem(e.target.value)}
          onKeyDown={handleKeyDown}
        />
        <button onClick={addItem}>Add</button>
      </div>

      {items.length > 0 && (
        <ul className="items-ul">
          {items.map((singleItem, index) => (
            <li key={index}>{singleItem}</li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default ItemList;
