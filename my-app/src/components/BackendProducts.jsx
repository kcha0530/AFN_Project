import { useEffect, useState } from "react";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5474";
const API_KEY = "my-react-app-key";

function BackendProducts() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const token = localStorage.getItem("authToken");

  useEffect(() => {
    fetchProducts();
  }, []);

  function fetchProducts() {
    setLoading(true);
    setError("");

    const headers = {
      "x-api-key": API_KEY,
    };

    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }

    fetch(`${API_BASE_URL}/products`, {
      headers,
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
    <div className="card" id="backend-products">
      <h2>Backend Products</h2>
      <p className="card-subtitle">Secure products from your ASP.NET API.</p>
      <button onClick={fetchProducts}>Refresh Products</button>

      {loading && <p>Loading products...</p>}
      {error && <p className="error">{error}</p>}

      {!loading && !error && (
        <ul className="product-list">
          {products.map((product) => (
            <li key={product.id}>
              <strong>{product.name}</strong> — ${product.price}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default BackendProducts;
