import { useState } from "react";

function ItemList() {
  const [item, setItem] = useState("");
  const [items, setItems] = useState([]);

  function addItem() {
    if (item.trim() === "") return;

    setItems([...items, item]);
    setItem("");
  }

  return (
    <div className="card">
      <h2>List Component</h2>

      <input
        type="text"
        placeholder="Enter item"
        value={item}
        onChange={(e) => setItem(e.target.value)}
      />

      <button onClick={addItem}>Add</button>

      <ul>
        {items.map((singleItem, index) => (
          <li key={index}>{singleItem}</li>
        ))}
      </ul>
    </div>
  );
}

export default ItemList;