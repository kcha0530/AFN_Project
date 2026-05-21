function Header({ isLoggedIn, onLogout }) {
  return (
    <header className="site-header">
      <div>
        <p className="logo">AFN Project</p>
        <p className="logo-tag">Clean, secure React + ASP.NET demo</p>
      </div>
      <nav>
        {isLoggedIn ? (
          <>
            <a href="#dashboard">Dashboard</a>
            <button type="button" className="nav-button outline" onClick={onLogout}>
              Logout
            </button>
          </>
        ) : (
          <a href="#login">Login</a>
        )}
      </nav>
    </header>
  );
}

export default Header;