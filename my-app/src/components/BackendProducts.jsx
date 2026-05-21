import { useEffect, useState } from "react";

const API_BASE_URL = "http://localhost:5474";
const API_KEY = "my-react-app-key";

function BackendProducts() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    fetchProducts();
  }, []);

  function fetchProducts() {
    setLoading(true);
    setError("");

    fetch(`${API_BASE_URL}/products`, {
      headers: {
        "x-api-key": API_KEY,
      },
    })
      .then((res) => {
        if (!res.ok) {
          throw new Error(`Failed to load products (${res.status})`);
        }
        return res.json();
      })
      .then((data) => {
        setProducts(data);
      })
      .catch((err) => {
        setError(err.message || "Error fetching backend products");
      })
      .finally(() => {
        setLoading(false);
      });
  }

  return (
    <div className="card">
      <h2>Backend Products</h2>
      <button onClick={fetchProducts}>Refresh Products</button>

      {loading && <p>Loading products...</p>}
      {error && <p className="error">{error}</p>}

      {!loading && !error && (
        <ul>
          {products.map((product) => (
            <li key={product.id}>
              {product.name} — ${product.price}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default BackendProducts;
