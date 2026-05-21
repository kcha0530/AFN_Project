function Header() {
  return (
    <header className="site-header">
      <div>
        <p className="logo">AFN Project</p>
        <p className="logo-tag">Clean, secure React + ASP.NET demo</p>
      </div>
      <nav>
        <a href="#login">Login</a>
        <a href="#backend-products">Products</a>
      </nav>
    </header>
  );
}

export default Header;